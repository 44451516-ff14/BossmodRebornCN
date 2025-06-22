namespace BossMod.Stormblood.Foray.Hydatos.Ceto;

public enum OID : uint
{
    Boss = 0x2765, // R5.0
    FaithlessGuard = 0x2767, // R2.0
    LifelessSlave1 = 0x2766, // R2.7
    LifelessSlave2 = 0x2785, // R2.7
    LifelessSlave3 = 0x2784, // R2.7
    HydatosDelphyne = 0x26C5, // R1.2
    DarkGargoyle = 0x26C6, // R2.3-4.14
    Helper = 0x2783
}

public enum AID : uint
{
    AutoAttack1 = 15496, // Boss->player, no cast, single-target
    AutoAttack2 = 15234, // HydatosDelphyne->player, no cast, single-target
    AutoAttack3 = 15235, // DarkGargoyle->player, no cast, single-target

    BuffSlaves = 16171, // Boss->self, no cast, range 50 circle, applies Magic Infusion buff to slaves
    SickleStrike = 15466, // Boss->player, 3.5s cast, single-target
    PetrifactionBoss = 15469, // Boss->self, 450 circle
    AbyssalReaper = 15468, // Boss->self, 4.0s cast, range 18 circle
    PetrifactionAdds = 15475, // LifelessSlave1/LifelessSlave2/LifelessSlave3->self, 4.0s cast, range 50 circle
    CircleOfFlames = 15472, // FaithlessGuard->location, 3.0s cast, range 5 circle
    TailSlap = 15471, // FaithlessGuard->self, 3.0s cast, range 12 120-degree cone
    PetrattractionVisual = 15473, // FaithlessGuard->Helper, 3.0s cast, single-target
    Petrattraction = 15476, // Helper->self, no cast, range 50 circle, pull 50, between hitboxes
    GrimFate = 15451, // DarkGargoyle->self, 3.0s cast, range 8 120-degree cone
    Desolation = 15453, // DarkGargoyle->self, 3.0s cast, range 12 width 8 rect

    CircleBlade1 = 15470, // FaithlessGuard->self, 3.0s cast, range 7 circle
    CircleBlade2 = 15448, // HydatosDelphyne->self, 3.0s cast, range 7 circle
    FireToss = 15449, // HydatosDelphyne->players, 3.0s cast, range 5 circle
}

class SickleStrike(BossModule module) : Components.SingleTargetCast(module, (uint)AID.SickleStrike);
class PetrifactionBoss(BossModule module) : Components.CastGaze(module, (uint)AID.PetrifactionBoss, range: 50f);
class PetrifactionAdds(BossModule module) : Components.CastGaze(module, (uint)AID.PetrifactionAdds, range: 50f);
class AbyssalReaper(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbyssalReaper, 18f);
class CircleOfFlames(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CircleOfFlames, 5f);
class TailSlap(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TailSlap, new AOEShapeCone(12f, 60f.Degrees()));
class GrimFate(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrimFate, new AOEShapeCone(8f, 60f.Degrees()));
class Desolation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Desolation, new AOEShapeRect(12f, 4f));

class Petrattraction(BossModule module) : Components.GenericKnockback(module)
{
    public Knockback? _source;

    private static readonly AOEShapeCircle circle = new(50f);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => Utils.ZeroOrOne(ref _source);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.PetrattractionVisual)
            _source = new(caster.Position, 50f, Module.CastFinishAt(spell, 1.4f), circle, Kind: Kind.TowardsOrigin);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Petrattraction)
            _source = null;
    }
}

class CircleBlade1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CircleBlade1, 7);
class CircleBlade2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CircleBlade2, 7);
class FireToss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FireToss, 5f);

class CetoStates : StateMachineBuilder
{
    public CetoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SickleStrike>()
            .ActivateOnEnter<PetrifactionBoss>()
            .ActivateOnEnter<PetrifactionAdds>()
            .ActivateOnEnter<AbyssalReaper>()
            .ActivateOnEnter<CircleOfFlames>()
            .ActivateOnEnter<TailSlap>()
            .ActivateOnEnter<Petrattraction>()
            .ActivateOnEnter<CircleBlade1>()
            .ActivateOnEnter<CircleBlade2>()
            .ActivateOnEnter<GrimFate>()
            .ActivateOnEnter<Desolation>()
            .ActivateOnEnter<FireToss>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "xan, Malediktus", GroupType = BossModuleInfo.GroupType.EurekaNM, GroupID = 639, NameID = 1421, SortOrder = 9)]
public class Ceto(WorldState ws, Actor primary) : SimpleBossModule(ws, primary)
{
    private static readonly uint[] trash = [(uint)OID.FaithlessGuard, (uint)OID.HydatosDelphyne, (uint)OID.DarkGargoyle];
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(trash));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Boss => 1,
                (uint)OID.FaithlessGuard => AIHints.Enemy.PriorityPointless,
                _ when e.Actor.InCombat => 0,
                _ => AIHints.Enemy.PriorityUndesirable
            };
        }
    }
}
