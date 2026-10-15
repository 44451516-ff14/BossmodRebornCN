using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;

namespace BossMod;

public sealed class ConfigRoot
{
    public Event Modified = new();
    public readonly Dictionary<Type, ConfigNode> _nodes = [];
    private readonly Dictionary<string, ConfigNode> _nodesByName = [];

    // fields temporarily overridden by other plugins (via IPC); the config file always keeps the user's original value
    private readonly record struct TransientOverride(object? Original, object? Value);
    private readonly Dictionary<(ConfigNode node, ConfigFieldMetadata field), TransientOverride> _transient = [];
    private int _transientVersion; // incremented under the lock whenever the overrides change

    public void Initialize() => GeneratedRegistries.RegisterConfigNodes(RegisterNode);

    private void RegisterNode(Type type, ConfigNode node)
    {
        // subscribed first, so that it runs before the save is triggered
        node.Modified.Subscribe(() => DropChangedTransients(node));
        node.Modified.Subscribe(Modified.Fire);
        _nodes[type] = node;
        if (type.FullName is { } fullName)
        {
            _nodesByName[fullName] = node;
        }
    }

    public T Get<T>() where T : ConfigNode => (T)_nodes[typeof(T)];
    public T Get<T>(Type derived) where T : ConfigNode => (T)_nodes[derived];
    public ConfigListener<T> GetAndSubscribe<T>(Action<T> modified) where T : ConfigNode => new(Get<T>(), modified);

    public void LoadFromFile(FileInfo file)
    {
        try
        {
            var data = ConfigConverter.Schema.Load(file);
            using var json = data.document;
            var ser = Serialization.BuildSerializationOptions();
            foreach (var jconfig in data.payload.EnumerateObject())
            {
                var node = _nodesByName.GetValueOrDefault(jconfig.Name);
                try
                {
                    node?.Deserialize(jconfig.Value, ser);
                }
                catch (AggregateException exc)
                {
                    Service.Logger.Warning(exc, "An error occurred while deserializing the plugin config. As a result, some settings may have unexpected values.");
                }
            }
        }
        catch (Exception e)
        {
            Service.Log($"Failed to load config from {file.FullName}: {e}");
        }
    }

    public void SaveToFile(FileInfo file)
    {
        try
        {
            var ser = Serialization.BuildSerializationOptions();
            var serializedNodes = new ConcurrentDictionary<Type, string>();
            // saves run in the background, so overrides can change while serializing; retry until the serialized values match the snapshot
            // (anything changing the overrides also fires Modified, so giving up is fine - another save is coming)
            Dictionary<(ConfigNode node, ConfigFieldMetadata field), TransientOverride> transient;
            int version;
            var attempts = 0;
            do
            {
                if (++attempts > 10)
                {
                    Service.Log($"配置保存已跳过：{file.FullName}，临时覆盖项持续变化");
                    return;
                }

                (transient, version) = TransientSnapshot();
                Parallel.ForEach(_nodes, entry =>
                {
                    using var ms = new MemoryStream();
                    using var tempWriter = new Utf8JsonWriter(ms);
                    entry.Value.Serialize(tempWriter, ser);
                    tempWriter.Flush();
                    serializedNodes[entry.Key] = Encoding.UTF8.GetString(ms.ToArray());
                });
            }
            while (version != Volatile.Read(ref _transientVersion));

            // temporarily overridden fields are saved with the user's original value
            if (transient.Count > 0)
            {
                Dictionary<Type, JsonObject> patched = [];
                foreach (var ((node, field), entry) in transient)
                {
                    var type = node.GetType();
                    if (!patched.TryGetValue(type, out var jnode))
                    {
                        patched[type] = jnode = JsonNode.Parse(serializedNodes[type])!.AsObject();
                    }
                    jnode[field.Name] = JsonSerializer.SerializeToNode(entry.Original, field.FieldType, ser);
                }
                foreach (var (type, jnode) in patched)
                {
                    serializedNodes[type] = jnode.ToJsonString();
                }
            }

            using var stream = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.None);
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
            writer.WriteStartObject();
            writer.WriteNumber("Version", ConfigConverter.Schema.CurrentVersion);
            writer.WritePropertyName("Payload");
            writer.WriteStartObject();
            foreach (var (type, json) in serializedNodes)
            {
                writer.WritePropertyName(type.FullName!);
                writer.WriteRawValue(json);
            }
            writer.WriteEndObject();
            writer.WriteEndObject();
        }
        catch (Exception e)
        {
            Service.Log($"Failed to save config to {file.FullName}: {e}");
        }
    }

    public List<string> ConsoleCommand(ReadOnlySpan<string> args, bool save = true)
    {
        List<string> result = [];
        if (!ResolveField(args, result, out var selectedNode, out var selectedField))
            return result;

        try
        {
            if (args.Length == 2)
            {
                result.Add(selectedField.Getter(selectedNode)?.ToString() ?? $"读取配置项“{selectedField.Name}”失败");
            }
            else
            {
                var value = FromConsoleString(args[2], selectedField.FieldType);
                if (value == null)
                {
                    result.Add($"无法将“{args[2]}”转换为 {selectedField.FieldType}");
                }
                else
                {
                    // an explicit set replaces any temporary override of this field
                    lock (_transient)
                    {
                        if (_transient.Remove((selectedNode, selectedField)))
                            ++_transientVersion;
                        selectedField.Setter(selectedNode, value);
                    }
                    if (save)
                        selectedNode.Modified.Fire();
                }
            }
        }
        catch (Exception e)
        {
            result.Add(args.Length == 2
                ? $"读取配置项 {selectedNode.GetType().Name}.{selectedField.Name} 失败：{e}"
                : $"无法将 {selectedNode.GetType().Name}.{selectedField.Name} 设置为“{args[2]}”：{e}");
        }
        return result;
    }

    // same arguments as ConsoleCommand, but the value is never saved: the config file keeps the user's value, which is restored by ClearTransient
    // if the user changes the field while it is overridden, their new value is kept instead
    public List<string> SetTransient(ReadOnlySpan<string> args)
    {
        List<string> result = [];
        if (!ResolveField(args, result, out var selectedNode, out var selectedField))
            return result;

        if (args.Length < 3)
        {
            result.Add($"缺少配置项 {selectedNode.GetType().Name}.{selectedField.Name} 的值");
            return result;
        }

        try
        {
            var value = FromConsoleString(args[2], selectedField.FieldType);
            if (value == null)
            {
                result.Add($"无法将“{args[2]}”转换为 {selectedField.FieldType}");
                return result;
            }

            // the value is changed under the lock, so that a concurrent save sees it together with its override entry
            lock (_transient)
            {
                // if the field is already overridden, keep the user's value from before the first override
                var original = _transient.TryGetValue((selectedNode, selectedField), out var existing) ? existing.Original : selectedField.Getter(selectedNode);
                _transient[(selectedNode, selectedField)] = new(original, value);
                ++_transientVersion;
                selectedField.Setter(selectedNode, value);
            }
            selectedNode.Modified.Fire();
        }
        catch (Exception e)
        {
            result.Add($"无法将 {selectedNode.GetType().Name}.{selectedField.Name} 设置为“{args[2]}”：{e}");
        }
        return result;
    }

    // restores the user's values of all temporarily overridden fields; returns number of fields restored
    public int ClearTransient()
    {
        HashSet<ConfigNode> modified = [];
        var restored = 0;
        lock (_transient)
        {
            if (_transient.Count == 0)
                return 0;

            ++_transientVersion;
            foreach (var ((node, field), entry) in _transient)
            {
                // don't clobber the field if something has changed it in the meantime
                if (Equals(field.Getter(node), entry.Value))
                {
                    field.Setter(node, entry.Original);
                    modified.Add(node);
                    ++restored;
                }
            }
            _transient.Clear();
        }
        foreach (var node in modified)
            node.Modified.Fire();
        return restored;
    }

    private (Dictionary<(ConfigNode node, ConfigFieldMetadata field), TransientOverride>, int version) TransientSnapshot()
    {
        lock (_transient)
        {
            return (new(_transient), _transientVersion);
        }
    }

    internal bool IsTransient(ConfigNode node, ConfigFieldMetadata field)
    {
        lock (_transient)
        {
            return _transient.ContainsKey((node, field));
        }
    }

    // a field that no longer has the overridden value was changed by the user (e.g. via the UI), so the new value should be saved
    private void DropChangedTransients(ConfigNode node)
    {
        lock (_transient)
        {
            foreach (var (key, entry) in _transient)
            {
                if (key.node == node && !Equals(key.field.Getter(node), entry.Value))
                {
                    _transient.Remove(key);
                    ++_transientVersion;
                }
            }
        }
    }

    private bool ResolveField(ReadOnlySpan<string> args, List<string> result, [NotNullWhen(true)] out ConfigNode? selectedNode, [NotNullWhen(true)] out ConfigFieldMetadata? selectedField)
    {
        selectedNode = null;
        selectedField = null;
        if (args.Length == 0)
        {
            result.Add("用法：/bmr cfg <配置类型> <字段> <值>");
            result.Add("配置类型和字段名均可使用缩写。有效的配置类型：");
            foreach (var type in _nodes.Keys)
                result.Add($"- {type.Name}");
            return false;
        }

        List<ConfigNode> matchingNodes = [];
        foreach (var (type, node) in _nodes)
        {
            var arg = args[0];
            if (!type.Name.Contains(arg, StringComparison.CurrentCultureIgnoreCase))
                continue;
            if (type.Name.Length == arg.Length)
            {
                matchingNodes.Clear();
                matchingNodes.Add(node);
                break;
            }
            matchingNodes.Add(node);
        }

        if (matchingNodes.Count == 0)
        {
            result.Add("未找到该配置类型。有效类型：");
            foreach (var type in _nodes.Keys)
                result.Add($"- {type.Name}");
            return false;
        }
        if (matchingNodes.Count > 1)
        {
            result.Add("配置类型不唯一，请输入更长的名称。匹配项：");
            foreach (var node in matchingNodes)
                result.Add($"- {node.GetType().Name}");
            return false;
        }

        selectedNode = matchingNodes[0];
        var fields = GeneratedConfigMetadata.Get(selectedNode).DisplayFields;
        if (args.Length == 1)
        {
            result.Add("用法：/bmr cfg <配置类型> <字段> <值>");
            result.Add($"{selectedNode.GetType().Name} 的有效字段：");
            foreach (var field in fields)
                result.Add($"- {field.Name}");
            return false;
        }

        List<ConfigFieldMetadata> matchingFields = [];
        foreach (var field in fields)
        {
            var arg = args[1];
            if (!field.Name.Contains(arg, StringComparison.CurrentCultureIgnoreCase))
                continue;
            if (field.Name.Length == arg.Length)
            {
                matchingFields.Clear();
                matchingFields.Add(field);
                break;
            }
            matchingFields.Add(field);
        }

        if (matchingFields.Count == 0)
        {
            result.Add($"未找到字段 {args[1]}。有效字段：");
            foreach (var field in fields)
                result.Add($"- {field.Name}");
            return false;
        }
        if (matchingFields.Count > 1)
        {
            result.Add("字段名不唯一，请输入更长的名称。匹配项：");
            foreach (var field in matchingFields)
                result.Add($"- {field.Name}");
            return false;
        }

        selectedField = matchingFields[0];
        return true;
    }

    private static object? FromConsoleString(string str, Type type)
        => type == typeof(bool) ? bool.Parse(str)
        : type == typeof(float) ? float.Parse(str)
        : type == typeof(int) ? int.Parse(str)
        : GeneratedEnumMetadata.IsRegistered(type) ? GeneratedEnumMetadata.Parse(type, str)
        : null;
}
