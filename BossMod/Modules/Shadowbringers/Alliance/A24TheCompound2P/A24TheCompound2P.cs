namespace BossMod.Shadowbringers.Alliance.A24TheCompound2P;

sealed class CentrifugalSlice(BossModule module) : Components.RaidwideCast(module, (uint)AID.CentrifugalSlice);
sealed class R012LaserAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.R012LaserAOE, 6f);
sealed class R012LaserSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.R012LaserSpread, 6f);
sealed class R012LaserTB(BossModule module) : Components.BaitAwayCast(module, (uint)AID.R012LaserTB, 6f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9646, SortOrder = 6)]
public sealed class A24TheCompound2P(WorldState ws, Actor primary) : BossModule(ws, primary, new(200f, -700f), new ArenaBoundsSquare(30f))
{
    public Actor? BossP2;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        if (BossP2 == null)
        {
            var b = Enemies((uint)OID.Compound2P);
            BossP2 = b.Count != 0 ? b[0] : null;
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        switch (StateMachine.ActivePhaseIndex)
        {
            case -1:
            case 0:
                Arena.Actor(PrimaryActor);
                break;
            case 1:
                Arena.Actor(BossP2);
                break;
        }
    }
}
