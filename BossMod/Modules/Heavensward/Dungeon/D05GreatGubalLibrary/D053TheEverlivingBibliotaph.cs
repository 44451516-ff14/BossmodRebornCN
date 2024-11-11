namespace BossMod.Heavensward.Dungeon.D05GreatGubalLibrary.D053EverlivingBibliotaph;

public enum OID : uint
{
    Boss = 0xE84, // R2.850, x?
    Tower = 0x1E99F3, // R0.5
    TowerLights = 0x1E991D, // R0.5
    Voidsphere = 0xEC5, // R1.0
    Biblioklept = 0xE85, // R1.5
    Bibliophile = 0xE87, // R0.45
    Bibliomancer = 0xE86, // R1.69
    AbyssalLance = 0xEA5, // R1.0
    Helper = 0xEA4
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    AutoAttack2 = 1700, // Biblioklept->player, no cast, single-target

    Thrub = 3527, // Boss->player, no cast, single-target
    VoidSparkBait = 3526, // Boss->player, no cast, single-target
    VoidSpark = 3780, // Voidsphere->self, 4.0s cast, range 7+R circle
    VoidCall = 3524, // Boss->self, 18.0s cast, single-target
    DeepDarkness = 3528, // Boss->self, 3.0s cast, range 12-25 donut
    MagicBurst = 3529, // Boss->self, 4.0s cast, range 15 circle
    AbyssalSwing = 3532, // Biblioklept->self, no cast, range 6+R 90-degree cone
    AbyssalChargeVisual = 3530, // Biblioklept->self, 1.0s cast, single-target
    AbyssalCharge = 3531, // AbyssalLance->self, 3.0s cast, range 40+R width 4 rect
    AbyssalTransfixion = 3533, // Biblioklept->player, 1.0s cast, single-target
    VoidBlizzard = 3537, // Bibliophile->player, 1.0s cast, single-target
    VoidBlizzardIII = 3824, // Bibliophile->player, 1.0s cast, single-target
    VoidBlizzardIIIAOE = 3535, // Bibliomancer->location, 3.0s cast, range 5 circle
    VoidThunderIII = 3534, // Bibliomancer->player, 3.0s cast, single-target
    Canker = 3536 // Bibliomancer->player, 1.0s cast, single-target
}

public enum IconID : uint
{
    VoidSpark = 23 // player
}

class VoidSparkBait(BossModule module) : Components.GenericBaitAway(module)
{
    public static readonly AOEShapeCircle Circle = new(8);

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.VoidSpark)
            CurrentBaits.Add(new(actor, actor, Circle, WorldState.FutureTime(5.1f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.VoidSparkBait)
            CurrentBaits.RemoveAll(x => x.Target == WorldState.Actors.Find(spell.MainTargetID));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (CurrentBaits.Any(x => x.Target == actor))
            hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 22.5f));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Any(x => x.Target == actor))
            hints.Add("Bait away!");
    }
}

class VoidSpark(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Voidsphere)
            _aoes.Add(new(VoidSparkBait.Circle, actor.Position, default, WorldState.FutureTime(9.8f)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.VoidSpark)
            _aoes.RemoveAt(0);
    }
}

class DeepDarkness(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DeepDarkness), new AOEShapeDonut(12, 25));
class MagicBurst(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MagicBurst), new AOEShapeCircle(15));
class VoidBlizzardIIIAOE(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.VoidBlizzardIIIAOE), 5);
class AbyssalSwing(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.AbyssalSwing), new AOEShapeCone(7.5f, 45.Degrees()), (uint)OID.Biblioklept);
class AbyssalCharge(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AbyssalCharge), new AOEShapeRect(41, 2));

class VoidCall(BossModule module) : Components.GenericTowers(module)
{
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Tower)
            Towers.Add(new(actor.Position, 3, activation: WorldState.FutureTime(17.8f)));
        else if ((OID)actor.OID is OID.Biblioklept or OID.Bibliophile or OID.Bibliomancer)
            Towers.Clear();
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00100020)
            Towers.RemoveAll(x => x.Position == actor.Position);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.VoidCall) // there seems to be a rare bug where a tower bugs, safeguard against it
            Towers.Clear();
    }

    public override void Update()
    {
        if (Towers.Count == 0)
            return;
        var towerCount = Module.Enemies(OID.Tower).Count;
        var soakers = towerCount == 2 ? 3 : towerCount == 3 ? 2 : 1;
        for (var i = 0; i < Towers.Count; ++i)
            Towers[i] = Towers[i] with { MinSoakers = soakers, MaxSoakers = soakers };
    }
}

class D053EverlivingBibliotaphStates : StateMachineBuilder
{
    public D053EverlivingBibliotaphStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VoidSparkBait>()
            .ActivateOnEnter<VoidSpark>()
            .ActivateOnEnter<DeepDarkness>()
            .ActivateOnEnter<MagicBurst>()
            .ActivateOnEnter<VoidBlizzardIIIAOE>()
            .ActivateOnEnter<AbyssalSwing>()
            .ActivateOnEnter<AbyssalCharge>()
            .ActivateOnEnter<VoidCall>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 31, NameID = 3930)]
public class D053EverlivingBibliotaph(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Circle(new(377.8f, -59.7f), 24.5f)], [new Rectangle(new(353.541f, -59.553f), 20, 1.25f, 89.977f.Degrees())]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.Bibliophile).Concat([PrimaryActor]).Concat(Enemies(OID.Biblioklept)).Concat(Enemies(OID.Bibliomancer)));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Biblioklept or OID.Bibliophile or OID.Bibliomancer => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
