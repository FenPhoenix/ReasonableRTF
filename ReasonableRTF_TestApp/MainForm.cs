namespace ReasonableRTF_TestApp;

public sealed partial class MainForm : Form
{
    private const string _configFile = "Config.ini";

    public MainForm()
    {
        InitializeComponent();
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
                    DataDirTextBox.Text = lineT[("DataDir=".Length + 1)..];
                }
            }
            return;
        }
        catch
        {
            // ignore
        }

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

    private void MainForm_Load(object sender, EventArgs e)
    {
        LoadConfig();
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        SaveConfig();
    }

    #region Convert and write

    private void ConvertAndWriteWithRichTextBoxButton_Click(object sender, EventArgs e)
    {

    }

    private void ConvertAndWriteWithCustomButton_Click(object sender, EventArgs e)
    {

    }

    #endregion

    #region Convert only (full)

    private void ConvertOnlyWithRichTextBoxButton_Click(object sender, EventArgs e)
    {

    }

    private void ConvertOnlyWithCustomButton_Click(object sender, EventArgs e)
    {

    }

    private void ConvertOnlyWithCustom20XButton_Click(object sender, EventArgs e)
    {

    }

    #endregion

    #region Convert only (small)

    private void ConvertOnlyWithRichTextBox_Small_Button_Click(object sender, EventArgs e)
    {

    }

    private void ConvertOnlyWithCustom_Small_Button_Click(object sender, EventArgs e)
    {

    }

    private void ConvertOnlyWithCustom20X_Small_Button_Click(object sender, EventArgs e)
    {

    }

    #endregion

    #region One

    private void ConvertOneButton_Click(object sender, EventArgs e)
    {

    }

    private void WriteOneButton_Click(object sender, EventArgs e)
    {

    }

    #endregion

    private void Test1Button_Click(object sender, EventArgs e)
    {
    }

    private void DataDirBrowseButton_Click(object sender, EventArgs e)
    {

    }
}
