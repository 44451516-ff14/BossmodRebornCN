namespace BossMod.RealmReborn.Dungeon.D07Brayflox.D074Aiatar;

public enum OID : uint
{
    Boss = 0x38C5, // x1
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast

    SalivousSnap = 28659, // Boss->player, 5.0s cast, tankbuster
    ToxicVomit = 28656, // Boss->self, 3.0s cast, visual
    ToxicVomitAOE = 28657, // Helper->self, 5.0s cast, range 2 aoe
    Burst = 28658, // Helper->self, 9.0s cast, range 10 aoe
    DragonBreath = 28660 // Boss->self, 3.0s cast, range 30 width 8 rect
}

class SalivousSnap(BossModule module) : Components.SingleTargetCast(module, (uint)AID.SalivousSnap);
class ToxicVomit(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ToxicVomitAOE, 2);
class Burst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Burst, 10, 4);
class DragonBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DragonBreath, new AOEShapeRect(30, 4));

class D074AiatarStates : StateMachineBuilder
{
    public D074AiatarStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SalivousSnap>()
            .ActivateOnEnter<ToxicVomit>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<DragonBreath>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 8, NameID = 1279)]
public class D074Aiatar(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly ArenaBoundsCustom arena = new([new Circle(new(-26, -236), 19.5f)], [new Rectangle(new(-46.75f, -236), 2, 20)]);
}
