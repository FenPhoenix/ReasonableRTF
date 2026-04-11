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

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    // Heavily modified version of SpanHelpers.IndexOfAnyValueType() from https://github.com/dotnet/maintenance-packages/
    // Made to handle the \binN situation while losing as little performance as possible.
    private static unsafe int SkipDest_Old(
        byte[] buffer,
        int startIndex,
        int count)
    {
        const byte openBraceByte = (byte)'{';
        const byte closingBraceByte = (byte)'}';
        const byte backslashByte = (byte)'\\';

        ReadOnlySpan<byte> span = buffer.AsSpan(startIndex, count);

        int length = span.Length;

        const uint uOpenBraceByte = openBraceByte; // Use uint for comparisons to avoid unnecessary 8->32 extensions
        const uint uClosingBraceByte = closingBraceByte; // Use uint for comparisons to avoid unnecessary 8->32 extensions
        const uint uBackslashByte = backslashByte; // Use uint for comparisons to avoid unnecessary 8->32 extensions
        IntPtr index = (IntPtr)0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
        IntPtr nLength = (IntPtr)length;

        ref byte searchSpace = ref MemoryMarshal.GetReference(span);

        if (Vector.IsHardwareAccelerated && length >= Vector<byte>.Count * 2)
        {
            int unaligned = (int)Unsafe.AsPointer(ref searchSpace) & (Vector<byte>.Count - 1);
            nLength = (IntPtr)((Vector<byte>.Count - unaligned) & (Vector<byte>.Count - 1));
        }

        SequentialScan:
        uint lookUp;
        while ((byte*)nLength >= (byte*)8)
        {
            nLength -= 8;

            lookUp = Unsafe.AddByteOffset(ref searchSpace, index);
            if (CountAsFound(ref searchSpace, lookUp, index))
                goto Found;
            lookUp = Unsafe.AddByteOffset(ref searchSpace, index + 1);
            if (CountAsFound(ref searchSpace, lookUp, index + 1))
                goto Found1;
            lookUp = Unsafe.AddByteOffset(ref searchSpace, index + 2);
            if (CountAsFound(ref searchSpace, lookUp, index + 2))
                goto Found2;
            lookUp = Unsafe.AddByteOffset(ref searchSpace, index + 3);
            if (CountAsFound(ref searchSpace, lookUp, index + 3))
                goto Found3;
            lookUp = Unsafe.AddByteOffset(ref searchSpace, index + 4);
            if (CountAsFound(ref searchSpace, lookUp, index + 4))
                goto Found4;
            lookUp = Unsafe.AddByteOffset(ref searchSpace, index + 5);
            if (CountAsFound(ref searchSpace, lookUp, index + 5))
                goto Found5;
            lookUp = Unsafe.AddByteOffset(ref searchSpace, index + 6);
            if (CountAsFound(ref searchSpace, lookUp, index + 6))
                goto Found6;
            lookUp = Unsafe.AddByteOffset(ref searchSpace, index + 7);
            if (CountAsFound(ref searchSpace, lookUp, index + 7))
                goto Found7;

            index += 8;
        }

        if ((byte*)nLength >= (byte*)4)
        {
            nLength -= 4;

            lookUp = Unsafe.AddByteOffset(ref searchSpace, index);
            if (CountAsFound(ref searchSpace, lookUp, index))
                goto Found;
            lookUp = Unsafe.AddByteOffset(ref searchSpace, index + 1);
            if (CountAsFound(ref searchSpace, lookUp, index + 1))
                goto Found1;
            lookUp = Unsafe.AddByteOffset(ref searchSpace, index + 2);
            if (CountAsFound(ref searchSpace, lookUp, index + 2))
                goto Found2;
            lookUp = Unsafe.AddByteOffset(ref searchSpace, index + 3);
            if (CountAsFound(ref searchSpace, lookUp, index + 3))
                goto Found3;

            index += 4;
        }

        while ((byte*)nLength > (byte*)0)
        {
            nLength -= 1;

            lookUp = Unsafe.AddByteOffset(ref searchSpace, index);
            if (CountAsFound_BoundsCheck(ref searchSpace, lookUp, index, length))
                goto Found;

            index += 1;
        }

        if (Vector.IsHardwareAccelerated && ((int)(byte*)index < length))
        {
            nLength = (IntPtr)((length - (int)(byte*)index) & ~(Vector<byte>.Count - 1));

            // Get comparison Vector
            Vector<byte> openBraceValues = GetVector(openBraceByte);
            Vector<byte> closingBraceValues = GetVector(closingBraceByte);
            Vector<byte> backslashValues = GetVector(backslashByte);

            while ((byte*)nLength > (byte*)index)
            {
                Vector<byte> vData = Unsafe.ReadUnaligned<Vector<byte>>(ref Unsafe.AddByteOffset(ref searchSpace, index));

                Vector<byte> equalsBraces = Vector.BitwiseOr(
                    Vector.Equals(vData, openBraceValues),
                    Vector.Equals(vData, closingBraceValues));
                Vector<byte> equalsBackslash = Vector.Equals(vData, backslashValues);
                Vector<byte> equals = Vector.BitwiseOr(equalsBraces, equalsBackslash);

                if (Vector<byte>.Zero.Equals(equals))
                {
                    index += Vector<byte>.Count;
                    continue;
                }

                if (!Vector<byte>.Zero.Equals(equalsBackslash))
                {
                    int backslashIndex = LocateFirstFoundByte(equalsBackslash);
                    int bracesIndex = LocateFirstFoundByte(equalsBraces);

                    if (backslashIndex < bracesIndex)
                    {
                        if (backslashIndex > Vector<byte>.Count - _binLength ||
                            (vData[backslashIndex + 1] == 'b' &&
                             vData[backslashIndex + 2] == 'i' &&
                             vData[backslashIndex + 3] == 'n'))
                        {
                            return startIndex + (int)(byte*)index + LocateFirstFoundByte(equals);
                        }
                    }

                    if (Vector<byte>.Zero.Equals(equalsBraces))
                    {
                        index += Vector<byte>.Count;
                        continue;
                    }
                    else
                    {
                        return startIndex + (int)(byte*)index + LocateFirstFoundByte(equals);
                    }
                }

                // Find offset of first match
                return startIndex + (int)(byte*)index + LocateFirstFoundByte(equals);
            }

            if ((int)(byte*)index < length)
            {
                nLength = (IntPtr)(length - (int)(byte*)index);
                goto SequentialScan;
            }
        }
        return -1;

        Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
        return startIndex + (int)(byte*)index;
        Found1:
        return startIndex + (int)(byte*)(index + 1);
        Found2:
        return startIndex + (int)(byte*)(index + 2);
        Found3:
        return startIndex + (int)(byte*)(index + 3);
        Found4:
        return startIndex + (int)(byte*)(index + 4);
        Found5:
        return startIndex + (int)(byte*)(index + 5);
        Found6:
        return startIndex + (int)(byte*)(index + 6);
        Found7:
        return startIndex + (int)(byte*)(index + 7);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool CountAsFound(ref byte searchSpace, uint lookUp, nint index)
        {
            return uOpenBraceByte == lookUp ||
                   uClosingBraceByte == lookUp ||
                   (uBackslashByte == lookUp &&
                    Unsafe.AddByteOffset(ref searchSpace, index + 1) == 'b' &&
                    Unsafe.AddByteOffset(ref searchSpace, index + 2) == 'i' &&
                    Unsafe.AddByteOffset(ref searchSpace, index + 3) == 'n');
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool CountAsFound_BoundsCheck(ref byte searchSpace, uint lookUp, nint index, int length)
        {
            return uOpenBraceByte == lookUp || uClosingBraceByte == lookUp ||
                   (uBackslashByte == lookUp &&
                    (index > length - _binLength ||
                     (Unsafe.AddByteOffset(ref searchSpace, index + 1) == 'b' &&
                      Unsafe.AddByteOffset(ref searchSpace, index + 2) == 'i' &&
                      Unsafe.AddByteOffset(ref searchSpace, index + 3) == 'n')));
        }
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
