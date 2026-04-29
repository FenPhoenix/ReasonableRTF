# ReasonableRTF - Fast RTF to Plain Text Converter 🚀

A lightweight and performant C# library designed for rapidly converting **Rich Text Format (RTF)** files into **plain text**.

- - -

## Features

- Fast: Over 100x faster than RichTextBox.
- Accurate: All characters converted correctly — even Wingdings.
- Small: Less than 60k.
- Fully cross-platform.
- Can read from forward-only streams such as DeflateStream.
- [We even support this obscure nonsense](https://therealfenphoenix.wordpress.com/2024/01/05/rtf-character-encoding-who-needs-a-spec-anyway/).

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

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8246/25H2/2025Update/HudsonValley2)
AMD Ryzen 5 5600 3.50GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.203
  [Host]     : .NET 10.0.7 (10.0.7, 10.0.726.21808), X64 RyuJIT x86-64-v3
  Job-KEOOAO : .NET 10.0.7 (10.0.7, 10.0.726.21808), X64 RyuJIT x86-64-v3

OutlierMode=DontRemove  MemoryRandomization=True  

```
| Method                            | Mean         | Error     | StdDev    | Speed        |          |
|---------------------------------- |-------------:|----------:|----------:|-------------:|----------|
| RichTextBox_FullSet               | 3,331.340 ms | 6.2250 ms | 5.5183 ms |   43.59 MB/s | 1x       |
| RichTextBox_NoImageSet            | 1,432.217 ms | 3.7089 ms | 3.4693 ms |    2.47 MB/s | 1x       |
| ReasonableRTF_FullSet             |    20.582 ms | 0.0269 ms | 0.0252 ms | 7055.28 MB/s | 162x     |
| ReasonableRTF_NoImageSet          |     5.187 ms | 0.0096 ms | 0.0089 ms |  683.18 MB/s | 276x     |
| ReasonableRTF_FullSet_Streamed    |    22.475 ms | 0.0574 ms | 0.0537 ms | 6461.04 MB/s | 148x     |
| ReasonableRTF_NoImageSet_Streamed |     5.201 ms | 0.0143 ms | 0.0133 ms |  681.34 MB/s | 275x     |

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
| ReasonableRTF_FullSet             |    28.226 ms | 0.0418 ms | 0.0349 ms | 5144.61 MB/s | 99x      |
| ReasonableRTF_NoImageSet          |     7.703 ms | 0.0187 ms | 0.0175 ms |  460.04 MB/s | 129x     |
| ReasonableRTF_FullSet_Streamed    |    30.723 ms | 0.0665 ms | 0.0555 ms | 4726.48 MB/s | 91x      |
| ReasonableRTF_NoImageSet_Streamed |     7.811 ms | 0.0057 ms | 0.0053 ms |  453.68 MB/s | 127x     |

### .NET Framework 4.8 32-bit

```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8039/25H2/2025Update/HudsonValley2)
AMD Ryzen 5 5600 3.50GHz, 1 CPU, 12 logical and 6 physical cores
  [Host]     : .NET Framework 4.8.1 (4.8.9325.0), X86 LegacyJIT
  DefaultJob : .NET Framework 4.8.1 (4.8.9325.0), X86 LegacyJIT


```
| Method                            | Mean         | Error       | StdDev      | Speed        |          |
|---------------------------------- |-------------:|------------:|------------:|-------------:|----------|
| RichTextBox_FullSet               | 6,932.056 ms | 131.6848 ms | 140.9013 ms |   20.95 MB/s | 1x       |
| RichTextBox_NoImageSet            | 2,885.139 ms |  57.0121 ms |  81.7651 ms |    1.23 MB/s | 1x       |
| ReasonableRTF_FullSet             |    45.670 ms |   0.1937 ms |   0.1812 ms | 3179.59 MB/s | 152x     |
| ReasonableRTF_NoImageSet          |     9.912 ms |   0.0412 ms |   0.0386 ms |  357.51 MB/s | 291x     |
| ReasonableRTF_FullSet_Streamed    |    50.416 ms |   0.1050 ms |   0.0982 ms | 2880.27 MB/s | 138x     |
| ReasonableRTF_NoImageSet_Streamed |     9.968 ms |   0.0300 ms |   0.0266 ms |  355.50 MB/s | 289x     |

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
