using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using static System.StringComparison;

namespace FenGen;

internal static partial class Misc
{
    #region Empty / whitespace checks

    /// <summary>
    /// Returns true if <paramref name="value"/> is null or empty.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsEmpty([NotNullWhen(false)] this string? value) => string.IsNullOrEmpty(value);

    /// <summary>
    /// Returns true if <paramref name="value"/> is null, empty, or whitespace.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsWhiteSpace([NotNullWhen(false)] this string? value) => string.IsNullOrWhiteSpace(value);

    #endregion

    #region StartsWith and EndsWith

    /// <summary>
    /// Ordinal case-sensitive.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool StartsWithO(this string str, string value) => str.StartsWith(value, Ordinal);

    /// <summary>
    /// Ordinal case-sensitive.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool EndsWithO(this string str, string value) => str.EndsWith(value, Ordinal);

    /// <summary>
    /// Ordinal case-sensitive.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static bool StartsWithOPlusWhiteSpace(this string str, string value)
    {
        int valLen;
        return str.StartsWith(value, Ordinal) &&
               str.Length > (valLen = value.Length) &&
               char.IsWhiteSpace(str[valLen]);
    }

    #endregion
}
