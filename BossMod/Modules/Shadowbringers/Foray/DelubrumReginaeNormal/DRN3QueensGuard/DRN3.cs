namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN3QueensGuard;

sealed class OptimalPlaySword(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OptimalPlaySword, 10f);
sealed class OptimalPlayShield(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OptimalPlayShield, new AOEShapeDonut(5f, 60f));
sealed class BloodAndBoneQueenShotUnseen(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.BloodAndBoneKnight, (uint)AID.BloodAndBoneSoldier, (uint)AID.BloodAndBoneWarrior, (uint)AID.QueensShotUnseen]);
sealed class RapidSeverShotInTheDark(BossModule module) : Components.SingleTargetCasts(module, [(uint)AID.RapidSeverKnight, (uint)AID.RapidSeverSoldier, (uint)AID.RapidSeverWarrior, (uint)AID.ShotInTheDark]);
sealed class Enrages(BossModule module) : Components.CastHints(module, [(uint)AID.BloodAndBoneKnightEnrage, (uint)AID.BloodAndBoneSoldierEnrage, (uint)AID.BloodAndBoneWarriorEnrage, (uint)AID.QueensShotEnrage], "Enrage!", true);

sealed class PawnOff(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PawnOffReal, 20f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", PrimaryActorOID = (uint)OID.QueensKnight, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 760, NameID = 9838)]
public sealed class DRN3QueensGuard(WorldState ws, Actor primary) : QueensGuard(ws, primary)
{
    private Actor? _warrior;
    private Actor? _soldier;
    private Actor? _gunner;

    protected override void UpdateModule()
    {
        _warrior ??= GetActor((uint)OID.QueensWarrior);
        _soldier ??= GetActor((uint)OID.QueensSoldier);
        _gunner ??= GetActor((uint)OID.QueensGunner);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_warrior);
        Arena.Actor(_soldier);
        Arena.Actor(_gunner);
    }
}
