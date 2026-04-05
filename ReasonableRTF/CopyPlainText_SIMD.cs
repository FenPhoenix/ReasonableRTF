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
    // Heavily modified version of .NET SpanHelpers.IndexOfAnyValueType()
    private static void CopyPlainText_SIMD(
        byte[] buffer,
        int startIndex,
        int count,
        byte[] nonPlainTextValues,
        ListFast<char> plainText,
        ref int currentPos)
    {
        ReadOnlySpan<byte> span = buffer.AsSpan(startIndex, count);

        int length = span.Length;

        ref byte searchSpace = ref MemoryMarshal.GetReference(span);
        ref byte valueRef = ref MemoryMarshal.GetReference(nonPlainTextValues);

        byte value0 = valueRef;
        byte value1 = Unsafe.Add(ref valueRef, 1);
        byte value2 = Unsafe.Add(ref valueRef, 2);
        byte value3 = Unsafe.Add(ref valueRef, 3);
        byte value4 = Unsafe.Add(ref valueRef, 4);
        byte value5 = Unsafe.Add(ref valueRef, 5);

        if (Vector512.IsHardwareAccelerated && length >= Vector512<byte>.Count)
        {
            Vector512<byte>
                values0 = Vector512.Create(value0),
                values1 = Vector512.Create(value1),
                values2 = Vector512.Create(value2),
                values3 = Vector512.Create(value3),
                values4 = Vector512.Create(value4),
                values5 = Vector512.Create(value5);
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, (uint)(length - Vector512<byte>.Count));

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                Vector512<byte> current = Vector512.LoadUnsafe(ref currentSearchSpace);
                Vector512<byte> equals =
                    Vector512.Equals(values0, current) |
                    Vector512.Equals(values1, current) |
                    Vector512.Equals(values2, current) |
                    Vector512.Equals(values3, current) |
                    Vector512.Equals(values4, current) |
                    Vector512.Equals(values5, current);

                if (equals != Vector512<byte>.Zero)
                {
                    ulong notEqualsElements = equals.ExtractMostSignificantBits();
                    int index = BitOperations.TrailingZeroCount(notEqualsElements);
                    if (index > Vector256<byte>.Count)
                    {
                        Vector256<byte> current256 = Vector256.LoadUnsafe(ref currentSearchSpace);
                        CopyVector256(current256, plainText, ref currentPos);
                    }
                    else if (index > Vector128<byte>.Count)
                    {
                        Vector128<byte> current128 = Vector128.LoadUnsafe(ref currentSearchSpace);
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
            Vector256<byte>
                values0 = Vector256.Create(value0),
                values1 = Vector256.Create(value1),
                values2 = Vector256.Create(value2),
                values3 = Vector256.Create(value3),
                values4 = Vector256.Create(value4),
                values5 = Vector256.Create(value5);
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, (uint)(length - Vector256<byte>.Count));

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                Vector256<byte> current = Vector256.LoadUnsafe(ref currentSearchSpace);
                Vector256<byte> equals =
                    Vector256.Equals(values0, current) |
                    Vector256.Equals(values1, current) |
                    Vector256.Equals(values2, current) |
                    Vector256.Equals(values3, current) |
                    Vector256.Equals(values4, current) |
                    Vector256.Equals(values5, current);

                if (equals != Vector256<byte>.Zero)
                {
                    uint notEqualsElements = equals.ExtractMostSignificantBits();
                    int index = BitOperations.TrailingZeroCount(notEqualsElements);
                    if (index > Vector128<byte>.Count)
                    {
                        Vector128<byte> current128 = Vector128.LoadUnsafe(ref currentSearchSpace);
                        CopyVector128(current128, plainText, ref currentPos);
                    }
                    return;
                }

                CopyVector256(current, plainText, ref currentPos);

                currentSearchSpace = ref Unsafe.Add(ref currentSearchSpace, Vector256<byte>.Count);
            } while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));
        }
        else if (Vector128<byte>.IsSupported && length >= Vector128<byte>.Count)
        {
            Vector128<byte>
                values0 = Vector128.Create(value0),
                values1 = Vector128.Create(value1),
                values2 = Vector128.Create(value2),
                values3 = Vector128.Create(value3),
                values4 = Vector128.Create(value4),
                values5 = Vector128.Create(value5);
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, (uint)(length - Vector128<byte>.Count));

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                Vector128<byte> current = Vector128.LoadUnsafe(ref currentSearchSpace);
                Vector128<byte> equals =
                    Vector128.Equals(values0, current) |
                    Vector128.Equals(values1, current) |
                    Vector128.Equals(values2, current) |
                    Vector128.Equals(values3, current) |
                    Vector128.Equals(values4, current) |
                    Vector128.Equals(values5, current);

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
}
#endif
