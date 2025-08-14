namespace BossMod.Dawntrail.Savage.M05SDancingGreen;

class LetsDance(BossModule module) : Components.GenericAOEs(module)
{
    protected readonly List<AOEInstance> _aoes = new(8);
    protected static readonly AOEShapeRect rect = new(25f, 25f);
    protected readonly GetDownBait _bait = module.FindComponent<GetDownBait>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _bait.CurrentBaits.Count == 0 && _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void AddGlobalHints(GlobalHints hints)
    {
        var count = _aoes.Count;
        if (count > 0)
        {
            var sb = new StringBuilder(4 * (count - 1) + 4 * count);
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                var roundedrot = (int)aoes[i].Rotation.Deg;
                var shapeHint = roundedrot switch
                {
                    89 => "East",
                    -90 => "West",
                    _ => ""
                };
                sb.Append(shapeHint);

                if (i < count - 1)
                    sb.Append(" -> ");
            }
            hints.Add(sb.ToString());
        }
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.Frogtourage && modelState is 5 or 7)
        {
            var count = _aoes.Count;
            var act = count != 0 ? _aoes[0].Activation.AddSeconds(count * 2.4d) : WorldState.FutureTime(26.1d);
            _aoes.Add(new(rect, Arena.Center.Quantized(), modelState == 5 ? Angle.AnglesCardinals[3] : Angle.AnglesCardinals[0], act));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LetsDance)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
            {
                _aoes.RemoveAt(0);
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_aoes.Count < 2)
        {
            return;
        }
        // make ai stay close to boss to ensure successfully dodging the combo
        hints.AddForbiddenZone(ShapeDistance.InvertedRect(Arena.Center, new WDir(1f, default), 2f, 2f, 40f), _aoes.Ref(0).Activation);
    }
}

sealed class LetsDanceRemix(BossModule module) : LetsDance(module)
{
    private static readonly Angle a180 = 180f.Degrees();

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0 || _bait.CurrentBaits.Count != 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];
            if (i == 0)
            {
                if (count > 1)
                {
                    aoe.Color = Colors.Danger;
                }
                aoe.Risky = true;
            }
            else
            {
                ref var aoe0 = ref aoes[0];
                if (aoe0.Rotation.AlmostEqual(aoe.Rotation + a180, Angle.DegToRad))
                {
                    aoe.Risky = false;
                }
            }
        }
        return aoes[..max];
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        var count = _aoes.Count;
        if (count > 0)
        {
            var sb = new StringBuilder(4 * (count - 1) + count * 5);
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                var roundedrot = (int)aoes[i].Rotation.Deg;
                var shapeHint = roundedrot switch
                {
                    89 => "East",
                    -90 => "West",
                    180 => "North",
                    0 => "South",
                    _ => ""
                };
                sb.Append(shapeHint);

                if (i < count - 1)
                    sb.Append(" -> ");
            }
            hints.Add(sb.ToString());
        }
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.Frogtourage && modelState is 5 or 7 or 31 or 32)
        {
            var count = _aoes.Count;
            var act = count != 0 ? _aoes.Ref(0).Activation.AddSeconds(count * 1.5d) : WorldState.FutureTime(26d);
            _aoes.Add(new(rect, Arena.Center.Quantized(), modelState == 5 ? Angle.AnglesCardinals[3] : modelState == 31 ? Angle.AnglesCardinals[1]
            : modelState == 32 ? a180 : Angle.AnglesCardinals[0], act));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LetsDanceRemix)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
            {
                _aoes.RemoveAt(0);
            }
        }
    }
}
