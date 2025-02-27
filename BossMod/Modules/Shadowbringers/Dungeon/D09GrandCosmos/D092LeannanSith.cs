namespace BossMod.Shadowbringers.Dungeon.D09GrandCosmos.D092LeananSith;

public enum OID : uint
{
    Boss = 0x2C04, // R2.4
    EnslavedLove = 0x2C06, // R3.6
    LoversRing = 0x2C05, // R2.04
    LeannanSeed1 = 0x1EAE9E, // R0.5
    LeannanSeed2 = 0x1EAE9F, // R0.5
    LeannanSeed3 = 0x1EAEA0, // R0.5
    LeannanSeed4 = 0x1EAEA1, // R0.5
    DirtTiles = 0x1EAEC6, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // LoversRing->player, no cast, single-target
    Teleport = 18207, // Boss->location, no cast, ???

    StormOfColor = 18203, // Boss->player, 4.0s cast, single-target
    OdeToLostLove = 18204, // Boss->self, 4.0s cast, range 60 circle
    DirectSeeding = 18205, // Boss->self, 4.0s cast, single-target
    GardenersHymn = 18206, // Boss->self, 14.0s cast, single-target

    ToxicSpout = 18208, // LoversRing->self, 8.0s cast, range 60 circle
    OdeToFarWinds = 18210, // Boss->self, 3.0s cast, single-target
    FarWind = 18211, // Helper->location, 5.0s cast, range 8 circle
    FarWindSpread = 18212, // Helper->player, 5.0s cast, range 5 circle
    OdeToFallenPetals = 18768, // Boss->self, 4.0s cast, range 5-60 donut
    IrefulWind = 18209 // 2C06->self, 13.0s cast, range 40+R width 40 rect, knockback 10, source forward
}

public enum SID : uint
{
    Transporting = 404 // none->player, extra=0x15
}

class OdeToLostLove(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.OdeToLostLove));
class StormOfColor(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.StormOfColor));
class FarWindSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.FarWindSpread), 5);
class FarWind(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.FarWind), 8);
class OdeToFallenPetals(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.OdeToFallenPetals), new AOEShapeDonut(5, 60));
class IrefulWind(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.IrefulWind), 10, kind: Kind.DirForward, stopAtWall: true);

class GreenTiles(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Dictionary<Actor, DateTime> transportingCheckStartTimes = [];
    private const int HalfSize = 5;
    private static readonly WPos[] defaultGreenTiles =
    [
        new(-5, -75), new(5, -75), new(-15, -65), new(15, -65),
        new(-15, -55), new(5, -55), new(-5, -45), new(15, -45)
    ];
    private Square[] tiles = [];
    public BitMask transporting;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (ShouldActivateAOEs)
            yield return new(new AOEShapeCustom(tiles), Arena.Center, Color: Colors.FutureVulnerable, Risky: transporting[slot] != default);
    }

    private bool ShouldActivateAOEs => NumCasts == 1 ? tiles.Length > 0 : tiles.Length > 8;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Transporting)
            transporting.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Transporting)
            transporting[Raid.FindSlot(actor.InstanceID)] = default;
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index != 0x0B)
            return;

        tiles = state switch
        {
            0x00020001 => GenerateTiles(defaultGreenTiles),
            0x00200010 => GenerateRotatedTiles(180),
            0x01000080 => GenerateRotatedTiles(-90),
            0x08000400 => GenerateRotatedTiles(90),
            _ => [],
        };
    }

    private static Square[] GenerateTiles(WPos[] positions) => [.. positions.Select(pos => new Square(pos, HalfSize))];

    private static Square[] GenerateRotatedTiles(float angle)
        => [.. defaultGreenTiles.Select(pos => new Square(WPos.RotateAroundOrigin(angle, D092LeananSith.ArenaCenter, pos), HalfSize))];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DirectSeeding)
            ++NumCasts;
        else if (spell.Action.ID == (uint)AID.IrefulWind)
        {
            var knockbackDirection = new Angle(MathF.Round(spell.Rotation.Deg / 90f) * 90f) * Angle.DegToRad;
            var offset = 10 * knockbackDirection.ToDirection();
            var tileList = tiles.ToList();
            var newTiles = new List<Square>(10);
            for (var i = 0; i < tileList.Count; ++i)
                tileList[i] = new(tileList[i].Center - offset, tileList[i].HalfSize);
            foreach (var t in tileList.Where(x => (caster.Position - x.Center).LengthSq() > 625f))
                newTiles.Add(new(t.Center + offset, t.HalfSize));
            tileList.AddRange(newTiles);
            tiles = [.. tileList];
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!ShouldActivateAOEs || AI.AIManager.Instance?.Beh == null)
            return;
        base.AddAIHints(slot, actor, assignment, hints);

        var shape = ActiveAOEs(slot, actor).FirstOrDefault();
        var clippedSeeds = GetClippedSeeds(shape);
        var closestSeed = clippedSeeds.Closest(actor.Position);

        if (transporting[slot] != default && closestSeed != null)
            HandleNonTransportingActor(actor, hints, clippedSeeds, closestSeed);
        else if (!shape.Shape.Check(actor.Position, shape.Origin, default))
            HandleTransportingActor(actor, hints);
        else
            transportingCheckStartTimes.Remove(actor);
    }

    private List<Actor> GetClippedSeeds(AOEInstance shape)
        => Module.Enemies(D092LeananSith.Seeds).Where(x => x.IsTargetable).InShape(shape.Shape, shape.Origin, default);

    private void HandleNonTransportingActor(Actor actor, AIHints hints, IEnumerable<Actor> clippedSeeds, Actor closestSeed)
    {
        var forbidden = new List<Func<WPos, float>>(4);
        foreach (var seed in clippedSeeds)
            forbidden.Add(ShapeDistance.InvertedCircle(seed.Position, 3f));
        var distance = (actor.Position - closestSeed.Position).LengthSq();
        if (forbidden.Count > 0 && distance > 9f)
            hints.AddForbiddenZone(ShapeDistance.Intersection(forbidden));
        else if (distance < 9f)
            hints.InteractWithTarget = closestSeed;
    }

    private void HandleTransportingActor(Actor actor, AIHints hints)
    {
        if (!transportingCheckStartTimes.TryGetValue(actor, out var checkStartTime))
            transportingCheckStartTimes[actor] = WorldState.CurrentTime;
        else if (WorldState.CurrentTime >= checkStartTime.AddSeconds(0.25f))  // we need to delay the drop or server lag will cause the seed to drop at an old position
        {
            hints.StatusesToCancel.Add(((uint)SID.Transporting, default));
            transportingCheckStartTimes.Remove(actor);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var trans = transporting[slot] != default;
        if (trans)
        {
            if (ActiveAOEs(slot, actor).Any(c => c.Check(actor.Position)))
                hints.Add("Drop seed outside of vulnerable area!");
            else
                hints.Add("Drop your seed!");
        }
        else if (!trans)
        {
            var aoes = ActiveAOEs(slot, actor).FirstOrDefault();
            if (aoes.Shape != null && GetClippedSeeds(aoes).Count != 0)
                hints.Add("Pick up seeds in vulnerable squares!");
        }
    }
}

class D092LeananSithStates : StateMachineBuilder
{
    public D092LeananSithStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<OdeToLostLove>()
            .ActivateOnEnter<StormOfColor>()
            .ActivateOnEnter<FarWindSpread>()
            .ActivateOnEnter<FarWind>()
            .ActivateOnEnter<OdeToFallenPetals>()
            .ActivateOnEnter<IrefulWind>()
            .ActivateOnEnter<GreenTiles>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 692, NameID = 9044)]
public class D092LeananSith(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsSquare(19.5f))
{
    public static readonly WPos ArenaCenter = new(0, -60);
    public static readonly uint[] Seeds = [(uint)OID.LeannanSeed1, (uint)OID.LeannanSeed2, (uint)OID.LeannanSeed3, (uint)OID.LeannanSeed4];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(Seeds), Colors.Object);
        Arena.Actors(Enemies(OID.LoversRing));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.LoversRing => 1,
                _ => 0
            };
        }
    }
}
