namespace BossMod.Heavensward.Dungeon.D04TheVault.D042SerGrinnaux;

public enum OID : uint
{
    Boss = 0x1054, // R2.2
    SerGrinnauxTheBull = 0x1053, // R0.5
    StellarImplodeArea = 0x1E91C1, // R0.5
    AetherialTear = 0x1055, // R2.0
    Helper = 0xD25
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Overpower = 2188, // SerGrinnauxTheBull->self, no cast, range 6+R 90-degree cone
    Rive = 1135, // SerGrinnauxTheBull->self, 2.5s cast, range 30+R width 2 rect

    AdventVisual1 = 4979,  // SerGrinnauxTheBull->self, no cast, single-target
    AdventVisual2 = 4122, // SerGrinnauxTheBull->self, no cast, single-target
    AdventVisual3 = 4123, // Boss->self, no cast, single-target
    Advent = 4980, // Helper->self, no cast, range 80 circle, knockback 18, away from source
    Retreat = 4257, // SerGrinnauxTheBull->self, no cast, single-target

    DimensionalCollapseVisual = 4136, // Boss->self, 2.5s cast, single-target
    DimensionalCollapse1 = 4137, // Helper->self, 3.0s cast, range 7+R 180-degree cone inner
    DimensionalCollapse2 = 4138, // Helper->self, 3.0s cast, range 12+R 180-degree cone middle
    DimensionalCollapse3 = 4139, // Helper->self, 3.0s cast, range 17+R 180-degree cone outer

    DimensionalRip = 4140, // Boss->location, 3.0s cast, range 5 circle
    StellarImplosion = 4141, // Helper->location, no cast, range 5 circle
    DimensionalTorsion = 4142, // AetherialTear->player, no cast, single-target

    FaithUnmoving = 4135, // Boss->self, 3.0s cast, range 80+R circle
    HeavySwing = 4133, // Boss->player, no cast, range 8+R 90-degree cone
    HyperdimensionalSlash = 4134, // Boss->self, 3.0s cast, range 45+R width 8 rect

    BossPhase1Vanish = 4124, // Boss->self, no cast, single-target
    BossPhase2Vanish = 4256 // SerGrinnauxTheBull->self, no cast, single-target
}

class HeavySwing(BossModule module) : Components.Cleave(module, (uint)AID.HeavySwing, new AOEShapeCone(6.5f, 45f.Degrees()), [(uint)OID.SerGrinnauxTheBull]);
class Overpower(BossModule module) : Components.Cleave(module, (uint)AID.Overpower, new AOEShapeCone(10.2f, 45f.Degrees()));
class DimensionalRip(BossModule module) : Components.VoidzoneAtCastTarget(module, 5f, (uint)AID.DimensionalRip, GetVoidzones, 1.1f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.StellarImplodeArea);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

class FaithUnmoving(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.FaithUnmoving, 20f, stopAtWall: true)
{
    private readonly AetherialTear _aoe1 = module.FindComponent<AetherialTear>()!;
    private readonly DimensionalRip _aoe2 = module.FindComponent<DimensionalRip>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes1 = _aoe1.ActiveAOEs(slot, actor);
        var len1 = aoes1.Length;
        for (var i = 0; i < len1; ++i)
        {
            if (aoes1[i].Check(pos))
                return true;
        }
        var aoes2 = _aoe2.ActiveAOEs(slot, actor);
        var len2 = aoes2.Length;
        for (var i = 0; i < len2; ++i)
        {
            if (aoes2[i].Check(pos))
                return true;
        }
        return false;
    }
}

class Rive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Rive, new AOEShapeRect(30.5f, 1f));
class HyperdimensionalSlash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HyperdimensionalSlash, new AOEShapeRect(47.2f, 4f));
class DimensionalCollapse1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DimensionalCollapse1, new AOEShapeDonutSector(2.5f, 7.5f, 90f.Degrees()));
class DimensionalCollapse2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DimensionalCollapse2, new AOEShapeDonutSector(7.5f, 12.5f, 90f.Degrees()));
class DimensionalCollapse3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DimensionalCollapse3, new AOEShapeDonutSector(12.5f, 17.5f, 90f.Degrees()));
class AetherialTear(BossModule module) : Components.Voidzone(module, 7f, GetTears)
{
    private static List<Actor> GetTears(BossModule module) => module.Enemies((uint)OID.AetherialTear);
}

class D042SerGrinnauxStates : StateMachineBuilder
{
    public D042SerGrinnauxStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Overpower>()
            .ActivateOnEnter<HeavySwing>()
            .ActivateOnEnter<DimensionalRip>()
            .ActivateOnEnter<AetherialTear>()
            .ActivateOnEnter<FaithUnmoving>()
            .ActivateOnEnter<Rive>()
            .ActivateOnEnter<DimensionalCollapse1>()
            .ActivateOnEnter<DimensionalCollapse2>()
            .ActivateOnEnter<DimensionalCollapse3>()
            .ActivateOnEnter<HyperdimensionalSlash>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS), Xyzzy", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 34, NameID = 3639)]
public class D042SerGrinnaux(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    protected override bool CheckPull() => PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.SerGrinnauxTheBull).Any(e => e.InCombat);

    public static readonly ArenaBoundsComplex arena = new([new Circle(new(0, 72), 19.7f)], [new Rectangle(new(19.5f, 72), 1.75f, 7.75f),
    new Rectangle(new(0, 51), 7.75f, 2), new Rectangle(new(-20.8f, 72), 1.75f, 5f)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.SerGrinnauxTheBull));
    }
}
