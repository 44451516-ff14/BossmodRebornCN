namespace BossMod.Heavensward.Dungeon.D15Xelphatol.D151NuzalHueloc;

public enum OID : uint
{
    Boss = 0x179B, // R1.5
    FloatingTurret = 0x179E, // R1.0
    IxaliStitcher = 0x179C, // R1.08
    Airstone = 0x179D // R1.5
}

public enum AID : uint
{
    AutoAttack1 = 872, // Boss->player, no cast, single-target
    AutoAttack2 = 6605, // FloatingTurret->player, no cast, single-target
    AutoAttack3 = 870, // IxaliStitcher->player, no cast, single-target
    ShortBurst1 = 6598, // Boss->player, no cast, single-target
    ShortBurst2 = 6603, // FloatingTurret->player, 3.0s cast, single-target

    WindBlast = 6599, // Boss->self, 3.0s cast, range 60+R width 8 rect
    Lift = 6601, // Boss->self, 3.0s cast, single-target
    AirRaid = 6602, // Boss->location, no cast, range 50 circle
    HotBlast = 6604, // FloatingTurret->self, 6.0s cast, range 25 circle
    LongBurst = 6600 // Boss->player, 3.0s cast, single-target
}

public enum SID : uint
{
    Invincibility = 775 // none->Boss/FloatingTurret, extra=0x0
}

class Airstone(BossModule module) : BossComponent(module)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Module.Enemies(OID.Airstone).Any(x => !x.IsDead))
            hints.Add("Destroy the airstones to remove invincibility!");
    }
}

class WindBlast(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WindBlast), new AOEShapeRect(61.5f, 4));

class HotBlast(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(4, true);
    private AOEInstance? _aoe;
    private const string RiskHint = "Go under boss!";

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HotBlast)
            _aoe = new(circle, Module.PrimaryActor.Position, default, Module.CastFinishAt(spell), Colors.SafeFromAOE);
    }

    public override void Update()
    {
        if (_aoe != null && (WorldState.CurrentTime - _aoe.Value.Activation).TotalSeconds >= 1)
            _aoe = null;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var activeAOEs = ActiveAOEs(slot, actor).ToList();
        if (activeAOEs.Any(c => !c.Check(actor.Position)))
            hints.Add(RiskHint);
        else if (activeAOEs.Any(c => c.Check(actor.Position)))
            hints.Add(RiskHint, false);
    }
}

class D151NuzalHuelocStates : StateMachineBuilder
{
    public D151NuzalHuelocStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Airstone>()
            .ActivateOnEnter<WindBlast>()
            .ActivateOnEnter<HotBlast>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 182, NameID = 5265)]
public class D151NuzalHueloc(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(-73.36f, -91.53f), new(-67.17f, -90.22f), new(-66.55f, -89.97f), new(-64.94f, -89.05f), new(-64.45f, -88.60f),
    new(-53.36f, -75.39f), new(-53.26f, -74.73f), new(-52.97f, -69.07f), new(-54.26f, -62.76f), new(-54.54f, -62.01f),
    new(-57.75f, -56.44f), new(-62.76f, -51.91f), new(-68.83f, -49.18f), new(-75.29f, -48.72f), new(-75.94f, -48.76f),
    new(-79.79f, -49.43f), new(-90.22f, -55.45f), new(-92.52f, -57.81f), new(-95.38f, -64.42f), new(-96.08f, -70.82f),
    new(-96.01f, -71.49f), new(-94.72f, -77.66f), new(-91.42f, -83.42f), new(-86.42f, -88.01f), new(-80.32f, -90.77f),
    new(-73.80f, -91.52f)];
    private static readonly ArenaBoundsComplex arena = new([new PolygonCustom(vertices)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.IxaliStitcher).Concat([PrimaryActor]).Concat(Enemies(OID.FloatingTurret)).Concat(Enemies(OID.Airstone)));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            if (e.Actor.FindStatus(SID.Invincibility) != null)
            {
                e.Priority = -2;
                continue;
            }
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Airstone => 2,
                OID.FloatingTurret => 1,
                _ => 0
            };
        }
    }
}
