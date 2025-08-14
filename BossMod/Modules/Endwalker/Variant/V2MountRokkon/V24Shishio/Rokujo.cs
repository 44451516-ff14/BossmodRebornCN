namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V24Shishio;

sealed class OnceTwiceThriceRokujo : Components.SimpleAOEGroups
{
    public OnceTwiceThriceRokujo(BossModule module) : base(module, [(uint)AID.OnceOnRokujo, (uint)AID.TwiceOnRokujo, (uint)AID.ThriceOnRokujo], new AOEShapeRect(60f, 7f))
    {
        MaxDangerColor = 1;
    }
}

sealed class Rokujo(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle[] circles = [new(8f), new(12f), new(23f)];
    private readonly List<AOEInstance> _aoes = new(12);
    private readonly List<Actor> _clouds = new(18);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 6 ? 6 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if (count > 2)
        {
            for (var i = 0; i < 2; ++i)
            {
                ref var aoe = ref aoes[i];
                aoe.Color = Colors.Danger;
            }
        }
        return aoes[..max];
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.Raiun)
        {
            _clouds.Remove(actor);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Raiun)
        {
            _clouds.Add(actor);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SmokeaterAbsorb)
        {
            _clouds.Remove(caster);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.OnceOnRokujo:
                RemoveCloudsAndAddAOEs(caster, circles[0], 8f, 1d, aoeCount => 1d + Math.Ceiling(aoeCount / 2d));
                break;
            case (uint)AID.TwiceOnRokujo:
                RemoveCloudsAndAddAOEs(caster, circles[1], 12f, 1.6d, aoeCount => 3.1d);
                break;
            case (uint)AID.ThriceOnRokujoVisual1:
                var count = _clouds.Count;
                for (var i = 0; i < count; ++i)
                {
                    _aoes.Add(new(circles[2], _clouds[i].Position.Quantized(), default, Module.CastFinishAt(spell, 7.1d)));
                }
                break;
        }

        void RemoveCloudsAndAddAOEs(Actor caster, AOEShapeCircle circle, float radius, double initialActivation, Func<int, double> activation)
        {
            var countClouds = _clouds.Count;
            var pos = caster.Position;
            var rot = spell.Rotation.ToDirection();
            for (var i = countClouds - 1; i >= 0; --i)
            {
                var cloud = _clouds[i].Position;
                if (cloud.InRect(pos, rot, 30f, 30f, 7f))
                {
                    _aoes.Add(new(circle, cloud.Quantized(), default, Module.CastFinishAt(spell, initialActivation)));
                    _clouds.RemoveAt(i);
                }
            }

            while (_clouds.Count > 0)
            {
                List<AOEInstance> newAOEs = [];
                var removedAny = false;
                var countClouds2 = _clouds.Count;
                for (var i = countClouds2 - 1; i >= 0; --i)
                {
                    var count = _aoes.Count;
                    var min = count - 2;
                    var max = min > 0 ? min : 0;
                    for (var j = max; j < count; ++j)
                    {
                        var cloud = _clouds[i].Position;
                        if (_aoes[j].Origin.InCircle(cloud, radius))
                        {
                            newAOEs.Add(new(circle, cloud.Quantized(), default, Module.CastFinishAt(spell, activation(count))));
                            _clouds.RemoveAt(i);
                            removedAny = true;
                            break;
                        }
                    }
                }

                if (!removedAny)
                {
                    break;  // Avoid infinite loop incase something goes wrong
                }
                _aoes.AddRange(newAOEs);
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.LeapingLevin1:
                case (uint)AID.LeapingLevin2:
                case (uint)AID.LeapingLevin3:
                    _aoes.RemoveAt(0);
                    break;
            }
        }
    }
}
