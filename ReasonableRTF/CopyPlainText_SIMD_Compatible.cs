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

#if NETSTANDARD2_0
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ReasonableRTF.Models.DataTypes;

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    private static readonly Vector<byte> _zeroVector = new((byte)'\0');
    private static readonly Vector<byte> _lfVector = new((byte)'\n');
    private static readonly Vector<byte> _crVector = new((byte)'\r');
    private static readonly Vector<byte> _backslashVector = new((byte)'\\');
    private static readonly Vector<byte> _openBraceVector = new((byte)'{');
    private static readonly Vector<byte> _closingBraceVector = new((byte)'}');

    // Heavily modified version of .NET SpanHelpers.IndexOfAnyValueType()
    private static void CopyPlainText_SIMD_Compatible(
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

        if (length >= Vector<byte>.Count)
        {
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, (uint)(length - Vector<byte>.Count));

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                Vector<byte> current = Unsafe.ReadUnaligned<Vector<byte>>(ref currentSearchSpace);

                Vector<byte> equals =
                    Vector.Equals(_zeroVector, current) |
                    Vector.Equals(_lfVector, current) |
                    Vector.Equals(_crVector, current) |
                    Vector.Equals(_backslashVector, current) |
                    Vector.Equals(_openBraceVector, current) |
                    Vector.Equals(_closingBraceVector, current);

                if (!Vector<byte>.Zero.Equals(equals))
                {
                    return;
                }

                CopyVector(current, plainText, ref currentPos);

                currentSearchSpace = ref Unsafe.Add(ref currentSearchSpace, Vector<byte>.Count);
            } while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));
        }

        // I think Vector128 should be supported on literally anything these days, but if it's not, just fall out
        // without doing anything and we'll take the non-SIMD path. We don't fall back to Vector64 because that's
        // slower than just doing the 8 bytes scalar.

        return;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void CopyVector(Vector<byte> current, ListFast<char> plainText, ref int currentPos)
        {
            Vector.Widen(current, out Vector<ushort> lower, out Vector<ushort> upper);

            int vectorCount = Vector<byte>.Count;
            plainText.EnsureCapacity(plainText.Count + vectorCount);

            lower.CopyTo(Unsafe.As<char[], ushort[]>(ref plainText.ItemsArray), plainText.Count);
            upper.CopyTo(Unsafe.As<char[], ushort[]>(ref plainText.ItemsArray), plainText.Count + (vectorCount / 2));

            plainText.Count += vectorCount;
            currentPos += vectorCount;
        }
    }
}
#endif
