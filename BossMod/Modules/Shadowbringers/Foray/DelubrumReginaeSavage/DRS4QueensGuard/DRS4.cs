namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4QueensGuard;

sealed class OptimalPlaySword(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OptimalPlaySword, 10f);
sealed class OptimalPlayShield(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OptimalPlayShield, new AOEShapeDonut(5f, 60f));
sealed class OptimalPlayCone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OptimalPlayCone, new AOEShapeCone(60f, 135f.Degrees()));

// note: apparently there is no 'front unseen' status
sealed class QueensShotUnseen(BossModule module) : Components.CastWeakpoint(module, (uint)AID.QueensShotUnseen, 60f, default, (uint)SID.BackUnseen, (uint)SID.LeftUnseen, (uint)SID.RightUnseen);
sealed class TurretsTourUnseen(BossModule module) : Components.CastWeakpoint(module, (uint)AID.TurretsTourUnseen, new AOEShapeRect(50f, 2.5f), default, (uint)SID.BackUnseen, (uint)SID.LeftUnseen, (uint)SID.RightUnseen);

sealed class FieryPortent(BossModule module) : Components.CastHint(module, (uint)AID.FieryPortent, "Stand still!");
sealed class IcyPortent(BossModule module) : Components.CastHint(module, (uint)AID.IcyPortent, "Move!");
sealed class PawnOff(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PawnOffReal, 20);
sealed class Fracture(BossModule module) : Components.CastCounter(module, (uint)AID.Fracture); // TODO: consider showing reflect hints

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.Knight, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 761, NameID = 9838, PlanLevel = 80)]
public sealed class DRS4QueensGuard : QueensGuard
{
    private readonly List<Actor> _warrior;
    private readonly List<Actor> _soldier;
    private readonly List<Actor> _gunner;

    public Actor? Knight() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
    public Actor? Warrior() => _warrior.Count > 0 ? _warrior[0] : null;
    public Actor? Soldier() => _soldier.Count > 0 ? _soldier[0] : null;
    public Actor? Gunner() => _gunner.Count > 0 ? _gunner[0] : null;
    public readonly List<Actor> GunTurrets;
    public readonly List<Actor> AuraSpheres;
    public readonly List<Actor> SpiritualSpheres;

    public DRS4QueensGuard(WorldState ws, Actor primary) : base(ws, primary)
    {
        _warrior = Enemies((uint)OID.Warrior);
        _soldier = Enemies((uint)OID.Soldier);
        _gunner = Enemies((uint)OID.Gunner);
        GunTurrets = Enemies((uint)OID.GunTurret);
        AuraSpheres = Enemies((uint)OID.AuraSphere);
        SpiritualSpheres = Enemies((uint)OID.SpiritualSphere);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(Knight());
        Arena.Actor(Warrior());
        Arena.Actor(Soldier());
        Arena.Actor(Gunner());
        Arena.Actors(GunTurrets);
        Arena.Actors(AuraSpheres);
        Arena.Actors(SpiritualSpheres);
    }
}
