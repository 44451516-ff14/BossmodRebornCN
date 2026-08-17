namespace BossMod;

[SkipLocalsInit]
public static class ArrayListWrapper<T>
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_items")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static extern ref T[] Items(List<T> list);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_size")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static extern ref int Size(List<T> list);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<T> Wrap(T[] array)
    {
        var list = new List<T>();

        Items(list) = array;
        Size(list) = array.Length;

        return list;
    }
}
