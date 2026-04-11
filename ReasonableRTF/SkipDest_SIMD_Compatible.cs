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

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    // Heavily modified version of .NET SpanHelpers.IndexOfAnyValueType().
    // Made to handle the \binN situation while losing as little performance as possible.
    private static int SkipDest_SIMD_Compatible(
        byte[] buffer,
        int startIndex,
        int count)
    {
        if (!Vector.IsHardwareAccelerated)
        {
            return -1;
        }

        ReadOnlySpan<byte> span = buffer.AsSpan(startIndex, count);

        int length = span.Length;

        if (length >= Vector<byte>.Count)
        {
            ref byte searchSpace = ref MemoryMarshal.GetReference(span);
            Vector<byte> equalsBraces;
            Vector<byte> equalsBackslash;
            Vector<byte> equals;
            Vector<byte> current;
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, length - Vector<byte>.Count);

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                current = Unsafe.ReadUnaligned<Vector<byte>>(ref currentSearchSpace);
                equalsBraces = Vector.Equals(_openBraceVector, current) | Vector.Equals(_closingBraceVector, current);
                equalsBackslash = Vector.Equals(_backslashVector, current);
                equals = equalsBraces | equalsBackslash;
                if (equals == Vector<byte>.Zero)
                {
                    currentSearchSpace = ref Unsafe.Add(ref currentSearchSpace, Vector<byte>.Count);
                    continue;
                }

                if (equalsBackslash != Vector<byte>.Zero)
                {
                    int backslashIndex = LocateFirstFoundByte(equalsBackslash);
                    int bracesIndex = LocateFirstFoundByte(equalsBraces);

                    if (backslashIndex < bracesIndex)
                    {
                        if (backslashIndex > Vector<byte>.Count - _binLength ||
                            (current[backslashIndex + 1] == 'b' &&
                             current[backslashIndex + 2] == 'i' &&
                             current[backslashIndex + 3] == 'n'))
                        {
                            return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
                        }

                        if (equalsBraces == Vector<byte>.Zero)
                        {
                            currentSearchSpace = ref Unsafe.Add(ref currentSearchSpace, Vector<byte>.Count);
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
            if ((uint)length % Vector<byte>.Count != 0)
            {
                current = Unsafe.ReadUnaligned<Vector<byte>>(ref oneVectorAwayFromEnd);
                equalsBraces = Vector.Equals(_openBraceVector, current) | Vector.Equals(_closingBraceVector, current);
                equalsBackslash = Vector.Equals(_backslashVector, current);
                equals = equalsBraces | equalsBackslash;
                if (equals != Vector<byte>.Zero)
                {
                    return startIndex + ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                }
            }
        }

        return -1;
    }

    // Compute index

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ComputeFirstIndex(ref byte searchSpace, ref byte current, Vector<byte> equals)
    {
        int index = LocateFirstFoundByte(equals);
        return index + (int)Unsafe.ByteOffset(ref searchSpace, ref current);
    }

    // Take precomputed index

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ComputeFirstIndex(ref byte searchSpace, ref byte current, int index)
    {
        return index + (int)Unsafe.ByteOffset(ref searchSpace, ref current);
    }

    // Vector sub-search adapted from https://github.com/aspnet/KestrelHttpServer/pull/1138
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int LocateFirstFoundByte(Vector<byte> match)
    {
        Vector<ulong> vector64 = Vector.AsVectorUInt64(match);
        ulong candidate = 0;
        int i = 0;
        // Pattern unrolled by jit https://github.com/dotnet/coreclr/pull/8001
        for (; i < Vector<ulong>.Count; i++)
        {
            candidate = vector64[i];
            if (candidate != 0)
            {
                break;
            }
        }

        // Single LEA instruction with jitted const (using function result)
        return i * 8 + LocateFirstFoundByte(candidate);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int LocateFirstFoundByte(ulong match)
    {
        // Flag least significant power of two bit
        ulong powerOfTwoFlag = match ^ (match - 1);
        // Shift all powers of two into the high byte and extract
        return (int)((powerOfTwoFlag * XorPowerOfTwoToHighByte) >> 57);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector<byte> GetVector(byte vectorByte)
    {
        // Vector<byte> .ctor doesn't become an intrinsic due to detection issue
        // However this does cause it to become an intrinsic (with additional multiply and reg->reg copy)
        // https://github.com/dotnet/coreclr/issues/7459#issuecomment-253965670
        return Vector.AsVectorByte(new Vector<uint>(vectorByte * 0x01010101u));
    }

    private const ulong XorPowerOfTwoToHighByte = (0x07ul |
                                                   0x06ul << 8 |
                                                   0x05ul << 16 |
                                                   0x04ul << 24 |
                                                   0x03ul << 32 |
                                                   0x02ul << 40 |
                                                   0x01ul << 48) + 1;
}
#endif
