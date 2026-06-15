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
| ReasonableRTF_FullSet             |    15.152 ms | 0.0231 ms | 0.0216 ms | 9583.67 MB/s | 220x     |
| ReasonableRTF_NoImageSet          |     3.436 ms | 0.0046 ms | 0.0043 ms | 1031.33 MB/s | 417x     |
| ReasonableRTF_FullSet_Streamed    |    16.880 ms | 0.0345 ms | 0.0323 ms | 8602.59 MB/s | 197x     |
| ReasonableRTF_NoImageSet_Streamed |     3.508 ms | 0.0069 ms | 0.0061 ms | 1010.17 MB/s | 408x     |

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
| ReasonableRTF_FullSet             |    19.146 ms | 0.0281 ms | 0.0249 ms | 7584.44 MB/s | 145x     |
| ReasonableRTF_NoImageSet          |     4.707 ms | 0.0069 ms | 0.0061 ms |  752.85 MB/s | 211x     |
| ReasonableRTF_FullSet_Streamed    |    21.323 ms | 0.0370 ms | 0.0289 ms | 6810.10 MB/s | 130x     |
| ReasonableRTF_NoImageSet_Streamed |     4.777 ms | 0.0109 ms | 0.0097 ms |  741.82 MB/s | 208x     |

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
| ReasonableRTF_FullSet             |    41.606 ms |   0.0493 ms |   0.0437 ms | 3490.16 MB/s | 167x     |
| ReasonableRTF_NoImageSet          |     8.244 ms |   0.0170 ms |   0.0159 ms |  429.85 MB/s | 350x     |
| ReasonableRTF_FullSet_Streamed    |    45.674 ms |   0.1325 ms |   0.1174 ms | 3179.31 MB/s | 152x     |
| ReasonableRTF_NoImageSet_Streamed |     8.083 ms |   0.0217 ms |   0.0193 ms |  438.41 MB/s | 357x     |

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
