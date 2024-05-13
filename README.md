# ReasonableRTF

I'm gonna lay it right out on the table. This ain't the simdjson of RTF to plaintext converters. But it is over a hundred times faster than a RichTextBox, and that's got to count for something.

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
