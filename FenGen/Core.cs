using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static FenGen.Misc;

namespace FenGen;

file static class Cache
{
    private static List<string>? _csFiles;
    internal static List<string> CSFiles => _csFiles ??= Directory.GetFiles(Core.SolutionPath, "*.cs", SearchOption.AllDirectories).ToList();
}

internal static class GenAttributes
{
    internal const string FenGen_ParseKeywordAttribute = nameof(FenGen_ParseKeywordAttribute);
}

internal static class Core
{
    private static class DefineHeaders
    {
        internal const string FenGen_ParseKeywordDuplicateSource = nameof(FenGen_ParseKeywordDuplicateSource);
        internal const string FenGen_ParseKeywordDuplicateDest = nameof(FenGen_ParseKeywordDuplicateDest);
    }

#if DEBUG
    private static Forms.MainForm View = null!;
#endif

    internal static readonly string SolutionPath = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\..\"));
    internal static readonly string MainProjectPath = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\..\ReasonableRTF"));
    internal static readonly string FenGenProjectPath = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\..\FenGen"));
    internal static readonly string MainProjectFile = Path.Combine(MainProjectPath, "ReasonableRTF.csproj");

    internal static void Init()
    {
#if DEBUG
        View = new Forms.MainForm();
        View.Show();
#else
        DoTasks();
        Environment.Exit(0);
#endif
    }

    internal static void DoTasks()
    {
        try
        {
            DoTasksInternal();
        }
        catch (Exception ex)
        {
            ThrowErrorAndTerminate(ex);
        }
    }

    private static void DoTasksInternal()
    {
        (string parseKeywordSourceFile, List<string> parseKeywordDestFiles) = GetParseKeywordSourceAndDestFiles();
        ParseKeywordGen.Generate(parseKeywordSourceFile, parseKeywordDestFiles);
    }

    private static (string SourceFile, List<string> DestFile)
    GetParseKeywordSourceAndDestFiles()
    {
        List<string> destFiles = new();

        string sourceFile = "";

        foreach (string f in Cache.CSFiles)
        {
            using StreamReader sr = new(f);
            while (sr.ReadLine() is { } line)
            {
                string lts = line.TrimStart();
                if (lts.IsWhiteSpace() || lts.StartsWithO("//")) continue;

                if (lts[0] != '#') break;

                if (lts.StartsWithOPlusWhiteSpace("#define"))
                {
                    string tag = lts.Substring(7).Trim();
                    if (tag == DefineHeaders.FenGen_ParseKeywordDuplicateSource)
                    {
                        sourceFile = f;
                        break;
                    }
                    else if (tag == DefineHeaders.FenGen_ParseKeywordDuplicateDest)
                    {
                        destFiles.Add(f);
                        break;
                    }
                }
            }
        }

        string error = "";

        if (sourceFile.IsEmpty())
        {
            error = AddError(
                error,
                "-No file found with #define " + DefineHeaders.FenGen_ParseKeywordDuplicateSource + " at top"
            );
        }
        if (destFiles.Count == 0)
        {
            error = AddError(
                error,
                "-No file found with #define " + DefineHeaders.FenGen_ParseKeywordDuplicateDest + " at top"
            );
        }
        if (!error.IsEmpty()) ThrowErrorAndTerminate(error);

        return (sourceFile, destFiles);

        static string AddError(string msg, string add) => msg + (msg.IsEmpty() ? "" : "\r\n") + add;
    }
}
