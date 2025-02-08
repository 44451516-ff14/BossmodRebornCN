namespace BossMod.Endwalker.VariantCriterion.C01ASS.C011Silkie;

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom square = new([new Square(C011Silkie.ArenaCenter, 30f)], [new Square(C011Silkie.ArenaCenter, 20f)]);

    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NFizzlingSuds or (uint)AID.SFizzlingSuds && Arena.Bounds == C011Silkie.StartingBounds)
            _aoe = new(square, Arena.Center, default, WorldState.FutureTime(3.8d));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x01)
        {
            Arena.Bounds = C011Silkie.DefaultBounds;
            _aoe = null;
        }
    }
}
