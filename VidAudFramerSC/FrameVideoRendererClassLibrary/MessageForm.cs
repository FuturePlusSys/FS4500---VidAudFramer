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
    public partial class MessageForm : Form
    {
        public MessageForm()
        {
            InitializeComponent();
        }
        public MessageForm(string message)
        {
            InitializeComponent();
            MessagerichTextBox.Text = message;
        }
    }
}
