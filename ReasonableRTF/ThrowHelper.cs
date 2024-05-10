using System.Diagnostics.CodeAnalysis;

namespace ReasonableRTF;

internal static class ThrowHelper
{
    [DoesNotReturn]
    internal static void IndexOutOfRange() => throw new IndexOutOfRangeException();

    [DoesNotReturn]
    internal static void ArgumentException(string? message, string? paramName) => throw new ArgumentException(message, paramName);
}
