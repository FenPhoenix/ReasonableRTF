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
    private Dictionary<int, FontEntry>? _dict;

    private readonly ListFast<FontEntry> _fontEntryPool;
    private int _fontEntryPoolVirtualCount;

    private readonly FontEntry?[] _array = new FontEntry?[_switchPoint];

    /*
    \fN params are normally in the signed int16 range, but the Windows RichEdit control supports them in the
    -30064771071 - 30064771070 (-0x6ffffffff - 0x6fffffffe) range (yes, bizarre numbers, but I tested and
    there they are). So we're going to use the array for the expected "normal" range, and fall back to the
    dictionary for weird crap that probably won't happen.
    */
    private const int _switchPoint = 32768;

    internal FontEntry? Top;

    private int _highestKey;

    internal FontDictionary(int capacity)
    {
        _capacity = capacity;
        _fontEntryPool = new ListFast<FontEntry>(capacity);
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
        if (key is < 0 or >= _switchPoint)
        {
            _dict ??= new Dictionary<int, FontEntry>(_capacity);
            _dict[key] = fontEntry;
        }
        else
        {
            _array[key] = fontEntry;
        }
        if (key > _highestKey) _highestKey = key;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ClearFull(int capacity)
    {
        _capacity = capacity;
        _highestKey = 0;
        _fontEntryPool.Capacity = capacity;
        _dict = null;
        Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Clear()
    {
        Top = null;
        _dict?.Reset();
        /*
        Clear only the required portion of the array to shave some time off.

        We can elide the clear and that works correctly as long as we don't have \f keywords that reference
        undefined font numbers, but if we did then it would index into the array and possibly get a font
        entry when it should have gotten null. That's unlikely, but we should probably not just ignore it.

        Aside from eliding the clear and reducing edge-case safety, this up-to-highest-key clear is probably
        about the best we can do.
        */

        // If the dictionary is not null, that means we had an out-of-array-range key, so all bets are off.
        // Just clear the entire thing in that case.
        if (_dict == null && (_highestKey is > 0 and < _switchPoint))
        {
            Array.Clear(_array, 0, _highestKey + 1);
        }
        else
        {
            Array.Clear(_array);
        }
        _highestKey = 0;
        _fontEntryPoolVirtualCount = _fontEntryPool.Count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryGetValue(int key, [NotNullWhen(true)] out FontEntry? value)
    {
        if (key is < 0 or >= _switchPoint)
        {
            if (_dict == null)
            {
                value = null;
                return false;
            }
            else
            {
                return _dict.TryGetValue(key, out value);
            }
        }
        else
        {
            value = _array[key];
            return value != null;
        }
    }
}
