namespace BossMod;

// shapes can be defined by distance from point to shape's border; distance is positive for points outside shape and negative for points inside shape
// union is min, intersection is max

public static class ShapeDistance
{
    private static readonly Angle a90 = 90f.Degrees();

    public static Func<WPos, float> HalfPlane(WPos point, WDir normal)
    {
        var normalX = normal.X;
        var normalZ = normal.Z;
        var pointX = point.X;
        var pointZ = point.Z;
        return p => normalX * (p.X - pointX) + normalZ * (p.Z - pointZ);
    }

    public static Func<WPos, float> Circle(WPos origin, float radius)
    {
        var originX = origin.X;
        var originZ = origin.Z;
        return radius <= 0f ? (_ => float.MaxValue) : (p =>
        {
            var pXoriginX = p.X - originX;
            var pZoriginZ = p.Z - originZ;
            return MathF.Sqrt(pXoriginX * pXoriginX + pZoriginZ * pZoriginZ) - radius;
        });
    }

    public static Func<WPos, float> InvertedCircle(WPos origin, float radius)
    {
        var originX = origin.X;
        var originZ = origin.Z;
        return radius <= 0f ? (_ => float.MinValue) : (p =>
        {
            var pXoriginX = p.X - originX;
            var pZoriginZ = p.Z - originZ;
            return radius - MathF.Sqrt(pXoriginX * pXoriginX + pZoriginZ * pZoriginZ);
        });
    }

    public static Func<WPos, float> Donut(WPos origin, float innerRadius, float outerRadius)
    {
        var originX = origin.X;
        var originZ = origin.Z;
        return outerRadius <= 0f || innerRadius >= outerRadius ? (_ => float.MaxValue) : innerRadius <= 0f ? Circle(origin, outerRadius) : (p =>
        {
            // intersection of outer circle and inverted inner circle
            var pXoriginX = p.X - originX;
            var pZoriginZ = p.Z - originZ;
            var distOrigin = MathF.Sqrt(pXoriginX * pXoriginX + pZoriginZ * pZoriginZ);
            var distSqOuter = distOrigin - outerRadius;
            var distSqInner = innerRadius - distOrigin;
            return distSqOuter > distSqInner ? distSqOuter : distSqInner;
        });
    }

    public static Func<WPos, float> InvertedDonut(WPos origin, float innerRadius, float outerRadius)
    {
        var originX = origin.X;
        var originZ = origin.Z;
        return outerRadius <= 0f || innerRadius >= outerRadius ? (_ => float.MaxValue) : innerRadius <= 0f ? Circle(origin, outerRadius) : (p =>
        {
            // intersection of outer circle and inverted inner circle
            var pXoriginX = p.X - originX;
            var pZoriginZ = p.Z - originZ;
            var distOrigin = MathF.Sqrt(pXoriginX * pXoriginX + pZoriginZ * pZoriginZ);
            var distOuter = distOrigin - outerRadius;
            var distInner = innerRadius - distOrigin;
            return distOuter > distInner ? -distOuter : -distInner;
        });
    }

    public static Func<WPos, float> Cone(WPos origin, float radius, Angle centerDir, Angle halfAngle)
    {
        if (halfAngle.Rad <= 0f || radius <= 0f)
            return _ => float.MaxValue;
        if (halfAngle.Rad >= MathF.PI)
            return Circle(origin, radius);
        // for <= 180-degree cone: result = intersection of circle and two half-planes with normals pointing outside cone sides
        // for > 180-degree cone: result = intersection of circle and negated intersection of two half-planes with inverted normals
        // both normals point outside
        var coneFactor = halfAngle.Rad > Angle.HalfPi ? -1f : 1f;
        var nl = coneFactor * (centerDir + halfAngle).ToDirection().OrthoL();
        var nr = coneFactor * (centerDir - halfAngle).ToDirection().OrthoR();
        var originX = origin.X;
        var originZ = origin.Z;
        var nlX = nl.X;
        var nlZ = nl.Z;
        var nrX = nr.X;
        var nrZ = nr.Z;
        return p =>
        {
            var pXoriginX = p.X - originX;
            var pZoriginZ = p.Z - originZ;
            var distOrigin = MathF.Sqrt(pXoriginX * pXoriginX + pZoriginZ * pZoriginZ);
            var distOuter = distOrigin - radius;
            var distLeft = pXoriginX * nlX + pZoriginZ * nlZ;
            var distRight = pXoriginX * nrX + pZoriginZ * nrZ;

            var maxSideDist = distLeft > distRight ? distLeft : distRight;
            var conef = coneFactor * maxSideDist;
            return distOuter > conef ? distOuter : conef;
        };
    }

    public static Func<WPos, float> InvertedCone(WPos origin, float radius, Angle centerDir, Angle halfAngle)
    {
        if (halfAngle.Rad <= 0f || radius <= 0f)
            return _ => float.MaxValue;
        if (halfAngle.Rad >= MathF.PI)
            return Circle(origin, radius);
        // for <= 180-degree cone: result = intersection of circle and two half-planes with normals pointing outside cone sides
        // for > 180-degree cone: result = intersection of circle and negated intersection of two half-planes with inverted normals
        // both normals point outside
        var coneFactor = halfAngle.Rad > Angle.HalfPi ? -1f : 1f;
        var nl = coneFactor * (centerDir + halfAngle).ToDirection().OrthoL();
        var nr = coneFactor * (centerDir - halfAngle).ToDirection().OrthoR();
        var originX = origin.X;
        var originZ = origin.Z;
        var nlX = nl.X;
        var nlZ = nl.Z;
        var nrX = nr.X;
        var nrZ = nr.Z;
        return p =>
        {
            var pXoriginX = p.X - originX;
            var pZoriginZ = p.Z - originZ;
            var distOrigin = MathF.Sqrt(pXoriginX * pXoriginX + pZoriginZ * pZoriginZ);
            var distOuter = distOrigin - radius;
            var distLeft = pXoriginX * nlX + pZoriginZ * nlZ;
            var distRight = pXoriginX * nrX + pZoriginZ * nrZ;

            var maxSideDist = distLeft > distRight ? distLeft : distRight;
            var conef = coneFactor * maxSideDist;
            return distOuter > conef ? -distOuter : -conef;
        };
    }

    public static Func<WPos, float> DonutSector(WPos origin, float innerRadius, float outerRadius, Angle centerDir, Angle halfAngle)
    {
        if (halfAngle.Rad <= 0f || outerRadius <= 0f || innerRadius >= outerRadius)
            return _ => float.MaxValue;

        if (halfAngle.Rad >= MathF.PI)
            return Donut(origin, innerRadius, outerRadius);

        if (innerRadius <= 0f)
            return Cone(origin, outerRadius, centerDir, halfAngle);

        var coneFactor = halfAngle.Rad > Angle.HalfPi ? -1f : 1f;
        var nl = coneFactor * (centerDir + halfAngle + a90).ToDirection();
        var nr = coneFactor * (centerDir - halfAngle - a90).ToDirection();
        var originX = origin.X;
        var originZ = origin.Z;
        var nlX = nl.X;
        var nlZ = nl.Z;
        var nrX = nr.X;
        var nrZ = nr.Z;

        return p =>
        {
            var pXoriginX = p.X - originX;
            var pZoriginZ = p.Z - originZ;
            var distOrigin = MathF.Sqrt(pXoriginX * pXoriginX + pZoriginZ * pZoriginZ);
            var distOuter = distOrigin - outerRadius;
            var distInner = innerRadius - distOrigin;
            var distLeft = pXoriginX * nlX + pZoriginZ * nlZ;
            var distRight = pXoriginX * nrX + pZoriginZ * nrZ;

            var maxRadial = distOuter > distInner ? distOuter : distInner;
            var maxCone = distLeft > distRight ? distLeft : distRight;
            var conef = coneFactor * maxCone;
            return maxRadial > conef ? maxRadial : conef;
        };
    }

    public static Func<WPos, float> InvertedDonutSector(WPos origin, float innerRadius, float outerRadius, Angle centerDir, Angle halfAngle)
    {
        if (halfAngle.Rad <= 0f || outerRadius <= 0f || innerRadius >= outerRadius)
            return _ => float.MaxValue;

        if (halfAngle.Rad >= MathF.PI)
            return Donut(origin, innerRadius, outerRadius);

        if (innerRadius <= 0f)
            return Cone(origin, outerRadius, centerDir, halfAngle);

        var coneFactor = halfAngle.Rad > Angle.HalfPi ? -1f : 1f;
        var nl = coneFactor * (centerDir + halfAngle + a90).ToDirection();
        var nr = coneFactor * (centerDir - halfAngle - a90).ToDirection();
        var originX = origin.X;
        var originZ = origin.Z;
        var nlX = nl.X;
        var nlZ = nl.Z;
        var nrX = nr.X;
        var nrZ = nr.Z;

        return p =>
        {
            var pXoriginX = p.X - originX;
            var pZoriginZ = p.Z - originZ;
            var distOrigin = MathF.Sqrt(pXoriginX * pXoriginX + pZoriginZ * pZoriginZ);
            var distOuter = distOrigin - outerRadius;
            var distInner = innerRadius - distOrigin;
            var distLeft = pXoriginX * nlX + pZoriginZ * nlZ;
            var distRight = pXoriginX * nrX + pZoriginZ * nrZ;

            var maxRadial = distOuter > distInner ? distOuter : distInner;
            var maxCone = distLeft > distRight ? distLeft : distRight;
            var conef = coneFactor * maxCone;
            return maxRadial > conef ? -maxRadial : -conef;
        };
    }

    public static Func<WPos, float> Tri(WPos origin, RelTriangle tri)
    {
        var ab = tri.B - tri.A;
        var bc = tri.C - tri.B;
        var ca = tri.A - tri.C;
        var n1 = ab.OrthoL().Normalized();
        var n2 = bc.OrthoL().Normalized();
        var n3 = ca.OrthoL().Normalized();
        if (ab.Cross(bc) < 0)
        {
            n1 = -n1;
            n2 = -n2;
            n3 = -n3;
        }
        var a = origin + tri.A;
        var b = origin + tri.B;
        var c = origin + tri.C;

        var n1X = n1.X;
        var n1Z = n1.Z;
        var n2X = n2.X;
        var n2Z = n2.Z;
        var n3X = n3.X;
        var n3Z = n3.Z;
        var aX = a.X;
        var aZ = a.Z;
        var bX = b.X;
        var bZ = b.Z;
        var cX = c.X;
        var cZ = c.Z;

        return p =>
        {
            var d1 = n1X * (p.X - aX) + n1Z * (p.Z - aZ);
            var d2 = n2X * (p.X - bX) + n2Z * (p.Z - bZ);
            var d3 = n3X * (p.X - cX) + n3Z * (p.Z - cZ);
            var max1 = d1 > d2 ? d1 : d2;
            return max1 > d3 ? max1 : d3;
        };
    }

    public static Func<WPos, float> InvertedTri(WPos origin, RelTriangle tri)
    {
        var ab = tri.B - tri.A;
        var bc = tri.C - tri.B;
        var ca = tri.A - tri.C;
        var n1 = ab.OrthoL().Normalized();
        var n2 = bc.OrthoL().Normalized();
        var n3 = ca.OrthoL().Normalized();
        if (ab.Cross(bc) < 0)
        {
            n1 = -n1;
            n2 = -n2;
            n3 = -n3;
        }
        var a = origin + tri.A;
        var b = origin + tri.B;
        var c = origin + tri.C;

        var n1X = n1.X;
        var n1Z = n1.Z;
        var n2X = n2.X;
        var n2Z = n2.Z;
        var n3X = n3.X;
        var n3Z = n3.Z;
        var aX = a.X;
        var aZ = a.Z;
        var bX = b.X;
        var bZ = b.Z;
        var cX = c.X;
        var cZ = c.Z;

        return p =>
        {
            var d1 = n1X * (p.X - aX) + n1Z * (p.Z - aZ);
            var d2 = n2X * (p.X - bX) + n2Z * (p.Z - bZ);
            var d3 = n3X * (p.X - cX) + n3Z * (p.Z - cZ);
            var max1 = d1 > d2 ? d1 : d2;
            return max1 > d3 ? -max1 : -d3;
        };
    }

    public static Func<WPos, float> Rect(WPos origin, WDir dir, float lenFront, float lenBack, float halfWidth)
    {
        // dir points outside far side
        var normal = dir.OrthoL(); // points outside left side
        var originX = origin.X;
        var originZ = origin.Z;
        var dirX = dir.X;
        var dirZ = dir.Z;
        var normalX = normal.X;
        var normalZ = normal.Z;

        return p =>
        {
            var pXoriginX = p.X - originX;
            var pZoriginZ = p.Z - originZ;
            var distParr = pXoriginX * dirX + pZoriginZ * dirZ;
            var distOrtho = pXoriginX * normalX + pZoriginZ * normalZ;
            var distFront = distParr - lenFront;
            var distBack = -distParr - lenBack;
            var distLeft = distOrtho - halfWidth;
            var distRight = -distOrtho - halfWidth;

            var maxParr = distFront > distBack ? distFront : distBack;
            var maxOrtho = distLeft > distRight ? distLeft : distRight;

            return maxParr > maxOrtho ? maxParr : maxOrtho;
        };
    }

    public static Func<WPos, float> Rect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth)
    {
        // dir points outside far side
        var dir = direction.ToDirection();
        var normal = dir.OrthoL(); // points outside left side
        var originX = origin.X;
        var originZ = origin.Z;
        var dirX = dir.X;
        var dirZ = dir.Z;
        var normalX = normal.X;
        var normalZ = normal.Z;

        return p =>
        {
            var pXoriginX = p.X - originX;
            var pZoriginZ = p.Z - originZ;
            var distParr = pXoriginX * dirX + pZoriginZ * dirZ;
            var distOrtho = pXoriginX * normalX + pZoriginZ * normalZ;
            var distFront = distParr - lenFront;
            var distBack = -distParr - lenBack;
            var distLeft = distOrtho - halfWidth;
            var distRight = -distOrtho - halfWidth;

            var maxParr = distFront > distBack ? distFront : distBack;
            var maxOrtho = distLeft > distRight ? distLeft : distRight;

            return maxParr > maxOrtho ? maxParr : maxOrtho;
        };
    }

    public static Func<WPos, float> Rect(WPos from, WPos to, float halfWidth)
    {
        var dir = to - from;
        var l = dir.Length();
        var normalizedDir = dir / l;
        var normal = normalizedDir.OrthoL();

        var originX = from.X;
        var originZ = from.Z;
        var dirX = normalizedDir.X;
        var dirZ = normalizedDir.Z;
        var normalX = normal.X;
        var normalZ = normal.Z;

        return p =>
        {
            var pXoriginX = p.X - originX;
            var pZoriginZ = p.Z - originZ;
            var distParr = pXoriginX * dirX + pZoriginZ * dirZ;
            var distOrtho = pXoriginX * normalX + pZoriginZ * normalZ;
            var distFront = distParr - l;
            var distBack = -distParr;
            var distLeft = distOrtho - halfWidth;
            var distRight = -distOrtho - halfWidth;

            var maxParr = distFront > distBack ? distFront : distBack;
            var maxOrtho = distLeft > distRight ? distLeft : distRight;

            return maxParr > maxOrtho ? maxParr : maxOrtho;
        };
    }

    public static Func<WPos, float> InvertedRect(WPos origin, WDir dir, float lenFront, float lenBack, float halfWidth)
    {
        // dir points outside far side
        var normal = dir.OrthoL(); // points outside left side

        var originX = origin.X;
        var originZ = origin.Z;
        var dirX = dir.X;
        var dirZ = dir.Z;
        var normalX = normal.X;
        var normalZ = normal.Z;

        return p =>
        {
            var pXoriginX = p.X - originX;
            var pZoriginZ = p.Z - originZ;
            var distParr = pXoriginX * dirX + pZoriginZ * dirZ;
            var distOrtho = pXoriginX * normalX + pZoriginZ * normalZ;
            var distFront = distParr - lenFront;
            var distBack = -distParr - lenBack;
            var distLeft = distOrtho - halfWidth;
            var distRight = -distOrtho - halfWidth;

            var maxParr = distFront > distBack ? distFront : distBack;
            var maxOrtho = distLeft > distRight ? distLeft : distRight;

            return maxParr > maxOrtho ? -maxParr : -maxOrtho;
        };
    }

    public static Func<WPos, float> InvertedRect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth)
    {
        // dir points outside far side
        var dir = direction.ToDirection();
        var normal = dir.OrthoL(); // points outside left side

        var originX = origin.X;
        var originZ = origin.Z;
        var dirX = dir.X;
        var dirZ = dir.Z;
        var normalX = normal.X;
        var normalZ = normal.Z;

        return p =>
        {
            var pXoriginX = p.X - originX;
            var pZoriginZ = p.Z - originZ;
            var distParr = pXoriginX * dirX + pZoriginZ * dirZ;
            var distOrtho = pXoriginX * normalX + pZoriginZ * normalZ;
            var distFront = distParr - lenFront;
            var distBack = -distParr - lenBack;
            var distLeft = distOrtho - halfWidth;
            var distRight = -distOrtho - halfWidth;

            var maxParr = distFront > distBack ? distFront : distBack;
            var maxOrtho = distLeft > distRight ? distLeft : distRight;

            return maxParr > maxOrtho ? -maxParr : -maxOrtho;
        };
    }

    public static Func<WPos, float> InvertedRect(WPos from, WPos to, float halfWidth)
    {
        var dir = to - from;
        var l = dir.Length();
        var normalizedDir = dir / l;
        var normal = normalizedDir.OrthoL();

        var originX = from.X;
        var originZ = from.Z;
        var dirX = normalizedDir.X;
        var dirZ = normalizedDir.Z;
        var normalX = normal.X;
        var normalZ = normal.Z;

        return p =>
        {
            var pXoriginX = p.X - originX;
            var pZoriginZ = p.Z - originZ;
            var distParr = pXoriginX * dirX + pZoriginZ * dirZ;
            var distOrtho = pXoriginX * normalX + pZoriginZ * normalZ;
            var distFront = distParr - l;
            var distBack = -distParr;
            var distLeft = distOrtho - halfWidth;
            var distRight = -distOrtho - halfWidth;

            var maxParr = distFront > distBack ? distFront : distBack;
            var maxOrtho = distLeft > distRight ? distLeft : distRight;

            return maxParr > maxOrtho ? -maxParr : -maxOrtho;
        };
    }

    public static Func<WPos, float> Capsule(WPos origin, WDir dir, float length, float radius)
    {
        var originX = origin.X;
        var originZ = origin.Z;
        var dirX = dir.X;
        var dirZ = dir.Z;

        return p =>
        {
            var pXoriginX = p.X - originX;
            var pZoriginZ = p.Z - originZ;
            var t = pXoriginX * dirX + pZoriginZ * dirZ;
            t = (t < 0f) ? 0f : (t > length ? length : t);
            var proj = origin + t * dir;
            var pXprojX = p.X - proj.X;
            var pZprojZ = p.Z - proj.Z;
            return MathF.Sqrt(pXprojX * pXprojX + pZprojZ * pZprojZ) - radius;
        };
    }

    public static Func<WPos, float> Capsule(WPos origin, Angle direction, float length, float radius)
    {
        var dir = direction.ToDirection();
        var originX = origin.X;
        var originZ = origin.Z;
        var dirX = dir.X;
        var dirZ = dir.Z;

        return p =>
        {
            var pXoriginX = p.X - originX;
            var pZoriginZ = p.Z - originZ;
            var t = pXoriginX * dirX + pZoriginZ * dirZ;
            t = (t < 0f) ? 0f : (t > length ? length : t);
            var proj = origin + t * dir;
            var pXprojX = p.X - proj.X;
            var pZprojZ = p.Z - proj.Z;
            return MathF.Sqrt(pXprojX * pXprojX + pZprojZ * pZprojZ) - radius;
        };
    }

    public static Func<WPos, float> InvertedCapsule(WPos origin, WDir dir, float length, float radius)
    {
        var originX = origin.X;
        var originZ = origin.Z;
        var dirX = dir.X;
        var dirZ = dir.Z;

        return p =>
        {
            var pXoriginX = p.X - originX;
            var pZoriginZ = p.Z - originZ;
            var t = pXoriginX * dirX + pZoriginZ * dirZ;
            t = (t < 0f) ? 0f : (t > length ? length : t);
            var proj = origin + t * dir;
            var pXprojX = p.X - proj.X;
            var pZprojZ = p.Z - proj.Z;
            return radius - MathF.Sqrt(pXprojX * pXprojX + pZprojZ * pZprojZ);
        };
    }

    public static Func<WPos, float> InvertedCapsule(WPos origin, Angle direction, float length, float radius)
    {
        var dir = direction.ToDirection();
        var originX = origin.X;
        var originZ = origin.Z;
        var dirX = dir.X;
        var dirZ = dir.Z;

        return p =>
        {
            var pXoriginX = p.X - originX;
            var pZoriginZ = p.Z - originZ;
            var t = pXoriginX * dirX + pZoriginZ * dirZ;
            t = (t < 0) ? 0 : (t > length ? length : t);
            var proj = origin + t * dir;
            var pXprojX = p.X - proj.X;
            var pZprojZ = p.Z - proj.Z;
            return radius - MathF.Sqrt(pXprojX * pXprojX + pZprojZ * pZprojZ);
        };
    }

    public static Func<WPos, float> Cross(WPos origin, Angle direction, float length, float halfWidth)
    {
        var dir = direction.ToDirection();
        var normal = dir.OrthoL();
        return p =>
        {
            var offset = p - origin;
            var distParr = offset.Dot(dir);
            var distOrtho = offset.Dot(normal);
            var distPFront = distParr - length;
            var distPBack = -distParr - length;
            var distPLeft = distOrtho - halfWidth;
            var distPRight = -distOrtho - halfWidth;
            var distOFront = distOrtho - length;
            var distOBack = -distOrtho - length;
            var distOLeft = distParr - halfWidth;
            var distORight = -distParr - halfWidth;

            var distPMax1 = distPFront > distPBack ? distPFront : distPBack;
            var distPMax2 = distPLeft > distPRight ? distPLeft : distPRight;
            var distP = distPMax1 > distPMax2 ? distPMax1 : distPMax2;

            var distOMax1 = distOFront > distOBack ? distOFront : distOBack;
            var distOMax2 = distOLeft > distORight ? distOLeft : distORight;
            var distO = distOMax1 > distOMax2 ? distOMax1 : distOMax2;

            return distP < distO ? distP : distO;
        };
    }

    public static Func<WPos, float> InvertedCross(WPos origin, Angle direction, float length, float halfWidth)
    {
        var dir = direction.ToDirection();
        var normal = dir.OrthoL();
        return p =>
        {
            var offset = p - origin;
            var distParr = offset.Dot(dir);
            var distOrtho = offset.Dot(normal);
            var distPFront = distParr - length;
            var distPBack = -distParr - length;
            var distPLeft = distOrtho - halfWidth;
            var distPRight = -distOrtho - halfWidth;
            var distOFront = distOrtho - length;
            var distOBack = -distOrtho - length;
            var distOLeft = distParr - halfWidth;
            var distORight = -distParr - halfWidth;

            var distPMax1 = distPFront > distPBack ? distPFront : distPBack;
            var distPMax2 = distPLeft > distPRight ? distPLeft : distPRight;
            var distP = distPMax1 > distPMax2 ? distPMax1 : distPMax2;

            var distOMax1 = distOFront > distOBack ? distOFront : distOBack;
            var distOMax2 = distOLeft > distORight ? distOLeft : distORight;
            var distO = distOMax1 > distOMax2 ? distOMax1 : distOMax2;

            return distP < distO ? -distP : -distO;
        };
    }

    public static Func<WPos, float> ConvexPolygon((WPos, WPos)[] edges, bool cw)
    {
        return point =>
        {
            var minDistance = float.MaxValue;
            var inside = true;
            var len = edges.Length;
            for (var i = 0; i < len; ++i)
            {
                var edge = edges[i];
                var a = edge.Item1;
                var b = edge.Item2;
                var ab = b - a;
                var ap = point - a;
                var distance = (ab.X * ap.Z - ab.Z * ap.X) / ab.Length();

                if (cw && distance > 0 || !cw && distance < 0)
                {
                    inside = false;
                }
                minDistance = Math.Min(minDistance, Math.Abs(distance));
            }
            return inside ? -minDistance : minDistance;
        };
    }

    public static Func<WPos, float> ConvexPolygon(ReadOnlySpan<WPos> vertices, bool cw)
    {
        var len = vertices.Length;
        var edges = new (WPos, WPos)[len];
        for (var i = 0; i < len; ++i)
        {
            var a = vertices[i];
            var b = vertices[(i + 1) % len];
            edges[i] = (a, b);
        }
        return ConvexPolygon(edges, cw);
    }

    public static Func<WPos, float> Intersection(List<Func<WPos, float>> funcs) // max distance func
    {
        var count = funcs.Count;
        return p =>
        {
            var maxDistance = float.MinValue;
            for (var i = 0; i < count; ++i)
            {
                var distance = funcs[i](p);
                if (distance > maxDistance)
                    maxDistance = distance;
            }
            return maxDistance;
        };
    }

    public static Func<WPos, float> Intersection(Func<WPos, float>[] funcs) // max distance func
    {
        var count = funcs.Length;
        return p =>
        {
            var maxDistance = float.MinValue;
            for (var i = 0; i < count; ++i)
            {
                var distance = funcs[i](p);
                if (distance > maxDistance)
                    maxDistance = distance;
            }
            return maxDistance;
        };
    }

    public static Func<WPos, float> Union(List<Func<WPos, float>> funcs) // min distance func
    {
        var count = funcs.Count;
        return p =>
        {
            var minDistance = float.MaxValue;
            for (var i = 0; i < count; ++i)
            {
                var distance = funcs[i](p);
                if (distance < minDistance)
                    minDistance = distance;
            }
            return minDistance;
        };
    }

    public static Func<WPos, float> Union(Func<WPos, float>[] funcs) // min distance func
    {
        var len = funcs.Length;
        return p =>
        {
            var minDistance = float.MaxValue;
            for (var i = 0; i < len; ++i)
            {
                var distance = funcs[i](p);
                if (distance < minDistance)
                    minDistance = distance;
            }
            return minDistance;
        };
    }

    public static Func<WPos, float> InvertedUnion(List<Func<WPos, float>> funcs) // min distance func
    {
        var count = funcs.Count;
        return p =>
        {
            var minDistance = float.MaxValue;
            for (var i = 0; i < count; ++i)
            {
                var distance = funcs[i](p);
                if (distance < minDistance)
                    minDistance = distance;
            }
            return -minDistance;
        };
    }

    public static Func<WPos, float> InvertedUnion(Func<WPos, float>[] funcs) // min distance func
    {
        var len = funcs.Length;
        return p =>
        {
            var minDistance = float.MaxValue;
            for (var i = 0; i < len; ++i)
            {
                var distance = funcs[i](p);
                if (distance < minDistance)
                    minDistance = distance;
            }
            return -minDistance;
        };
    }

    // special distance function for precise positioning, finer than map resolution
    // it's an inverted rect of a size equal to one grid cell, with a special adjustment if starting position is in the same cell, but farther than tolerance
    public static Func<WPos, float> PrecisePosition(WPos origin, WDir dir, float cellSize, WPos starting, float tolerance = 0f)
    {
        var delta = starting - origin;
        var dparr = delta.Dot(dir);
        if (dparr > tolerance && dparr <= cellSize)
            origin -= cellSize * dir;
        else if (dparr < -tolerance && dparr >= -cellSize)
            origin += cellSize * dir;

        var normal = dir.OrthoL();
        var dortho = delta.Dot(normal);
        if (dortho > tolerance && dortho <= cellSize)
            origin -= cellSize * normal;
        else if (dortho < -tolerance && dortho >= -cellSize)
            origin += cellSize * normal;

        return InvertedRect(origin, dir, cellSize, cellSize, cellSize);
    }
}
