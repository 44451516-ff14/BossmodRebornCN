namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V11Geryon;

sealed class Intake(BossModule module) : Components.GenericKnockback(module, stopAtWall: true)
{
    private readonly List<Knockback> _kbs = new(4);
    private readonly Explosion _aoe = module.FindComponent<Explosion>()!;
    private static readonly AOEShapeRect rect = new(40f, 5f);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _aoe.AOEs.Count != 0 ? CollectionsMarshal.AsSpan(_kbs) : [];

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x09)
        {
            var originX = state switch
            {
                0x00200010u => 20f,
                0x00020001u => -20f,
                _ => default
            };
            if (originX != default)
            {
                var act = WorldState.FutureTime(14.2d);
                var angle = originX == 20f ? Angle.AnglesCardinals[0] : Angle.AnglesCardinals[3];
                for (var i = 0; i < 4; ++i)
                {
                    _kbs.Add(new(new WPos(originX, 15f - i * 10f).Quantized(), 25f, act, rect, angle, Kind.TowardsOrigin));
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Intake)
        {
            _kbs.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kbs.Count != 0)
        {
            // square intentionally slightly smaller to prevent sus knockback, voidzones are treated as squares because we dont want to get knocked through them (there might be space behind them otherwise)
            ref var kb = ref _kbs.Ref(0);
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                var center = kb.Origin;
                var kbs = _kbs;
                var aoes = CollectionsMarshal.AsSpan(_aoe.AOEs);
                var len = aoes.Length;
                var donutPos = new WPos[2];
                var index = 0;
                for (var i = 0; i < len; ++i)
                {
                    ref var aoe = ref aoes[i];
                    if (aoe.Shape == Explosion.Donut)
                    {
                        donutPos[index++] = aoe.Origin;
                        if (index == 2)
                        {
                            break;
                        }
                    }
                }
                var dir = kb.Direction.ToDirection();

                var centerX = center.X;
                hints.AddForbiddenZone(p =>
                {
                    var kbz = CollectionsMarshal.AsSpan(kbs);
                    for (var i = 0; i < 4; ++i)
                    {
                        ref var kb = ref kbz[i];
                        var origin = kb.Origin;
                        if (p.InRect(origin, dir, 40f, default, 5f))
                        {
                            var projected = p + 25f * (origin - p).Normalized(); // we don't need to clamp the distance since origin is outside of the arena
                            for (var j = 0; j < 2; ++j)
                            {
                                if (projected.InCircle(donutPos[j], 6f)) // just need to get pulled into the general direction of the donut inner circle, still have about 4s to move into place after
                                {
                                    return 1f;
                                }
                            }
                            return default;
                        }
                    }
                    return default;
                }, act);
            }
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoe.AOEs);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Shape != Explosion.Donut && aoe.Check(pos)) // allow getting knocked into the donut since there is time to move after pull
            {
                return true;
            }
        }
        return false;
    }
}
