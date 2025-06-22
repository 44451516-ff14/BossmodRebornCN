namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Sartauvoir;

sealed class PyrokinesisAOE(BossModule module) : Components.RaidwideCast(module, (uint)AID.PyrokinesisAOE);

sealed class Flamedive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Flamedive, new AOEShapeRect(55f, 2.5f));
sealed class BurningBlade(BossModule module) : Components.SingleTargetCast(module, (uint)AID.BurningBlade);

sealed class MannatheihwonFlameRW(BossModule module) : Components.RaidwideCast(module, (uint)AID.MannatheihwonFlameRaidwide);
sealed class MannatheihwonFlameRect(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MannatheihwonFlameRect, new AOEShapeRect(50f, 4f));
sealed class MannatheihwonFlameCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MannatheihwonFlameCircle, 10f);

sealed class Brand(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LeftBrand, (uint)AID.RightBrand], new AOEShapeCone(40f, 90f.Degrees()));

sealed class Pyrocrisis(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Pyrocrisis, 6f);
sealed class Pyrodoxy(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Pyrodoxy, 6f, 8);

sealed class ThermalGustAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThermalGustAOE, new AOEShapeRect(44f, 5f));
sealed class GrandCrossflameAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrandCrossflameAOE, new AOEShapeCross(40f, 9f));
sealed class TimeEruption(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ReverseTimeEruption1, (uint)AID.ReverseTimeEruption2,
(uint)AID.TimeEruption1, (uint)AID.TimeEruption2], new AOEShapeRect(20f, 10f), 2, 4);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.TheDaldriada, GroupID = 778, NameID = 9384, SortOrder = 2)]
public sealed class DAL1Sartauvoir(WorldState ws, Actor primary) : BossModule(ws, primary, new(631f, 157f), new ArenaBoundsSquare(19f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
    }
}
