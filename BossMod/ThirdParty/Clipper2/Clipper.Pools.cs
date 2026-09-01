namespace Clipper2Lib;

// Clipper's sweep builds several short-lived object graphs for every operation
// Keeping their storage on the owning engine avoids repeatedly allocating and collecting the same high-water-mark set of nodes
[SkipLocalsInit]
internal abstract class ClipperObjectPool<T> where T : class
{
    private const int DefaultCapacity = 8;
    protected T?[] _items = [];
    protected int _size;

    public int Count => _size;

    public T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((uint)index >= (uint)_size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return GetSlot(index)!;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private protected ref T? GetSlot(int index)
    {
        return ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_items), index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int AllocateSlot()
    {
        var slot = _size;
        if ((uint)slot >= (uint)_items.Length)
        {
            Grow(slot + 1);
        }
        _size = slot + 1;
        return slot;
    }

    public void EnsureCapacity(int minimum)
    {
        if (minimum > _items.Length)
        {
            Grow(minimum);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int minimum)
    {
        var len = _items.Length;
        var capacity = len == 0 ? DefaultCapacity : len <= Array.MaxLength / 2 ? len * 2 : Array.MaxLength;
        if (capacity < minimum)
        {
            capacity = minimum;
        }
        Array.Resize(ref _items, capacity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual void Clear() => _size = 0;
}

[SkipLocalsInit]
internal sealed class VertexPoolList : ClipperObjectPool<Vertex>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vertex Add(Point64 point, VertexFlags flags, Vertex? previous)
    {
        var slot = AllocateSlot();
        ref var vertexSlot = ref GetSlot(slot);
        var vertex = vertexSlot;
        if (vertex == null)
        {
            vertex = new Vertex(point, flags, previous);
            vertexSlot = vertex;
        }
        else
        {
            vertex.pt = point;
            vertex.flags = flags;
            vertex.prev = previous;
            vertex.next = null;
        }
        return vertex;
    }
}

[SkipLocalsInit]
internal sealed class OutPtPoolList : ClipperObjectPool<OutPt>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public OutPt Add(Point64 point, OutRec outrec)
    {
        var slot = AllocateSlot();
        ref var outPtSlot = ref GetSlot(slot);
        var outPt = outPtSlot;
        if (outPt == null)
        {
            outPt = new OutPt(point, outrec);
            outPtSlot = outPt;
        }
        else
        {
            outPt.pt = point;
            outPt.outrec = outrec;
            outPt.next = outPt;
            outPt.prev = outPt;
            outPt.horz = null;
        }
        ++outrec.outPtCount;
        return outPt;
    }
}

[SkipLocalsInit]
internal sealed class OutRecPoolList : ClipperObjectPool<OutRec>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public OutRec Add()
    {
        var slot = AllocateSlot();
        ref var outrecSlot = ref GetSlot(slot);
        var outrec = outrecSlot;
        if (outrec == null)
        {
            outrec = new OutRec();
            outrecSlot = outrec;
        }
        else
        {
            outrec.outPtCount = 0;
            outrec.bounds = default;
            outrec.isOpen = false;
            outrec.splits?.Clear();
        }
        return outrec;
    }

    public override void Clear()
    {
        // Detach result trees and paths that may outlive the engine. Pool entries retain only their own reusable storage after an operation completes
        for (var i = 0; i < _size; ++i)
        {
            var outrec = GetSlot(i)!;
            outrec.owner = null;
            outrec.frontEdge = null;
            outrec.backEdge = null;
            outrec.pts = null;
            outrec.polypath = null;
            outrec.path = null;
            outrec.recursiveSplit = null;
        }
        _size = 0;
    }
}

[SkipLocalsInit]
internal sealed class HorzSegmentPoolList : ClipperObjectPool<HorzSegment>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HorzSegment Add(OutPt outPt)
    {
        var slot = AllocateSlot();
        ref var segmentSlot = ref GetSlot(slot);
        var segment = segmentSlot;
        if (segment == null)
        {
            segment = new HorzSegment(outPt);
            segmentSlot = segment;
        }
        else
        {
            segment.leftOp = outPt;
            segment.rightOp = null;
            segment.leftToRight = true;
        }
        return segment;
    }

    public void Sort(IComparer<HorzSegment?> comparer) => Array.Sort(_items, 0, _size, comparer);
}

[SkipLocalsInit]
internal sealed class HorzJoinPoolList : ClipperObjectPool<HorzJoin>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HorzJoin Add(OutPt leftToRight, OutPt rightToLeft)
    {
        var slot = AllocateSlot();
        ref var joinSlot = ref GetSlot(slot);
        var join = joinSlot;
        if (join == null)
        {
            join = new HorzJoin(leftToRight, rightToLeft);
            joinSlot = join;
        }
        else
        {
            join.op1 = leftToRight;
            join.op2 = rightToLeft;
        }
        return join;
    }
}

[SkipLocalsInit]
internal sealed class OutPt2PoolList : ClipperObjectPool<OutPt2>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public OutPt2 Add(Point64 point)
    {
        var slot = AllocateSlot();
        ref var outPtSlot = ref GetSlot(slot);
        var outPt = outPtSlot;
        if (outPt == null)
        {
            outPt = new OutPt2(point);
            outPtSlot = outPt;
        }
        else
        {
            outPt.pt = point;
            outPt.next = null;
            outPt.prev = null;
            outPt.ownerIdx = 0;
            outPt.edge = null;
        }
        return outPt;
    }
}
