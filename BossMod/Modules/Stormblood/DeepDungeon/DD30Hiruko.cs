namespace BossMod.Stormblood.DeepDungeon.HeavenOnHigh.DD30Hiruko;

public enum OID : uint
{
    Boss = 0x2251, // R5.25
    RaiunClouds = 0x2252 // R1.0
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target

    CloudCall = 11290, // Boss->self, 3.0s cast, single-target, spawns the clouds with this cast
    LightningBolt = 11294, // RaiunClouds->self, 8.0s cast, range 8 circle
    LightningStrike = 11292, // Boss->self, 2.5s cast, range 50+R width 6 rect 
    Shiko = 11291, // Boss->self, 5.0s cast, range 100 circle, 6 yalms is the damage falloff, Knockup into the cloud to make a safe spot
    Supercell = 11293 // Boss->self, 7.0s cast, range 50+R width 100 rect
}

class LightningStrike(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightningStrike, new AOEShapeRect(55.25f, 3f));
class Shiko(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shiko, 6f);
class LightningBolt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightningBolt, new AOEShapeDonut(1f, 8f));
class Supercell(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Supercell, new AOEShapeRect(55.25f, 50f));

class DD30HirukoStates : StateMachineBuilder
{
    public DD30HirukoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Supercell>()
            .ActivateOnEnter<Shiko>()
            .ActivateOnEnter<LightningStrike>()
            .ActivateOnEnter<LightningBolt>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 542, NameID = 7482)]
public class DD30Hiruko(WorldState ws, Actor primary) : HoHArena1(ws, primary);
