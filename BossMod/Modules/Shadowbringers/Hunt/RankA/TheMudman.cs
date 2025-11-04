namespace BossMod.Shadowbringers.Hunt.RankA.TheMudman;

public enum OID : uint
{
    Boss = 0x281F // R=4.2
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    FeculentFlood = 16828, // Boss->self, 3.0s cast, range 40 60-degree cone
    RoyalFlush = 16826, // Boss->self, 3.0s cast, range 8 circle
    BogBequest = 16827, // Boss->self, 5.0s cast, range 5-20 donut
    GravityForce = 16829 // Boss->player, 5.0s cast, range 6 circle, interruptible, applies heavy
}

class BogBequest(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BogBequest, new AOEShapeDonut(5f, 20f));
class FeculentFlood(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FeculentFlood, new AOEShapeCone(40f, 30f.Degrees()));
class RoyalFlush(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RoyalFlush, 8f);

class GravityForce(BossModule module) : Components.BaitAwayCast(module, (uint)AID.GravityForce, 6f)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (IsBaitTarget(actor))
        {
            hints.Add("Bait away or interrupt!");
        }
    }
}

class GravityForceHint(BossModule module) : Components.CastInterruptHint(module, (uint)AID.GravityForce);

class TheMudmanStates : StateMachineBuilder
{
    public TheMudmanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BogBequest>()
            .ActivateOnEnter<FeculentFlood>()
            .ActivateOnEnter<RoyalFlush>()
            .ActivateOnEnter<GravityForce>()
            .ActivateOnEnter<GravityForceHint>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 8654)]
public class TheMudman(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
