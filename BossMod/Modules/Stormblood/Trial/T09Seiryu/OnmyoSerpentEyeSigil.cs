namespace BossMod.Stormblood.Trial.T09Seiryu;

sealed class OnmyoSerpentEyeSigil(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = new AOEInstance[1];
    private bool aoeInit;
    private static readonly AOEShapeDonut donut = new(7f, 30f);
    private static readonly AOEShapeCircle circle = new(12f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => aoeInit ? _aoe : [];

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        AOEShape? shape = modelState switch
        {
            32 => circle,
            33 => donut,
            _ => null
        };
        if (shape != null)
        {
            _aoe = [new(shape, actor.Position.Quantized(), default, WorldState.FutureTime(5.6d))];
            aoeInit = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.OnmyoSigil or (uint)AID.SerpentEyeSigil)
        {
            aoeInit = false;
        }
    }
}
