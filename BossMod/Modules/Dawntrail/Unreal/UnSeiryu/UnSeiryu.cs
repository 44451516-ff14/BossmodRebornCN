namespace BossMod.Dawntrail.Unreal.UnSeiryu;

sealed class FifthElement(BossModule module) : Components.RaidwideCast(module, (uint)AID.FifthElement);
sealed class SerpentDescendingSpread(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.SerpentDescending, (uint)AID.SerpentDescendingSpread, 5f, 6.1d);
sealed class SerpentsDescendingAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SerpentDescendingAOE, 5f);
sealed class FortuneBladeSigil(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.FortuneBladeSigil1, (uint)AID.FortuneBladeSigil2], new AOEShapeRect(50.5f, 2f), 9, 18);
sealed class Handprint(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Handprint1, new AOEShapeCone(40f, 90f.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.Seiryu, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1042u, NameID = 7922u, Category = BossModuleInfo.Category.Unreal, Expansion = BossModuleInfo.Expansion.Dawntrail, PlanLevel = 100, SortOrder = 1)]
public sealed class UnSeiryu(WorldState ws, Actor primary) : Stormblood.Trial.T09Seiryu.Seiryu(ws, primary);
