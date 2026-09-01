using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace BossMod;

// a complex polygon that is a single simple-polygon exterior minus 0 or more simple-polygon holes; all edges are assumed to be non intersecting
// hole-starts list contains starting index of each hole
[SkipLocalsInit]
public sealed class RelPolygonWithHoles(List<WDir> vertices, List<int> holeStarts)
{
    // constructor for simple polygon
    public readonly List<WDir> Vertices = vertices;
    public readonly List<int> HoleStarts = holeStarts;
    public RelPolygonWithHoles(List<WDir> simpleVertices) : this(simpleVertices, []) { }
    public ReadOnlySpan<WDir> AllVertices => CollectionsMarshal.AsSpan(Vertices);
    public ReadOnlySpan<WDir> Exterior => AllVertices[..ExteriorEnd];
    public ReadOnlySpan<WDir> Interior(int index) => AllVertices[HoleStarts[index]..HoleEnd(index)];

    private int ExteriorEnd => HoleStarts.Count > 0 ? HoleStarts[0] : Vertices.Count;
    private int HoleEnd(int index) => index + 1 < HoleStarts.Count ? HoleStarts[index + 1] : Vertices.Count;

    // add new hole; input is assumed to be a simple polygon
    public void AddHole(List<WDir> simpleHole)
    {
        HoleStarts.Add(Vertices.Count);
        Vertices.AddRange(simpleHole);
    }

    // build a new polygon by transformation
    public RelPolygonWithHoles Transform(WDir offset, WDir rotation)
    {
        var count = Vertices.Count;
        var newVerts = new List<WDir>(count);
        CollectionsMarshal.SetCount(newVerts, count);

        var src = CollectionsMarshal.AsSpan(Vertices);
        var dst = CollectionsMarshal.AsSpan(newVerts);

        if (Avx.IsSupported && count >= 4)
        {
            TransformAVX(src, dst, offset, rotation);
        }
        else
        {
            for (var i = 0; i < count; ++i)
            {
                dst[i] = src[i].Rotate(rotation) + offset;
            }
        }

        return new RelPolygonWithHoles(newVerts, [.. HoleStarts]);
    }

    private static void TransformAVX(ReadOnlySpan<WDir> src, Span<WDir> dst, WDir offset, WDir rotation)
    {
        // WDir is two contiguous floats, so a Vector256<float> contains four vertices:
        // [x0, z0, x1, z1, x2, z2, x3, z3]
        var srcFloats = MemoryMarshal.Cast<WDir, float>(src);
        var dstFloats = MemoryMarshal.Cast<WDir, float>(dst);

        var rotationX = rotation.X;
        var offsetX = offset.X;
        var offsetZ = offset.Z;
        var cos = Vector256.Create(rotation.Z);
        var sinPattern = Vector256.Create(rotationX, -rotationX, rotationX, -rotationX, rotationX, -rotationX, rotationX, -rotationX);
        var translation = Vector256.Create(offsetX, offsetZ, offsetX, offsetZ, offsetX, offsetZ, offsetX, offsetZ);

        ref var srcRef = ref MemoryMarshal.GetReference(srcFloats);
        ref var dstRef = ref MemoryMarshal.GetReference(dstFloats);
        var count = Vector256<float>.Count;
        var simdFloatCount = srcFloats.Length & -count;
        var i = 0;

        for (; i < simdFloatCount; i += count)
        {
            var v = Vector256.LoadUnsafe(ref srcRef, (nuint)i);
            // Swap x/z in every WDir pair: [x,z,x,z] -> [z,x,z,x].
            var swapped = Avx.Permute(v, 0b10_11_00_01);

            // WDir.Rotate(dir):
            // x' = x * dir.Z + z * dir.X
            // z' = z * dir.Z - x * dir.X
            if (Fma.IsSupported)
            {
                var transformed = Fma.MultiplyAdd(swapped, sinPattern, Fma.MultiplyAdd(v, cos, translation));

                transformed.StoreUnsafe(ref dstRef, (nuint)i);
            }
            else
            {
                var rotated = Avx.Add(Avx.Multiply(v, cos), Avx.Multiply(swapped, sinPattern));

                Avx.Add(rotated, translation).StoreUnsafe(ref dstRef, (nuint)i);
            }
        }

        // Scalar tail: each vertex is two floats, so i is always on a WDir boundary
        var len = src.Length;
        for (var vertex = i >> 1; vertex < len; ++vertex)
        {
            dst[vertex] = src[vertex].Rotate(rotation) + offset;
        }
    }
}
