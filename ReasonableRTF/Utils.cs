using System.Runtime.CompilerServices;

namespace ReasonableRTF;

internal static class Utils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsAsciiHex(this byte b) => char.IsAsciiHexDigit((char)b);

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

    /// <summary>
    /// Clears the dictionary and sets its internal storage to zero-length.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dictionary"></param>
    internal static void Reset<TKey, TValue>(this Dictionary<TKey, TValue> dictionary) where TKey : notnull
    {
        dictionary.Clear();
        dictionary.TrimExcess();
    }

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
}
