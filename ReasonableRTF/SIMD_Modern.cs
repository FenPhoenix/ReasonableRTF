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

internal static partial class SIMD
{
    #region Private fields

    private static readonly Vector512<byte> _zeroVector512 = Vector512.Create((byte)'\0');
    private static readonly Vector512<byte> _lfVector512 = Vector512.Create((byte)'\n');
    private static readonly Vector512<byte> _crVector512 = Vector512.Create((byte)'\r');
    private static readonly Vector512<byte> _backslashVector512 = Vector512.Create((byte)'\\');
    private static readonly Vector512<byte> _openBraceVector512 = Vector512.Create((byte)'{');
    private static readonly Vector512<byte> _closingBraceVector512 = Vector512.Create((byte)'}');
    // We're searching for "bin" rather than "\bin" because the letters themselves are much less common than
    // backslashes, so we minimize our hit count. We still check for the backslash after we've confirmed we've
    // hit a "bin".
    private static readonly Vector512<byte> _bVector512 = Vector512.Create((byte)'b');
    private static readonly Vector512<byte> _nVector512 = Vector512.Create((byte)'n');

    private static readonly Vector256<byte> _zeroVector256 = Vector256.Create((byte)'\0');
    private static readonly Vector256<byte> _lfVector256 = Vector256.Create((byte)'\n');
    private static readonly Vector256<byte> _crVector256 = Vector256.Create((byte)'\r');
    private static readonly Vector256<byte> _backslashVector256 = Vector256.Create((byte)'\\');
    private static readonly Vector256<byte> _openBraceVector256 = Vector256.Create((byte)'{');
    private static readonly Vector256<byte> _closingBraceVector256 = Vector256.Create((byte)'}');
    private static readonly Vector256<byte> _bVector256 = Vector256.Create((byte)'b');
    private static readonly Vector256<byte> _nVector256 = Vector256.Create((byte)'n');

    private static readonly Vector128<byte> _zeroVector128 = Vector128.Create((byte)'\0');
    private static readonly Vector128<byte> _lfVector128 = Vector128.Create((byte)'\n');
    private static readonly Vector128<byte> _crVector128 = Vector128.Create((byte)'\r');
    private static readonly Vector128<byte> _backslashVector128 = Vector128.Create((byte)'\\');
    private static readonly Vector128<byte> _openBraceVector128 = Vector128.Create((byte)'{');
    private static readonly Vector128<byte> _closingBraceVector128 = Vector128.Create((byte)'}');
    private static readonly Vector128<byte> _bVector128 = Vector128.Create((byte)'b');
    private static readonly Vector128<byte> _nVector128 = Vector128.Create((byte)'n');

    #endregion

    #region API

    // Heavily modified version of .NET SpanHelpers.IndexOfAnyValueType().
    // Made to handle the \binN situation while losing as little performance as possible.
    internal static int SkipDest(
        byte[] buffer,
        int startIndex,
        int count)
    {
        if (!Vector512.IsHardwareAccelerated &&
            !Vector256.IsHardwareAccelerated &&
            !Vector128.IsHardwareAccelerated)
        {
            return -1;
        }

        const int binLettersLength = 3;
        uint binUint = BitConverter.IsLittleEndian ? 0x6E69625Cu : 0x5C62696Eu;
        int currentSpanPosition = 0;

        ReadOnlySpan<byte> span = buffer.AsSpan(startIndex, count);

        if (Vector512.IsHardwareAccelerated && count >= Vector512<byte>.Count)
        {
            ref byte searchSpace = ref MemoryMarshal.GetReference(span);
            Vector512<byte> equalsBraces;
            Vector512<byte> equalsBackslash;
            Vector512<byte> equals;
            Vector512<byte> current;
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, count - Vector512<byte>.Count);

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
                    int backslashIndex = BitOperations.TrailingZeroCount(notEqualsElementsBackslash);

                    ulong notEqualsElementsBraces = equalsBraces.ExtractMostSignificantBits();
                    int bracesIndex = BitOperations.TrailingZeroCount(notEqualsElementsBraces);

                    if (bracesIndex >= Vector512<byte>.Count || backslashIndex < bracesIndex)
                    {
                        if (currentSpanPosition + Vector512<byte>.Count + (binLettersLength - 1) <= count)
                        {
                            Vector512<byte> lastBlock = Vector512.LoadUnsafe(ref Unsafe.Add(ref currentSearchSpace, binLettersLength - 1));
                            Vector512<byte> firstEquals = Vector512.Equals(_bVector512, current);
                            Vector512<byte> lastEquals = Vector512.Equals(_nVector512, lastBlock);

                            ulong mask = Vector512.BitwiseAnd(firstEquals, lastEquals).ExtractMostSignificantBits();
                            while (mask != 0)
                            {
                                int index = currentSpanPosition + (BitOperations.TrailingZeroCount(mask) - 1);
                                if (index < 0 || index >= count - sizeof(uint) ||
                                    Unsafe.ReadUnaligned<uint>(in span[index]) == binUint)
                                {
                                    return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, backslashIndex);
                                }

                                mask = ResetLowestSetBit(mask);
                            }
                        }
                        else
                        {
                            if (TryFindBin(
                                    currentSpanPosition,
                                    Vector512<byte>.Count,
                                    span,
                                    currentSpanPosition,
                                    startIndex,
                                    count,
                                    ref searchSpace,
                                    ref currentSearchSpace,
                                    backslashIndex,
                                    binUint,
                                    out int ret))
                            {
                                return ret;
                            }
                        }

                        if (equalsBraces == Vector512<byte>.Zero)
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
            if ((uint)count % Vector512<byte>.Count != 0)
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
        else if (Vector256.IsHardwareAccelerated && count >= Vector256<byte>.Count)
        {
            ref byte searchSpace = ref MemoryMarshal.GetReference(span);
            Vector256<byte> equalsBraces;
            Vector256<byte> equalsBackslash;
            Vector256<byte> equals;
            Vector256<byte> current;
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, count - Vector256<byte>.Count);

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
                    int backslashIndex = BitOperations.TrailingZeroCount(notEqualsElementsBackslash);

                    uint notEqualsElementsBraces = equalsBraces.ExtractMostSignificantBits();
                    int bracesIndex = BitOperations.TrailingZeroCount(notEqualsElementsBraces);

                    if (bracesIndex >= Vector256<byte>.Count || backslashIndex < bracesIndex)
                    {
                        if (currentSpanPosition + Vector256<byte>.Count + (binLettersLength - 1) <= count)
                        {
                            Vector256<byte> lastBlock = Vector256.LoadUnsafe(ref Unsafe.Add(ref currentSearchSpace, binLettersLength - 1));
                            Vector256<byte> firstEquals = Vector256.Equals(_bVector256, current);
                            Vector256<byte> lastEquals = Vector256.Equals(_nVector256, lastBlock);

                            uint mask = Vector256.BitwiseAnd(firstEquals, lastEquals).ExtractMostSignificantBits();
                            while (mask != 0)
                            {
                                int index = currentSpanPosition + (BitOperations.TrailingZeroCount(mask) - 1);
                                if (index < 0 || index >= count - sizeof(uint) ||
                                    Unsafe.ReadUnaligned<uint>(in span[index]) == binUint)
                                {
                                    return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, backslashIndex);
                                }

                                mask = ResetLowestSetBit(mask);
                            }
                        }
                        else
                        {
                            if (TryFindBin(
                                    currentSpanPosition,
                                    Vector256<byte>.Count,
                                    span,
                                    currentSpanPosition,
                                    startIndex,
                                    count,
                                    ref searchSpace,
                                    ref currentSearchSpace,
                                    backslashIndex,
                                    binUint,
                                    out int ret))
                            {
                                return ret;
                            }
                        }

                        if (equalsBraces == Vector256<byte>.Zero)
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
            if ((uint)count % Vector256<byte>.Count != 0)
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
        else if (Vector128.IsHardwareAccelerated && count >= Vector128<byte>.Count)
        {
            ref byte searchSpace = ref MemoryMarshal.GetReference(span);
            Vector128<byte> equalsBraces;
            Vector128<byte> equalsBackslash;
            Vector128<byte> equals;
            Vector128<byte> current;
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, count - Vector128<byte>.Count);

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
                    int backslashIndex = BitOperations.TrailingZeroCount(notEqualsElementsBackslash);

                    uint notEqualsElementsBraces = equalsBraces.ExtractMostSignificantBits();
                    int bracesIndex = BitOperations.TrailingZeroCount(notEqualsElementsBraces);

                    if (bracesIndex >= Vector128<byte>.Count || backslashIndex < bracesIndex)
                    {
                        if (currentSpanPosition + Vector128<byte>.Count + (binLettersLength - 1) <= count)
                        {
                            Vector128<byte> lastBlock = Vector128.LoadUnsafe(ref Unsafe.Add(ref currentSearchSpace, binLettersLength - 1));
                            Vector128<byte> firstEquals = Vector128.Equals(_bVector128, current);
                            Vector128<byte> lastEquals = Vector128.Equals(_nVector128, lastBlock);

                            uint mask = Vector128.BitwiseAnd(firstEquals, lastEquals).ExtractMostSignificantBits();
                            while (mask != 0)
                            {
                                int index = currentSpanPosition + (BitOperations.TrailingZeroCount(mask) - 1);
                                if (index < 0 || index >= count - sizeof(uint) ||
                                    Unsafe.ReadUnaligned<uint>(in span[index]) == binUint)
                                {
                                    return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, backslashIndex);
                                }

                                mask = ResetLowestSetBit(mask);
                            }
                        }
                        else
                        {
                            if (TryFindBin(
                                    currentSpanPosition,
                                    Vector128<byte>.Count,
                                    span,
                                    currentSpanPosition,
                                    startIndex,
                                    count,
                                    ref searchSpace,
                                    ref currentSearchSpace,
                                    backslashIndex,
                                    binUint,
                                    out int ret))
                            {
                                return ret;
                            }
                        }

                        if (equalsBraces == Vector128<byte>.Zero)
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
            if ((uint)count % Vector128<byte>.Count != 0)
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

    // It's called extremely rarely, so it's okay that it's separated out
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryFindBin(
        int index,
        int sliceLength,
        ReadOnlySpan<byte> span,
        int currentSpanPosition,
        int startIndex,
        int count,
        ref byte searchSpace,
        ref byte currentSearchSpace,
        int backslashIndex,
        uint binUint,
        out int result)
    {
        while (true)
        {
            index = span.Slice(index, sliceLength).IndexOf((byte)'b');
            if (index == -1) break;

            int spanIndex = currentSpanPosition + (index - 1);
            if (spanIndex < 0 || spanIndex >= count - sizeof(uint) ||
                Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref MemoryMarshal.GetReference(span), spanIndex)) == binUint)
            {
                result = startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, backslashIndex);
                return true;
            }

            ++index;
            sliceLength -= index;
        }

        result = 0;
        return false;
    }

    // Heavily modified version of .NET SpanHelpers.IndexOfAnyValueType().
    internal static void CopyPlainText(
        byte[] buffer,
        int startIndex,
        int count,
        ListFast<char> plainText,
        ref int currentPos)
    {
        if (!Vector.IsHardwareAccelerated)
        {
            return;
        }

        ReadOnlySpan<byte> span = buffer.AsSpan(startIndex, count);

        int length = span.Length;

        ref byte searchSpace = ref MemoryMarshal.GetReference(span);

        if (Vector512.IsHardwareAccelerated && length >= Vector512<byte>.Count)
        {
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, (uint)(length - Vector512<byte>.Count));

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                Vector512<byte> current = Vector512.LoadUnsafe(ref currentSearchSpace);
                Vector512<byte> equals =
                    Vector512.Equals(_zeroVector512, current) |
                    Vector512.Equals(_lfVector512, current) |
                    Vector512.Equals(_crVector512, current) |
                    Vector512.Equals(_backslashVector512, current) |
                    Vector512.Equals(_openBraceVector512, current) |
                    Vector512.Equals(_closingBraceVector512, current);

                if (equals != Vector512<byte>.Zero)
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

                CopyVector512(current, plainText, ref currentPos);

                currentSearchSpace = ref Unsafe.Add(ref currentSearchSpace, Vector512<byte>.Count);
            } while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));
        }
        else if (Vector256.IsHardwareAccelerated && length >= Vector256<byte>.Count)
        {
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, (uint)(length - Vector256<byte>.Count));

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                Vector256<byte> current = Vector256.LoadUnsafe(ref currentSearchSpace);
                Vector256<byte> equals =
                    Vector256.Equals(_zeroVector256, current) |
                    Vector256.Equals(_lfVector256, current) |
                    Vector256.Equals(_crVector256, current) |
                    Vector256.Equals(_backslashVector256, current) |
                    Vector256.Equals(_openBraceVector256, current) |
                    Vector256.Equals(_closingBraceVector256, current);

                if (equals != Vector256<byte>.Zero)
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

                CopyVector256(current, plainText, ref currentPos);

                currentSearchSpace = ref Unsafe.Add(ref currentSearchSpace, Vector256<byte>.Count);
            } while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));
        }
        else if (Vector128.IsHardwareAccelerated && length >= Vector128<byte>.Count)
        {
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, (uint)(length - Vector128<byte>.Count));

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                Vector128<byte> current = Vector128.LoadUnsafe(ref currentSearchSpace);
                Vector128<byte> equals =
                    Vector128.Equals(_zeroVector128, current) |
                    Vector128.Equals(_lfVector128, current) |
                    Vector128.Equals(_crVector128, current) |
                    Vector128.Equals(_backslashVector128, current) |
                    Vector128.Equals(_openBraceVector128, current) |
                    Vector128.Equals(_closingBraceVector128, current);

                if (equals != Vector128<byte>.Zero)
                {
                    return;
                }

                CopyVector128(current, plainText, ref currentPos);

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
            (Vector512<ushort> lower, Vector512<ushort> upper) = Vector512.Widen(current);

            int vectorCount = Vector512<byte>.Count;
            plainText.EnsureCapacity(plainText.Count + vectorCount);

            lower.CopyTo(Unsafe.As<char[], ushort[]>(ref plainText.ItemsArray), plainText.Count);
            upper.CopyTo(Unsafe.As<char[], ushort[]>(ref plainText.ItemsArray), plainText.Count + (vectorCount / 2));

            plainText.Count += vectorCount;
            currentPos += vectorCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CopyVector256(Vector256<byte> current, ListFast<char> plainText, ref int currentPos)
        {
            (Vector256<ushort> lower, Vector256<ushort> upper) = Vector256.Widen(current);

            int vectorCount = Vector256<byte>.Count;
            plainText.EnsureCapacity(plainText.Count + vectorCount);

            lower.CopyTo(Unsafe.As<char[], ushort[]>(ref plainText.ItemsArray), plainText.Count);
            upper.CopyTo(Unsafe.As<char[], ushort[]>(ref plainText.ItemsArray), plainText.Count + (vectorCount / 2));

            plainText.Count += vectorCount;
            currentPos += vectorCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CopyVector128(Vector128<byte> current, ListFast<char> plainText, ref int currentPos)
        {
            (Vector128<ushort> lower, Vector128<ushort> upper) = Vector128.Widen(current);

            int vectorCount = Vector128<byte>.Count;
            plainText.EnsureCapacity(plainText.Count + vectorCount);

            lower.CopyTo(Unsafe.As<char[], ushort[]>(ref plainText.ItemsArray), plainText.Count);
            upper.CopyTo(Unsafe.As<char[], ushort[]>(ref plainText.ItemsArray), plainText.Count + (vectorCount / 2));

            plainText.Count += vectorCount;
            currentPos += vectorCount;
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
