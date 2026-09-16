namespace BossMod;

// Stable compile-time contract implemented by BossMod.SourceGen. Explicitly
// accessible partial methods require generated implementations, so a generator
// load or execution failure is a build error rather than a runtime fallback.
internal static partial class GeneratedRegistries
{
    internal static partial void RegisterBossModules(Action<BossModuleRegistry.Info> register);

    internal static partial void RegisterZoneModules(Action<ZoneModuleRegistry.Info> register);

    internal static partial void RegisterRotationModules(Dictionary<Type, Autorotation.RotationModuleRegistry.Entry> modules,
        Dictionary<string, Type> modulesByName);

    internal static partial void RegisterActionDefinitions(ActionDefinitions definitions);

    internal static partial void RegisterConfigNodes(Action<Type, ConfigNode> register);

    internal static partial bool TryCreateBossComponent<T>(BossModule module, out T component) where T : BossComponent;
}
