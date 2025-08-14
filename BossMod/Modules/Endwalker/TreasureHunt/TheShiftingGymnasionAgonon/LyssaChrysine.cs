namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.LyssaChrysine;

public enum OID : uint
{
    Boss = 0x3D43, //R=5.0
    IcePillar = 0x3D44, //R=2.0
    GymnasiouLyssa = 0x3D4E, //R=3.75, bonus loot adds
    GymnasiouLampas = 0x3D4D, //R=2.001, bonus loot adds
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/GymnasiouLyssa->player, no cast, single-target

    Icicall = 32307, // Boss->self, 2.5s cast, single-target, spawns ice pillars
    IcePillar = 32315, // IcePillar->self, 3.0s cast, range 6 circle
    SkullDasher = 32306, // Boss->player, 5.0s cast, single-target
    PillarPierce = 32316, // IcePillar->self, 3.0s cast, range 80 width 4 rect
    HeavySmash = 32314, // Boss->players, 5.0s cast, range 6 circle, stack
    Howl = 32296, // Boss->self, 2.5s cast, single-target, calls adds

    FrigidNeedleVisual = 32310, // Boss->self, 3.5s cast, single-target --> combo start FrigidNeedle --> CircleofIce (out-->in)
    FrigidNeedle = 32311, // Helper->self, 4.0s cast, range 10 circle
    CircleOfIceVisual = 32312, // Boss->self, 3.5s cast, single-target --> combo start CircleofIce --> FrigidNeedle (in-->out)
    CircleOfIce = 32313, // Helper->self, 4.0s cast, range 10-20 donut

    FrigidStoneVisual = 32308, // Boss->self, 2.5s cast, single-target
    FrigidStone = 32309, // Helper->location, 3.0s cast, range 5 circle

    HeavySmash2 = 32317, // GymnasiouLyssa->location, 3.0s cast, range 6 circle
    Telega = 9630 // GymnasiouLyssa/Lampas->self, no cast, single-target, bonus add disappear
}

class HeavySmash2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavySmash2, 6f);
class FrigidStone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FrigidStone, 5f);

class FrigidNeedle(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 20f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FrigidNeedleVisual)
            AddSequence(Arena.Center.Quantized(), Module.CastFinishAt(spell, 0.5f));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.FrigidNeedle => 0,
                (uint)AID.CircleOfIce => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

class CircleOfIce(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeDonut(10f, 20f), new AOEShapeCircle(10f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CircleOfIceVisual)
            AddSequence(Arena.Center.Quantized(), Module.CastFinishAt(spell, 0.5f));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.CircleOfIce => 0,
                (uint)AID.FrigidNeedle => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

class PillarPierce(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PillarPierce, new AOEShapeRect(80f, 2f));
class SkullDasher(BossModule module) : Components.SingleTargetCast(module, (uint)AID.SkullDasher);
class HeavySmash(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.HeavySmash, 6f, 8, 8);

class IcePillar(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(6f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.IcePillar)
            _aoes.Add(new(circle, actor.Position.Quantized(), default, WorldState.FutureTime(3.7d)));
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.IcePillar)
            _aoes.Clear();
    }
}

class Howl(BossModule module) : Components.CastHint(module, (uint)AID.Howl, "Calls adds");

class LyssaChrysineStates : StateMachineBuilder
{
    public LyssaChrysineStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IcePillar>()
            .ActivateOnEnter<HeavySmash>()
            .ActivateOnEnter<SkullDasher>()
            .ActivateOnEnter<Howl>()
            .ActivateOnEnter<FrigidNeedle>()
            .ActivateOnEnter<CircleOfIce>()
            .ActivateOnEnter<FrigidStone>()
            .ActivateOnEnter<HeavySmash2>()
            .ActivateOnEnter<PillarPierce>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(LyssaChrysine.All);
                var count = enemies.Count;
                for (var i = 0; i < count; ++i)
                {
                    if (!enemies[i].IsDeadOrDestroyed)
                        return false;
                }
                return true;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12024)]
public class LyssaChrysine(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.GymnasiouLampas, (uint)OID.GymnasiouLyssa];
    public static readonly uint[] All = [(uint)OID.Boss, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(bonusAdds), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.GymnasiouLampas => 2,
                (uint)OID.GymnasiouLyssa => 1,
                _ => 0
            };
        }
    }
}
