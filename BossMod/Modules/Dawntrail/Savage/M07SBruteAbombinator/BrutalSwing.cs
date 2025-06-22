namespace BossMod.Dawntrail.Savage.M07SBruteAbombinator;

sealed class BrutalSwing(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(25f, 90f.Degrees());
    private static readonly AOEShapeCircle circle = new(12f);
    private static readonly AOEShapeDonut donut = new(9f, 60f);
    private static readonly AOEShapeDonutSector donutSector = new(22f, 88f, 90f.Degrees());
    public AOEInstance? AOE;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref AOE);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.BrutishSwingCircle => circle,
            (uint)AID.BrutishSwingCone1 or (uint)AID.BrutishSwingCone2 => cone,
            (uint)AID.BrutishSwingDonut => donut,
            (uint)AID.BrutishSwingDonutSegment1 or (uint)AID.BrutishSwingDonutSegment2 => donutSector,
            _ => null
        };
        if (shape != null)
            AOE = new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BrutishSwingCircle:
            case (uint)AID.BrutishSwingCone1:
            case (uint)AID.BrutishSwingCone2:
            case (uint)AID.BrutishSwingDonut:
            case (uint)AID.BrutishSwingDonutSegment1:
            case (uint)AID.BrutishSwingDonutSegment2:
                AOE = null;
                ++NumCasts;
                break;
        }
    }
}
