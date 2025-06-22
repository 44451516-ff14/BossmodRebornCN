namespace BossMod.RealmReborn.Trial.T08ThornmarchH;

public enum OID : uint
{
    WhiskerwallKupdiKoop = 0x391C, // R0.900, x1 (first set)
    RuffletuftKuptaKapa = 0x391D, // R0.900, x1 (second set)
    WoolywartKupquKogi = 0x391F, // R0.900, x1 (second set)
    FurryfootKupliKipp = 0x391E, // R0.900, x1 (third set)
    PuksiPikoTheShaggysong = 0x3921, // R0.900, x1 (third set)
    PuklaPukiThePomburner = 0x3920, // R0.900, x1 (fourth set)
    PuknaPakoTheTailturner = 0x3922, // R0.900, x1 (fourth set)
    GoodKingMoggleMogXII = 0x3923, // R3.000, spawn during fight (main boss)
    PomBog = 0x1E8F67, // R0.500, EventObj type, spawn during fight
    Helper = 0x233C

}

public enum AID : uint
{
    AutoAttack1 = 870, // WhiskerwallKupdiKoop/RuffletuftKuptaKapa/PuknaPakoTheTailturner->player, no cast, single-target
    AutoAttack2 = 872, // FurryfootKupliKipp/PuklaPukiThePomburner/GoodKingMoggleMogXII->player, no cast, single-target
    AutoAttack3 = 29187, // WoolywartKupquKogi->player, no cast, single-target

    SpinningMogshield = 29216, // WhiskerwallKupdiKoop->self, 3.0s cast, range 6 circle aoe

    ThousandKuponzeSwipe = 29215, // RuffletuftKuptaKapa->self, 5.0s cast, range 20 90-degree cone aoe
    MograinOfDeath = 29191, // WoolywartKupquKogi->self, 3.0s cast, single-target, visual
    MograinOfDeathAOE = 29192, // Helper->player, 6.0s cast, range 6 circle spread

    PomHoly = 29211, // FurryfootKupliKipp->self, 5.0s cast, raidwide
    PomCure = 29212, // FurryfootKupliKipp->PuksiPikoTheShaggysong, 3.0s cast, single-target
    MoggledayNightFever = 29213, // PuksiPikoTheShaggysong->self, 5.0s cast, range 30 120-degree cone aoe

    MoogleThrust = 29214, // PuknaPakoTheTailturner->player, 5.0s cast, single-target, tankbuster
    PomMeteor = 29193, // PuklaPukiThePomburner->self, 3.0s cast, single-target, visual
    Explosion = 29194, // Helper->location, no cast, range 5 circle tower
    BigExplosion = 29195, // Helper->location, no cast, raidwide (by unsoaked tower)

    MementoMoogle = 29217, // GoodKingMoggleMogXII->self, 6.0s cast, raidwide
    PomHolyBoss = 29210, // GoodKingMoggleMogXII->self, 5.0s cast, raidwide
    ThousandKuponzeCharge = 29209, // GoodKingMoggleMogXII->player, 5.0s cast, single-target tankbuster

    GoodKingsDecree1 = 29188, // GoodKingMoggleMogXII->self, 4.0s cast, single-target, visual
    PomBog = 29207, // PuknaPakoTheTailturner->self, 5.0s cast, range 8 circle voidzone
    MogStone = 29203, // GoodKingMoggleMogXII->self, 3.0s cast, single-target, visual
    MogStoneAOE = 29204, // Helper->player, 6.0s cast, range 6 circle stack

    GoodKingsDecree2 = 29189, // GoodKingMoggleMogXII->self, 4.0s cast, single-target, visual
    MoogleGoRoundBoss = 29196, // GoodKingMoggleMogXII->self, 9.0s cast, range 20 circle aoe
    MoogleGoRoundAdd = 29197, // WhiskerwallKupdiKoop/RuffletuftKuptaKapa->self, 9.0s cast, range 20 circle aoe
    TwinPomMeteor = 29205, // GoodKingMoggleMogXII->self, 5.0s cast, single-target, visual
    TwinPomMeteorAOE = 29206, // Helper->player, 5.0s cast, range 6 circle shared tankbuster

    GoodKingsDecree3 = 29190, // GoodKingMoggleMogXII->self, 4.0s cast, single-target, visual
    MogComet = 29198, // GoodKingMoggleMogXII->self, 3.0s cast, single-target, visual
    MogCometAOE = 29199, // Helper->location, 3.0s cast, range 6 circle baited aoe
    PomStone = 29200, // FurryfootKupliKipp->self, 3.0s cast, single-target, visual
    PomStoneIn = 29201, // Helper->self, 5.0s cast, range 10 circle
    PomStoneMid = 29202, // Helper->self, 5.0s cast, range 10-20 donut
    PomStoneOut = 29619, // Helper->self, 5.0s cast, range 20-30 donut
    MogCreation = 29208 // GoodKingMoggleMogXII->self, 3.0s cast, range 50 width 10 rect aoe
}

class SpinningMogshield(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpinningMogshield, 6f);
class ThousandKuponzeSwipe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThousandKuponzeSwipe, new AOEShapeCone(20f, 45f.Degrees()));
class MograinOfDeath(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.MograinOfDeathAOE, 6f);
class PomHoly(BossModule module) : Components.RaidwideCast(module, (uint)AID.PomHoly);
class MoggledayNightFever(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MoggledayNightFever, new AOEShapeCone(30f, 60f.Degrees()));
class MoogleThrust(BossModule module) : Components.SingleTargetCast(module, (uint)AID.MoogleThrust);
class MementoMoogle(BossModule module) : Components.RaidwideCast(module, (uint)AID.MementoMoogle);
class PomHolyBoss(BossModule module) : Components.RaidwideCast(module, (uint)AID.PomHolyBoss);
class ThousandKuponzeCharge(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ThousandKuponzeCharge);
class PomBog(BossModule module) : Components.VoidzoneAtCastTarget(module, 8f, (uint)AID.PomBog, GetVoidzones, 0.8f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.PomBog);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}
class MogStone(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.MogStoneAOE, 6f, 8, 8);
class TwinPomMeteor(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.TwinPomMeteorAOE, 6f);
class MogComet(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MogCometAOE, 6);
class MogCreation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MogCreation, new AOEShapeRect(50f, 5f));

// note: this fight has well-timed state machine for all phases, but it's just too simple to bother...
class T08ThornmarchHStates : StateMachineBuilder
{
    public T08ThornmarchHStates(BossModule module) : base(module)
    {
        SimplePhase(0, id => { SimpleState(id, 10000, "Enrage"); }, "Single phase")
            .ActivateOnEnter<SpinningMogshield>()
            .ActivateOnEnter<ThousandKuponzeSwipe>()
            .ActivateOnEnter<MograinOfDeath>()
            .ActivateOnEnter<PomHoly>()
            .ActivateOnEnter<MoggledayNightFever>()
            .ActivateOnEnter<MoogleThrust>()
            .ActivateOnEnter<PomMeteor>()
            .ActivateOnEnter<MementoMoogle>()
            .ActivateOnEnter<PomHolyBoss>()
            .ActivateOnEnter<ThousandKuponzeCharge>()
            .ActivateOnEnter<PomBog>()
            .ActivateOnEnter<MogStone>()
            .ActivateOnEnter<MoogleGoRound>()
            .ActivateOnEnter<TwinPomMeteor>()
            .ActivateOnEnter<MogComet>()
            .ActivateOnEnter<PomStone>()
            .ActivateOnEnter<MogCreation>()
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.WhiskerwallKupdiKoop, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 66, NameID = 725)]
public class T08ThornmarchH(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsCircle(21))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.RuffletuftKuptaKapa));
        Arena.Actors(Enemies((uint)OID.WoolywartKupquKogi));
        Arena.Actors(Enemies((uint)OID.FurryfootKupliKipp));
        Arena.Actors(Enemies((uint)OID.PuksiPikoTheShaggysong));
        Arena.Actors(Enemies((uint)OID.PuklaPukiThePomburner));
        Arena.Actors(Enemies((uint)OID.PuknaPakoTheTailturner));
        Arena.Actors(Enemies((uint)OID.GoodKingMoggleMogXII));
    }
}
