namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

sealed class NorthernCross(BossModule module) : Components.GenericAOEs(module)
{
    public AOEInstance? AOE;

    private static readonly AOEShapeRect _shape = new(25f, 30f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var component = Module.FindComponent<ChillingCataclysm>(); // prevent NotherCross from hiding the safespot
        return component == null || component.ActiveAOEs(slot, actor).Length == 0 ? Utils.ZeroOrOne(ref AOE) : [];
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index != 0x03)
            return;
        var offset = state switch
        {
            0x00200010 => -90f.Degrees(),
            0x00020001 => 90f.Degrees(),
            _ => default
        };
        if (offset != default)
            AOE = new(_shape, Arena.Center, -126.875f.Degrees() + offset, WorldState.FutureTime(9.2d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NorthernCrossL or (uint)AID.NorthernCrossR)
        {
            ++NumCasts;
            AOE = null;
        }
    }
}
