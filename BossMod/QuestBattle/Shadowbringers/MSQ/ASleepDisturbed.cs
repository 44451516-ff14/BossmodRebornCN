namespace BossMod.QuestBattle.Shadowbringers.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 724)]
public class ASleepDisturbed(WorldState ws) : QuestBattle(ws)
{
    private static readonly HashSet<int> InteractTargets = BuildInteractTargets();

    private static HashSet<int> BuildInteractTargets()
    {
        var set = new HashSet<int>
        {
            // opo statue (first correct answer)
            0x1EAF81,
            // wolf statue (second correct answer)
            // easier to just pick wolf twice than figure out what state transition makes the serpent statue the right choice
            0x1EAF7E,
        };
        // list of all the correct cards
        for (var i = 0x1EAF8C; i < 0x1EAF8C + 6; ++i)
            set.Add(i);
        return set;
    }

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .Hints((player, hints) => {
                Actor? best = null;
                foreach (var a in World.Actors)
                {
                    if (InteractTargets.Contains((int)a.OID) && a.IsTargetable)
                    {
                        if (best == null || a.OID < best.OID)
                            best = a;
                    }
                }
                hints.InteractWithTarget = best;
            })
    ];
}
