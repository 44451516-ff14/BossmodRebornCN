namespace BossMod.Shadowbringers.Alliance.A35FalseIdol;

sealed class UnevenFooting(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeRect rect = new(80f, 15f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.UnevenFooting)
        {
            var rot = actor.Rotation - 90f.Degrees();
            _aoe = new(rect, (actor.Position - 40f * rot.ToDirection()).Quantized(), rot, WorldState.FutureTime(13.2d), risky: false);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.UnevenFooting)
        {
            _aoe = null;
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008u && actor.OID == (uint)OID.UnevenFooting && _aoe is AOEInstance aoe)
        {
            aoe.Risky = true;
            _aoe = aoe;
        }
    }
}
