namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

sealed class Cloudsplitter(BossModule module) : Components.BaitAwayCast(module, (uint)AID.CloudsplitterAOE, 6f, tankbuster: true);
sealed class CriticalReaverRaidwide(BossModule module) : Components.CastCounter(module, (uint)AID.CriticalReaverRaidwide);
sealed class CriticalReaverEnrage(BossModule module) : Components.CastInterruptHint(module, (uint)AID.CriticalReaverEnrage);
sealed class Meteor(BossModule module) : Components.CastInterruptHint(module, (uint)AID.Meteor);
sealed class TachiGekko(BossModule module) : Components.CastGaze(module, (uint)AID.TachiGekko);
sealed class TachiKasha(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TachiKasha, 20f);
sealed class TachiYukikaze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TachiYukikaze, new AOEShapeRect(50f, 2.5f));
sealed class Raiton(BossModule module) : Components.RaidwideCast(module, (uint)AID.Raiton);
sealed class Utsusemi(BossModule module) : Components.StretchTetherSingle(module, (uint)TetherID.Utsusemi, 10f, needToKite: true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.BossGK, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1015, NameID = 13640, SortOrder = 7, PlanLevel = 100)]
public sealed class A13ArkAngels(WorldState ws, Actor primary) : BossModule(ws, primary, new(865f, -820f), new ArenaBoundsCircle(34.5f))
{
    public static readonly ArenaBoundsCircle DefaultBounds = new(25f);
    public static readonly uint[] Bosses = [(uint)OID.BossHM, (uint)OID.BossEV, (uint)OID.BossTT, (uint)OID.BossMR, (uint)OID.BossGK];

    private Actor? _bossHM;
    private Actor? _bossEV;
    private Actor? _bossMR;
    private Actor? _bossTT;
    public Actor? BossHM() => _bossHM;
    public Actor? BossEV() => _bossEV;
    public Actor? BossMR() => _bossMR;
    public Actor? BossTT() => _bossTT;
    public Actor? BossGK() => PrimaryActor;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _bossHM ??= Enemies((uint)OID.BossHM)[0];
        _bossEV ??= Enemies((uint)OID.BossEV)[0];
        _bossMR ??= Enemies((uint)OID.BossMR)[0];
        _bossTT ??= Enemies((uint)OID.BossTT)[0];
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        var comp = FindComponent<DecisiveBattle>();
        if (comp != null)
        {
            var slot = comp.AssignedBoss[pcSlot];
            if (slot != null)
                Arena.Actor(slot);
        }
        else if (Enemies((uint)OID.ArkShield) is var shield && shield.Count != 0 && !shield[0].IsDead)
            Arena.Actor(shield[0]);
        else
            Arena.Actors(Enemies(Bosses));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            if (e.Actor.FindStatus((uint)SID.Invincibility) != null)
            {
                e.Priority = AIHints.Enemy.PriorityInvincible;
                break;
            }
        }
    }
}
