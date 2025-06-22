namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.DD10Gancanagh;

public enum OID : uint
{
    Boss = 0x3D54, // R1.8
    PachypodiumMine = 0x3D55 // R1.5
}

public enum AID : uint
{
    Attack = 6499, // Boss->player, no cast, single-target
    AuthoritativeShriek = 31477, // Boss->self, 3.0s cast, single-target
    Mandrashock1 = 31478, // PachypodiumMine->self, 5.0s cast, range 10 circle
    Mandrashock2 = 32700, // PachypodiumMine->self, 8.0s cast, range 10 circle
    Mandrastorm = 31479 // Boss->self, 5.0s cast, range 60 circle, damage fall off AOE
}

class MandraStorm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mandrastorm, 20f);
class Mandrashock1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mandrashock1, 10f);
class Mandrashock2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mandrashock2, 10f, 6);

class DD10GancanaghStates : StateMachineBuilder
{
    public DD10GancanaghStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Mandrashock1>()
            .ActivateOnEnter<Mandrashock2>()
            .ActivateOnEnter<MandraStorm>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 897, NameID = 12240)]
public class DD10Gancanagh(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300f, -300f), new ArenaBoundsSquare(19.5f));
