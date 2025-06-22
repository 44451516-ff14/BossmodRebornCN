namespace BossMod.Stormblood.Trial.T09Seiryu;

class HundredTonzeSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HundredTonzeSwing, 16f);
class CoursingRiver(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.CoursingRiverAOE, 25f, true, kind: Kind.DirForward)
{
    private readonly Handprint _aoe = module.FindComponent<Handprint>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoe.Casters.Count == 0 && Casters.Count != 0)
        {
            var c = Casters[0];
            hints.AddForbiddenZone(ShapeDistance.Rect(c.CastInfo!.Rotation.AlmostEqual(90f.Degrees(), Angle.DegToRad) ? c.Position - new WDir(12.5f, default) : c.Position - new WDir(-12.5f, default), c.Rotation, 50f, default, 20f), Module.CastFinishAt(c.CastInfo));
        }
    }
}

class DragonsWake(BossModule module) : Components.RaidwideCast(module, (uint)AID.DragonsWake2);
class FifthElement(BossModule module) : Components.RaidwideCast(module, (uint)AID.FifthElement);
class FortuneBladeSigil(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FortuneBladeSigil, new AOEShapeRect(100f, 2f));

class InfirmSoul(BossModule module) : Components.BaitAwayCast(module, (uint)AID.InfirmSoul, 4f, tankbuster: true);

class SerpentDescending(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, (uint)AID.SerpentDescending, 5f, 6f);
class YamaKagura(BossModule module) : Components.SimpleAOEs(module, (uint)AID.YamaKagura, new AOEShapeRect(60f, 3f));
class Handprint(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Handprint2, new AOEShapeCone(40f, 90f.Degrees()));

class ForceOfNature1(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.ForceOfNature1, 10f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 10f), Module.CastFinishAt(Casters[0].CastInfo));
    }
}
class ForceOfNature2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ForceOfNature2, 5f);
class KanaboBait(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(45f, 30f.Degrees()), (uint)TetherID.BaitAway, (uint)AID.KanaboVisual2, (uint)OID.IwaNoShiki, 5.9f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (CurrentBaits.Any(x => x.Target == actor))
            hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 19f), WorldState.FutureTime(ActivationDelay));
    }
}

class KanaboAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Kanabo, new AOEShapeCone(45f, 30f.Degrees()));
class BlueBolt(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.BlueBoltMarker, (uint)AID.BlueBolt, 5.9f, 83f, 2.5f);
class ForbiddenArts(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.ForbiddenArtsMarker, (uint)AID.ForbiddenArtsSecond, 5.2f, 84.4f, 4); // this hits twice
class RedRush(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(82.6f, 2.5f), (uint)TetherID.BaitAway, (uint)AID.RedRush, (uint)OID.AkaNoShiki, 6f)
{
    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        base.OnTethered(source, tether);
        var (player, enemy) = DetermineTetherSides(source, tether);
        if (player != null && enemy != null)
            Module.FindComponent<BlueBolt>()!.ForbiddenActors.Add(player);
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        base.OnUntethered(source, tether);
        var (player, enemy) = DetermineTetherSides(source, tether);
        if (player != null && enemy != null)
            Module.FindComponent<BlueBolt>()!.ForbiddenActors.Remove(player);
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var activation = WorldState.FutureTime(ActivationDelay);
        if (CurrentBaits.Any(x => x.Target == actor) && Arena.Bounds == T09Seiryu.Phase2Bounds)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 5), activation);
        else if (CurrentBaits.Any(x => x.Target == actor) && Arena.Bounds == T09Seiryu.Phase1Bounds)
            hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 18.5f), activation);
    }
}

class ArenaChange(BossModule module) : BossComponent(module)
{

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.StrengthOfSpirit) // in phase 2 the arena no longer got a wall and we need to add back the player hitboxradius
            Arena.Bounds = T09Seiryu.Phase2Bounds;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 637, NameID = 7922)]
public class T09Seiryu(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), Phase1Bounds)
{
    public static readonly ArenaBounds Phase1Bounds = new ArenaBoundsCircle(19.5f);
    public static readonly ArenaBounds Phase2Bounds = new ArenaBoundsCircle(20);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.DoroNoShiki));
        Arena.Actors(Enemies(OID.NumaNoShiki));
    }
}
