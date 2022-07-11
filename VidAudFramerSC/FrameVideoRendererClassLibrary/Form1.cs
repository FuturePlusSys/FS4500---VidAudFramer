using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FrameVideoRendererClassLibrary
{
    public partial class AnalyzeForm : Form
    {
        public AnalyzeForm()
        {
            InitializeComponent();
        }

        public AnalyzeForm(string text)
        {
            InitializeComponent();
            AnalyzeRichTextBox.Text = text;
        }
    }
}
