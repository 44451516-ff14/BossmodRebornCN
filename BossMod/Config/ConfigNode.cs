using System.Text.Json;
using System.Text.Json.Serialization;

namespace BossMod;

// attribute that specifies how config node should be shown in the UI
[AttributeUsage(AttributeTargets.Class)]
public sealed class ConfigDisplayAttribute : Attribute
{
    public string? Name { get; set; }
    public int Order { get; set; }
    public Type? Parent { get; set; }
    public string[]? Tags { get; set; }
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class SectionStartAttribute(string label = "", bool separator = true) : Attribute
{
    public string Label { get; } = label;
    public bool Separator { get; } = separator;
}

// attribute that specifies how config node field or enumeration value is shown in the UI
[AttributeUsage(AttributeTargets.Field)]
public sealed class PropertyDisplayAttribute(string label, uint color = default, string tooltip = "", string? since = null, string? depends = null, string[]? tags = null, Type? renderer = null, bool separator = false) : Attribute
{
    public string Label { get; } = label;
    public uint Color => color == default ? Colors.TextColor1 : color;
    public string Tooltip { get; } = tooltip;
    public string? Since { get; } = since;
    public string? Depends { get; } = depends;
    public string[] Tags { get; } = tags ?? [];
    public Type? Renderer { get; } = renderer;
    public bool Separator { get; } = separator;
}

// attribute that specifies combobox should be used for displaying int/bool property
[AttributeUsage(AttributeTargets.Field)]
public sealed class PropertyComboAttribute(string[] values) : Attribute
{
    public string[] Values { get; } = values;

#pragma warning disable CA1019
    public PropertyComboAttribute(string falseText, string trueText) : this([falseText, trueText]) { }
#pragma warning restore CA1019
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class PropertyRadioAttribute : Attribute;

// attribute that specifies slider should be used for displaying float/int property
[AttributeUsage(AttributeTargets.Field)]
public sealed class PropertySliderAttribute(float min, float max) : Attribute
{
    public float Speed { get; set; } = 1f;
    public float Min { get; } = min;
    public float Max { get; } = max;
    public bool Logarithmic { get; set; }
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class PropertyStringOrderAttribute(string[] values) : Attribute
{
    public string[] Values { get; } = values;
}

// base class for configuration nodes
public abstract class ConfigNode
{
    // event fired when configuration node was modified; should be fired by anyone making any modifications
    // root subscribes to modification event to save updated configuration
    [JsonIgnore]
    public Event Modified = new();

    public virtual void DrawCustom(UITree tree, WorldState ws) { }

    // deserialize fields from json; default implementation should work fine for most cases
    public virtual void Deserialize(JsonElement j, JsonSerializerOptions ser)
    {
        var agg = new List<JsonException>();
        var metadata = GeneratedConfigMetadata.Get(this);

        foreach (var jfield in j.EnumerateObject())
        {
            if (metadata.FieldsByName.GetValueOrDefault(jfield.Name) is not { Serializable: true } field)
                continue;

            try
            {
                var value = jfield.Value.Deserialize(field.FieldType, ser);
                if (value != null)
                    field.Setter(this, value);
            }
            catch (JsonException ex)
            {
                agg.Add(ex);
            }
        }

        if (agg.Count > 0)
            throw new AggregateException(agg);
    }

    // serialize node to json;
    public virtual void Serialize(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        var fields = GeneratedConfigMetadata.Get(this).SerializableFields;
        var len = fields.Length;
        for (var i = 0; i < len; ++i)
        {
            var field = fields[i];
            var fieldValue = field.Getter(this);
            writer.WritePropertyName(field.Name);
            if (fieldValue is ConfigNode subNode)
            {
                subNode.Serialize(writer, options);
            }
            else
            {
                JsonSerializer.Serialize(writer, fieldValue, field.FieldType, options);
            }
        }
        writer.WriteEndObject();
    }
}

// utility to simplify listening for config modifications; callback is executed immediately for initial state
public sealed class ConfigListener<T>(T data, Action<T> modified) : IDisposable where T : ConfigNode
{
    public readonly T Data = data;
    private readonly EventSubscription _listener = data.Modified.ExecuteAndSubscribe(() => modified(data));

    public void Dispose() => _listener.Dispose();
}

public abstract class PropertyRenderer
{
    public abstract bool Draw(PropertyDisplayAttribute attrs, bool nested, ConfigNode node, object value, ConfigRoot root, UITree tree, WorldState ws);
}
