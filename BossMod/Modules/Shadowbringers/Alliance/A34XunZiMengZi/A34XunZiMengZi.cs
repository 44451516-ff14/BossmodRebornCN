namespace BossMod.Shadowbringers.Alliance.A34XunZiMengZi;

sealed class DeployArmaments(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.DeployArmaments1, (uint)AID.DeployArmaments2,
(uint)AID.DeployArmaments3, (uint)AID.DeployArmaments4], new AOEShapeRect(50f, 9f));
sealed class UniversalAssault(BossModule module) : Components.RaidwideCast(module, (uint)AID.UniversalAssault);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", PrimaryActorOID = (uint)OID.XunZi, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9921, SortOrder = 5)]
public sealed class A34XunZiMengZi(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    // the small squares are actually dodecagons (12-gon) with a radius of 4.4, but only one edge affects the arena so we approximate it with a square
    private static readonly Square squareSmall = new(new(773.00928f, 826.94116f), 4.75f, 45f.Degrees());
    private static readonly ArenaBoundsComplex arena = new([new Square(new(800f, 800f), 24.5f)], [squareSmall,
    squareSmall with { Center = new(772.99066f, 773.00934f) }, squareSmall with { Center = new(826.99066f, 772.99066f) }, squareSmall with { Center = new(827.00928f, 826.92249f) }]);
    public Actor? BossMengZi;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        if (BossMengZi == null)
        {
            var b = Enemies((uint)OID.MengZi);
            BossMengZi = b.Count != 0 ? b[0] : null;
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(BossMengZi);
    }
}
