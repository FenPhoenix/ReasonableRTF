/*
 * The MIT License (MIT)
 * 
 * Copyright (c) .NET Foundation and Contributors
 * 
 * All rights reserved.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
*/

#if NET8_0_OR_GREATER
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using ReasonableRTF.Models.DataTypes;

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    #region Private fields

    private static readonly Vector512<byte> _zeroVector512 = Vector512.Create((byte)'\0');
    private static readonly Vector512<byte> _lfVector512 = Vector512.Create((byte)'\n');
    private static readonly Vector512<byte> _crVector512 = Vector512.Create((byte)'\r');
    private static readonly Vector512<byte> _backslashVector512 = Vector512.Create((byte)'\\');
    private static readonly Vector512<byte> _openBraceVector512 = Vector512.Create((byte)'{');
    private static readonly Vector512<byte> _closingBraceVector512 = Vector512.Create((byte)'}');
    private static readonly Vector512<byte> _nVector512 = Vector512.Create((byte)'n');
    // CreateSequence() was introduced in .NET 9, so since we support 8, we need to do it manually for now.
    private static readonly Vector512<byte> _indexVec_512 = Vector512.Create(
        (byte)
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
        16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31,
        32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47,
        48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63
    );

    private static readonly Vector256<byte> _zeroVector256 = Vector256.Create((byte)'\0');
    private static readonly Vector256<byte> _lfVector256 = Vector256.Create((byte)'\n');
    private static readonly Vector256<byte> _crVector256 = Vector256.Create((byte)'\r');
    private static readonly Vector256<byte> _backslashVector256 = Vector256.Create((byte)'\\');
    private static readonly Vector256<byte> _openBraceVector256 = Vector256.Create((byte)'{');
    private static readonly Vector256<byte> _closingBraceVector256 = Vector256.Create((byte)'}');
    private static readonly Vector256<byte> _nVector256 = Vector256.Create((byte)'n');
    private static readonly Vector256<byte> _indexVec_256 = Vector256.Create(
        (byte)
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
        16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31
    );

    private static readonly Vector128<byte> _zeroVector128 = Vector128.Create((byte)'\0');
    private static readonly Vector128<byte> _lfVector128 = Vector128.Create((byte)'\n');
    private static readonly Vector128<byte> _crVector128 = Vector128.Create((byte)'\r');
    private static readonly Vector128<byte> _backslashVector128 = Vector128.Create((byte)'\\');
    private static readonly Vector128<byte> _openBraceVector128 = Vector128.Create((byte)'{');
    private static readonly Vector128<byte> _closingBraceVector128 = Vector128.Create((byte)'}');
    private static readonly Vector128<byte> _nVector128 = Vector128.Create((byte)'n');
    private static readonly Vector128<byte> _indexVec_128 = Vector128.Create(
        (byte)
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15
    );

    private static ReadOnlySpan<byte> _isParEndingChar =>
    [
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        4, // '\n' (10)
        0, 0,
        4, // '\r' (13)
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        5, // ' ' (32)
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0,
        4, // '\\' (92)
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0,
    ];

    private static ReadOnlySpan<bool> _isIgnoreChar =>
    [
        true, // '\0' (0)
        false, false, false, false, false, false, false, false, false,
        true, // '\n' (10)
        false, false,
        true, // '\r' (13)
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false,
    ];

    #endregion

    #region API

    // Heavily modified version of .NET SpanHelpers.IndexOfAnyValueType().
    // Made to handle the \binN situation while losing as little performance as possible.
    private static int SIMD_SkipDest(
        byte[] buffer,
        int startIndex,
        int spanLength)
    {
        if (!Vector512.IsHardwareAccelerated &&
            !Vector256.IsHardwareAccelerated &&
            !Vector128.IsHardwareAccelerated)
        {
            return -1;
        }

        uint binUInt = BitConverter.IsLittleEndian ? 0x6E69625Cu : 0x5C62696Eu;
        int currentSpanPosition = 0;

        ReadOnlySpan<byte> span = buffer.AsSpan(startIndex, spanLength);

        if (Vector512.IsHardwareAccelerated && spanLength >= Vector512<byte>.Count)
        {
            ref byte searchSpace = ref MemoryMarshal.GetReference(span);
            Vector512<byte> equalsBraces;
            Vector512<byte> equalsBackslash;
            Vector512<byte> equals;
            Vector512<byte> current;
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, spanLength - Vector512<byte>.Count);

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                current = Vector512.LoadUnsafe(ref currentSearchSpace);
                equalsBraces = Vector512.Equals(_openBraceVector512, current) | Vector512.Equals(_closingBraceVector512, current);
                equalsBackslash = Vector512.Equals(_backslashVector512, current);
                equals = equalsBraces | equalsBackslash;
                if (equals == Vector512<byte>.Zero)
                {
                    currentSpanPosition += Vector512<byte>.Count;
                    currentSearchSpace = ref Unsafe.Add(ref currentSearchSpace, Vector512<byte>.Count);
                    continue;
                }

                if (equalsBackslash != Vector512<byte>.Zero)
                {
                    ulong notEqualsElementsBackslash = equalsBackslash.ExtractMostSignificantBits();
                    int backslashIndex = -1;
                    int bracesIndex = 0;

                    bool bracesFound = equalsBraces != Vector512<byte>.Zero;
                    if (!bracesFound || (backslashIndex = BitOperations.TrailingZeroCount(notEqualsElementsBackslash)) < (bracesIndex = BitOperations.TrailingZeroCount(equalsBraces.ExtractMostSignificantBits())))
                    {
                        if (currentSpanPosition + Vector512<byte>.Count + (_binLength - 1) <= spanLength)
                        {
                            Vector512<byte> lastBlock = Vector512.LoadUnsafe(ref Unsafe.Add(ref currentSearchSpace, _binLength - 1));
                            Vector512<byte> lastEquals = Vector512.Equals(_nVector512, lastBlock);

                            ulong mask = Vector512.BitwiseAnd(equalsBackslash, lastEquals).ExtractMostSignificantBits();
                            while (mask != 0)
                            {
                                int index = currentSpanPosition + BitOperations.TrailingZeroCount(mask);
                                if (index < 0 || index >= spanLength - sizeof(uint) ||
                                    Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref searchSpace, index)) == binUInt)
                                {
                                    if (backslashIndex == -1) backslashIndex = BitOperations.TrailingZeroCount(notEqualsElementsBackslash);
                                    return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, backslashIndex);
                                }

                                mask = ResetLowestSetBit(mask);
                            }
                        }
                        else
                        {
                            if (backslashIndex == -1) backslashIndex = BitOperations.TrailingZeroCount(notEqualsElementsBackslash);
                            int currentVectorIndex = backslashIndex;
                            ulong mask = notEqualsElementsBackslash;
                            while (currentVectorIndex < Vector512<byte>.Count)
                            {
                                int spanIndex = currentSpanPosition + currentVectorIndex;
                                if (spanIndex >= spanLength - sizeof(uint) ||
                                    Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref searchSpace, spanIndex)) == binUInt)
                                {
                                    return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, backslashIndex);
                                }
                                mask = ResetLowestSetBit(mask);
                                currentVectorIndex = BitOperations.TrailingZeroCount(mask);
                            }
                        }

                        if (!bracesFound)
                        {
                            currentSpanPosition += Vector512<byte>.Count;
                            currentSearchSpace = ref Unsafe.Add(ref currentSearchSpace, Vector512<byte>.Count);
                            continue;
                        }
                        else
                        {
                            return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, bracesIndex);
                        }
                    }
                }

                return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
            }
            while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));

            // If any elements remain, process the last vector in the search space.
            if ((uint)spanLength % Vector512<byte>.Count != 0)
            {
                current = Vector512.LoadUnsafe(ref oneVectorAwayFromEnd);
                equalsBraces = Vector512.Equals(_openBraceVector512, current) | Vector512.Equals(_closingBraceVector512, current);
                equalsBackslash = Vector512.Equals(_backslashVector512, current);
                equals = equalsBraces | equalsBackslash;
                if (equals != Vector512<byte>.Zero)
                {
                    return startIndex + ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                }
            }
        }
        else if (Vector256.IsHardwareAccelerated && spanLength >= Vector256<byte>.Count)
        {
            ref byte searchSpace = ref MemoryMarshal.GetReference(span);
            Vector256<byte> equalsBraces;
            Vector256<byte> equalsBackslash;
            Vector256<byte> equals;
            Vector256<byte> current;
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, spanLength - Vector256<byte>.Count);

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                current = Vector256.LoadUnsafe(ref currentSearchSpace);
                equalsBraces = Vector256.Equals(_openBraceVector256, current) | Vector256.Equals(_closingBraceVector256, current);
                equalsBackslash = Vector256.Equals(_backslashVector256, current);
                equals = equalsBraces | equalsBackslash;
                if (equals == Vector256<byte>.Zero)
                {
                    currentSpanPosition += Vector256<byte>.Count;
                    currentSearchSpace = ref Unsafe.Add(ref currentSearchSpace, Vector256<byte>.Count);
                    continue;
                }

                if (equalsBackslash != Vector256<byte>.Zero)
                {
                    uint notEqualsElementsBackslash = equalsBackslash.ExtractMostSignificantBits();
                    int backslashIndex = -1;
                    int bracesIndex = 0;

                    bool bracesFound = equalsBraces != Vector256<byte>.Zero;
                    if (!bracesFound || (backslashIndex = BitOperations.TrailingZeroCount(notEqualsElementsBackslash)) < (bracesIndex = BitOperations.TrailingZeroCount(equalsBraces.ExtractMostSignificantBits())))
                    {
                        if (currentSpanPosition + Vector256<byte>.Count + (_binLength - 1) <= spanLength)
                        {
                            Vector256<byte> lastBlock = Vector256.LoadUnsafe(ref Unsafe.Add(ref currentSearchSpace, _binLength - 1));
                            Vector256<byte> lastEquals = Vector256.Equals(_nVector256, lastBlock);

                            uint mask = Vector256.BitwiseAnd(equalsBackslash, lastEquals).ExtractMostSignificantBits();
                            while (mask != 0)
                            {
                                int index = currentSpanPosition + BitOperations.TrailingZeroCount(mask);
                                if (index < 0 || index >= spanLength - sizeof(uint) ||
                                    Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref searchSpace, index)) == binUInt)
                                {
                                    if (backslashIndex == -1) backslashIndex = BitOperations.TrailingZeroCount(notEqualsElementsBackslash);
                                    return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, backslashIndex);
                                }

                                mask = ResetLowestSetBit(mask);
                            }
                        }
                        else
                        {
                            if (backslashIndex == -1) backslashIndex = BitOperations.TrailingZeroCount(notEqualsElementsBackslash);
                            int currentVectorIndex = backslashIndex;
                            uint mask = notEqualsElementsBackslash;
                            while (currentVectorIndex < Vector256<byte>.Count)
                            {
                                int spanIndex = currentSpanPosition + currentVectorIndex;
                                if (spanIndex >= spanLength - sizeof(uint) ||
                                    Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref searchSpace, spanIndex)) == binUInt)
                                {
                                    return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, backslashIndex);
                                }
                                mask = ResetLowestSetBit(mask);
                                currentVectorIndex = BitOperations.TrailingZeroCount(mask);
                            }
                        }

                        if (!bracesFound)
                        {
                            currentSpanPosition += Vector256<byte>.Count;
                            currentSearchSpace = ref Unsafe.Add(ref currentSearchSpace, Vector256<byte>.Count);
                            continue;
                        }
                        else
                        {
                            return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, bracesIndex);
                        }
                    }
                }

                return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
            }
            while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));

            // If any elements remain, process the last vector in the search space.
            if ((uint)spanLength % Vector256<byte>.Count != 0)
            {
                current = Vector256.LoadUnsafe(ref oneVectorAwayFromEnd);
                equalsBraces = Vector256.Equals(_openBraceVector256, current) | Vector256.Equals(_closingBraceVector256, current);
                equalsBackslash = Vector256.Equals(_backslashVector256, current);
                equals = equalsBraces | equalsBackslash;
                if (equals != Vector256<byte>.Zero)
                {
                    return startIndex + ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                }
            }
        }
        else if (Vector128.IsHardwareAccelerated && spanLength >= Vector128<byte>.Count)
        {
            ref byte searchSpace = ref MemoryMarshal.GetReference(span);
            Vector128<byte> equalsBraces;
            Vector128<byte> equalsBackslash;
            Vector128<byte> equals;
            Vector128<byte> current;
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, spanLength - Vector128<byte>.Count);

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                current = Vector128.LoadUnsafe(ref currentSearchSpace);
                equalsBraces = Vector128.Equals(_openBraceVector128, current) | Vector128.Equals(_closingBraceVector128, current);
                equalsBackslash = Vector128.Equals(_backslashVector128, current);
                equals = equalsBraces | equalsBackslash;
                if (equals == Vector128<byte>.Zero)
                {
                    currentSpanPosition += Vector128<byte>.Count;
                    currentSearchSpace = ref Unsafe.Add(ref currentSearchSpace, Vector128<byte>.Count);
                    continue;
                }

                if (equalsBackslash != Vector128<byte>.Zero)
                {
                    uint notEqualsElementsBackslash = equalsBackslash.ExtractMostSignificantBits();
                    int backslashIndex = -1;
                    int bracesIndex = 0;

                    bool bracesFound = equalsBraces != Vector128<byte>.Zero;
                    if (!bracesFound || (backslashIndex = BitOperations.TrailingZeroCount(notEqualsElementsBackslash)) < (bracesIndex = BitOperations.TrailingZeroCount(equalsBraces.ExtractMostSignificantBits())))
                    {
                        if (currentSpanPosition + Vector128<byte>.Count + (_binLength - 1) <= spanLength)
                        {
                            Vector128<byte> lastBlock = Vector128.LoadUnsafe(ref Unsafe.Add(ref currentSearchSpace, _binLength - 1));
                            Vector128<byte> lastEquals = Vector128.Equals(_nVector128, lastBlock);

                            uint mask = Vector128.BitwiseAnd(equalsBackslash, lastEquals).ExtractMostSignificantBits();
                            while (mask != 0)
                            {
                                int index = currentSpanPosition + BitOperations.TrailingZeroCount(mask);
                                if (index < 0 || index >= spanLength - sizeof(uint) ||
                                    Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref searchSpace, index)) == binUInt)
                                {
                                    if (backslashIndex == -1) backslashIndex = BitOperations.TrailingZeroCount(notEqualsElementsBackslash);
                                    return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, backslashIndex);
                                }

                                mask = ResetLowestSetBit(mask);
                            }
                        }
                        else
                        {
                            if (backslashIndex == -1) backslashIndex = BitOperations.TrailingZeroCount(notEqualsElementsBackslash);
                            int currentVectorIndex = backslashIndex;
                            uint mask = notEqualsElementsBackslash;
                            while (currentVectorIndex < Vector128<byte>.Count)
                            {
                                int spanIndex = currentSpanPosition + currentVectorIndex;
                                if (spanIndex >= spanLength - sizeof(uint) ||
                                    Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref searchSpace, spanIndex)) == binUInt)
                                {
                                    return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, backslashIndex);
                                }
                                mask = ResetLowestSetBit(mask);
                                currentVectorIndex = BitOperations.TrailingZeroCount(mask);
                            }
                        }

                        if (!bracesFound)
                        {
                            currentSpanPosition += Vector128<byte>.Count;
                            currentSearchSpace = ref Unsafe.Add(ref currentSearchSpace, Vector128<byte>.Count);
                            continue;
                        }
                        else
                        {
                            return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, bracesIndex);
                        }
                    }
                }

                return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
            }
            while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));

            // If any elements remain, process the last vector in the search space.
            if ((uint)spanLength % Vector128<byte>.Count != 0)
            {
                current = Vector128.LoadUnsafe(ref oneVectorAwayFromEnd);
                equalsBraces = Vector128.Equals(_openBraceVector128, current) | Vector128.Equals(_closingBraceVector128, current);
                equalsBackslash = Vector128.Equals(_backslashVector128, current);
                equals = equalsBraces | equalsBackslash;
                if (equals != Vector128<byte>.Zero)
                {
                    return startIndex + ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                }
            }
        }
        return -1;
    }

    // Heavily modified version of .NET SpanHelpers.IndexOfAnyValueType().
    private void SIMD_CopyPlainText(
        byte[] buffer,
        int startIndex,
        int spanLength,
        ListFast<char> plainText,
        ref int currentPos)
    {
        if (!Vector.IsHardwareAccelerated)
        {
            return;
        }

        uint parUInt = BitConverter.IsLittleEndian ? 0x7261705Cu : 0x5C706172u;
        const int parMaxLength = 5;

        ReadOnlySpan<byte> span = buffer.AsSpan(startIndex, spanLength);

        ref byte searchSpace = ref MemoryMarshal.GetReference(span);

        if (Vector512.IsHardwareAccelerated && spanLength >= Vector512<byte>.Count)
        {
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, (uint)(spanLength - Vector512<byte>.Count));

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                Vector512<byte> current = Vector512.LoadUnsafe(ref currentSearchSpace);

                Vector512<byte> equalsBackslash =
                    Vector512.Equals(_backslashVector512, current);

                Vector512<byte> equalsBraces =
                    Vector512.Equals(_openBraceVector512, current) |
                    Vector512.Equals(_closingBraceVector512, current);

                Vector512<byte> equalsLineBreaks =
                    Vector512.Equals(_lfVector512, current) |
                    Vector512.Equals(_crVector512, current);

                Vector512<byte> equalsZeroBytes =
                    Vector512.Equals(_zeroVector512, current);

                Vector512<byte> equalsOtherExceptLineBreaks =
                    equalsZeroBytes |
                    equalsBraces;

                Vector512<byte> equals =
                    equalsZeroBytes |
                    equalsLineBreaks |
                    equalsBraces |
                    equalsBackslash;

                Vector512<byte> equalsBackslashOrLineBreak = equalsBackslash | equalsLineBreaks;

                if (equalsBackslash != Vector512<byte>.Zero && equalsOtherExceptLineBreaks == Vector512<byte>.Zero)
                {
                    ulong mask = equalsBackslashOrLineBreak.ExtractMostSignificantBits();
                    uint shiftLeftCount = 0;
                    while (true)
                    {
                        int parLength;
                        byte index = (byte)BitOperations.TrailingZeroCount(mask);
                        if (index >= Vector512<byte>.Count - parMaxLength)
                        {
                            CopyVector_ParSupport(current, index, shiftLeftCount, plainText, false);
                            currentPos += index;
                            return;
                        }
                        else if (IsPar(current, ref currentSearchSpace, index, parUInt, out int length))
                        {
                            parLength = length;
                            CopyVector_ParSupport(current, index, shiftLeftCount, plainText, true);
                        }
                        else if (_isIgnoreChar[current[index]])
                        {
                            parLength = 1;
                            CopyVector_ParSupport(current, index, shiftLeftCount, plainText, false);
                        }
                        else
                        {
                            CopyVector_ParSupport(current, index, shiftLeftCount, plainText, false);
                            currentPos += index;
                            return;
                        }

                        shiftLeftCount = (uint)(index + parLength);
                        mask = ResetLowestSetBit(mask);
                    }
                }
                else if (equals != Vector512<byte>.Zero)
                {
                    ulong notEqualsElements = equals.ExtractMostSignificantBits();
                    int index = BitOperations.TrailingZeroCount(notEqualsElements);
                    if (index >= Vector256<byte>.Count)
                    {
                        Vector256<byte> current256 = current.GetLower();
                        CopyVector256(current256, plainText, ref currentPos);
                    }
                    else if (index >= Vector128<byte>.Count)
                    {
                        Vector128<byte> current128 = current.GetLower().GetLower();
                        CopyVector128(current128, plainText, ref currentPos);
                    }
                    return;
                }
                else
                {
                    CopyVector512(current, plainText, ref currentPos);
                }

                currentSearchSpace = ref Unsafe.Add(ref currentSearchSpace, Vector512<byte>.Count);
            } while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));
        }
        else if (Vector256.IsHardwareAccelerated && spanLength >= Vector256<byte>.Count)
        {
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, (uint)(spanLength - Vector256<byte>.Count));

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                Vector256<byte> current = Vector256.LoadUnsafe(ref currentSearchSpace);

                Vector256<byte> equalsBackslash =
                    Vector256.Equals(_backslashVector256, current);

                Vector256<byte> equalsBraces =
                    Vector256.Equals(_openBraceVector256, current) |
                    Vector256.Equals(_closingBraceVector256, current);

                Vector256<byte> equalsLineBreaks =
                    Vector256.Equals(_lfVector256, current) |
                    Vector256.Equals(_crVector256, current);

                Vector256<byte> equalsZeroBytes =
                    Vector256.Equals(_zeroVector256, current);

                Vector256<byte> equalsOtherExceptLineBreaks =
                    equalsZeroBytes |
                    equalsBraces;

                Vector256<byte> equals =
                    equalsZeroBytes |
                    equalsLineBreaks |
                    equalsBraces |
                    equalsBackslash;

                Vector256<byte> equalsBackslashOrLineBreak = equalsBackslash | equalsLineBreaks;

                if (equalsBackslash != Vector256<byte>.Zero && equalsOtherExceptLineBreaks == Vector256<byte>.Zero)
                {
                    uint mask = equalsBackslashOrLineBreak.ExtractMostSignificantBits();
                    uint shiftLeftCount = 0;
                    while (true)
                    {
                        int parLength;
                        byte index = (byte)BitOperations.TrailingZeroCount(mask);
                        if (index >= Vector256<byte>.Count - parMaxLength)
                        {
                            CopyVector_ParSupport(current, index, shiftLeftCount, plainText, false);
                            currentPos += index;
                            return;
                        }
                        else if (IsPar(current, ref currentSearchSpace, index, parUInt, out int length))
                        {
                            parLength = length;
                            CopyVector_ParSupport(current, index, shiftLeftCount, plainText, true);
                        }
                        else if (_isIgnoreChar[current[index]])
                        {
                            parLength = 1;
                            CopyVector_ParSupport(current, index, shiftLeftCount, plainText, false);
                        }
                        else
                        {
                            CopyVector_ParSupport(current, index, shiftLeftCount, plainText, false);
                            currentPos += index;
                            return;
                        }

                        shiftLeftCount = (uint)(index + parLength);
                        mask = ResetLowestSetBit(mask);
                    }
                }
                else if (equals != Vector256<byte>.Zero)
                {
                    uint notEqualsElements = equals.ExtractMostSignificantBits();
                    int index = BitOperations.TrailingZeroCount(notEqualsElements);
                    if (index >= Vector128<byte>.Count)
                    {
                        Vector128<byte> current128 = current.GetLower();
                        CopyVector128(current128, plainText, ref currentPos);
                    }
                    return;
                }
                else
                {
                    CopyVector256(current, plainText, ref currentPos);
                }

                currentSearchSpace = ref Unsafe.Add(ref currentSearchSpace, Vector256<byte>.Count);
            } while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));
        }
        else if (Vector128.IsHardwareAccelerated && spanLength >= Vector128<byte>.Count)
        {
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, (uint)(spanLength - Vector128<byte>.Count));

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                Vector128<byte> current = Vector128.LoadUnsafe(ref currentSearchSpace);

                Vector128<byte> equalsBackslash =
                    Vector128.Equals(_backslashVector128, current);

                Vector128<byte> equalsBraces =
                    Vector128.Equals(_openBraceVector128, current) |
                    Vector128.Equals(_closingBraceVector128, current);

                Vector128<byte> equalsLineBreaks =
                    Vector128.Equals(_lfVector128, current) |
                    Vector128.Equals(_crVector128, current);

                Vector128<byte> equalsZeroBytes =
                    Vector128.Equals(_zeroVector128, current);

                Vector128<byte> equalsOtherExceptLineBreaks =
                    equalsZeroBytes |
                    equalsBraces;

                Vector128<byte> equals =
                    equalsZeroBytes |
                    equalsLineBreaks |
                    equalsBraces |
                    equalsBackslash;

                Vector128<byte> equalsBackslashOrLineBreak = equalsBackslash | equalsLineBreaks;

                if (equalsBackslash != Vector128<byte>.Zero && equalsOtherExceptLineBreaks == Vector128<byte>.Zero)
                {
                    uint mask = equalsBackslashOrLineBreak.ExtractMostSignificantBits();
                    uint shiftLeftCount = 0;
                    while (true)
                    {
                        int parLength;
                        byte index = (byte)BitOperations.TrailingZeroCount(mask);
                        if (index >= Vector128<byte>.Count - parMaxLength)
                        {
                            CopyVector_ParSupport(current, index, shiftLeftCount, plainText, false);
                            currentPos += index;
                            return;
                        }
                        else if (IsPar(current, ref currentSearchSpace, index, parUInt, out int length))
                        {
                            parLength = length;
                            CopyVector_ParSupport(current, index, shiftLeftCount, plainText, true);
                        }
                        else if (_isIgnoreChar[current[index]])
                        {
                            parLength = 1;
                            CopyVector_ParSupport(current, index, shiftLeftCount, plainText, false);
                        }
                        else
                        {
                            CopyVector_ParSupport(current, index, shiftLeftCount, plainText, false);
                            currentPos += index;
                            return;
                        }

                        shiftLeftCount = (uint)(index + parLength);
                        mask = ResetLowestSetBit(mask);
                    }
                }
                else if (equals != Vector128<byte>.Zero)
                {
                    return;
                }
                else
                {
                    CopyVector128(current, plainText, ref currentPos);
                }

                currentSearchSpace = ref Unsafe.Add(ref currentSearchSpace, Vector128<byte>.Count);
            } while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));
        }

        // I think Vector128 should be supported on literally anything these days, but if it's not, just fall out
        // without doing anything and we'll take the non-SIMD path. We don't fall back to Vector64 because that's
        // slower than just doing the 8 bytes scalar.

        return;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CopyVector512(Vector512<byte> current, ListFast<char> plainText, ref int currentPos)
        {
            if (Vector512.Equals(_zeroVector512, current) != Vector512<byte>.Zero)
            {
                // Nulls are unlikely to occur, so just use a crappy scalar fallback instead of writing tons of
                // extra code.
                for (int i = 0; i < Vector512<byte>.Count; i++)
                {
                    byte b = current[i];
                    if (b != 0)
                    {
                        plainText.Add((char)b);
                    }
                }
            }
            else
            {
                (Vector512<ushort> lower, Vector512<ushort> upper) = Vector512.Widen(current);

                plainText.EnsureCapacity(plainText.Count + Vector512<byte>.Count);

                lower.CopyTo(Unsafe.As<char[], ushort[]>(ref plainText.ItemsArray), plainText.Count);
                upper.CopyTo(Unsafe.As<char[], ushort[]>(ref plainText.ItemsArray), plainText.Count + (Vector512<byte>.Count / 2));

                plainText.Count += Vector512<byte>.Count;
            }

            currentPos += Vector512<byte>.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CopyVector256(Vector256<byte> current, ListFast<char> plainText, ref int currentPos)
        {
            if (Vector256.Equals(_zeroVector256, current) != Vector256<byte>.Zero)
            {
                for (int i = 0; i < Vector256<byte>.Count; i++)
                {
                    byte b = current[i];
                    if (b != 0)
                    {
                        plainText.Add((char)b);
                    }
                }
            }
            else
            {
                (Vector256<ushort> lower, Vector256<ushort> upper) = Vector256.Widen(current);

                plainText.EnsureCapacity(plainText.Count + Vector256<byte>.Count);

                lower.CopyTo(Unsafe.As<char[], ushort[]>(ref plainText.ItemsArray), plainText.Count);
                upper.CopyTo(Unsafe.As<char[], ushort[]>(ref plainText.ItemsArray), plainText.Count + (Vector256<byte>.Count / 2));

                plainText.Count += Vector256<byte>.Count;
            }

            currentPos += Vector256<byte>.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CopyVector128(Vector128<byte> current, ListFast<char> plainText, ref int currentPos)
        {
            if (Vector128.Equals(_zeroVector128, current) != Vector128<byte>.Zero)
            {
                for (int i = 0; i < Vector128<byte>.Count; i++)
                {
                    byte b = current[i];
                    if (b != 0)
                    {
                        plainText.Add((char)b);
                    }
                }
            }
            else
            {
                (Vector128<ushort> lower, Vector128<ushort> upper) = Vector128.Widen(current);

                plainText.EnsureCapacity(plainText.Count + Vector128<byte>.Count);

                lower.CopyTo(Unsafe.As<char[], ushort[]>(ref plainText.ItemsArray), plainText.Count);
                upper.CopyTo(Unsafe.As<char[], ushort[]>(ref plainText.ItemsArray), plainText.Count + (Vector128<byte>.Count / 2));

                plainText.Count += Vector128<byte>.Count;
            }

            currentPos += Vector128<byte>.Count;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsPar(Vector512<byte> current, ref byte currentSearchSpace, byte index, uint parUInt, out int parLength)
    {
        if (Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref currentSearchSpace, index)) == parUInt &&
            (parLength = _isParEndingChar[current[index + 4]]) > 0)
        {
            return true;
        }
        else
        {
            parLength = 0;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsPar(Vector256<byte> current, ref byte currentSearchSpace, byte index, uint parUInt, out int parLength)
    {
        if (Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref currentSearchSpace, index)) == parUInt &&
            (parLength = _isParEndingChar[current[index + 4]]) > 0)
        {
            return true;
        }
        else
        {
            parLength = 0;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsPar(Vector128<byte> current, ref byte currentSearchSpace, byte index, uint parUInt, out int parLength)
    {
        if (Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref currentSearchSpace, index)) == parUInt &&
            (parLength = _isParEndingChar[current[index + 4]]) > 0)
        {
            return true;
        }
        else
        {
            parLength = 0;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector512<byte> ShiftVectorElementsLeft(Vector512<byte> v, uint count)
    {
        Vector512<byte> mask = Vector512.Add(_indexVec_512, Vector512.Create((byte)count));
        return Vector512.Shuffle(v, mask);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<byte> ShiftVectorElementsLeft(Vector256<byte> v, uint count)
    {
        Vector256<byte> mask = Vector256.Add(_indexVec_256, Vector256.Create((byte)count));
        return Vector256.Shuffle(v, mask);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> ShiftVectorElementsLeft(Vector128<byte> v, uint count)
    {
        Vector128<byte> mask = Vector128.Add(_indexVec_128, Vector128.Create((byte)count));
        return Vector128.Shuffle(v, mask);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CopyVector_ParSupport(
        Vector512<byte> current,
        int index,
        uint shiftLeftCount,
        ListFast<char> plainText,
        bool parFollows)
    {
        int diff = index - (int)shiftLeftCount;
        if (diff > 0)
        {
            Vector512<byte> maskVec = Vector512.GreaterThan(Vector512.Create((byte)index), _indexVec_512);
            Vector512<byte> working = Vector512.BitwiseAnd(current, maskVec);
            working = ShiftVectorElementsLeft(working, shiftLeftCount);

            (Vector512<ushort> lower, Vector512<ushort> upper) = Vector512.Widen(working);

            int vectorCount = Vector512<byte>.Count;
            plainText.EnsureCapacity(plainText.Count + vectorCount);

            lower.CopyTo(Unsafe.As<char[], ushort[]>(ref plainText.ItemsArray), plainText.Count);
            upper.CopyTo(Unsafe.As<char[], ushort[]>(ref plainText.ItemsArray), plainText.Count + (vectorCount / 2));

            plainText.Count += diff;
        }

        if (parFollows)
        {
            AddLineBreak();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CopyVector_ParSupport(
        Vector256<byte> current,
        int index,
        uint shiftLeftCount,
        ListFast<char> plainText,
        bool parFollows)
    {
        int diff = index - (int)shiftLeftCount;
        if (diff > 0)
        {
            Vector256<byte> maskVec = Vector256.GreaterThan(Vector256.Create((byte)index), _indexVec_256);
            Vector256<byte> working = Vector256.BitwiseAnd(current, maskVec);
            working = ShiftVectorElementsLeft(working, shiftLeftCount);

            (Vector256<ushort> lower, Vector256<ushort> upper) = Vector256.Widen(working);

            int vectorCount = Vector256<byte>.Count;
            plainText.EnsureCapacity(plainText.Count + vectorCount);

            lower.CopyTo(Unsafe.As<char[], ushort[]>(ref plainText.ItemsArray), plainText.Count);
            upper.CopyTo(Unsafe.As<char[], ushort[]>(ref plainText.ItemsArray), plainText.Count + (vectorCount / 2));

            plainText.Count += diff;
        }

        if (parFollows)
        {
            AddLineBreak();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CopyVector_ParSupport(
        Vector128<byte> current,
        int index,
        uint shiftLeftCount,
        ListFast<char> plainText,
        bool parFollows)
    {
        int diff = index - (int)shiftLeftCount;
        if (diff > 0)
        {
            Vector128<byte> maskVec = Vector128.GreaterThan(Vector128.Create((byte)index), _indexVec_128);
            Vector128<byte> working = Vector128.BitwiseAnd(current, maskVec);
            working = ShiftVectorElementsLeft(working, shiftLeftCount);

            (Vector128<ushort> lower, Vector128<ushort> upper) = Vector128.Widen(working);

            int vectorCount = Vector128<byte>.Count;
            plainText.EnsureCapacity(plainText.Count + vectorCount);

            lower.CopyTo(Unsafe.As<char[], ushort[]>(ref plainText.ItemsArray), plainText.Count);
            upper.CopyTo(Unsafe.As<char[], ushort[]>(ref plainText.ItemsArray), plainText.Count + (vectorCount / 2));

            plainText.Count += diff;
        }

        if (parFollows)
        {
            AddLineBreak();
        }
    }

    #endregion

    #region Helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ComputeFirstIndex(ref byte searchSpace, ref byte current, Vector512<byte> equals)
    {
        ulong notEqualsElements = equals.ExtractMostSignificantBits();
        int index = BitOperations.TrailingZeroCount(notEqualsElements);
        return index + (int)(nuint)Unsafe.ByteOffset(ref searchSpace, ref current);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ComputeFirstIndex(ref byte searchSpace, ref byte current, Vector256<byte> equals)
    {
        uint notEqualsElements = equals.ExtractMostSignificantBits();
        int index = BitOperations.TrailingZeroCount(notEqualsElements);
        return index + (int)(nuint)Unsafe.ByteOffset(ref searchSpace, ref current);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ComputeFirstIndex(ref byte searchSpace, ref byte current, Vector128<byte> equals)
    {
        uint notEqualsElements = equals.ExtractMostSignificantBits();
        int index = BitOperations.TrailingZeroCount(notEqualsElements);
        return index + (int)(nuint)Unsafe.ByteOffset(ref searchSpace, ref current);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ComputeFirstIndex(ref byte searchSpace, ref byte current, int index)
    {
        return index + (int)(nuint)Unsafe.ByteOffset(ref searchSpace, ref current);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint ResetLowestSetBit(uint value)
    {
        // It's lowered to BLSR on x86
        return value & (value - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong ResetLowestSetBit(ulong value)
    {
        // It's lowered to BLSR on x86
        return value & (value - 1);
    }

    #endregion
}
#endif
