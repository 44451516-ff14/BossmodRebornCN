namespace BossMod;

internal static partial class GeneratedFactories
{
    private static readonly Lazy<Dictionary<string, Type>> _typesByName = new(BuildTypes);

    private static Dictionary<string, Type> BuildTypes()
    {
        Dictionary<string, Type> result = [];
        RegisterTypes(result);
        return result;
    }

    private static partial void RegisterTypes(Dictionary<string, Type> types);

    internal static partial bool TryCreateStrategyRenderer(Type type, out Autorotation.IStrategyRenderer renderer);

    internal static Autorotation.IStrategyRenderer CreateStrategyRenderer(Type type)
        => TryCreateStrategyRenderer(type, out var renderer) ? renderer : throw new ArgumentException($"No generated strategy renderer factory for {type.FullName}");

    internal static partial bool TryCreatePropertyRenderer(Type type, out PropertyRenderer renderer);

    internal static PropertyRenderer CreatePropertyRenderer(Type type)
        => TryCreatePropertyRenderer(type, out var renderer) ? renderer : throw new ArgumentException($"No generated property renderer factory for {type.FullName}");

    internal static partial R CreateUnmanagedRotation<R>(BossModule module) where R : QuestBattle.UnmanagedRotation;

    public static Type? FindType(string name)
    {
        var assemblySeparator = name.IndexOf(',');
        return _typesByName.Value.GetValueOrDefault(assemblySeparator >= 0 ? name[..assemblySeparator].Trim() : name);
    }
}
