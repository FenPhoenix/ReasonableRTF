using System.Diagnostics;
using System.Globalization;
using ReasonableRTF;

namespace ReasonableRTF_TestApp;

public sealed partial class MainForm : Form
{
    private const string _configFile = "Config.ini";
    private const string _rtfFullSetDir = "RTF_Test_Set_Full";
    private const string _rtfSmallSetDir = "RTF_Test_Set_Small";
    private const string _outputCustomDir = "Output_Custom";
    private const string _outputRichTextBoxDir = "Output_RichTextBox";
    private const string _rftValidityTestDir = "Validity_Test_Files";
    private const string _rtfValidityTestOutputCustomDir = "Output_Validity_Test_Custom";
    private const string _rtfValidityTestOutputRichTextBoxDir = "Output_Validity_Test_RichTextBox";

    private enum SourceSet
    {
        Full,
        Small,
        ValidityTest,
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

    private static string GetMBsString(long totalSize, long elapsedMilliseconds)
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
        (_, byte[][] byteArrays, long totalSize) = GetStuff_Custom(sourceSet);
        RtfToTextConverter rtfConverter = new();

        using (new TimingScope(totalSize))
        {
            for (int i = 0; i < byteArrays.Length; i++)
            {
                _ = rtfConverter.Convert(byteArrays[i]);
            }
        }
    }

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
                "fldinst.rtf"
            ;
        SourceSet sourceSet = SourceSet.ValidityTest;

        string finalFile = Path.Combine(GetRtfSetDir(sourceSet), file);

        using var fs = File.OpenRead(finalFile);
        byte[] array = new byte[fs.Length];
        fs.ReadExactly(array, 0, (int)fs.Length);
        RtfResult result = rtfConverter.Convert(array);
        Trace.WriteLine(result.ToString());
        if (write)
        {
            string outputDir = sourceSet == SourceSet.ValidityTest
                ? _rtfValidityTestOutputCustomDir
                : _outputCustomDir;

            WritePlaintextFile(finalFile, result.Text, outputDir, sourceSet);
        }
    }

    #region Convert and write

    private void ConvertAndWriteWithRichTextBoxButton_Click(object sender, EventArgs e)
    {
        (string[] rtfFiles, MemoryStream[] memStreams, long totalSize) = GetStuff_RichTextBox(SourceSet.Full);
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
                    WritePlaintextFile(f, text, _outputRichTextBoxDir, SourceSet.Full);
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

    private void ConvertAndWriteWithCustomButton_Click(object sender, EventArgs e)
    {
        (string[] rtfFiles, byte[][] byteArrays, long totalSize) = GetStuff_Custom(SourceSet.Full);
        RtfToTextConverter rtfConverter = new();

        using (new TimingScope(totalSize))
        {
            for (int i = 0; i < byteArrays.Length; i++)
            {
                string f = rtfFiles[i];
                Trace.WriteLine(f);
                byte[] array = byteArrays[i];
                RtfResult result = rtfConverter.Convert(array);
                WritePlaintextFile(f, result.Text, _outputCustomDir, SourceSet.Full);
            }
        }
    }

    #region Write validity test

    private void ConvertAndWriteValidityTestFiles_RTB_Button_Click(object sender, EventArgs e)
    {
        (string[] rtfFiles, MemoryStream[] memStreams, long totalSize) = GetStuff_RichTextBox(SourceSet.ValidityTest);
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
                    WritePlaintextFile(f, text, _rtfValidityTestOutputRichTextBoxDir, SourceSet.ValidityTest);
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

    private void ConvertAndWriteValidityTestFiles_Custom_Button_Click(object sender, EventArgs e)
    {
        (string[] rtfFiles, byte[][] byteArrays, long totalSize) = GetStuff_Custom(SourceSet.ValidityTest);
        RtfToTextConverter rtfConverter = new();

        using (new TimingScope(totalSize))
        {
            for (int i = 0; i < byteArrays.Length; i++)
            {
                string f = rtfFiles[i];
                Trace.WriteLine(f);
                byte[] array = byteArrays[i];
                RtfResult result = rtfConverter.Convert(array);
                WritePlaintextFile(f, result.Text, _rtfValidityTestOutputCustomDir, SourceSet.ValidityTest);
            }
        }
    }

    #endregion

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

    private void Test1Button_Click(object sender, EventArgs e)
    {
    }
}
