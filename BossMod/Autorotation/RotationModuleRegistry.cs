namespace BossMod.Autorotation;

// database containing all registered rotation module definitions and builder functions
public static class RotationModuleRegistry
{
    public readonly record struct Entry(RotationModuleDefinition Definition, Func<RotationModuleManager, Actor, RotationModule> Builder);

    private static readonly Dictionary<string, Type> _modulesByName = [];
    public static readonly Dictionary<Type, Entry> Modules = BuildModules();

    private static Dictionary<Type, Entry> BuildModules()
    {
        Dictionary<Type, Entry> res = [];
        GeneratedRegistries.RegisterRotationModules(res, _modulesByName);
        return res;
    }

    public static Type? FindType(string typeName)
    {
        var assemblySeparator = typeName.IndexOf(',');
        return _modulesByName.GetValueOrDefault(assemblySeparator >= 0 ? typeName[..assemblySeparator].Trim() : typeName);
    }
}
