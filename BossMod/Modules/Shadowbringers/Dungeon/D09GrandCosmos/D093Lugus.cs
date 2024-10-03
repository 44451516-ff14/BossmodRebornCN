namespace BossMod.Shadowbringers.Dungeon.D09GrandCosmos.D093Lugus;

public enum OID : uint
{
    Boss = 0x2C13, // R4.0
    CrystalChandelier = 0x2C12, // R5.0
    VelvetDrapery = 0x2C0E, // R3.0
    GrandPiano = 0x2C0F, // R4.2
    PlushSofa = 0x2C11, // R3.0
    GildedStool = 0x2C10, // R1.0
    // furniture helpers used to check AOE clipping with furniture since it otherwise would only burn if the center is in shape
    FurnitureHelper1 = 0x2C16, // R1.0 (plush sofa, velvet drapery)
    FurnitureHelper2 = 0x2C18, // R0.8 (gilded stool)
    FurnitureHelper3 = 0x2C19, // R3.0 (chandelier)
    FurnitureHelper4 = 0x2C14, // R2.5 (velvet drapery)
    FurnitureHelper5 = 0x2C17, // R1.5 (grand piano)
    FurnitureHelper6 = 0x2C15, // R0.5 (plush sofa)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    ScorchingLeft = 18275, // Boss->self, 5.0s cast, range 40 180-degree cone
    ScorchingRight = 18274, // Boss->self, 5.0s cast, range 40 180-degree cone
    BlackFlame = 18269, // Helper->players, no cast, range 6 circle
    OtherworldlyHeatVisual = 18267, // Boss->self, 5.0s cast, single-target
    OtherworldlyHeat = 18268, // Helper->self, 2.5s cast, range 10 width 4 cross
    CaptiveBolt = 18276, // Boss->player, 5.0s cast, single-target, tankbuster
    MortalFlameVisual = 18265, // Boss->self, 5.0s cast, single-target
    MortalFlame = 18266, // Helper->player, 5.5s cast, single-target
    FiresDomainVisual = 18270, // Boss->self, 8.0s cast, single-target
    FiresDomain1 = 18272, // Boss->player, no cast, width 4 rect charge
    FiresDomain2 = 18271, // Boss->player, no cast, width 4 rect charge
    FiresIre = 18273, // Boss->self, 2.0s cast, range 20 90-degree cone
    CullingBlade = 18277, // Boss->self, 6.0s cast, range 80 circle
    CullingBladeVisual = 18278, // Helper->self, no cast, range 80 circle
    Plummet = 18279 // Helper->self, 1.6s cast, range 3 circle
}

public enum IconID : uint
{
    BlackFlame = 25, // player
    MortalFlame = 195, // player
    Target1 = 50, // player
    Target2 = 51, // player
    Target3 = 52, // player
    Target4 = 53, // player
    Tankbuster = 218 // player
}

public enum SID : uint
{
    MortalFlame = 2136 // Helper->player, extra=0x0
}

class MortalFlame(BossModule module) : BossComponent(module)
{
    public static bool IsBurning(Actor actor) => actor.FindStatus(SID.MortalFlame) != null;
    private static readonly HashSet<OID> furniture = [OID.FurnitureHelper1, OID.FurnitureHelper2, OID.FurnitureHelper4, OID.FurnitureHelper5, OID.FurnitureHelper6]; // without chandeliers since they get enabled later, but exist invisible to the player
    private HashSet<Actor> _furniture = [];
    private bool updated;

    public override void OnActorDestroyed(Actor actor)
    {
        _furniture.Remove(actor);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (furniture.Contains((OID)actor.OID))
            _furniture.Add(actor);
    }

    public override void Update()
    {
        if (!updated && Module.Enemies(OID.CrystalChandelier).Any(x => x.IsTargetable))
        {
            var list = _furniture.Concat(Module.Enemies(OID.FurnitureHelper3)).ToHashSet();
            _furniture = list;
            updated = true;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!IsBurning(actor))
            return;
        var forbidden = new List<Func<WPos, float>>();
        foreach (var h in _furniture)
            forbidden.Add(ShapeDistance.InvertedCircle(h.Position, h.HitboxRadius));
        if (forbidden.Count > 0)
            hints.AddForbiddenZone(p => forbidden.Select(f => f(p)).Max());
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (IsBurning(actor))
            hints.Add("Pass flames debuff to furniture!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (IsBurning(pc))
            foreach (var a in _furniture)
                Arena.AddCircle(a.Position, a.HitboxRadius, Colors.Safe);
    }
}

class BlackFlame(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeCircle circle = new(6);
    private static readonly AOEShapeCross cross = new(10, 2);
    private static readonly HashSet<OID> furniture = [OID.FurnitureHelper1, OID.FurnitureHelper2, OID.FurnitureHelper4, OID.FurnitureHelper5, OID.FurnitureHelper6]; // without chandeliers since they get enabled later, but exist invisible to the player
    private HashSet<Actor> _furniture = [];
    private bool updated;

    public override void OnActorDestroyed(Actor actor)
    {
        _furniture.Remove(actor);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (furniture.Contains((OID)actor.OID))
            _furniture.Add(actor);
    }

    public override void Update()
    {
        if (!updated && Module.Enemies(OID.CrystalChandelier).Any(x => x.IsTargetable))
        {
            var list = _furniture.Concat(Module.Enemies(OID.FurnitureHelper3)).ToHashSet();
            _furniture = list;
            updated = true;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.BlackFlame)
        {
            var activation = WorldState.FutureTime(4);
            CurrentBaits.Add(new(actor, actor, circle, activation));
            CurrentBaits.Add(new(actor, actor, cross, activation, -0.003f.Degrees()));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BlackFlame)
            CurrentBaits.RemoveAll(x => x.Target == WorldState.Actors.Find(spell.MainTargetID));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (CurrentBaits.Any(x => x.Target == actor))
        {
            var b = ActiveBaitsOn(actor).FirstOrDefault(x => x.Shape == cross);
            foreach (var p in _furniture)
            {
                // AOE and hitboxes seem to be forbidden to intersect
                hints.AddForbiddenZone(ShapeDistance.Cross(p.Position, b.Rotation, cross.Length + p.HitboxRadius, cross.HalfWidth + p.HitboxRadius), b.Activation);
                hints.AddForbiddenZone(ShapeDistance.Circle(p.Position, circle.Radius + p.HitboxRadius), b.Activation);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (!ActiveBaits.Any(x => x.Target == pc))
            return;
        foreach (var a in _furniture)
            Arena.AddCircle(a.Position, a.HitboxRadius, Colors.Danger);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Any(x => x.Target == actor))
            hints.Add("Bait away, avoid intersecting furniture hitboxes!");
    }
}

class OtherworldlyHeat(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.OtherworldlyHeat), new AOEShapeCross(10, 2));

abstract class Scorching(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(40, 90.Degrees()));
class ScorchingLeft(BossModule module) : Scorching(module, AID.ScorchingLeft);
class ScorchingRight(BossModule module) : Scorching(module, AID.ScorchingRight);

class CullingBlade(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.CullingBlade));
class CaptiveBolt(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.CaptiveBolt));
class FiresIre(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FiresIre), new AOEShapeCone(20, 45.Degrees()));
class Plummet(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Plummet), 3);
class FiresDomainTether(BossModule module) : Components.StretchTetherDuo(module, default, default)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Any(x => x.Target == actor))
            hints.AddForbiddenZone(ShapeDistance.Rect(Arena.Center + new WDir(0, 24), Arena.Center + new WDir(0, -24), 24));
    }
}

class FiresDomain(BossModule module) : Components.GenericBaitAway(module)
{
    public override void Update()
    {
        foreach (ref var b in CurrentBaits.AsSpan())
        {
            if (b.Shape is AOEShapeRect shape)
            {
                var len = (b.Target.Position - b.Source.Position).Length();
                if (shape.LengthFront != len)
                    b.Shape = shape with { LengthFront = len };
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (CurrentBaits.Count > 0 && (AID)spell.Action.ID is AID.FiresDomain1 or AID.FiresDomain2)
            CurrentBaits.RemoveAt(0);
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if ((IconID)iconID is >= IconID.Target1 and <= IconID.Target4)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, new AOEShapeRect(0, 2)));
    }
}

class FiresIreBait(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeCone cone = new(20, 45.Degrees());
    private static readonly HashSet<OID> furniture = [OID.FurnitureHelper1, OID.FurnitureHelper2, OID.FurnitureHelper4, OID.FurnitureHelper5, OID.FurnitureHelper6]; // without chandeliers since they get enabled later, but exist invisible to the player
    private HashSet<Actor> _furniture = [];
    private bool updated;

    public override void OnActorDestroyed(Actor actor)
    {
        _furniture.Remove(actor);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (furniture.Contains((OID)actor.OID))
            _furniture.Add(actor);
    }

    public override void Update()
    {
        if (!updated && Module.Enemies(OID.CrystalChandelier).Any(x => x.IsTargetable))
        {
            var list = _furniture.Concat(Module.Enemies(OID.FurnitureHelper3)).ToHashSet();
            _furniture = list;
            updated = true;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if ((IconID)iconID is >= IconID.Target1 and <= IconID.Target4)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, cone));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (CurrentBaits.Count > 0 && (AID)spell.Action.ID is AID.FiresDomain1 or AID.FiresDomain2)
            CurrentBaits.RemoveAt(0);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var b in ActiveBaitsNotOn(actor))
            hints.AddForbiddenZone(b.Shape, b.Target.Position - (b.Target.HitboxRadius + Module.PrimaryActor.HitboxRadius) * Module.PrimaryActor.DirectionTo(b.Target), b.Rotation);

        if (CurrentBaits.Any(x => x.Target == actor))
        {
            var b = ActiveBaitsOn(actor).FirstOrDefault();
            var actors = _furniture.Concat(Raid.WithoutSlot().Exclude([actor]).ToHashSet());
            foreach (var a in actors)
                hints.AddForbiddenZone(ShapeDistance.Circle(a.Position, 10), b.Activation);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!ActiveBaits.Any(x => x.Target == pc))
            return;
        foreach (var a in _furniture)
            Arena.AddCircle(a.Position, a.HitboxRadius, Colors.Danger);
        foreach (var b in ActiveBaitsOn(pc))
            b.Shape.Outline(Arena, b.Target.Position - (b.Target.HitboxRadius + Module.PrimaryActor.HitboxRadius) * Module.PrimaryActor.DirectionTo(b.Target), b.Rotation);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var b in ActiveBaitsNotOn(pc))
            b.Shape.Draw(Arena, b.Target.Position - (b.Target.HitboxRadius + Module.PrimaryActor.HitboxRadius) * Module.PrimaryActor.DirectionTo(b.Target), b.Rotation);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Any(x => x.Target == actor))
            hints.Add("Bait away, avoid intersecting furniture hitboxes!");
    }
}

class D093LugusStates : StateMachineBuilder
{
    public D093LugusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FiresDomainTether>()
            .ActivateOnEnter<FiresDomain>()
            .ActivateOnEnter<FiresIre>()
            .ActivateOnEnter<FiresIreBait>()
            .ActivateOnEnter<ScorchingLeft>()
            .ActivateOnEnter<ScorchingRight>()
            .ActivateOnEnter<OtherworldlyHeat>()
            .ActivateOnEnter<BlackFlame>()
            .ActivateOnEnter<CaptiveBolt>()
            .ActivateOnEnter<CullingBlade>()
            .ActivateOnEnter<Plummet>()
            .ActivateOnEnter<MortalFlame>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 692, NameID = 9046)]
public class D093Lugus(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -340), new ArenaBoundsSquare(24.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.FurnitureHelper1).Concat(Enemies(OID.FurnitureHelper2)).Concat(Enemies(OID.FurnitureHelper4)).Concat(Enemies(OID.FurnitureHelper5)).Concat(Enemies(OID.FurnitureHelper6)), Colors.Object, true);
        if (Enemies(OID.CrystalChandelier).Any(x => x.IsTargetable))
            Arena.Actors(Enemies(OID.FurnitureHelper3), Colors.Object, true);
    }
}
