namespace BossMod.Global.MaskedCarnivale.Stage27;

public enum OID : uint
{
    Boss = 0x2CF8, //R=4.5
    Bomb = 0x2CF9, //R=0.8
    MagitekExplosive = 0x2CEC, //R=0.8
    Helper = 0x233C
}

public enum AID : uint
{
    BombsSpawn = 19260, // Boss->self, no cast, single-target
    Fungah = 19256, // Boss->self, no cast, range 8+R 90-degree cone, knockback 15 away from source
    Explosion = 19259, // Bomb->self, no cast, range 8 circle, wipe if in range
    Fireball = 19258, // Boss->location, 4.0s cast, range 8 circle
    Fungahhh = 19257, // Boss->self, no cast, range 8+R 90-degree cone, knockback 15 away from source
    Snort = 19266, // Boss->self, 10.0s cast, range 60+R circle, knockback 15 away from source
    MassiveExplosion = 19261 // MagitekExplosive->self, no cast, range 60 circle, wipe, failed to destroy Magitek Explosive in time
}

class Fireball(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Fireball, 8f);
class Snort(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Snort, 15f, stopAtWall: true);

class Fungah(BossModule module) : Components.GenericKnockback(module, stopAtWall: true)
{
    private DateTime _activation;
    private bool otherpatterns;
    private static readonly AOEShapeCone cone = new(12.5f, 45f.Degrees());
    private readonly Explosion _aoe = module.FindComponent<Explosion>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_activation != default || otherpatterns)
            return new Knockback[1] { new(Module.PrimaryActor.Position, 15f, _activation, cone, Direction: Angle.FromDirection(actor.Position - Module.PrimaryActor.Position)) };
        return [];
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Bomb && Module.Enemies((uint)OID.Bomb).Count == 8)
        {
            _activation = WorldState.FutureTime(5d);
            var magitekExplosives = Module.Enemies((uint)OID.MagitekExplosive);
            var explosive = magitekExplosives.Count != 0 ? magitekExplosives[0] : null;
            if (explosive != null && (explosive.Position.AlmostEqual(new(96f, 94f), 3f) || explosive.Position.AlmostEqual(new(92f, 100f), 3f) ||
             explosive.Position.AlmostEqual(new(96f, 106f), 3) || explosive.Position.AlmostEqual(new(108f, 100f), 3f)))
            {
                _activation = WorldState.FutureTime(5.3d);
                otherpatterns = true;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Explosion)
            _activation = default;
        else if (spell.Action.ID is (uint)AID.Fungah or (uint)AID.Fungahhh)
            otherpatterns = false;
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Check(pos))
                return true;
        }
        return false;
    }
}

class Explosion(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _casters = [];
    private static readonly AOEShapeCircle circle = new(8);
    private DateTime _activation;
    private DateTime _snortingeffectends;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _casters.Count;
        if (count == 0)
            return [];

        var aoes = new AOEInstance[count];
        if (_snortingeffectends == default)
        {
            for (var i = 0; i < count; ++i)
                aoes[i] = new(circle, _casters[i].Position, default, _activation);
            return aoes;
        }
        else if (_snortingeffectends > WorldState.CurrentTime)
        {
            var primary = Module.PrimaryActor.Position;
            for (var i = 0; i < count; ++i)
            {
                var pos = _casters[i].Position;
                var raydir = (pos - primary).Normalized();
                aoes[i] = new(circle, pos + Math.Min(15f, Module.Arena.IntersectRayBounds(pos, raydir) - 0.4f) * raydir, default, _activation); // 0.4f = half of hitbox radius is used for some reason
            }
            return aoes;
        }
        return [];
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.Bomb)
        {
            if (animState1 == 1)
            {
                _casters.Add(actor);
                _activation = WorldState.FutureTime(6d);
            }
        }
    }

    public override void Update()
    {
        if (_snortingeffectends != default && _snortingeffectends < WorldState.CurrentTime)
            _snortingeffectends = default;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_casters.Count > 0 && spell.Action.ID == (uint)AID.Explosion)
            _casters.Remove(caster);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Snort)
            _snortingeffectends = Module.CastFinishAt(spell, 2.5f);
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.PrimaryActor.Name} will spawn Bombs and Magitek Explosives throughout the fight.\nUse Snort to push away Bombs from Magitek Explosives and bait Fireballs\naway from the MEs. Meanwhile destroy the MEs asap because they will blow\nup on their own after about 35s. If any ME detonates you will be wiped.\nThe MEs are weak against water abilities and strong against fire attacks.");
    }
}

class Hints2(BossModule module) : BossComponent(module)
{
    private DateTime _activation;

    public override void AddGlobalHints(GlobalHints hints)
    {
        var explosives = Module.Enemies((uint)OID.MagitekExplosive);
        var count = explosives.Count;
        if (count == 0)
            return;
        var explosive = explosives[0];
        if (!explosive.IsDead)
            hints.Add($"A {explosive!.Name} spawned, destroy it asap.");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var explosives = Module.Enemies((uint)OID.MagitekExplosive);
        var count = explosives.Count;
        if (count == 0)
            return;
        var explosive = explosives[0];
        if (explosive.IsTargetable)
            hints.Add($"Explosion in ca.: {Math.Max(35 - (WorldState.CurrentTime - _activation).TotalSeconds, 0.0f):f1}s");
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.MagitekExplosive)
            _activation = WorldState.CurrentTime;
    }
}

class Stage27States : StateMachineBuilder
{
    public Stage27States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Fireball>()
            .ActivateOnEnter<Snort>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<Fungah>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 696, NameID = 3046)]
public class Stage27 : BossModule
{
    public Stage27(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Bomb), Colors.Object);
        Arena.Actors(Enemies((uint)OID.MagitekExplosive), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.MagitekExplosive => 1,
                (uint)OID.Boss => 0,
                _ => AIHints.Enemy.PriorityPointless,
            };
        }
    }
}
