namespace BossMod.Dawntrail.Savage.M10STheXtremes;

public class VoidzoneShape(BossModule module, uint OID, AOEShape shape) : Components.GenericAOEs(module, default, "GTFO from voidzone!")
{
    public VoidzoneShape(BossModule module, uint OID, float radius) : this(module, OID, new AOEShapeCircle(radius)) { }
    public readonly AOEShape Shape = shape;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = new List<AOEInstance>();
        foreach (var source in GetVoidActors())
        {
            aoes.Add(new(Shape, source.Position, source.Rotation));
        }
        return CollectionsMarshal.AsSpan(aoes);
    }

    private ReadOnlySpan<Actor> GetVoidActors()
    {
        var enemies = Module.Enemies(OID);
        var enemySpan = CollectionsMarshal.AsSpan(enemies);
        var len = enemySpan.Length;

        if (len == 0)
            return [];

        List<Actor> voids = [];
        for (var i = 0; i < len; i++)
        {
            var enemy = enemySpan[i];
            if (enemy.EventState != 7)
            {
                voids.Add(enemy);
            }
        }

        return CollectionsMarshal.AsSpan(voids);
    }
}

sealed class FlameFloaterPuddle(BossModule module) : VoidzoneShape(module, (uint)OID.PuddleFlameFloater, new AOEShapeRect(60f, 4f));
sealed class AlleyOopInfernoPuddle(BossModule module) : VoidzoneShape(module, (uint)OID.PuddleAlleyOopInferno, 5f);
sealed class CutbackBlazePuddle(BossModule module) : VoidzoneShape(module, (uint)OID.PuddleCutbackBlaze, new AOEShapeCone(60f, 165f.Degrees()));
sealed class Pyrorotation(BossModule module) : Components.StackWithIcon(module, (uint)IconID.PyrorotationStack, (uint)AID.Pyrotation, 6f, 5d, 6, 8, 3);
sealed class PyrorotationPuddle(BossModule module) : VoidzoneShape(module, (uint)OID.PuddlePyrorotation, 6f);
sealed class BlastingSnapPuddle(BossModule module) : VoidzoneShape(module, (uint)OID.PuddleInsaneAirCone, new AOEShapeCone(60f, 22.5f.Degrees()));
sealed class HotImpact(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.HotImpact, 6f);
sealed class CutbackBlaze(BossModule module) : Components.CastCounter(module, (uint)AID.CutbackBlaze)
{
    private readonly DebuffTracker _debuff = module.FindComponent<DebuffTracker>()!;
    private bool _active = false;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!_active)
            return;

        // don't make it AOE so AI doesn't freak out
        // assuming it's baited by top aggro
        // looks more like furthest player
        var primary = Module.PrimaryActor;
        var target = GetFurthest();

        if (target == null)
            return;

        Arena.ZoneCone(primary.Position, 0f, 60f, (target.Position - primary.Position).ToAngle(), 165f.Degrees(), Colors.AOE);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CutbackBlazeCast)
        {
            _active = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            NumCasts++;
            _active = false;
        }
    }

    private Actor? GetFurthest()
    {
        var party = Raid.WithSlot(false, true, true);
        var fires = _debuff.FirePlayers;

        if (fires.Any())
        {
            party = [.. party.IncludedInMask(fires)];
        }

        var len = party.Length;

        if (len == 0)
            return null;

        var source = Module.PrimaryActor;
        Actor? furthest = null;
        var furthestDistSq = float.MinValue;

        for (var i = 0; i < len; i++)
        {
            ref readonly var player = ref party[i].Item2;
            var distSq = (player.Position - source.Position).LengthSq();

            if (distSq > furthestDistSq)
            {
                furthestDistSq = distSq;
                furthest = player;
            }
        }
        return furthest;
    }
}
sealed class DiversDareRed(BossModule module) : Components.RaidwideCast(module, (uint)AID.DiversDareRed);
sealed class AlleyOopInferno(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.AlleyOopInferno, 5f)
{
    private BitMask _targets;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Firesnaking)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot == -1)
                return;

            _targets.Set(slot);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Firesnaking)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot == -1)
                return;

            _targets.Clear(slot);
        }
    }
}
sealed class HotImpact2(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.HotImpact2, 6f);
sealed class HotAerial(BossModule module) : Components.GenericStackSpread(module)
{
    private readonly DebuffTracker _debuffs = module.FindComponent<DebuffTracker>()!;
    private bool _active = false;

    // TODO: jumps onto furthest player with firesnaking (or random if players dead?)
    // don't show all baits at once
    // only show baits if player has the right status
    // missing cutback 330 cone, not necessarily boss rotation? or just lag?
    // missing purorotation stack indicator
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HotAeriaCast)
        {
            _active = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.HotAerial4)
        {
            _active = false;
            Spreads.Clear();
        }
    }

    public override void Update()
    {
        if (!_active)
            return;

        Spreads.Clear();

        var target = FindActorByDistance(false);
        if (target == null)
            return;

        Spreads.Add(new(target, 6f));
    }

    private Actor? FindActorByDistance(bool findClosest)
    {
        var fires = Raid.WithSlot(false, true, true).IncludedInMask(_debuffs.FirePlayers);
        var source = Module.PrimaryActor;
        Actor? selected = null;

        var bestDistance = findClosest ? float.MaxValue : float.MinValue;

        foreach (var (_, p) in fires)
        {
            var distSq = (p.Position - source.Position).LengthSq();
            var isBetter = findClosest ? distSq < bestDistance : distSq > bestDistance;

            if (isBetter)
            {
                bestDistance = distSq;
                selected = p;
            }
        }
        return selected;
    }
}
sealed class FreakyPyrotation(BossModule module) : Components.StackWithIcon(module, (uint)IconID.FreakyPyrotation, (uint)AID.FreakyPyrotation, 6f, 5f, 2, 2);
sealed class FlameFloaterSplit(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlameFloaterSplit, new AOEShapeRect(60f, 4f))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FlameFloaterSplitCast)
        {
            var origin = spell.LocXZ;
            var rotation = spell.Rotation;
            Casters.Add(new(Shape, origin, rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, shapeDistance: Shape.Distance(origin, rotation)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Casters.Clear();
        }
    }
}
