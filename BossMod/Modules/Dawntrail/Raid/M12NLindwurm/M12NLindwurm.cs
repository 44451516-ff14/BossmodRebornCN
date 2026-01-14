namespace BossMod.Dawntrail.Raid.M12NLindwurm;

sealed class TheFixer(BossModule module) : Components.RaidwideCast(module, (uint)AID.TheFixer);
sealed class SerpentineScourge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SerpentineScourge, new AOEShapeRect(30f, 10f));
sealed class RavenousReach(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RavenousReach, new AOEShapeCone(35f, 60f.Degrees()), riskyWithSecondsLeft: 7d);
sealed class Splattershed(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.Splattershed1Visual, (uint)AID.Splattershed2Visual]);
sealed class BringDownTheHouse(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BringDownTheHouse, new AOEShapeRect(15f, 5f));
sealed class SplitScourge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SplitScourge, new AOEShapeRect(30f, 5f));
sealed class VenomousScourge(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.VenomousScourge, (uint)AID.VenomousScourge, 5f, 5d);
sealed class GrandEntrance(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.GrandEntrance1, (uint)AID.GrandEntrance2], 2f);
sealed class VisceralBurst(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.TankBait, (uint)AID.VisceralBurst, 6f, 5d);
sealed class MindlessFlesh(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.MindlessFlesh1, (uint)AID.MindlessFlesh2, (uint)AID.MindlessFlesh3, (uint)AID.MindlessFlesh4, (uint)AID.MindlessFlesh5], new AOEShapeRect(30f, 4f), 2);
sealed class MindlessFleshBig(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MindlessFleshBig, new AOEShapeRect(30f, 17.5f), riskyWithSecondsLeft: 9d)
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return [];
        }

        var aoes = CollectionsMarshal.AsSpan(Casters);
        if (aoes[0].Activation.AddSeconds(-9d) < WorldState.CurrentTime)
        {
            return aoes;
        }
        return [];
    }
}
sealed class Burst(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCircle circle = new(12f);
    public readonly List<AOEInstance> AOEs = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan([.. AOEs]);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.BurstBlob)
        {
            if (state == 0x00100020u)
                AOEs.Add(new(circle, actor.Position.Quantized()));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Burst)
            AOEs.Clear();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
StatesType = typeof(M12NLindwurmStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = typeof(SID),
TetherIDType = null,
IconIDType = typeof(IconID),
PrimaryActorOID = (uint)OID.Lindwurm,
Contributors = "gynorhino",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.Raid,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1074u,
NameID = 14378u,
SortOrder = 1,
PlanLevel = 0)]
public sealed class M12NLindwurm(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, ArenaBounds)
{
    public static readonly WPos ArenaCenter = new(100f, 100f);
    public static readonly ArenaBoundsRect ArenaBounds = new(20f, 15f);
}
