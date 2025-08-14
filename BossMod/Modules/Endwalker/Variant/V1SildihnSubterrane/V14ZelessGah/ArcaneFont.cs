namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V14ZelessGah;

sealed class ArcaneFont(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(3);
    private static readonly AOEShapeRect rect = new(100f, 5f);
    private readonly List<Actor> cachedPortals = new(4);
    private readonly List<Actor> fireballs = new(4);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    // various packages can appear in random order in the replays, so we have to carefully manage the information
    public override void OnActorCreated(Actor actor)
    {
        switch (actor.OID)
        {
            case (uint)OID.ArcaneFont:
                var portals = Module.Enemies((uint)OID.Portal);
                var countP = portals.Count;
                var rot = actor.Rotation;
                _aoes.Add(new(rect, (actor.Position - 50f * rot.Round(90f).ToDirection()).Quantized(), rot, WorldState.FutureTime(countP == 0 ? 10.1d : 12.8d)));
                break;
            case (uint)OID.BallOfFire:
                fireballs.Add(actor);
                break;
            case (uint)OID.Portal:
                var count = _aoes.Count;
                var found = false;
                if (count != 0)
                {
                    if (fireballs.Count != 0) // fonts wont be teleported if there are teleporting fireballs
                    {
                        cachedPortals.Clear();
                        return;
                    }
                    var aoes = CollectionsMarshal.AsSpan(_aoes);
                    var pos = actor.Position;
                    var act = WorldState.FutureTime(12.5d);
                    ref var aoe0 = ref aoes[0];
                    var dir = aoe0.Rotation.Round(90f).ToDirection();

                    for (var i = 0; i < count; ++i)
                    {
                        ref var aoe = ref aoes[i];
                        if (aoe.ActorID != default) // aoe already got updated
                        {
                            continue;
                        }
                        aoe.Activation = act;
                        var origin = aoe.Origin;
                        var vecSQ = (origin - (pos - 50f * dir)).LengthSq();
                        if (vecSQ is > 99f and < 105f or < 1f)
                        {
                            var o = vecSQ < 10f ? new WDir(default, 10f) : new WDir(default, -10f);
                            aoe.Origin = (new WPos(MathF.Round(origin.X), MathF.Round(origin.Z)) + o).Quantized();
                            ++aoe.ActorID;
                            found = true;
                        }
                    }
                }
                if (!found)
                {
                    cachedPortals.Add(actor);
                }
                break;
        }
    }

    public override void Update() // fallback incase of portals spawning before aoe
    {
        var count = cachedPortals.Count;
        if (count == 0)
        {
            return;
        }
        if (Module.Enemies((uint)OID.BallOfFire).Count != 0) // fonts wont be teleported if there are teleporting fireballs
        {
            cachedPortals.Clear();
            return;
        }
        var countA = _aoes.Count;
        if (countA == 0)
        {
            return;
        }

        var count2 = count - 1;
        var act = WorldState.FutureTime(12.5d);
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = count2; i >= 0; --i)
        {
            var p = cachedPortals[i];
            var pos = p.Position;
            ref var aoe0 = ref aoes[0];
            var dir = aoe0.Rotation.Round(90f).ToDirection();
            for (var j = 0; j < countA; ++j)
            {
                ref var aoe = ref aoes[j];
                if (aoe.ActorID != default) // aoe already got updated
                {
                    continue;
                }
                aoe.Activation = act;
                var origin = aoe.Origin;
                var vecSQ = (origin - (pos - 50f * dir)).LengthSq();
                if (vecSQ is > 99f and < 105f or < 1f)
                {
                    var o = vecSQ < 10f ? new WDir(default, 10f) : new WDir(default, -10f);
                    aoe.Origin = (new WPos(MathF.Round(origin.X), MathF.Round(origin.Z)) + o).Quantized();
                    ++aoe.ActorID;
                    cachedPortals.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Burn:
                fireballs.Remove(caster);
                cachedPortals.Clear();
                break;
            case (uint)AID.BlazingBenifice:
                if (++NumCasts == _aoes.Count) // sometimes the activation of an aoe is delayed by multiple frames, so we wait until all are done
                {
                    NumCasts = 0;
                    _aoes.Clear();
                }
                break;
        }
    }
}
