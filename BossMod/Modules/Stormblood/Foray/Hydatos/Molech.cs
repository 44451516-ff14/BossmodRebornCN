namespace BossMod.Stormblood.Foray.Hydatos.Molech;

public enum OID : uint
{
    Boss = 0x275D, // R6.0
    Adulator = 0x275E // R2.8
}

public enum AID : uint
{
    AutoAttack = 15492, // Boss->player, no cast, single-target

    W11TonzeSwipeAdds = 14978, // Adulator->player, no cast, single-target
    W11TonzeSwipe = 14972, // Boss->self, 3.0s cast, range 9 120-degree cone
    W111TonzeSwing = 14973, // Boss->self, 4.0s cast, range 13 circle
    OrderToAssault = 14975, // Boss->self, 3.0s cast, range 100 circle, buffs Adulators
    OrderToStandFast = 14976, // Boss->self, 3.0s cast, range 100 circle, buffs Adulators
    W111TonzeSwingAdds = 14979, // Adulator->self, 3.0s cast, range 13 circle
    W111TonzeSwingBig = 14974, // Boss->self, 4.0s cast, range 20 circle
    ZoomIn = 14980 // Adulator->location, 3.0s cast, width 8 rect charge
}

class W11TonzeSwipe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.W11TonzeSwipe, new AOEShapeCone(9f, 60f.Degrees()));
class W111TonzeSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.W111TonzeSwing, 13f);
class W111TonzeSwingAdds(BossModule module) : Components.SimpleAOEs(module, (uint)AID.W111TonzeSwingAdds, 13f);
class W111TonzeSwingBig(BossModule module) : Components.SimpleAOEs(module, (uint)AID.W111TonzeSwingBig, 20f);
class ZoomIn(BossModule module) : Components.ChargeAOEs(module, (uint)AID.ZoomIn, 4f);

class MolechStates : StateMachineBuilder
{
    public MolechStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<W11TonzeSwipe>()
            .ActivateOnEnter<W111TonzeSwing>()
            .ActivateOnEnter<W111TonzeSwingAdds>()
            .ActivateOnEnter<W111TonzeSwingBig>()
            .ActivateOnEnter<ZoomIn>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "xan, Malediktus", GroupType = BossModuleInfo.GroupType.EurekaNM, GroupID = 639, NameID = 1414, SortOrder = 3)]
public class Molech(WorldState ws, Actor primary) : SimpleBossModule(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Adulator));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Boss => 1,
                (uint)OID.Adulator => AIHints.Enemy.PriorityPointless,
                _ when e.Actor.InCombat => 0,
                _ => AIHints.Enemy.PriorityUndesirable
            };
        }
    }
}
