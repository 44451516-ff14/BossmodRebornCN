namespace BossMod.Endwalker.Ultimate.TOP;

sealed class P3OversampledWaveCannon(BossModule module) : BossComponent(module)
{
    private Actor? _boss;
    private Angle _bossAngle;
    private readonly Angle[] _playerAngles = new Angle[PartyState.MaxPartySize];
    private readonly int[] _playerOrder = new int[PartyState.MaxPartySize];
    private int _numPlayerAngles;
    private readonly List<int> _monitorOrder = [];
    private readonly TOPConfig _config = Service.Config.Get<TOPConfig>();

    private static readonly AOEShapeRect _shape = new(50, 50);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_playerOrder[slot] != 0)
            hints.Add($"Order: {(IsMonitor(slot) != default ? "M" : "N")}{_playerOrder[slot]}", false);

        var numHitBy = AOEs(slot).Count(a => !a.source && _shape.Check(actor.Position, a.origin, a.rot));
        if (numHitBy != 1)
            hints.Add($"Hit by {numHitBy} monitors!");
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        foreach (var p in SafeSpots(slot).Where(p => p.assigned))
            movementHints.Add(actor.Position, p.pos, Colors.Safe);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var a in AOEs(pcSlot))
            _shape.Draw(Arena, a.origin, a.rot, a.safe ? Colors.SafeFromAOE : default);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var p in SafeSpots(pcSlot))
            Arena.AddCircle(p.pos, 1f, p.assigned ? Colors.Safe : default);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var angle = status.ID switch
        {
            (uint)SID.OversampledWaveCannonLoadingL => 90f.Degrees(),
            (uint)SID.OversampledWaveCannonLoadingR => -90f.Degrees(),
            _ => default
        };
        if (angle != default && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            _playerAngles[slot] = angle;
            if (++_numPlayerAngles == 3)
            {
                int n = 0, m = 0;
                foreach (var sg in Service.Config.Get<TOPConfig>().P3MonitorsAssignments.Resolve(Raid).OrderBy(sg => sg.group))
                {
                    _playerOrder[sg.slot] = IsMonitor(sg.slot) ? ++m : ++n;
                    if (IsMonitor(sg.slot))
                        _monitorOrder.Add(sg.slot);
                }
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var angle = spell.Action.ID switch
        {
            (uint)AID.OversampledWaveCannonL => 90f.Degrees(),
            (uint)AID.OversampledWaveCannonR => -90f.Degrees(),
            _ => default
        };
        if (angle != default)
        {
            _boss = caster;
            _bossAngle = angle;
        }
    }

    private bool IsMonitor(int slot) => _playerAngles[slot] != default;

    private List<(WPos pos, bool assigned)> SafeSpots(int slot)
    {
        if (_numPlayerAngles < 3 || _bossAngle == default)
            return [];

        WPos adjust(float x, float z) => Arena.Center + new WDir(_bossAngle.Rad < 0 ? -x : x, z);
        var safespots = new List<(WPos, bool)>(5);
        if (IsMonitor(slot))
        {
            var nextSlot = 0;
            if (!_config.P3LastMonitorSouth)
                safespots.Add((adjust(10f, -11f), _playerOrder[slot] == ++nextSlot));
            safespots.Add((adjust(-11f, -9f), _playerOrder[slot] == ++nextSlot));
            safespots.Add((adjust(-11f, +9f), _playerOrder[slot] == ++nextSlot));
            if (_config.P3LastMonitorSouth)
                safespots.Add((adjust(10f, 11f), _playerOrder[slot] == ++nextSlot));
        }
        else
        {
            var nextSlot = 0;
            safespots.Add((adjust(1f, -15f), _playerOrder[slot] == ++nextSlot));
            if (_config.P3LastMonitorSouth)
                safespots.Add((adjust(10f, -11f), _playerOrder[slot] == ++nextSlot));
            safespots.Add((adjust(15f, -4f), _playerOrder[slot] == ++nextSlot));
            safespots.Add((adjust(15f, +4f), _playerOrder[slot] == ++nextSlot));
            if (!_config.P3LastMonitorSouth)
                safespots.Add((adjust(10f, 11f), _playerOrder[slot] == ++nextSlot));
            safespots.Add((adjust(1f, 15f), _playerOrder[slot] == ++nextSlot));
        }
        return safespots;
    }

    private (WPos origin, Angle rot, bool safe, bool source)[] AOEs(int slot)
    {
        var isMonitor = IsMonitor(slot);
        var order = (isMonitor, _playerOrder[slot]) switch
        {
            (_, 1) => 2, // N1/M1 are hit by M2
            (true, _) => 0, // M2/M3 are hit by boss
            (_, 2 or 3) => 1, // N2/N3 are hit by M1
            _ => 3, // N4/N5 are hit by M3
        };
        var aoes = AOEs();
        var len = aoes.Length;
        var aoesNew = new (WPos, Angle, bool, bool)[len];
        var index = 0;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.origin != null)
            {
                aoesNew[index++] = (aoe.origin.Position, aoe.origin.Rotation + aoe.offset, aoe.order == order, isMonitor && aoe.order == _playerOrder[slot]);
            }
        }
        return aoesNew[..index];
    }

    private (Actor? origin, Angle offset, int order)[] AOEs()
    {
        var count = _monitorOrder.Count;
        var aoes = new (Actor?, Angle, int)[count + 1];
        aoes[0] = (_boss, _bossAngle, 0);
        for (var i = 0; i < _monitorOrder.Count; ++i)
        {
            var slot = _monitorOrder[i];
            aoes[i + 1] = (Raid[slot], _playerAngles[slot], i + 1);
        }
        return aoes;
    }
}

sealed class P3OversampledWaveCannonSpread(BossModule module) : Components.UniformStackSpread(module, default, 7f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.OversampledWaveCannonR or (uint)AID.OversampledWaveCannonL)
            AddSpreads(Raid.WithoutSlot(true, true, true), Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.OversampledWaveCannonAOE)
            Spreads.Clear();
    }
}
