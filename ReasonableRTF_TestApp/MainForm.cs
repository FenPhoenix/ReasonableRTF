using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using ReasonableRTF;
using ReasonableRTF.Enums;
using ReasonableRTF.Models;

namespace ReasonableRTF_TestApp;

public sealed partial class MainForm : Form
{
    private const string _configFile = "Config.ini";

    private const string _rtfFullSetDir = "RTF_Test_Set_Full";
    private const string _rtfSmallSetDir = "RTF_Test_Set_Small";
    private const string _rftValidityTestDir = "Validity_Test_Files";
    private const string _workingNewSetDir = "WorkingNewSet";

    private const string _outputCustomDir = "Output_Custom";
    private const string _outputRichTextBoxDir = "Output_RichTextBox";

    private const string _rtfValidityTestOutputCustomDir = "Output_Validity_Test_Custom";
    private const string _rtfValidityTestOutputRichTextBoxDir = "Output_Validity_Test_RichTextBox";

    private const string _outputWorkingNewSetCustomDir = "Output_WorkingNewSet_Custom";
    private const string _outputWorkingNewSetRichTextBoxDir = "Output_WorkingNewSet_RichTextBox";

    private const string DeflateStreamTest_Full_FileName = "DeflateStreamTest_Full.zip";
    private const string DeflateStreamTest_Small_FileName = "DeflateStreamTest_Small.zip";
    private const string DeflateStreamTest_Validity_Test_Files_FileName = "DeflateStreamTest_Validity_Test_Files.zip";
    private const string DeflateStreamTest_WorkingNewSet_FileName = "DeflateStreamTest_WorkingNewSet.zip";

    private enum SourceSet
    {
        Full,
        Small,
        ValidityTest,
        WorkingNewSet,
    }

    private sealed class TimingScope : IDisposable
    {
        private readonly Stopwatch _stopWatch = new();
        private readonly long TotalSize;

        public TimingScope(long totalSize)
        {
            TotalSize = totalSize;
            _stopWatch.Start();
        }

        public void Dispose()
        {
            _stopWatch.Stop();
            ShowPerfResults(_stopWatch, TotalSize);
        }
    }

    public MainForm()
    {
        InitializeComponent();
        LoadConfig();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        ConvertOnlyWithCustomButton.Focus();
    }

    private void LoadConfig()
    {
        try
        {
            string configFile = Path.Combine(Application.StartupPath, _configFile);
            string[] lines = File.ReadAllLines(configFile);
            for (int i = 0; i < lines.Length; i++)
            {
                string lineT = lines[i].Trim();
                if (lineT.StartsWith("DataDir=", StringComparison.Ordinal))
                {
                    DataDirTextBox.Text = lineT["DataDir=".Length..];
                }
            }
            return;
        }
        catch
        {
            // ignore
        }

        ResetDataDir();
    }

    private void ResetDataDir()
    {
        const string defaultDir = @"..\..\..\Data";

        try
        {
            string finalDir = Path.GetFullPath(defaultDir);

            if (Directory.Exists(finalDir))
            {
                DataDirTextBox.Text = finalDir;
            }
        }
        catch
        {
            DataDirTextBox.Clear();
        }
        finally
        {
            SaveConfig();
        }
    }

    private void SaveConfig()
    {
        try
        {
            string configFile = Path.Combine(Application.StartupPath, _configFile);
            using var sw = new StreamWriter(configFile);
            sw.WriteLine("DataDir=" + DataDirTextBox.Text);
        }
        catch
        {
            MessageBox.Show("Couldn't save config file.");
        }
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        SaveConfig();
    }

    private static string GetMBsString(long totalSize, double elapsedMilliseconds)
    {
        double megs = (double)totalSize / 1024 / 1024;
        double intermediate = megs / elapsedMilliseconds;
        double finalMBs = Math.Round(intermediate * 1000, 2, MidpointRounding.AwayFromZero);
        return finalMBs.ToString(CultureInfo.CurrentCulture) + " MB/s";
    }

    private static void ShowPerfResults(Stopwatch sw, long totalSize)
    {
        MessageBox.Show(
            sw.Elapsed + "\r\n" +
            GetMBsString(totalSize, sw.ElapsedMilliseconds));
    }

    private string GetRtfSetDir(SourceSet sourceSet)
    {
        string dir = sourceSet switch
        {
            SourceSet.Full => _rtfFullSetDir,
            SourceSet.Small => _rtfSmallSetDir,
            SourceSet.WorkingNewSet => _workingNewSetDir,
            _ => _rftValidityTestDir,
        };
        return Path.Combine(DataDirTextBox.Text, dir);
    }

    private (string[] RtfFiles, MemoryStream[] MemStreams, long TotalSize)
    GetStuff_RichTextBox(SourceSet sourceSet)
    {
        string[] rtfFiles = Directory.GetFiles(GetRtfSetDir(sourceSet));
        MemoryStream[] memStreams = new MemoryStream[rtfFiles.Length];

        long totalSize = 0;

        for (int i = 0; i < rtfFiles.Length; i++)
        {
            string f = rtfFiles[i];
            using var fs = File.OpenRead(f);
            byte[] array = new byte[fs.Length];
            fs.ReadExactly(array, 0, (int)fs.Length);
            memStreams[i] = new MemoryStream(array);
            totalSize += fs.Length;
        }

        return (rtfFiles, memStreams, totalSize);
    }

    private (string[] RtfFiles, byte[][] ByteArrays, long TotalSize)
    GetStuff_Custom(SourceSet sourceSet)
    {
        string[] rtfFiles = Directory.GetFiles(GetRtfSetDir(sourceSet));

        byte[][] byteArrays = new byte[rtfFiles.Length][];

        long totalSize = 0;

        for (int i = 0; i < rtfFiles.Length; i++)
        {
            string f = rtfFiles[i];
            using var fs = File.OpenRead(f);
            byte[] array = new byte[fs.Length];
            fs.ReadExactly(array, 0, (int)fs.Length);
            byteArrays[i] = array;
            totalSize += byteArrays[i].Length;
        }

        return (rtfFiles, byteArrays, totalSize);
    }

    private void ConvertWithCustom(SourceSet sourceSet)
    {
        (string[] rtfFiles, byte[][] byteArrays, long totalSize) = GetStuff_Custom(sourceSet);
        RtfToTextConverter rtfConverter = new();

        if (Convert_ByteArrayRadioButton.Checked)
        {
            using (new TimingScope(totalSize))
            {
                for (int i = 0; i < byteArrays.Length; i++)
                {
                    _ = rtfConverter.Convert(byteArrays[i]);
                }
            }
        }
        else if (Convert_MemoryStreamRadioButton.Checked)
        {
            MemoryStream[] memoryStreams = new MemoryStream[byteArrays.Length];
            try
            {
                for (int i = 0; i < byteArrays.Length; i++)
                {
                    memoryStreams[i] = new MemoryStream(byteArrays[i]);
                }

                using (new TimingScope(totalSize))
                {
                    for (int i = 0; i < memoryStreams.Length; i++)
                    {
                        //Trace.WriteLine(rtfFiles[i]);
                        _ = rtfConverter.Convert(memoryStreams[i]);
                    }
                }
            }
            finally
            {
                foreach (MemoryStream ms in memoryStreams)
                {
                    ms.Dispose();
                }
            }
        }
        else if (Convert_FileStreamRadioButton.Checked)
        {
            FileStream[] fileStreams = new FileStream[byteArrays.Length];
            try
            {
                for (int i = 0; i < rtfFiles.Length; i++)
                {
                    fileStreams[i] = File.OpenRead(rtfFiles[i]);
                }

                using (new TimingScope(totalSize))
                {
                    for (int i = 0; i < fileStreams.Length; i++)
                    {
                        _ = rtfConverter.Convert(fileStreams[i]);
                    }
                }
            }
            finally
            {
                foreach (FileStream ms in fileStreams)
                {
                    ms.Dispose();
                }
            }
        }
        else if (Convert_DeflateStreamRadioButton.Checked)
        {
            string zipFile = Path.Combine(DataDirTextBox.Text, GetDeflateStreamTestFileName(sourceSet));
            using FileStream zipFileStream = File.OpenRead(zipFile);
            using ZipArchive archive = new(zipFileStream, ZipArchiveMode.Read);

            using (new TimingScope(totalSize))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    using Stream es = entry.Open();
                    _ = rtfConverter.Convert(es);
                }
            }
        }
    }

    private static string GetDeflateStreamTestFileName(SourceSet sourceSet) => sourceSet switch
    {
        SourceSet.Full => DeflateStreamTest_Full_FileName,
        SourceSet.Small => DeflateStreamTest_Small_FileName,
        SourceSet.ValidityTest => DeflateStreamTest_Validity_Test_Files_FileName,
        SourceSet.WorkingNewSet => DeflateStreamTest_WorkingNewSet_FileName,
        _ => throw new Exception("No deflate stream test file for the set " + sourceSet),
    };

    private void ConvertWithCustom20x(SourceSet sourceSet)
    {
        (_, byte[][] byteArrays, long totalSize) = GetStuff_Custom(sourceSet);
        RtfToTextConverter rtfConverter = new();

        using (new TimingScope(totalSize * 20))
        {
            for (int c = 0; c < 20; c++)
            {
                for (int i = 0; i < byteArrays.Length; i++)
                {
                    _ = rtfConverter.Convert(byteArrays[i]);
                }
            }
        }
    }

    private void ConvertWithRichTextBox(SourceSet sourceSet)
    {
        (_, MemoryStream[] memStreams, long totalSize) = GetStuff_RichTextBox(sourceSet);
        using var rtfBox = new RichTextBox();

        try
        {
            using (new TimingScope(totalSize))
            {
                for (int i = 0; i < memStreams.Length; i++)
                {
                    rtfBox.LoadFile(memStreams[i], RichTextBoxStreamType.RichText);
                    _ = rtfBox.Text;
                }
            }
        }
        finally
        {
            foreach (MemoryStream ms in memStreams)
            {
                ms.Dispose();
            }
        }
    }

    private void WritePlaintextFile(string f, string text, string destDir, SourceSet sourceSet)
    {
        string ff = f.Substring(GetRtfSetDir(sourceSet).Length).Replace("\\", "__").Replace("/", "__");
        ff = Path.GetFileNameWithoutExtension(ff) + "_rtf_to_plaintext.txt";
        File.WriteAllText(Path.Combine(DataDirTextBox.Text, destDir, ff), text);
    }

    private void HandleOne(bool write)
    {
        RtfToTextConverter rtfConverter = new();

        const string file =
                //"1mil_CinderNotes__CinderNotes.rtf";
                //"param_too_long.rtf"
                //"2000-12-30_Uneaffaireenor__Readme.rtf"
                //"fldinst.rtf"
                //"Issue50-2.rtf"
                //"2007-12-28_DooM_V1_2__ReadMe.rtf"
                "10Rooms_Hammered_EnglishV1_0__FmInfo-en.rtf"
            //"10Rooms_LostInTheFarEdgesV1_1__Lost In The Far Edges.rtf"
            //"2004-02-29_c5Summit_The__summit.rtf"
            //"2007-11-11_WayoftheSword_v1_2__The Way of The Sword - Read Me.rtf"
            //"2002-04-04_Mistrz_ENG__mistrz_eng.rtf"
            //"TDP20AC_An_Enigmatic_Treasure___TDP20AC_An_Enigmatic_Treasure_With_A_Recondite_Discovery.rtf"
            //"Issue23.rtf"
            ;
        SourceSet sourceSet = SourceSet.Full;

        string finalFile = Path.Combine(GetRtfSetDir(sourceSet), file);

        using var fs = File.OpenRead(finalFile);
        //byte[] array = new byte[fs.Length];
        //fs.ReadExactly(array, 0, (int)fs.Length);
        RtfResult result = rtfConverter.Convert(fs);
        Trace.WriteLine(result.ToString());
        if (write)
        {
            string outputDir =
                sourceSet switch
                {
                    SourceSet.ValidityTest => _rtfValidityTestOutputCustomDir,
                    SourceSet.WorkingNewSet => _outputWorkingNewSetCustomDir,
                    _ => _outputCustomDir,
                };

            WritePlaintextFile(finalFile, result.Text, outputDir, sourceSet);
        }
    }

    #region Convert and write

    private void ConvertAndWrite_RichTextBox(SourceSet sourceSet, string outputDir)
    {
        (string[] rtfFiles, MemoryStream[] memStreams, long totalSize) = GetStuff_RichTextBox(sourceSet);
        using var rtfBox = new RichTextBox();

        try
        {
            using (new TimingScope(totalSize))
            {
                for (int i = 0; i < memStreams.Length; i++)
                {
                    string f = rtfFiles[i];
                    string text;
                    try
                    {
                        rtfBox.LoadFile(memStreams[i], RichTextBoxStreamType.RichText);
                        // On .NET 8, RichTextBox plaintext has vertical tabs (0x0B) in place of \line keywords(?!?!)
                        text = rtfBox.Text.Replace('\x0B', '\n');
                    }
                    catch
                    {
                        text = "<RichTextBox could not convert this file>";
                    }
                    WritePlaintextFile(f, text, outputDir, sourceSet);
                }
            }
        }
        finally
        {
            foreach (MemoryStream ms in memStreams)
            {
                ms.Dispose();
            }
        }
    }

    private void ConvertAndWrite_Custom(SourceSet sourceSet, string outputDir)
    {
        (string[] rtfFiles, byte[][] byteArrays, long totalSize) = GetStuff_Custom(sourceSet);
        RtfToTextConverter rtfConverter = new();

        if (Convert_ByteArrayRadioButton.Checked)
        {
            using (new TimingScope(totalSize))
            {
                for (int i = 0; i < byteArrays.Length; i++)
                {
                    string f = rtfFiles[i];
                    RtfResult result = rtfConverter.Convert(byteArrays[i]);
                    WritePlaintextFile(f, result.Text, outputDir, sourceSet);
                }
            }
        }
        else if (Convert_MemoryStreamRadioButton.Checked)
        {
            MemoryStream[] memoryStreams = new MemoryStream[byteArrays.Length];
            try
            {
                for (int i = 0; i < byteArrays.Length; i++)
                {
                    memoryStreams[i] = new MemoryStream(byteArrays[i]);
                }

                using (new TimingScope(totalSize))
                {
                    for (int i = 0; i < memoryStreams.Length; i++)
                    {
                        string f = rtfFiles[i];
                        Trace.WriteLine(f);
                        using var fs = File.OpenRead(f);
                        //if (f.Contains("WayoftheSword"))
                        {
                            RtfResult result = rtfConverter.Convert(fs);
                            if (result.Error != RtfError.OK)
                            {
                                Trace.WriteLine(result);
                            }
                            WritePlaintextFile(f, result.Text, outputDir, sourceSet);
                        }
                    }
                }
            }
            finally
            {
                foreach (MemoryStream ms in memoryStreams)
                {
                    ms.Dispose();
                }
            }
        }
        else if (Convert_FileStreamRadioButton.Checked)
        {
            FileStream[] fileStreams = new FileStream[byteArrays.Length];
            try
            {
                for (int i = 0; i < rtfFiles.Length; i++)
                {
                    fileStreams[i] = File.OpenRead(rtfFiles[i]);
                }

                using (new TimingScope(totalSize))
                {
                    for (int i = 0; i < fileStreams.Length; i++)
                    {
                        string f = rtfFiles[i];
                        RtfResult result = rtfConverter.Convert(fileStreams[i]);
                        WritePlaintextFile(f, result.Text, outputDir, sourceSet);
                    }
                }
            }
            finally
            {
                foreach (FileStream ms in fileStreams)
                {
                    ms.Dispose();
                }
            }
        }
        else if (Convert_DeflateStreamRadioButton.Checked)
        {
            string setDir = GetRtfSetDir(sourceSet);
            string zipFile = Path.Combine(DataDirTextBox.Text, GetDeflateStreamTestFileName(sourceSet));
            using FileStream zipFileStream = File.OpenRead(zipFile);
            using ZipArchive archive = new(zipFileStream, ZipArchiveMode.Read);

            using (new TimingScope(totalSize))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string f = Path.Combine(setDir, entry.Name);
                    using Stream es = entry.Open();
                    RtfResult result = rtfConverter.Convert(es);
                    WritePlaintextFile(f, result.Text, outputDir, sourceSet);
                }
            }
        }
    }

    private void ConvertAndWriteWithRichTextBoxButton_Click(object sender, EventArgs e)
    {
        ConvertAndWrite_RichTextBox(SourceSet.Full, _outputRichTextBoxDir);
    }

    private void ConvertAndWriteWithCustomButton_Click(object sender, EventArgs e)
    {
        ConvertAndWrite_Custom(SourceSet.Full, _outputCustomDir);
    }

    #region Write validity test

    private void ConvertAndWriteValidityTestFiles_RTB_Button_Click(object sender, EventArgs e)
    {
        ConvertAndWrite_RichTextBox(SourceSet.ValidityTest, _rtfValidityTestOutputRichTextBoxDir);
    }

    private void ConvertAndWriteValidityTestFiles_Custom_Button_Click(object sender, EventArgs e)
    {
        ConvertAndWrite_Custom(SourceSet.ValidityTest, _rtfValidityTestOutputCustomDir);
    }

    #endregion

    private void WriteWorkingNewSetRTBButton_Click(object sender, EventArgs e)
    {
        ConvertAndWrite_RichTextBox(SourceSet.WorkingNewSet, _outputWorkingNewSetRichTextBoxDir);
    }

    private void WriteWorkingNewSetCustomButton_Click(object sender, EventArgs e)
    {
        ConvertAndWrite_Custom(SourceSet.WorkingNewSet, _outputWorkingNewSetCustomDir);
    }

    #endregion

    #region Convert only (full)

    private void ConvertOnlyWithRichTextBoxButton_Click(object sender, EventArgs e)
    {
        ConvertWithRichTextBox(SourceSet.Full);
    }

    private void ConvertOnlyWithCustomButton_Click(object sender, EventArgs e)
    {
        ConvertWithCustom(SourceSet.Full);
    }

    private void ConvertOnlyWithCustom20XButton_Click(object sender, EventArgs e)
    {
        ConvertWithCustom20x(SourceSet.Full);
    }

    #endregion

    #region Convert only (small)

    private void ConvertOnlyWithRichTextBox_Small_Button_Click(object sender, EventArgs e)
    {
        ConvertWithRichTextBox(SourceSet.Small);
    }

    private void ConvertOnlyWithCustom_Small_Button_Click(object sender, EventArgs e)
    {
        ConvertWithCustom(SourceSet.Small);
    }

    private void ConvertOnlyWithCustom20X_Small_Button_Click(object sender, EventArgs e)
    {
        ConvertWithCustom20x(SourceSet.Small);
    }

    #endregion

    #region One

    private void ConvertOneButton_Click(object sender, EventArgs e)
    {
        HandleOne(write: false);
    }

    private void WriteOneButton_Click(object sender, EventArgs e)
    {
        HandleOne(write: true);
    }

    #endregion

    private void DataDirBrowseButton_Click(object sender, EventArgs e)
    {
        using var d = new FolderBrowserDialog();
        DialogResult result = d.ShowDialog(this);
        if (result != DialogResult.OK) return;
        DataDirTextBox.Text = d.SelectedPath;
    }

    private void DataDirResetButton_Click(object sender, EventArgs e)
    {
        ResetDataDir();
    }

    private long GetDirectorySize(SourceSet sourceSet)
    {
        long totalSize = 0;
        var di = new DirectoryInfo(GetRtfSetDir(sourceSet));
        foreach (var fi in di.EnumerateFiles())
        {
            totalSize += fi.Length;
        }
        return totalSize;
    }

    private void Test1Button_Click(object sender, EventArgs e)
    {
        // Change this when we want to re-measure the benchmark MB/s

        long fullBytes = GetDirectorySize(SourceSet.Full);
        long smallBytes = GetDirectorySize(SourceSet.Small);

        Trace.WriteLine("RTB Full MB/s: " + GetMBsString(fullBytes, 3331.340));

        Trace.WriteLine("RTB Small MB/s: " + GetMBsString(smallBytes, 1432.217));

        Trace.WriteLine("RC Full (Streamable/Array) MB/s: " + GetMBsString(fullBytes, 28.777));

        Trace.WriteLine("RC Small (Streamable/Array) MB/s: " + GetMBsString(smallBytes, 8.008));

        Trace.WriteLine("RC Full (Streamable/Stream) MB/s: " + GetMBsString(fullBytes, 30.587));

        Trace.WriteLine("RC Small (Streamable/Stream) MB/s: " + GetMBsString(smallBytes, 7.925));
    }
}
