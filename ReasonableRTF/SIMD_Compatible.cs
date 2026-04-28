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

#if !NET8_0_OR_GREATER
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ReasonableRTF.Models.DataTypes;

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    #region Private fields

    private static readonly Vector<byte> _zeroVector = new((byte)'\0');
    private static readonly Vector<byte> _lfVector = new((byte)'\n');
    private static readonly Vector<byte> _crVector = new((byte)'\r');
    private static readonly Vector<byte> _backslashVector = new((byte)'\\');
    private static readonly Vector<byte> _openBraceVector = new((byte)'{');
    private static readonly Vector<byte> _closingBraceVector = new((byte)'}');
    private static readonly Vector<byte> _nVector = new((byte)'n');

    private const ulong XorPowerOfTwoToHighByte = (0x07ul |
                                                   0x06ul << 8 |
                                                   0x05ul << 16 |
                                                   0x04ul << 24 |
                                                   0x03ul << 32 |
                                                   0x02ul << 40 |
                                                   0x01ul << 48) + 1;

    #endregion

    #region API

    // Heavily modified version of .NET SpanHelpers.IndexOfAnyValueType().
    // Made to handle the \binN situation while losing as little performance as possible.
    private static int SIMD_SkipDest(
        byte[] buffer,
        int startIndex,
        int spanLength)
    {
        if (!Vector.IsHardwareAccelerated)
        {
            return -1;
        }

        uint binUInt = BitConverter.IsLittleEndian ? 0x6E69625Cu : 0x5C62696Eu;
        int currentSpanPosition = 0;

        ReadOnlySpan<byte> span = buffer.AsSpan(startIndex, spanLength);

        if (spanLength >= Vector<byte>.Count)
        {
            ref byte searchSpace = ref MemoryMarshal.GetReference(span);
            Vector<byte> equalsBraces;
            Vector<byte> equalsBackslash;
            Vector<byte> equals;
            Vector<byte> current;
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, spanLength - Vector<byte>.Count);

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                current = Unsafe.ReadUnaligned<Vector<byte>>(ref currentSearchSpace);
                equalsBraces = Vector.Equals(_openBraceVector, current) | Vector.Equals(_closingBraceVector, current);
                equalsBackslash = Vector.Equals(_backslashVector, current);
                equals = equalsBraces | equalsBackslash;
                if (equals == Vector<byte>.Zero)
                {
                    currentSpanPosition += Vector<byte>.Count;
                    currentSearchSpace = ref Unsafe.Add(ref currentSearchSpace, Vector<byte>.Count);
                    continue;
                }

                if (equalsBackslash != Vector<byte>.Zero)
                {
                    int backslashIndex = -1;
                    int bracesIndex = 0;

                    bool bracesFound = equalsBraces != Vector<byte>.Zero;
                    if (!bracesFound || (backslashIndex = LocateFirstFoundByte(equalsBackslash)) < (bracesIndex = LocateFirstFoundByte(equalsBraces)))
                    {
                        Vector<ulong> vector64;
                        if (currentSpanPosition + Vector<byte>.Count + (_binLength - 1) <= spanLength)
                        {
                            Vector<byte> lastBlock = Unsafe.ReadUnaligned<Vector<byte>>(ref Unsafe.Add(ref currentSearchSpace, _binLength - 1));
                            Vector<byte> lastEquals = Vector.Equals(_nVector, lastBlock);

                            Vector<byte> containsBin = Vector.BitwiseAnd(equalsBackslash, lastEquals);

                            if (containsBin == Vector<byte>.Zero)
                            {
                                if (!bracesFound)
                                {
                                    currentSpanPosition += Vector<byte>.Count;
                                    currentSearchSpace = ref Unsafe.Add(ref currentSearchSpace, Vector<byte>.Count);
                                    continue;
                                }
                                else
                                {
                                    return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, bracesIndex);
                                }
                            }
                            else
                            {
                                vector64 = Vector.AsVectorUInt64(containsBin);
                            }
                        }
                        else
                        {
                            vector64 = Vector.AsVectorUInt64(equalsBackslash);
                        }

                        int currentVectorIndex = 0;
                        while (true)
                        {
                            currentVectorIndex = LocateFirstFoundByte(vector64, currentVectorIndex);
                            if (currentVectorIndex == -1) break;

                            int spanIndex = currentSpanPosition + currentVectorIndex;

                            if (spanIndex >= spanLength - sizeof(uint) ||
                                Unsafe.ReadUnaligned<uint>(ref Unsafe.Add(ref MemoryMarshal.GetReference(span), spanIndex)) == binUInt)
                            {
                                if (backslashIndex == -1) backslashIndex = LocateFirstFoundByte(equalsBackslash);
                                return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, backslashIndex);
                            }
                            ++currentVectorIndex;
                        }

                        if (!bracesFound)
                        {
                            currentSpanPosition += Vector<byte>.Count;
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
            if ((uint)spanLength % Vector<byte>.Count != 0)
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

    // Heavily modified version of .NET SpanHelpers.IndexOfAnyValueType().
    private static void SIMD_CopyPlainText(
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

        ReadOnlySpan<byte> span = buffer.AsSpan(startIndex, spanLength);

        ref byte searchSpace = ref MemoryMarshal.GetReference(span);

        if (spanLength >= Vector<byte>.Count)
        {
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.Add(ref searchSpace, (uint)(spanLength - Vector<byte>.Count));

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

    #endregion

    #region Helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ComputeFirstIndex(ref byte searchSpace, ref byte current, Vector<byte> equals)
    {
        int index = LocateFirstFoundByte(equals);
        return index + (int)Unsafe.ByteOffset(ref searchSpace, ref current);
    }

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

    // Vector sub-search adapted from https://github.com/aspnet/KestrelHttpServer/pull/1138
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int LocateFirstFoundByte(Vector<ulong> vector64, int start)
    {
        int i = start;
        // Pattern unrolled by jit https://github.com/dotnet/coreclr/pull/8001
        for (; i < Vector<ulong>.Count; i++)
        {
            ulong candidate = vector64[i];
            if (candidate != 0)
            {
                // Single LEA instruction with jitted const (using function result)
                return i * 8 + LocateFirstFoundByte(candidate);
            }
        }

        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int LocateFirstFoundByte(ulong match)
    {
        // Flag least significant power of two bit
        ulong powerOfTwoFlag = match ^ (match - 1);
        // Shift all powers of two into the high byte and extract
        return (int)((powerOfTwoFlag * XorPowerOfTwoToHighByte) >> 57);
    }

    #endregion
}
#endif
