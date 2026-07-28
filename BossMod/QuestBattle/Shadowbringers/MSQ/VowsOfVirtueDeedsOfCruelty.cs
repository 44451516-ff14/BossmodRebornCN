namespace BossMod.QuestBattle.Shadowbringers.MSQ;

class AutoEstinien(WorldState ws) : UnmanagedRotation(ws, 3f)
{
    protected override void Exec(Actor? primaryTarget)
    {
        if (primaryTarget == null)
        {
            return;
        }

        var gcd = ComboAction switch
        {
            Roleplay.AID.SonicThrust => Roleplay.AID.CoerthanTorment,
            Roleplay.AID.DoomSpike => Roleplay.AID.SonicThrust,
            _ => Roleplay.AID.DoomSpike
        };

        UseAction(gcd, primaryTarget);
        if (Player.HPRatio < 0.5f)
            UseAction(Roleplay.AID.AquaVitae, Player, -10);

        UseAction(Roleplay.AID.SkydragonDive, primaryTarget, -10f);
    }
}

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 702u)]
public class VowsOfVirtueDeedsOfCruelty(WorldState ws) : QuestBattle(ws)
{
    private readonly AutoEstinien _ai = new(ws);

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(134f, default, 400f))
            .With(obj => {
                obj.OnConditionChange += (flag, val) => obj.CompleteIf(flag == Dalamud.Game.ClientState.Conditions.ConditionFlag.Jumping61 && !val);
            }),

        new QuestObjective(ws)
            .WithConnection(new Vector3(240f, -40f, 287f))
    ];

    public override void AddQuestAIHints(Actor player, AIHints hints) => _ai.Execute(player, hints);
}
