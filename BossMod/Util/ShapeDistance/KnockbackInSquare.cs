namespace BossMod;

[SkipLocalsInit]
public sealed class SDKnockbackInSquareFixedDirection(WPos Center, WDir Direction, float HalfWidth, Angle Rotation) : ShapeDistance
{
    private readonly WPos center = Center;
    private readonly WDir direction = Direction; // direction includes distance, not normalized
    private readonly float halfWidth = HalfWidth;
    private readonly WDir directionSquare = Rotation.ToDirection();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        return !(p + direction).InSquare(center, halfWidth, directionSquare);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}