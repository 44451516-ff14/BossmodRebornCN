namespace BossMod.Endwalker.VariantCriterion.V02MR.V022Moko;

class AzureAuspice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AzureAuspice, new AOEShapeDonut(6f, 60f));
class KenkiRelease(BossModule module) : Components.RaidwideCast(module, (uint)AID.KenkiRelease);
class IronRain(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IronRain, 10f);
class Unsheathing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Unsheathing, 3f);
class VeilSever(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VeilSever, new AOEShapeRect(40f, 2.5f));
class ScarletAuspice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ScarletAuspice, 6f);
class MoonlessNight(BossModule module) : Components.RaidwideCast(module, (uint)AID.MoonlessNight);
class Clearout(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Clearout, new AOEShapeCone(22f, 90f.Degrees()));

abstract class Boundless(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeRect(60f, 5f));
class BoundlessScarlet(BossModule module) : Boundless(module, (uint)AID.BoundlessScarlet);
class BoundlessAzure(BossModule module) : Boundless(module, (uint)AID.BoundlessAzure);

class Explosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, new AOEShapeRect(60f, 15f), 2)
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
            return [];
        var hasDifferentRotations = count > 1 && Casters[0].Rotation != Casters[1].Rotation;
        var max = count > MaxCasts ? MaxCasts : count;
        var aoes = new AOEInstance[max];
        for (var i = 0; i < max; ++i)
        {
            var caster = Casters[i];
            aoes[i] = (caster with { Color = i == 0 && count > i ? Colors.Danger : 0, Risky = caster.Origin == Casters[0].Origin || hasDifferentRotations });
        }
        return aoes;
    }
}

class GhastlyGrasp(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GhastlyGrasp, 5);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945, NameID = 12357, SortOrder = 2)]
public class V022MokoOtherPaths(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaChange.ArenaCenter, ArenaChange.StartingBounds);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", PrimaryActorOID = (uint)OID.BossP2, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945, NameID = 12357, SortOrder = 3)]
public class V022MokoPath2(WorldState ws, Actor primary) : V022MokoOtherPaths(ws, primary);
