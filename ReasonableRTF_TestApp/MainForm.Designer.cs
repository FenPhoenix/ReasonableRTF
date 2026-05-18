namespace ReasonableRTF_TestApp;

sealed partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        Test1Button = new Button();
        ConvertAndWriteToDiskGroupBox = new GroupBox();
        ConvertAndWriteWithRichTextBoxButton = new Button();
        ConvertAndWriteWithCustomButton = new Button();
        ConvertOnly_Full_GroupBox = new GroupBox();
        ConvertOnlyWithRichTextBoxButton = new Button();
        ConvertOnlyWithCustom20XButton = new Button();
        ConvertOnlyWithCustomButton = new Button();
        ConverOnly_Small_GroupBox = new GroupBox();
        ConvertOnlyWithRichTextBox_Small_Button = new Button();
        ConvertOnlyWithCustom20X_Small_Button = new Button();
        ConvertOnlyWithCustom_Small_Button = new Button();
        ConvertOneButton = new Button();
        WriteOneButton = new Button();
        ValidityTestGroupBox = new GroupBox();
        ConvertAndWriteValidityTestFiles_Custom_Button = new Button();
        ConvertAndWriteValidityTestFiles_RTB_Button = new Button();
        WriteWorkingNewSetGroupBox = new GroupBox();
        WriteWorkingNewSetRTBButton = new Button();
        WriteWorkingNewSetCustomButton = new Button();
        ConvertSourceGroupBox = new GroupBox();
        Convert_DeflateStreamRadioButton = new RadioButton();
        Convert_FileStreamRadioButton = new RadioButton();
        Convert_MemoryStreamRadioButton = new RadioButton();
        Convert_ByteArrayRadioButton = new RadioButton();
        WriteBenchmarkFilesGroupBox = new GroupBox();
        AllTargetsButton = new Button();
        Net48_32Button = new Button();
        Net48_64Button = new Button();
        Net64Button = new Button();
        ConvertAndWriteToDiskGroupBox.SuspendLayout();
        ConvertOnly_Full_GroupBox.SuspendLayout();
        ConverOnly_Small_GroupBox.SuspendLayout();
        ValidityTestGroupBox.SuspendLayout();
        WriteWorkingNewSetGroupBox.SuspendLayout();
        ConvertSourceGroupBox.SuspendLayout();
        WriteBenchmarkFilesGroupBox.SuspendLayout();
        SuspendLayout();
        // 
        // Test1Button
        // 
        Test1Button.Location = new Point(504, 16);
        Test1Button.Name = "Test1Button";
        Test1Button.Size = new Size(91, 27);
        Test1Button.TabIndex = 7;
        Test1Button.Text = "Test";
        Test1Button.UseVisualStyleBackColor = true;
        Test1Button.Click += Test1Button_Click;
        // 
        // ConvertAndWriteToDiskGroupBox
        // 
        ConvertAndWriteToDiskGroupBox.Controls.Add(ConvertAndWriteWithRichTextBoxButton);
        ConvertAndWriteToDiskGroupBox.Controls.Add(ConvertAndWriteWithCustomButton);
        ConvertAndWriteToDiskGroupBox.Location = new Point(16, 16);
        ConvertAndWriteToDiskGroupBox.Margin = new Padding(4, 3, 4, 3);
        ConvertAndWriteToDiskGroupBox.Name = "ConvertAndWriteToDiskGroupBox";
        ConvertAndWriteToDiskGroupBox.Padding = new Padding(4, 3, 4, 3);
        ConvertAndWriteToDiskGroupBox.Size = new Size(233, 64);
        ConvertAndWriteToDiskGroupBox.TabIndex = 1;
        ConvertAndWriteToDiskGroupBox.TabStop = false;
        ConvertAndWriteToDiskGroupBox.Text = "Write converted files to disk";
        // 
        // ConvertAndWriteWithRichTextBoxButton
        // 
        ConvertAndWriteWithRichTextBoxButton.Location = new Point(16, 24);
        ConvertAndWriteWithRichTextBoxButton.Margin = new Padding(4, 3, 4, 3);
        ConvertAndWriteWithRichTextBoxButton.Name = "ConvertAndWriteWithRichTextBoxButton";
        ConvertAndWriteWithRichTextBoxButton.Size = new Size(93, 27);
        ConvertAndWriteWithRichTextBoxButton.TabIndex = 0;
        ConvertAndWriteWithRichTextBoxButton.Text = "RichTextBox";
        ConvertAndWriteWithRichTextBoxButton.UseVisualStyleBackColor = true;
        ConvertAndWriteWithRichTextBoxButton.Click += ConvertAndWriteWithRichTextBoxButton_Click;
        // 
        // ConvertAndWriteWithCustomButton
        // 
        ConvertAndWriteWithCustomButton.Location = new Point(120, 24);
        ConvertAndWriteWithCustomButton.Margin = new Padding(4, 3, 4, 3);
        ConvertAndWriteWithCustomButton.Name = "ConvertAndWriteWithCustomButton";
        ConvertAndWriteWithCustomButton.Size = new Size(93, 27);
        ConvertAndWriteWithCustomButton.TabIndex = 1;
        ConvertAndWriteWithCustomButton.Text = "Custom*";
        ConvertAndWriteWithCustomButton.UseVisualStyleBackColor = true;
        ConvertAndWriteWithCustomButton.Click += ConvertAndWriteWithCustomButton_Click;
        // 
        // ConvertOnly_Full_GroupBox
        // 
        ConvertOnly_Full_GroupBox.Controls.Add(ConvertOnlyWithRichTextBoxButton);
        ConvertOnly_Full_GroupBox.Controls.Add(ConvertOnlyWithCustom20XButton);
        ConvertOnly_Full_GroupBox.Controls.Add(ConvertOnlyWithCustomButton);
        ConvertOnly_Full_GroupBox.Location = new Point(16, 88);
        ConvertOnly_Full_GroupBox.Margin = new Padding(4, 3, 4, 3);
        ConvertOnly_Full_GroupBox.Name = "ConvertOnly_Full_GroupBox";
        ConvertOnly_Full_GroupBox.Padding = new Padding(4, 3, 4, 3);
        ConvertOnly_Full_GroupBox.Size = new Size(336, 64);
        ConvertOnly_Full_GroupBox.TabIndex = 2;
        ConvertOnly_Full_GroupBox.TabStop = false;
        ConvertOnly_Full_GroupBox.Text = "Convert only (full set):";
        // 
        // ConvertOnlyWithRichTextBoxButton
        // 
        ConvertOnlyWithRichTextBoxButton.Location = new Point(16, 24);
        ConvertOnlyWithRichTextBoxButton.Margin = new Padding(4, 3, 4, 3);
        ConvertOnlyWithRichTextBoxButton.Name = "ConvertOnlyWithRichTextBoxButton";
        ConvertOnlyWithRichTextBoxButton.Size = new Size(93, 27);
        ConvertOnlyWithRichTextBoxButton.TabIndex = 0;
        ConvertOnlyWithRichTextBoxButton.Text = "RichTextBox";
        ConvertOnlyWithRichTextBoxButton.UseVisualStyleBackColor = true;
        ConvertOnlyWithRichTextBoxButton.Click += ConvertOnlyWithRichTextBoxButton_Click;
        // 
        // ConvertOnlyWithCustom20XButton
        // 
        ConvertOnlyWithCustom20XButton.Location = new Point(224, 24);
        ConvertOnlyWithCustom20XButton.Margin = new Padding(4, 3, 4, 3);
        ConvertOnlyWithCustom20XButton.Name = "ConvertOnlyWithCustom20XButton";
        ConvertOnlyWithCustom20XButton.Size = new Size(93, 27);
        ConvertOnlyWithCustom20XButton.TabIndex = 2;
        ConvertOnlyWithCustom20XButton.Text = "Custom 20x";
        ConvertOnlyWithCustom20XButton.UseVisualStyleBackColor = true;
        ConvertOnlyWithCustom20XButton.Click += ConvertOnlyWithCustom20XButton_Click;
        // 
        // ConvertOnlyWithCustomButton
        // 
        ConvertOnlyWithCustomButton.Location = new Point(120, 24);
        ConvertOnlyWithCustomButton.Margin = new Padding(4, 3, 4, 3);
        ConvertOnlyWithCustomButton.Name = "ConvertOnlyWithCustomButton";
        ConvertOnlyWithCustomButton.Size = new Size(93, 27);
        ConvertOnlyWithCustomButton.TabIndex = 1;
        ConvertOnlyWithCustomButton.Text = "Custom*";
        ConvertOnlyWithCustomButton.UseVisualStyleBackColor = true;
        ConvertOnlyWithCustomButton.Click += ConvertOnlyWithCustomButton_Click;
        // 
        // ConverOnly_Small_GroupBox
        // 
        ConverOnly_Small_GroupBox.Controls.Add(ConvertOnlyWithRichTextBox_Small_Button);
        ConverOnly_Small_GroupBox.Controls.Add(ConvertOnlyWithCustom20X_Small_Button);
        ConverOnly_Small_GroupBox.Controls.Add(ConvertOnlyWithCustom_Small_Button);
        ConverOnly_Small_GroupBox.Location = new Point(16, 160);
        ConverOnly_Small_GroupBox.Margin = new Padding(4, 3, 4, 3);
        ConverOnly_Small_GroupBox.Name = "ConverOnly_Small_GroupBox";
        ConverOnly_Small_GroupBox.Padding = new Padding(4, 3, 4, 3);
        ConverOnly_Small_GroupBox.Size = new Size(336, 64);
        ConverOnly_Small_GroupBox.TabIndex = 3;
        ConverOnly_Small_GroupBox.TabStop = false;
        ConverOnly_Small_GroupBox.Text = "Convert only (small-file set):";
        // 
        // ConvertOnlyWithRichTextBox_Small_Button
        // 
        ConvertOnlyWithRichTextBox_Small_Button.Location = new Point(16, 24);
        ConvertOnlyWithRichTextBox_Small_Button.Margin = new Padding(4, 3, 4, 3);
        ConvertOnlyWithRichTextBox_Small_Button.Name = "ConvertOnlyWithRichTextBox_Small_Button";
        ConvertOnlyWithRichTextBox_Small_Button.Size = new Size(93, 27);
        ConvertOnlyWithRichTextBox_Small_Button.TabIndex = 0;
        ConvertOnlyWithRichTextBox_Small_Button.Text = "RichTextBox";
        ConvertOnlyWithRichTextBox_Small_Button.UseVisualStyleBackColor = true;
        ConvertOnlyWithRichTextBox_Small_Button.Click += ConvertOnlyWithRichTextBox_Small_Button_Click;
        // 
        // ConvertOnlyWithCustom20X_Small_Button
        // 
        ConvertOnlyWithCustom20X_Small_Button.Location = new Point(224, 24);
        ConvertOnlyWithCustom20X_Small_Button.Margin = new Padding(4, 3, 4, 3);
        ConvertOnlyWithCustom20X_Small_Button.Name = "ConvertOnlyWithCustom20X_Small_Button";
        ConvertOnlyWithCustom20X_Small_Button.Size = new Size(93, 27);
        ConvertOnlyWithCustom20X_Small_Button.TabIndex = 2;
        ConvertOnlyWithCustom20X_Small_Button.Text = "Custom 20x";
        ConvertOnlyWithCustom20X_Small_Button.UseVisualStyleBackColor = true;
        ConvertOnlyWithCustom20X_Small_Button.Click += ConvertOnlyWithCustom20X_Small_Button_Click;
        // 
        // ConvertOnlyWithCustom_Small_Button
        // 
        ConvertOnlyWithCustom_Small_Button.Location = new Point(120, 24);
        ConvertOnlyWithCustom_Small_Button.Margin = new Padding(4, 3, 4, 3);
        ConvertOnlyWithCustom_Small_Button.Name = "ConvertOnlyWithCustom_Small_Button";
        ConvertOnlyWithCustom_Small_Button.Size = new Size(93, 27);
        ConvertOnlyWithCustom_Small_Button.TabIndex = 1;
        ConvertOnlyWithCustom_Small_Button.Text = "Custom*";
        ConvertOnlyWithCustom_Small_Button.UseVisualStyleBackColor = true;
        ConvertOnlyWithCustom_Small_Button.Click += ConvertOnlyWithCustom_Small_Button_Click;
        // 
        // ConvertOneButton
        // 
        ConvertOneButton.Location = new Point(392, 16);
        ConvertOneButton.Name = "ConvertOneButton";
        ConvertOneButton.Size = new Size(91, 27);
        ConvertOneButton.TabIndex = 5;
        ConvertOneButton.Text = "Convert one";
        ConvertOneButton.UseVisualStyleBackColor = true;
        ConvertOneButton.Click += ConvertOneButton_Click;
        // 
        // WriteOneButton
        // 
        WriteOneButton.Location = new Point(392, 48);
        WriteOneButton.Name = "WriteOneButton";
        WriteOneButton.Size = new Size(91, 27);
        WriteOneButton.TabIndex = 6;
        WriteOneButton.Text = "Write one";
        WriteOneButton.UseVisualStyleBackColor = true;
        WriteOneButton.Click += WriteOneButton_Click;
        // 
        // ValidityTestGroupBox
        // 
        ValidityTestGroupBox.Controls.Add(ConvertAndWriteValidityTestFiles_Custom_Button);
        ValidityTestGroupBox.Controls.Add(ConvertAndWriteValidityTestFiles_RTB_Button);
        ValidityTestGroupBox.Location = new Point(376, 88);
        ValidityTestGroupBox.Name = "ValidityTestGroupBox";
        ValidityTestGroupBox.Size = new Size(232, 64);
        ValidityTestGroupBox.TabIndex = 4;
        ValidityTestGroupBox.TabStop = false;
        ValidityTestGroupBox.Text = "Write converted validity test files";
        // 
        // ConvertAndWriteValidityTestFiles_Custom_Button
        // 
        ConvertAndWriteValidityTestFiles_Custom_Button.Location = new Point(120, 24);
        ConvertAndWriteValidityTestFiles_Custom_Button.Name = "ConvertAndWriteValidityTestFiles_Custom_Button";
        ConvertAndWriteValidityTestFiles_Custom_Button.Size = new Size(93, 27);
        ConvertAndWriteValidityTestFiles_Custom_Button.TabIndex = 1;
        ConvertAndWriteValidityTestFiles_Custom_Button.Text = "Custom *";
        ConvertAndWriteValidityTestFiles_Custom_Button.UseVisualStyleBackColor = true;
        ConvertAndWriteValidityTestFiles_Custom_Button.Click += ConvertAndWriteValidityTestFiles_Custom_Button_Click;
        // 
        // ConvertAndWriteValidityTestFiles_RTB_Button
        // 
        ConvertAndWriteValidityTestFiles_RTB_Button.Location = new Point(16, 24);
        ConvertAndWriteValidityTestFiles_RTB_Button.Name = "ConvertAndWriteValidityTestFiles_RTB_Button";
        ConvertAndWriteValidityTestFiles_RTB_Button.Size = new Size(93, 27);
        ConvertAndWriteValidityTestFiles_RTB_Button.TabIndex = 0;
        ConvertAndWriteValidityTestFiles_RTB_Button.Text = "RichTextBox";
        ConvertAndWriteValidityTestFiles_RTB_Button.UseVisualStyleBackColor = true;
        ConvertAndWriteValidityTestFiles_RTB_Button.Click += ConvertAndWriteValidityTestFiles_RTB_Button_Click;
        // 
        // WriteWorkingNewSetGroupBox
        // 
        WriteWorkingNewSetGroupBox.Controls.Add(WriteWorkingNewSetRTBButton);
        WriteWorkingNewSetGroupBox.Controls.Add(WriteWorkingNewSetCustomButton);
        WriteWorkingNewSetGroupBox.Location = new Point(376, 160);
        WriteWorkingNewSetGroupBox.Margin = new Padding(4, 3, 4, 3);
        WriteWorkingNewSetGroupBox.Name = "WriteWorkingNewSetGroupBox";
        WriteWorkingNewSetGroupBox.Padding = new Padding(4, 3, 4, 3);
        WriteWorkingNewSetGroupBox.Size = new Size(233, 64);
        WriteWorkingNewSetGroupBox.TabIndex = 1;
        WriteWorkingNewSetGroupBox.TabStop = false;
        WriteWorkingNewSetGroupBox.Text = "Write (working new set)";
        // 
        // WriteWorkingNewSetRTBButton
        // 
        WriteWorkingNewSetRTBButton.Location = new Point(16, 24);
        WriteWorkingNewSetRTBButton.Margin = new Padding(4, 3, 4, 3);
        WriteWorkingNewSetRTBButton.Name = "WriteWorkingNewSetRTBButton";
        WriteWorkingNewSetRTBButton.Size = new Size(93, 27);
        WriteWorkingNewSetRTBButton.TabIndex = 0;
        WriteWorkingNewSetRTBButton.Text = "RichTextBox";
        WriteWorkingNewSetRTBButton.UseVisualStyleBackColor = true;
        WriteWorkingNewSetRTBButton.Click += WriteWorkingNewSetRTBButton_Click;
        // 
        // WriteWorkingNewSetCustomButton
        // 
        WriteWorkingNewSetCustomButton.Location = new Point(120, 24);
        WriteWorkingNewSetCustomButton.Margin = new Padding(4, 3, 4, 3);
        WriteWorkingNewSetCustomButton.Name = "WriteWorkingNewSetCustomButton";
        WriteWorkingNewSetCustomButton.Size = new Size(93, 27);
        WriteWorkingNewSetCustomButton.TabIndex = 1;
        WriteWorkingNewSetCustomButton.Text = "Custom *";
        WriteWorkingNewSetCustomButton.UseVisualStyleBackColor = true;
        WriteWorkingNewSetCustomButton.Click += WriteWorkingNewSetCustomButton_Click;
        // 
        // ConvertSourceGroupBox
        // 
        ConvertSourceGroupBox.Controls.Add(Convert_DeflateStreamRadioButton);
        ConvertSourceGroupBox.Controls.Add(Convert_FileStreamRadioButton);
        ConvertSourceGroupBox.Controls.Add(Convert_MemoryStreamRadioButton);
        ConvertSourceGroupBox.Controls.Add(Convert_ByteArrayRadioButton);
        ConvertSourceGroupBox.Location = new Point(16, 232);
        ConvertSourceGroupBox.Name = "ConvertSourceGroupBox";
        ConvertSourceGroupBox.Size = new Size(592, 56);
        ConvertSourceGroupBox.TabIndex = 8;
        ConvertSourceGroupBox.TabStop = false;
        ConvertSourceGroupBox.Text = "Convert source (applies only to buttons marked with *)";
        // 
        // Convert_DeflateStreamRadioButton
        // 
        Convert_DeflateStreamRadioButton.AutoSize = true;
        Convert_DeflateStreamRadioButton.Location = new Point(312, 24);
        Convert_DeflateStreamRadioButton.Name = "Convert_DeflateStreamRadioButton";
        Convert_DeflateStreamRadioButton.Size = new Size(99, 19);
        Convert_DeflateStreamRadioButton.TabIndex = 4;
        Convert_DeflateStreamRadioButton.Text = "DeflateStream";
        Convert_DeflateStreamRadioButton.UseVisualStyleBackColor = true;
        // 
        // Convert_FileStreamRadioButton
        // 
        Convert_FileStreamRadioButton.AutoSize = true;
        Convert_FileStreamRadioButton.Location = new Point(224, 24);
        Convert_FileStreamRadioButton.Name = "Convert_FileStreamRadioButton";
        Convert_FileStreamRadioButton.Size = new Size(80, 19);
        Convert_FileStreamRadioButton.TabIndex = 2;
        Convert_FileStreamRadioButton.Text = "FileStream";
        Convert_FileStreamRadioButton.UseVisualStyleBackColor = true;
        // 
        // Convert_MemoryStreamRadioButton
        // 
        Convert_MemoryStreamRadioButton.AutoSize = true;
        Convert_MemoryStreamRadioButton.Location = new Point(104, 24);
        Convert_MemoryStreamRadioButton.Name = "Convert_MemoryStreamRadioButton";
        Convert_MemoryStreamRadioButton.Size = new Size(107, 19);
        Convert_MemoryStreamRadioButton.TabIndex = 1;
        Convert_MemoryStreamRadioButton.Text = "MemoryStream";
        Convert_MemoryStreamRadioButton.UseVisualStyleBackColor = true;
        // 
        // Convert_ByteArrayRadioButton
        // 
        Convert_ByteArrayRadioButton.AutoSize = true;
        Convert_ByteArrayRadioButton.Checked = true;
        Convert_ByteArrayRadioButton.Location = new Point(16, 24);
        Convert_ByteArrayRadioButton.Name = "Convert_ByteArrayRadioButton";
        Convert_ByteArrayRadioButton.Size = new Size(77, 19);
        Convert_ByteArrayRadioButton.TabIndex = 0;
        Convert_ByteArrayRadioButton.TabStop = true;
        Convert_ByteArrayRadioButton.Text = "Byte array";
        Convert_ByteArrayRadioButton.UseVisualStyleBackColor = true;
        // 
        // WriteBenchmarkFilesGroupBox
        // 
        WriteBenchmarkFilesGroupBox.Controls.Add(AllTargetsButton);
        WriteBenchmarkFilesGroupBox.Controls.Add(Net48_32Button);
        WriteBenchmarkFilesGroupBox.Controls.Add(Net48_64Button);
        WriteBenchmarkFilesGroupBox.Controls.Add(Net64Button);
        WriteBenchmarkFilesGroupBox.Location = new Point(16, 296);
        WriteBenchmarkFilesGroupBox.Name = "WriteBenchmarkFilesGroupBox";
        WriteBenchmarkFilesGroupBox.Size = new Size(592, 72);
        WriteBenchmarkFilesGroupBox.TabIndex = 9;
        WriteBenchmarkFilesGroupBox.TabStop = false;
        WriteBenchmarkFilesGroupBox.Text = "Write benchmark files";
        // 
        // AllTargetsButton
        // 
        AllTargetsButton.Location = new Point(256, 32);
        AllTargetsButton.Name = "AllTargetsButton";
        AllTargetsButton.Size = new Size(75, 23);
        AllTargetsButton.TabIndex = 0;
        AllTargetsButton.Text = "All";
        AllTargetsButton.UseVisualStyleBackColor = true;
        AllTargetsButton.Click += AllTargetsButton_Click;
        // 
        // Net48_32Button
        // 
        Net48_32Button.Location = new Point(176, 32);
        Net48_32Button.Name = "Net48_32Button";
        Net48_32Button.Size = new Size(75, 23);
        Net48_32Button.TabIndex = 0;
        Net48_32Button.Text = "net48 32";
        Net48_32Button.UseVisualStyleBackColor = true;
        Net48_32Button.Click += Net48_32Button_Click;
        // 
        // Net48_64Button
        // 
        Net48_64Button.Location = new Point(96, 32);
        Net48_64Button.Name = "Net48_64Button";
        Net48_64Button.Size = new Size(75, 23);
        Net48_64Button.TabIndex = 0;
        Net48_64Button.Text = "net48 64";
        Net48_64Button.UseVisualStyleBackColor = true;
        Net48_64Button.Click += Net48_64Button_Click;
        // 
        // Net64Button
        // 
        Net64Button.Location = new Point(16, 32);
        Net64Button.Name = "Net64Button";
        Net64Button.Size = new Size(75, 23);
        Net64Button.TabIndex = 0;
        Net64Button.Text = "net 64";
        Net64Button.UseVisualStyleBackColor = true;
        Net64Button.Click += Net64Button_Click;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(625, 384);
        Controls.Add(WriteBenchmarkFilesGroupBox);
        Controls.Add(ConvertSourceGroupBox);
        Controls.Add(ValidityTestGroupBox);
        Controls.Add(WriteOneButton);
        Controls.Add(ConvertOneButton);
        Controls.Add(ConverOnly_Small_GroupBox);
        Controls.Add(ConvertOnly_Full_GroupBox);
        Controls.Add(WriteWorkingNewSetGroupBox);
        Controls.Add(ConvertAndWriteToDiskGroupBox);
        Controls.Add(Test1Button);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "MainForm";
        Text = "ReasonableRTF Test App";
        Shown += MainForm_Shown;
        ConvertAndWriteToDiskGroupBox.ResumeLayout(false);
        ConvertOnly_Full_GroupBox.ResumeLayout(false);
        ConverOnly_Small_GroupBox.ResumeLayout(false);
        ValidityTestGroupBox.ResumeLayout(false);
        WriteWorkingNewSetGroupBox.ResumeLayout(false);
        ConvertSourceGroupBox.ResumeLayout(false);
        ConvertSourceGroupBox.PerformLayout();
        WriteBenchmarkFilesGroupBox.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private Button Test1Button;
    private GroupBox ConvertAndWriteToDiskGroupBox;
    private Button ConvertAndWriteWithRichTextBoxButton;
    private Button ConvertAndWriteWithCustomButton;
    private GroupBox ConvertOnly_Full_GroupBox;
    private Button ConvertOnlyWithRichTextBoxButton;
    private Button ConvertOnlyWithCustom20XButton;
    private Button ConvertOnlyWithCustomButton;
    private GroupBox ConverOnly_Small_GroupBox;
    private Button ConvertOnlyWithRichTextBox_Small_Button;
    private Button ConvertOnlyWithCustom20X_Small_Button;
    private Button ConvertOnlyWithCustom_Small_Button;
    private Button ConvertOneButton;
    private Button WriteOneButton;
    private GroupBox ValidityTestGroupBox;
    private Button ConvertAndWriteValidityTestFiles_Custom_Button;
    private Button ConvertAndWriteValidityTestFiles_RTB_Button;
    private GroupBox WriteWorkingNewSetGroupBox;
    private Button WriteWorkingNewSetRTBButton;
    private Button WriteWorkingNewSetCustomButton;
    private GroupBox ConvertSourceGroupBox;
    private RadioButton Convert_FileStreamRadioButton;
    private RadioButton Convert_MemoryStreamRadioButton;
    private RadioButton Convert_ByteArrayRadioButton;
    private RadioButton Convert_DeflateStreamRadioButton;
    private GroupBox WriteBenchmarkFilesGroupBox;
    private Button Net64Button;
    private Button AllTargetsButton;
    private Button Net48_32Button;
    private Button Net48_64Button;
}
