namespace BossMod.QuestBattle.Shadowbringers.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 664u)]
public class AFeastOfLies(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(0.02f, 5.96f, -56.50f))
            .Hints((player, hints) => {
                hints.PathfindMapCenter = new(default, player.PosRot.Z);
                hints.PathfindMapBounds = new ArenaBoundsRect(8f, 20f);
            })
            .With(obj => {
                obj.OnEventObjectStateChanged += (act, state) => obj.CompleteIf(act.OID == 0x1EACEFu && state == 2);
            }),

        new QuestObjective(ws)
            .WithConnection(new Vector3(-0.00f, 6.00f, -29.00f))
            .Hints((player, hints) => {
                hints.PathfindMapCenter = new(default, -28.5f);
                hints.PathfindMapBounds = new ArenaBoundsRect(11f, 16f);
            })
            .With(obj => {
                var redDead = false;
                var blueDead = false;
                obj.OnActorKilled += (act) => {
                    redDead |= act.OID == 0x2959u;
                    blueDead |= act.OID == 0x2958u;
                    obj.CompleteIf(redDead && blueDead);
                };
            }),

        new QuestObjective(ws)
            .WithConnection(new Vector3(default, 82.9f, -38f))
            .CompleteOnCreated(0x295Au),

        new QuestObjective(ws)
            .Hints((player, hints) => hints.ForcedMovement = new(default, default, 1f))
    ];
}
