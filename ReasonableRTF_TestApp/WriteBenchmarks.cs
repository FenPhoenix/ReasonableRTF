using System.Globalization;
using static ReasonableRTF_TestApp.FileSetLocator;

namespace ReasonableRTF_TestApp;

internal static class WriteBenchmarks
{
    private const string _topLineExtension1 = " Speed        |          |";
    private const string _topLineExtension2 = "-------------:|----------|";

    private const int _speedSectionWidth = 14;
    private const int _multipleSectionWidth = 10;

    private const int _meanIndexAfter = 6;
    private const int _errorIndexAfter = 7;
    private const int _stdDevIndexAfter = 8;

    private sealed class ExtraCharsOnColumn
    {
        internal readonly int MeanColumn;
        internal readonly int ErrorColumn;
        internal readonly int StdDevColumn;

        public ExtraCharsOnColumn(int meanColumn, int errorColumn, int stdDevColumn)
        {
            MeanColumn = meanColumn;
            ErrorColumn = errorColumn;
            StdDevColumn = stdDevColumn;
        }
    }

    private sealed class TargetBenchmarkSet
    {
        internal readonly string FileName;
        internal readonly string RTB_BenchmarkLine_Full;
        internal readonly string RTB_BenchmarkLine_Small;
        internal readonly decimal RTB_Time_Full;
        internal readonly decimal RTB_Time_Small;
        internal readonly ExtraCharsOnColumn ExtraCharsOnColumn;

        public TargetBenchmarkSet(
            string fileName,
            string rtbBenchmarkLineFull,
            string rtbBenchmarkLineSmall,
            decimal rtbTimeFull,
            decimal rtbTimeSmall,
            ExtraCharsOnColumn extraCharsOnColumn)
        {
            FileName = fileName;
            RTB_BenchmarkLine_Full = rtbBenchmarkLineFull;
            RTB_BenchmarkLine_Small = rtbBenchmarkLineSmall;
            RTB_Time_Full = rtbTimeFull;
            RTB_Time_Small = rtbTimeSmall;
            ExtraCharsOnColumn = extraCharsOnColumn;
        }
    }

    private static readonly TargetBenchmarkSet[] _targetBenchmarkSets;

    static WriteBenchmarks()
    {
        _targetBenchmarkSets = new TargetBenchmarkSet[Enum.GetValues(typeof(TargetBenchmark)).Length];

        _targetBenchmarkSets[(int)TargetBenchmark.NET_64] = new TargetBenchmarkSet("net modern 64.md",
            "| RichTextBox_FullSet               | 3,331.340 ms | 6.2250 ms | 5.5183 ms |   43.59 MB/s | 1x       |",
            "| RichTextBox_NoImageSet            | 1,432.217 ms | 3.7089 ms | 3.4693 ms |    2.47 MB/s | 1x       |",
            (decimal)3_331.340,
            (decimal)1_432.217,
            new ExtraCharsOnColumn(3, 0, 0)
        );

        _targetBenchmarkSets[(int)TargetBenchmark.NET48_64] = new TargetBenchmarkSet("net48 64.md",
            "| RichTextBox_FullSet               | 2,779.775 ms | 3.9318 ms | 3.2833 ms |   52.24 MB/s | 1x       |",
            "| RichTextBox_NoImageSet            |   992.237 ms | 2.5478 ms | 2.2585 ms |    3.57 MB/s | 1x       |",
            (decimal)2_779.775,
            (decimal)992.237,
            new ExtraCharsOnColumn(3, 0, 0)
        );

        _targetBenchmarkSets[(int)TargetBenchmark.NET48_32] = new TargetBenchmarkSet("net48 32.md",
            "| RichTextBox_FullSet               | 6,932.056 ms | 131.6848 ms | 140.9013 ms |   20.95 MB/s | 1x       |",
            "| RichTextBox_NoImageSet            | 2,885.139 ms |  57.0121 ms |  81.7651 ms |    1.23 MB/s | 1x       |",
            (decimal)6_932.056,
            (decimal)2_885.139,
            new ExtraCharsOnColumn(3, 2, 2)
        );
    }

    internal static void WriteAll()
    {
        long fullBytes = GetDirectorySize(SourceSet.Full);
        long smallBytes = GetDirectorySize(SourceSet.Small);

        WriteBenchmarkSet(TargetBenchmark.NET_64, fullBytes, smallBytes);
        WriteBenchmarkSet(TargetBenchmark.NET48_64, fullBytes, smallBytes);
        WriteBenchmarkSet(TargetBenchmark.NET48_32, fullBytes, smallBytes);
    }

    internal static void Write(TargetBenchmark target)
    {
        long fullBytes = GetDirectorySize(SourceSet.Full);
        long smallBytes = GetDirectorySize(SourceSet.Small);

        WriteBenchmarkSet(target, fullBytes, smallBytes);
    }

    private static void WriteBenchmarkSet(TargetBenchmark target, long fullBytes, long smallBytes)
    {
        TargetBenchmarkSet tbs = _targetBenchmarkSets[(int)target];

        string sourceFileName = GetBenchmarkArtifactsDir(target);

        List<string> lines = File.ReadAllLines(sourceFileName).ToList();
        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            if (line.StartsWithO("| Method"))
            {
                lines[i] += _topLineExtension1;
                WriteExtraCharsSet(lines, i, ' ', tbs.ExtraCharsOnColumn, true);
            }
            else if (line.StartsWithO("|"))
            {
                if (line.StartsWithO("|-"))
                {
                    lines[i] += _topLineExtension2;
                    WriteExtraCharsSet(lines, i, '-', tbs.ExtraCharsOnColumn, false);
                }
                else
                {
                    WriteExtraCharsSet(lines, i, ' ', tbs.ExtraCharsOnColumn, false);
                    decimal time = GetBenchmarkTime(line);

                    bool isFull = line.Contains("FullSet", StringComparison.Ordinal);

                    decimal comparisonTime = isFull
                        ? tbs.RTB_Time_Full
                        : tbs.RTB_Time_Small;

                    decimal multiple = comparisonTime / time;
                    int intMultiple = (int)Math.Round(multiple, 0, MidpointRounding.AwayFromZero);

                    long setBytes = isFull ? fullBytes : smallBytes;

                    string mbsString = GetMBsString(setBytes, time);

                    mbsString += " ";
                    mbsString = new string(' ', _speedSectionWidth - mbsString.Length) + mbsString;

                    lines[i] += mbsString + "|";

                    lines[i] +=
                        " " +
                        intMultiple +
                        "x" +
                        new string(' ', _multipleSectionWidth - (intMultiple.ToString().Length + 2)) +
                        "|";
                }
            }
        }

        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            if (line.StartsWithO("|-"))
            {
                lines.Insert(i + 1, tbs.RTB_BenchmarkLine_Small);
                lines.Insert(i + 1, tbs.RTB_BenchmarkLine_Full);
                break;
            }
        }

        string outputFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Path.GetFileNameWithoutExtension(tbs.FileName) + "_final.md");

        using var sw = new StreamWriter(outputFile);
        for (int i = 0; i < lines.Count; i++)
        {
            // No empty last line, to make it easier to copy-paste without grabbing the empty last line by accident
            if (i == lines.Count - 1)
            {
                sw.Write(lines[i]);
            }
            else
            {
                sw.WriteLine(lines[i]);
            }
        }
    }

    private static void WriteExtraCharsSet(List<string> lines, int i, char c, ExtraCharsOnColumn extra, bool relativeIndex)
    {
        if (relativeIndex)
        {
            WriteExtraChars(lines, i, c, extra.MeanColumn, _meanIndexAfter, 2);
            WriteExtraChars(lines, i, c, extra.ErrorColumn, _errorIndexAfter, 3);
            WriteExtraChars(lines, i, c, extra.StdDevColumn, _stdDevIndexAfter, 4);
        }
        else
        {
            WriteExtraChars(lines, i, c, extra.MeanColumn, 1, 2);
            WriteExtraChars(lines, i, c, extra.ErrorColumn, 1, 3);
            WriteExtraChars(lines, i, c, extra.StdDevColumn, 1, 4);
        }

        return;

        static void WriteExtraChars(List<string> lines, int i, char c, int numberOfChars, int indexAfterBarToInsertChars, int barIndex)
        {
            int secondBarIndex = GetBarIndex(lines[i], barIndex);
            lines[i] = lines[i].Substring(0, secondBarIndex + indexAfterBarToInsertChars) + new string(c, numberOfChars) +
                       lines[i].Substring(secondBarIndex + indexAfterBarToInsertChars);
        }
    }

    private static decimal GetBenchmarkTime(string line)
    {
        int secondBarIndex = GetBarIndex(line, 2);
        int thirdBarIndex = GetBarIndex(line, 3);

        string timeStr = line.Substring(secondBarIndex + 1, thirdBarIndex - (secondBarIndex + 1));
        timeStr = timeStr.Trim().TrimEnd('m', 's').Trim();

        decimal.TryParse(timeStr, out decimal result);

        return result;
    }

    private static string GetMBsString(long totalSize, decimal elapsedMilliseconds)
    {
        decimal megs = (decimal)totalSize / 1024 / 1024;
        decimal intermediate = megs / elapsedMilliseconds;
        decimal finalMBs = Math.Round(intermediate * 1000, 2, MidpointRounding.AwayFromZero);
        return finalMBs.ToString("F2", CultureInfo.CurrentCulture) + " MB/s";
    }

    private static int GetBarIndex(string line, int barNumberToGet)
    {
        int barIndex = -1;

        for (int i = 0; i < barNumberToGet; i++)
        {
            barIndex = line.IndexOf('|', barIndex + 1);
        }

        return barIndex;
    }
}
