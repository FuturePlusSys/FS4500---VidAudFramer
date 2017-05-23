namespace PixelRenderer
{
    partial class MainForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.videoTabPage = new System.Windows.Forms.TabPage();
            this.audioTabPage = new System.Windows.Forms.TabPage();
            this.frameRendererCtrl = new PixelRenderer.FrameVideoRendererCtrl();
            this.audioRendererCtrl1 = new PixelRenderer.AudioRendererCtrl();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.tabControl1.SuspendLayout();
            this.videoTabPage.SuspendLayout();
            this.audioTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.videoTabPage);
            this.tabControl1.Controls.Add(this.audioTabPage);
            this.tabControl1.Location = new System.Drawing.Point(12, 43);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1356, 785);
            this.tabControl1.TabIndex = 0;
            // 
            // videoTabPage
            // 
            this.videoTabPage.Controls.Add(this.frameRendererCtrl);
            this.videoTabPage.Location = new System.Drawing.Point(4, 22);
            this.videoTabPage.Name = "videoTabPage";
            this.videoTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.videoTabPage.Size = new System.Drawing.Size(1348, 759);
            this.videoTabPage.TabIndex = 0;
            this.videoTabPage.Text = "Video";
            this.videoTabPage.UseVisualStyleBackColor = true;
            // 
            // audioTabPage
            // 
            this.audioTabPage.Controls.Add(this.audioRendererCtrl1);
            this.audioTabPage.Location = new System.Drawing.Point(4, 22);
            this.audioTabPage.Name = "audioTabPage";
            this.audioTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.audioTabPage.Size = new System.Drawing.Size(1348, 759);
            this.audioTabPage.TabIndex = 1;
            this.audioTabPage.Text = "Audio";
            this.audioTabPage.UseVisualStyleBackColor = true;
            // 
            // frameRendererCtrl
            // 
            this.frameRendererCtrl.Location = new System.Drawing.Point(6, 6);
            this.frameRendererCtrl.Name = "frameRendererCtrl";
            this.frameRendererCtrl.Size = new System.Drawing.Size(1380, 789);
            this.frameRendererCtrl.TabIndex = 0;
            this.frameRendererCtrl.Load += new System.EventHandler(this.frameRendererCtrl_Load);
            // 
            // audioRendererCtrl1
            // 
            this.audioRendererCtrl1.Location = new System.Drawing.Point(6, 28);
            this.audioRendererCtrl1.Name = "audioRendererCtrl1";
            this.audioRendererCtrl1.Size = new System.Drawing.Size(1274, 693);
            this.audioRendererCtrl1.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1527, 886);
            this.Controls.Add(this.tabControl1);
            this.Name = "MainForm";
            this.Text = "Pixel Renderer";
            this.tabControl1.ResumeLayout(false);
            this.videoTabPage.ResumeLayout(false);
            this.audioTabPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage videoTabPage;
        private System.Windows.Forms.TabPage audioTabPage;
        private FrameVideoRendererCtrl frameRendererCtrl;
        private AudioRendererCtrl audioRendererCtrl1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
    }
}