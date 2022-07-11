using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FrameVideoRendererClassLibrary
{
    public partial class FrameUserControl : UserControl
    {
        #region Members

        private int m_startstate = 0;
        public int Startstate { get { return m_startstate; } set { m_startstate = value; } }
        private int m_endstate = 0;
        public int Endstate { get { return m_endstate; } set { m_endstate = value; } }
        private string m_framestring = "";
        public string Framestring { get { return m_framestring; } set { m_framestring = value; } }
        private string m_protocol = "";
        public string Protocol { get { return m_protocol; } set { m_protocol = value; } }
        private int m_virtualchannel = 0;
        public int Virtualchannel { get { return m_virtualchannel; } set { m_virtualchannel = value; } }
        private int m_lanewidth = 0;
        public int Lanewidth { get { return m_lanewidth; } set { m_lanewidth = value; } }
        private string m_file = "";
        public string File { get { return m_file; } set { m_file = value; } }


        #endregion
        #region Constructors
        public FrameUserControl()
        {
            InitializeComponent();
        }
        public FrameUserControl(int start, int end, string text, string protocol, int virtualchannel, int lanewidth, string file)
        {
            InitializeComponent();
            m_startstate = start;
            m_endstate = end;
            m_protocol = protocol;
            m_virtualchannel = virtualchannel;
            m_lanewidth = lanewidth;
            m_file = file;
            m_framestring = "Frame " + text + " Protocol:" + m_protocol.ToString() + " VC:" + m_virtualchannel.ToString() + " #Lanes:" + m_lanewidth.ToString();
            frameCheckBox.Text = Framestring;

        }
        #endregion

        private void frameCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (frameCheckBox.Checked == true)
            {
                PictureRenderer form = new PictureRenderer(Startstate,Endstate,Protocol,Virtualchannel,Lanewidth,File);
                foreach (Control control in form.Controls)
                {
                    if (control is Panel)
                    {
                        foreach (Control c in ((Panel)control).Controls)
                        {
                            if (c is PictureUserControl)
                            {
                                ((PictureUserControl)c).RegisterInfoGroupDisplayEvent += new DP14MST_ECSummaryRegGrpDisplayEvent(processPaintEvent);
                            }
                        }
                       
                    }
                }
                form.Show();
            }
            else
            {
                
            }
        }
        private void processPaintEvent(object sender, DP14MST_ECSummaryRegGrpDisplayArgs e)
        {
            if (e.Painting == true)
            {
                paintButton.Enabled = false;
            }
        }
    }

}
