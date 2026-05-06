using System.Diagnostics;

namespace ReasonableRTF;

internal static class GenAttributes
{
    [Conditional("nonexistent_so_ignore")]
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class FenGen_ParseKeywordAttribute : Attribute
    {
        internal FenGen_ParseKeywordAttribute(
            string getByteFunctionName,
            string bufferName,
            string incrementFunctionName)
        { }
    }
}
