using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace ReasonableRTF_TestApp;

internal static class FileSetLocator
{
    internal enum SourceSet
    {
        Full,
        Small,
        ValidityTest,
        WorkingNewSet,
    }

    internal enum TargetBenchmark
    {
        NET_64,
        NET48_64,
        NET48_32,
    }

    internal const string RtfFullSetDir = "RTF_Test_Set_Full";
    internal const string RtfSmallSetDir = "RTF_Test_Set_Small";

    internal const string RftValidityTestDir = "Validity_Test_Files";
    internal const string WorkingNewSetDir = "WorkingNewSet";

    internal const string OutputCustomDir = "Output_Custom";
    internal const string OutputRichTextBoxDir = "Output_RichTextBox";

    internal const string RtfValidityTestOutputCustomDir = "Output_Validity_Test_Custom";
    internal const string RtfValidityTestOutputRichTextBoxDir = "Output_Validity_Test_RichTextBox";

    internal const string OutputWorkingNewSetCustomDir = "Output_WorkingNewSet_Custom";
    internal const string OutputWorkingNewSetRichTextBoxDir = "Output_WorkingNewSet_RichTextBox";

    internal const string DeflateStreamTest_Full_FileName = "DeflateStreamTest_Full.zip";
    internal const string DeflateStreamTest_Small_FileName = "DeflateStreamTest_Small.zip";
    internal const string DeflateStreamTest_Validity_Test_Files_FileName = "DeflateStreamTest_Validity_Test_Files.zip";
    internal const string DeflateStreamTest_WorkingNewSet_FileName = "DeflateStreamTest_WorkingNewSet.zip";

    internal const string BenchmarkOutputFileName = "ReasonableRTF_Benchmark.Test-report-github.md";

    internal static long GetDirectorySize(SourceSet sourceSet)
    {
        long totalSize = 0;
        DirectoryInfo di = new(GetFileSet(sourceSet));
        foreach (FileInfo fi in di.EnumerateFiles())
        {
            totalSize += fi.Length;
        }
        return totalSize;
    }

    public static string GetFileSet(SourceSet sourceSet)
    {
        string directory = sourceSet switch
        {
            SourceSet.Full => RtfFullSetDir,
            SourceSet.Small => RtfSmallSetDir,
            SourceSet.WorkingNewSet => WorkingNewSetDir,
            SourceSet.ValidityTest => RftValidityTestDir,
            _ => throw new ArgumentOutOfRangeException(nameof(sourceSet), sourceSet, null),
        };

        return Path.Combine(DataDir, directory);
    }

    internal static string DataDir
    {
        get
        {
            if (field == null)
            {
                string currentPath = GetCurrentPath();
                string path;
                while (true)
                {
                    path = Path.GetDirectoryName(currentPath)!;
                    if (!path.Contains(Path.DirectorySeparatorChar) && !path.Contains(Path.AltDirectorySeparatorChar))
                    {
                        throw new Exception("Couldn't find project path for ReasonableRTF_TestApp.");
                    }
                    if (new DirectoryInfo(path).Name.Equals("ReasonableRTF_TestApp", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                }

                field = Path.Combine(path, "Data");
            }

            return field;
        }
    }

    private sealed class DotNetModernDirName
    {
        internal readonly DirectoryInfo Info;
        internal readonly Version Version;

        public DotNetModernDirName(DirectoryInfo info, Version version)
        {
            Info = info;
            Version = version;
        }
    }

    // Just do some stupid heuristics so we're always using the latest .NET version output directory, and don't
    // have to manually change a constant when we upgrade.
    private static string GetLatestDotNetModernBenchmarkOutputFile(string baseBenchmarkPath)
    {
        string path = Path.Combine(baseBenchmarkPath, "Release");
        var infos = new DirectoryInfo(path).GetDirectories("*", SearchOption.TopDirectoryOnly).Where(static x =>
                x.Name.Contains("windows", StringComparison.OrdinalIgnoreCase)).ToArray();

        List<DotNetModernDirName> sortedInfos = new();
        foreach (var item in infos)
        {
            Match match = Regex.Match(item.Name, @"net(?<Version>(\d|.)+)-windows");
            if (match.Success)
            {
                sortedInfos.Add(new DotNetModernDirName(item, Version.Parse(match.Groups["Version"].Value)));
            }
        }

        sortedInfos = sortedInfos.OrderByDescending(static x => x.Version).ToList();

        if (sortedInfos.Count > 0)
        {
            foreach (DotNetModernDirName info in sortedInfos)
            {
                string benchmarkFile = Path.Combine(info.Info.FullName, "BenchmarkDotNet.Artifacts", "results", BenchmarkOutputFileName);
                if (File.Exists(benchmarkFile))
                {
                    return benchmarkFile;
                }
            }
        }

        throw new Exception("Couldn't find any benchmark directories for any modern .NET versions.");
    }

    internal static string GetBenchmarkArtifactsDir(TargetBenchmark targetBenchmark)
    {
        string dataDir = DataDir;
        string baseBenchmarkPath = Path.GetFullPath(Path.Combine(dataDir, "..", "..", "ReasonableRTF_Benchmark", "bin"));

        switch (targetBenchmark)
        {
            case TargetBenchmark.NET_64:
                return GetLatestDotNetModernBenchmarkOutputFile(baseBenchmarkPath);
            case TargetBenchmark.NET48_64:
                return Path.Combine(
                    baseBenchmarkPath,
                    "Release_Framework",
                    "net48",
                    "BenchmarkDotNet.Artifacts",
                    "results",
                    BenchmarkOutputFileName
                );
            case TargetBenchmark.NET48_32:
            default:
                return Path.Combine(
                    baseBenchmarkPath,
                    "x86",
                    "Release_Framework",
                    "net48",
                    "BenchmarkDotNet.Artifacts",
                    "results",
                    BenchmarkOutputFileName
                );
        }
    }

    internal static string GetCurrentPath([CallerFilePath] string callerFilePath = "")
    {
        return callerFilePath;
    }
}
