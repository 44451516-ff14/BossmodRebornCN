namespace BossMod;

public sealed class SDIntersection : ShapeDistance // max distance func
{
    private readonly ShapeDistance[] zones;
    private readonly int length;

    public SDIntersection(ShapeDistance[] Zones)
    {
        zones = Zones;
        length = zones.Length;
    }

    public override float Distance(WPos p)
    {
        var array = zones;
        var max = float.MinValue;
        var point = p;

        for (var i = 0; i < length; ++i)
        {
            var d = array[i].Distance(point);
            if (d > max)
            {
                max = d;
            }
        }
        return max;
    }
}

public sealed class SDUnion : ShapeDistance // min distance func
{
    private readonly ShapeDistance[] zones;
    private readonly int length;

    public SDUnion(ShapeDistance[] Zones)
    {
        zones = Zones;
        length = zones.Length;
    }

    public override float Distance(WPos p)
    {
        var array = zones;
        var min = float.MaxValue;
        var point = p;

        for (var i = 0; i < length; ++i)
        {
            var d = array[i].Distance(point);
            if (d < min)
            {
                min = d;
            }
        }
        return min;
    }
}

public sealed class SDInvertedUnion : ShapeDistance // -min distance func
{
    private readonly ShapeDistance[] zones;
    private readonly int length;

    public SDInvertedUnion(ShapeDistance[] Zones)
    {
        zones = Zones;
        length = zones.Length;
    }

    public override float Distance(WPos p)
    {
        var array = zones;
        var min = float.MaxValue;
        var point = p;

        for (var i = 0; i < length; ++i)
        {
            var d = array[i].Distance(point);
            if (d < min)
            {
                min = d;
            }
        }
        return -min;
    }
}

public sealed class SDInvertedUnionOffset : ShapeDistance // -min distance func
{
    private readonly ShapeDistance[] zones;
    private readonly int length;
    private readonly float offset;

    public SDInvertedUnionOffset(ShapeDistance[] Zones, float Offset)
    {
        zones = Zones;
        length = zones.Length;
        offset = Offset;
    }

    public override float Distance(WPos p)
    {
        var array = zones;
        var min = float.MaxValue;
        var point = p;

        for (var i = 0; i < length; ++i)
        {
            var d = array[i].Distance(point);
            if (d < min)
            {
                min = d;
            }
        }
        return -min + offset;
    }
}

public sealed class SDXor : ShapeDistance
{
    private readonly ShapeDistance[] zones;
    private readonly int length;
    public SDXor(ShapeDistance[] Zones)
    {
        zones = Zones;
        length = zones.Length;
    }

    public override float Distance(WPos p)
    {
        var insideCount = 0;
        var minAbs = float.MaxValue;

        var array = zones;
        for (var i = 0; i < length; ++i)
        {
            var d = array[i].Distance(p);
            if (d < 0f)
            {
                ++insideCount;
            }

            var ad = Math.Abs(d);
            if (ad < minAbs)
            {
                minAbs = ad;
            }
        }

        // If nothing contributed, treat as empty (outside everywhere)
        if (minAbs == float.MaxValue)
        {
            return float.MaxValue;
        }

        // Odd parity => inside => negative; even parity => outside => positive
        var odd = (insideCount & 1) == 1;
        return odd ? -minAbs : minAbs;
    }
}

// special XOR SD that considers multiple overlaps as a single overlap
public sealed class SDXORExactlyOne : ShapeDistance
{
    private readonly ShapeDistance[] zones;
    private readonly int length;

    public SDXORExactlyOne(ShapeDistance[] Zones)
    {
        zones = Zones;
        length = zones.Length;
    }

    public override float Distance(WPos p)
    {
        var insideCount = 0;
        var minAbs = float.MaxValue;
        var array = zones;
        for (var i = 0; i < length; ++i)
        {
            var d = array[i].Distance(p);
            if (d < 0f)
            {
                ++insideCount;
            }

            var ad = Math.Abs(d);
            if (ad < minAbs)
            {
                minAbs = ad;
            }
        }

        // If nothing contributed, treat as empty (outside everywhere)
        if (minAbs == float.MaxValue)
        {
            return float.MaxValue;
        }

        // Exactly one shape contains the point => inside (negative)
        return (insideCount == 1) ? -minAbs : minAbs;
    }
}

public sealed class SDInvertedXORExactlyOne : ShapeDistance
{
    private readonly ShapeDistance[] zones;
    private readonly int length;

    public SDInvertedXORExactlyOne(ShapeDistance[] Zones)
    {
        zones = Zones;
        length = zones.Length;
    }

    public override float Distance(WPos p)
    {
        var insideCount = 0;
        var minAbs = float.MaxValue;
        var array = zones;
        for (var i = 0; i < length; ++i)
        {
            var d = array[i].Distance(p);
            if (d < 0f)
            {
                ++insideCount;
            }

            var ad = Math.Abs(d);
            if (ad < minAbs)
            {
                minAbs = ad;
            }
        }

        // If nothing contributed, treat as empty (outside everywhere)
        if (minAbs == float.MaxValue)
        {
            return float.MaxValue;
        }

        // Exactly one shape contains the point => outside (positive)
        return (insideCount == 1) ? minAbs : -minAbs;
    }
}
