namespace BossMod.Stormblood.Dungeon.D15TheGhimlytDark.D150ScholaMarkIIColossus;

public enum OID : uint
{
    Boss = 0x2528, //R=3.2
    ScholaColossusRubricatus = 0x2527, //R=3.4
    KanESenna = 0x2632, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/ScholaColossusRubricatus->player, no cast, single-target

    MagitekMissileVisual = 15041, // Boss->self, 3.0s cast, single-target
    MagitekMissile = 15042, // Helper->location, 5.0s cast, range 30 circle, damage fall off aoe
    Exhaust = 14966, // Boss->self, 2.5s cast, range 40+R width 10 rect
    ElementalBlessing = 14472, // KanESenna->self, no cast, ???
    UnbreakableCermetBlade = 14470, // ScholaColossusRubricatus->self, 9.0s cast, range 30 circle
    GrandSword = 14967, // ScholaColossusRubricatus->self, 3.0s cast, range 15+R 120-degree cone
    SelfDetonate = 14574 // ScholaColossusRubricatus->self, 35.0s cast, range 30 circle, enrage
}

class MagitekMissile(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.MagitekMissile), 15);
class Exhaust(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Exhaust), new AOEShapeRect(43.2f, 5));
class GrandSword(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GrandSword), new AOEShapeCone(18.4f, 60.Degrees()));
class SelfDetonate(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.SelfDetonate), "Enrage!", true);

class UnbreakableCermetBlade(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private const string RiskHint = "Go under shield!";
    private const string StayHint = "Wait under shield!";
    private static readonly AOEShapeCircle circle = new(4.5f, true);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ElementalBlessing)
            _aoe = new(circle, caster.Position, default, default, Colors.SafeFromAOE);
        else if ((AID)spell.Action.ID == AID.UnbreakableCermetBlade)
            _aoe = null;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var activeAOEs = ActiveAOEs(slot, actor).ToList();
        if (activeAOEs.Any(c => !c.Check(actor.Position)))
            hints.Add(RiskHint);
        else if (activeAOEs.Any(c => c.Check(actor.Position)))
            hints.Add(StayHint, false);
    }
}

class D0150ScholaMarkIIColossusStates : StateMachineBuilder
{
    public D0150ScholaMarkIIColossusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MagitekMissile>()
            .ActivateOnEnter<Exhaust>()
            .ActivateOnEnter<GrandSword>()
            .ActivateOnEnter<SelfDetonate>()
            .ActivateOnEnter<UnbreakableCermetBlade>()
            .Raw.Update = () => module.Enemies(OID.ScholaColossusRubricatus).All(e => e.IsDeadOrDestroyed) && module.PrimaryActor.IsDestroyed;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 611, NameID = 7888, SortOrder = 4)]
public class D0150ScholaMarkIIColossus(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(373.47f, -161.67f), new(373.99f, -161.47f), new(374.49f, -161.26f), new(375, -161.01f), new(375.48f, -160.74f),
    new(379.46f, -157.86f), new(379.88f, -157.58f), new(380.42f, -157.56f), new(383.86f, -154.94f), new(385, -153.85f),
    new(385.94f, -153.33f), new(386.41f, -152.99f), new(387.15f, -152.27f), new(389.86f, -151.12f), new(390.39f, -151.1f),
    new(391.76f, -150.34f), new(392.22f, -150.05f), new(392.64f, -149.73f), new(393.05f, -149.39f), new(393.49f, -149.06f),
    new(393.88f, -148.66f), new(394.22f, -148.25f), new(394.53f, -147.82f), new(394.88f, -147.39f), new(395.36f, -146.46f),
    new(395.55f, -145.96f), new(395.69f, -145.43f), new(395.79f, -144.91f), new(395.96f, -144.39f), new(396.27f, -143.96f),
    new(396.25f, -143.39f), new(396.15f, -142.83f), new(396.04f, -142.33f), new(395.98f, -141.83f), new(397.02f, -136.7f),
    new(397.4f, -135.73f), new(397.82f, -135.32f), new(398.35f, -135.12f), new(398.85f, -134.87f), new(399.35f, -134.65f),
    new(399.22f, -134.12f), new(399.06f, -133.59f), new(398.86f, -133.1f), new(398.63f, -132.59f), new(398.36f, -132.1f),
    new(398.05f, -131.63f), new(397.7f, -131.2f), new(397.3f, -130.8f), new(396.88f, -130.43f), new(396.44f, -130.08f),
    new(394.21f, -128.51f), new(393.74f, -128.24f), new(393.25f, -127.99f), new(390.81f, -126.88f), new(390.3f, -126.69f),
    new(389.78f, -126.54f), new(386.14f, -125.7f), new(385.62f, -125.79f), new(382.76f, -127.31f), new(382.21f, -127.34f),
    new(371.8f, -124.7f), new(368.01f, -124.4f), new(367.51f, -124.32f), new(366.92f, -124.19f), new(365.26f, -123.99f),
    new(364.69f, -123.94f), new(364.12f, -123.96f), new(359.64f, -124.71f), new(359.09f, -124.87f), new(358.56f, -125.05f),
    new(358.03f, -125.25f), new(357.51f, -125.49f), new(355.52f, -126.58f), new(355.05f, -126.89f), new(354.58f, -127.23f),
    new(354.13f, -127.6f), new(353.71f, -128.02f), new(353.36f, -128.47f), new(353.03f, -128.95f), new(352.73f, -129.46f),
    new(352.45f, -129.97f), new(352.2f, -130.49f), new(351.98f, -131.04f), new(351.81f, -131.58f), new(351.68f, -132.15f),
    new(351.59f, -132.72f), new(351.47f, -133.86f), new(351.8f, -147.25f), new(351.93f, -147.8f), new(352.08f, -148.33f),
    new(352.25f, -148.85f), new(352.44f, -149.37f), new(352.66f, -149.88f), new(352.9f, -150.39f), new(353.42f, -151.37f),
    new(355.28f, -154.19f), new(355.64f, -154.63f), new(356.02f, -155.06f), new(356.41f, -155.49f), new(358.53f, -157.45f),
    new(359.01f, -157.79f), new(359.53f, -158.04f), new(360.06f, -158.21f), new(361.13f, -158.48f), new(371.63f, -161.31f),
    new(373.18f, -161.7f)];
    private static readonly ArenaBoundsComplex arena = new([new PolygonCustom(vertices)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.ScholaColossusRubricatus).Concat([PrimaryActor]));
    }
}
