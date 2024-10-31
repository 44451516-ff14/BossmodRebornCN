namespace BossMod.Shadowbringers.Dungeon.D10AnamnesisAnyder.D101Unknown;

public enum OID : uint
{
    Boss = 0x2CD9, // R4.9
    Unknown = 0x2CDA, // R4.9
    SinisterBubble = 0x2CDB, // R1.5
    Clock = 0x1EAF6C, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/Unknown->player, no cast, single-target

    FetidFang1 = 19305, // Boss->player, 4.0s cast, single-target, tankbuster
    FetidFang2 = 19314, // Unknown->player, 4.0s cast, single-target
    NursedGrudge = 19309, // Boss->self, no cast, single-target
    Scrutiny = 20005, // Boss/Unknown->self, 13.0s cast, single-target
    Reflection = 19311, // Boss/Unknown->self, 1.2s cast, range 40 45-degree cone
    Explosion = 19310, // SinisterBubble->self, 14.0s cast, range 8 circle

    LuminousRay1 = 20006, // Boss->self, 5.0s cast, range 50 width 8 rect
    LuminousRay2 = 20007, // Unknown->self, 5.0s cast, range 50 width 8 rect

    Inscrutability1 = 19306, // Boss->self, 4.0s cast, range 40 circle
    Inscrutability2 = 19315, // Unknown->self, 4.0s cast, range 40 circle
    PlainWeirdness = 20043, // Boss/Unknown->self, 3.0s cast, single-target, damage up buff when partner boss dies

    EctoplasmicRayMarker1 = 19319, // Helper->player, no cast, single-target, line stack
    EctoplasmicRayVisual1 = 19322, // Unknown->self, 5.0s cast, single-target
    EctoplasmicRay1 = 19320, // Unknown->self, no cast, range 50 width 8 rect
    EctoplasmicRayMarker2 = 19312, // Helper->player, no cast, single-target
    EctoplasmicRayVisual2 = 19321, // Boss->self, 5.0s cast, single-target
    EctoplasmicRay2 = 19313, // Boss->self, no cast, range 50 width 8 rect

    Clearout1 = 19316, // Unknown->self, 3.0s cast, range 9 120-degree cone
    Clearout2 = 19307, // Boss->self, 3.0s cast, range 9 120-degree cone

    Setback1 = 19308, // Boss->self, 3.0s cast, range 9 120-degree cone
    Setback2 = 19317 // Unknown->self, 3.0s cast, range 9 120-degree cone
}

class Inscrutability1(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Inscrutability1));
class Inscrutability2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Inscrutability2));
class FetidFang1(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.FetidFang1));
class FetidFang2(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.FetidFang2));

abstract class Deg120Cone(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(9, 60.Degrees()));
class Setback1(BossModule module) : Deg120Cone(module, AID.Setback1);
class Setback2(BossModule module) : Deg120Cone(module, AID.Setback2);
class Clearout1(BossModule module) : Deg120Cone(module, AID.Clearout1);
class Clearout2(BossModule module) : Deg120Cone(module, AID.Clearout2);

class Explosion(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Explosion), new AOEShapeCircle(8));

abstract class LuminousRay(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(50, 4));
class LuminousRay1(BossModule module) : LuminousRay(module, AID.LuminousRay1);
class LuminousRay2(BossModule module) : LuminousRay(module, AID.LuminousRay2);

class Reflection(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(40, 22.5f.Degrees());
    private AOEInstance? _aoe;
    private static readonly Dictionary<uint, Angle> stateToAngle = new()
    {
        { 0x00041000, Angle.AnglesIntercardinals[3] },
        { 0x00042000, Angle.AnglesIntercardinals[2] },
        { 0x00044000, Angle.AnglesCardinals[1] }
    };

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (stateToAngle.TryGetValue(state, out var angle))
            _aoe = new(cone, actor.Position, angle, WorldState.FutureTime(14.4f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Reflection)
            _aoe = null;
    }
}

abstract class EctoplasmicRay(BossModule module, AID aid1, AID aid2) : Components.LineStack(module, ActionID.MakeSpell(aid1), ActionID.MakeSpell(aid2), 5.2f);
class EctoplasmicRay1(BossModule module) : EctoplasmicRay(module, AID.EctoplasmicRayMarker1, AID.EctoplasmicRay1);
class EctoplasmicRay2(BossModule module) : EctoplasmicRay(module, AID.EctoplasmicRayMarker2, AID.EctoplasmicRay2);

class D101UnknownStates : StateMachineBuilder
{
    public D101UnknownStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Inscrutability1>()
            .ActivateOnEnter<Inscrutability2>()
            .ActivateOnEnter<FetidFang1>()
            .ActivateOnEnter<FetidFang2>()
            .ActivateOnEnter<Setback1>()
            .ActivateOnEnter<Setback2>()
            .ActivateOnEnter<Clearout1>()
            .ActivateOnEnter<Clearout2>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<LuminousRay1>()
            .ActivateOnEnter<LuminousRay2>()
            .ActivateOnEnter<Reflection>()
            .ActivateOnEnter<EctoplasmicRay1>()
            .ActivateOnEnter<EctoplasmicRay2>()
            .Raw.Update = () => module.Enemies(OID.Unknown).Concat([module.PrimaryActor]).All(e => e.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 714, NameID = 9260)]
public class D101Unknown(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(-40, 290), 19.5f, 40)], [new Rectangle(new(-40, 310.534f), 20, 1.2f), new Rectangle(new(-40, 269.5f), 20, 2)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.Unknown).Concat([PrimaryActor]));
    }
}
