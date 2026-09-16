namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL4Dawon;

sealed class Obey(BossModule module) : Components.GenericAOEs(module)
{
    public readonly AOEShapeCross cross = new(50f, 7f);
    public readonly AOEShapeDonut donut = new(12f, 60f);
    private readonly List<AOEInstance> _aoes = [with(4)];
    private bool first = true;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnActorCreated(Actor actor)
    {
        AOEShape? shape = actor.OID switch
        {
            (uint)OID.FervidPulseJump => cross,
            (uint)OID.FrigidPulseJump => donut,
            _ => null
        };
        if (shape != null)
        {
            Angle rotation = default;
            var pos = actor.Position;
            if (shape == cross)
            {
                rotation = Angle.FromDirection(pos - (_aoes.Count == 0 ? Module.PrimaryActor.Position : _aoes[^1].Origin));
            }
            var posQ = pos.Quantized();
            _aoes.Add(new(shape, posQ, rotation, _aoes.Count == 0 ? WorldState.FutureTime(first ? 11.5d : 13.8d) : _aoes[0].Activation.AddSeconds(5.1d * _aoes.Count),
            shapeDistance: shape.Distance(posQ, rotation), arenaProjectionLayer: 0, restrictToArenaProjectionLayer: true));
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
