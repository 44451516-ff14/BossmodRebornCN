namespace BossMod.Endwalker.Alliance.A33Oschon;

class P2WanderingShot(BossModule module) : Components.GenericAOEs(module, (uint)AID.GreatWhirlwind)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCircle _shape = new(23f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var coords = spell.Action.ID switch
        {
            (uint)AID.WanderingShotN or (uint)AID.WanderingVolleyN => new WPos(default, 740f).Quantized(),
            (uint)AID.WanderingShotS or (uint)AID.WanderingVolleyS => new WPos(default, 760f).Quantized(),
            _ => default
        };
        if (coords != default)
        {
            _aoe = new(_shape, coords, default, Module.CastFinishAt(spell, 3.6d));
        }
    }
}
