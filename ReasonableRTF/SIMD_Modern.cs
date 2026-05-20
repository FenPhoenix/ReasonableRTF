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
using System.Runtime.Intrinsics;
using ReasonableRTF.Helper;

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
    private static readonly Vector512<byte> _semicolonVector512 = Vector512.Create((byte)';');
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
    private static readonly Vector256<byte> _semicolonVector256 = Vector256.Create((byte)';');
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
    private static readonly Vector128<byte> _semicolonVector128 = Vector128.Create((byte)';');
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

    #endregion

    #region API

    // Heavily modified version of .NET SpanHelpers.IndexOfAnyValueType().
    // Made to handle the \binN situation while losing as little performance as possible.
    private int SIMD_SkipDest(
        ref byte bufferRef,
        int startIndex,
        int spanLength)
    {
        if (!Vector512.IsHardwareAccelerated &&
            !Vector256.IsHardwareAccelerated &&
            !Vector128.IsHardwareAccelerated)
        {
            return -1;
        }

        if (Vector512.IsHardwareAccelerated && spanLength >= Vector512<byte>.Count)
        {
            ref byte searchSpace = ref GetRefAtPos(ref bufferRef, startIndex);
            Vector512<byte> equalsBraces;
            Vector512<byte> equalsBackslash;
            Vector512<byte> equals;
            Vector512<byte> current;
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.AddByteOffset(ref searchSpace, spanLength - Vector512<byte>.Count);

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                current = Vector512.LoadUnsafe(ref currentSearchSpace);
                equalsBraces = Vector512.Equals(_openBraceVector512, current) | Vector512.Equals(_closingBraceVector512, current);
                equalsBackslash = Vector512.Equals(_backslashVector512, current);
                equals = equalsBraces | equalsBackslash;
                if (equals == Vector512<byte>.Zero)
                {
                    currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, Vector512<byte>.Count);
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
                        if (ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, Vector512<byte>.Count + (_binLength - 1)) <= spanLength)
                        {
                            Vector512<byte> lastBlock = Vector512.LoadUnsafe(ref Unsafe.AddByteOffset(ref currentSearchSpace, _binLength - 1));
                            Vector512<byte> lastEquals = Vector512.Equals(_nVector512, lastBlock);

                            ulong mask = Vector512.BitwiseAnd(equalsBackslash, lastEquals).ExtractMostSignificantBits();
                            while (mask != 0)
                            {
                                int index = ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, BitOperations.TrailingZeroCount(mask));
                                if (index >= spanLength - sizeof(uint) ||
                                    Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref searchSpace, index)) == _binUInt)
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
                                int spanIndex = ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, currentVectorIndex);
                                if (spanIndex >= spanLength - sizeof(uint) ||
                                    Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref searchSpace, spanIndex)) == _binUInt)
                                {
                                    return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, backslashIndex);
                                }
                                mask = ResetLowestSetBit(mask);
                                currentVectorIndex = BitOperations.TrailingZeroCount(mask);
                            }
                        }

                        if (!bracesFound)
                        {
                            currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, Vector512<byte>.Count);
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
            ref byte searchSpace = ref GetRefAtPos(ref bufferRef, startIndex);
            Vector256<byte> equalsBraces;
            Vector256<byte> equalsBackslash;
            Vector256<byte> equals;
            Vector256<byte> current;
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.AddByteOffset(ref searchSpace, spanLength - Vector256<byte>.Count);

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                current = Vector256.LoadUnsafe(ref currentSearchSpace);
                equalsBraces = Vector256.Equals(_openBraceVector256, current) | Vector256.Equals(_closingBraceVector256, current);
                equalsBackslash = Vector256.Equals(_backslashVector256, current);
                equals = equalsBraces | equalsBackslash;
                if (equals == Vector256<byte>.Zero)
                {
                    currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, Vector256<byte>.Count);
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
                        if (ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, Vector256<byte>.Count + (_binLength - 1)) <= spanLength)
                        {
                            Vector256<byte> lastBlock = Vector256.LoadUnsafe(ref Unsafe.AddByteOffset(ref currentSearchSpace, _binLength - 1));
                            Vector256<byte> lastEquals = Vector256.Equals(_nVector256, lastBlock);

                            uint mask = Vector256.BitwiseAnd(equalsBackslash, lastEquals).ExtractMostSignificantBits();
                            while (mask != 0)
                            {
                                int index = ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, BitOperations.TrailingZeroCount(mask));
                                if (index >= spanLength - sizeof(uint) ||
                                    Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref searchSpace, index)) == _binUInt)
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
                                int spanIndex = ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, currentVectorIndex);
                                if (spanIndex >= spanLength - sizeof(uint) ||
                                    Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref searchSpace, spanIndex)) == _binUInt)
                                {
                                    return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, backslashIndex);
                                }
                                mask = ResetLowestSetBit(mask);
                                currentVectorIndex = BitOperations.TrailingZeroCount(mask);
                            }
                        }

                        if (!bracesFound)
                        {
                            currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, Vector256<byte>.Count);
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
            ref byte searchSpace = ref GetRefAtPos(ref bufferRef, startIndex);
            Vector128<byte> equalsBraces;
            Vector128<byte> equalsBackslash;
            Vector128<byte> equals;
            Vector128<byte> current;
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.AddByteOffset(ref searchSpace, spanLength - Vector128<byte>.Count);

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                current = Vector128.LoadUnsafe(ref currentSearchSpace);
                equalsBraces = Vector128.Equals(_openBraceVector128, current) | Vector128.Equals(_closingBraceVector128, current);
                equalsBackslash = Vector128.Equals(_backslashVector128, current);
                equals = equalsBraces | equalsBackslash;
                if (equals == Vector128<byte>.Zero)
                {
                    currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, Vector128<byte>.Count);
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
                        if (ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, Vector128<byte>.Count + (_binLength - 1)) <= spanLength)
                        {
                            Vector128<byte> lastBlock = Vector128.LoadUnsafe(ref Unsafe.AddByteOffset(ref currentSearchSpace, _binLength - 1));
                            Vector128<byte> lastEquals = Vector128.Equals(_nVector128, lastBlock);

                            uint mask = Vector128.BitwiseAnd(equalsBackslash, lastEquals).ExtractMostSignificantBits();
                            while (mask != 0)
                            {
                                int index = ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, BitOperations.TrailingZeroCount(mask));
                                if (index >= spanLength - sizeof(uint) ||
                                    Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref searchSpace, index)) == _binUInt)
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
                                int spanIndex = ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, currentVectorIndex);
                                if (spanIndex >= spanLength - sizeof(uint) ||
                                    Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref searchSpace, spanIndex)) == _binUInt)
                                {
                                    return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, backslashIndex);
                                }
                                mask = ResetLowestSetBit(mask);
                                currentVectorIndex = BitOperations.TrailingZeroCount(mask);
                            }
                        }

                        if (!bracesFound)
                        {
                            currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, Vector128<byte>.Count);
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
    private bool SIMD_CopyPlainText(ref byte bufferRef)
    {
        if (!Vector.IsHardwareAccelerated)
        {
            return false;
        }

        int startIndex = _currentPos;
        int spanLength = _currentBufferChunkLength - _currentPos;

        ref byte searchSpace = ref GetRefAtPos(ref bufferRef, startIndex);

        if (Vector512.IsHardwareAccelerated && spanLength >= Vector512<byte>.Count)
        {
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.AddByteOffset(ref searchSpace, (uint)(spanLength - Vector512<byte>.Count));

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
                        if (index == Vector512<byte>.Count)
                        {
                            CopyVector_ParSupport(current, index, shiftLeftCount);
                            _currentPos += index;
                            break;
                        }
                        else if (index >= Vector512<byte>.Count - _parMaxLength)
                        {
                            if (_isIgnoreChar[GetByteAtPos(ref currentSearchSpace, index)])
                            {
                                parLength = 1;
                                CopyVector_ParSupport(current, index, shiftLeftCount);
                            }
                            else
                            {
                                CopyVector_ParSupport(current, index, shiftLeftCount);
                                _currentPos += index;
                                if (_currentPos < _currentBufferChunkLength - _parMaxLength &&
                                    Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref currentSearchSpace, index)) == _parUInt &&
                                    (parLength = _isParEndingChar[Unsafe.AddByteOffset(ref currentSearchSpace, index + 4)]) > 0)
                                {
                                    currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, index + parLength);
                                    _currentPos += parLength;
                                    AddLineBreak();
                                    goto outerLoop;
                                }

                                return true;
                            }
                        }
                        else if (IsPar(ref currentSearchSpace, index, _parUInt, out int length))
                        {
                            parLength = length;
                            CopyVector_ParSupport(current, index, shiftLeftCount);
                            AddLineBreak();
                        }
                        else if (_isIgnoreChar[GetByteAtPos(ref currentSearchSpace, index)])
                        {
                            parLength = 1;
                            CopyVector_ParSupport(current, index, shiftLeftCount);
                        }
                        else
                        {
                            CopyVector_ParSupport(current, index, shiftLeftCount);
                            _currentPos += index;
                            return true;
                        }

                        shiftLeftCount = (uint)(index + parLength);
                        mask = ResetLowestSetBit(mask);
                    }
                }
                else if (equals != Vector512<byte>.Zero)
                {
                    int index = BitOperations.TrailingZeroCount(equals.ExtractMostSignificantBits());
                    if (index == 0) return true;
                    CopyVector(current, index);
                    return true;
                }
                else
                {
                    CopyVector(current, Vector512<byte>.Count);
                }

                currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, Vector512<byte>.Count);
                outerLoop:;
            } while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));
        }
        else if (Vector256.IsHardwareAccelerated && spanLength >= Vector256<byte>.Count)
        {
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.AddByteOffset(ref searchSpace, (uint)(spanLength - Vector256<byte>.Count));

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
                        if (index == Vector256<byte>.Count)
                        {
                            CopyVector_ParSupport(current, index, shiftLeftCount);
                            _currentPos += index;
                            break;
                        }
                        else if (index >= Vector256<byte>.Count - _parMaxLength)
                        {
                            if (_isIgnoreChar[GetByteAtPos(ref currentSearchSpace, index)])
                            {
                                parLength = 1;
                                CopyVector_ParSupport(current, index, shiftLeftCount);
                            }
                            else
                            {
                                CopyVector_ParSupport(current, index, shiftLeftCount);
                                _currentPos += index;
                                if (_currentPos < _currentBufferChunkLength - _parMaxLength &&
                                    Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref currentSearchSpace, index)) == _parUInt &&
                                    (parLength = _isParEndingChar[Unsafe.AddByteOffset(ref currentSearchSpace, index + 4)]) > 0)
                                {
                                    currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, index + parLength);
                                    _currentPos += parLength;
                                    AddLineBreak();
                                    goto outerLoop;
                                }

                                return true;
                            }
                        }
                        else if (IsPar(ref currentSearchSpace, index, _parUInt, out int length))
                        {
                            parLength = length;
                            CopyVector_ParSupport(current, index, shiftLeftCount);
                            AddLineBreak();
                        }
                        else if (_isIgnoreChar[GetByteAtPos(ref currentSearchSpace, index)])
                        {
                            parLength = 1;
                            CopyVector_ParSupport(current, index, shiftLeftCount);
                        }
                        else
                        {
                            CopyVector_ParSupport(current, index, shiftLeftCount);
                            _currentPos += index;
                            return true;
                        }

                        shiftLeftCount = (uint)(index + parLength);
                        mask = ResetLowestSetBit(mask);
                    }
                }
                else if (equals != Vector256<byte>.Zero)
                {
                    int index = BitOperations.TrailingZeroCount(equals.ExtractMostSignificantBits());
                    if (index == 0) return true;
                    CopyVector(current, index);
                    return true;
                }
                else
                {
                    CopyVector(current, Vector256<byte>.Count);
                }

                currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, Vector256<byte>.Count);
                outerLoop:;
            } while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));
        }
        else if (Vector128.IsHardwareAccelerated && spanLength >= Vector128<byte>.Count)
        {
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.AddByteOffset(ref searchSpace, (uint)(spanLength - Vector128<byte>.Count));

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
                        byte index = (byte)UtilHelper.Vector128_TrailingZeroCount(mask);
                        if (index == Vector128<byte>.Count)
                        {
                            CopyVector_ParSupport(current, index, shiftLeftCount);
                            _currentPos += index;
                            break;
                        }
                        else if (index >= Vector128<byte>.Count - _parMaxLength)
                        {
                            if (_isIgnoreChar[GetByteAtPos(ref currentSearchSpace, index)])
                            {
                                parLength = 1;
                                CopyVector_ParSupport(current, index, shiftLeftCount);
                            }
                            else
                            {
                                CopyVector_ParSupport(current, index, shiftLeftCount);
                                _currentPos += index;
                                if (_currentPos < _currentBufferChunkLength - _parMaxLength &&
                                    Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref currentSearchSpace, index)) == _parUInt &&
                                    (parLength = _isParEndingChar[Unsafe.AddByteOffset(ref currentSearchSpace, index + 4)]) > 0)
                                {
                                    currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, index + parLength);
                                    _currentPos += parLength;
                                    AddLineBreak();
                                    goto outerLoop;
                                }

                                return true;
                            }
                        }
                        else if (IsPar(ref currentSearchSpace, index, _parUInt, out int length))
                        {
                            parLength = length;
                            CopyVector_ParSupport(current, index, shiftLeftCount);
                            AddLineBreak();
                        }
                        else if (_isIgnoreChar[GetByteAtPos(ref currentSearchSpace, index)])
                        {
                            parLength = 1;
                            CopyVector_ParSupport(current, index, shiftLeftCount);
                        }
                        else
                        {
                            CopyVector_ParSupport(current, index, shiftLeftCount);
                            _currentPos += index;
                            return true;
                        }

                        shiftLeftCount = (uint)(index + parLength);
                        mask = ResetLowestSetBit(mask);
                    }
                }
                else if (equals != Vector128<byte>.Zero)
                {
                    int index = UtilHelper.Vector128_TrailingZeroCount(equals.ExtractMostSignificantBits());
                    if (index == 0) return true;
                    CopyVector(current, index);
                    return true;
                }
                else
                {
                    CopyVector(current, Vector128<byte>.Count);
                }

                currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, Vector128<byte>.Count);
                outerLoop:;
            } while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));
        }

        // I think Vector128 should be supported on literally anything these days, but if it's not, just fall out
        // without doing anything and we'll take the non-SIMD path. We don't fall back to Vector64 because that's
        // slower than just doing the 8 bytes scalar.

        return false;

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CopyVector(Vector512<byte> current, int length)
    {
        (Vector512<ushort> lower, Vector512<ushort> upper) = Vector512.Widen(current);

        PlainText_EnsureCapacity(_plainText_Count + Vector512<byte>.Count);

        Unsafe.WriteUnaligned(ref Unsafe.As<char, byte>(ref _plainText[_plainText_Count]), lower);
        Unsafe.WriteUnaligned(ref Unsafe.As<char, byte>(ref _plainText[_plainText_Count + (Vector512<byte>.Count / 2)]), upper);

        _plainText_Count += length;
        _currentPos += length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CopyVector(Vector256<byte> current, int length)
    {
        (Vector256<ushort> lower, Vector256<ushort> upper) = Vector256.Widen(current);

        PlainText_EnsureCapacity(_plainText_Count + Vector256<byte>.Count);

        Unsafe.WriteUnaligned(ref Unsafe.As<char, byte>(ref _plainText[_plainText_Count]), lower);
        Unsafe.WriteUnaligned(ref Unsafe.As<char, byte>(ref _plainText[_plainText_Count + (Vector256<byte>.Count / 2)]), upper);

        _plainText_Count += length;
        _currentPos += length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CopyVector(Vector128<byte> current, int length)
    {
        (Vector128<ushort> lower, Vector128<ushort> upper) = Vector128.Widen(current);

        PlainText_EnsureCapacity(_plainText_Count + Vector128<byte>.Count);

        Unsafe.WriteUnaligned(ref Unsafe.As<char, byte>(ref _plainText[_plainText_Count]), lower);
        Unsafe.WriteUnaligned(ref Unsafe.As<char, byte>(ref _plainText[_plainText_Count + (Vector128<byte>.Count / 2)]), upper);

        _plainText_Count += length;
        _currentPos += length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsPar(ref byte currentSearchSpace, byte index, uint parUInt, out int parLength)
    {
        if (Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref currentSearchSpace, index)) == parUInt &&
            (parLength = _isParEndingChar[GetByteAtPos(ref currentSearchSpace, index + 4)]) > 0)
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
        uint shiftLeftCount)
    {
        int diff = index - (int)shiftLeftCount;
        if (diff > 0)
        {
            Vector512<byte> maskVec = Vector512.GreaterThan(Vector512.Create((byte)index), _indexVec_512);
            Vector512<byte> working = Vector512.BitwiseAnd(current, maskVec);
            working = ShiftVectorElementsLeft(working, shiftLeftCount);

            (Vector512<ushort> lower, Vector512<ushort> upper) = Vector512.Widen(working);

            PlainText_EnsureCapacity(_plainText_Count + Vector512<byte>.Count);

            Unsafe.WriteUnaligned(ref Unsafe.As<char, byte>(ref _plainText[_plainText_Count]), lower);
            Unsafe.WriteUnaligned(ref Unsafe.As<char, byte>(ref _plainText[_plainText_Count + (Vector512<byte>.Count / 2)]), upper);

            _plainText_Count += diff;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CopyVector_ParSupport(
        Vector256<byte> current,
        int index,
        uint shiftLeftCount)
    {
        int diff = index - (int)shiftLeftCount;
        if (diff > 0)
        {
            Vector256<byte> maskVec = Vector256.GreaterThan(Vector256.Create((byte)index), _indexVec_256);
            Vector256<byte> working = Vector256.BitwiseAnd(current, maskVec);
            working = ShiftVectorElementsLeft(working, shiftLeftCount);

            (Vector256<ushort> lower, Vector256<ushort> upper) = Vector256.Widen(working);

            PlainText_EnsureCapacity(_plainText_Count + Vector256<byte>.Count);

            Unsafe.WriteUnaligned(ref Unsafe.As<char, byte>(ref _plainText[_plainText_Count]), lower);
            Unsafe.WriteUnaligned(ref Unsafe.As<char, byte>(ref _plainText[_plainText_Count + (Vector256<byte>.Count / 2)]), upper);

            _plainText_Count += diff;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CopyVector_ParSupport(
        Vector128<byte> current,
        int index,
        uint shiftLeftCount)
    {
        int diff = index - (int)shiftLeftCount;
        if (diff > 0)
        {
            Vector128<byte> maskVec = Vector128.GreaterThan(Vector128.Create((byte)index), _indexVec_128);
            Vector128<byte> working = Vector128.BitwiseAnd(current, maskVec);
            working = ShiftVectorElementsLeft(working, shiftLeftCount);

            (Vector128<ushort> lower, Vector128<ushort> upper) = Vector128.Widen(working);

            PlainText_EnsureCapacity(_plainText_Count + Vector128<byte>.Count);

            Unsafe.WriteUnaligned(ref Unsafe.As<char, byte>(ref _plainText[_plainText_Count]), lower);
            Unsafe.WriteUnaligned(ref Unsafe.As<char, byte>(ref _plainText[_plainText_Count + (Vector128<byte>.Count / 2)]), upper);

            _plainText_Count += diff;
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
