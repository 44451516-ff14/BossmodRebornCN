namespace BossMod.Shadowbringers.Alliance.A22SuperiorFlightUnits;

sealed class HighOrderExplosiveBlastCross(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(12);
    private static readonly AOEShapeCross cross = new(20f, 2.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HighOrderExplosiveBlastCircle)
        {
            var pos = spell.LocXZ;
            var activation = Module.CastFinishAt(spell, 2.1d);
            AddAOE(Angle.AnglesIntercardinals[1]);
            AddAOE(Angle.AnglesCardinals[1]);
            void AddAOE(Angle angle) => _aoes.Add(new(cross, pos, angle, activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.HighOrderExplosiveBlastCross)
        {
            _aoes.RemoveAt(0);
        }
    }
}
