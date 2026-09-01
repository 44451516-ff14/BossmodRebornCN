namespace BossMod.Dawntrail.Foray.FATE.WavedAway;

public enum OID : uint
{
    ArchKelpie = 0x4B1F,
    Helper = 0x233C,
    ArchKelpieHelper = 0x4B5B, // R0.500, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 47381, // ArchKelpie->player, no cast, single-target
    Teleport = 47382, // ArchKelpie->player, no cast, single-target
    WaveWhistle = 47383, // ArchKelpie->self, 5.0s cast, range 25 width 50 rect
    WaterIV = 47386, // ArchKelpie->self, 5.5s cast, range 60 circle

    BloodyPuddleCast = 47384, // ArchKelpie->self, 3.0+1.0s cast, single-target
    BloodyPuddle = 47385, // 4B5B->location, 3.0s cast, range 8 circle

    StormWaveStart = 47387, // 4B5B->location, 5.0s cast, range 50 width 10 rect
    StormWaveNext = 47388, // 4B5B->location, no cast, range 50 width 5 rect
}

[SkipLocalsInit]
sealed class WaveWhistle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WaveWhistle, new AOEShapeRect(25f, 25f));
[SkipLocalsInit]
sealed class WaterIV(BossModule module) : Components.RaidwideCast(module, (uint)AID.WaterIV);

[SkipLocalsInit]
sealed class BloodyPuddle : Components.SimpleAOEs
{
    public BloodyPuddle(BossModule module) : base(module, (uint)AID.BloodyPuddle, 8f)
    {
        Color = Colors.Danger;
    }
}

[SkipLocalsInit]
sealed class StormWave(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _cardinal = [with(9)];
    private readonly List<AOEInstance> _intercardinal = [with(9)];
    private readonly AOEInstance[] _active = new AOEInstance[8];
    private readonly AOEShapeRect _rect1 = new(50f, 5f), _rect2 = new(50f, 2.5f);
    private int aoeAmount;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _active.AsSpan()[..aoeAmount];
    }

    private void UpdateActiveAOEs()
    {
        aoeAmount = CopyActive(_cardinal, _active);
        aoeAmount += CopyActive(_intercardinal, _active.AsSpan()[aoeAmount..]);
    }

    private static int CopyActive(List<AOEInstance> source, Span<AOEInstance> dest)
    {
        var count = source.Count;
        if (count == 0)
        {
            return 0;
        }

        var max = count == 9 ? 3 : count > 3 ? 4 : count;
        CollectionsMarshal.AsSpan(source)[..max].CopyTo(dest);
        return max;
    }

    private static void UpdateAOEs(List<AOEInstance> aoes)
    {
        var count = aoes.Count;
        var max = count == 9 ? 3 : count > 3 ? 4 : count;
        var active = CollectionsMarshal.AsSpan(aoes)[..max];

        var isFourAOEs = max == 4;
        var isThreeAOEs = max == 3;

        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref active[i];

            var shouldBeDanger = isFourAOEs && i < 2 || isThreeAOEs && i == 0;
            var shouldBeRisky = shouldBeDanger || max == 2 && i < 2;

            if (shouldBeDanger)
            {
                aoe.Color = Colors.Danger;
            }

            if (shouldBeRisky)
            {
                aoe.Risky = true;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.StormWaveStart)
        {
            return;
        }

        var rot = spell.Rotation;
        var aoes = IsCardinal(rot) ? _cardinal : _intercardinal;

        var pos = caster.Position;
        var activation = Module.CastFinishAt(spell);

        AddAOE(_rect1, activation, spell.LocXZ, rot);

        var a180 = 180f.Degrees();
        var dir1 = (rot + a180).Round(1f).ToDirection();
        var dir2 = rot.Round(1f).ToDirection();
        var dirOrtho = (rot + a180 + 90f.Degrees()).Round(1f).ToDirection();

        for (var i = 0; i < 4; ++i)
        {
            var act = activation.AddSeconds(2d + 2d * i);
            var dirOrthoAdj = (7.5f + 5f * i) * dirOrtho;

            AddAOE(_rect2, act, (pos - 25f * dir1 + dirOrthoAdj).Quantized(), rot + a180);
            AddAOE(_rect2, act, (pos - 25f * dir2 - dirOrthoAdj).Quantized(), rot);
        }

        UpdateAOEs(aoes);
        UpdateActiveAOEs();
        void AddAOE(AOEShapeRect shape, DateTime act, WPos position, Angle rotation)
            => aoes.Add(new(shape, position, rotation, act, risky: false));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is not ((uint)AID.StormWaveStart or (uint)AID.StormWaveNext))
        {
            return;
        }

        var aoes = IsCardinal(spell.Rotation) ? _cardinal : _intercardinal;
        var count = aoes.Count;

        if (count == 0)
        {
            return;
        }

        aoes.RemoveAt(0);

        if (count >= 4)
        {
            UpdateAOEs(aoes);
        }
        UpdateActiveAOEs();
    }

    private static bool IsCardinal(Angle rotation)
    {
        for (var i = 0; i < 4; ++i)
        {
            if (Angle.AnglesCardinals[i].AlmostEqual(rotation, Angle.DegToRad))
            {
                return true;
            }
        }

        return false;
    }
}

[SkipLocalsInit]
sealed class WavedAwayStates : StateMachineBuilder
{
    public WavedAwayStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WaveWhistle>()
            .ActivateOnEnter<WaterIV>()
            .ActivateOnEnter<BloodyPuddle>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(WavedAwayStates),
    ConfigType = null, // replace null with typeof(ArchKelpieConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.ArchKelpie,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.ForayFATE,
    GroupID = 1093u,
    NameID = 2077u,
    SortOrder = 6,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class WavedAway : OpenWorldFate
{
    public WavedAway(WorldState ws, Actor primary) : base(ws, primary)
    {
        ActivateComponent<StormWave>();
    }
}
