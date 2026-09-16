namespace BossMod;

public static class BossModuleRegistry
{
    public sealed class Info
    {
        public Type ModuleType;
        public Type StatesType;
        public Type? ConfigType;
        public Type? ObjectIDType;
        public Type? ActionIDType;
        public Type? StatusIDType;
        public Type? TetherIDType;
        public Type? IconIDType;
        public uint PrimaryActorOID;
        public bool HasPrePullHints;
        public Func<WorldState, Actor, BossModule> ModuleFactory;
        public Func<BossModule, StateMachine> StateMachineFactory;

        public BossModuleInfo.Maturity Maturity;
        public string Contributors;
        public BossModuleInfo.Expansion Expansion;
        public BossModuleInfo.Category Category;
        public BossModuleInfo.GroupType GroupType;
        public uint GroupID;
        public uint NameID;
        public int SortOrder;
        public int PlanLevel;

        internal Info(Type moduleType, Type statesType, Type? configType, Type? objectIDType, Type? actionIDType,
            Type? statusIDType, Type? tetherIDType, Type? iconIDType, uint primaryActorOID, bool hasPrePullHints, Func<WorldState, Actor, BossModule> moduleFactory,
            Func<BossModule, StateMachine> stateMachineFactory, BossModuleInfo.Maturity maturity, string contributors, BossModuleInfo.Expansion expansion,
            BossModuleInfo.Category category, BossModuleInfo.GroupType groupType, uint groupID, uint nameID, int sortOrder, int planLevel)
        {
            ModuleType = moduleType;
            StatesType = statesType;
            ConfigType = configType;
            ObjectIDType = objectIDType;
            ActionIDType = actionIDType;
            StatusIDType = statusIDType;
            TetherIDType = tetherIDType;
            IconIDType = iconIDType;
            PrimaryActorOID = primaryActorOID;
            HasPrePullHints = hasPrePullHints;
            ModuleFactory = moduleFactory;
            StateMachineFactory = stateMachineFactory;
            Maturity = maturity;
            Contributors = contributors;
            Expansion = expansion;
            Category = category;
            GroupType = groupType;
            GroupID = groupID;
            NameID = nameID;
            SortOrder = sortOrder;
            PlanLevel = planLevel;
        }
    }

    public static readonly Dictionary<uint, Info> RegisteredModules = []; // [primary-actor-oid] = module info
    private static readonly Dictionary<Type, Info> _modulesByType = []; // [module-type] = module info
    private static readonly Dictionary<string, Info> _modulesByName = []; // [module-type-full-name] = module info

    static BossModuleRegistry() => GeneratedRegistries.RegisterBossModules(Register);

    private static void Register(Info info)
    {
        var type = info.ModuleType;
        _modulesByType[type] = info;
        if (type.FullName is { } fullName)
        {
            _modulesByName[fullName] = info;
        }
        if (!RegisteredModules.TryAdd(info.PrimaryActorOID, info))
        {
            Service.Log($"[ModuleRegistry] Two boss modules have same primary actor OID: {type.FullName} and {RegisteredModules[info.PrimaryActorOID].ModuleType.FullName}");
        }
    }

    public static Info? FindByOID(uint oid) => RegisteredModules.GetValueOrDefault(oid);
    public static Info? FindByType(Type type) => _modulesByType.GetValueOrDefault(type);
    public static Info? FindByTypeName(string typeName)
    {
        var assemblySeparator = typeName.IndexOf(',');
        return _modulesByName.GetValueOrDefault(assemblySeparator >= 0 ? typeName[..assemblySeparator].Trim() : typeName);
    }

    public static BossModule? CreateModule(Info? info, WorldState ws, Actor primary) => info?.ModuleFactory(ws, primary);

    public static BossModule? CreateModuleForActor(WorldState ws, Actor primary)
    {
        if (primary.Type is not ActorType.Enemy and not ActorType.EventObj)
        {
            return null;
        }

        return CreateModule(FindByOID(primary.OID), ws, primary);
    }

    // TODO: this is a hack...
    public static BossModule? CreateModuleForConfigPlanning(Type module)
    {
        var info = FindByType(module);
        return info != null ? CreateModule(info, new(TimeSpan.TicksPerSecond, "fake"), new(0, info.PrimaryActorOID, -1, 0, "", 0, ActorType.None, Class.None, 0, default)) : null;
    }

    // TODO: this is a hack...
    public static BossModule? CreateModuleForTimeline(uint oid) => CreateModule(FindByOID(oid), new(TimeSpan.TicksPerSecond, "fake"), new(0, oid, -1, 0, "", 0, ActorType.None, Class.None, 0, default));
}
