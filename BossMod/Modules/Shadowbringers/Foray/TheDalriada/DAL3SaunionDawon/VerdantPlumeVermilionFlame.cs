namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL3SaunionDawon;

sealed class VerdantPlumeVermilionFlame(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(10f);
    private static readonly AOEShapeDonut donut = new(3f, 12f);
    private readonly List<AOEInstance> _aoes = new(12);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        AOEShape? shape = actor.OID switch
        {
            (uint)OID.VerdantPlume => donut,
            (uint)OID.VermilionFlame => circle,
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, actor.Position.Quantized(), default, WorldState.FutureTime(14.4d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.RawHeat or (uint)AID.Explosion)
        {
            _aoes.Clear();
        }
    }
}
