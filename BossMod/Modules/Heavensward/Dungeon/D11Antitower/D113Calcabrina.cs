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

class TerrifyingGlanceBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(42.25f, 61f.Degrees()), (uint)IconID.Gaze, (uint)AID.TerrifyingGlance, 3.1f, source: module.Enemies((uint)OID.Calcabrina)[0]);
class TerrifyingGlanceGaze(BossModule module) : Components.GenericGaze(module)
{
    private DateTime activation;
    private readonly TerrifyingGlanceBait _bait = module.FindComponent<TerrifyingGlanceBait>()!;
    private static readonly Angle a61 = 61f.Degrees();

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        if (activation == default)
            return [];
        var primary = Module.Enemies((uint)OID.Calcabrina)[0].Position;
        var dir = _bait.CurrentBaits[0].Target.Position - primary;
        if (actor.Position.InCone(primary, dir, a61)) // 1° extra safety margin since calculated and actual rotation seems to be slightly off
        {
            return new Eye[1] { new(primary, activation) };
        }
        return [];
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Gaze)
            activation = WorldState.FutureTime(3.1d);

    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.TerrifyingGlance)
            activation = default;
    }
}

class Brace(BossModule module) : Components.DirectionalParry(module, [(uint)OID.Calcabrina])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Brace)
            PredictParrySide(caster.InstanceID, Side.Back | Side.Right | Side.Left);
    }
}

class HeatGazeBrina(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeatGazeBrina, new AOEShapeDonut(5f, 10f));
class HeatGazeCalca(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeatGazeCalca, new AOEShapeCone(19.9f, 30f.Degrees()));
class Knockout(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Knockout);

class Slapstick(BossModule module) : BossComponent(module)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var party = Raid.WithoutSlot(false, true, true);
        var len = party.Length;
        for (var i = 0; i < len; ++i)
        {
            if (party[i].FindStatus((uint)SID.Fetters) != null)
            {
                hints.Add("Kill the small dolls to free the players!");
                return;
            }
        }
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
            .Raw.Update = () => AllDeadOrDestroyed(D113Calcabrina.NpcDolls);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 141, NameID = 4813)]
public class D113Calcabrina(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(232f, -182f), 19.5f * CosPI.Pi36th, 36)], [new Rectangle(new(252f, -182f), 1.15f, 20f)]);
    private static readonly uint[] playerDolls = [(uint)OID.CalcaPlayer1, (uint)OID.CalcaPlayer2, (uint)OID.BrinaPlayer1, (uint)OID.BrinaPlayer2];
    public static readonly uint[] NpcDolls = [(uint)OID.Boss, (uint)OID.Brina, (uint)OID.Calcabrina];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, NpcDolls);
        Arena.Actors(this, playerDolls, Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.BrinaPlayer1 or (uint)OID.BrinaPlayer2 or (uint)OID.CalcaPlayer1 or (uint)OID.CalcaPlayer2 => 1,
                _ => 0
            };
        }
    }
}
