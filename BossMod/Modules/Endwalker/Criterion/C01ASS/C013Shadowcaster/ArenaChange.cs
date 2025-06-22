namespace BossMod.Endwalker.VariantCriterion.C01ASS.C013Shadowcaster;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom rect = new([new Rectangle(C013Shadowcaster.ArenaCenter, 25f, 30f)], [new Rectangle(C013Shadowcaster.ArenaCenter, 15f, 20f)]);

    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NShowOfStrength or (uint)AID.SShowOfStrength && Arena.Bounds == C013Shadowcaster.StartingBounds)
            _aoe = new(rect, Arena.Center, default, Module.CastFinishAt(spell, 0.8f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00080004u && index == 0x02u)
        {
            Arena.Bounds = C013Shadowcaster.DefaultBounds;
            _aoe = null;
        }
    }
}
