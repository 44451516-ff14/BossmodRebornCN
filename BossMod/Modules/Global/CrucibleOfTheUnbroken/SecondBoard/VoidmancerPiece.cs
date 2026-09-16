namespace BossMod.Global.CrucibleOfTheUnbroken.SecondBoard.VoidmancerPiece;

public enum OID : uint
{
    VoidmancerPiece = 0x4C5C,
    ZombiePiece = 0x4C5D, // R0.750, x20
    Malady = 0x4C5E, // R1.000, x0 (spawn during fight)
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttackWater = 50793, // VoidmancerPiece->player, no cast, single-target
    AutoAttackZombie = 48251, // 4C5D->player, no cast, single-target
    DeathDriveBait = 48181, // VoidmancerPiece->self, 7.0s cast, single-target
    DeathDrive = 48182, // Helper->location, 4.0s cast, range 10 circle
    DarkOrbBoss = 48185, // VoidmancerPiece->self, 6.5+1.5s cast, single-target
    DarkOrb = 48186, // Helper->self, 8.0s cast, range 18 circle
    EvilMist = 48183, // VoidmancerPiece->self, 4.0s cast, range 60 circle
    Necropurge = 48184, // 4C5E->self, 1.0s cast, range 8 circle
    Mindjack = 48187, // VoidmancerPiece->self, 4.0s cast, range 100 circle
}

public enum SID : uint
{
    WitsEnd = 5424, // 4C5D/4C5E->player, extra=0x1/0x2/0x4/0x5/0x6/0x7/0x8 - reaching 12 stacks will cause confuse on the player
    LeftFace = 2163, // VoidmancerPiece->player, extra=0x0
    RightFace = 2164, // VoidmancerPiece->player, extra=0x0
    ForcedMarch = 1257, // VoidmancerPiece->player, extra=0x4
}

public enum IconID : uint
{
    DeathDrive = 707, // player->self
}

public enum TetherID : uint
{
    ZombieTether = 17, // 4C5D->player
}

sealed class DeathDrive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeathDrive, 10.0f);
sealed class DarkOrb(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkOrb, 18.0f);
sealed class Malady(BossModule module) : Components.Voidzone(module, 8.0f, module => module.Enemies((uint)OID.Malady).Where(z => z.EventState != 7));

sealed class DeathDriveBait(BossModule module) : Components.BaitAwayIcon(module, 10.0f, (uint)IconID.DeathDrive, (uint)AID.DeathDriveBait, 7.5f)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (!IsBaitTarget(pc) || CurrentBaits.Count == 0)
        {
            return;
        }

        var bait = CurrentBaits[0];
        var baitRadius = ((AOEShapeCircle)bait.Shape).Radius;

        foreach (var zombie in Module.Enemies((uint)OID.ZombiePiece))
        {
            var reach = baitRadius + zombie.HitboxRadius;

            var onHitbox = (zombie.Position - bait.Target.Position).LengthSq() <= reach * reach;
            Arena.ZoneCircleOutline(zombie.Position, zombie.HitboxRadius, onHitbox ? Colors.Danger : Colors.Border);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!IsBaitTarget(actor) || CurrentBaits.Count == 0)
        {
            return;
        }

        hints.Add("Avoid intersecting zombies hitboxes!");
    }
}

sealed class Mindjack(BossModule module) : Components.StatusDrivenForcedMarch(module, 3.0f, default, default, (uint)SID.LeftFace, (uint)SID.RightFace);

sealed class VoidmancerPieceStates : StateMachineBuilder
{
    public VoidmancerPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DeathDriveBait>()
            .ActivateOnEnter<DeathDrive>()
            .ActivateOnEnter<DarkOrb>()
            .ActivateOnEnter<Malady>()
            .ActivateOnEnter<Mindjack>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, PrimaryActorOID = (uint)OID.VoidmancerPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1089u, NameID = 14552u, SortOrder = 3)]
public sealed class VoidmancerPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, 0f), new ArenaBoundsRect(20f, 20f))
{

    public override bool ShouldPrioritizeAllEnemies => true;

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.ZombiePiece));
    }
}
