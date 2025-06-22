namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS8Queen;

sealed class NorthswainsGlow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NorthswainsGlowAOE, 20f);
sealed class CleansingSlashSecond(BossModule module) : Components.CastCounter(module, (uint)AID.CleansingSlashSecond);
sealed class GodsSaveTheQueen(BossModule module) : Components.CastCounter(module, (uint)AID.GodsSaveTheQueenAOE);
sealed class JudgmentBlade(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.JudgmentBladeLAOE, (uint)AID.JudgmentBladeRAOE], new AOEShapeRect(70f, 15f));
// note: apparently there is no 'front unseen' status
sealed class QueensShot(BossModule module) : Components.CastWeakpoint(module, (uint)AID.QueensShot, 60f, default, (uint)SID.BackUnseen, (uint)SID.LeftUnseen, (uint)SID.RightUnseen);
sealed class TurretsTourUnseen(BossModule module) : Components.CastWeakpoint(module, (uint)AID.TurretsTourUnseen, new AOEShapeRect(50f, 2.5f), default, (uint)SID.BackUnseen, (uint)SID.LeftUnseen, (uint)SID.RightUnseen);

sealed class OptimalOffensive(BossModule module) : Components.ChargeAOEs(module, (uint)AID.OptimalOffensive, 2.5f);

// note: there are two casters (as usual in bozja content for raidwides)
sealed class OptimalOffensiveKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.OptimalOffensiveKnockback, 10f, true, 1);

sealed class OptimalPlaySword(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OptimalPlaySword, 10f);
sealed class OptimalPlayShield(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OptimalPlayShield, new AOEShapeDonut(5f, 60f));
sealed class OptimalPlayCone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OptimalPlayCone, new AOEShapeCone(60f, 135f.Degrees()));
sealed class PawnOff(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PawnOffReal, 20f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 761, NameID = 9863, PlanLevel = 80)]
public sealed class DRS8Queen(WorldState ws, Actor primary) : Queen(ws, primary)
{
    private Actor? _warrior;
    private Actor? _soldier;
    private Actor? _gunner;
    private Actor? _knight;
    public Actor? Knight() => _knight;
    public Actor? Soldier() => _soldier;
    public Actor? Gunner() => _gunner;
    public Actor? Warrior() => _warrior;
    private bool updated;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
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
        if (_knight == null)
        {
            var b = Enemies((uint)OID.QueensKnight);
            _knight = b.Count != 0 ? b[0] : null;
        }
        updated = true;
    }
}
