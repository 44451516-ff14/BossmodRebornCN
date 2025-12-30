namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

sealed class NorthernCross(BossModule module) : Components.GenericAOEs(module)
{
    public AOEInstance[] AOE = [];
    private ChillingCataclysm? _aoe;
    private readonly AOEShapeRect rect = new(60f, 12.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoe ??= Module.FindComponent<ChillingCataclysm>(); // prevent NotherCross from hiding the safespot
        return _aoe == null || _aoe.AOEs.Count == 0 ? AOE : [];
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x03)
            return;
        var pos = state switch
        {
            0x00200010u => new(131.487f, 107.988f),
            0x00020001u => new(116.472f, 127.977f),
            _ => (WPos)default
        };
        if (pos != default)
        {
            var rot = -126.875f.Degrees();
            AOE = [new(rect, pos, -126.875f.Degrees(), WorldState.FutureTime(9.2d), shapeDistance: rect.Distance(pos, rot))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NorthernCrossL or (uint)AID.NorthernCrossR)
        {
            ++NumCasts;
            AOE = [];
        }
    }
}
