namespace BossMod;

// class that creates and manages instances of proper boss modules in response to world state changes
public sealed class BossModuleManager : IDisposable
{
    public readonly WorldState WorldState;
    public readonly RaidCooldowns RaidCooldowns;
    public static readonly BossModuleConfig Config = Service.Config.Get<BossModuleConfig>();
    private readonly EventSubscriptions _subsciptions;

    public readonly List<BossModule> PendingModules = [];
    public readonly List<BossModule> LoadedModules = [];
    public Event<BossModule> ModuleLoaded = new();
    public Event<BossModule> ModuleUnloaded = new();
    public Event<BossModule> ModuleActivated = new();
    public Event<BossModule> ModuleDeactivated = new();

    // drawn module among loaded modules; this can be changed explicitly if needed
    // usually we don't have multiple concurrently active modules, since this prevents meaningful cd planning, raid cooldown tracking, etc.
    // but it can theoretically happen e.g. around checkpoints and in typically trivial outdoor content
    private BossModule? _activeModule;
    private bool _activeModuleOverridden;
    public BossModule? ActiveModule
    {
        get => _activeModule;
        set
        {
            Service.Log($"[BMM] Active module override: from {_activeModule?.GetType().FullName ?? "<n/a>"} (manual-override={_activeModuleOverridden}) to {value?.GetType().FullName ?? "<n/a>"}");
            _activeModule = value;
            _activeModuleOverridden = true;
        }
    }

    public BossModuleManager(WorldState ws)
    {
        WorldState = ws;
        RaidCooldowns = new(ws);
        _subsciptions = new
        (
            WorldState.Actors.Added.Subscribe(ActorAdded),
            WorldState.DirectorUpdate.Subscribe(OnDirectorUpdate),
            WorldState.CurrentZoneChanged.Subscribe(OnZoneChange),
            Config.Modified.ExecuteAndSubscribe(ConfigChanged)
        );

        foreach (var a in WorldState.Actors)
        {
            ActorAdded(a);
        }
    }

    public void Dispose()
    {
        _activeModule = null;
        foreach (var m in LoadedModules)
        {
            m.Dispose();
        }

        LoadedModules.Clear();

        foreach (var m in PendingModules)
        {
            m.Dispose();
        }
        PendingModules.Clear();

        _subsciptions.Dispose();
        RaidCooldowns.Dispose();
    }

    public void Update()
    {
        // update all loaded modules, handle activation/deactivation
        var bestPriority = 0;
        BossModule? bestModule = null;
        var anyModuleActivated = false;

        if (WorldState.Party[0]?.PosRot.AsVector3() is Vector3 playerPos)
        {
            var countP = PendingModules.Count - 1;
            var maxDist = Config.MaxLoadDistance;
            if (maxDist < 100f) // protect users from themselves, distances smaller than 100 could make make modules disappear if boss and player stand at the opposite sides of an arena
            {
                maxDist = 100f;
            }
            var maxSq = maxDist * maxDist;
            for (var i = countP; i >= 0; --i)
            {
                var m = PendingModules[i];
                var prim = m.PrimaryActor;
                if (prim.IsDeadOrDestroyed)
                {
                    PendingModules.RemoveAt(i);
                    m.Dispose();
                    continue;
                }
                if (!ModuleEligible(m))
                {
                    continue;
                }
                if (m.OnlyLoadIfTargetable && !prim.IsTargetable)
                {
                    continue;
                }
                if ((playerPos - prim.PosRot.AsVector3()).LengthSquared() <= maxSq)
                {
                    var countL = LoadedModules.Count;
                    var oid = prim.OID;

                    var exists = false;
                    for (var j = 0; j < countL; ++j)
                    {
                        if (oid == LoadedModules[j].PrimaryActor.OID)
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (!exists)
                    {
                        LoadedModules.Add(m);
                        Service.Log($"[BMM] Boss module '{m.GetType()}' moved from pending to loaded for actor {prim}");
                        ModuleLoaded.Fire(m);
                        PendingModules.RemoveAt(i);
                    }
                }
            }

            for (var i = 0; i < LoadedModules.Count; ++i)
            {
                var m = LoadedModules[i];
                var wasActive = m.StateMachine.ActiveState != null;
                var actor = m.PrimaryActor;

                bool isActive;

                try
                {
                    if (!_wipeInProgress)
                    {
                        m.Update();
                    }
                    isActive = m.StateMachine.ActiveState != null;
                }
                catch (Exception ex)
                {
                    Service.Log($"Boss module {m.GetType()} crashed: {ex}");
                    wasActive = true; // force unload if exception happened before activation
                    isActive = false;
                }

                // if module was activated or deactivated, notify listeners
                if (isActive != wasActive)
                {
                    (isActive ? ModuleActivated : ModuleDeactivated).Fire(m);
                }

                // unload module because player is out of desired range
                if ((playerPos - actor.PosRot.AsVector3()).LengthSquared() > maxSq && actor.SpawnIndex != -99)
                {
                    MoveModuleToPending(i--);
                    continue;
                }

                // unload module either if it became deactivated or its primary actor disappeared without ever activating
                if (!isActive && (wasActive || actor.IsDestroyed))
                {
                    UnloadModule(i--);
                    continue;
                }

                // if module is active and wants to be reset, oblige
                if (isActive && m.CheckReset())
                {
                    ModuleDeactivated.Fire(m);
                    UnloadModule(i--);
                    if (!actor.IsDestroyed)
                    {
                        ActorAdded(actor);
                    }

                    continue;
                }

                // module remains loaded
                var priority = ModuleDisplayPriority(m);
                if (priority > bestPriority)
                {
                    bestPriority = priority;
                    bestModule = m;
                }

                if (!wasActive && isActive)
                {
                    Service.Log($"[BMM] Boss module '{m.GetType()}' for actor {actor.InstanceID:X} ({actor.OID:X}) '{actor.Name}' activated");
                    anyModuleActivated |= true;
                }
            }

            var curPriority = ModuleDisplayPriority(_activeModule);
            if (bestPriority > curPriority && (anyModuleActivated || !_activeModuleOverridden))
            {
                Service.Log($"[BMM] Active module change: from {_activeModule?.GetType().FullName ?? "<n/a>"} (prio {curPriority}, manual-override={_activeModuleOverridden}) to {bestModule?.GetType().FullName ?? "<n/a>"} (prio {bestPriority})");
                _activeModule = bestModule;
                _activeModuleOverridden = false;
            }
        }
    }

    private void LoadModule(BossModule m, bool oidExists = false)
    {
        var maxDist = Config.MaxLoadDistance;
        if (ModuleEligible(m) && !oidExists && WorldState.Party[0]?.PosRot.AsVector3() is Vector3 playerPos && (playerPos - m.PrimaryActor.PosRot.AsVector3()).LengthSquared() <= maxDist * maxDist)
        {
            LoadedModules.Add(m);
            Service.Log($"[BMM] Boss module '{m.GetType()}' loaded for actor {m.PrimaryActor}");
            ModuleLoaded.Fire(m);
        }
        else
        {
            PendingModules.Add(m);
            Service.Log($"[BMM] Boss module '{m.GetType()}' loaded as pending for actor {m.PrimaryActor}");
        }
    }

    private static bool ModuleEligible(BossModule m)
    {
        if (m is DemoModule || m.Info == null)
        {
            return true;
        }
        return m.Info.Maturity >= Config.MinMaturity && Config.IsModuleEnabled(m.Info.PrimaryActorOID);
    }

    private void UnloadModule(int index)
    {
        var m = LoadedModules[index];
        Service.Log($"[BMM] Boss module '{m.GetType()}' unloaded for actor {m.PrimaryActor}");
        ModuleUnloaded.Fire(m);
        if (_activeModule == m)
        {
            _activeModule = null;
            _activeModuleOverridden = false;
        }
        m.Dispose();
        LoadedModules.RemoveAt(index);
    }

    private void MoveModuleToPending(int index)
    {
        var m = LoadedModules[index];
        Service.Log($"[BMM] Boss module '{m.GetType()}' for actor {m.PrimaryActor} was moved to pending status");
        if (_activeModule == m)
        {
            _activeModule = null;
            _activeModuleOverridden = false;
            ModuleDeactivated.Fire(m);
        }
        LoadedModules.RemoveAt(index);
        PendingModules.Add(m);
    }

    private static int ModuleDisplayPriority(BossModule? m)
    {
        if (m == null)
        {
            return 0;
        }

        if (m.StateMachine.ActiveState != null)
        {
            return 4;
        }

        if (m.PrimaryActor.InstanceID == default)
        {
            return 2; // demo module
        }

        if (!m.PrimaryActor.IsDestroyed && !m.PrimaryActor.IsDead && m.PrimaryActor.IsTargetable)
        {
            return 3;
        }

        return 1;
    }

    private DemoModule CreateDemoModule() => new(WorldState, new(default, default, -99, default, default!, default, default, default, default, WorldState.Party[0]?.PosRot ?? default));

    private void ActorAdded(Actor actor)
    {
        // Actor objects can be replaced while retaining instance id (eg due to teleports at Necron).
        // Keep both loaded and pending modules pointed at the current object instead of creating duplicates.
        var countL = LoadedModules.Count;
        var oid = actor.OID;
        var iid = actor.InstanceID;
        for (var i = 0; i < countL; ++i)
        {
            ref var prim = ref LoadedModules[i].PrimaryActor;
            if (prim.OID == oid && prim.InstanceID == iid)
            {
                prim = actor;
                return;
            }
        }
        var countP = PendingModules.Count;
        for (var i = 0; i < countP; ++i)
        {
            ref var prim = ref PendingModules[i].PrimaryActor;
            if (prim.OID == oid && prim.InstanceID == iid)
            {
                prim = actor;
                return;
            }
        }

        // Always construct a known module, even when maturity/explicit enable settings currently prevent loading.
        // Such modules remain pending so a setting change can activate them without requiring the actor to respawn.
        var m = BossModuleRegistry.CreateModuleForActor(WorldState, actor);
        if (m == null)
        {
            return;
        }

        var oidLoaded = false;
        for (var i = 0; i < countL; ++i)
        {
            if (LoadedModules[i].PrimaryActor.OID == oid)
            {
                oidLoaded = true;
                break;
            }
        }
        LoadModule(m, oidLoaded || m.OnlyLoadIfTargetable);
    }

    private void ConfigChanged()
    {
        var demoIndex = LoadedModules.FindIndex(m => m is DemoModule);
        if (Config.ShowDemo && demoIndex < 0)
        {
            LoadModule(CreateDemoModule());
        }
        else if (!Config.ShowDemo && demoIndex >= 0)
        {
            UnloadModule(demoIndex);
        }

        // Maturity and Supported-fights enable state are load eligibility, not discovery filters.
        // If a loaded module becomes ineligible, move it to pending
        for (var i = LoadedModules.Count - 1; i >= 0; --i)
        {
            var m = LoadedModules[i];
            if (m is DemoModule || ModuleEligible(m))
            {
                continue;
            }

            MoveModuleToPending(i);
        }
    }

    private bool _wipeInProgress;

    private void OnDirectorUpdate(WorldState.OpDirectorUpdate diru)
    {
        switch (diru.UpdateID)
        {
            case 0x4000_0005u:
                _wipeInProgress = true;
                ForceUnload("wipe");
                break;
            // TODO: reverse these; 0005 is referenced in Dalamud as the DutyWipe op, but there are a few different IDs that are always triggered after wipe, including 000F, 0011, 0013
            // 0006 is Duty Recommenced, but is unsuitable here because it fires after actors are recreated (at least i think it does lol i didnt check)
            case 0x4000_0011u:
                _wipeInProgress = false;
                break;
        }
    }

    private void OnZoneChange(WorldState.OpZoneChange zc)
    {
        ForceUnload("ZoneInit");
        _wipeInProgress = false;
    }

    public void ForceUnload(string? cause = null)
    {
        if (cause != null)
        {
            Service.Log($"[BMM] Unload requested with cause: {cause}");
        }

        var count = LoadedModules.Count;
        for (var i = count - 1; i >= 0; --i)
        {
            var m = LoadedModules[i];
            if (m.StateMachine.ActiveState != null)
            {
                ModuleDeactivated.Fire(m);
            }
            UnloadModule(i);
        }
        count = PendingModules.Count;
        for (var i = count - 1; i >= 0; --i)
        {
            var m = PendingModules[i];
            m.Dispose();
            PendingModules.RemoveAt(i);
        }
    }
}
