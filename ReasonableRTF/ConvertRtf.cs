using ReasonableRTF.Models;

namespace ReasonableRTF;

public static class ConvertRtf
{
    #region Byte array

    /// <summary>
    /// Converts a byte array of RTF data into plain text.
    /// </summary>
    /// <param name="source">The byte array containing the RTF to convert.</param>
    /// <returns>An <see cref="RtfResult"/> containing the converted plain text, or error information if the conversion was not successful.</returns>
    public static RtfResult ToText(byte[] source)
    {
        RtfToTextConverter converter = new();
        return converter.Convert(source);
    }

    /// <summary>
    /// Converts a byte array of RTF data into plain text.
    /// </summary>
    /// <param name="source">The byte array containing the RTF to convert.</param>
    /// <param name="options">A set of options.</param>
    /// <returns>An <see cref="RtfResult"/> containing the converted plain text, or error information if the conversion was not successful.</returns>
    public static RtfResult ToText(byte[] source, RtfToTextConverterOptions options)
    {
        RtfToTextConverter converter = new();
        return converter.Convert(source, options);
    }

    /// <summary>
    /// Converts a byte array of RTF data into plain text.
    /// </summary>
    /// <param name="source">The byte array containing the RTF to convert.</param>
    /// <param name="length">The maximum number of bytes to read from the RTF byte array.</param>
    /// <returns>An <see cref="RtfResult"/> containing the converted plain text, or error information if the conversion was not successful.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static RtfResult ToText(byte[] source, int length)
    {
        RtfToTextConverter converter = new();
        return converter.Convert(source, length);
    }

    /// <summary>
    /// Converts a byte array of RTF data into plain text.
    /// </summary>
    /// <param name="source">The byte array containing the RTF to convert.</param>
    /// <param name="length">The maximum number of bytes to read from the RTF byte array.</param>
    /// <param name="options">A set of options.</param>
    /// <returns>An <see cref="RtfResult"/> containing the converted plain text, or error information if the conversion was not successful.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static RtfResult ToText(byte[] source, int length, RtfToTextConverterOptions options)
    {
        RtfToTextConverter converter = new();
        return converter.Convert(source, length, options);
    }

    #endregion

    #region Filename

    /// <summary>
    /// Converts an RTF file into plain text.
    /// </summary>
    /// <param name="fileName">The path to the RTF file to convert.</param>
    /// <returns>An <see cref="RtfResult"/> containing the converted plain text, or error information if the conversion was not successful.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="PathTooLongException"></exception>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="IOException"></exception>
    public static RtfResult ToText(string fileName)
    {
        RtfToTextConverter converter = new();
        return converter.Convert(fileName);
    }

    /// <summary>
    /// Converts an RTF file into plain text.
    /// </summary>
    /// <param name="fileName">The path to the RTF file to convert.</param>
    /// <param name="bufferSize">The size of the buffer to use during streaming. The default value is 81920.</param>
    /// <returns>An <see cref="RtfResult"/> containing the converted plain text, or error information if the conversion was not successful.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="PathTooLongException"></exception>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="IOException"></exception>
    public static RtfResult ToText(string fileName, int bufferSize)
    {
        RtfToTextConverter converter = new();
        return converter.Convert(fileName, bufferSize);
    }

    /// <summary>
    /// Converts an RTF file into plain text.
    /// </summary>
    /// <param name="fileName">The path to the RTF file to convert.</param>
    /// <param name="options">A set of options.</param>
    /// <returns>An <see cref="RtfResult"/> containing the converted plain text, or error information if the conversion was not successful.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="PathTooLongException"></exception>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="IOException"></exception>
    public static RtfResult ToText(string fileName, RtfToTextConverterOptions options)
    {
        RtfToTextConverter converter = new();
        return converter.Convert(fileName, options);
    }

    /// <summary>
    /// Converts an RTF file into plain text.
    /// </summary>
    /// <param name="fileName">The path to the RTF file to convert.</param>
    /// <param name="bufferSize">The size of the buffer to use during streaming. The default value is 81920.</param>
    /// <param name="options">A set of options.</param>
    /// <returns>An <see cref="RtfResult"/> containing the converted plain text, or error information if the conversion was not successful.</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="PathTooLongException"></exception>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="IOException"></exception>
    public static RtfResult ToText(string fileName, int bufferSize, RtfToTextConverterOptions options)
    {
        RtfToTextConverter converter = new();
        return converter.Convert(fileName, bufferSize, options);
    }

    #endregion

    #region Stream

    /// <summary>
    /// Converts a stream of RTF data into plain text.
    /// </summary>
    /// <param name="stream">A stream containing the RTF to convert.</param>
    /// <returns>An <see cref="RtfResult"/> containing the converted plain text, or error information if the conversion was not successful.</returns>
    public static RtfResult ToText(Stream stream)
    {
        RtfToTextConverter converter = new();
        return converter.Convert(stream);
    }

    /// <summary>
    /// Converts a stream of RTF data into plain text.
    /// </summary>
    /// <param name="stream">A stream containing the RTF to convert.</param>
    /// <param name="bufferSize">The size of the buffer to use during streaming. The default value is 81920.</param>
    /// <returns>An <see cref="RtfResult"/> containing the converted plain text, or error information if the conversion was not successful.</returns>
    public static RtfResult ToText(Stream stream, int bufferSize)
    {
        RtfToTextConverter converter = new();
        return converter.Convert(stream, bufferSize);
    }

    /// <summary>
    /// Converts a stream of RTF data into plain text.
    /// </summary>
    /// <param name="stream">A stream containing the RTF to convert.</param>
    /// <param name="options">A set of options.</param>
    /// <returns>An <see cref="RtfResult"/> containing the converted plain text, or error information if the conversion was not successful.</returns>
    public static RtfResult ToText(Stream stream, RtfToTextConverterOptions options)
    {
        RtfToTextConverter converter = new();
        return converter.Convert(stream, options);
    }

    /// <summary>
    /// Converts a stream of RTF data into plain text.
    /// </summary>
    /// <param name="stream">A stream containing the RTF to convert.</param>
    /// <param name="bufferSize">The size of the buffer to use during streaming. The default value is 81920.</param>
    /// <param name="options">A set of options.</param>
    /// <returns>An <see cref="RtfResult"/> containing the converted plain text, or error information if the conversion was not successful.</returns>
    public static RtfResult ToText(Stream stream, int bufferSize, RtfToTextConverterOptions options)
    {
        RtfToTextConverter converter = new();
        return converter.Convert(stream, bufferSize, options);
    }

    #endregion
}
