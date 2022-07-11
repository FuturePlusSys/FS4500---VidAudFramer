namespace FrameVideoRendererClassLibrary
{
    partial class FrameUserControl
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
            this.paintButton = new System.Windows.Forms.Button();
            this.frameCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // paintButton
            // 
            this.paintButton.Location = new System.Drawing.Point(451, 13);
            this.paintButton.Name = "paintButton";
            this.paintButton.Size = new System.Drawing.Size(75, 23);
            this.paintButton.TabIndex = 0;
            this.paintButton.Text = "Paint";
            this.paintButton.UseVisualStyleBackColor = true;
            // 
            // frameCheckBox
            // 
            this.frameCheckBox.AutoSize = true;
            this.frameCheckBox.Location = new System.Drawing.Point(14, 13);
            this.frameCheckBox.Name = "frameCheckBox";
            this.frameCheckBox.Size = new System.Drawing.Size(85, 17);
            this.frameCheckBox.TabIndex = 1;
            this.frameCheckBox.Text = "Show Frame";
            this.frameCheckBox.UseVisualStyleBackColor = true;
            this.frameCheckBox.CheckedChanged += new System.EventHandler(this.frameCheckBox_CheckedChanged);
            // 
            // FrameUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Highlight;
            this.Controls.Add(this.frameCheckBox);
            this.Controls.Add(this.paintButton);
            this.Name = "FrameUserControl";
            this.Size = new System.Drawing.Size(541, 52);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button paintButton;
        private System.Windows.Forms.CheckBox frameCheckBox;
    }
}
