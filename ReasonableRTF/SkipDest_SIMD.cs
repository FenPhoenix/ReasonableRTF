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

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    // Heavily modified version of .NET SpanHelpers.IndexOfAnyValueType().
    // Made to handle the \binN situation while losing as little performance as possible.
    private static int SkipDest_SIMD(
        byte[] buffer,
        int startIndex,
        int count)
    {
        const byte openBraceByte = (byte)'{';
        const byte closingBraceByte = (byte)'}';
        const byte backslashByte = (byte)'\\';

        ReadOnlySpan<byte> span = buffer.AsSpan(startIndex, count);

        int length = span.Length;

        if (Vector512.IsHardwareAccelerated && length >= Vector512<byte>.Count)
        {
            ref byte searchSpace = ref MemoryMarshal.GetReference(span);
            Vector512<byte> equalsBraces;
            Vector512<byte> equalsBackslash;
            Vector512<byte> equals;
            Vector512<byte> current;
            Vector512<byte> openBrace = Vector512.Create(openBraceByte);
            Vector512<byte> closingBrace = Vector512.Create(closingBraceByte);
            Vector512<byte> backslash = Vector512.Create(backslashByte);
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, length - Vector512<byte>.Count);

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                current = Vector512.LoadUnsafe(ref currentSearchSpace);
                equalsBraces = Vector512.Equals(openBrace, current) | Vector512.Equals(closingBrace, current);
                equalsBackslash = Vector512.Equals(backslash, current);
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

                    if (backslashIndex < bracesIndex)
                    {
                        if (backslashIndex > Vector256<byte>.Count - _binLength ||
                            (current[backslashIndex + 1] == 'b' &&
                             current[backslashIndex + 2] == 'i' &&
                             current[backslashIndex + 3] == 'n'))
                        {
                            return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
                        }

                        if (equalsBraces == Vector512<byte>.Zero)
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
            if ((uint)length % Vector512<byte>.Count != 0)
            {
                current = Vector512.LoadUnsafe(ref oneVectorAwayFromEnd);
                equalsBraces = Vector512.Equals(openBrace, current) | Vector512.Equals(closingBrace, current);
                equalsBackslash = Vector512.Equals(backslash, current);
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
            Vector256<byte> openBrace = Vector256.Create(openBraceByte);
            Vector256<byte> closingBrace = Vector256.Create(closingBraceByte);
            Vector256<byte> backslash = Vector256.Create(backslashByte);
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, length - Vector256<byte>.Count);

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                current = Vector256.LoadUnsafe(ref currentSearchSpace);
                equalsBraces = Vector256.Equals(openBrace, current) | Vector256.Equals(closingBrace, current);
                equalsBackslash = Vector256.Equals(backslash, current);
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

                    if (backslashIndex < bracesIndex)
                    {
                        if (backslashIndex > Vector256<byte>.Count - _binLength ||
                            (current[backslashIndex + 1] == 'b' &&
                             current[backslashIndex + 2] == 'i' &&
                             current[backslashIndex + 3] == 'n'))
                        {
                            return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
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
                equalsBraces = Vector256.Equals(openBrace, current) | Vector256.Equals(closingBrace, current);
                equalsBackslash = Vector256.Equals(backslash, current);
                equals = equalsBraces | equalsBackslash;
                if (equals != Vector256<byte>.Zero)
                {
                    return startIndex + ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                }
            }
        }
#if true
        else if (Vector128.IsHardwareAccelerated && length >= Vector128<byte>.Count)
        {
            ref byte searchSpace = ref MemoryMarshal.GetReference(span);
            Vector128<byte> equalsBraces;
            Vector128<byte> equalsBackslash;
            Vector128<byte> equals;
            Vector128<byte> current;
            Vector128<byte> openBrace = Vector128.Create(openBraceByte);
            Vector128<byte> closingBrace = Vector128.Create(closingBraceByte);
            Vector128<byte> backslash = Vector128.Create(backslashByte);
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, length - Vector128<byte>.Count);

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                current = Vector128.LoadUnsafe(ref currentSearchSpace);
                equalsBraces = Vector128.Equals(openBrace, current) | Vector128.Equals(closingBrace, current);
                equalsBackslash = Vector128.Equals(backslash, current);
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

                    if (backslashIndex < bracesIndex)
                    {
                        if (backslashIndex > Vector128<byte>.Count - _binLength ||
                            (current[backslashIndex + 1] == 'b' &&
                             current[backslashIndex + 2] == 'i' &&
                             current[backslashIndex + 3] == 'n'))
                        {
                            return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
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
                equalsBraces = Vector128.Equals(openBrace, current) | Vector128.Equals(closingBrace, current);
                equalsBackslash = Vector128.Equals(backslash, current);
                equals = equalsBraces | equalsBackslash;
                if (equals != Vector128<byte>.Zero)
                {
                    return startIndex + ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                }
            }
        }
#endif
        return -1;
    }

    // Compute index

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ComputeFirstIndex(ref byte searchSpace, ref byte current, Vector128<byte> equals)
    {
        uint notEqualsElements = equals.ExtractMostSignificantBits();
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
    private static int ComputeFirstIndex(ref byte searchSpace, ref byte current, Vector512<byte> equals)
    {
        ulong notEqualsElements = equals.ExtractMostSignificantBits();
        int index = BitOperations.TrailingZeroCount(notEqualsElements);
        return index + (int)(nuint)Unsafe.ByteOffset(ref searchSpace, ref current);
    }

    // Take precomputed index

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ComputeFirstIndex(ref byte searchSpace, ref byte current, int index)
    {
        return index + (int)(nuint)Unsafe.ByteOffset(ref searchSpace, ref current);
    }
}
#endif
