/*
 * MIT License
 * 
 * Copyright (c) 2024 Brian Tobin
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
*/

using System.Runtime.CompilerServices;
using ReasonableRTF.Models.DataTypes;

namespace ReasonableRTF.Helper;

internal static class UtilHelper
{
    /// <summary>
    /// Returns an array of type <typeparamref name="T"/> with all elements initialized to <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="length"></param>
    /// <param name="value">The value to initialize all elements with.</param>
    internal static T[] InitializedArray<T>(int length, T value) where T : new()
    {
        T[] ret = new T[length];
        for (int i = 0; i < length; i++)
        {
            ret[i] = value;
        }
        return ret;
    }

#if NET8_0_OR_GREATER
    /// <summary>
    /// Clears the dictionary and sets its internal storage to zero-length.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dictionary"></param>
    /// <param name="capacity"></param>
    internal static void Reset<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, int capacity) where TKey : notnull
    {
        dictionary.Clear();
        dictionary.TrimExcess(capacity);
    }
#endif

    /// <summary>
    /// Copy of .NET 7 version (fewer branches than Framework) but with a fast null return on fail instead of the infernal exception-throwing.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ListFast<char>? ConvertFromUtf32(uint utf32u, ListFast<char> charBuffer)
    {
        if (((utf32u - 0x110000u) ^ 0xD800u) < 0xFFEF0800u)
        {
            return null;
        }

        if (utf32u <= char.MaxValue)
        {
            charBuffer.ItemsArray[0] = (char)utf32u;
            charBuffer.Count = 1;
            return charBuffer;
        }

        charBuffer.ItemsArray[0] = (char)((utf32u + ((0xD800u - 0x40u) << 10)) >> 10);
        charBuffer.ItemsArray[1] = (char)((utf32u & 0x3FFu) + 0xDC00u);
        charBuffer.Count = 2;

        return charBuffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int Array_IndexOfByte_Fast(byte[] array, byte value, int startIndex, int count)
    {
        // On .NET, Array.IndexOf() uses crazy fast SIMD. On Framework, it normally doesn't.
#if NET8_0_OR_GREATER
        return Array.IndexOf(array, value, startIndex, count);
#else
        /*
        However, on Framework 64-bit only, we can make it use SIMD by using span.IndexOf(), if we reference the
        appropriate package (directly or indirectly), System.Memory or whatever it is.
        If we're 32-bit, though, SIMD is not supported, so we just stick to the regular Array.IndexOf(), which
        while substantially slower than the SIMD version, is still reasonably fast.

        But instead of checking for 64-bit vs. 32-bit, we can just check directly if SIMD is supported.
        */
        if (System.Numerics.Vector.IsHardwareAccelerated)
        {
            int index = array.AsSpan(startIndex, count).IndexOf(value);
            if (index > -1) index += startIndex;
            return index;
        }
        else
        {
            return Array.IndexOf(array, value, startIndex, count);
        }
#endif
    }

#if NETSTANDARD2_0
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int Array_IndexOfOpenOrClosingCurlyBrace_Fast(byte[] array, int startIndex, int count)
    {
        /*
        Very unfortunately, the accursed \binN keyword and its associated binary run - which can contain unescaped
        curly braces that must not be parsed as such - can appear anywhere. So we have to stop at every single
        backslash to check for it, and thus the spec cruelly yanks away performance we could almost taste. We're
        still way faster than before, though, so it's not that bad.
        */
        int index = array.AsSpan(startIndex, count).IndexOfAny((byte)'{', (byte)'}', (byte)'\\');
        if (index > -1) index += startIndex;
        return index;
    }
#endif

    internal static void ValidateArgs(byte[] source, int length)
    {
        if (length > source.Length)
        {
            throw new ArgumentException(nameof(length) + " is greater than the length of " + nameof(source) + ".", nameof(length));
        }
    }
}
