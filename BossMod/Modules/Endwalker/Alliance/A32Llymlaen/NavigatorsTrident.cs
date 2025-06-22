namespace BossMod.Endwalker.Alliance.A32Llymlaen;

class DireStraits(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeRect _shape = new(40f, 40f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        aoes[0].Risky = true;
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DireStraitsVisualFirst or (uint)AID.DireStraitsVisualSecond)
        {
            _aoes.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 4.8f), _aoes.Count == 0 ? Colors.Danger : default, false));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DireStraitsAOEFirst or (uint)AID.DireStraitsAOESecond)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
                _aoes.RemoveAt(0);
        }
    }
}

class NavigatorsTridentAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NavigatorsTridentAOE, new AOEShapeRect(40f, 5f));

class NavigatorsTridentKnockback(BossModule module) : Components.GenericKnockback(module)
{
    private readonly SerpentsTide? _serpentsTide = module.FindComponent<SerpentsTide>();
    private readonly List<Knockback> _sources = new(2);

    private static readonly AOEShapeCone _shape = new(30f, 90f.Degrees());

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(_sources);

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (_serpentsTide == null)
            return false;
        var aoes = _serpentsTide.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Check(pos))
                return true;
        }
        return !Module.InBounds(pos);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.NavigatorsTridentAOE)
        {
            _sources.Add(new(spell.LocXZ, 20f, Module.CastFinishAt(spell), _shape, spell.Rotation + 90f.Degrees(), Kind.DirForward));
            _sources.Add(new(spell.LocXZ, 20f, Module.CastFinishAt(spell), _shape, spell.Rotation - 90f.Degrees(), Kind.DirForward));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.NavigatorsTridentAOE)
        {
            _sources.Clear();
            ++NumCasts;
        }
    }
}
