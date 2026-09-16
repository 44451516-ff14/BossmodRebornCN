using System.IO;
using System.Text.Json;

namespace BossMod;

public sealed class ConfigRoot
{
    public Event Modified = new();
    public readonly Dictionary<Type, ConfigNode> _nodes = [];
    private readonly Dictionary<string, ConfigNode> _nodesByName = [];

    public void Initialize() => GeneratedRegistries.RegisterConfigNodes(RegisterNode);

    private void RegisterNode(Type type, ConfigNode node)
    {
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
            Parallel.ForEach(_nodes, entry =>
            {
                using var ms = new MemoryStream();
                using var tempWriter = new Utf8JsonWriter(ms);
                entry.Value.Serialize(tempWriter, ser);
                tempWriter.Flush();
                serializedNodes[entry.Key] = Encoding.UTF8.GetString(ms.ToArray());
            });

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
        if (args.Length == 0)
        {
            result.Add("Usage: /bmr cfg <config-type> <field> <value>");
            result.Add("Both config-type and field can be shortened. Valid config-types:");
            foreach (var type in _nodes.Keys)
                result.Add($"- {type.Name}");
            return result;
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
            result.Add("Config type not found. Valid types:");
            foreach (var type in _nodes.Keys)
                result.Add($"- {type.Name}");
            return result;
        }
        if (matchingNodes.Count > 1)
        {
            result.Add("Ambiguous config type, pass longer pattern. Matches:");
            foreach (var node in matchingNodes)
                result.Add($"- {node.GetType().Name}");
            return result;
        }

        var selectedNode = matchingNodes[0];
        var fields = GeneratedConfigMetadata.Get(selectedNode).DisplayFields;
        if (args.Length == 1)
        {
            result.Add("Usage: /bmr cfg <config-type> <field> <value>");
            result.Add($"Valid fields for {selectedNode.GetType().Name}:");
            foreach (var field in fields)
                result.Add($"- {field.Name}");
            return result;
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
            result.Add($"Field not found {args[1]}, Valid fields:");
            foreach (var field in fields)
                result.Add($"- {field.Name}");
            return result;
        }
        if (matchingFields.Count > 1)
        {
            result.Add("Ambiguous field name, pass longer pattern. Matches:");
            foreach (var field in matchingFields)
                result.Add($"- {field.Name}");
            return result;
        }

        var selectedField = matchingFields[0];
        try
        {
            if (args.Length == 2)
            {
                result.Add(selectedField.Getter(selectedNode)?.ToString() ?? $"Failed to get value of '{selectedField.Name}'");
            }
            else
            {
                var value = FromConsoleString(args[2], selectedField.FieldType);
                if (value == null)
                {
                    result.Add($"Failed to convert '{args[2]}' to {selectedField.FieldType}");
                }
                else
                {
                    selectedField.Setter(selectedNode, value);
                    if (save)
                        selectedNode.Modified.Fire();
                }
            }
        }
        catch (Exception e)
        {
            result.Add(args.Length == 2
                ? $"Failed to get value of {selectedNode.GetType().Name}.{selectedField.Name}: {e}"
                : $"Failed to set {selectedNode.GetType().Name}.{selectedField.Name} to {args[2]}: {e}");
        }
        return result;
    }

    private static object? FromConsoleString(string str, Type type)
        => type == typeof(bool) ? bool.Parse(str)
        : type == typeof(float) ? float.Parse(str)
        : type == typeof(int) ? int.Parse(str)
        : GeneratedEnumMetadata.IsRegistered(type) ? GeneratedEnumMetadata.Parse(type, str)
        : null;
}
