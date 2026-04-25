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
    private static readonly Vector512<byte> _bVector512 = Vector512.Create((byte)'b');
    private static readonly Vector512<byte> _iVector512 = Vector512.Create((byte)'i');
    private static readonly Vector512<byte> _nVector512 = Vector512.Create((byte)'n');

    private static readonly Vector256<byte> _zeroVector256 = Vector256.Create((byte)'\0');
    private static readonly Vector256<byte> _lfVector256 = Vector256.Create((byte)'\n');
    private static readonly Vector256<byte> _crVector256 = Vector256.Create((byte)'\r');
    private static readonly Vector256<byte> _backslashVector256 = Vector256.Create((byte)'\\');
    private static readonly Vector256<byte> _openBraceVector256 = Vector256.Create((byte)'{');
    private static readonly Vector256<byte> _closingBraceVector256 = Vector256.Create((byte)'}');
    private static readonly Vector256<byte> _bVector256 = Vector256.Create((byte)'b');
    private static readonly Vector256<byte> _iVector256 = Vector256.Create((byte)'i');
    private static readonly Vector256<byte> _nVector256 = Vector256.Create((byte)'n');

    private static readonly Vector128<byte> _zeroVector128 = Vector128.Create((byte)'\0');
    private static readonly Vector128<byte> _lfVector128 = Vector128.Create((byte)'\n');
    private static readonly Vector128<byte> _crVector128 = Vector128.Create((byte)'\r');
    private static readonly Vector128<byte> _backslashVector128 = Vector128.Create((byte)'\\');
    private static readonly Vector128<byte> _openBraceVector128 = Vector128.Create((byte)'{');
    private static readonly Vector128<byte> _closingBraceVector128 = Vector128.Create((byte)'}');
    private static readonly Vector128<byte> _bVector128 = Vector128.Create((byte)'b');
    private static readonly Vector128<byte> _iVector128 = Vector128.Create((byte)'i');
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

        ReadOnlySpan<byte> span = buffer.AsSpan(startIndex, count);

        int length = span.Length;

        if (Vector512.IsHardwareAccelerated && length >= Vector512<byte>.Count)
        {
            ref byte searchSpace = ref MemoryMarshal.GetReference(span);
            Vector512<byte> equalsBraces;
            Vector512<byte> equalsBackslash;
            Vector512<byte> equals;
            Vector512<byte> current;
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, length - Vector512<byte>.Count);

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                current = Vector512.LoadUnsafe(ref currentSearchSpace);
                equalsBraces = Vector512.Equals(_openBraceVector512, current) | Vector512.Equals(_closingBraceVector512, current);
                equalsBackslash = Vector512.Equals(_backslashVector512, current);
                equals = equalsBraces | equalsBackslash;
                if (equals == Vector512<byte>.Zero)
                {
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
                        if (backslashIndex > Vector512<byte>.Count - RtfToTextConverter._binLength)
                        {
                            return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
                        }

                        Vector512<byte> containsBinLetters =
                            Vector512.Equals(_bVector512, current) |
                            Vector512.Equals(_iVector512, current) |
                            Vector512.Equals(_nVector512, current);

                        if (containsBinLetters != Vector512<byte>.Zero)
                        {
                            ulong remainingSetBackslashBits = notEqualsElementsBackslash;
                            int currentBackslashIndex = backslashIndex;
                            while (remainingSetBackslashBits != 0)
                            {
                                if (currentBackslashIndex > Vector512<byte>.Count - RtfToTextConverter._binLength ||
                                    (current[currentBackslashIndex + 1] == 'b' &&
                                     current[currentBackslashIndex + 2] == 'i' &&
                                     current[currentBackslashIndex + 3] == 'n'))
                                {
                                    return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
                                }

                                remainingSetBackslashBits ^= 1u << currentBackslashIndex;
                                currentBackslashIndex = BitOperations.TrailingZeroCount(remainingSetBackslashBits);
                            }
                        }

                        if (equalsBraces == Vector512<byte>.Zero)
                        {
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
            if ((uint)length % Vector512<byte>.Count != 0)
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
        else if (Vector256.IsHardwareAccelerated && length >= Vector256<byte>.Count)
        {
            ref byte searchSpace = ref MemoryMarshal.GetReference(span);
            Vector256<byte> equalsBraces;
            Vector256<byte> equalsBackslash;
            Vector256<byte> equals;
            Vector256<byte> current;
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, length - Vector256<byte>.Count);

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                current = Vector256.LoadUnsafe(ref currentSearchSpace);
                equalsBraces = Vector256.Equals(_openBraceVector256, current) | Vector256.Equals(_closingBraceVector256, current);
                equalsBackslash = Vector256.Equals(_backslashVector256, current);
                equals = equalsBraces | equalsBackslash;
                if (equals == Vector256<byte>.Zero)
                {
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
                        if (backslashIndex > Vector256<byte>.Count - RtfToTextConverter._binLength)
                        {
                            return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
                        }

                        Vector256<byte> containsBinLetters =
                            Vector256.Equals(_bVector256, current) |
                            Vector256.Equals(_iVector256, current) |
                            Vector256.Equals(_nVector256, current);

                        if (containsBinLetters != Vector256<byte>.Zero)
                        {
                            uint remainingSetBackslashBits = notEqualsElementsBackslash;
                            int currentBackslashIndex = backslashIndex;
                            while (remainingSetBackslashBits != 0)
                            {
                                if (currentBackslashIndex > Vector256<byte>.Count - RtfToTextConverter._binLength ||
                                    (current[currentBackslashIndex + 1] == 'b' &&
                                     current[currentBackslashIndex + 2] == 'i' &&
                                     current[currentBackslashIndex + 3] == 'n'))
                                {
                                    return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
                                }

                                remainingSetBackslashBits ^= 1u << currentBackslashIndex;
                                currentBackslashIndex = BitOperations.TrailingZeroCount(remainingSetBackslashBits);
                            }
                        }

                        if (equalsBraces == Vector256<byte>.Zero)
                        {
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
            if ((uint)length % Vector256<byte>.Count != 0)
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
        else if (Vector128.IsHardwareAccelerated && length >= Vector128<byte>.Count)
        {
            ref byte searchSpace = ref MemoryMarshal.GetReference(span);
            Vector128<byte> equalsBraces;
            Vector128<byte> equalsBackslash;
            Vector128<byte> equals;
            Vector128<byte> current;
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, length - Vector128<byte>.Count);

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                current = Vector128.LoadUnsafe(ref currentSearchSpace);
                equalsBraces = Vector128.Equals(_openBraceVector128, current) | Vector128.Equals(_closingBraceVector128, current);
                equalsBackslash = Vector128.Equals(_backslashVector128, current);
                equals = equalsBraces | equalsBackslash;
                if (equals == Vector128<byte>.Zero)
                {
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
                        if (backslashIndex > Vector128<byte>.Count - RtfToTextConverter._binLength)
                        {
                            return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
                        }

                        Vector128<byte> containsBinLetters =
                            Vector128.Equals(_bVector128, current) |
                            Vector128.Equals(_iVector128, current) |
                            Vector128.Equals(_nVector128, current);

                        if (containsBinLetters != Vector128<byte>.Zero)
                        {
                            uint remainingSetBackslashBits = notEqualsElementsBackslash;
                            int currentBackslashIndex = backslashIndex;
                            while (remainingSetBackslashBits != 0)
                            {
                                if (currentBackslashIndex > Vector128<byte>.Count - RtfToTextConverter._binLength ||
                                    (current[currentBackslashIndex + 1] == 'b' &&
                                     current[currentBackslashIndex + 2] == 'i' &&
                                     current[currentBackslashIndex + 3] == 'n'))
                                {
                                    return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
                                }

                                remainingSetBackslashBits ^= 1u << currentBackslashIndex;
                                currentBackslashIndex = BitOperations.TrailingZeroCount(remainingSetBackslashBits);
                            }
                        }

                        if (equalsBraces == Vector128<byte>.Zero)
                        {
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
            if ((uint)length % Vector128<byte>.Count != 0)
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

    #endregion
}
#endif
