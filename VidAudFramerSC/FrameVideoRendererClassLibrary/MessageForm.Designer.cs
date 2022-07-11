namespace FrameVideoRendererClassLibrary
{
    partial class MessageForm
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
            this.MessagerichTextBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // MessagerichTextBox
            // 
            this.MessagerichTextBox.Location = new System.Drawing.Point(3, 12);
            this.MessagerichTextBox.Name = "MessagerichTextBox";
            this.MessagerichTextBox.Size = new System.Drawing.Size(269, 103);
            this.MessagerichTextBox.TabIndex = 0;
            this.MessagerichTextBox.Text = "";
            // 
            // MessageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.MessagerichTextBox);
            this.Name = "MessageForm";
            this.Text = "Message";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox MessagerichTextBox;
    }
}