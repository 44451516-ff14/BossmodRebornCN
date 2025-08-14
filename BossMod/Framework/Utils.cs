using Dalamud.Game.ClientState.Objects.Types;
using PInvoke;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

namespace BossMod;

public static partial class Utils
{
    public static string ObjectString(IGameObject obj) => $"{obj.DataId:X} '{obj.Name}' <{obj.EntityId:X}>";

    public static string ObjectString(ulong id)
    {
        var obj = (id >> 32) == 0 ? Service.ObjectTable.SearchById((uint)id) : null;
        return obj != null ? ObjectString(obj) : $"(not found) <{id:X}>";
    }

    public static string ObjectKindString(IGameObject obj)
        => obj.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.BattleNpc ? $"{obj.ObjectKind}/{(Dalamud.Game.ClientState.Objects.Enums.BattleNpcSubKind)obj.SubKind}"
        : obj.SubKind == 0 ? $"{obj.ObjectKind}"
        : $"{obj.ObjectKind}/{obj.SubKind}";

    public static Vector2 XY(this Vector4 v) => new(v.X, v.Y);
    public static Vector3 XYZ(this Vector4 v) => new(v.X, v.Y, v.Z);
    public static Vector2 XZ(this Vector4 v) => new(v.X, v.Z);
    public static Vector2 XZ(this Vector3 v) => new(v.X, v.Z);
    public static Vector3 ToSystem(this FFXIVClientStructs.FFXIV.Common.Math.Vector3 v) => new(v.X, v.Y, v.Z);
    public static Vector3 ToVector3(this (float X, float Y, float Z) t) => new(t.X, t.Y, t.Z);

    public static bool AlmostEqual(float a, float b, float eps) => Math.Abs(a - b) <= eps;
    public static bool AlmostEqual(Vector3 a, Vector3 b, float eps) => (a - b).LengthSquared() <= eps * eps;

    public static string Vec3String(Vector3 pos) => $"[{pos.X:f3}, {pos.Y:f3}, {pos.Z:f3}]";
    public static string QuatString(Quaternion q) => $"[{q.X:f3}, {q.Y:f3}, {q.Z:f3}, {q.W:f3}]";
    public static string PosRotString(Vector4 posRot) => $"[{posRot.X:f3}, {posRot.Y:f3}, {posRot.Z:f3}, {posRot.W.Radians()}]";
    public static bool CharacterIsOmnidirectional(uint oid) => Service.LuminaRow<Lumina.Excel.Sheets.BNpcBase>(oid)?.IsOmnidirectional ?? false;
    public static string StatusString(uint statusID) => $"{statusID} '{Service.LuminaRow<Lumina.Excel.Sheets.Status>(statusID)?.Name ?? "<not found>"}'";
    public static string StatusTimeString(DateTime expireAt, DateTime now) => $"{Math.Max(0, (expireAt - now).TotalSeconds):f3}";
    public static string CastTimeString(float current, float total) => $"{current:f2}/{total:f2}";
    public static string CastTimeString(ActorCastInfo cast, DateTime now) => CastTimeString(cast.ElapsedTime, cast.TotalTime);
    public static string LogMessageString(uint id) => $"{id} '{Service.LuminaRow<Lumina.Excel.Sheets.LogMessage>(id)?.Text}'";

    public static readonly Func<uint, bool> StatusIsRemovable = Memoize((uint statusID) => Service.LuminaRow<Lumina.Excel.Sheets.Status>(statusID)?.CanDispel ?? false);
    public static readonly Func<uint, bool> IsBossFate = Memoize((uint fateID) => Service.LuminaRow<Lumina.Excel.Sheets.Fate>(fateID)?.MapIcon is 60502 or 60802);
    public static bool IsNonBossFate(uint fateID) => fateID > 0 && !IsBossFate(fateID);

    public static unsafe T ReadField<T>(void* address, int offset) where T : unmanaged => *(T*)((IntPtr)address + offset);
    public static unsafe void WriteField<T>(void* address, int offset, T value) where T : unmanaged => *(T*)((IntPtr)address + offset) = value;

    public static unsafe FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* GameObjectInternal(IGameObject? obj) => obj != null ? (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)obj.Address : null;
    public static unsafe FFXIVClientStructs.FFXIV.Client.Game.Character.Character* CharacterInternal(ICharacter? chr) => chr != null ? (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)chr.Address : null;
    public static unsafe FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara* BattleCharaInternal(IBattleChara? chara) => chara != null ? (FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara*)chara.Address : null;

    public static unsafe ulong SceneObjectFlags(FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object* o) => ReadField<ulong>(o, 0x38);

    public static bool IsPlayerSyncedToFate(WorldState world)
    {
        if (world.Client.ActiveFate.ID == 0)
            return false;

        var fate = GetFateData(world.Client.ActiveFate.ID);
        return fate.EurekaFate == 1
            ? world.Client.ElementalLevelSynced <= fate.ClassJobLevelMax
            : world.Party.Player()?.Level <= fate.ClassJobLevelMax;
    }

    private static readonly Dictionary<uint, (byte, byte)> _fateCache = [];
    private static (byte ClassJobLevelMax, byte EurekaFate) GetFateData(uint fateID)
    {
        if (_fateCache.TryGetValue(fateID, out var fateRow))
            return fateRow;
        var row = Service.LuminaRow<Lumina.Excel.Sheets.Fate>(fateID);
        (byte, byte)? data;
        data = (row!.Value.ClassJobLevelMax, row.Value.EurekaFate);
        return _fateCache[fateID] = data.Value;
    }

    // lumina extensions
    public static int FindIndex<T>(this Lumina.Excel.Collection<T> collection, Func<T, bool> predicate) where T : struct
    {
        for (int i = 0; i < collection.Count; ++i)
            if (predicate(collection[i]))
                return i;
        return -1;
    }

    // backport from .net 6, except that it doesn't throw on empty enumerable...
    public static TSource? MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) where TKey : IComparable
    {
        using var e = source.GetEnumerator();
        if (!e.MoveNext())
            return default;

        var res = e.Current;
        var score = keySelector(res);
        while (e.MoveNext())
        {
            var cur = e.Current;
            var curScore = keySelector(cur);
            if (curScore.CompareTo(score) < 0)
            {
                score = curScore;
                res = cur;
            }
        }
        return res;
    }

    public static TSource? MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) where TKey : IComparable
    {
        using var e = source.GetEnumerator();
        if (!e.MoveNext())
            return default;

        var res = e.Current;
        var score = keySelector(res);
        while (e.MoveNext())
        {
            var cur = e.Current;
            var curScore = keySelector(cur);
            if (curScore.CompareTo(score) > 0)
            {
                score = curScore;
                res = cur;
            }
        }
        return res;
    }

    // get existing map element or create new
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> map, TKey key) where TValue : new()
    {
        if (!map.TryGetValue(key, out var value))
        {
            value = new();
            map[key] = value;
        }
        return value;
    }

    public static T MaxAll<T>(T first, params T[] rest) where T : IComparable
    {
        foreach (var v in rest)
            if (v.CompareTo(first) > 0)
                first = v;
        return first;
    }

    // add value to the list, if it is not null
    public static bool AddIfNonNull<T>(this List<T> list, T? value)
    {
        if (value == null)
            return false;
        list.Add(value);
        return true;
    }

    // get reference to the list element (a bit of a hack, but oh well...)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T Ref<T>(this List<T> list, int index) => ref CollectionsMarshal.AsSpan(list)[index];
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> AsSpan<T>(this List<T> list) => CollectionsMarshal.AsSpan(list);

    // lower bound: given sorted list, find index of first element with key >= than test value
    public static int LowerBound<TKey, TValue>(this SortedList<TKey, TValue> list, TKey test) where TKey : notnull, IComparable
    {
        int first = 0, size = list.Count;
        while (size > 0)
        {
            var step = size / 2;
            var mid = first + step;
            if (list.Keys[mid].CompareTo(test) < 0)
            {
                first = mid + 1;
                size -= step + 1;
            }
            else
            {
                size = step;
            }
        }
        return first;
    }

    // upper bound: given sorted list, find index of first element with key > than test value
    public static int UpperBound<TKey, TValue>(this SortedList<TKey, TValue> list, TKey test) where TKey : notnull, IComparable
    {
        int first = 0, size = list.Count;
        while (size > 0)
        {
            var step = size / 2;
            var mid = first + step;
            if (list.Keys[mid].CompareTo(test) <= 0)
            {
                first = mid + 1;
                size -= step + 1;
            }
            else
            {
                size = step;
            }
        }
        return first;
    }

    // get read only span of zero or one elements, depending on whether argument is null
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<T> ZeroOrOne<T>(ref readonly T? value) where T : struct
    {
        return value != null ? new T[1] { value.Value } : [];
    }

    // swap two values
    public static void Swap<T>(ref T l, ref T r) => (r, l) = (l, r);

    // sort a list and remove duplicates
    public static void SortAndRemoveDuplicates<T>(this List<T> list) where T : notnull, IComparable
    {
        list.Sort();
        var span = list.AsSpan();
        var last = 0;
        for (var i = 1; i < list.Count; ++i)
        {
            if (!span[i].Equals(span[last]))
            {
                ++last;
                if (i != last)
                    span[last] = span[i];
            }
        }
        ++last;
        if (last < list.Count)
            list.RemoveRange(last, list.Count - last);
    }

    // linear interpolation
    public static float Lerp(float a, float b, float t) => a + (b - a) * t;

    // build an array with N copies of same element
    public static T[] MakeArray<T>(int count, T value)
    {
        var res = new T[count];
        Array.Fill(res, value);
        return res;
    }

    // bounds-checking access
    public static T? BoundSafeAt<T>(this T[] array, int index, T? outOfBounds = default) => index >= 0 && index < array.Length ? array[index] : outOfBounds;
    public static T? BoundSafeAt<T>(this List<T> array, int index, T? outOfBounds = default) => index >= 0 && index < array.Count ? array[index] : outOfBounds;

    // get all types defined in specified assembly
    public static IEnumerable<Type?> GetAllTypes(Assembly asm)
    {
        try
        {
            return asm.DefinedTypes;
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types;
        }
    }

    // get all types derived from specified type in specified assembly
    public static IEnumerable<Type> GetDerivedTypes<Base>(Assembly asm)
    {
        var b = typeof(Base);
        return GetAllTypes(asm).Where(t => t?.IsSubclassOf(b) ?? false).Select(t => t!);
    }

    // generate valid identifier name from human-readable string
    public static string StringToIdentifier(string v)
    {
        v = v.Replace("'", null, StringComparison.Ordinal);
        v = v.Replace('-', ' ');
        v = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(v);
        v = NonAlphaNumRegex().Replace(v, "");
        return v;
    }

    [GeneratedRegex("[^a-zA-Z0-9]")]
    private static partial Regex NonAlphaNumRegex();

    public static IEnumerable<(string, T)> DedupKeys<T>(IEnumerable<(string, T)> items)
    {
        var keys = new HashSet<string>();

        foreach (var (k, v) in items)
        {
            var i = 0;
            var key = k;
            while (!keys.Add(key))
                key = $"{k}{++i}";

            yield return (key, v);
        }
    }

    public static IEnumerable<(string, T)> DedupKeys<T>(Dictionary<string, T> items) => DedupKeys(items.Select(i => (i.Key, i.Value)));

#pragma warning disable
    /// <summary>
    /// Sets whether <see cref="User32.GetKeyState"/> or <see cref="User32.GetAsyncKeyState"/> will be used when calling <see cref="IsKeyPressed(Keys)"/> or <see cref="IsKeyPressed(LimitedKeys)"/>
    /// </summary>
    private static bool UseAsyncKeyCheck;
#pragma warning restore
    /// <summary>
    /// Checks if a key is pressed via winapi.
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>Whether the key is currently pressed</returns>
    public static bool IsKeyPressed(int key)
    {
        if (key == 0)
            return false;
        if (UseAsyncKeyCheck)
        {
            return Bitmasks.IsBitSet(User32.GetKeyState(key), 15);
        }
        else
        {
            return Bitmasks.IsBitSet(User32.GetAsyncKeyState(key), 15);
        }
    }

    /// <summary>
    /// Checks if a key is pressed via winapi.
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>Whether the key is currently pressed</returns>
    public static bool IsKeyPressed(Util.LimitedKeys key) => IsKeyPressed((int)key);

    public static bool IsAnyKeyPressed(IEnumerable<Util.LimitedKeys> keys) => keys.Any(IsKeyPressed);

    public static bool IsKeyPressed(IEnumerable<Util.LimitedKeys> keys)
    {
        foreach (var x in keys)
        {
            if (IsKeyPressed(x))
                return true;
        }
        return false;
    }

    public static bool IsKeyPressed(IEnumerable<int> keys)
    {
        foreach (var x in keys)
        {
            if (IsKeyPressed(x))
                return true;
        }
        return false;
    }

    public static Vector3 RotatePoint(float cx, float cy, float angle, Vector3 p)
    {
        if (angle == 0f)
            return p;
        var s = (float)Math.Sin(angle);
        var c = (float)Math.Cos(angle);

        // translate point back to origin:
        p.X -= cx;
        p.Z -= cy;

        // rotate point
        var xnew = p.X * c - p.Z * s;
        var ynew = p.X * s + p.Z * c;

        // translate point back:
        p.X = xnew + cx;
        p.Z = ynew + cy;
        return p;
    }

    public static Func<TIn, TOut> Memoize<TIn, TOut>(this Func<TIn, TOut> func) where TIn : notnull
    {
        var cache = new Dictionary<TIn, TOut>();
        return input =>
        {
            if (cache.TryGetValue(input, out var cached))
                return cached;

            return cache[input] = func(input);
        };
    }
}

public static class Bitmasks
{
    public static bool IsBitSet(ulong b, int pos)
    {
        return (b & (1UL << pos)) != 0;
    }

    public static void SetBit(ref ulong b, int pos)
    {
        b |= 1UL << pos;
    }

    public static void ResetBit(ref ulong b, int pos)
    {
        b &= ~(1UL << pos);
    }

    public static bool IsBitSet(byte b, int pos)
    {
        return (b & (1 << pos)) != 0;
    }

    public static bool IsBitSet(short b, int pos)
    {
        return (b & (1 << pos)) != 0;
    }
}
