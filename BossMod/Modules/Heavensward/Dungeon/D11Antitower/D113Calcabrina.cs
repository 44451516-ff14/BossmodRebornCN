namespace BossMod.Heavensward.Dungeon.D11Antitower.D113Calcabrina;

public enum OID : uint
{
    Boss = 0x1503, // R0.9
    Brina = 0x1504, // R0.9
    BrinaPlayer1 = 0x15E3, // R0.9
    BrinaPlayer2 = 0x15E1, // R0.9
    CalcaPlayer1 = 0x15E2, // R0.9
    CalcaPlayer2 = 0x15E0, // R0.9
    Calcabrina = 0x1502, // R2.25
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Calcabrina->player, no cast, single-target
    Teleport1 = 5554, // Brina/Boss->location, no cast, ???
    Teleport2 = 5553, // Boss/Brina/CalcaPlayer2/CalcaPlayer1/BrinaPlayer->location, no cast, ???

    HeatGazeBrina = 5552, // Brina/BrinaPlayer1/BrinaPlayer2->self, 3.0s cast, range 5-10 donut
    HeatGazeCalca = 5551, // Boss/CalcaPlayer1/CalcaPlayer2->self, 3.0s cast, range 19+R 60-degree cone

    CalcabrinaSpawn = 5555, // Calcabrina->self, no cast, single-target

    Knockout = 5556, // Calcabrina->player, 4.0s cast, single-target, tankbuster

    TerrifyingGlance = 5559, // Calcabrina->self, no cast, range 40+R 120-degree cone

    Brace = 5557, // Calcabrina->self, 3.0s cast, single-target, parry sides+back
    Breach = 5558, // Helper->player, no cast, single-target, knockback 5, on hitting parry sides

    Dollhouse = 5561, // Calcabrina->self, 3.0s cast, ???
    DollActivates = 5562, // CalcaPlayer2/CalcaPlayer1/BrinaPlayer->self, no cast, single-target
    DollMorph = 5810, // Helper->player, no cast, single-target
    DollUnmorph = 5563, // CalcaPlayer2/CalcaPlayer1/BrinaPlayer->self, no cast, single-target, player turns back to normal
    Slapstick = 5560 // Calcabrina->self, no cast, range 40 circle, raidwides until players unmorphed
}

public enum SID : uint
{
    Fetters = 1055 // CalcaPlayer1/CalcaPlayer/BrinaPlayer1/BrinaPlayer2->player, extra=0x0
}

public enum IconID : uint
{
    Gaze = 73 // player
}

class TerrifyingGlanceBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(42.25f, 61.Degrees()), (uint)IconID.Gaze, ActionID.MakeSpell(AID.TerrifyingGlance), 3.1f, source: module.Enemies(OID.Calcabrina)[0]);
class TerrifyingGlanceGaze(BossModule module) : Components.GenericGaze(module)
{
    private DateTime activation;
    private readonly TerrifyingGlanceBait _bait = module.FindComponent<TerrifyingGlanceBait>()!;

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor)
    {
        if (activation == default)
            yield break;
        var primary = Module.Enemies(OID.Calcabrina)[0].Position;
        var dir = _bait.CurrentBaits[0].Target.Position - primary;
        if (actor.Position.InCone(primary, dir, 61.Degrees())) // 1° extra safety margin since calculated and actual rotation seems to be slightly off
        {
            yield return new(primary, activation);
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.Gaze)
            activation = WorldState.FutureTime(3.1f);

    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TerrifyingGlance)
            activation = default;
    }
}

class Brace(BossModule module) : Components.DirectionalParry(module, [(uint)OID.Calcabrina])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Brace)
            PredictParrySide(caster.InstanceID, Side.Back | Side.Right | Side.Left);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActorStates.Count > 0)
        {
            var primary = Module.Enemies(OID.Calcabrina)[0];
            hints.AddForbiddenZone(ShapeDistance.InvertedDonutSector(primary.Position, primary.HitboxRadius, 20, primary.Rotation, 45.Degrees()));
        }
    }
}

class HeatGazeBrina(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HeatGazeBrina), new AOEShapeDonut(5, 10));
class HeatGazeCalca(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HeatGazeCalca), new AOEShapeCone(19.9f, 30.Degrees()));
class Knockout(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Knockout));

class Slapstick(BossModule module) : BossComponent(module)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.FindStatus(SID.Fetters) != null)
            return;
        if (Raid.WithoutSlot().Any(x => x.FindStatus(SID.Fetters) != null))
            hints.Add("Kill the small dolls to free the players!");
    }
}

class D113CalcabrinaStates : StateMachineBuilder
{
    public D113CalcabrinaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TerrifyingGlanceBait>()
            .ActivateOnEnter<TerrifyingGlanceGaze>()
            .ActivateOnEnter<Brace>()
            .ActivateOnEnter<HeatGazeBrina>()
            .ActivateOnEnter<HeatGazeCalca>()
            .ActivateOnEnter<Slapstick>()
            .ActivateOnEnter<Knockout>()
            .Raw.Update = () => module.Enemies(OID.Calcabrina).Concat(module.Enemies(OID.Boss)).Concat(module.Enemies(OID.Brina)).All(e => e.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 141, NameID = 4813)]
public class D113Calcabrina(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(232, -182), 19.5f / MathF.Cos(MathF.PI / 36), 36)], [new Rectangle(new(252, -182), 20, 1.15f, 90.Degrees())]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.Brina).Concat(Enemies(OID.Boss)).Concat(Enemies(OID.Calcabrina)));
        Arena.Actors(Enemies(OID.CalcaPlayer1).Concat(Enemies(OID.CalcaPlayer2)).Concat(Enemies(OID.BrinaPlayer1)).Concat(Enemies(OID.BrinaPlayer2)), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.BrinaPlayer1 or OID.BrinaPlayer2 or OID.CalcaPlayer1 or OID.CalcaPlayer2 => 1,
                _ => 0
            };
        }
    }
}
