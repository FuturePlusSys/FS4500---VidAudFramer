namespace FrameVideoRendererClassLibrary
{
    partial class PictureRenderer
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
            this.PictureUserControlPanel = new System.Windows.Forms.Panel();
            this.pictureUserControl1 = new FrameVideoRendererClassLibrary.PictureUserControl();
            this.PictureUserControlPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // PictureUserControlPanel
            // 
            this.PictureUserControlPanel.AutoScroll = true;
            this.PictureUserControlPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.PictureUserControlPanel.Controls.Add(this.pictureUserControl1);
            this.PictureUserControlPanel.Location = new System.Drawing.Point(12, 12);
            this.PictureUserControlPanel.Name = "PictureUserControlPanel";
            this.PictureUserControlPanel.Size = new System.Drawing.Size(1164, 806);
            this.PictureUserControlPanel.TabIndex = 0;
            // 
            // pictureUserControl1
            // 
            this.pictureUserControl1.AutoScroll = true;
            this.pictureUserControl1.Data = null;
            this.pictureUserControl1.Endstate = 0;
            this.pictureUserControl1.Frameid = 0;
            this.pictureUserControl1.Framestring = "";
            this.pictureUserControl1.Lanewidth = 0;
            this.pictureUserControl1.Location = new System.Drawing.Point(3, 3);
            this.pictureUserControl1.Name = "pictureUserControl1";
            this.pictureUserControl1.PathFile = "";
            this.pictureUserControl1.Pixelstate = 0;
            this.pictureUserControl1.Size = new System.Drawing.Size(1138, 780);
            this.pictureUserControl1.Startstate = 0;
            this.pictureUserControl1.StatesBeforeTrigger = 0;
            this.pictureUserControl1.TabIndex = 0;
            this.pictureUserControl1.Test = "";
            this.pictureUserControl1.Virtualchannel = 0;
            // 
            // PictureRenderer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(1207, 850);
            this.Controls.Add(this.PictureUserControlPanel);
            this.Name = "PictureRenderer";
            this.Text = "PictureRenderer";
            this.SizeChanged += new System.EventHandler(this.PictureRenderer_SizeChanged);
            this.PictureUserControlPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel PictureUserControlPanel;
        private PictureUserControl pictureUserControl1;
    }
}