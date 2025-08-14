namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V25Enenra;

sealed class IntoTheFire(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(50f, 25f);
    private readonly List<AOEInstance> _aoes = new(2);
    private const float offset = 15.556349f;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00010002u)
        {
            var rot = actor.Rotation;
            var (positionOffset, rotation) = actor.OID switch
            {
                (uint)OID.SmokeVisual1 => (offset * (rot - 45f.Degrees()).ToDirection(), rot + 90f.Degrees()),
                (uint)OID.SmokeVisual2 => (offset * (rot + 45f.Degrees()).ToDirection(), rot - 90f.Degrees()),
                (uint)OID.SmokeVisual3 => (22f * rot.ToDirection(), rot + 180f.Degrees()),
                _ => ((WDir?)null, (Angle?)null)
            };

            if (positionOffset is WDir o && rotation is Angle a)
            {
                var correctedPosition = RoundPosition(actor.Position + o).Quantized();
                _aoes.Add(new(rect, correctedPosition, a, WorldState.FutureTime(16.6d)));
            }

            static WPos RoundPosition(WPos position) => new(MathF.Round(position.X), MathF.Round(position.Z));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.IntoTheFire)
        {
            _aoes.RemoveAt(0);
        }
    }
}
