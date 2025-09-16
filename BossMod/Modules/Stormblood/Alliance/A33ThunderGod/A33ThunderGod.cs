namespace BossMod.Stormblood.Alliance.A33ThunderGod;

class HallowedBolt(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(15f), new AOEShapeDonut(15f, 30f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HallowedBolt1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.HallowedBolt1 => 0,
                (uint)AID.HallowedBolt2 => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7899)] //7917
public class A33ThunderGod(WorldState ws, Actor primary) : BossModule(ws, primary, new(-600, -600), arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Circle(new(-612.5f, -578.4f), 10), new Circle(new(-587.5f, -578.4f), 10), new Circle(new(-575, -600), 10), new Circle(new(-587.5f, -621.5f), 10), new Circle(new(-612.5f, -621.5f), 10), new Circle(new(-625, -600), 10), new Donut(new(-600, -600), 20, 27)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.EphemeralKnight));
    }
}
