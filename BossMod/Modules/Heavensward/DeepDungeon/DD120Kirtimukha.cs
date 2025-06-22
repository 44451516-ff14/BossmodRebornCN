namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.DD120Kirtimukha;

public enum OID : uint
{
    Boss = 0x1819, // R3.600, x1
    DeepPalaceHornet = 0x1905 // R0.400, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    AutoAttackAdds = 6498, // DeepPalaceHornet->player, no cast, single-target

    AcidMist = 7134, // Boss->self, 3.0s cast, range 6+R circle
    BloodyCaress = 7133, // Boss->self, no cast, range 8+R 120-degree cone
    FinalSting = 919, // DeepPalaceHornet->player, 3.0s cast, single-target
    GoldDust = 7135, // Boss->location, 3.0s cast, range 8 circle
    Leafstorm = 7136, // Boss->self, 3.0s cast, range 50 circle
    RottenStench = 7137 // Boss->self, 3.0s cast, range 45+R width 12 rect
}

class AcidMist(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AcidMist, 9.6f);
class BossAdds(BossModule module) : Components.Adds(module, (uint)OID.DeepPalaceHornet)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            switch (e.Actor.OID)
            {
                case (uint)OID.DeepPalaceHornet:
                    e.Priority = 2;
                    e.ForbidDOTs = true;
                    break;
                case (uint)OID.Boss:
                    e.Priority = 1;
                    break;
            }
        }
    }
}
class BloodyCaress(BossModule module) : Components.Cleave(module, (uint)AID.BloodyCaress, new AOEShapeCone(11.6f, 60f.Degrees()), activeWhileCasting: false);
class FinalSting(BossModule module) : Components.SingleTargetCast(module, (uint)AID.FinalSting, "Final sting is being cast! \nKill the add or take 98% of your hp!");
class GoldDust(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GoldDust, 8f);
class Leafstorm(BossModule module) : Components.RaidwideCast(module, (uint)AID.Leafstorm);
class RottenStench(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RottenStench, new AOEShapeRect(48.6f, 6f));

class DD120KirtimukhaStates : StateMachineBuilder
{
    public DD120KirtimukhaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AcidMist>()
            .ActivateOnEnter<BossAdds>()
            .ActivateOnEnter<BloodyCaress>()
            .ActivateOnEnter<FinalSting>()
            .ActivateOnEnter<GoldDust>()
            .ActivateOnEnter<Leafstorm>()
            .ActivateOnEnter<RottenStench>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 210, NameID = 5384)]
public class DD120Kirtimukha(WorldState ws, Actor primary) : BossModule(ws, primary, SharedBounds.ArenaBounds120130.Center, SharedBounds.ArenaBounds120130);
