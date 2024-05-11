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
        DataDirTextBox = new TextBox();
        DataDirBrowseButton = new Button();
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
        DataDirResetButton = new Button();
        DataDirGroupBox = new GroupBox();
        ValidityTestGroupBox = new GroupBox();
        ConvertAndWriteValidityTestFiles_Custom_Button = new Button();
        ConvertAndWriteValidityTestFiles_RTB_Button = new Button();
        WriteWorkingNewSetGroupBox = new GroupBox();
        WriteWorkingNewSetRTBButton = new Button();
        WriteWorkingNewSetCustomButton = new Button();
        ConvertAndWriteToDiskGroupBox.SuspendLayout();
        ConvertOnly_Full_GroupBox.SuspendLayout();
        ConverOnly_Small_GroupBox.SuspendLayout();
        DataDirGroupBox.SuspendLayout();
        ValidityTestGroupBox.SuspendLayout();
        WriteWorkingNewSetGroupBox.SuspendLayout();
        SuspendLayout();
        // 
        // Test1Button
        // 
        Test1Button.Location = new Point(504, 96);
        Test1Button.Name = "Test1Button";
        Test1Button.Size = new Size(91, 27);
        Test1Button.TabIndex = 7;
        Test1Button.Text = "Test";
        Test1Button.UseVisualStyleBackColor = true;
        Test1Button.Click += Test1Button_Click;
        // 
        // DataDirTextBox
        // 
        DataDirTextBox.Location = new Point(14, 25);
        DataDirTextBox.Name = "DataDirTextBox";
        DataDirTextBox.Size = new Size(392, 23);
        DataDirTextBox.TabIndex = 0;
        // 
        // DataDirBrowseButton
        // 
        DataDirBrowseButton.Location = new Point(406, 24);
        DataDirBrowseButton.Name = "DataDirBrowseButton";
        DataDirBrowseButton.Size = new Size(75, 25);
        DataDirBrowseButton.TabIndex = 1;
        DataDirBrowseButton.Text = "Browse...";
        DataDirBrowseButton.UseVisualStyleBackColor = true;
        DataDirBrowseButton.Click += DataDirBrowseButton_Click;
        // 
        // ConvertAndWriteToDiskGroupBox
        // 
        ConvertAndWriteToDiskGroupBox.Controls.Add(ConvertAndWriteWithRichTextBoxButton);
        ConvertAndWriteToDiskGroupBox.Controls.Add(ConvertAndWriteWithCustomButton);
        ConvertAndWriteToDiskGroupBox.Location = new Point(16, 96);
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
        ConvertAndWriteWithCustomButton.Text = "Custom";
        ConvertAndWriteWithCustomButton.UseVisualStyleBackColor = true;
        ConvertAndWriteWithCustomButton.Click += ConvertAndWriteWithCustomButton_Click;
        // 
        // ConvertOnly_Full_GroupBox
        // 
        ConvertOnly_Full_GroupBox.Controls.Add(ConvertOnlyWithRichTextBoxButton);
        ConvertOnly_Full_GroupBox.Controls.Add(ConvertOnlyWithCustom20XButton);
        ConvertOnly_Full_GroupBox.Controls.Add(ConvertOnlyWithCustomButton);
        ConvertOnly_Full_GroupBox.Location = new Point(16, 168);
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
        ConvertOnlyWithCustomButton.Text = "Custom";
        ConvertOnlyWithCustomButton.UseVisualStyleBackColor = true;
        ConvertOnlyWithCustomButton.Click += ConvertOnlyWithCustomButton_Click;
        // 
        // ConverOnly_Small_GroupBox
        // 
        ConverOnly_Small_GroupBox.Controls.Add(ConvertOnlyWithRichTextBox_Small_Button);
        ConverOnly_Small_GroupBox.Controls.Add(ConvertOnlyWithCustom20X_Small_Button);
        ConverOnly_Small_GroupBox.Controls.Add(ConvertOnlyWithCustom_Small_Button);
        ConverOnly_Small_GroupBox.Location = new Point(16, 240);
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
        ConvertOnlyWithCustom_Small_Button.Text = "Custom";
        ConvertOnlyWithCustom_Small_Button.UseVisualStyleBackColor = true;
        ConvertOnlyWithCustom_Small_Button.Click += ConvertOnlyWithCustom_Small_Button_Click;
        // 
        // ConvertOneButton
        // 
        ConvertOneButton.Location = new Point(392, 96);
        ConvertOneButton.Name = "ConvertOneButton";
        ConvertOneButton.Size = new Size(91, 27);
        ConvertOneButton.TabIndex = 5;
        ConvertOneButton.Text = "Convert one";
        ConvertOneButton.UseVisualStyleBackColor = true;
        ConvertOneButton.Click += ConvertOneButton_Click;
        // 
        // WriteOneButton
        // 
        WriteOneButton.Location = new Point(392, 128);
        WriteOneButton.Name = "WriteOneButton";
        WriteOneButton.Size = new Size(91, 27);
        WriteOneButton.TabIndex = 6;
        WriteOneButton.Text = "Write one";
        WriteOneButton.UseVisualStyleBackColor = true;
        WriteOneButton.Click += WriteOneButton_Click;
        // 
        // DataDirResetButton
        // 
        DataDirResetButton.Location = new Point(480, 24);
        DataDirResetButton.Name = "DataDirResetButton";
        DataDirResetButton.Size = new Size(99, 25);
        DataDirResetButton.TabIndex = 2;
        DataDirResetButton.Text = "Reset to default";
        DataDirResetButton.UseVisualStyleBackColor = true;
        DataDirResetButton.Click += DataDirResetButton_Click;
        // 
        // DataDirGroupBox
        // 
        DataDirGroupBox.Controls.Add(DataDirResetButton);
        DataDirGroupBox.Controls.Add(DataDirTextBox);
        DataDirGroupBox.Controls.Add(DataDirBrowseButton);
        DataDirGroupBox.Location = new Point(16, 16);
        DataDirGroupBox.Name = "DataDirGroupBox";
        DataDirGroupBox.Size = new Size(592, 64);
        DataDirGroupBox.TabIndex = 0;
        DataDirGroupBox.TabStop = false;
        DataDirGroupBox.Text = "Test data base directory";
        // 
        // ValidityTestGroupBox
        // 
        ValidityTestGroupBox.Controls.Add(ConvertAndWriteValidityTestFiles_Custom_Button);
        ValidityTestGroupBox.Controls.Add(ConvertAndWriteValidityTestFiles_RTB_Button);
        ValidityTestGroupBox.Location = new Point(376, 168);
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
        ConvertAndWriteValidityTestFiles_Custom_Button.Text = "Custom";
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
        WriteWorkingNewSetGroupBox.Location = new Point(376, 240);
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
        WriteWorkingNewSetCustomButton.Text = "Custom";
        WriteWorkingNewSetCustomButton.UseVisualStyleBackColor = true;
        WriteWorkingNewSetCustomButton.Click += WriteWorkingNewSetCustomButton_Click;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(625, 398);
        Controls.Add(ValidityTestGroupBox);
        Controls.Add(DataDirGroupBox);
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
        FormClosing += MainForm_FormClosing;
        Shown += MainForm_Shown;
        ConvertAndWriteToDiskGroupBox.ResumeLayout(false);
        ConvertOnly_Full_GroupBox.ResumeLayout(false);
        ConverOnly_Small_GroupBox.ResumeLayout(false);
        DataDirGroupBox.ResumeLayout(false);
        DataDirGroupBox.PerformLayout();
        ValidityTestGroupBox.ResumeLayout(false);
        WriteWorkingNewSetGroupBox.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private Button Test1Button;
    private TextBox DataDirTextBox;
    private Button DataDirBrowseButton;
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
    private Button DataDirResetButton;
    private GroupBox DataDirGroupBox;
    private GroupBox ValidityTestGroupBox;
    private Button ConvertAndWriteValidityTestFiles_Custom_Button;
    private Button ConvertAndWriteValidityTestFiles_RTB_Button;
    private GroupBox WriteWorkingNewSetGroupBox;
    private Button WriteWorkingNewSetRTBButton;
    private Button WriteWorkingNewSetCustomButton;
}
