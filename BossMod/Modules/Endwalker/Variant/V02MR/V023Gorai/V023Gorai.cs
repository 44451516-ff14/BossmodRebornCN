namespace BossMod.Endwalker.VariantCriterion.V02MR.V023Gorai;

class Unenlightenment(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.Unenlightenment, (uint)AID.UnenlightenmentAOE, 0.5f);
class SpikeOfFlameAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpikeOfFlameAOE, 5f);

class StringSnap(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 20f), new AOEShapeDonut(20f, 30f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.StringSnap1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.StringSnap1 => 0,
                (uint)AID.StringSnap2 => 1,
                (uint)AID.StringSnap3 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

class TorchingTorment(BossModule module) : Components.BaitAwayIcon(module, 6f, (uint)IconID.Tankbuster, (uint)AID.TorchingTorment, 5.9f, tankbuster: true);

class PureShock(BossModule module) : Components.RaidwideCast(module, (uint)AID.PureShock);
class HumbleHammer(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HumbleHammer, 3f);
class FightingSpirits(BossModule module) : Components.RaidwideCast(module, (uint)AID.FightingSpirits);
class BiwaBreaker(BossModule module) : Components.RaidwideCast(module, (uint)AID.BiwaBreakerFirst, "Raidwide x5");

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945, NameID = 12373, SortOrder = 4)]
public class V023Gorai(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaChange.ArenaCenter, ArenaChange.StartingBounds)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.ShishuWhiteBaboon));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.ShishuWhiteBaboon => 1,
                _ => 0
            };
        }
    }
}
