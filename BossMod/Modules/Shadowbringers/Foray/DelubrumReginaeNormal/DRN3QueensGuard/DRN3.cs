namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN3QueensGuard;

sealed class OptimalPlaySword(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OptimalPlaySword, 10f);
sealed class OptimalPlayShield(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OptimalPlayShield, new AOEShapeDonut(5f, 60f));
sealed class BloodAndBoneQueenShotUnseen(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.BloodAndBoneKnight, (uint)AID.BloodAndBoneSoldier, (uint)AID.BloodAndBoneWarrior, (uint)AID.QueensShotUnseen]);
sealed class RapidSeverShotInTheDark(BossModule module) : Components.SingleTargetCasts(module, [(uint)AID.RapidSeverKnight, (uint)AID.RapidSeverSoldier, (uint)AID.RapidSeverWarrior, (uint)AID.ShotInTheDark]);
sealed class Enrages(BossModule module) : Components.CastHints(module, [(uint)AID.BloodAndBoneKnightEnrage, (uint)AID.BloodAndBoneSoldierEnrage, (uint)AID.BloodAndBoneWarriorEnrage, (uint)AID.QueensShotEnrage], "Enrage!", true);

sealed class PawnOff(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PawnOffReal, 20f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", PrimaryActorOID = (uint)OID.QueensKnight, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 760, NameID = 9838)]
public sealed class DRN3QueensGuard : QueensGuard
{
    private Actor? _warrior;
    private Actor? _soldier;
    private Actor? _gunner;
    private bool updated;

    public DRN3QueensGuard(WorldState ws, Actor primary) : base(ws, primary)
    {
        _warrior = Enemies((uint)OID.QueensWarrior) is var warrior && warrior.Count != 0 ? warrior[0] : null;
        _soldier = Enemies((uint)OID.QueensSoldier) is var soldier && soldier.Count != 0 ? soldier[0] : null;
        _gunner = Enemies((uint)OID.QueensGunner) is var gunner && gunner.Count != 0 ? gunner[0] : null;
    }

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame

        // incase actor spawn was delayed, we need to update the fields...
        if (updated)
            return;
        if (_warrior == null)
        {
            var b = Enemies((uint)OID.QueensWarrior);
            _warrior = b.Count != 0 ? b[0] : null;
        }
        if (_soldier == null)
        {
            var b = Enemies((uint)OID.QueensSoldier);
            _soldier = b.Count != 0 ? b[0] : null;
        }
        if (_gunner == null)
        {
            var b = Enemies((uint)OID.QueensGunner);
            _gunner = b.Count != 0 ? b[0] : null;
        }
        updated = true;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_warrior);
        Arena.Actor(_soldier);
        Arena.Actor(_gunner);
    }
}
