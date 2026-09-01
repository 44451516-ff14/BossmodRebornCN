using Dalamud.Bindings.ImGui;

namespace BossMod;

// Repeated hint strings normally pass through ImU8String's UTF-16 -> UTF-8 conversion on every
// submission. Keep two small, thread-local generations of null-terminated UTF-8 instead.
// The cache is deliberately bounded: encounter-authored literals stay hot, while rapidly changing actor names and timers cannot retain strings for the lifetime of the plugin.
[SkipLocalsInit]
internal static class UIText
{
    private const int MaxEntriesPerGeneration = 512;
    private const int MaxBytesPerGeneration = 128 * 1024;
    private const int MaxCachedStringBytes = 4096;

    private readonly struct CachedString(byte[] buffer, int length)
    {
        public readonly byte[] Buffer = buffer;
        public readonly int Length = length;
    }

    private sealed class Utf8Cache
    {
        private Dictionary<string, CachedString> _current = new(MaxEntriesPerGeneration, StringComparer.Ordinal);
        private Dictionary<string, CachedString> _previous = new(MaxEntriesPerGeneration, StringComparer.Ordinal);
        private int _currentBytes;

        public CachedString Get(string text)
        {
            if (_current.TryGetValue(text, out var cached))
            {
                return cached;
            }
            if (_previous.TryGetValue(text, out cached))
            {
                Add(text, cached);
                return cached;
            }

            var length = Encoding.UTF8.GetByteCount(text);
            var buffer = new byte[length + 1];
            Encoding.UTF8.GetBytes(text, 0, text.Length, buffer, 0);
            var result = new CachedString(buffer, length); // new byte[] supplies the trailing zero
            if (length <= MaxCachedStringBytes)
            {
                Add(text, result);
            }
            return result;
        }

        private void Add(string text, CachedString cached)
        {
            if (_current.Count >= MaxEntriesPerGeneration || _currentBytes + cached.Length > MaxBytesPerGeneration)
            {
                _previous.Clear();
                (_current, _previous) = (_previous, _current);
                _currentBytes = 0;
            }

            if (_current.TryAdd(text, cached))
            {
                _currentBytes += cached.Length;
            }
        }
    }

    [ThreadStatic]
    private static Utf8Cache? _threadCache;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TextUnformatted(string text)
    {
        var cached = (_threadCache ??= new()).Get(text);
        var utf8 = new ImU8String(cached.Buffer.AsSpan(0, cached.Length));
        // In the current Dalamud bindings Text is the direct unformatted native submission; calling
        //  TextUnformatted would wrap this span in a second ImU8String before reaching the same entrypoint
        ImGui.Text(utf8);
    }
}
