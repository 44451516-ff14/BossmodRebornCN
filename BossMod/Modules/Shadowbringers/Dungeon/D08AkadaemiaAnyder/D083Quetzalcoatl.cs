namespace BossMod.Shadowbringers.Dungeon.D08AkadaemiaAnyder.D083Quetzalcoatl;

public enum OID : uint
{
    Boss = 0x28DA, // R5.4
    CollectableOrb = 0x28DB, // R0.7
    ExpandingOrb = 0x1EAB51, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Shockbolt = 15907, // Boss->player, 3.0s cast, single-target, tankbuster
    Thunderbolt = 15908, // Boss->self, 4.0s cast, range 40 circle, raidwide

    ThunderstormVisual = 15898, // Boss->self, 4.0s cast, single-target
    ThunderstormAOE = 15900, // Helper->location, 4.7s cast, range 5 circle
    ThunderstormSpread = 15899, // Helper->player, 4.7s cast, range 5 circle

    CollectedOrb = 15901, // CollectableOrb->player, no cast, single-target, damage buff to player
    UncollectedOrb = 17170, // CollectableOrb->Boss, no cast, single-target, damage buff to boss

    ShockingPlumageVisual = 15905, // Boss->self, 5.0s cast, single-target
    ShockingPlumage = 15906, // Helper->self, 5.0s cast, range 40 60-degree cone

    ReverseCurrent = 15902, // Boss->self, no cast, range 40 circle, knockback for growing orb mechanic
    ExpandingOrb = 15904, // Helper->self, no cast, range 1 circle, grows 1y per ~1s
    WindingCurrent = 15903 // Boss->self, 15.0s cast, range 5-40 donut
}

class Thunderbolt(BossModule module) : Components.RaidwideCast(module, (uint)AID.Thunderbolt);
class Shockbolt(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Shockbolt);
class ShockingPlumage(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ShockingPlumage, new AOEShapeCone(40f, 30f.Degrees()));
class WindingCurrent(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WindingCurrent, new AOEShapeDonut(5f, 40f));
class ThunderstormAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThunderstormAOE, 5f);
class ThunderstormSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.ThunderstormSpread, 5f);

class OrbCollecting(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _orbs = [];
    private readonly ShockingPlumage _aoe = module.FindComponent<ShockingPlumage>()!;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.CollectableOrb)
            _orbs.Add(actor);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_orbs.Count != 0)
            hints.Add("Soak the orbs!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _orbs.Count;
        if (count == 0)
            return;
        var orbs = new Func<WPos, float>[count];
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var activation = aoes.Length != 0 ? aoes[0].Activation.AddSeconds(1.1f) : WorldState.FutureTime(2d);
        for (var i = 0; i < count; ++i)
        {
            var o = _orbs[i];
            orbs[i] = ShapeDistance.InvertedRect(o.Position + 0.5f * o.Rotation.ToDirection(), new WDir(0f, 1f), 0.5f, 0.5f, 0.5f);
        }
        hints.AddForbiddenZone(ShapeDistance.Intersection(orbs), activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = _orbs.Count;
        for (var i = 0; i < count; ++i)
            Arena.AddCircle(_orbs[i].Position, 0.7f, Colors.Safe);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.CollectedOrb or (uint)AID.UncollectedOrb)
            _orbs.Remove(caster);
    }
}

class ExpandingOrb(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _aoes = [];
    private static readonly AOEShapeCircle circle = new(1.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var aoes = new AOEInstance[count];
        var size = Math.Clamp(circle.Radius + NumCasts / 4, default, 12f);
        for (var i = 0; i < count; ++i)
            aoes[i] = new(circle with { Radius = size }, _aoes[i].Position, default, WorldState.FutureTime(1.1d));
        return aoes;
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.ExpandingOrb)
        {
            if (state == 0x00100020)
                _aoes.Add(actor);
            else if (state == 0x00010002)
                _aoes.Remove(actor);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WindingCurrent)
            NumCasts = 0;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ExpandingOrb)
            ++NumCasts;
    }
}

class D083QuetzalcoatlStates : StateMachineBuilder
{
    public D083QuetzalcoatlStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Thunderbolt>()
            .ActivateOnEnter<Shockbolt>()
            .ActivateOnEnter<ShockingPlumage>()
            .ActivateOnEnter<WindingCurrent>()
            .ActivateOnEnter<ThunderstormAOE>()
            .ActivateOnEnter<ThunderstormSpread>()
            .ActivateOnEnter<OrbCollecting>()
            .ActivateOnEnter<ExpandingOrb>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 661, NameID = 8273)]
public class D083Quetzalcoatl(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(default, -379f), 19.5f * CosPI.Pi48th, 48)], [new Rectangle(new(default, -359), 20, 1.1f)]);
}
