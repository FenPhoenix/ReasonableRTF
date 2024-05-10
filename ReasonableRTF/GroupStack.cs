using System.Runtime.CompilerServices;
using static ReasonableRTF.Enums;

namespace ReasonableRTF;

internal sealed class GroupStack
{
    // SOA and removal of bounds checking through fixed-sized buffers improves perf

    internal unsafe struct ByteArrayWrapper
    {
        internal fixed byte Array[MaxGroups];
    }

    private unsafe struct BoolArrayWrapper
    {
        internal fixed bool Array[MaxGroups];
    }

    // Highest measured was 10
    /// <summary>100</summary>
    internal const int MaxGroups = 100;

    private BoolArrayWrapper _skipDestinations;
    private BoolArrayWrapper _inFontTables;
    internal ByteArrayWrapper _symbolFonts;
    internal readonly int[][] Properties = new int[MaxGroups][];

    /// <summary>Do not modify!</summary>
    internal int Count;

    internal GroupStack()
    {
        for (int i = 0; i < MaxGroups; i++)
        {
            Properties[i] = new int[PropertiesLen];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal unsafe void DeepCopyToNext()
    {
        _skipDestinations.Array[Count + 1] = _skipDestinations.Array[Count];
        _inFontTables.Array[Count + 1] = _inFontTables.Array[Count];
        _symbolFonts.Array[Count + 1] = _symbolFonts.Array[Count];
        for (int i = 0; i < PropertiesLen; i++)
        {
            Properties[Count + 1][i] = Properties[Count][i];
        }
        ++Count;
    }

    #region Current group

    internal unsafe bool CurrentSkipDest
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _skipDestinations.Array[Count];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _skipDestinations.Array[Count] = value;
    }

    internal unsafe bool CurrentInFontTable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _inFontTables.Array[Count];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _inFontTables.Array[Count] = value;
    }

    internal unsafe SymbolFont CurrentSymbolFont
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (SymbolFont)_symbolFonts.Array[Count];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _symbolFonts.Array[Count] = (byte)value;
    }

    internal int[] CurrentProperties
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Properties[Count];
    }

    // Current group always begins at group 0, so reset just that one
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal unsafe void ResetFirst()
    {
        _skipDestinations.Array[0] = false;
        _inFontTables.Array[0] = false;
        _symbolFonts.Array[0] = (int)SymbolFont.None;

        Properties[0][(int)Property.Hidden] = 0;
        Properties[0][(int)Property.UnicodeCharSkipCount] = 1;
        Properties[0][(int)Property.FontNum] = RtfToTextConverter.NoFontNumber;
        Properties[0][(int)Property.Lang] = -1;
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ClearFast() => Count = 0;
}
