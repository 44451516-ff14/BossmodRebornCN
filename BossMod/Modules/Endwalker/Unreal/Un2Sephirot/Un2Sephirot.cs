namespace BossMod.Endwalker.Unreal.Un2Sephirot;

class P1TripleTrial(BossModule module) : Components.Cleave(module, (uint)AID.TripleTrial, new AOEShapeCone(18.5f, 30.Degrees())); // TODO: verify angle
class P1Ein(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Ein, new AOEShapeRect(50, 22.5f));
class P2GenesisCochma(BossModule module) : Components.CastCounter(module, (uint)AID.GenesisCochma);
class P2GenesisBinah(BossModule module) : Components.CastCounter(module, (uint)AID.GenesisBinah);
class P3EinSofOhr(BossModule module) : Components.CastCounter(module, (uint)AID.EinSofOhrAOE);
class P3Yesod(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Yesod, 4);
class P3PillarOfMercyAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PillarOfMercyAOE, 5);
class P3PillarOfMercyKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.PillarOfMercyAOE, 17);
class P3Malkuth(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Malkuth, 25);
class P3Ascension(BossModule module) : Components.CastCounter(module, (uint)AID.Ascension); // TODO: show safe spot?..
class P3PillarOfSeverity(BossModule module) : Components.CastCounter(module, (uint)AID.PillarOfSeverityAOE);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.RemovedUnreal, GroupID = 875, NameID = 4776)]
public class Un2Sephirot(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsCircle(20))
{
    public Actor? BossP1() => PrimaryActor.IsDestroyed ? null : PrimaryActor;

    private Actor? _bossP3;
    public Actor? BossP3() => _bossP3;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _bossP3 ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies(OID.BossP3).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        if (StateMachine.ActivePhaseIndex <= 0)
            Arena.Actor(PrimaryActor);
        else if (StateMachine.ActivePhaseIndex == 2)
            Arena.Actor(_bossP3);
    }
}
