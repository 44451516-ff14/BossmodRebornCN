namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS8Queen;

// limiting to one aoe since there are 2 casters to circumvent aoe target limit
sealed class HeavensWrathAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavensWrathKnockback, new AOEShapeRect(60f, 5f), 1);

// TODO: generalize
sealed class HeavensWrathKnockback(BossModule module) : Components.GenericKnockback(module)
{
    private readonly List<Knockback> _sources = new(2);
    private static readonly AOEShapeRect _shape = new(60f, 50f, -5f);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(_sources);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_sources.Count == 0 && spell.Action.ID == (uint)AID.HeavensWrathKnockback)
        {
            var act = Module.CastFinishAt(spell);
            _sources.Add(new(caster.Position, 15f, act, _shape, spell.Rotation + 90f.Degrees(), Kind.DirForward));
            _sources.Add(new(caster.Position, 15f, act, _shape, spell.Rotation - 90f.Degrees(), Kind.DirForward));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HeavensWrathKnockback)
        {
            _sources.Clear();
            ++NumCasts;
        }
    }
}
