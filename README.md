# ReasonableRTF - Fast RTF to Plain Text Converter 🚀

A lightweight and performant C# library designed for rapidly converting **Rich Text Format (RTF)** files into **plain text**.

- - -

## Features

- Fast: Up to 400x faster than RichTextBox — and over 100x faster at minimum
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

### Converting An RTF File

Use the static `ConvertRtf.ToText()` method to do a one-off conversion:

```cs
using ReasonableRTF;
using ReasonableRTF.Enums;
using ReasonableRTF.Models;

// ...

RtfResult result = ConvertRtf.ToText(@"C:\rtf_files\some_file.rtf");

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

### Converting Many RTF Files Efficiently

You can create and reuse an `RtfToTextConverter` instance to minimize overhead when converting many files:

```cs
using ReasonableRTF;
using ReasonableRTF.Enums;
using ReasonableRTF.Models;

// ...

RtfToTextConverter converter = new RtfToTextConverter();
foreach (string fileName in Directory.EnumerateFiles(@"C:\rtf_files\", "*.rtf"))
{
    RtfResult result = converter.Convert(fileName);
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
}
```

For efficiency, an `RtfToTextConverter` will not trim its internal growable buffers between conversions. You can use the `ResetMemory()` method to reset these buffers back to their default sizes. This way, you can keep one `RtfToTextConverter` instance around indefinitely without worrying about memory use.

- - -

## The RtfResult Struct

The `RtfResult` struct provides full details about the conversion process, ensuring you can robustly handle success and failure cases.

| Property | Type | Description |
| --- | --- | --- |
| `Text` | `string` | The converted plain text. |
| `Error` | `RtfError` | The error code. This will be `RtfError.OK` upon successful conversion. |
| `BytePositionOfError` | `int` | The approximate position in the data stream where the error occurred, or `-1` if no error. |
| `Exception` | `Exception?` | The caught exception, or `null` if no exception occurred during conversion. |

- - -

## Conversion Options

For more control over the output, you can provide an instance of the **`RtfToTextConverterOptions`** class.

```cs
RtfToTextConverterOptions options = new RtfToTextConverterOptions
{
    ConvertHiddenText = true,          // Include text marked as hidden
    LineBreakStyle = LineBreakStyle.LF // Use Unix-style line breaks
};

RtfResult result = ConvertRtf.ToText(@"C:\rtf_files\some_file.rtf", options);
// ... check result
```

The available options are documented in [RtfToTextConverterOptions.cs](https://github.com/FenPhoenix/ReasonableRTF/blob/main/ReasonableRTF/Models/RtfToTextConverterOptions.cs).

- - -

## Benchmarks

### .NET 10 64-bit

```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8655/25H2/2025Update/HudsonValley2)
AMD Ryzen 5 5600 3.50GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v3


```
| Method                            | Mean         | Error     | StdDev    | Speed        |          |
|---------------------------------- |-------------:|----------:|----------:|-------------:|----------|
| RichTextBox_FullSet               | 3,331.340 ms | 6.2250 ms | 5.5183 ms |   43.59 MB/s | 1x       |
| RichTextBox_NoImageSet            | 1,432.217 ms | 3.7089 ms | 3.4693 ms |    2.47 MB/s | 1x       |
| ReasonableRTF_FullSet             |    15.051 ms | 0.0343 ms | 0.0320 ms | 9647.98 MB/s | 221x     |
| ReasonableRTF_NoImageSet          |     3.372 ms | 0.0055 ms | 0.0046 ms | 1050.91 MB/s | 425x     |
| ReasonableRTF_FullSet_Streamed    |    16.851 ms | 0.0570 ms | 0.0533 ms | 8617.40 MB/s | 198x     |
| ReasonableRTF_NoImageSet_Streamed |     3.465 ms | 0.0080 ms | 0.0075 ms | 1022.70 MB/s | 413x     |

### .NET Framework 4.8 64-bit

```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8655/25H2/2025Update/HudsonValley2)
AMD Ryzen 5 5600 3.50GHz, 1 CPU, 12 logical and 6 physical cores
  [Host]     : .NET Framework 4.8.1 (4.8.9337.0), X64 RyuJIT VectorSize=256
  DefaultJob : .NET Framework 4.8.1 (4.8.9337.0), X64 RyuJIT VectorSize=256


```
| Method                            | Mean         | Error     | StdDev    | Speed        |          |
|---------------------------------- |-------------:|----------:|----------:|-------------:|----------|
| RichTextBox_FullSet               | 2,779.775 ms | 3.9318 ms | 3.2833 ms |   52.24 MB/s | 1x       |
| RichTextBox_NoImageSet            |   992.237 ms | 2.5478 ms | 2.2585 ms |    3.57 MB/s | 1x       |
| ReasonableRTF_FullSet             |    18.969 ms | 0.0621 ms | 0.0581 ms | 7655.21 MB/s | 147x     |
| ReasonableRTF_NoImageSet          |     4.686 ms | 0.0122 ms | 0.0108 ms |  756.22 MB/s | 212x     |
| ReasonableRTF_FullSet_Streamed    |    21.456 ms | 0.0797 ms | 0.0746 ms | 6767.89 MB/s | 130x     |
| ReasonableRTF_NoImageSet_Streamed |     4.766 ms | 0.0116 ms | 0.0108 ms |  743.53 MB/s | 208x     |

### .NET Framework 4.8 32-bit

```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8655/25H2/2025Update/HudsonValley2)
AMD Ryzen 5 5600 3.50GHz, 1 CPU, 12 logical and 6 physical cores
  [Host]     : .NET Framework 4.8.1 (4.8.9337.0), X86 LegacyJIT
  DefaultJob : .NET Framework 4.8.1 (4.8.9337.0), X86 LegacyJIT


```
| Method                            | Mean         | Error       | StdDev      | Speed        |          |
|---------------------------------- |-------------:|------------:|------------:|-------------:|----------|
| RichTextBox_FullSet               | 6,932.056 ms | 131.6848 ms | 140.9013 ms |   20.95 MB/s | 1x       |
| RichTextBox_NoImageSet            | 2,885.139 ms |  57.0121 ms |  81.7651 ms |    1.23 MB/s | 1x       |
| ReasonableRTF_FullSet             |    41.745 ms |   0.1710 ms |   0.1599 ms | 3478.54 MB/s | 166x     |
| ReasonableRTF_NoImageSet          |     8.240 ms |   0.0178 ms |   0.0158 ms |  430.06 MB/s | 350x     |
| ReasonableRTF_FullSet_Streamed    |    45.633 ms |   0.0670 ms |   0.0594 ms | 3182.17 MB/s | 152x     |
| ReasonableRTF_NoImageSet_Streamed |     8.057 ms |   0.0220 ms |   0.0205 ms |  439.82 MB/s | 358x     |

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

[Flamifly](https://github.com/Flamifly) contributed to the readme (installation, quick start, documentation, etc), and ported the original .NET-only version to .NET Standard.
