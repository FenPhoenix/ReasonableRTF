# ReasonableRTF: Parsing gigabytes (sometimes) of RTF per second

So you're using C# and you want to convert some RTF to text. The solution is easy: You reach for the WinForms RichTextBox. Load your RTF in, access the Text property, and presto, it's all there. Mostly. Except smiley faces become the letter J. And sometimes non-ASCII text becomes gibberish even though old versions used to display it fine. And it's really, really slow. Also it [leaks native memory](https://github.com/FenPhoenix/ReasonableRTF/blob/a8077dc484e8568a4aec5115320dc7c0babeae4f/ReasonableRTF_TestApp/Data/RTF_Test_Set_Full/TDP20AC_An_Enigmatic_Treasure___TDP20AC_An_Enigmatic_Treasure_With_A_Recondite_Discovery.rtf).

You try the WPF version. Wait, did that one file take _twenty-five seconds_ to load just because it had a 240x180 image in it?!

Forget it! You need something better. You need...

<p align="center"><img src="https://github.com/FenPhoenix/AngelLoader/blob/master/docs/images/reasonable_rtf/perf_bar_charts3.png" /></p>

... the converter that's consistently over a hundred times faster than RichTextBox. 1.48 megs a second? That's unreasonable. 214 megs a second is slightly less unreasonable! That's like step 2½ out of 8 in *[Context is Everything](https://vimeo.com/644068002)*!

## Features

- Wingdings 1, 2 and 3, Webdings, Symbol, and Zapf Dingbats all converted to equivalent Unicode characters.  
- Non-ASCII text correctly converted where RichTextBox can't.  
- Got huge files with tons of images? No problem. We blaze past image data so fast it may as well not exist.  

## Benchmarks

```

BenchmarkDotNet v0.13.12, Windows 10 (10.0.19045.3448/22H2/2022Update)
AMD Ryzen 9 3950X, 1 CPU, 32 logical and 16 physical cores
.NET SDK 8.0.204
  [Host]     : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.4 (8.0.424.16909), X64 RyuJIT AVX2


```
| Method                   | Mean        | Error    | StdDev   | Performance |
|------------------------- |------------:|---------:|---------:|-------------|
| RichTextBox_FullSet      | 5,556.20 ms | 8.693 ms | 8.132 ms | 1x          |
| ReasonableRTF_FullSet    |    42.31 ms | 0.188 ms | 0.176 ms | 131x        |
| RichTextBox_NoImageSet   | 2,389.84 ms | 3.342 ms | 3.126 ms | 1x          |
| ReasonableRTF_NoImageSet |    16.52 ms | 0.023 ms | 0.019 ms | 145x        |

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
