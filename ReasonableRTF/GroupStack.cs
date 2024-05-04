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
    internal const int MaxGroups = 100;

    private BoolArrayWrapper SkipDestinations;
    private BoolArrayWrapper InFontTables;
    internal ByteArrayWrapper SymbolFonts;
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
        SkipDestinations.Array[Count + 1] = SkipDestinations.Array[Count];
        InFontTables.Array[Count + 1] = InFontTables.Array[Count];
        SymbolFonts.Array[Count + 1] = SymbolFonts.Array[Count];
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
        get => SkipDestinations.Array[Count];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SkipDestinations.Array[Count] = value;
    }

    internal unsafe bool CurrentInFontTable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => InFontTables.Array[Count];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => InFontTables.Array[Count] = value;
    }

    internal unsafe SymbolFont CurrentSymbolFont
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (SymbolFont)SymbolFonts.Array[Count];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SymbolFonts.Array[Count] = (byte)value;
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
        SkipDestinations.Array[0] = false;
        InFontTables.Array[0] = false;
        SymbolFonts.Array[0] = (int)SymbolFont.None;

        Properties[0][(int)Property.Hidden] = 0;
        Properties[0][(int)Property.UnicodeCharSkipCount] = 1;
        Properties[0][(int)Property.FontNum] = RtfToTextConverter.NoFontNumber;
        Properties[0][(int)Property.Lang] = -1;
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ClearFast() => Count = 0;
}
