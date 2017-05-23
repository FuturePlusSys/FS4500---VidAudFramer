namespace PixelRenderer
{
    partial class AudioRendererCtrl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AudioRendererCtrl));
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.audioSDPtext = new System.Windows.Forms.TextBox();
            this.maudnaudbutton = new System.Windows.Forms.Button();
            this.maudmaxtext = new System.Windows.Forms.TextBox();
            this.maudmintext = new System.Windows.Forms.TextBox();
            this.maudaveragetext = new System.Windows.Forms.TextBox();
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
            this.getfilebutton = new System.Windows.Forms.Button();
            this.axWindowsMediaPlayer1 = new AxWMPLib.AxWindowsMediaPlayer();
            this.maudnaudchart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.audioVSDPtext = new System.Windows.Forms.TextBox();
            this.createbutton = new System.Windows.Forms.Button();
            this.StateIndexLabel = new System.Windows.Forms.Label();
            this.StartStateNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.EndStateLabel = new System.Windows.Forms.Label();
            this.EndStatenumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.panel3 = new System.Windows.Forms.Panel();
            this.channel8checkbox = new System.Windows.Forms.CheckBox();
            this.channel7checkbox = new System.Windows.Forms.CheckBox();
            this.channel6checkbox = new System.Windows.Forms.CheckBox();
            this.channel5checkbox = new System.Windows.Forms.CheckBox();
            this.channel4checkbox = new System.Windows.Forms.CheckBox();
            this.channel3checkbox = new System.Windows.Forms.CheckBox();
            this.channel2checkbox = new System.Windows.Forms.CheckBox();
            this.channel1checkbox = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.sampleratetext = new System.Windows.Forms.TextBox();
            this.bitspersampletext = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.naudmaxtext = new System.Windows.Forms.TextBox();
            this.naudmintext = new System.Windows.Forms.TextBox();
            this.naudaveragetext = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.Maud = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axWindowsMediaPlayer1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maudnaudchart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.StartStateNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.EndStatenumericUpDown)).BeginInit();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // audioSDPtext
            // 
            this.audioSDPtext.Location = new System.Drawing.Point(710, 298);
            this.audioSDPtext.Name = "audioSDPtext";
            this.audioSDPtext.Size = new System.Drawing.Size(100, 20);
            this.audioSDPtext.TabIndex = 1;
            // 
            // maudnaudbutton
            // 
            this.maudnaudbutton.Location = new System.Drawing.Point(684, 153);
            this.maudnaudbutton.Name = "maudnaudbutton";
            this.maudnaudbutton.Size = new System.Drawing.Size(126, 23);
            this.maudnaudbutton.TabIndex = 2;
            this.maudnaudbutton.Text = "Get Maud and Naud Values";
            this.maudnaudbutton.UseVisualStyleBackColor = true;
            this.maudnaudbutton.Click += new System.EventHandler(this.maudnaudbutton_Click);
            // 
            // maudmaxtext
            // 
            this.maudmaxtext.Location = new System.Drawing.Point(684, 202);
            this.maudmaxtext.Name = "maudmaxtext";
            this.maudmaxtext.Size = new System.Drawing.Size(60, 20);
            this.maudmaxtext.TabIndex = 3;
            // 
            // maudmintext
            // 
            this.maudmintext.Location = new System.Drawing.Point(684, 228);
            this.maudmintext.Name = "maudmintext";
            this.maudmintext.Size = new System.Drawing.Size(60, 20);
            this.maudmintext.TabIndex = 4;
            // 
            // maudaveragetext
            // 
            this.maudaveragetext.Location = new System.Drawing.Point(684, 254);
            this.maudaveragetext.Name = "maudaveragetext";
            this.maudaveragetext.Size = new System.Drawing.Size(60, 20);
            this.maudaveragetext.TabIndex = 5;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lane4button);
            this.groupBox3.Controls.Add(this.Lane2button);
            this.groupBox3.Controls.Add(this.Lane1button);
            this.groupBox3.Location = new System.Drawing.Point(96, 20);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(77, 118);
            this.groupBox3.TabIndex = 55;
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
            this.groupBox1.Location = new System.Drawing.Point(16, 18);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(77, 120);
            this.groupBox1.TabIndex = 54;
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
            this.panel2.Location = new System.Drawing.Point(179, 20);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(95, 119);
            this.panel2.TabIndex = 56;
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
            // getfilebutton
            // 
            this.getfilebutton.Location = new System.Drawing.Point(323, 463);
            this.getfilebutton.Name = "getfilebutton";
            this.getfilebutton.Size = new System.Drawing.Size(82, 23);
            this.getfilebutton.TabIndex = 57;
            this.getfilebutton.Text = "Get Wav File";
            this.getfilebutton.UseVisualStyleBackColor = true;
            this.getfilebutton.Click += new System.EventHandler(this.getfilebutton_Click);
            // 
            // axWindowsMediaPlayer1
            // 
            this.axWindowsMediaPlayer1.Enabled = true;
            this.axWindowsMediaPlayer1.Location = new System.Drawing.Point(22, 492);
            this.axWindowsMediaPlayer1.Name = "axWindowsMediaPlayer1";
            this.axWindowsMediaPlayer1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axWindowsMediaPlayer1.OcxState")));
            this.axWindowsMediaPlayer1.Size = new System.Drawing.Size(383, 45);
            this.axWindowsMediaPlayer1.TabIndex = 64;
            // 
            // maudnaudchart
            // 
            chartArea1.Name = "ChartArea1";
            this.maudnaudchart.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.maudnaudchart.Legends.Add(legend1);
            this.maudnaudchart.Location = new System.Drawing.Point(16, 152);
            this.maudnaudchart.Name = "maudnaudchart";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Value";
            this.maudnaudchart.Series.Add(series1);
            this.maudnaudchart.Size = new System.Drawing.Size(613, 292);
            this.maudnaudchart.TabIndex = 65;
            this.maudnaudchart.Text = "chart1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(713, 282);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 66;
            this.label1.Text = "# of SDPs";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(713, 329);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 13);
            this.label2.TabIndex = 68;
            this.label2.Text = "# of Vertical SDPs";
            // 
            // audioVSDPtext
            // 
            this.audioVSDPtext.Location = new System.Drawing.Point(710, 345);
            this.audioVSDPtext.Name = "audioVSDPtext";
            this.audioVSDPtext.Size = new System.Drawing.Size(100, 20);
            this.audioVSDPtext.TabIndex = 67;
            // 
            // createbutton
            // 
            this.createbutton.Location = new System.Drawing.Point(235, 463);
            this.createbutton.Name = "createbutton";
            this.createbutton.Size = new System.Drawing.Size(82, 23);
            this.createbutton.TabIndex = 69;
            this.createbutton.Text = "Create File";
            this.createbutton.UseVisualStyleBackColor = true;
            this.createbutton.Click += new System.EventHandler(this.createbutton_Click);
            // 
            // StateIndexLabel
            // 
            this.StateIndexLabel.AutoSize = true;
            this.StateIndexLabel.Location = new System.Drawing.Point(19, 450);
            this.StateIndexLabel.Name = "StateIndexLabel";
            this.StateIndexLabel.Size = new System.Drawing.Size(57, 13);
            this.StateIndexLabel.TabIndex = 71;
            this.StateIndexLabel.Text = "Start State";
            // 
            // StartStateNumericUpDown
            // 
            this.StartStateNumericUpDown.Location = new System.Drawing.Point(22, 466);
            this.StartStateNumericUpDown.Maximum = new decimal(new int[] {
            1215752192,
            23,
            0,
            0});
            this.StartStateNumericUpDown.Name = "StartStateNumericUpDown";
            this.StartStateNumericUpDown.Size = new System.Drawing.Size(96, 20);
            this.StartStateNumericUpDown.TabIndex = 70;
            // 
            // EndStateLabel
            // 
            this.EndStateLabel.AutoSize = true;
            this.EndStateLabel.Location = new System.Drawing.Point(121, 450);
            this.EndStateLabel.Name = "EndStateLabel";
            this.EndStateLabel.Size = new System.Drawing.Size(54, 13);
            this.EndStateLabel.TabIndex = 73;
            this.EndStateLabel.Text = "End State";
            // 
            // EndStatenumericUpDown
            // 
            this.EndStatenumericUpDown.Location = new System.Drawing.Point(124, 466);
            this.EndStatenumericUpDown.Maximum = new decimal(new int[] {
            1215752192,
            23,
            0,
            0});
            this.EndStatenumericUpDown.Name = "EndStatenumericUpDown";
            this.EndStatenumericUpDown.Size = new System.Drawing.Size(96, 20);
            this.EndStatenumericUpDown.TabIndex = 72;
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.channel8checkbox);
            this.panel3.Controls.Add(this.channel7checkbox);
            this.panel3.Controls.Add(this.channel6checkbox);
            this.panel3.Controls.Add(this.channel5checkbox);
            this.panel3.Controls.Add(this.channel4checkbox);
            this.panel3.Controls.Add(this.channel3checkbox);
            this.panel3.Controls.Add(this.channel2checkbox);
            this.panel3.Controls.Add(this.channel1checkbox);
            this.panel3.Controls.Add(this.label4);
            this.panel3.Location = new System.Drawing.Point(280, 20);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(156, 80);
            this.panel3.TabIndex = 57;
            // 
            // channel8checkbox
            // 
            this.channel8checkbox.AutoSize = true;
            this.channel8checkbox.Location = new System.Drawing.Point(121, 56);
            this.channel8checkbox.Name = "channel8checkbox";
            this.channel8checkbox.Size = new System.Drawing.Size(32, 17);
            this.channel8checkbox.TabIndex = 19;
            this.channel8checkbox.Text = "8";
            this.channel8checkbox.UseVisualStyleBackColor = true;
            // 
            // channel7checkbox
            // 
            this.channel7checkbox.AutoSize = true;
            this.channel7checkbox.Location = new System.Drawing.Point(83, 56);
            this.channel7checkbox.Name = "channel7checkbox";
            this.channel7checkbox.Size = new System.Drawing.Size(32, 17);
            this.channel7checkbox.TabIndex = 18;
            this.channel7checkbox.Text = "7";
            this.channel7checkbox.UseVisualStyleBackColor = true;
            // 
            // channel6checkbox
            // 
            this.channel6checkbox.AutoSize = true;
            this.channel6checkbox.Location = new System.Drawing.Point(45, 56);
            this.channel6checkbox.Name = "channel6checkbox";
            this.channel6checkbox.Size = new System.Drawing.Size(32, 17);
            this.channel6checkbox.TabIndex = 17;
            this.channel6checkbox.Text = "6";
            this.channel6checkbox.UseVisualStyleBackColor = true;
            // 
            // channel5checkbox
            // 
            this.channel5checkbox.AutoSize = true;
            this.channel5checkbox.Location = new System.Drawing.Point(7, 56);
            this.channel5checkbox.Name = "channel5checkbox";
            this.channel5checkbox.Size = new System.Drawing.Size(32, 17);
            this.channel5checkbox.TabIndex = 16;
            this.channel5checkbox.Text = "5";
            this.channel5checkbox.UseVisualStyleBackColor = true;
            // 
            // channel4checkbox
            // 
            this.channel4checkbox.AutoSize = true;
            this.channel4checkbox.Location = new System.Drawing.Point(121, 30);
            this.channel4checkbox.Name = "channel4checkbox";
            this.channel4checkbox.Size = new System.Drawing.Size(32, 17);
            this.channel4checkbox.TabIndex = 15;
            this.channel4checkbox.Text = "4";
            this.channel4checkbox.UseVisualStyleBackColor = true;
            // 
            // channel3checkbox
            // 
            this.channel3checkbox.AutoSize = true;
            this.channel3checkbox.Location = new System.Drawing.Point(83, 30);
            this.channel3checkbox.Name = "channel3checkbox";
            this.channel3checkbox.Size = new System.Drawing.Size(32, 17);
            this.channel3checkbox.TabIndex = 14;
            this.channel3checkbox.Text = "3";
            this.channel3checkbox.UseVisualStyleBackColor = true;
            // 
            // channel2checkbox
            // 
            this.channel2checkbox.AutoSize = true;
            this.channel2checkbox.Location = new System.Drawing.Point(45, 30);
            this.channel2checkbox.Name = "channel2checkbox";
            this.channel2checkbox.Size = new System.Drawing.Size(32, 17);
            this.channel2checkbox.TabIndex = 13;
            this.channel2checkbox.Text = "2";
            this.channel2checkbox.UseVisualStyleBackColor = true;
            this.channel2checkbox.CheckedChanged += new System.EventHandler(this.channel2checkbox_CheckedChanged);
            // 
            // channel1checkbox
            // 
            this.channel1checkbox.AutoSize = true;
            this.channel1checkbox.Location = new System.Drawing.Point(7, 30);
            this.channel1checkbox.Name = "channel1checkbox";
            this.channel1checkbox.Size = new System.Drawing.Size(32, 17);
            this.channel1checkbox.TabIndex = 12;
            this.channel1checkbox.Text = "1";
            this.channel1checkbox.UseVisualStyleBackColor = true;
            this.channel1checkbox.CheckedChanged += new System.EventHandler(this.channel1checkbox_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(81, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Audio Channels";
            // 
            // sampleratetext
            // 
            this.sampleratetext.Location = new System.Drawing.Point(280, 119);
            this.sampleratetext.Name = "sampleratetext";
            this.sampleratetext.Size = new System.Drawing.Size(67, 20);
            this.sampleratetext.TabIndex = 74;
            // 
            // bitspersampletext
            // 
            this.bitspersampletext.Location = new System.Drawing.Point(364, 119);
            this.bitspersampletext.Name = "bitspersampletext";
            this.bitspersampletext.Size = new System.Drawing.Size(70, 20);
            this.bitspersampletext.TabIndex = 75;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(280, 103);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 13);
            this.label3.TabIndex = 20;
            this.label3.Text = "Sample Rate";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(361, 103);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(78, 13);
            this.label5.TabIndex = 76;
            this.label5.Text = "Bits per sample";
            // 
            // naudmaxtext
            // 
            this.naudmaxtext.Location = new System.Drawing.Point(750, 203);
            this.naudmaxtext.Name = "naudmaxtext";
            this.naudmaxtext.Size = new System.Drawing.Size(60, 20);
            this.naudmaxtext.TabIndex = 77;
            // 
            // naudmintext
            // 
            this.naudmintext.Location = new System.Drawing.Point(750, 228);
            this.naudmintext.Name = "naudmintext";
            this.naudmintext.Size = new System.Drawing.Size(60, 20);
            this.naudmintext.TabIndex = 78;
            // 
            // naudaveragetext
            // 
            this.naudaveragetext.Location = new System.Drawing.Point(750, 254);
            this.naudaveragetext.Name = "naudaveragetext";
            this.naudaveragetext.Size = new System.Drawing.Size(60, 20);
            this.naudaveragetext.TabIndex = 79;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(645, 206);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(27, 13);
            this.label6.TabIndex = 80;
            this.label6.Text = "Max";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(645, 231);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(24, 13);
            this.label7.TabIndex = 81;
            this.label7.Text = "Min";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(645, 257);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(34, 13);
            this.label8.TabIndex = 82;
            this.label8.Text = "Mean";
            // 
            // Maud
            // 
            this.Maud.AutoSize = true;
            this.Maud.Location = new System.Drawing.Point(698, 184);
            this.Maud.Name = "Maud";
            this.Maud.Size = new System.Drawing.Size(34, 13);
            this.Maud.TabIndex = 83;
            this.Maud.Text = "Maud";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(762, 184);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(33, 13);
            this.label10.TabIndex = 84;
            this.label10.Text = "Naud";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(420, 514);
            this.progressBar1.MarqueeAnimationSpeed = 1;
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(387, 23);
            this.progressBar1.TabIndex = 85;
            // 
            // AudioRendererCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.Maud);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.naudaveragetext);
            this.Controls.Add(this.naudmintext);
            this.Controls.Add(this.naudmaxtext);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.bitspersampletext);
            this.Controls.Add(this.sampleratetext);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.EndStateLabel);
            this.Controls.Add(this.EndStatenumericUpDown);
            this.Controls.Add(this.StateIndexLabel);
            this.Controls.Add(this.StartStateNumericUpDown);
            this.Controls.Add(this.createbutton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.audioVSDPtext);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.maudnaudchart);
            this.Controls.Add(this.axWindowsMediaPlayer1);
            this.Controls.Add(this.getfilebutton);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.maudaveragetext);
            this.Controls.Add(this.maudmintext);
            this.Controls.Add(this.maudmaxtext);
            this.Controls.Add(this.maudnaudbutton);
            this.Controls.Add(this.audioSDPtext);
            this.Name = "AudioRendererCtrl";
            this.Size = new System.Drawing.Size(822, 554);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axWindowsMediaPlayer1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maudnaudchart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.StartStateNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.EndStatenumericUpDown)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox audioSDPtext;
        private System.Windows.Forms.Button maudnaudbutton;
        private System.Windows.Forms.TextBox maudmaxtext;
        private System.Windows.Forms.TextBox maudmintext;
        private System.Windows.Forms.TextBox maudaveragetext;
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
        private System.Windows.Forms.Button getfilebutton;
        private AxWMPLib.AxWindowsMediaPlayer axWindowsMediaPlayer1;
        private System.Windows.Forms.DataVisualization.Charting.Chart maudnaudchart;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox audioVSDPtext;
        private System.Windows.Forms.Button createbutton;
        private System.Windows.Forms.Label StateIndexLabel;
        private System.Windows.Forms.NumericUpDown StartStateNumericUpDown;
        private System.Windows.Forms.Label EndStateLabel;
        private System.Windows.Forms.NumericUpDown EndStatenumericUpDown;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.CheckBox channel8checkbox;
        private System.Windows.Forms.CheckBox channel7checkbox;
        private System.Windows.Forms.CheckBox channel6checkbox;
        private System.Windows.Forms.CheckBox channel5checkbox;
        private System.Windows.Forms.CheckBox channel4checkbox;
        private System.Windows.Forms.CheckBox channel3checkbox;
        private System.Windows.Forms.CheckBox channel2checkbox;
        private System.Windows.Forms.CheckBox channel1checkbox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox sampleratetext;
        private System.Windows.Forms.TextBox bitspersampletext;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox naudmaxtext;
        private System.Windows.Forms.TextBox naudmintext;
        private System.Windows.Forms.TextBox naudaveragetext;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label Maud;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}
