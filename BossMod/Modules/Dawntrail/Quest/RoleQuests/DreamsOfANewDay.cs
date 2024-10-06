namespace BossMod.Dawntrail.Quest.RoleQuests.DreamsOfANewDay;

public enum OID : uint
{
    Boss = 0x4481, // R1.5
    BossP2 = 0x44A1, // R1.5
    TentoawaTheWideEye = 0x447D, // R0.5
    UnboundRavager1 = 0x4484, // R0.5
    UnboundRavager2 = 0x4572, // R0.5
    UnboundRavager3 = 0x4585, // R0.5
    UnboundRavager4 = 0x4587, // R0.5
    UnboundRavager5 = 0x4586, // R0.5
    UnboundRavager6 = 0x4485, // R0.5
    UnboundRavager7 = 0x456F, // R0.5
    UnboundRavager8 = 0x456E, // R0.5
    UnboundRaider1 = 0x4482, // R0.5
    UnboundRaider2 = 0x4483, // R0.5
    UnboundRaider3 = 0x456D, // R0.5
    UnboundRaider4 = 0x456C, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss/TentoawaTheWideEye->player/Boss/TentoawaTheWideEye, no cast, single-target
    AutoAttack2 = 39796, // UnboundRavager4->4583, no cast, single-target
    AutoAttack3 = 873, // UnboundRavager5->4583, no cast, single-target
    Teleport = 39054, // Boss->location, no cast, single-target
    ZunduWarriorVisual = 39033, // Helper->self, no cast, single-target

    BladestormVisual = 39043, // Boss->self, 3.0+1,0s cast, single-target
    Bladestorm = 39044, // Helper->self, 4.0s cast, range 20 90-degree cone
    KeenTempestVisual = 39034, // Boss->self, 5.0s cast, single-target
    KeenTempest = 39035, // Helper->self, 6.0s cast, range 8 circle
    AethericBurstVisual = 39047, // Boss->self, 5.0s cast, single-target
    AethericBurst = 39049, // Helper->self, no cast, range 100 circle
    ConvictionVisual = 39036, // Boss->self, 10.0s cast, single-target, towers
    Conviction = 39037, // Helper->self, 10.0s cast, range 4 circle
    ConvictionFail = 39038, // Helper->self, no cast, range 100 circle

    ActivateShield = 38329, // TentoawaTheWideEye->self, no cast, single-target
    CradleOfTheSleepless = 39050, // BossP2->self, 15.0s cast, range 100 circle
    DaggerflightVisual = 39052, // BossP2->self, 8.0s cast, single-target
    Daggerflight = 39053, // BossP2->player/TentoawaTheWideEye, no cast, single-target
    AetherialRayVisual = 39039, // BossP2->self, 6.0+1,0s cast, single-target
    AetherialRay = 39040, // Helper->self, 7.0s cast, range 40 width 4 rect
    Aethershot = 39042, // Helper->player/TentoawaTheWideEye/LoazenikweTheShutEye, 5.0s cast, range 5 circle, spread
    BloodyTrinity = 39032, // BossP2->players, 5.0s cast, single-target, tankbuster
    AetherialExposure = 39041, // Helper->LoazenikweTheShutEye, 8.0s cast, range 6 circle, stack
    PoisonDaggersVisual = 39045, // BossP2->self, 8.0s cast, single-target
    PoisonDaggers = 39046, // Helper->player/TentoawaTheWideEye/LoazenikweTheShutEye, no cast, single-target
}

public enum TetherID : uint
{
    Wakeup = 45, // Zundus->TentoawaTheWideEye
    Daggerflight = 84, // BossP2->TentoawaTheWideEye/player
}

class Bladestorm(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Bladestorm), new AOEShapeCone(20, 45.Degrees()));
class KeenTempest(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.KeenTempest), new AOEShapeCircle(8))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Casters.Count > 0)
            foreach (var a in Casters)
                if ((Module.CastFinishAt(a.CastInfo) - Module.CastFinishAt(Casters[0].CastInfo)).TotalSeconds <= 1)
                    yield return new(Shape, a.Position, a.CastInfo!.Rotation, Module.CastFinishAt(a.CastInfo));
    }
}

class AethericBurst(BossModule module) : Components.RaidwideCastDelay(module, ActionID.MakeSpell(AID.AethericBurstVisual), ActionID.MakeSpell(AID.AethericBurst), 0.9f);
class AetherialExposure(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.AetherialExposure), 6, 3, 3);
class Conviction(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID.Conviction), 4)
{
    private readonly AetherialExposure _stack = module.FindComponent<AetherialExposure>()!;

    public override void Update()
    {
        if (Towers.Count == 0)
            return;

        var stack = _stack.ActiveStacks.Any();
        for (var i = 0; i < Towers.Count; i++)
            Towers[i] = new(Towers[i].Position, Towers[i].Radius, stack ? 0 : 1, stack ? 0 : 1, default, Towers[i].Activation);
    }
}

class AetherialRay(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AetherialRay), new AOEShapeRect(40, 2), 10);
class Aethershot(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Aethershot), 5);
class BloodyTrinity(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.BloodyTrinity));
class PoisonDaggers(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.PoisonDaggersVisual));
class Daggerflight(BossModule module) : Components.InterceptTether(module, ActionID.MakeSpell(AID.DaggerflightVisual), (uint)TetherID.Daggerflight)
{
    private DateTime _activation;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if ((AID)spell.Action.ID == AID.DaggerflightVisual)
            _activation = Module.CastFinishAt(spell, 0.2f);
    }

    //TODO: consider moving this logic to the component
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Active)
        {
            var source = Module.PrimaryActor;
            var target = Module.Enemies(OID.TentoawaTheWideEye).FirstOrDefault()!;
            var direction = (target.Position - source.Position).Normalized();
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(target.Position - (source.HitboxRadius + 0.1f) * Angle.FromDirection(direction).ToDirection(), source.Position, 0.5f), _activation);
        }
    }
}

class CradleOfTheSleepless(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private const string RiskHint = "Go behind shield or duty fails!";
    private const string StayHint = "Wait behind shield!";
    private static readonly AOEShapeCone cone = new(8, 60.Degrees(), InvertForbiddenZone: true);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ActivateShield)
            _aoe = new(cone, caster.Position, caster.Rotation + 180.Degrees(), default, Colors.SafeFromAOE);
        else if ((AID)spell.Action.ID == AID.CradleOfTheSleepless)
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

class DreamsOfANewDayStates : StateMachineBuilder
{
    public DreamsOfANewDayStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Bladestorm>()
            .ActivateOnEnter<KeenTempest>()
            .ActivateOnEnter<AethericBurst>()
            .ActivateOnEnter<AetherialExposure>()
            .ActivateOnEnter<Conviction>()
            .ActivateOnEnter<AetherialRay>()
            .ActivateOnEnter<Aethershot>()
            .ActivateOnEnter<BloodyTrinity>()
            .ActivateOnEnter<PoisonDaggers>()
            .ActivateOnEnter<CradleOfTheSleepless>()
            .ActivateOnEnter<Daggerflight>();
    }
}

class DreamsOfANewDayP2States(BossModule module) : DreamsOfANewDayStates(module) { }

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70359, NameID = 13046, SortOrder = 1)]
public class DreamsOfANewDay(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(-757, -719), 19.5f, 20)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.BossP2).Concat([PrimaryActor]).Concat(Enemies(OID.UnboundRaider1)).Concat(Enemies(OID.UnboundRaider2))
        .Concat(Enemies(OID.UnboundRaider3)).Concat(Enemies(OID.UnboundRaider4)).Concat(Enemies(OID.UnboundRavager1)).Concat(Enemies(OID.UnboundRavager2))
        .Concat(Enemies(OID.UnboundRavager3)).Concat(Enemies(OID.UnboundRavager4)).Concat(Enemies(OID.UnboundRavager5)).Concat(Enemies(OID.UnboundRavager6)
        .Concat(Enemies(OID.UnboundRavager7))).Concat(Enemies(OID.UnboundRavager8)));
    }

    protected override bool CheckPull() => Raid.WithoutSlot().Any(x => x.InCombat);
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.BossP2, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70359, NameID = 13046, SortOrder = 2)]
public class DreamsOfANewDayP2(WorldState ws, Actor primary) : DreamsOfANewDay(ws, primary);
