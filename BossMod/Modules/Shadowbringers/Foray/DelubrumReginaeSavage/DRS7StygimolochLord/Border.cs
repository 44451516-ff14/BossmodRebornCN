namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

class Border(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly WPos BoundsCenter = new(-416, -184);
    public static readonly ArenaBoundsCircle DefaultBounds = new(34.5f);
    private const float _innerRingRadius = 14.5f;
    private const float _outerRingRadius = 27.5f;
    private const float _ringHalfWidth = 2.5f;
    private const float _alcoveDepth = 1;
    private const float _alcoveWidth = 2;
    private bool Active;
    private static readonly Shape[] labyrinth = [new PolygonCustom(InDanger()), new PolygonCustom(MidDanger()), new PolygonCustom(OutDanger())];
    private static readonly AOEShapeCustom customShape = new(labyrinth);
    private static readonly ArenaBoundsComplex labPhase = new([new Circle(BoundsCenter, 34.5f)], labyrinth);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Active)
            yield return new(customShape, Arena.Center);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.MemoryOfTheLabyrinth)
        {
            Active = true;
            Arena.Bounds = labPhase;
        }
    }

    private static List<WPos> RingBorder(Angle centerOffset, float ringRadius, bool innerBorder)
    {
        var offsetMultiplier = innerBorder ? -1 : 1;
        var halfWidth = (_alcoveWidth / ringRadius).Radians();
        var radiusWithDepth = ringRadius + offsetMultiplier * (_ringHalfWidth + _alcoveDepth);
        var radiusWithoutDepth = ringRadius + offsetMultiplier * _ringHalfWidth;
        var stepAngle = 45.Degrees();

        var points = new List<WPos>();

        for (var i = 0; i < 8; ++i)
        {
            var currentCenter = centerOffset + i * stepAngle;
            points.AddRange(CurveApprox.CircleArc(BoundsCenter, radiusWithDepth, currentCenter - halfWidth, currentCenter + halfWidth, Shape.MaxApproxError));
            var nextCenter = currentCenter + stepAngle;
            points.AddRange(CurveApprox.CircleArc(BoundsCenter, radiusWithoutDepth, currentCenter + halfWidth, nextCenter - halfWidth, Shape.MaxApproxError));
        }
        points.Add(points[0]);

        return points;
    }

    private static List<WPos> InDanger() => RingBorder(22.5f.Degrees(), _innerRingRadius, true);

    private static List<WPos> MidDanger()
    {
        var outerRing = RingBorder(0.Degrees(), _outerRingRadius, true);
        var innerRing = RingBorder(22.5f.Degrees(), _innerRingRadius, false);
        innerRing.Reverse();
        outerRing.AddRange(innerRing);
        return outerRing;
    }

    private static List<WPos> OutDanger()
    {
        var outerBoundary = CurveApprox.Circle(BoundsCenter, 34.6f, Shape.MaxApproxError).ToList();
        outerBoundary.Add(outerBoundary[0]);
        var innerRing = RingBorder(0.Degrees(), _outerRingRadius, false);
        innerRing.Reverse();
        outerBoundary.AddRange(innerRing);
        return outerBoundary;
    }
}
