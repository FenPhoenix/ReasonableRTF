﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using static ReasonableRTF.Enums;

namespace ReasonableRTF;

internal sealed class GroupStack
{
    private const int DefaultCapacity = 100;
    private int Capacity;

    private bool[] _skipDestinations;
    private bool[] _inFontTables;
    internal byte[] _symbolFonts;
    internal int[][] Properties;

    /// <summary>Do not modify!</summary>
    internal int Count;

    internal GroupStack() => Init();

    [MemberNotNull(
        nameof(_skipDestinations),
        nameof(_inFontTables),
        nameof(_symbolFonts),
        nameof(Properties))]
    private void Init()
    {
        Count = 0;
        Capacity = DefaultCapacity;

        _skipDestinations = new bool[Capacity];
        _inFontTables = new bool[Capacity];
        _symbolFonts = new byte[Capacity];
        Properties = new int[Capacity][];

        for (int i = 0; i < Capacity; i++)
        {
            Properties[i] = new int[PropertiesLen];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void DeepCopyToNext()
    {
        // We don't really take a speed hit from this at all, but we support files with a stupid amount of
        // nested groups now.
        if (Count >= Capacity - 1)
        {
            int oldMaxGroups = Capacity;

            int newCapacity = Capacity * 2;
            if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;

            Capacity = newCapacity;
            Array.Resize(ref _skipDestinations, Capacity);
            Array.Resize(ref _inFontTables, Capacity);
            Array.Resize(ref _symbolFonts, Capacity);
            Array.Resize(ref Properties, Capacity);

            for (int i = oldMaxGroups; i < Capacity; i++)
            {
                Properties[i] = new int[PropertiesLen];
            }
        }

        _skipDestinations[Count + 1] = _skipDestinations[Count];
        _inFontTables[Count + 1] = _inFontTables[Count];
        _symbolFonts[Count + 1] = _symbolFonts[Count];
        for (int i = 0; i < PropertiesLen; i++)
        {
            Properties[Count + 1][i] = Properties[Count][i];
        }
        ++Count;
    }

    #region Current group

    internal bool CurrentSkipDest
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _skipDestinations[Count];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _skipDestinations[Count] = value;
    }

    internal bool CurrentInFontTable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _inFontTables[Count];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _inFontTables[Count] = value;
    }

    internal SymbolFont CurrentSymbolFont
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (SymbolFont)_symbolFonts[Count];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _symbolFonts[Count] = (byte)value;
    }

    internal int[] CurrentProperties
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Properties[Count];
    }

    // Current group always begins at group 0, so reset just that one
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ResetFirst()
    {
        _skipDestinations[0] = false;
        _inFontTables[0] = false;
        _symbolFonts[0] = (int)SymbolFont.None;

        Properties[0][(int)Property.Hidden] = 0;
        Properties[0][(int)Property.UnicodeCharSkipCount] = 1;
        Properties[0][(int)Property.FontNum] = RtfToTextConverter.NoFontNumber;
        Properties[0][(int)Property.Lang] = -1;
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ClearFast() => Count = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ResetCapacityIfTooHigh()
    {
        if (Capacity > DefaultCapacity)
        {
            Init();
        }
    }
}
