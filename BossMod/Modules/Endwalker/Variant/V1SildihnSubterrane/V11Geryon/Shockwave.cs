namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V11Geryon;

sealed class Shockwave(BossModule module) : Components.GenericKnockback(module)
{
    private readonly List<Knockback> _kbs = new(2);
    private static readonly AOEShapeRect _shape = new(40f, 40f);
    private readonly RunawaySludge _aoe = module.FindComponent<RunawaySludge>()!;
    private readonly List<WPos> voidzones = new(4);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(_kbs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Shockwave)
        {
            var act = Module.CastFinishAt(spell);
            // knockback rect always happens through center, so create two sources with origin at center looking orthogonally
            AddKnockback(-90f.Degrees());
            AddKnockback(90f.Degrees());
            var vz = Module.Enemies((uint)OID.SludgeVoidzone);
            var count = vz.Count;
            for (var i = 0; i < count; ++i)
            {
                voidzones.Add(vz[i].Position);
            }
            void AddKnockback(Angle offset) => _kbs.Add(new(Arena.Center, 15f, act, _shape, offset, Kind.DirForward));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Shockwave)
        {
            _kbs.Clear();
            voidzones.Clear();
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return !Module.InBounds(pos);
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
                var dir = kb.Direction.ToDirection();
                var dir1 = -15f * dir;
                var dir2 = 15f * dir;
                var vzs = voidzones;
                var count = vzs.Count;
                var centerX = center.X;

                hints.AddForbiddenZone(p =>
                {
                    var projected1 = p + dir1;
                    var projected2 = p + dir2;
                    var pX = p.X;
                    for (var i = 0; i < count; ++i)
                    {
                        var pos = vzs[i];
                        if (pX > centerX && projected1.InSquare(pos, 9f) || pX < centerX && projected2.InSquare(pos, 9f))
                        {
                            return default;
                        }
                    }
                    if (pX > centerX && projected1.InSquare(center, 19f) || pX < centerX && projected2.InSquare(center, 19f))
                    {
                        return 1f;
                    }
                    return default;
                }, act);
            }
        }
    }
}
