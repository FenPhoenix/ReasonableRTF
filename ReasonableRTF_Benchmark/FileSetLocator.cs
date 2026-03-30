using System.Runtime.CompilerServices;

namespace ReasonableRTF_Benchmark;

internal static class FileSetLocator
{
    private const string RtfFullSetDir = "RTF_Test_Set_Full";
    private const string RtfSmallSetDir = "RTF_Test_Set_Small";

    private const string DataSetLocation = @"ReasonableRTF_TestApp\Data";

    public static string GetFileSet(FileSetType type)
    {
        string currentPath = GetCurrentPath();
        string directory = type switch
        {
            FileSetType.Full => RtfFullSetDir,
            FileSetType.Small => RtfSmallSetDir,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };

        return Path.GetFullPath(Path.Combine(currentPath, "..", "..", DataSetLocation, directory));
    }

    private static string GetCurrentPath([CallerFilePath] string callerFilePath = "")
    {
        return callerFilePath;
    }
}

public enum FileSetType
{
    Full,
    Small
}