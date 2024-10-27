namespace BossMod;

// helpers that can potentially be reused for different things

public static class Helpers
{
    public static readonly float sqrt3 = MathF.Sqrt(3);
    public static readonly Angle[] AnglesIntercardinals = [-45.003f.Degrees(), 44.998f.Degrees(), 134.999f.Degrees(), -135.005f.Degrees()];
    public static readonly Angle[] AnglesCardinals = [-90.004f.Degrees(), -0.003f.Degrees(), 180.Degrees(), 89.999f.Degrees()];
    public static readonly Actor FakeActor = new(0, 0, -1, "dummy", 0, ActorType.None, Class.None, 0, new(100, 0, 100, 0));

    public static WPos RotateAroundOrigin(float rotateByDegrees, WPos origin, WPos caster)
    {
        var (sin, cos) = MathF.SinCos(rotateByDegrees * Angle.DegToRad);
        var deltaX = caster.X - origin.X;
        var deltaZ = caster.Z - origin.Z;
        var rotatedX = cos * deltaX - sin * deltaZ;
        var rotatedZ = sin * deltaX + cos * deltaZ;
        return new WPos(origin.X + rotatedX, origin.Z + rotatedZ);
    }
}
