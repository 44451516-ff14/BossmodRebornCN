namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL4Dawon;

sealed class Obey(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly AOEShapeCross Cross = new(50f, 7f);
    public static readonly AOEShapeDonut Donut = new(12f, 60f);
    private readonly List<AOEInstance> _aoes = new(4);
    private bool first = true;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnActorCreated(Actor actor)
    {
        AOEShape? shape = actor.OID switch
        {
            (uint)OID.FervidPulseJump => Cross,
            (uint)OID.FrigidPulseJump => Donut,
            _ => null
        };
        if (shape != null)
        {
            Angle rotation = default;
            if (shape == Cross)
            {
                rotation = Angle.FromDirection(actor.Position.Quantized() - (_aoes.Count == 0 ? Module.PrimaryActor.Position : _aoes[^1].Origin));
            }
            _aoes.Add(new(shape, actor.Position.Quantized(), rotation, _aoes.Count == 0 ? WorldState.FutureTime(first ? 11.5d : 13.8d) : _aoes[0].Activation.AddSeconds(5.1d * _aoes.Count)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.FervidPulseJump or (uint)AID.FrigidPulseJump)
        {
            _aoes.RemoveAt(0);
            first = false;
        }
    }
}
