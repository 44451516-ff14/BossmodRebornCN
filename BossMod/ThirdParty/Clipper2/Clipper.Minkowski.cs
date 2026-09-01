/*******************************************************************************
* Author    :  Angus Johnson                                                   *
* Date      :  10 October 2024                                                 *
* Website   :  https://www.angusj.com                                          *
* Copyright :  Angus Johnson 2010-2024                                         *
* Purpose   :  Minkowski Sum and Difference                                    *
* License   :  https://www.boost.org/LICENSE_1_0.txt                           *
*******************************************************************************/

using System.Buffers;

namespace Clipper2Lib;

[SkipLocalsInit]
public static class Minkowski
{
    private const int StackScratchThreshold = 128;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TranslatePattern(ReadOnlySpan<Point64> pattern, Point64 offset, bool isSum, Span<Point64> destination)
    {
        var len = pattern.Length;
        if (isSum)
        {

            for (var i = 0; i < len; ++i)
            {
                destination[i] = offset + pattern[i];
            }
        }
        else
        {
            for (var i = 0; i < len; ++i)
            {
                destination[i] = offset - pattern[i];
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsPositiveQuad(ReadOnlySpan<Point64> quad)
    {
        double area = 0;
        var previous = quad[3];
        for (var i = 0; i < 4; ++i)
        {
            var point = quad[i];
            area += (double)(previous.Y + point.Y) * (previous.X - point.X);
            previous = point;
        }
        return area * 0.5 >= 0d;
    }

    private static void AddTranslatedQuads(Clipper64 clipper, ReadOnlySpan<Point64> pattern,
      ReadOnlySpan<Point64> path, bool isSum, int delta, Span<Point64> previous, Span<Point64> current)
    {
        Span<Point64> quad = stackalloc Point64[4];
        var lenPath = path.Length;
        var lenPattern = lenPath;
        var g = delta == 0 ? lenPath - 1 : 0;
        TranslatePattern(pattern, path[g], isSum, previous);
        for (var i = delta; i < lenPath; ++i)
        {
            TranslatePattern(pattern, path[i], isSum, current);
            var h = pattern.Length - 1;
            for (var j = 0; j < lenPattern; ++j)
            {
                quad[0] = previous[h];
                quad[1] = current[h];
                quad[2] = current[j];
                quad[3] = previous[j];
                if (!IsPositiveQuad(quad))
                {
                    quad.Reverse();
                }
                clipper.AddPath(quad, PathType.Subject);
                h = j;
            }
            var swap = previous;
            previous = current;
            current = swap;
        }
    }

    private static Paths64 ExecuteMinkowski(ReadOnlySpan<Point64> pattern, ReadOnlySpan<Point64> path, bool isSum, int delta, int quadCount,
      Span<Point64> previous, Span<Point64> current)
    {
        Paths64 result = [];
        var clipper = Clipper.RentClipper64();
        try
        {
            clipper.EnsureVertexCapacity(quadCount * 4);
            AddTranslatedQuads(clipper, pattern, path, isSum, delta, previous, current);
            clipper.Execute(ClipType.Union, FillRule.NonZero, result);
        }
        finally
        {
            Clipper.ReturnClipper64(clipper);
        }
        return result;
    }

    private static Paths64 MinkowskiInternal(Path64 pattern, Path64 path, bool isSum, bool isClosed)
    {
        var delta = isClosed ? 0 : 1;
        int patLen = pattern.Count, pathLen = path.Count;
        var quadCount = (pathLen - delta) * patLen;

        if (patLen == 0 || pathLen <= delta)
        {
            return [];
        }

        var pathPoints = CollectionsMarshal.AsSpan(path);
        var patternPoints = CollectionsMarshal.AsSpan(pattern);
        if (patLen <= StackScratchThreshold)
        {
            Span<Point64> scratch = stackalloc Point64[patLen * 2];
            return ExecuteMinkowski(patternPoints, pathPoints, isSum, delta, quadCount,
              scratch[..patLen], scratch[patLen..]);
        }

        var rentedPrevious = ArrayPool<Point64>.Shared.Rent(patLen);
        Point64[]? rentedCurrent = null;
        try
        {
            rentedCurrent = ArrayPool<Point64>.Shared.Rent(patLen);
            return ExecuteMinkowski(patternPoints, pathPoints, isSum, delta, quadCount,
              rentedPrevious.AsSpan(0, patLen), rentedCurrent!.AsSpan(0, patLen));
        }
        finally
        {
            ArrayPool<Point64>.Shared.Return(rentedPrevious);
            if (rentedCurrent != null)
            {
                ArrayPool<Point64>.Shared.Return(rentedCurrent);
            }
        }
    }

    public static Paths64 Sum(Path64 pattern, Path64 path, bool isClosed)
    {
        return MinkowskiInternal(pattern, path, true, isClosed);
    }

    public static PathsD Sum(PathD pattern, PathD path, bool isClosed, int decimalPlaces = 2)
    {
        var scale = Math.Pow(10, decimalPlaces);
        var tmp = MinkowskiInternal(Clipper.ScalePath64(pattern, scale), Clipper.ScalePath64(path, scale), true, isClosed);
        return Clipper.ScalePathsD(tmp, 1 / scale);
    }

    public static Paths64 Diff(Path64 pattern, Path64 path, bool isClosed)
    {
        return MinkowskiInternal(pattern, path, false, isClosed);
    }

    public static PathsD Diff(PathD pattern, PathD path, bool isClosed, int decimalPlaces = 2)
    {
        var scale = Math.Pow(10, decimalPlaces);
        var tmp = MinkowskiInternal(Clipper.ScalePath64(pattern, scale), Clipper.ScalePath64(path, scale), false, isClosed);
        return Clipper.ScalePathsD(tmp, 1d / scale);
    }
}
