using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using static ReasonableRTF.Enums;

namespace ReasonableRTF;

internal sealed class FontEntry
{
    internal int CodePage = -1;

    // We need to store names in case we get codepage 42 nonsense, we need to know which font to translate to
    internal SymbolFont SymbolFont = SymbolFont.Unset;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Reset()
    {
        CodePage = -1;
        SymbolFont = SymbolFont.Unset;
    }
}

internal sealed class FontDictionary
{
    private int _capacity;
    private readonly Dictionary<int, FontEntry> _dict;

    private readonly ListFast<FontEntry> _fontEntryPool;
    private int _fontEntryPoolVirtualCount;

    /*
    \fN params are normally in the signed int16 range, but the Windows RichEdit control supports them in the
    -30064771071 - 30064771070 (-0x6ffffffff - 0x6fffffffe) range (yes, bizarre numbers, but I tested and
    there they are).
    */

    internal FontEntry? Top;

    internal FontDictionary(int capacity)
    {
        _capacity = capacity;
        _fontEntryPool = new ListFast<FontEntry>(capacity);
        _dict = new Dictionary<int, FontEntry>(_capacity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Add(int key)
    {
        FontEntry fontEntry;
        if (_fontEntryPoolVirtualCount > 0)
        {
            --_fontEntryPoolVirtualCount;
            fontEntry = _fontEntryPool[_fontEntryPoolVirtualCount];
            fontEntry.Reset();
        }
        else
        {
            fontEntry = new FontEntry();
            _fontEntryPool.Add(fontEntry);
        }

        Top = fontEntry;
        _dict[key] = fontEntry;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Clear()
    {
        Top = null;
        _dict.Clear();
        _fontEntryPoolVirtualCount = _fontEntryPool.Count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ClearFull(int capacity)
    {
        _capacity = capacity;
        _fontEntryPool.HardReset(capacity);
        _dict.Reset(capacity);
        Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryGetValue(int key, [NotNullWhen(true)] out FontEntry? value)
    {
        return _dict.TryGetValue(key, out value);
    }
}
