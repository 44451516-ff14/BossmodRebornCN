using Dalamud.Bindings.ImGui;
using System.IO;

namespace BossMod.ReplayVisualization;

sealed class OpList(Replay replay, Replay.Encounter? enc, BossModuleRegistry.Info? moduleInfo, IEnumerable<WorldState.Operation> ops, Action<DateTime> scrollTo)
{
    public readonly Replay.Encounter? Encounter = enc;
    public readonly BossModuleRegistry.Info? ModuleInfo = moduleInfo;
    private DateTime _relativeTS;
    private readonly List<(int Index, DateTime Timestamp, string Text, Action<UITree>? Children, Action? ContextMenu)> _nodes = [];
    private readonly HashSet<uint> _filteredOIDs = [];
    public static readonly HashSet<uint> BoringOIDs = [0x3E1Au, 0x3E1Bu, 0x3E1Cu, 0x447Eu, 0x447Du, 0x4480u, 0x4583u, 0x447Fu, 0x4584u, 0x4570u, 0x4256u, 0x260Eu, 0x260Bu,
    0x2630u, 0x2611u, 0x2610u, 0x2617u, 0x2608u, 0x2613u, 0x2618u, 0x2609u, 0x261Au, 0x262Fu, 0x2609u, 0x2614u, 0x2664u, 0x2668u, 0x2619u, 0x2631u, 0x2632u, 0x260Au, 0x2616u, 0x2667u,
    0x2E7Fu, 0x2F33u, 0x2F32u, 0x2F38u, 0x2E80u, 0x2E82u, 0x2E81u, 0x2F36u, 0x2E7Du, 0x2F35u, 0x2EB0u, 0x2F31u, 0x2F37u, 0x2E7Cu, 0x2E7Bu, 0x2EAEu, 0x2F3Au, 0x2F30u, 0x2E7Eu, 0x2EAFu,
    0x428Bu, 0x44B8u, 0x43D2u, 0x43D1u, 0x41FDu, 0x42A4u, 0x41C5u, 0x30B7u, 0x4021u, 0x4019u, 0x401Cu, 0x401Bu, 0x401Fu, 0x40FBu, 0x4105u, 0x401Du, 0x4102u, 0x4629u, 0x4628u, 0x4631u,
    0x4630u, 0x46D6u, 0xF5Bu, 0xF5Cu, 0x2E20u, 0x2E21u, 0x318Au, 0x2E1Eu, 0x3346u, 0x3353u, 0x31D4u, 0x3345u, 0x3355u, 0x3326u, 0x3344u, 0x31B1u, 0x3343u, 0x1EB165u, 0x1EB166u,
    0x1EB167u, 0x1EB168u, 0x4339u, 0x4144u, 0x4146u, 0x4348u, 0x4339u, 0x4337u, 0x35F5u, 0x3226u, 0x35FAu, 0x35F6u, 0x35F9u, 0x35F7u, 0x361Au, 0x35F5u, 0x34A4u, 0x35F4u, 0x3605u, 0x35F2u,
    0x375Cu, 0x375Au, 0x3759u, 0x375Bu, 0x35E0u, 0x35E1u, 0x35F1u, 0x35F3u, 0x3604u, 0x39BFu, 0x39BDu, 0x39C0u, 0x39C1u, 0x39BEu, 0x402Du, 0x402Eu, 0x40B1u, 0x3D7Fu, 0x3D80u, 0x3D7Eu, 0x465Cu, 0x465Du, 0x465Eu,
    0x466Du, 0x466Eu, 0x466Fu, 0x466Bu, 0x466Cu, 0x2ED7u, 0x2EDBu, 0x2EDAu, 0x2EF2u, 0x2EDCu, 0x2EF5u, 0x2EF6u, 0x2EF4u, 0x2EDDu, 0x2EF1u, 0x2EDCu, 0x2EF3u, 0x2EEEu, 0x2EEDu, 0x2EF0u,
    0x2EEFu, 0x2FCCu, 0x2FCBu, 0x195Du, 0x195Bu, 0x195Cu, 0x338Fu, 0x326Au, 0x3269u, 0x334Bu, 0x3267u, 0x3268u, 0x3266u, 0x31A8u, 0x488Eu, 0x49B3u, 0x49B4u, 0x49B5u,
    0x4A60u, 0x4A59u, 0x4ACEu, 0x4A5Au, 0x4A57u, 0x5CAu, 0x603u, 0x4D99u, 0x4D98u, 0x4D66u, 0x4DAEu, 0x4DB0u, 0x4DAFu, 0x4DACu, 0x4DAEu, 0x4DB1u, 0x4DB2u, 0x4DB3u, 0x4DADu];
    public static readonly HashSet<uint> BoringSIDs = [43u, 44u, 418u, 364u, 902u, 414u, 1050u, 368u, 362u, 1086u, 1461u, 1463u, 365u, 1778u, 1755u, 360u, 1411u,
    2625u, 2626u, 2627u, 2415u, 2449u, 361u, 367u, 2355u, 413u, 4233u, 4244u, 4227u, 4239u, 4226u, 4229u, 4209u, 4265u, 2932u, 4266u, 4267u, 4268u, 4262u, 4228u];
    private readonly HashSet<ActionID> _filteredActions = [];
    private readonly HashSet<uint> _filteredStatuses = [];
    private readonly HashSet<uint> _filteredDirectorUpdateTypes = [];
    private bool _nodesUpToDate;

    public bool ShowActorSizeEvents
    {
        get;
        set
        {
            field = value;
            _nodesUpToDate = false;
        }
    } = false;

    public bool ShowCLMVEvents
    {
        get;
        set
        {
            field = value;
            _nodesUpToDate = false;
        }
    } = false;

    Task _filterTask = Task.CompletedTask;

    void RebuildNodes()
    {
        if (!_filterTask.IsCompleted)
            return;

        _filterTask = Task.Run(() =>
        {
            _nodes.Clear();
            var i = 0;
            foreach (var op in ops)
            {
                if (FilterOp(op))
                {
                    _nodes.Add((i, op.Timestamp, OpName(op), OpChildren(op), OpContextMenu(op)));
                }
                ++i;
            }
            _nodesUpToDate = true;
        });
    }

    public void Draw(UITree tree, DateTime reference)
    {
        //foreach (var n in _tree.Node("Settings"))
        //{
        //    DrawSettings();
        //}

        if (!_nodesUpToDate)
        {
            RebuildNodes();
            ImGui.Text($"Filtering...");
            return;
        }

        var timeRef = ImGui.GetIO().KeyShift && _relativeTS != default ? _relativeTS : reference;

        var c = new ImGuiListClipper();
        c.Begin(_nodes.Count, ImGui.GetFrameHeight() - 2);

        while (c.Step())
        {
            for (var i = c.DisplayStart; i < c.DisplayEnd; ++i)
            {
                var node = _nodes[i];
                foreach (var n in tree.Node($"{(node.Timestamp - timeRef).TotalSeconds:f3}: {node.Text}###{node.Index}", node.Children == null, Colors.TextColor1, node.ContextMenu, () => scrollTo(node.Timestamp), () => _relativeTS = node.Timestamp))
                {
                    node.Children?.Invoke(tree);
                }
            }
        }

        c.End();
    }

    public void ClearFilters()
    {
        _filteredOIDs.Clear();
        _filteredActions.Clear();
        _filteredStatuses.Clear();
        _filteredDirectorUpdateTypes.Clear();
        _nodesUpToDate = false;
    }

    private bool FilterInterestingActor(ulong instanceID, DateTime timestamp, bool allowPlayers)
    {
        var p = replay.FindParticipant(instanceID, timestamp)!;
        if ((p.OwnerID & 0xFF000000) == 0x10000000ul && p.Type != ActorType.Buddy)
        {
            return false; // player's pet/area
        }

        return (p.Type is not ActorType.Player and not ActorType.Buddy and not ActorType.Pet || allowPlayers) && !_filteredOIDs.Contains(p.OID) && !BoringOIDs.Contains(p.OID);
    }

    private bool FilterInterestingStatus(Replay.Status s)
    {
        if (s.Source?.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo or ActorType.Buddy)
        {
            return false; // don't care about statuses applied by players
        }

        if (s.Target.Type is ActorType.Pet)
        {
            return false; // don't care about statuses applied to pets
        }

        if (BoringSIDs.Contains(s.ID))
        {
            return false; // don't care about resurrect-related and other trivial statuses
        }

        if (_filteredOIDs.Contains(s.Target.OID))
        {
            return false; // don't care about filtered out targets
        }

        if (_filteredStatuses.Contains(s.ID))
        {
            return false; // don't care about filtered out statuses
        }

        return true;
    }

    private bool FilterInterestingStatuses(ulong instanceID, int index, DateTime timestamp)
    {
        foreach (var s in FindStatuses(instanceID, index, timestamp))
        {
            if (FilterInterestingStatus(s))
            {
                return true;
            }
        }

        return false;
    }

    private bool FilterOp(WorldState.Operation o) => o switch
    {
        WorldState.OpFrameStart => false,
        WorldState.OpDirectorUpdate op => !_filteredDirectorUpdateTypes.Contains(op.UpdateID),
        ActorState.OpForayInfo => false,
        ActorState.OpCreate op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
        ActorState.OpDestroy op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
        ActorState.OpMove => false,
        ActorState.OpSizeChange op => ShowActorSizeEvents && FilterInterestingActor(op.InstanceID, op.Timestamp, false),
        ActorState.OpHPMP => false,
        ActorState.OpTargetable op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
        ActorState.OpDead op => FilterInterestingActor(op.InstanceID, op.Timestamp, true),
        ActorState.OpCombat => false,
        ActorState.OpAggroPlayer => false,
        ActorState.OpEventState op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
        ActorState.OpTarget => false,
        ActorState.OpCastInfo op => FilterInterestingActor(op.InstanceID, op.Timestamp, false) && !_filteredActions.Contains(FindCast(replay.FindParticipant(op.InstanceID, op.Timestamp), op.Timestamp, op.Value != null)?.ID ?? new()),
        ActorState.OpCastEvent op => FilterInterestingActor(op.InstanceID, op.Timestamp, false) && !_filteredActions.Contains(op.Value.Action),
        ActorState.OpEffectResult => false,
        ActorState.OpStatus op => FilterInterestingStatuses(op.InstanceID, op.Index, op.Timestamp) && FilterInterestingActor(op.InstanceID, op.Timestamp, true),
        ActorState.OpPlayActionTimelineEvent op => FilterInterestingActor(op.InstanceID, op.Timestamp, true),
        ActorState.OpIncomingEffect => false,
        PartyState.OpLimitBreakChange => false,
        PartyState.OpModify => false,
        ActorState.OpModelState op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
        ActorState.OpEventNpcYell op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
        ActorState.OpEventObjectStateChange op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
        ActorState.OpEventObjectAnimation op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
        ActorState.OpRename op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
        ActorState.OpIcon op => FilterInterestingActor(op.InstanceID, op.Timestamp, true),
        ActorState.OpTether op => FilterInterestingActor(op.InstanceID, op.Timestamp, true),
        ActorState.OpRenderflags op => FilterInterestingActor(op.InstanceID, op.Timestamp, false),
        ActorState.OpVisibility => false,
        ClientState.OpActionRequest => false,
        ClientState.OpHateChange => false,
        ClientState.OpActiveCompanionChange => false,
        ClientState.OpActionReject => false,
        ClientState.OpProcTimersChange => false,
        ClientState.OpAnimationLockChange => false,
        ClientState.OpComboChange => false,
        ClientState.OpCooldown => false,
        ClientState.OpForcedMovementDirectionChange => false,
        ClientState.OpFlyingChange => false,
        WorldState.OpRSVData => false,
        ClientState.OpMoveSpeedChange => ShowCLMVEvents,
        NetworkState.OpServerIPC => false,
        _ => true
    };

    private string DumpOp(WorldState.Operation op)
    {
        using var stream = new MemoryStream(1024);
        var writer = new ReplayRecorder.TextOutput(stream, null);
        op.Write(writer);
        writer.Flush();
        stream.Position = 0;
        var bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);
        var start = Array.IndexOf(bytes, (byte)'|') + 1;
        return Encoding.UTF8.GetString(bytes, start, bytes.Length - start);
    }

    private string OpName(WorldState.Operation o) => o switch
    {
        ActorState.OpCreate op => $"Actor create: {ActorString(op.InstanceID, op.Timestamp)} #{op.SpawnIndex}",
        ActorState.OpDestroy op => $"Actor destroy: {ActorString(op.InstanceID, op.Timestamp)}",
        ActorState.OpRename op => $"Actor rename: {ActorString(op.InstanceID, op.Timestamp)} -> {op.Name}",
        ActorState.OpClassChange op => $"Actor class change: {ActorString(op.InstanceID, op.Timestamp)} -> {op.Class} L{op.Level}",
        ActorState.OpTargetable op => $"{(op.Value ? "Targetable" : "Untargetable")}: {ActorString(op.InstanceID, op.Timestamp)}",
        ActorState.OpDead op => $"{(op.Value ? "Die" : "Resurrect")}: {ActorString(op.InstanceID, op.Timestamp)}",
        ActorState.OpAggroPlayer op => $"Aggro player: {ActorString(op.InstanceID, op.Timestamp)} = {op.Has}",
        ActorState.OpEventState op => $"Event state: {ActorString(op.InstanceID, op.Timestamp)} -> {op.Value}",
        ActorState.OpTarget op => $"Target: {ActorString(op.InstanceID, op.Timestamp)} -> {ActorString(op.Value, op.Timestamp)}",
        ActorState.OpMount op => $"Mount: {ActorString(op.InstanceID, op.Timestamp)} = {Service.LuminaRow<Lumina.Excel.Sheets.Mount>(op.Value)?.Singular ?? "<unknown>"}",
        ActorState.OpTether op => $"Tether: {ActorString(op.InstanceID, op.Timestamp)} {op.Value.ID} ({ModuleInfo?.TetherIDType?.GetEnumName(op.Value.ID)}) @ {ActorString(op.Value.Target, op.Timestamp)}",
        ActorState.OpCastInfo op => $"Cast {(op.Value != null ? "started" : "ended")}: {CastString(op.InstanceID, op.Timestamp, op.Value != null)}",
        ActorState.OpCastEvent op => $"Cast event: {ActorString(op.InstanceID, op.Timestamp)}: {op.Value.Action} ({ModuleInfo?.ActionIDType?.GetEnumName(op.Value.Action.ID)}) @ {CastEventTargetString(op.Value, op.Timestamp)} ({op.Value.Targets.Count} targets affected) #{op.Value.GlobalSequence}",
        ActorState.OpStatus op => $"Status change: {ActorString(op.InstanceID, op.Timestamp)} #{op.Index}: {StatusesString(op.InstanceID, op.Index, op.Timestamp)}",
        ActorState.OpIcon op => $"Icon: {ActorString(op.InstanceID, op.Timestamp)} -> {ActorString(op.TargetID, op.Timestamp)}: {op.IconID} ({ModuleInfo?.IconIDType?.GetEnumName(op.IconID)})",
        ActorState.OpVFX op => $"VFX: {ActorString(op.InstanceID, op.Timestamp)} -> {ActorString(op.TargetID, op.Timestamp)}: {op.VfxID}",
        ActorState.OpEventObjectStateChange op => $"EObjState: {ActorString(op.InstanceID, op.Timestamp)} = {op.State:X4}",
        ActorState.OpEventObjectAnimation op => $"EObjAnim: {ActorString(op.InstanceID, op.Timestamp)} = {((uint)op.Param1 << 16) | op.Param2:X8}",
        ActorState.OpPlayActionTimelineEvent op => $"Play action timeline: {ActorString(op.InstanceID, op.Timestamp)} = {op.ActionTimelineID:X4}",
        ActorState.OpPlayActionTimelineSync op => $"Play action timeline multi: {ActorString(op.InstanceID, op.Timestamp)}",
        ActorState.OpEventNpcYell op => $"Yell: {ActorString(op.InstanceID, op.Timestamp)} = {op.Message} '{Service.LuminaRow<Lumina.Excel.Sheets.NpcYell>(op.Message)?.Text}'",
        ActorState.OpRenderflags op => $"Renderflag: {ActorString(op.InstanceID, op.Timestamp)} -> {op.Value}",
        ActorState.OpModelState op => $"Model state: {ActorString(op.InstanceID, op.Timestamp)} -> {op.Value}",
        ClientState.OpDutyActionsChange op => $"Player duty actions change: {string.Join(", ", op.Slots)}",
        ClientState.OpBozjaHolsterChange op => $"Player bozja holster change: {GetOpBozjaHolsterChangeString(op.Contents)}",
        ClientState.OpPlayerStatsChange op => $"Player stats: sks={op.Value.SkillSpeed}, sps={op.Value.SpellSpeed}, haste={op.Value.Haste}",
        ClientState.OpBlueMageSpellsChange op => $"Player BLU spellbook: {GetOpBlueMageSpellsChangeString(op.Values)}",
        ClientState.OpClassJobLevelsChange op => $"Player levels: {string.Join(", ", op.Values)}",
        ClientState.OpActiveFateChange op => $"FATE: {op.Value.ID} '{Service.LuminaRow<Lumina.Excel.Sheets.Fate>(op.Value.ID)?.Name}' {op.Value.Progress}%",
        ClientState.OpActivePetChange op => $"Player pet: {ActorString(op.Value.InstanceID, op.Timestamp)}",
        ClientState.OpInventoryChange op => ItemString(op),
        PartyState.OpModify op => $"Party slot {op.Slot}: {ActorString(op.Member.InstanceId, op.Timestamp)}",
        WorldState.OpDirectorUpdate op => $"DirectorUpdate: DirectorID: {op.DirectorID:X8}, UpdateID: {op.UpdateID:X8}, Params: {op.Param1:X8}|{op.Param2:X8}|{op.Param3:X8}|{op.Param4:X8}",
        WorldState.OpMapEffect op => $"MapEffect: {op.Index:X2} {op.State:X8}",
        WorldState.OpLegacyMapEffect op => $"MapEffect (legacy): seq={op.Sequence} param={op.Param} data={GetOpLegacyMapEffectString(op.Data)}",
        WorldState.OpSystemLogMessage op => $"LogMessage {op.MessageID}: '{Service.LuminaRow<Lumina.Excel.Sheets.LogMessage>(op.MessageID)?.Text}' [{string.Join(", ", op.Args)}]",
        WorldState.OpZoneChange op => $"Zone change: {op.Zone} ({Service.LuminaRow<Lumina.Excel.Sheets.TerritoryType>(op.Zone)?.PlaceName.Value.Name}) / {op.CFCID} ({(op.CFCID > 0 ? Service.LuminaRow<Lumina.Excel.Sheets.ContentFinderCondition>(op.CFCID)?.Name : "n/a")})",
        WaymarkState.OpSignChange op => op.Target == 0 ? $"Sign: {op.ID} cleared" : $"Sign: {op.ID} on {ActorString(op.Target, op.Timestamp)}",
        _ => DumpOp(o)
    };

    private static string GetOpBlueMageSpellsChangeString(uint[] contents)
    {
        var count = contents.Length;
        var str = new string[count];
        for (var i = 0; i < count; ++i)
        {
            var c = contents[i];
            str[i] = $"{new ActionID(ActionType.Spell, c)}";
        }
        return string.Join(", ", str);
    }

    private static string GetOpBozjaHolsterChangeString(List<(BozjaHolsterID entry, byte count)> contents)
    {
        var count = contents.Count;
        var str = new string[count];
        for (var i = 0; i < count; ++i)
        {
            var c = contents[i];
            str[i] = $"{c.count}x {c.entry}";
        }
        return string.Join(", ", str);
    }

    private static string GetOpLegacyMapEffectString(byte[] data)
    {
        var len = data.Length;
        var str = new string[len];
        for (var i = 0; i < len; ++i)
        {
            str[i] = data[i].ToString("X2");
        }
        return string.Join(" ", str);
    }

    private Action<UITree>? OpChildren(WorldState.Operation o) => o switch
    {
        ActorState.OpCastEvent op => op.Value.Targets.Count != 0 ? tree => DrawEventCast(tree, op) : null,
        ActorState.OpPlayActionTimelineSync op => tree => DrawActionTimelineSync(tree, op),
        _ => null
    };

    private void DrawEventCast(UITree tree, ActorState.OpCastEvent op)
    {
        var action = replay.Actions.Find(a => a.GlobalSequence == op.Value.GlobalSequence);
        if (action != null && action.Timestamp == op.Timestamp && action.Source.InstanceID == op.InstanceID)
        {
            foreach (var t in tree.Nodes(action.Targets, t => new(ReplayUtils.ActionTargetString(t, op.Timestamp))))
            {
                tree.LeafNodes(t.Effects.ValidEffects(), ReplayUtils.ActionEffectString);
            }
        }
        else
        {
            foreach (var t in tree.Nodes(op.Value.Targets, t => new(ActorString(t.ID, op.Timestamp))))
            {
                tree.LeafNodes(t.Effects.ValidEffects(), ReplayUtils.ActionEffectString);
            }
        }
    }

    private void DrawActionTimelineSync(UITree tree, ActorState.OpPlayActionTimelineSync op) => tree.LeafNodes(op.Actions, iii => $"{ActorString(iii.Item1, op.Timestamp)}: {iii.Item2:X4}");

    private Action? OpContextMenu(WorldState.Operation o)
    {
        Action? opSpecific = o switch
        {
            WorldState.OpDirectorUpdate op => () => ContextMenuDirectorUpdate(op),
            ActorState.OpStatus op => () => ContextMenuActorStatus(op),
            ActorState.OpCastInfo op => () => ContextMenuActorCast(op),
            ActorState.OpCastEvent op => () => ContextMenuEventCast(op),
            ActorState.Operation op => () => ContextMenuActor(op),
            _ => null,
        };

        return () =>
        {
            if (opSpecific != null)
            {
                opSpecific.Invoke();
                ImGui.Separator();
            }

            if (ImGui.MenuItem("Jump to timestamp", "double click"))
                scrollTo(o.Timestamp);
        };
    }

    private void ContextMenuDirectorUpdate(WorldState.OpDirectorUpdate op)
    {
        if (ImGui.MenuItem($"Filter out type {op.UpdateID:X8}"))
        {
            _filteredDirectorUpdateTypes.Add(op.UpdateID);
            _nodesUpToDate = false;
        }
    }

    private void ContextMenuActor(ActorState.Operation op)
    {
        var oid = replay.FindParticipant(op.InstanceID, op.Timestamp)!.OID;
        if (ImGui.MenuItem($"Filter out OID {oid:X}"))
        {
            _filteredOIDs.Add(oid);
            _nodesUpToDate = false;
        }
    }

    private void ContextMenuActorStatus(ActorState.OpStatus op)
    {
        ContextMenuActor(op);
        foreach (var s in FindStatuses(op.InstanceID, op.Index, op.Timestamp))
        {
            if (ImGui.MenuItem($"Filter out {Utils.StatusString(s.ID)}"))
            {
                _filteredStatuses.Add(s.ID);
                _nodesUpToDate = false;
            }
        }
    }

    private void ContextMenuActorCast(ActorState.OpCastInfo op)
    {
        ContextMenuActor(op);
        var cast = FindCast(replay.FindParticipant(op.InstanceID, op.Timestamp), op.Timestamp, op.Value != null);
        if (cast != null && ImGui.MenuItem($"Filter out {cast.ID}"))
        {
            _filteredActions.Add(cast.ID);
            _nodesUpToDate = false;
        }
    }

    private void ContextMenuEventCast(ActorState.OpCastEvent op)
    {
        ContextMenuActor(op);
        if (ImGui.MenuItem($"Filter out {op.Value.Action}"))
        {
            _filteredActions.Add(op.Value.Action);
            _nodesUpToDate = false;
        }
    }

    private IEnumerable<Replay.Status> FindStatuses(ulong instanceID, int index, DateTime timestamp)
    {
        var statuses = replay.Statuses;
        for (var i = 0; i < statuses.Count; ++i)
        {
            var s = statuses[i];
            if (s.Target.InstanceID == instanceID && s.Index == index && (s.Time.Start == timestamp || s.Time.End == timestamp))
            {
                yield return s;
            }
        }
    }
    private Replay.Cast? FindCast(Replay.Participant? participant, DateTime timestamp, bool start) => participant?.Casts.Find(c => (start ? c.Time.Start : c.Time.End) == timestamp);

    private string ActorString(Replay.Participant? p, DateTime timestamp)
        => p != null ? $"{ReplayUtils.ParticipantString(p, timestamp)} ({ModuleInfo?.ObjectIDType?.GetEnumName(p.OID)}) {Utils.PosRotString(p.PosRotAt(timestamp))}" : "<none>";

    private string ActorString(ulong instanceID, DateTime timestamp)
    {
        var p = replay.FindParticipant(instanceID, timestamp);
        return p != null || instanceID == default ? ActorString(p, timestamp) : $"<unknown> {instanceID:X}";
    }

    private string CastEventTargetString(ActorCastEvent ev, DateTime timestamp) => $"{ActorString(ev.MainTargetID, timestamp)} / {Utils.Vec3String(ev.TargetPos)} / {ev.Rotation}";

    private string CastString(ulong instanceID, DateTime timestamp, bool start)
    {
        var p = replay.FindParticipant(instanceID, timestamp);
        var c = FindCast(p, timestamp, start);
        if (c == null)
        {
            return $"{ActorString(p, timestamp)}: <unknown cast>";
        }

        return $"{ActorString(p, timestamp)}: {c.ID} ({ModuleInfo?.ActionIDType?.GetEnumName(c.ID.ID)}), {c.ExpectedCastTime:f2}s ({c.Time} actual){(c.Interruptible ? " (interruptible)" : "")} @ {ReplayUtils.ParticipantPosRotString(c.Target, timestamp)} / {Utils.Vec3String(c.Location)} / {c.Rotation}";
    }

    private string StatusesString(ulong instanceID, int index, DateTime timestamp)
    {
        string Classify(Replay.Status s)
        {
            var parts = new List<string>(2);
            if (s.Time.Start == timestamp)
            {
                parts.Add("gain");
            }

            if (s.Time.End == timestamp)
            {
                parts.Add("lose");
            }

            return string.Join("/", parts);
        }
        var sb = new StringBuilder();
        var first = true;
        foreach (var s in FindStatuses(instanceID, index, timestamp))
        {
            if (!first)
            {
                sb.Append("; ");
            }

            first = false;
            sb.Append($"{Classify(s)} {Utils.StatusString(s.ID)} ({ModuleInfo?.StatusIDType?.GetEnumName(s.ID)}) ({s.StartingExtra:X}), {s.InitialDuration:f2}s / {s.Time}, from {ActorString(s.Source, timestamp)}");
        }
        return sb.ToString();
    }

    private string ItemString(ClientState.OpInventoryChange op)
    {
        if (op.ItemId > 2000000)
        {
            return $"Item quantity: {op.ItemId} '{Service.LuminaRow<Lumina.Excel.Sheets.EventItem>(op.ItemId)?.Name}' x{op.Quantity}";
        }

        return $"Item quantity: {op.ItemId % 500000} '{Service.LuminaRow<Lumina.Excel.Sheets.Item>(op.ItemId % 500000)?.Name}' (hq={op.ItemId > 1000000}) x{op.Quantity}";
    }
}
