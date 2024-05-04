using System.Diagnostics.CodeAnalysis;

namespace ReasonableRTF;

internal static class ThrowHelper
{
    [DoesNotReturn]
    internal static void IndexOutOfRange() => throw new IndexOutOfRangeException();
}
