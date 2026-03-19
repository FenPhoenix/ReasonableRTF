using System.Diagnostics.CodeAnalysis;

namespace ReasonableRTF;

// Total hack so we don't have to return and check a value eight trillion times (perf)
internal sealed class UnmatchedBraceException : Exception;

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

    [DoesNotReturn]
    internal static void UnmatchedBraceException() => throw new UnmatchedBraceException();
}
