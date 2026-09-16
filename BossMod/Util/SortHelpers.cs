namespace BossMod;

public static class SortHelpers
{
    private static readonly ActorIDComparer actorIDComparer = new();
    private static readonly ActorSlotIDComparer actorSlotIDComparer = new();
    private static readonly AOEActorIDComparer aoeActorIDComparer = new();
    private static readonly AOEActivationComparer aOEActivationComparer = new();
    private static readonly KnockbackActivationComparer knockbackActivationComparer = new();
    private static readonly EyeActivationComparer eyeActivationComparer = new();
    private static readonly ForbiddenZonesActivationComparer forbiddenZonesActivationComparer = new();
    private static readonly ForbiddenDirectionActivationComparer forbiddenDirectionActivationComparer = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SortAOEsByActorID(List<Components.GenericAOEs.AOEInstance> list) => RefSort.Sort(CollectionsMarshal.AsSpan(list), aoeActorIDComparer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SortActorsByID(List<Actor> list) => RefSort.Sort(CollectionsMarshal.AsSpan(list), actorIDComparer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SortActorsSlotByID(List<(int, Actor)> list) => RefSort.Sort(CollectionsMarshal.AsSpan(list), actorSlotIDComparer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SortAOEsByActorIDDescending(List<Components.GenericAOEs.AOEInstance> list)
        => RefSort.Sort(CollectionsMarshal.AsSpan(list), new ReverseComparer<AOEActorIDComparer, Components.GenericAOEs.AOEInstance>(aoeActorIDComparer));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SortActorsByIDDescending(List<Actor> list)
        => RefSort.Sort(CollectionsMarshal.AsSpan(list), new ReverseComparer<ActorIDComparer, Actor>(actorIDComparer));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SortAOEByActivation(List<Components.GenericAOEs.AOEInstance> list)
        => RefSort.Sort(CollectionsMarshal.AsSpan(list), aOEActivationComparer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SortEyesByActivation(List<Components.GenericGaze.Eye> list)
        => RefSort.Sort(CollectionsMarshal.AsSpan(list), eyeActivationComparer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SortKnockbacksByActivation(List<Components.GenericKnockback.Knockback> list)
        => RefSort.Sort(CollectionsMarshal.AsSpan(list), knockbackActivationComparer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SortForbiddenZonesByActivation(List<(ShapeDistance, DateTime, ulong)> list)
        => RefSort.Sort(CollectionsMarshal.AsSpan(list), forbiddenZonesActivationComparer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SortForbiddenDirectionsByActivation(List<(Angle, Angle, DateTime)> list)
        => RefSort.Sort(CollectionsMarshal.AsSpan(list), forbiddenDirectionActivationComparer);

    private readonly struct ActorIDComparer : IRefComparer<Actor>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(ref Actor a, ref Actor b) => a.InstanceID.CompareTo(b.InstanceID);
    }

    private readonly struct ActorSlotIDComparer : IRefComparer<(int, Actor)>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(ref (int, Actor) a, ref (int, Actor) b) => a.Item2.InstanceID.CompareTo(b.Item2.InstanceID);
    }

    private readonly struct AOEActorIDComparer : IRefComparer<Components.GenericAOEs.AOEInstance>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(ref Components.GenericAOEs.AOEInstance a, ref Components.GenericAOEs.AOEInstance b) => a.ActorID.CompareTo(b.ActorID);
    }

    private readonly struct AOEActivationComparer : IRefComparer<Components.GenericAOEs.AOEInstance>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(ref Components.GenericAOEs.AOEInstance a, ref Components.GenericAOEs.AOEInstance b) => a.Activation.CompareTo(b.Activation);
    }

    private readonly struct KnockbackActivationComparer : IRefComparer<Components.GenericKnockback.Knockback>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(ref Components.GenericKnockback.Knockback a, ref Components.GenericKnockback.Knockback b) => a.Activation.CompareTo(b.Activation);
    }

    private readonly struct EyeActivationComparer : IRefComparer<Components.GenericGaze.Eye>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(ref Components.GenericGaze.Eye a, ref Components.GenericGaze.Eye b) => a.Activation.CompareTo(b.Activation);
    }

    private readonly struct ForbiddenZonesActivationComparer : IRefComparer<(ShapeDistance, DateTime, ulong)>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(ref (ShapeDistance, DateTime, ulong) a, ref (ShapeDistance, DateTime, ulong) b) => a.Item2.CompareTo(b.Item2);
    }

    private readonly struct ForbiddenDirectionActivationComparer : IRefComparer<(Angle, Angle, DateTime)>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(ref (Angle, Angle, DateTime) a, ref (Angle, Angle, DateTime) b) => a.Item3.CompareTo(b.Item3);
    }
}

public interface IRefComparer<T>
{
    int Compare(ref T a, ref T b);
}

public readonly struct ReverseComparer<TComparer, T>(TComparer comparer) : IRefComparer<T>
    where TComparer : struct, IRefComparer<T>
{
    private readonly TComparer _comparer = comparer;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Compare(ref T a, ref T b) => _comparer.Compare(ref b, ref a);
}

public static class RefSort
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort<T, TComparer>(Span<T> span, TComparer comparer) where TComparer : struct, IRefComparer<T>
        => QuickSort(span, 0, span.Length - 1, comparer);

    private static void QuickSort<T, TComparer>(Span<T> span, int left, int right, TComparer comparer) where TComparer : struct, IRefComparer<T>
    {
        while (left < right)
        {
            var pivot = Partition(span, left, right, comparer);

            if (pivot - left < right - pivot)
            {
                QuickSort(span, left, pivot - 1, comparer);
                left = pivot + 1;
            }
            else
            {
                QuickSort(span, pivot + 1, right, comparer);
                right = pivot - 1;
            }
        }
    }

    private static int Partition<T, TComparer>(Span<T> span, int left, int right, TComparer comparer) where TComparer : struct, IRefComparer<T>
    {
        ref var pivot = ref span[right];
        var storeIndex = left;

        for (var i = left; i < right; ++i)
        {
            if (comparer.Compare(ref span[i], ref pivot) < 0)
            {
                if (storeIndex != i)
                {
                    Swap(ref span[storeIndex], ref span[i]);
                }
                ++storeIndex;
            }
        }

        Swap(ref span[storeIndex], ref span[right]);
        return storeIndex;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Swap<T>(ref T a, ref T b) => (b, a) = (a, b);
}
