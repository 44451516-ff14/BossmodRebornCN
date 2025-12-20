namespace BossMod.Dawntrail.Dungeon.D12Mistwake.D122Amdusias;

public enum OID : uint
{
    Amdusias = 0x4A77, // R3.96
    PoisonCloud = 0x4A78, // R1.2
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 46839, // Amdusias->player, no cast, single-target
    Teleport = 45357, // Amdusias->location, no cast, single-target

    StaticCharge1 = 45333, // Amdusias->self, no cast, single-target
    StaticCharge2 = 45335, // Amdusias->self, no cast, single-target
    ThunderclapConcertoVisual1 = 45336, // Amdusias->self, 5.0+0,5s cast, single-target
    ThunderclapConcertoVisual2 = 45341, // Amdusias->self, 5.0+0,5s cast, single-target
    ThunderclapConcerto1 = 45337, // Helper->self, 5.5s cast, range 40 300-degree cone
    ThunderclapConcerto2 = 45342, // Helper->self, 5.5s cast, range 40 300-degree cone

    BioIIVisual = 45344, // Amdusias->self, 5.0s cast, single-target
    BioII = 45345, // Helper->location, 5.0s cast, range 20 circle

    GallopingThunderVisual = 45346, // Amdusias->location, 10.0s cast, single-target
    GallopingThunderTelegraph = 45348, // Helper->location, 1.5s cast, width 5 rect charge
    GallopingThunder = 45347, // Amdusias->location, no cast, width 5 rect charge
    Burst = 45349, // PoisonCloud->self, 2.5s cast, range 9 circle
    ThunderIVVisual = 45350, // Amdusias->self, 4.4+0,6s cast, single-target
    ThunderIV = 45351, // Helper->self, 5.0s cast, range 70 circle, raidwide
    ShockboltVisual = 45355, // Amdusias->self, 4.4+0,6s cast, single-target
    Shockbolt = 45356, // Helper->player, 5.0s cast, single-target, tankbuster
    Thunder = 45343, // Helper->player, 5.0s cast, range 5 circle
    ThunderIIIVisual = 45352, // Amdusias->self, 4.4+0,6s cast, single-target, stack x3
    ThunderIIIFirst = 45353, // Helper->players, 5.0s cast, range 6 circle
    ThunderIIIRepeat = 45354 // Helper->players, no cast, range 6 circle
}

public enum SID : uint
{
    Burst = 2536 // none->PoisonCloud, extra=0x21E
}

[SkipLocalsInit]
sealed class ThunderclapConcerto(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ThunderclapConcerto1, (uint)AID.ThunderclapConcerto2], new AOEShapeCone(40f, 150f.Degrees()));

[SkipLocalsInit]
sealed class ThunderIVBioII(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.ThunderIV, (uint)AID.BioII]);

[SkipLocalsInit]
sealed class Shockbolt(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Shockbolt);

[SkipLocalsInit]
sealed class ThunderIII(BossModule module) : Components.UniformStackSpread(module, 6f, default, 4, 4)
{
    private int numCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ThunderIIIFirst && WorldState.Actors.Find(spell.TargetID) is Actor t)
        {
            AddStack(t, Module.CastFinishAt(spell));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ThunderIIIRepeat)
        {
            ++numCasts;
            if (numCasts == 2)
            {
                Stacks.Clear();
                numCasts = 0;
            }
        }
    }

    public override void Update()
    {
        var count = Stacks.Count - 1;
        for (var i = count; i >= 0; --i)
        {
            if (Stacks.Ref(i).Target.IsDead)
            {
                Stacks.RemoveAt(i);
            }
        }
    }
}

[SkipLocalsInit]
sealed class GallopingThunder(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(5);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GallopingThunderTelegraph)
        {
            var count = _aoes.Count;
            var dir = spell.LocXZ - caster.Position;
            var shape = new AOEShapeRect(dir.Length(), 2.5f);
            var origin = caster.Position;
            var rotation = Angle.FromDirection(dir);
            _aoes.Add(new(shape, origin, rotation, Module.CastFinishAt(spell, 8.7d + 0.5d * count), count == 0 ? Colors.Danger : default, shapeDistance: shape.Distance(origin, rotation)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.GallopingThunder)
        {
            _aoes.RemoveAt(0);
            if (_aoes.Count > 1)
            {
                _aoes.Ref(0).Color = Colors.Danger;
            }
        }
    }
}

[SkipLocalsInit]
sealed class Burst(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(8);
    private readonly AOEShapeCircle circle = new(9f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var deadline = aoes[0].Activation.AddSeconds(1d);
        var isNotLastSet = aoes[^1].Activation > deadline;
        var color = Colors.Danger;
        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (aoe.Activation < deadline)
            {
                if (isNotLastSet)
                {
                    aoe.Color = color;
                }
            }
        }
        return aoes;
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Burst)
        {
            var origin = actor.Position.Quantized();
            _aoes.Add(new(circle, origin, default, WorldState.FutureTime(4.5d), shapeDistance: circle.Distance(origin, default)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.Burst)
        {
            _aoes.RemoveAt(0);
        }
    }
}

[SkipLocalsInit]
sealed class Thunder(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Thunder, 5f);

[SkipLocalsInit]
sealed class D122AmdusiasStates : StateMachineBuilder
{
    public D122AmdusiasStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ThunderclapConcerto>()
            .ActivateOnEnter<ThunderIII>()
            .ActivateOnEnter<ThunderIVBioII>()
            .ActivateOnEnter<Shockbolt>()
            .ActivateOnEnter<GallopingThunder>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<Thunder>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified,
StatesType = typeof(D122AmdusiasStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = typeof(SID),
TetherIDType = null,
IconIDType = null,
PrimaryActorOID = (uint)OID.Amdusias,
Contributors = "The Combat Reborn Team (Malediktus)",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.Dungeon,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1064u,
NameID = 14271u,
SortOrder = 2,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class D122Amdusias : BossModule
{
    public D122Amdusias(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D122Amdusias(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(281f, -285f), 19.5f, 64)], [new Rectangle(new(281f, -305f), 8f, 1.25f), new Rectangle(new(281f, -265f), 8f, 1.25f)]);
        return (arena.Center, arena);
    }
}
