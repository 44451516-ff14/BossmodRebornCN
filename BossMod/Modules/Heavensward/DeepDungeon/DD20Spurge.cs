namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.DD20Spurge;

public enum OID : uint
{
    Boss = 0x169F, // R3.6
    PalaceHornet = 0x1763 // R0.4
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    AutoAttackHornet = 6498, // PalaceHornet->player, no cast, single-target

    AcidMist = 6422, // Boss->self, 3.0s cast, range 6+R circle
    BloodyCaress = 6421, // Boss->self, no cast, range 8+R 120-degree cone
    GoldDust = 6423, // Boss->location, 3.0s cast, range 8 circle
    Leafstorm = 6424, // Boss->self, 3.0s cast, range 50 circle
    RottenStench = 6425 // Boss->self, 3.0s cast, range 45+R width 12 rect
}

class BossAdds(BossModule module) : Components.Adds(module, (uint)OID.PalaceHornet)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            switch (e.Actor.OID)
            {
                case (uint)OID.Boss:
                    e.Priority = 1;
                    break;
                case (uint)OID.PalaceHornet:
                    e.Priority = 2;
                    e.ForbidDOTs = true;
                    break;
            }
        }
    }
}
class AcidMist(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AcidMist, 9.6f);
class BloodyCaress(BossModule module) : Components.Cleave(module, (uint)AID.BloodyCaress, new AOEShapeCone(11.6f, 60f.Degrees()), activeWhileCasting: false);
class GoldDust(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GoldDust, 8f);
class Leafstorm(BossModule module) : Components.RaidwideCast(module, (uint)AID.Leafstorm);
class RottenStench(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RottenStench, new AOEShapeRect(48.6f, 6f));

class DD20SpurgeStates : StateMachineBuilder
{
    public DD20SpurgeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BossAdds>()
            .ActivateOnEnter<AcidMist>()
            .ActivateOnEnter<BloodyCaress>()
            .ActivateOnEnter<GoldDust>()
            .ActivateOnEnter<Leafstorm>()
            .ActivateOnEnter<RottenStench>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 175, NameID = 4999)]
public class DD20Spurge(WorldState ws, Actor primary) : BossModule(ws, primary, SharedBounds.ArenaBounds2090110.Center, SharedBounds.ArenaBounds2090110);
