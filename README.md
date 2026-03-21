# ReasonableRTF - Fast RTF to Plain Text Converter 🚀

A lightweight and performant C# library designed for rapidly converting **Rich Text Format (RTF)** files into **plain text**.

- - -

## Features

- Fast: Over 100x faster than RichTextBox.
- Accurate: All characters converted correctly — even Wingdings.
- Small: Less than 50k and with no dependencies.
- Fully cross-platform.
- [We even support this obscure nonsense](https://therealfenphoenix.wordpress.com/2024/01/05/rtf-character-encoding-who-needs-a-spec-anyway/).

- - -

## Installation

Install the library via NuGet:

```
dotnet add package ReasonableRTF.Standard
```

- - -

## Quick Start

Converting an RTF file (as a byte array) to plain text is straightforward. The `Convert` method returns an **`RtfResult`** object, providing the converted text and comprehensive error information, if any.

### Basic Conversion

Here is how to convert an RTF byte array to a plain text string:

```cs
using ReasonableRTF;
using ReasonableRTF.Models;
using System.IO;

// ...

// 1. Initialize the converter
RtfToTextConverter converter = new RtfToTextConverter();

// 2. Load the RTF data (e.g., from a file) and convert
RtfResult result = converter.Convert(File.ReadAllBytes(Context.Path));

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

RtfResult result = converter.Convert(File.ReadAllBytes(Context.Path), options);
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

```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8037/25H2/2025Update/HudsonValley2)
AMD Ryzen 5 5600 3.50GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.201
  [Host]     : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.5 (10.0.5, 10.0.526.15411), X64 RyuJIT x86-64-v3


```
| Method                            | Mean         | Error     | StdDev    | Performance  | Performance Multiple |
|---------------------------------- |-------------:|----------:|----------:|-------------:|----------------------|
| RichTextBox_FullSet               | 3,316.729 ms | 8.9031 ms | 8.3280 ms |   43.78 MB/s | 1x                   |
| RichTextBox_NoImageSet            | 1,421.105 ms | 2.5863 ms | 2.4192 ms |    2.49 MB/s | 1x                   |
| ReasonableRTF_FullSet             |    30.940 ms | 0.0422 ms | 0.0395 ms | 4693.33 MB/s | 107x                 |
| ReasonableRTF_NoImageSet          |     8.691 ms | 0.0130 ms | 0.0122 ms |  407.74 MB/s | 164x                 |
| ReasonableRTF_FullSet_Streamed    |    33.950 ms | 0.5634 ms | 0.4994 ms | 4277.22 MB/s | 98x                  |
| ReasonableRTF_NoImageSet_Streamed |     9.127 ms | 0.0799 ms | 0.0708 ms |  388.26 MB/s | 156x                 |


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
