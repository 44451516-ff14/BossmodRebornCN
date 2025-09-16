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
public class Un2Sephirot(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsCircle(20f))
{
    public Actor? BossP1() => PrimaryActor.IsDestroyed ? null : PrimaryActor;

    private Actor? _bossP3;
    public Actor? BossP3() => _bossP3;

    protected override void UpdateModule()
    {
        _bossP3 ??= GetActor((uint)OID.BossP3);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        if (StateMachine.ActivePhaseIndex <= 0)
            Arena.Actor(PrimaryActor);
        else if (StateMachine.ActivePhaseIndex == 2)
            Arena.Actor(_bossP3);
    }
}
