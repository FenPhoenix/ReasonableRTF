using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace FenGen.Forms;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public sealed partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

    private void GenerateButton_Click(object sender, EventArgs e)
    {
        Core.DoTasks();
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        Environment.Exit(0);
    }
}
