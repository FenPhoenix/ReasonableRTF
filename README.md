# ReasonableRTF - Fast RTF to Plain Text Converter 🚀

A lightweight and performant C# library designed for rapidly converting **Rich Text Format (RTF)** files into **plain text**.

- - -

## Features

- Fast: Over 100x faster than RichTextBox
- Accurate: All characters converted correctly — even Wingdings
- Fully cross-platform
- Can read from forward-only streams such as DeflateStream
- [We even support this obscure nonsense](https://therealfenphoenix.wordpress.com/2024/01/05/rtf-character-encoding-who-needs-a-spec-anyway/)

- - -

## Installation

Install the library via NuGet:

```
dotnet add package ReasonableRTF.Standard
```

- - -

## Quick Start

Converting an RTF file to plain text is straightforward. The `Convert` method returns an **`RtfResult`** object, providing the converted text and comprehensive error information, if any.

### Basic Conversion

Here is how to convert an RTF file to a plain text string:

```cs
using ReasonableRTF;
using ReasonableRTF.Models;
using System.IO;

// ...

// 1. Initialize the converter
RtfToTextConverter converter = new RtfToTextConverter();

// 2. Load the RTF data from a file and convert
RtfResult result = converter.Convert("some_file.rtf");

if (result.Error == RtfError.OK)
{
    // Conversion was successful
    string plainText = result.Text;
    Console.WriteLine("Converted Text:\n" + plainText);
}
else
{
    // Handle conversion errors
    Console.WriteLine($"Conversion Error: {result.Error} at byte position: {result.BytePositionOfError}");
    if (result.Exception != null)
    {
        Console.WriteLine($"Exception Details: {result.Exception.Message}");
    }
}
```

- - -

## The RtfResult Class

The `RtfResult` object provides full details about the conversion process, ensuring you can robustly handle success and failure cases.

| Property | Type | Description |
| --- | --- | --- |
| `Text` | `string` | The converted plain text. |
| `Error` | `RtfError` | The error code. This will be `RtfError.OK` upon successful conversion. |
| `BytePositionOfError` | `int` | The approximate position in the data stream where the error occurred, or `-1` if no error. |
| `Exception` | `Exception?` | The caught exception, or `null` if no exception occurred during conversion. |

- - -

## Conversion Options

For more control over the output, you can provide an instance of the **`RtfToTextConverterOptions`** class to the `Convert` method. This allows customization of line breaks, special character handling, and hidden text inclusion.

### Applying Options

```cs
RtfToTextConverter converter = new RtfToTextConverter();
RtfToTextConverterOptions options = new RtfToTextConverterOptions
{
    ConvertHiddenText = true, // Include text marked as hidden
    LineBreakStyle = LineBreakStyle.LF // Use Unix-style line breaks
};

RtfResult result = converter.Convert("some_file.rtf", options);
// ... check result
```

### RtfToTextConverterOptions Properties

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `SwapUppercaseAndLowercasePhiSymbols` | `bool` | `true` | If set to `true`, swaps the uppercase and lowercase Greek phi characters in the Symbol font translation. This addresses a common reversal in the Windows Symbol font. |
| `SymbolFontA0Char` | `SymbolFontA0Char` | `SymbolFontA0Char.EuroSign` | Sets the character at index 0xA0 (160) in the Symbol font to Unicode translation table. Important for compatibility with older Symbol font versions. |
| `LineBreakStyle` | `LineBreakStyle` | `LineBreakStyle.EnvironmentDefault` | Specifies the line break style (CRLF, LF, or environment default) for the converted plain text output. |
| `ConvertHiddenText` | `bool` | `false` | Determines whether text marked as **hidden** in the RTF file should be included in the plain text output. |

- - -

## Benchmarks

### .NET 10 64-bit

```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8457/25H2/2025Update/HudsonValley2)
AMD Ryzen 5 5600 3.50GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.204
  [Host]     : .NET 10.0.8 (10.0.8, 10.0.826.23019), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.8 (10.0.8, 10.0.826.23019), X64 RyuJIT x86-64-v3


```
| Method                            | Mean         | Error     | StdDev    | Speed        |          |
|---------------------------------- |-------------:|----------:|----------:|-------------:|----------|
| RichTextBox_FullSet               | 3,331.340 ms | 6.2250 ms | 5.5183 ms |   43.59 MB/s | 1x       |
| RichTextBox_NoImageSet            | 1,432.217 ms | 3.7089 ms | 3.4693 ms |    2.47 MB/s | 1x       |
| ReasonableRTF_FullSet             |    17.925 ms | 0.0439 ms | 0.0411 ms | 8101.08 MB/s | 186x     |
| ReasonableRTF_NoImageSet          |     4.213 ms | 0.0123 ms | 0.0115 ms |  841.13 MB/s | 340x     |
| ReasonableRTF_FullSet_Streamed    |    19.705 ms | 0.0572 ms | 0.0535 ms | 7369.29 MB/s | 169x     |
| ReasonableRTF_NoImageSet_Streamed |     4.305 ms | 0.0098 ms | 0.0092 ms |  823.15 MB/s | 333x     |

### .NET Framework 4.8 64-bit

```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8246/25H2/2025Update/HudsonValley2)
AMD Ryzen 5 5600 3.50GHz, 1 CPU, 12 logical and 6 physical cores
  [Host]     : .NET Framework 4.8.1 (4.8.9325.0), X64 RyuJIT VectorSize=256
  DefaultJob : .NET Framework 4.8.1 (4.8.9325.0), X64 RyuJIT VectorSize=256


```
| Method                            | Mean         | Error     | StdDev    | Speed        |          |
|---------------------------------- |-------------:|----------:|----------:|-------------:|----------|
| RichTextBox_FullSet               | 2,779.775 ms | 3.9318 ms | 3.2833 ms |   52.24 MB/s | 1x       |
| RichTextBox_NoImageSet            |   992.237 ms | 2.5478 ms | 2.2585 ms |    3.57 MB/s | 1x       |
| ReasonableRTF_FullSet             |    23.410 ms | 0.0405 ms | 0.0338 ms | 6202.98 MB/s | 119x     |
| ReasonableRTF_NoImageSet          |     6.005 ms | 0.0192 ms | 0.0179 ms |  590.12 MB/s | 165x     |
| ReasonableRTF_FullSet_Streamed    |    26.056 ms | 0.0703 ms | 0.0658 ms | 5573.06 MB/s | 107x     |
| ReasonableRTF_NoImageSet_Streamed |     6.131 ms | 0.0095 ms | 0.0089 ms |  577.99 MB/s | 162x     |

### .NET Framework 4.8 32-bit

```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8246/25H2/2025Update/HudsonValley2)
AMD Ryzen 5 5600 3.50GHz, 1 CPU, 12 logical and 6 physical cores
  [Host]     : .NET Framework 4.8.1 (4.8.9325.0), X86 LegacyJIT
  DefaultJob : .NET Framework 4.8.1 (4.8.9325.0), X86 LegacyJIT


```
| Method                            | Mean         | Error       | StdDev      | Speed        |          |
|---------------------------------- |-------------:|------------:|------------:|-------------:|----------|
| RichTextBox_FullSet               | 6,932.056 ms | 131.6848 ms | 140.9013 ms |   20.95 MB/s | 1x       |
| RichTextBox_NoImageSet            | 2,885.139 ms |  57.0121 ms |  81.7651 ms |    1.23 MB/s | 1x       |
| ReasonableRTF_FullSet             |    43.566 ms |   0.0682 ms |   0.0638 ms | 3333.14 MB/s | 159x     |
| ReasonableRTF_NoImageSet          |     9.030 ms |   0.0099 ms |   0.0093 ms |  392.43 MB/s | 320x     |
| ReasonableRTF_FullSet_Streamed    |    48.786 ms |   0.1351 ms |   0.1264 ms | 2976.50 MB/s | 142x     |
| ReasonableRTF_NoImageSet_Streamed |     9.282 ms |   0.0304 ms |   0.0284 ms |  381.78 MB/s | 311x     |

- - -

## Supported RTF features

### Supported

- All basic plain text, hex-encoded chars, Unicode-encoded chars
- Symbol fonts (Wingdings 1, 2 and 3, Webdings, Symbol, and Zapf Dingbats) converted to Unicode equivalents
- Characters specified as "SYMBOL" field instructions
- Undocumented use of the \langN keyword to [specify character encoding](https://therealfenphoenix.wordpress.com/2024/01/05/rtf-character-encoding-who-needs-a-spec-anyway/) - old versions of RichTextBox used to support this

### Partially supported

- Tables: Cells and rows have spaces between them, but not much functionality beyond that.
- Lists: Numbers and bullets show up (that's better than RichTextBox most of the time), but indentation usually doesn't.

### Not currently supported

- Footnotes
- "HYPERLINK" field instruction value
- Math objects

- - -

## License and Attribution

### Code License

The **original code** for this RTF converter was written by **Brian Tobin** and is licensed under the **MIT License** (Copyright 2024-2026 Brian Tobin). For the full license text, please refer to the [LICENSE file ReasonableRTF](https://github.com/FenPhoenix/ReasonableRTF/blob/main/LICENSE).

[Flamifly](https://github.com/Flamifly) contributed to the readme (installation, quick start, documentation, etc), and created the .NET Standard version.
