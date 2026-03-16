# ReasonableRTF: Parsing gigabytes (sometimes) of RTF per second

So you're using C# and you want to convert some RTF to text. The solution is easy: You reach for the WinForms RichTextBox. Load your RTF in, access the Text property, and presto, it's all there. Mostly. Except smiley faces become the letter J. And sometimes non-ASCII text becomes gibberish even though old versions used to display it fine. And it's really, really slow. Also it [leaks native memory](https://github.com/FenPhoenix/ReasonableRTF/blob/a8077dc484e8568a4aec5115320dc7c0babeae4f/ReasonableRTF_TestApp/Data/RTF_Test_Set_Full/TDP20AC_An_Enigmatic_Treasure___TDP20AC_An_Enigmatic_Treasure_With_A_Recondite_Discovery.rtf).

You try the WPF version. Wait, did that one file take _twenty-five seconds_ to load just because it had a 240x180 image in it?!

Forget it! You need something better. You need...

<p align="center"><img src="https://github.com/FenPhoenix/ReasonableRTF/blob/main/docs/2026_perf_bar_charts.png" /></p>

... the converter that's consistently over a hundred times faster than RichTextBox. 1.48 megs a second? That's unreasonable. 214 megs a second is slightly less unreasonable! That's like step 2½ out of 8 in *[Context is Everything](https://vimeo.com/644068002)*!

## Features

- Wingdings 1, 2 and 3, Webdings, Symbol, and Zapf Dingbats all converted to equivalent Unicode characters.  
- Non-ASCII text correctly converted where RichTextBox can't.  
- Got huge files with tons of images? No problem. We blaze past image data so fast it may as well not exist.  

## Benchmarks

```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8037/25H2/2025Update/HudsonValley2)
AMD Ryzen 5 5600 3.50GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.200
  [Host]     : .NET 10.0.4 (10.0.4, 10.0.426.12010), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.4 (10.0.4, 10.0.426.12010), X64 RyuJIT x86-64-v3


```
| Method                   | Mean         | Error     | StdDev    | Performance |
|------------------------- |-------------:|----------:|----------:|-------------|
| RichTextBox_FullSet      | 3,307.722 ms | 7.3919 ms | 6.5527 ms | 1x          |
| ReasonableRTF_FullSet    |    30.666 ms | 0.0438 ms | 0.0410 ms | 108x        |
| RichTextBox_NoImageSet   | 1,422.991 ms | 4.8078 ms | 4.4972 ms | 1x          |
| ReasonableRTF_NoImageSet |     8.376 ms | 0.0197 ms | 0.0184 ms | 170x        |

## Supported

- All basic plain text, hex-encoded chars, Unicode-encoded chars
- Symbol fonts (the abovementioned ones) converted to Unicode equivalents
- Characters specified as "SYMBOL" field instructions
- Undocumented use of the \langN keyword to [specify character encoding](https://therealfenphoenix.wordpress.com/2024/01/05/rtf-character-encoding-who-needs-a-spec-anyway/) - old versions of RichTextBox used to support this

## Partially supported

- Tables: Cells and rows have spaces between them, but not much functionality beyond that.
- Lists: Numbers and bullets show up (that's better than RichTextBox most of the time), but indentation usually doesn't.

## Not currently supported

- Footnotes
- "HYPERLINK" field instruction value
- Math objects
