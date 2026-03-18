using System.Diagnostics.CodeAnalysis;

namespace ReasonableRTF;

internal static class ThrowHelper
{
    [DoesNotReturn]
    internal static void IndexOutOfRange() => throw new IndexOutOfRangeException();

    [DoesNotReturn]
    internal static void ArgumentException(string? message, string? paramName) => throw new ArgumentException(message, paramName);

    [DoesNotReturn]
    internal static void IOException(string message) => throw new IOException(message);

    [DoesNotReturn]
    internal static void EndOfStreamException(string message) => throw new EndOfStreamException(message);
}
