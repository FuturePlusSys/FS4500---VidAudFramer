namespace FrameVideoRendererClassLibrary
{
    partial class FrameVideoRendererCtrl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lane4button = new System.Windows.Forms.RadioButton();
            this.Lane2button = new System.Windows.Forms.RadioButton();
            this.Lane1button = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.DP1_4MSTbutton = new System.Windows.Forms.RadioButton();
            this.DP1_4SSTbutton = new System.Windows.Forms.RadioButton();
            this.DP1_2MSTbutton = new System.Windows.Forms.RadioButton();
            this.DP1_2SSTbutton = new System.Windows.Forms.RadioButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.VC4button = new System.Windows.Forms.RadioButton();
            this.VChannelLabel = new System.Windows.Forms.Label();
            this.VC3button = new System.Windows.Forms.RadioButton();
            this.VC2button = new System.Windows.Forms.RadioButton();
            this.VC1button = new System.Windows.Forms.RadioButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.DataFolderPath_TextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Browsebutton = new System.Windows.Forms.Button();
            this.GetFramesbutton = new System.Windows.Forms.Button();
            this.FramesPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.getstatesButton = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.CancelButton = new System.Windows.Forms.Button();
            this.ComparePicturesButton = new System.Windows.Forms.Button();
            this.DeleteButton = new System.Windows.Forms.Button();
            this.TestChecked = new System.Windows.Forms.Button();
            this.ClearButton = new System.Windows.Forms.Button();
            this.LoadReferenceButton = new System.Windows.Forms.Button();
            this.ComparePanel = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lane4button);
            this.groupBox3.Controls.Add(this.Lane2button);
            this.groupBox3.Controls.Add(this.Lane1button);
            this.groupBox3.Location = new System.Drawing.Point(97, 14);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(77, 118);
            this.groupBox3.TabIndex = 76;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Link Width";
            // 
            // lane4button
            // 
            this.lane4button.AutoSize = true;
            this.lane4button.Location = new System.Drawing.Point(7, 68);
            this.lane4button.Name = "lane4button";
            this.lane4button.Size = new System.Drawing.Size(58, 17);
            this.lane4button.TabIndex = 2;
            this.lane4button.TabStop = true;
            this.lane4button.Text = "4 Lane";
            this.lane4button.UseVisualStyleBackColor = true;
            // 
            // Lane2button
            // 
            this.Lane2button.AutoSize = true;
            this.Lane2button.Location = new System.Drawing.Point(7, 44);
            this.Lane2button.Name = "Lane2button";
            this.Lane2button.Size = new System.Drawing.Size(58, 17);
            this.Lane2button.TabIndex = 1;
            this.Lane2button.TabStop = true;
            this.Lane2button.Text = "2 Lane";
            this.Lane2button.UseVisualStyleBackColor = true;
            // 
            // Lane1button
            // 
            this.Lane1button.AutoSize = true;
            this.Lane1button.Location = new System.Drawing.Point(7, 20);
            this.Lane1button.Name = "Lane1button";
            this.Lane1button.Size = new System.Drawing.Size(58, 17);
            this.Lane1button.TabIndex = 0;
            this.Lane1button.TabStop = true;
            this.Lane1button.Text = "1 Lane";
            this.Lane1button.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.DP1_4MSTbutton);
            this.groupBox1.Controls.Add(this.DP1_4SSTbutton);
            this.groupBox1.Controls.Add(this.DP1_2MSTbutton);
            this.groupBox1.Controls.Add(this.DP1_2SSTbutton);
            this.groupBox1.Location = new System.Drawing.Point(17, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(77, 120);
            this.groupBox1.TabIndex = 75;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Protocol";
            // 
            // DP1_4MSTbutton
            // 
            this.DP1_4MSTbutton.AutoSize = true;
            this.DP1_4MSTbutton.Location = new System.Drawing.Point(6, 88);
            this.DP1_4MSTbutton.Name = "DP1_4MSTbutton";
            this.DP1_4MSTbutton.Size = new System.Drawing.Size(63, 17);
            this.DP1_4MSTbutton.TabIndex = 4;
            this.DP1_4MSTbutton.TabStop = true;
            this.DP1_4MSTbutton.Text = "1.4MST";
            this.DP1_4MSTbutton.UseVisualStyleBackColor = true;
            this.DP1_4MSTbutton.CheckedChanged += new System.EventHandler(this.DP1_3MSTbutton_CheckedChanged);
            // 
            // DP1_4SSTbutton
            // 
            this.DP1_4SSTbutton.AutoSize = true;
            this.DP1_4SSTbutton.Location = new System.Drawing.Point(6, 65);
            this.DP1_4SSTbutton.Name = "DP1_4SSTbutton";
            this.DP1_4SSTbutton.Size = new System.Drawing.Size(61, 17);
            this.DP1_4SSTbutton.TabIndex = 3;
            this.DP1_4SSTbutton.TabStop = true;
            this.DP1_4SSTbutton.Text = "1.4SST";
            this.DP1_4SSTbutton.UseVisualStyleBackColor = true;
            this.DP1_4SSTbutton.CheckedChanged += new System.EventHandler(this.DP1_3SSTbutton_CheckedChanged);
            // 
            // DP1_2MSTbutton
            // 
            this.DP1_2MSTbutton.AutoSize = true;
            this.DP1_2MSTbutton.Location = new System.Drawing.Point(6, 42);
            this.DP1_2MSTbutton.Name = "DP1_2MSTbutton";
            this.DP1_2MSTbutton.Size = new System.Drawing.Size(63, 17);
            this.DP1_2MSTbutton.TabIndex = 2;
            this.DP1_2MSTbutton.TabStop = true;
            this.DP1_2MSTbutton.Text = "1.2MST";
            this.DP1_2MSTbutton.UseVisualStyleBackColor = true;
            this.DP1_2MSTbutton.CheckedChanged += new System.EventHandler(this.DP1_2MSTbutton_CheckedChanged);
            // 
            // DP1_2SSTbutton
            // 
            this.DP1_2SSTbutton.AutoSize = true;
            this.DP1_2SSTbutton.Location = new System.Drawing.Point(6, 18);
            this.DP1_2SSTbutton.Name = "DP1_2SSTbutton";
            this.DP1_2SSTbutton.Size = new System.Drawing.Size(61, 17);
            this.DP1_2SSTbutton.TabIndex = 1;
            this.DP1_2SSTbutton.TabStop = true;
            this.DP1_2SSTbutton.Text = "1.2SST";
            this.DP1_2SSTbutton.UseVisualStyleBackColor = true;
            this.DP1_2SSTbutton.CheckedChanged += new System.EventHandler(this.DP1_2SSTbutton_CheckedChanged);
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.VC4button);
            this.panel2.Controls.Add(this.VChannelLabel);
            this.panel2.Controls.Add(this.VC3button);
            this.panel2.Controls.Add(this.VC2button);
            this.panel2.Controls.Add(this.VC1button);
            this.panel2.Location = new System.Drawing.Point(180, 12);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(95, 119);
            this.panel2.TabIndex = 71;
            // 
            // VC4button
            // 
            this.VC4button.AutoSize = true;
            this.VC4button.Location = new System.Drawing.Point(3, 98);
            this.VC4button.Name = "VC4button";
            this.VC4button.Size = new System.Drawing.Size(48, 17);
            this.VC4button.TabIndex = 8;
            this.VC4button.TabStop = true;
            this.VC4button.Text = "VC 4";
            this.VC4button.UseVisualStyleBackColor = true;
            // 
            // VChannelLabel
            // 
            this.VChannelLabel.AutoSize = true;
            this.VChannelLabel.Location = new System.Drawing.Point(6, 9);
            this.VChannelLabel.Name = "VChannelLabel";
            this.VChannelLabel.Size = new System.Drawing.Size(83, 13);
            this.VChannelLabel.TabIndex = 11;
            this.VChannelLabel.Text = "Virtual Channels";
            // 
            // VC3button
            // 
            this.VC3button.AutoSize = true;
            this.VC3button.Location = new System.Drawing.Point(3, 75);
            this.VC3button.Name = "VC3button";
            this.VC3button.Size = new System.Drawing.Size(48, 17);
            this.VC3button.TabIndex = 7;
            this.VC3button.TabStop = true;
            this.VC3button.Text = "VC 3";
            this.VC3button.UseVisualStyleBackColor = true;
            // 
            // VC2button
            // 
            this.VC2button.AutoSize = true;
            this.VC2button.Location = new System.Drawing.Point(3, 51);
            this.VC2button.Name = "VC2button";
            this.VC2button.Size = new System.Drawing.Size(48, 17);
            this.VC2button.TabIndex = 6;
            this.VC2button.TabStop = true;
            this.VC2button.Text = "VC 2";
            this.VC2button.UseVisualStyleBackColor = true;
            // 
            // VC1button
            // 
            this.VC1button.AutoSize = true;
            this.VC1button.Location = new System.Drawing.Point(3, 27);
            this.VC1button.Name = "VC1button";
            this.VC1button.Size = new System.Drawing.Size(48, 17);
            this.VC1button.TabIndex = 5;
            this.VC1button.TabStop = true;
            this.VC1button.Text = "VC 1";
            this.VC1button.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripProgressBar1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 592);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1144, 22);
            this.statusStrip1.TabIndex = 85;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(39, 17);
            this.toolStripStatusLabel1.Text = "Ready";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(200, 16);
            // 
            // DataFolderPath_TextBox
            // 
            this.DataFolderPath_TextBox.Location = new System.Drawing.Point(281, 83);
            this.DataFolderPath_TextBox.Name = "DataFolderPath_TextBox";
            this.DataFolderPath_TextBox.Size = new System.Drawing.Size(180, 20);
            this.DataFolderPath_TextBox.TabIndex = 91;
            this.DataFolderPath_TextBox.Text = "C:\\\\";
            this.DataFolderPath_TextBox.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(278, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 92;
            this.label2.Text = "Folder Path";
            this.label2.Visible = false;
            // 
            // Browsebutton
            // 
            this.Browsebutton.Location = new System.Drawing.Point(379, 105);
            this.Browsebutton.Name = "Browsebutton";
            this.Browsebutton.Size = new System.Drawing.Size(82, 23);
            this.Browsebutton.TabIndex = 93;
            this.Browsebutton.Text = "Browse";
            this.Browsebutton.UseVisualStyleBackColor = true;
            this.Browsebutton.Visible = false;
            this.Browsebutton.Click += new System.EventHandler(this.Browsebutton_Click);
            // 
            // GetFramesbutton
            // 
            this.GetFramesbutton.Location = new System.Drawing.Point(86, 490);
            this.GetFramesbutton.Name = "GetFramesbutton";
            this.GetFramesbutton.Size = new System.Drawing.Size(76, 23);
            this.GetFramesbutton.TabIndex = 94;
            this.GetFramesbutton.Text = "Get Frames";
            this.GetFramesbutton.UseVisualStyleBackColor = true;
            this.GetFramesbutton.Click += new System.EventHandler(this.GetFramesbutton_Click);
            // 
            // FramesPanel
            // 
            this.FramesPanel.AutoScroll = true;
            this.FramesPanel.BackColor = System.Drawing.SystemColors.Control;
            this.FramesPanel.Location = new System.Drawing.Point(17, 138);
            this.FramesPanel.Name = "FramesPanel";
            this.FramesPanel.Size = new System.Drawing.Size(145, 346);
            this.FramesPanel.TabIndex = 95;
            // 
            // getstatesButton
            // 
            this.getstatesButton.Location = new System.Drawing.Point(514, 490);
            this.getstatesButton.Name = "getstatesButton";
            this.getstatesButton.Size = new System.Drawing.Size(96, 23);
            this.getstatesButton.TabIndex = 96;
            this.getstatesButton.Text = "Get Start States";
            this.getstatesButton.UseVisualStyleBackColor = true;
            this.getstatesButton.Click += new System.EventHandler(this.getstatesButton_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(168, 138);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(544, 346);
            this.richTextBox1.TabIndex = 97;
            this.richTextBox1.Text = "";
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(17, 490);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(69, 23);
            this.CancelButton.TabIndex = 98;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // ComparePicturesButton
            // 
            this.ComparePicturesButton.Location = new System.Drawing.Point(828, 490);
            this.ComparePicturesButton.Name = "ComparePicturesButton";
            this.ComparePicturesButton.Size = new System.Drawing.Size(104, 23);
            this.ComparePicturesButton.TabIndex = 100;
            this.ComparePicturesButton.Text = "Compare Pictures";
            this.ComparePicturesButton.UseVisualStyleBackColor = true;
            this.ComparePicturesButton.Click += new System.EventHandler(this.ComparePicturesButton_Click);
            // 
            // DeleteButton
            // 
            this.DeleteButton.Location = new System.Drawing.Point(718, 490);
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Size = new System.Drawing.Size(104, 23);
            this.DeleteButton.TabIndex = 101;
            this.DeleteButton.Text = "Delete Checked Pics";
            this.DeleteButton.UseVisualStyleBackColor = true;
            this.DeleteButton.Visible = false;
            this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            // 
            // TestChecked
            // 
            this.TestChecked.Location = new System.Drawing.Point(737, 556);
            this.TestChecked.Name = "TestChecked";
            this.TestChecked.Size = new System.Drawing.Size(104, 23);
            this.TestChecked.TabIndex = 102;
            this.TestChecked.Text = "Test Checked Pics";
            this.TestChecked.UseVisualStyleBackColor = true;
            this.TestChecked.Visible = false;
            this.TestChecked.Click += new System.EventHandler(this.TestChecked_Click);
            // 
            // ClearButton
            // 
            this.ClearButton.Location = new System.Drawing.Point(616, 490);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new System.Drawing.Size(96, 23);
            this.ClearButton.TabIndex = 103;
            this.ClearButton.Text = "Clear Text";
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // LoadReferenceButton
            // 
            this.LoadReferenceButton.Location = new System.Drawing.Point(828, 111);
            this.LoadReferenceButton.Name = "LoadReferenceButton";
            this.LoadReferenceButton.Size = new System.Drawing.Size(104, 23);
            this.LoadReferenceButton.TabIndex = 104;
            this.LoadReferenceButton.Text = "Load Reference";
            this.LoadReferenceButton.UseVisualStyleBackColor = true;
            this.LoadReferenceButton.Visible = false;
            this.LoadReferenceButton.Click += new System.EventHandler(this.LoadReferenceButton_Click);
            // 
            // ComparePanel
            // 
            this.ComparePanel.BackColor = System.Drawing.SystemColors.Control;
            this.ComparePanel.Location = new System.Drawing.Point(718, 140);
            this.ComparePanel.Name = "ComparePanel";
            this.ComparePanel.Size = new System.Drawing.Size(214, 344);
            this.ComparePanel.TabIndex = 105;
            // 
            // FrameVideoRendererCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.Controls.Add(this.ComparePanel);
            this.Controls.Add(this.LoadReferenceButton);
            this.Controls.Add(this.ClearButton);
            this.Controls.Add(this.TestChecked);
            this.Controls.Add(this.DeleteButton);
            this.Controls.Add(this.ComparePicturesButton);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.getstatesButton);
            this.Controls.Add(this.FramesPanel);
            this.Controls.Add(this.GetFramesbutton);
            this.Controls.Add(this.Browsebutton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.DataFolderPath_TextBox);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel2);
            this.Name = "FrameVideoRendererCtrl";
            this.Size = new System.Drawing.Size(1144, 614);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton lane4button;
        private System.Windows.Forms.RadioButton Lane2button;
        private System.Windows.Forms.RadioButton Lane1button;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton DP1_4MSTbutton;
        private System.Windows.Forms.RadioButton DP1_4SSTbutton;
        private System.Windows.Forms.RadioButton DP1_2MSTbutton;
        private System.Windows.Forms.RadioButton DP1_2SSTbutton;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RadioButton VC4button;
        private System.Windows.Forms.Label VChannelLabel;
        private System.Windows.Forms.RadioButton VC3button;
        private System.Windows.Forms.RadioButton VC2button;
        private System.Windows.Forms.RadioButton VC1button;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.TextBox DataFolderPath_TextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button Browsebutton;
        private System.Windows.Forms.Button GetFramesbutton;
        private System.Windows.Forms.FlowLayoutPanel FramesPanel;
        private System.Windows.Forms.Button getstatesButton;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.Button ComparePicturesButton;
        private System.Windows.Forms.Button DeleteButton;
        private System.Windows.Forms.Button TestChecked;
        private System.Windows.Forms.Button ClearButton;
        private System.Windows.Forms.Button LoadReferenceButton;
        private System.Windows.Forms.FlowLayoutPanel ComparePanel;
    }
}
