namespace BossMod.Endwalker.Alliance.A34Eulogia;

class ClimbingShot(BossModule module) : Components.GenericKnockback(module)
{
    private readonly AsAboveSoBelow? _exaflare = module.FindComponent<AsAboveSoBelow>();
    private Knockback? _knockback;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => Utils.ZeroOrOne(ref _knockback);

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (_exaflare == null)
            return false;
        var aoes = _exaflare.ActiveAOEs(slot, actor);
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
        if (spell.Action.ID is (uint)AID.ClimbingShotNald or (uint)AID.ClimbingShotThal)
            _knockback = new(spell.LocXZ, 20f, Module.CastFinishAt(spell, 0.2f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ClimbingShotAOE1 or (uint)AID.ClimbingShotAOE2 or (uint)AID.ClimbingShotAOE3 or (uint)AID.ClimbingShotAOE4)
        {
            ++NumCasts;
            _knockback = null;
        }
    }
}
