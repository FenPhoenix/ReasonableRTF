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
                    DataDirTextBox.Text = lineT[("DataDir=".Length)..];
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

    private string GetRtfSetDir(bool small)
    {
        string dir = small ? _rtfSmallSetDir : _rtfFullSetDir;
        return Path.Combine(DataDirTextBox.Text, dir);
    }

    private (string[] RtfFiles, MemoryStream[] MemStreams, long TotalSize)
    GetStuff_RichTextBox(bool small)
    {
        string[] rtfFiles = Directory.GetFiles(GetRtfSetDir(small));
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
    GetStuff_Custom(bool small)
    {
        string[] rtfFiles = Directory.GetFiles(GetRtfSetDir(small));

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

    private void ConvertWithCustom(bool small)
    {
        (_, byte[][] byteArrays, long totalSize) = GetStuff_Custom(small);
        RtfToTextConverter rtfConverter = new();

        using (new TimingScope(totalSize))
        {
            for (int i = 0; i < byteArrays.Length; i++)
            {
                _ = rtfConverter.Convert(byteArrays[i]);
            }
        }
    }

    private void ConvertWithCustom20x(bool small)
    {
        (_, byte[][] byteArrays, long totalSize) = GetStuff_Custom(small);
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

    private void ConvertWithRichTextBox(bool small)
    {
        (_, MemoryStream[] memStreams, long totalSize) = GetStuff_RichTextBox(small);
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

    private void WritePlaintextFile(string f, string text, string destDir, bool small)
    {
        string ff = f.Substring(GetRtfSetDir(small).Length).Replace("\\", "__").Replace("/", "__");
        ff = Path.GetFileNameWithoutExtension(ff) + "_rtf_to_plaintext.txt";
        File.WriteAllText(Path.Combine(DataDirTextBox.Text, destDir, ff), text);
    }

    private void HandleOne(bool write)
    {
        RtfToTextConverter rtfConverter = new();

        const string file = "1mil_CinderNotes__CinderNotes.rtf";

        string finalFile = Path.Combine(GetRtfSetDir(small: false), file);

        using var fs = File.OpenRead(finalFile);
        byte[] array = new byte[fs.Length];
        fs.ReadExactly(array, 0, (int)fs.Length);
        (_, string text) = rtfConverter.Convert(array);
        if (write)
        {
            WritePlaintextFile(finalFile, text, _outputCustomDir, small: false);
        }
    }

    #region Convert and write

    private void ConvertAndWriteWithRichTextBoxButton_Click(object sender, EventArgs e)
    {
        (string[] rtfFiles, MemoryStream[] memStreams, long totalSize) = GetStuff_RichTextBox(small: false);
        using var rtfBox = new RichTextBox();

        try
        {
            using (new TimingScope(totalSize))
            {
                for (int i = 0; i < memStreams.Length; i++)
                {
                    rtfBox.LoadFile(memStreams[i], RichTextBoxStreamType.RichText);
                    string f = rtfFiles[i];
                    // On .NET 8, RichTextBox plaintext has vertical tabs (0x0B) in place of \line keywords(?!?!)
                    string text = rtfBox.Text.Replace('\x0B', '\n');
                    WritePlaintextFile(f, text, _outputRichTextBoxDir, small: false);
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
        (string[] rtfFiles, byte[][] byteArrays, long totalSize) = GetStuff_Custom(small: false);
        RtfToTextConverter rtfConverter = new();

        using (new TimingScope(totalSize))
        {
            for (int i = 0; i < byteArrays.Length; i++)
            {
                string f = rtfFiles[i];
                Trace.WriteLine(f);
                byte[] array = byteArrays[i];
                (_, string text) = rtfConverter.Convert(array);
                WritePlaintextFile(f, text, _outputCustomDir, small: false);
            }
        }
    }

    #endregion

    #region Convert only (full)

    private void ConvertOnlyWithRichTextBoxButton_Click(object sender, EventArgs e)
    {
        ConvertWithRichTextBox(small: false);
    }

    private void ConvertOnlyWithCustomButton_Click(object sender, EventArgs e)
    {
        ConvertWithCustom(small: false);
    }

    private void ConvertOnlyWithCustom20XButton_Click(object sender, EventArgs e)
    {
        ConvertWithCustom20x(small: false);
    }

    #endregion

    #region Convert only (small)

    private void ConvertOnlyWithRichTextBox_Small_Button_Click(object sender, EventArgs e)
    {
        ConvertWithRichTextBox(small: true);
    }

    private void ConvertOnlyWithCustom_Small_Button_Click(object sender, EventArgs e)
    {
        ConvertWithCustom(small: true);
    }

    private void ConvertOnlyWithCustom20X_Small_Button_Click(object sender, EventArgs e)
    {
        ConvertWithCustom20x(small: true);
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

    private void Test1Button_Click(object sender, EventArgs e)
    {
    }

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
}
