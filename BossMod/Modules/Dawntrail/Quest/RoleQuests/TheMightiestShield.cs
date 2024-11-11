namespace BossMod.Dawntrail.Quest.RoleQuests.TheMightiestShield;

public enum OID : uint
{
    Boss = 0x43C7, // R5.4
    CrackedMettle1 = 0x43E1, // R2.0
    CrackedMettle2 = 0x43E4, // R4.86
    UnyieldingMettle1 = 0x43E2, // R1.0
    UnyieldingMettle2 = 0x43E0, // R2.0
    UnyieldingMettle3 = 0x43E3, // R4.86
    CravenFollower1 = 0x43CF, // R2.0
    CravenFollower2 = 0x43CB, // R2.0
    CravenFollower3 = 0x43C9, // R2.0
    CravenFollower4 = 0x43CD, // R2.0
    CravenFollower5 = 0x43D0, // R0.5
    CravenFollower6 = 0x43CE, // R0.5
    CravenFollower7 = 0x43CC, // R0.5
    CravenFollower8 = 0x43CA, // R0.5
    MagitekMissile = 0x43C8, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 39025, // Boss->Kaqool, no cast, single-target

    PhotonStream = 37001, // CravenFollower3/CravenFollower1/CravenFollower2/CravenFollower4->Kaqool, no cast, single-target
    HomingLaserMarker = 38622, // CravenFollower1/CravenFollower4/CravenFollower2->player/Kaqool, 5.0s cast, single-target
    HomingLaser = 38624, // CravenFollower1/CravenFollower4/CravenFollower2->player/Kaqool, no cast, range 42 width 8 rect
    MissileStormVisual = 38601, // Boss->self, no cast, single-target
    MissileStorm = 38602, // Helper->self, no cast, range 60 circle
    AreaBombardmentVisual = 38599, // Boss->self, 7.0s cast, single-target
    AreaBombardment = 38600, // Helper->self, 7.0s cast, range 4 circle
    MagitekCannon = 39006, // CravenFollower3->player, 5.0s cast, range 6 circle, spread

    GuidedMissileVisual1 = 38603, // Boss->self, no cast, single-target
    GuidedMissileVisual2 = 35763, // MagitekMissile->self, no cast, single-target
    GuidedMissileVisual3 = 35764, // MagitekMissile->self, no cast, single-target
    Burst = 38598, // MagitekMissile->self, no cast, range 3 circle

    SteelhogsMettle = 38621, // Boss->self, 3.0s cast, single-target
    SteelhogsRevenge = 38625, // UnyieldingMettle1->self, 7.0s cast, range 12 circle

    HeavySurfaceMissilesVisual = 38606, // Boss->self, 3.0s cast, single-target, damage fall off aoes
    HeavySurfaceMissiles1 = 38607, // Helper->self, 7.0s cast, range 60 circle
    HeavySurfaceMissiles2 = 38608, // Helper->self, 10.0s cast, range 60 circle
    HeavySurfaceMissiles3 = 38609, // Helper->self, 13.0s cast, range 60 circle
    HeavySurfaceMissiles4 = 38610, // Helper->self, 16.0s cast, range 60 circle

    GunsBlazing1 = 38614, // Boss->self, 5.0s cast, single-target
    GunsBlazing2 = 38616, // Boss->self, 5.0s cast, single-target
    NeedleGun1 = 38618, // Helper->self, 5.0s cast, range 40 90-degree cone
    NeedleGun2 = 38712, // Helper->self, 7.0s cast, range 40 90-degree cone
    OilShower1 = 38619, // Helper->self, 7.0s cast, range 40 270-degree cone
    OilShower2 = 38711, // Helper->self, 5.0s cast, range 40 270-degree cone

    RuthlessBombardmentVisual = 38611, // Boss->self, 7.0s cast, single-target
    RuthlessBombardment1 = 38613, // Helper->self, 7.0s cast, range 8 circle
    RuthlessBombardment2 = 38612, // Helper->self, 7.0s cast, range 4 circle

    Visual1 = 38615, // Boss->self, no cast, single-target
    Visual2 = 38617, // Boss->self, no cast, single-target

    SurfaceMissile1 = 38604, // Boss->self, no cast, single-target
    SurfaceMissile2 = 38605, // Helper->location, 5.0s cast, range 6 circle
    UnbreakableCermetDrill = 38620, // Boss->Kaqool, no cast, single-target

    RagingArtilleryVisual1 = 38741, // Boss->self, 5.0s cast, single-target
    RagingArtilleryVisual2 = 38743, // Boss->self, no cast, single-target
    RagingArtilleryFirst = 38742, // Helper->self, 5.0s cast, range 60 circle, multiple raidwides
    RagingArtilleryRest = 38744 // Helper->self, no cast, range 60 circle
}

public enum SID : uint
{
    RightwardFracture = 4036, // none->CrackedMettle1/CrackedMettle2, extra=0x0
    LeftwardFracture = 4037, // none->CrackedMettle1, extra=0x0
    BackwardFracture = 4039 // none->CrackedMettle1, extra=0x0
}

class Fractures(BossModule module) : Components.DirectionalParry(module, [(uint)OID.CrackedMettle1, (uint)OID.CrackedMettle2])
{
    private const int sideR = (int)(Side.Front | Side.Back | Side.Left) << 4;
    private const int sideL = (int)(Side.Right | Side.Back | Side.Front) << 4;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((OID)actor.OID is not OID.CrackedMettle1 and not OID.CrackedMettle2)
            return;
        var sides = (SID)status.ID switch
        {
            SID.RightwardFracture => Side.Front | Side.Back | Side.Left,
            SID.LeftwardFracture => Side.Right | Side.Back | Side.Front,
            SID.BackwardFracture => Side.Front | Side.Left | Side.Right,
            _ => Side.None
        };
        if (sides != Side.None)
            PredictParrySide(actor.InstanceID, sides);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.RightwardFracture or SID.LeftwardFracture or SID.BackwardFracture)
            UpdateState(actor.InstanceID, 0);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActorStates.Count > 0)
        {
            var first = ActorStates.FirstOrDefault();
            var target = WorldState.Actors.Find(first.Key)!;
            var dir = first.Value == sideR ? target.Rotation - 90.Degrees() : first.Value == sideL ? target.Rotation + 90.Degrees() : target.Rotation + 180.Degrees();
            hints.AddForbiddenZone(ShapeDistance.InvertedDonutSector(target.Position, target.HitboxRadius, 20, dir, 20.Degrees()));
        }
    }
}

class SteelhogsRevenge(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SteelhogsRevenge), new AOEShapeCircle(12));
class RuthlessBombardment1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RuthlessBombardment1), new AOEShapeCircle(8));

abstract class Bombardment(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCircle(4));
class RuthlessBombardment2(BossModule module) : Bombardment(module, AID.RuthlessBombardment2);
class AreaBombardment(BossModule module) : Bombardment(module, AID.AreaBombardment);

class RagingArtillery(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.RagingArtilleryFirst));
class MagitekCannon(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.MagitekCannon), 6);
class MagitekMissile(BossModule module) : Components.PersistentVoidzone(module, 3, m => m.Enemies(OID.MagitekMissile).Where(x => !x.IsDead), 5);

class NeedleGunOilShower(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCone cone1 = new(40, 135.Degrees());
    private static readonly AOEShapeCone cone2 = new(40, 45.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count > 0)
            yield return _aoes[0] with { Color = Colors.Danger };
        if (count > 1)
            yield return _aoes[1] with { Risky = false };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.OilShower1 or AID.OilShower2)
            _aoes.Add(new(cone1, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
        else if ((AID)spell.Action.ID is AID.NeedleGun1 or AID.NeedleGun2)
            _aoes.Add(new(cone2, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
        if (_aoes.Count > 0)
            _aoes.SortBy(x => x.Activation);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.OilShower1 or AID.OilShower2 or AID.NeedleGun1 or AID.NeedleGun2)
            _aoes.RemoveAt(0);
    }
}

class HeavySurfaceMissiles(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly HashSet<AID> casts = [AID.HeavySurfaceMissiles1, AID.HeavySurfaceMissiles2, AID.HeavySurfaceMissiles3, AID.HeavySurfaceMissiles4];
    private static readonly AOEShapeCircle circle = new(14);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(2);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (casts.Contains((AID)spell.Action.ID))
            _aoes.Add(new(circle, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
        if (_aoes.Count > 0)
            _aoes.SortBy(x => x.Activation);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (casts.Contains((AID)spell.Action.ID))
            _aoes.RemoveAt(0);
    }
}

class HomingLaser(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeRect rect = new(42, 4);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HomingLaserMarker)
            CurrentBaits.Add(new(caster, WorldState.Actors.Find(spell.TargetID)!, rect, WorldState.FutureTime(5.7f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.HomingLaser)
            CurrentBaits.Clear();
    }
}

class TheMightiestShieldStates : StateMachineBuilder
{
    public TheMightiestShieldStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Fractures>()
            .ActivateOnEnter<RuthlessBombardment1>()
            .ActivateOnEnter<RuthlessBombardment2>()
            .ActivateOnEnter<AreaBombardment>()
            .ActivateOnEnter<RagingArtillery>()
            .ActivateOnEnter<MagitekCannon>()
            .ActivateOnEnter<NeedleGunOilShower>()
            .ActivateOnEnter<HeavySurfaceMissiles>()
            .ActivateOnEnter<MagitekMissile>()
            .ActivateOnEnter<SteelhogsRevenge>()
            .ActivateOnEnter<HomingLaser>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70377, NameID = 12923)]
public class TheMightiestShield(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(-191, 72), 14.5f, 20)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.UnyieldingMettle1).Concat([PrimaryActor]).Concat(Enemies(OID.UnyieldingMettle2)).Concat(Enemies(OID.UnyieldingMettle3))
        .Concat(Enemies(OID.CrackedMettle1)).Concat(Enemies(OID.CrackedMettle2)).Concat(Enemies(OID.CravenFollower1)).Concat(Enemies(OID.CravenFollower2))
        .Concat(Enemies(OID.CravenFollower3)).Concat(Enemies(OID.CravenFollower4)).Concat(Enemies(OID.CravenFollower5)).Concat(Enemies(OID.CravenFollower6)
        .Concat(Enemies(OID.CravenFollower7))).Concat(Enemies(OID.CravenFollower8)).Concat(Enemies(OID.MagitekMissile)));
    }

    protected override bool CheckPull() => Raid.WithoutSlot().Any(x => x.InCombat);
}
