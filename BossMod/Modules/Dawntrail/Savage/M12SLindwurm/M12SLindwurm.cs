namespace BossMod.Dawntrail.Savage.M12SLindwurm;

sealed class Slaughtershed0(BossModule module) : Components.RaidwideCast(module, (uint)AID.Slaughtershed0);
sealed class unk_46194(BossModule module) : Components.RaidwideCast(module, (uint)AID.unk_46194);
sealed class RavenousReach1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RavenousReach1, new AOEShapeCone(35f, 60f.Degrees()));
sealed class PhagocyteSpotlight0(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PhagocyteSpotlight0, 5f);
sealed class GrandEntrance0(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrandEntrance0, 2f);
sealed class GrandEntrance1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrandEntrance1, 2f);
sealed class GrandEntrance2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrandEntrance2, 2f);
sealed class GrandEntrance3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrandEntrance3, 2f);
sealed class BringDownTheHouse0(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BringDownTheHouse0, new AOEShapeRect(10f, 10f));
sealed class BringDownTheHouse1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BringDownTheHouse1, new AOEShapeRect(10f, 7.5f));
sealed class BringDownTheHouse2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BringDownTheHouse2, new AOEShapeRect(10f, 5f));
sealed class DramaticLysis0(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DramaticLysis0, 6f);
sealed class DramaticLysis1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DramaticLysis1, 9f);
sealed class DramaticLysis2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DramaticLysis2, 4f);
sealed class DramaticLysis4(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DramaticLysis4, 6f);
sealed class FourthWallFusion0(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FourthWallFusion0, 6f);
sealed class FourthWallFusion1(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.FourthWallFusion1, 6f);
sealed class SplitScourge1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SplitScourge1, new AOEShapeRect(60f, 5f));
sealed class RoilingMass0(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RoilingMass0, 3f);
sealed class RoilingMass1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RoilingMass1, 3f);
sealed class PhagocyteSpotlight1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PhagocyteSpotlight1, 5f);
sealed class VisceralBurst(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.VisceralBurst, 6f);
sealed class TheFixer(BossModule module) : Components.RaidwideCast(module, (uint)AID.TheFixer);
sealed class UnmitigatedExplosion(BossModule module) : Components.RaidwideCast(module, (uint)AID.UnmitigatedExplosion);
sealed class WingedScourge2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WingedScourge2, new AOEShapeCone(50f, 15f.Degrees()));
sealed class TopTierSlam1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TopTierSlam1, 5f);
sealed class MightyMagic1(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.MightyMagic1, 5f);
sealed class FirefallSplash1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FirefallSplash1, 5f);
sealed class ManaBurst1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ManaBurst1, 20f);
sealed class HeavySlam0(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavySlam0, 5f);
sealed class HeavySlam2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavySlam2, 5f);
sealed class EsotericFinisher(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.EsotericFinisher, 10f);
sealed class FourthWallFusion2(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.FourthWallFusion2, 6f);
sealed class unk_46395(BossModule module) : Components.SimpleAOEs(module, (uint)AID.unk_46395, new AOEShapeDonut(20f, 30f));
sealed class Metamitosis1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Metamitosis1, 3f);
sealed class SerpentineScourge2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SerpentineScourge2, new AOEShapeRect(30f, 10f));
sealed class RaptorKnuckles2(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.RaptorKnuckles2, 30f);
sealed class UnmitigatedImpact(BossModule module) : Components.RaidwideCast(module, (uint)AID.UnmitigatedImpact);
sealed class ArcadiaAflame(BossModule module) : Components.RaidwideCast(module, (uint)AID.ArcadiaAflame);
sealed class RefreshingOverkill1(BossModule module) : Components.RaidwideCast(module, (uint)AID.RefreshingOverkill1);
sealed class RefreshingOverkill2(BossModule module) : Components.RaidwideCast(module, (uint)AID.RefreshingOverkill2);
sealed class Splattershed2(BossModule module) : Components.RaidwideCast(module, (uint)AID.Splattershed2);
sealed class unk_48028(BossModule module) : Components.RaidwideCast(module, (uint)AID.unk_48028);

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    Contributors = "SRP",
    PrimaryActorOID = (uint)OID.Lindwurm1,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = (uint)GroupID.Lindwurm,
    NameID = (uint)NameID.Lindwurm1)]
public sealed class M12SLindwurm(WorldState ws, Actor primary) : BossModule(ws, primary, arenaCenter, DefaultBounds)
{
    private static readonly WPos arenaCenter = new(100f, 100f);
    public static readonly ArenaBoundsRect DefaultBounds = new ArenaBoundsRect(20f, 15f);
}

