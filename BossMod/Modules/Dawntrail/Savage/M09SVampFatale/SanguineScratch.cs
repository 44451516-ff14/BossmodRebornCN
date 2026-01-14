
namespace BossMod.Dawntrail.Savage.M09SVampFatale;

// TODO: track bat players are tethered to, figure out max distance
// 3s from last scratch to bat explode; explosion only 1s cast so maybe necessary
// can AI move into donut if standing on edge of hitbox during mech?
sealed class BreakdownWing(BossModule module) : Components.GenericAOEs(module)
{
    // end pos always opposite of start; unnecessary to store end pos?
    class Vampette
    {
        public Actor Actor;
        public WPos StartPos;
        public WPos EndPos;
    }
    private readonly List<Vampette> _vamps = [];
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
            return [];

        // make it only show closer to activation? no need to show too early
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID._Gen_2056)
        {
            // 0x426 = circle, 0x427 = donut
            // action enum says 8-15 but inner radius more like 4 in replay; based on boss stacks?
            if (status.Extra is 0x426 or 0x427)
            {
                for (var i = 0; i < _vamps.Count(); i++)
                {
                    if (_vamps[i].Actor.InstanceID == actor.InstanceID)
                    {
                        _aoes.Add(new(status.Extra == 0x426 ? new AOEShapeCircle(7f) : new AOEShapeDonut(4f, 15f), NumCasts == 0 ? _vamps[i].EndPos : _vamps[i].StartPos, activation: WorldState.FutureTime(5.1d)));
                    }
                }
            }
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        // get vampette start pos here, end pos is opposite of starting
        // rotates CW or CCW but returns to start pos regardless
        if (actor.OID == (uint)OID.VampetteFatale)
        {
            if (id == 0x1E46)
            {
                var start = actor.Position;
                var end = Arena.Center - (actor.Position - Arena.Center);
                _vamps.Add(new() { Actor = actor, StartPos = start, EndPos = end });
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.BreakdownDrop1 or (uint)AID.BreakdownDrop2 or (uint)AID.BreakwingBeat1 or (uint)AID.BreakwingBeat2)
        {
            ++NumCasts;
            _aoes.Clear();
        }
    }
}
//sealed class BreakdownDrop(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BreakdownDrop1, (uint)AID.BreakdownDrop2], 7f);
//sealed class BreakwingBeat(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BreakwingBeat1, (uint)AID.BreakwingBeat2], new AOEShapeDonut(8f, 15f));
/*
sealed class SanguineScratch(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeCone _cone = new(40f, 15f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
            return [];
        else if (_aoes.Count < 8)
            return CollectionsMarshal.AsSpan(_aoes);
        else
            return CollectionsMarshal.AsSpan(_aoes)[..8];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SanguineScratchFirst)
        {
            // boss does 8 cones x5, including SanguineScratchFirst
            // hector video shows cones rotate to previous safe spots, rotate 90/4 (22.5) degrees each cast?
            // 2.4s between casts
            // handle in groups of 8? add all AOEs for 5 sets and sort by activation?
            for (var i = 0; i < 5; i++)
            {
                _aoes.Add(new(_cone, caster.Position, caster.Rotation + (22.5f * i).Degrees(), Module.CastFinishAt(spell).AddSeconds(2.4d * i)));
            }

            if (_aoes.Count == 40)
                _aoes.Sort((a, b) => a.Activation <= b.Activation ? 1 : -1);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SanguineScratchFirst or (uint)AID.SanguineScratchRest)
        {
            // remove one at a time or by same activation?
            if (_aoes.Count > 0)
            {
                ++NumCasts;
                _aoes.RemoveAt(0);
            }
        }
    }
}
*/
sealed class SanguineScratch(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
            return [];
        return CollectionsMarshal.AsSpan(_aoes)[..1];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SanguineScratchFirst)
        {
            if (_aoes.Count > 0)
                return;

            for (var i = 0; i < 5; i++)
            {
                _aoes.Add(new(CreateScratchCones(caster.Rotation + (22.5f * i).Degrees()), caster.Position, default, Module.CastFinishAt(spell).AddSeconds(2.4d * i)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SanguineScratchFirst or (uint)AID.SanguineScratchRest)
        {
            if (_aoes.Count > 0)
            {
                ++NumCasts;
                if (NumCasts % 8 == 0)
                {
                    _aoes.RemoveAt(0);
                }
            }
        }
    }

    private AOEShapeCustom CreateScratchCones(Angle rotation)
    {
        var cones = new Shape[8];
        for (var i = 0; i < cones.Length; i++)
        {
            cones[i] = new Cone(Arena.Center, 40f, (i * 45f - 15f).Degrees() + rotation, (i * 45f + 15f).Degrees() + rotation);
        }

        return new AOEShapeCustom(cones);
    }
}
