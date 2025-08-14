namespace BossMod.Dawntrail.Raid.M04NWickedThunder;

sealed class StampedingThunder(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(40f, 15f);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var activation = WorldState.FutureTime(9.4d);
        if (spell.Action.ID == (uint)AID.StampedingThunderVisualWest)
            _aoe = new(rect, new(95f, 80f), caster.Rotation, activation);
        else if (spell.Action.ID == (uint)AID.StampedingThunderVisualEast)
            _aoe = new(rect, new(105f, 80f), caster.Rotation, activation);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x00 && state is 0x00200010u or 0x00020001u)
            _aoe = null;
    }
}

sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    public static readonly ArenaBoundsSquare DefaultBounds = new(20f);
    public static readonly WPos DefaultCenter = new(100f, 100f);
    public static readonly WPos WestRemovedCenter = new(115f, 100f);
    public static readonly WPos EastremovedCenter = new(85f, 100f);
    private static readonly ArenaBoundsRect damagedPlatform = new(5f, 20f);

    public override void OnEventEnvControl(byte index, uint state)
    {
        // index 0x00
        // 0x00200010 - west 3/4 disappears
        // 0x00020001 - east 3/4 disappears
        if (index == 0x00)
        {
            if (state == 0x00020001u)
            {
                Arena.Bounds = damagedPlatform;
                Arena.Center = EastremovedCenter;
            }
            else if (state == 0x00200010u)
            {
                Arena.Bounds = damagedPlatform;
                Arena.Center = WestRemovedCenter;
            }
            else if (state is 0x00400004u or 0x00800004u)
            {
                Arena.Bounds = DefaultBounds;
                Arena.Center = DefaultCenter;
            }
        }
    }
}
