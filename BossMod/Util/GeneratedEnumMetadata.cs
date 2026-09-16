namespace BossMod;

internal sealed class EnumMetadata(string[] names, ulong[] rawValues, Array? values, string[]? displayNames,
    Attribute[][]? attributes, Func<Enum, ulong> rawValue, Func<ulong, Enum> valueFactory)
{
    public readonly string[] Names = names;
    public readonly ulong[] RawValues = rawValues;
    public readonly Array? Values = values;
    public readonly string[] DisplayNames = displayNames ?? names;
    public readonly Attribute[][]? Attributes = attributes;
    public readonly Func<Enum, ulong> RawValue = rawValue;
    public readonly Func<ulong, Enum> ValueFactory = valueFactory;

    public int IndexOf(ulong raw)
    {
        var len = RawValues.Length;
        for (var i = 0; i < len; ++i)
        {
            if (RawValues[i] == raw)
            {
                return i;
            }
        }
        return -1;
    }
}

// Lazy compile-time enum tables. Convention ID enums retain only names/raw values; enums used by UI/config/strategies also contain boxed value tables
// and member attributes.
internal static partial class GeneratedEnumMetadata
{
    private static readonly Dictionary<Type, Lazy<EnumMetadata>> _byType = Build();

    private static class Cache<T> where T : struct, Enum
    {
        // don't follow the compiler suggestion, Get<T>() will cause a circular initialization
        public static readonly EnumMetadata Metadata = Get(typeof(T));
    }

    private static Dictionary<Type, Lazy<EnumMetadata>> Build()
    {
        Dictionary<Type, Lazy<EnumMetadata>> result = [];
        Register(result);
        return result;
    }

    private static partial void Register(Dictionary<Type, Lazy<EnumMetadata>> metadata);

    public static bool IsRegistered(Type enumType) => _byType.ContainsKey(enumType);

    public static EnumMetadata Get(Type enumType)
        => _byType.GetValueOrDefault(enumType)?.Value ?? throw new ArgumentException($"No generated enum metadata for {enumType.FullName}");

    private static EnumMetadata Get<T>() where T : struct, Enum
        => Cache<T>.Metadata;

    public static T[] Values<T>() where T : struct, Enum
        => Get<T>().Values as T[] ?? throw new ArgumentException($"Enum {typeof(T).FullName} has name-only generated metadata");

    public static Array Values(Type enumType)
        => Get(enumType).Values ?? throw new ArgumentException($"Enum {enumType.FullName} has name-only generated metadata");

    public static string[] Names<T>() where T : struct, Enum => Get<T>().Names;
    public static string[] Names(Type enumType) => Get(enumType).Names;
    public static int Count(Type enumType) => Get(enumType).RawValues.Length;

    public static string? Name(Type enumType, ulong raw)
    {
        var metadata = Get(enumType);
        var index = metadata.IndexOf(raw);
        return index >= 0 ? metadata.Names[index] : null;
    }

    public static Enum ValueByRaw(Type enumType, ulong raw) => Get(enumType).ValueFactory(raw);

    public static int IndexOf<T>(T value) where T : struct, Enum
    {
        var metadata = Get<T>();
        return IndexOf(metadata, value);
    }

    public static string DisplayName<T>(T value) where T : struct, Enum
    {
        var metadata = Get<T>();
        var index = IndexOf(metadata, value);
        return index >= 0 ? metadata.DisplayNames[index] : value.ToString();
    }

    public static EnumValue<T> For<T>(T value) where T : struct, Enum => new(value);

    public readonly struct EnumValue<T>(T value) where T : struct, Enum
    {
        public TAttribute? Attribute<TAttribute>() where TAttribute : Attribute
        {
            var metadata = Get<T>();
            return GeneratedEnumMetadata.Attribute<TAttribute>(metadata, IndexOf(metadata, value));
        }
    }

    public static int IndexOf(Type enumType, Enum value)
    {
        var metadata = Get(enumType);
        return IndexOf(metadata, metadata.RawValue(value));
    }

    public static string DisplayName(Type enumType, Enum value)
    {
        var metadata = Get(enumType);
        return DisplayName(metadata, metadata.RawValue(value), value);
    }

    public static TAttribute? Attribute<TAttribute>(Type enumType, Enum value) where TAttribute : Attribute
    {
        var metadata = Get(enumType);
        return Attribute<TAttribute>(metadata, metadata.RawValue(value));
    }

    private static int IndexOf(EnumMetadata metadata, ulong raw) => metadata.IndexOf(raw);

    private static int IndexOf<T>(EnumMetadata metadata, T value) where T : struct, Enum
    {
        if (metadata.Values is not T[] values)
        {
            throw new ArgumentException($"Enum {typeof(T).FullName} has name-only generated metadata");
        }
        return Array.IndexOf(values, value);
    }

    private static string DisplayName<T>(EnumMetadata metadata, ulong raw, T value) where T : Enum
    {
        var index = metadata.IndexOf(raw);
        return index >= 0 ? metadata.DisplayNames[index] : value.ToString();
    }

    private static TAttribute? Attribute<TAttribute>(EnumMetadata metadata, ulong raw) where TAttribute : Attribute
        => Attribute<TAttribute>(metadata, metadata.IndexOf(raw));

    private static TAttribute? Attribute<TAttribute>(EnumMetadata metadata, int index) where TAttribute : Attribute
    {
        if (index < 0 || metadata.Attributes == null)
        {
            return null;
        }
        var ats = metadata.Attributes[index];
        var len = ats.Length;
        for (var i = 0; i < len; ++i)
        {
            if (ats[i] is TAttribute typed)
            {
                return typed;
            }
        }
        return null;
    }

    public static object Parse(Type enumType, string name)
    {
        var metadata = Get(enumType);
        ulong combined = 0;
        var foundAny = false;
        var names = name.Split(',');
        var len = names.Length;
        for (var i = 0; i < len; ++i)
        {
            var token = names[i].Trim();
            var found = false;
            var namesA = metadata.Names;
            var lenN = namesA.Length;
            for (var j = 0; j < lenN; ++j)
            {
                if (namesA[j] != token)
                {
                    continue;
                }
                combined |= metadata.RawValues[j];
                foundAny = found = true;
                break;
            }
            if (!found)
            {
                if (name.IndexOf(',') < 0)
                {
                    if (ulong.TryParse(token, out var unsigned))
                    {
                        return metadata.ValueFactory(unsigned);
                    }
                    if (long.TryParse(token, out var signed))
                    {
                        return metadata.ValueFactory((ulong)signed);
                    }
                }
                throw new ArgumentException($"Requested value '{name}' was not found in enum {enumType.FullName}");
            }
        }
        if (foundAny)
        {
            return metadata.ValueFactory(combined);
        }
        throw new ArgumentException($"Requested value '{name}' was not found in enum {enumType.FullName}");
    }

    public static T Parse<T>(string name) where T : Enum => (T)Parse(typeof(T), name);
}

internal static class GeneratedEnumExtensions
{
    public static string? GeneratedEnumName(this Type enumType, ulong raw) => GeneratedEnumMetadata.Name(enumType, raw);
    public static Array GeneratedEnumValues(this Type enumType) => GeneratedEnumMetadata.Values(enumType);
    public static string[] GeneratedEnumNames(this Type enumType) => GeneratedEnumMetadata.Names(enumType);
}
