namespace BossMod.Endwalker.Alliance.A34Eulogia;

class Hydrostasis(BossModule module) : Components.GenericKnockback(module)
{
    private readonly List<Knockback> _sources = new(3);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(_sources);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.HydrostasisAOE1 or (uint)AID.HydrostasisAOE2 or (uint)AID.HydrostasisAOE3)
        {
            _sources.Add(new(caster.Position, 28f, Module.CastFinishAt(spell)));
            _sources.Sort((a, b) => a.Activation.CompareTo(b.Activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.HydrostasisAOE1 or (uint)AID.HydrostasisAOE2 or (uint)AID.HydrostasisAOE3)
        {
            ++NumCasts;
            if (_sources.Count != 0)
                _sources.RemoveAt(0);
        }
    }
}
