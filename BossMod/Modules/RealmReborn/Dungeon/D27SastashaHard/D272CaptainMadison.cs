namespace BossMod.RealmReborn.Dungeon.D27SastashaHard.D272CaptainMadison;

public enum OID : uint
{
    Boss = 0xCBB,
    CaptainsGuard = 0xCBC,
    CaptainsBoy = 0x0CBD,
    CaptainsLeech = 0xCBE,
    CaptainsSlave = 0xCBF
}

public enum AID : uint
{
    Water = 971, // CBE->player, 1.0s cast, single-target
    AutoAttack = 870, // CBD/Boss/CBC->player, no cast, single-target
    Hornswaggle = 3062, // Boss->self, no cast, range 5+R ?-degree cone
    Rive = 1299, // CBD->self, 2.5s cast, range 30+R width 2 rect
    Bloodstain = 1099, // CBC->self, 2.5s cast, range 5 circle
    Thunder = 968, // CBE->player, 1.0s cast, single-target
    Tackle = 3068 // CBF->player, no cast, single-target
}

class Bloodstain(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Bloodstain, new AOEShapeCircle(7.5f));

// TODO: Confirm angle
class Hornswaggle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hornswaggle, new AOEShapeCone(5f, 45f.Degrees()));

class Rive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Rive, new AOEShapeRect(18f, 1.5f));

class Water(BossModule module) : Components.SingleTargetInstant(module, (uint)AID.Water, 1f);

class CaptainMadisonAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.CaptainsGuard, (uint)OID.CaptainsBoy, (uint)OID.CaptainsLeech]);

class D272CaptainMadisonStates : StateMachineBuilder
{
    public D272CaptainMadisonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CaptainMadisonAdds>()
            .ActivateOnEnter<Hornswaggle>()
            .ActivateOnEnter<Rive>()
            .ActivateOnEnter<Water>()
            .ActivateOnEnter<Bloodstain>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, Contributors = "Zaventh", GroupID = 28, NameID = 3015)]
public class D272CaptainMadison(WorldState ws, Actor primary) : BossModule(ws, primary, new(-103f, 136f), new ArenaBoundsRect(18f, 20f));
