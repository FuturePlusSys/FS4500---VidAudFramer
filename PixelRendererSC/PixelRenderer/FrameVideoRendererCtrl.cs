using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DP12MSTClassLibrary;
using DP14MSTClassLibrary;
using DP12SSTClassLibrary;
using DP14SSTClassLibrary;
using FPSProbeMgr_Gen2;
using SharedProject1;
using System.Xml;
using System.Collections;
using System.IO;

namespace PixelRenderer
{
    public partial class FrameVideoRendererCtrl : UserControl
    {
        #region Members
        //private DP11SST m_DP11aProbe = null;
        private DP12SST m_DP12SSTProbe = null;
        private DP12MST m_DP12MSTProbe = null;
        private DP14SST m_DP14SSTProbe = null;
        private DP14MST m_DP14MSTProbe = null;
        IProbeMgrGen2 m_IProbe = null;              // this sets us up for polymorphism...  because DP12MST inherits the FPSProbeMgrGen2 interface,
                                                    // then all the different probe versions can be assigned to this one base type.
        private List<long> pixelList = new List<long>();

        private int m_width = 0;
        private int Width { get { return m_width; } set { m_width = value; } }

        private string m_format = "";
        private string Format { get { return m_format; } set { m_format = value; } }

        private string m_protocol = "";
        private string Protocol { get { return m_protocol; } set { m_protocol = value; } }

        private long m_mask = 0;
        private long Mask { get { return m_mask; } set { m_mask = value; } }

        private int m_eventwidth = 0;
        private int EventWidth { get { return m_eventwidth; } set { m_eventwidth = value; } }

        private int m_eventsb = 0;
        private int Eventsb { get { return m_eventsb; } set { m_eventsb = value; } }

        private int m_lane0sb = 0;
        private int Lane0sb { get { return m_lane0sb; } set { m_lane0sb = value; } }

        private int m_lane1sb = 0;
        private int Lane1sb { get { return m_lane1sb; } set { m_lane1sb = value; } }

        private int m_lane2sb = 0;
        private int Lane2sb { get { return m_lane2sb; } set { m_lane2sb = value; } }

        private int m_lane3sb = 0;
        private int Lane3sb { get { return m_lane3sb; } set { m_lane3sb = value; } }
        #endregion // Members

        #region Ctor

        /// <summary>
        /// Default Constructor(s)
        /// </summary>
        public FrameVideoRendererCtrl()
        {
            InitializeComponent();
            DefaultSetup();
        }
        #endregion // Ctor(s)

        #region Event Handlers



        /// <summary>
        /// Protocol selection event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DP1_2SSTbutton_CheckedChanged(object sender, EventArgs e)
        {
            VC1button.Enabled = false;
            VC2button.Enabled = false;
            VC3button.Enabled = false;
            VC4button.Enabled = false;
            VC1button.Checked = false;
            VC2button.Checked = false;
            VC3button.Checked = false;
            VC4button.Checked = false;
        }
        private void DP1_2MSTbutton_CheckedChanged(object sender, EventArgs e)
        {
            if (DP1_2MSTbutton.Checked == true)
            {
                VC1button.Enabled = true;
                VC2button.Enabled = true;
                VC3button.Enabled = true;
                VC4button.Enabled = true;
            }
        }
        private void DP1_3SSTbutton_CheckedChanged(object sender, EventArgs e)
        {
            VC1button.Enabled = false;
            VC2button.Enabled = false;
            VC3button.Enabled = false;
            VC4button.Enabled = false;
            VC1button.Checked = false;
            VC2button.Checked = false;
            VC3button.Checked = false;
            VC4button.Checked = false;
        }
        private void DP1_3MSTbutton_CheckedChanged(object sender, EventArgs e)
        {
            if (DP1_4MSTbutton.Checked == true)
            {
                VC1button.Enabled = true;
                VC2button.Enabled = true;
                VC3button.Enabled = true;
                VC4button.Enabled = true;
            }
        }
        private void RGBbutton_CheckedChanged(object sender, EventArgs e)
        {
            if (RGBbutton.Checked == false && YCbCr444button.Checked == false)
            {
                RGBgroupbox.Enabled = false;
                RGB18button.Checked = false;
                RGB24button.Checked = false;
                RGB30button.Checked = false;
                RGB36button.Checked = false;
                RGB48button.Checked = false;
            }
            else if (RGBbutton.Checked == true || YCbCr444button.Checked == true)
            {
                RGBgroupbox.Enabled = true;
            }
            if (YCbCr422button.Checked == true)
            {
                YCbCr422groupbox.Enabled = true;
            }
            else if (YCbCr420button.Checked == true)
            {
                YCbCr420groupbox.Enabled = true;
            }
        }
        private void YCbCr444button_CheckedChanged(object sender, EventArgs e)
        {
            if (RGBbutton.Checked == false && YCbCr444button.Checked == false)
            {
                RGBgroupbox.Enabled = false;
                RGB18button.Checked = false;
                RGB24button.Checked = false;
                RGB30button.Checked = false;
                RGB36button.Checked = false;
                RGB48button.Checked = false;
            }
            else if (RGBbutton.Checked == true || YCbCr444button.Checked == true)
            {
                RGBgroupbox.Enabled = true;
            }
            if (YCbCr422button.Checked == true)
            {
                YCbCr422groupbox.Enabled = true;
            }
            else if (YCbCr420button.Checked == true)
            {
                YCbCr420groupbox.Enabled = true;
            }
        }
        private void YCbCr422button_CheckedChanged(object sender, EventArgs e)
        {
            if (RGBbutton.Checked == true || YCbCr444button.Checked == true || YCbCr420button.Checked == true)
            {
                YCbCr422groupbox.Enabled = false;
                YCbCr16button.Checked = false;
                YCbCr20button.Checked = false;
                YCbCr24button.Checked = false;
                YCbCr32button.Checked = false;
            }
            else if (YCbCr422button.Checked == true)
            {
                YCbCr422groupbox.Enabled = true;
            }
        }
        private void YCbCr420button_CheckedChanged(object sender, EventArgs e)
        {
            if (RGBbutton.Checked == true || YCbCr444button.Checked == true || YCbCr422button.Checked == true)
            {
                YCbCr420groupbox.Enabled = false;
                YCbCr15button.Checked = false;
                YCbCr18button.Checked = false;
                YCbCr420_24button.Checked = false;
            }
            else if (YCbCr420button.Checked == true)
            {
                YCbCr420groupbox.Enabled = true;
            }
        }
        private void Savebutton_Click(object sender, EventArgs e)
        {
            if (PictureBox.Image != null)
            {
                SaveFileDialog sfile = new SaveFileDialog();
                sfile.Filter = "JPEG files(*.jpeg)|*.jpeg";
                if (DialogResult.OK == sfile.ShowDialog())
                {
                    this.PictureBox.Image.Save(sfile.FileName);
                }
            }
        }
        private void Clearbutton_Click(object sender, EventArgs e)
        {
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is GroupBox)
                {
                    foreach (Control grpBoxCtrl in ctrl.Controls)
                    {
                        if (grpBoxCtrl is RadioButton)
                        {
                            if (((RadioButton)grpBoxCtrl).Checked == true)
                            {
                                ((RadioButton)grpBoxCtrl).Checked = false;
                            }
                        }
                        else if (grpBoxCtrl is CheckBox)
                        {
                            if (((CheckBox)grpBoxCtrl).Checked == true)
                            {
                                ((CheckBox)grpBoxCtrl).Checked = false;
                            }
                        }
                        else if (grpBoxCtrl is GroupBox)
                        {
                            foreach (Control grpbox in grpBoxCtrl.Controls)
                            {
                                if (grpbox is RadioButton)
                                {
                                    if (((RadioButton)grpbox).Checked == true)
                                    {
                                        ((RadioButton)grpbox).Checked = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            PictureBox.Image = null;
            DefaultSetup();
            StateNumberNumericUpDown.Value = 0;
        }
        private void paintbutton_Click(object sender, EventArgs e)
        {
            this.progressBar1.Value = 0;
            PictureBox.Image = null;
            Protocol = getprotocol();
            int states = 0;
            if (Protocol == "error")
            {
                string err = "No Protocal selected";
                Runerror error = new Runerror(err);
                error.Show();
                Format = " ";
            }
            else
            {
                createInterfaceObject();  // assign the m_IProbe variable.
                m_IProbe.Initialize();
                if ((DP1_2MSTbutton.Checked == true || DP1_4MSTbutton.Checked == true) && getvc() == 0)
                {
                    string err = "No Virtual Channel selected in MST mode";
                    Runerror error = new Runerror(err);
                    error.Show();
                    Format = " ";
                }
                else
                {
                    states = Convert.ToInt32(m_IProbe.GetNumberOfStates(getvc()));
                }
            }
            if (pixelList.Count != 0)
            {
                pixelList.Clear();
            }
            int lane = 0;
            if (Lane1button.Checked)
            {
                lane = 1;
            }
            else if (Lane2button.Checked)
            {
                lane = 2;
            }
            else if (lane4button.Checked)
            {
                lane = 4;
            }

            Width = getpixelwidth();
            Format = getformat();
            Mask = getmask(Width);
            xmlreader();
            if (states == 0)
            {
                string err = "No states found. Check Protocal or Probe Manager";
                Runerror error = new Runerror(err);
                error.Show();
                Format = " ";
            }
            if ((int)WidthnumericUpDown.Value == 0 || (int)HeightnumericUpDown.Value == 0)
            {
                string err = "Set bitmap Dimentions";
                Runerror error = new Runerror(err);
                error.Show();
                Format = " ";
            }
            else
            {
                this.progressBar1.Maximum = (int)WidthnumericUpDown.Value * (int)HeightnumericUpDown.Value;
            }
            if (Format == "RGB" || Format == "YCbCr444")
            {
                getpixeldata(lane, Width);
            }
            if (Format == "YCbCr422")
            {
                getpixeldataYCbCr422(lane, Width);
            }
            if (Format == "YCbCr420")
            {
                getpixeldataYCbCr420(lane, Width);
            }
        }

        #endregion // Event Handlers

        #region Private Methods

        /// <summary>
        /// Create the class library and initialize the interface variable
        /// </summary>
        private bool createInterfaceObject()
        {
            string protocol = getprotocol();
            bool status = true;
            switch (protocol)
            {
                case "SST-1.2":
                    if (m_DP12SSTProbe != null)
                        m_DP12SSTProbe = null;
                    m_DP12SSTProbe = new DP12SST();
                    m_IProbe = (IProbeMgrGen2)m_DP12SSTProbe;
                    break;
                case "MST-1.2":
                    if (m_DP12MSTProbe != null)
                        m_DP12MSTProbe = null;
                    m_DP12MSTProbe = new DP12MST();
                    m_IProbe = (IProbeMgrGen2)m_DP12MSTProbe;
                    break;
                case "SST-1.4":
                    if (m_DP14SSTProbe != null)
                        m_DP14SSTProbe = null;
                    m_DP14SSTProbe = new DP14SST();
                    m_IProbe = (IProbeMgrGen2)m_DP14SSTProbe;
                    break;
                case "MST-1.4":
                    if (m_DP14MSTProbe != null)
                        m_DP14MSTProbe = null;
                    m_DP14MSTProbe = new DP14MST();
                    m_IProbe = (IProbeMgrGen2)m_DP14MSTProbe;
                    break;
            }
            //    if (m_DP11aProbe != null)
            //        m_DP11aProbe = null;

            //    if (m_DP12SSTProbe != null)
            //        m_DP12SSTProbe = null;

            //if (m_DP12SSTProbe != null)
            //    m_DP12SSTProbe = null;
            //if (m_DP12MSTProbe != null)
            //    m_DP12MSTProbe = null;
            //if (m_DP13SSTProbe != null)
            //    m_DP13SSTProbe = null;
            //if (m_DP13MSTProbe != null)
            //    m_DP13MSTProbe = null;

            //switch (toolStripModeComboBox.SelectedIndex)
            //{
            //    case 0:
            //        m_DP11aProbe = new DP11ProbeMgrGen2();
            //        m_IProbe = (IProbeMgrGen2)m_DP11aProbe;
            //        m_IProbe.OnLogMsgEvent += new LogMsgEvent(processLogMsgEvent);
            //        m_IProbe.OnTBUploadEvent += new TBUploadEvent(processTBUploadEvent);
            //        m_IProbe.OnProbeCommEvent += new ProbeCommEvent(processProbeCommEvent);
            //        break;
            //    case 1:
            //        m_DP12SSTProbe = new DP12SSTProbeMgrGen2();
            //        m_IProbe = (IProbeMgrGen2)m_DP12SSTProbe;
            //        m_IProbe.OnLogMsgEvent += new LogMsgEvent(processLogMsgEvent);
            //        m_IProbe.OnTBUploadEvent += new TBUploadEvent(processTBUploadEvent);
            //        m_IProbe.OnProbeCommEvent += new ProbeCommEvent(processProbeCommEvent);
            //        break;
            //    case 2:

            //m_DP12SSTProbe = new DP12SST();
            //m_IProbe = (IProbeMgrGen2)m_DP12SSTProbe;   // from now on, all calls to the trace data mgr goes through this class member (interface variable)




            //        break;
            //    case 3:
            //        m_DP13SSTProbe = new DP13SSTProbeMgrGen2();
            //        m_IProbe = (IProbeMgrGen2)m_DP13SSTProbe;
            //        m_IProbe.OnLogMsgEvent += new LogMsgEvent(processLogMsgEvent);
            //        m_IProbe.OnTBUploadEvent += new TBUploadEvent(processTBUploadEvent);
            //        m_IProbe.OnProbeCommEvent += new ProbeCommEvent(processProbeCommEvent);
            //        break;
            //    case 4:
            //        m_DP13MSTProbe = new DP13MSTProbeMgrGen2();
            //        m_IProbe = (IProbeMgrGen2)m_DP13MSTProbe;
            //        m_IProbe.OnLogMsgEvent += new LogMsgEvent(processLogMsgEvent);
            //        m_IProbe.OnTBUploadEvent += new TBUploadEvent(processTBUploadEvent);
            //        m_IProbe.OnProbeCommEvent += new ProbeCommEvent(processProbeCommEvent);
            //        break;

            //    default:
            //        m_DP11aProbe = new DP11ProbeMgrGen2();
            //        m_IProbe = (IProbeMgrGen2)m_DP11aProbe;
            //        m_IProbe.OnLogMsgEvent += new LogMsgEvent(processLogMsgEvent);
            //        m_IProbe.OnTBUploadEvent += new TBUploadEvent(processTBUploadEvent);
            //        m_IProbe.OnProbeCommEvent += new ProbeCommEvent(processProbeCommEvent);
            //        break;
            //}

            return status;
        }


        /// <summary>
        /// The DefaultSetup that first appears and when the clear button is pushed.
        /// </summary>
        private void DefaultSetup()
        {
            RGBbutton.Checked = true;
            RGB24button.Checked = true;
            YCbCr422groupbox.Enabled = false;
            YCbCr420groupbox.Enabled = false;
            Lane1button.Checked = true;
        }

        /// <summary>
        /// Returns the protocal that is being used
        /// </summary>
        /// <returns></returns>
        private string getprotocol()
        {
            string protocal = "";
            if (DP1_2SSTbutton.Checked)
                protocal = "SST-1.2";
            else if (DP1_2MSTbutton.Checked)
                protocal = "MST-1.2";
            else if (DP1_4SSTbutton.Checked)
                protocal = "SST-1.4";
            else if (DP1_4MSTbutton.Checked)
                protocal = "MST-1.4";
            else
                protocal = "error";
            return protocal;
        }

        /// <summary>
        /// Return virtual channel
        /// </summary>
        /// <returns></returns>
        private int getvc()
        {
            int vc = 0;
            if (getprotocol() == "SST-1.4" || getprotocol() == "SST-1.2")
            {
                vc = 1;
            }
            else
            {
                if (VC1button.Checked)
                    vc = 1;
                else if (VC2button.Checked)
                    vc = 2;
                else if (VC3button.Checked)
                    vc = 3;
                else if (VC4button.Checked)
                    vc = 4;
            }
            return vc;
        }

        /// <summary>
        /// Returns the pixel format being used.
        /// </summary>
        /// <returns></returns>
        private string getformat()
        {
            string format = "";
            if (RGBbutton.Checked)
                format = "RGB";
            else if (YCbCr444button.Checked)
                format = "YCbCr444";
            else if (YCbCr422button.Checked)
                format = "YCbCr422";
            else if (YCbCr420button.Checked)
                format = "YCbCr420";
            return format;
        }

        /// <summary>
        /// This will check if a frame can be found in the file, will return true if found, false if not. 
        /// Will also increment the state number
        /// </summary>
        /// <param name="data"></param>
        /// <param name="i"></param>
        /// <param name="vc"></param>
        /// <returns></returns>
        private bool checkforframe(ref int i, int vc)
        {
            string VBS = "0x4A";
            string HBE = "0x15";
            string VBE = "0x55";
            int flag = 0;
            bool check = false;
            long states = m_IProbe.GetNumberOfStates(vc);
            while (i < states)
            {
                StringBuilder sb = Geteventcode(m_IProbe.GetStateData(vc, i));
                if (sb.ToString() == VBS && flag == 0) //Vertical Blanking Start
                {
                    flag = 1;
                }
                if (flag == 1) //After Blanking Start was found
                {
                    if (sb.ToString() == HBE || sb.ToString() == VBE) //Horizontal Blanking End or Vertical
                    {
                        check = true;
                        i++;
                        break;
                    }
                }
                i++;
            }
            if (check != true)
            {
                string s = "Frame not found";
                Runerror error = new Runerror(s);
                error.Show();
            }
            return check;
        }

        /// <summary>
        /// Converts YCbCr components into RGB to place in Bitmap
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="y"></param>
        /// <param name="cb"></param>
        /// <param name="cr"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        private void YCbCr_to_RGB(ref Bitmap bmp, int y, int cb, int cr, int w, int h)
        {
            double r = (y + 1.4 * (cr - 128));
            if (r < 0)
                r = 0;
            if (r > 255)
                r = 255;
            double g = (y - .343 * (cb - 128) - 0.711 * (cr - 128));
            if (g < 0)
                g = 0;
            if (g > 255)
                g = 255;
            double b = (y + 1.765 * (cb - 0x80));
            if (b < 0)
                b = 0;
            if (b > 255)
                b = 255;

            if (w != (int)(WidthnumericUpDown.Value))
            {
                bmp.SetPixel(w, h, Color.FromArgb(Convert.ToInt32(r), Convert.ToInt32(g), Convert.ToInt32(b)));
            }
        }

        /// <summary>
        /// Get width of the pixel
        /// </summary>
        /// <returns></returns>
        private int getpixelwidth()
        {
            int width = 0;
            if (RGB18button.Checked)
                width = 18;
            else if (RGB24button.Checked)
                width = 24;
            else if (RGB30button.Checked)
                width = 30;
            else if (RGB36button.Checked)
                width = 36;
            else if (RGB48button.Checked)
                width = 48;
            else if (YCbCr16button.Checked)
                width = 16;
            else if (YCbCr20button.Checked)
                width = 20;
            else if (YCbCr24button.Checked)
                width = 24;
            else if (YCbCr32button.Checked)
                width = 32;
            else if (YCbCr12button.Checked)
                width = 12;
            else if (YCbCr15button.Checked)
                width = 15;
            else if (YCbCr18button.Checked)
                width = 18;
            else if (YCbCr420_24button.Checked)
                width = 24;
            return width;
        }

        private void xmlreader()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string mypath = Path.Combine(path, "FuturePlus\\FS4500\\pixelrender.xml");

            XmlReader reader = XmlReader.Create(mypath);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == Protocol)
                {
                    if (reader.GetAttribute("Name") == "Event")
                    {
                        EventWidth = Convert.ToInt32(reader.GetAttribute("Width"));
                        string decoy = reader.GetAttribute("StartBit");
                        Eventsb = Convert.ToInt32(decoy);
                    }
                    if (reader.GetAttribute("Name") == "Lane0")
                    {
                        Lane0sb = Convert.ToInt32(reader.GetAttribute("StartBit"));
                    }
                    if (reader.GetAttribute("Name") == "Lane1")
                    {
                        Lane1sb = Convert.ToInt32(reader.GetAttribute("StartBit"));
                    }
                    if (reader.GetAttribute("Name") == "Lane2")
                    {
                        Lane2sb = Convert.ToInt32(reader.GetAttribute("StartBit"));
                    }
                    if (reader.GetAttribute("Name") == "Lane3")
                    {
                        Lane3sb = Convert.ToInt32(reader.GetAttribute("StartBit"));
                    }
                }
            }
        }

        /// <summary>
        /// Returns a StringBuilding with the eventcode bits
        /// </summary>
        /// <param name="dataBytes"></param>
        /// <returns></returns>
        private StringBuilder Geteventcode(byte[] dataBytes)
        {
            byte result = 0x00;
            if (dataBytes != null)
            {
                StringBuilder sb = new StringBuilder();
                byte bits = dataBytes[(15 - Eventsb / EventWidth)];
                byte bits2 = dataBytes[(15 - (Eventsb - 7) / EventWidth)];
                result = (byte)(bits << EventWidth - ((Eventsb % EventWidth) + 1) | (bits2 >> (Eventsb - 7) % EventWidth));
                sb.Append("0x" + result.ToString("X2"));
                return sb;
            }
            return null;
        }


        /// <summary>
        /// Extracting the lane data from an inputed lane with help from xml file
        /// </summary>
        /// <param name="dataBytes"></param>
        /// <param name="startbit"></param>
        /// <returns></returns>
        private List<byte> Getlanedata(byte[] dataBytes)
        {
            List<byte> result = new List<Byte>();
            int startbit = 0;
            int i = 0;
            while (i != 4)
            {
                switch (i)
                {
                    case 0:
                        startbit = Lane0sb;
                        break;
                    case 1:
                        startbit = Lane1sb;
                        break;
                    case 2:
                        startbit = Lane2sb;
                        break;
                    case 3:
                        startbit = Lane3sb;
                        break;

                    default:
                        break;
                }
                StringBuilder sb = new StringBuilder();
                byte bits = dataBytes[15 - (startbit / 8)];
                byte bits2 = dataBytes[(15 - (startbit - 7) / 8)];
                result.Add((byte)(bits << 8 - ((startbit % 8) + 1) | (bits2 >> (startbit - 7) % 8)));
                i++;
            }
            return result;
        }

        /// <summary>
        /// Returns a mask for the length of the byte
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private long getmask(int n)
        {
            long mask = 0;
            switch (n)
            {
                case 18:
                    mask = Convert.ToInt64(0x3FFFF);
                    break;
                case 20:
                    mask = Convert.ToInt64(0xFFFFF);
                    break;
                case 24:
                    mask = Convert.ToInt64(0xFFFFFF);
                    break;
                case 30:
                    mask = Convert.ToInt64(0x3FFFFFFF);
                    break;
                case 36:
                    mask = Convert.ToInt64(0xFFFFFFFFF);
                    break;
                case 48:
                    mask = Convert.ToInt64(0xFFFFFFFFFFFF);
                    break;
                default:
                    break;
            }
            return mask;
        }

        /// <summary>
        /// Returns the masks for YCbCr for a component, example width of 16 means 8 bit components, mask will be 8.
        /// </summary>
        /// <returns></returns>
        private int getbitmaskYCbCr()
        {
            int mask = 0;
            if (YCbCr16button.Checked)
                mask = 0xFF;
            else if (YCbCr20button.Checked)
                mask = 0x3FF;
            else if (YCbCr24button.Checked)
                mask = 0xFFF;
            else if (YCbCr32button.Checked)
                mask = 0xFFFF;
            return mask;
        }

        /// <summary>
        /// Returns the component masks for RGB formats
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private int getbitmask(int n)
        {
            int mask = 0;
            switch (n)
            {
                case 18:
                    mask = Convert.ToInt32(0x3F);
                    break;
                case 24:
                    mask = Convert.ToInt32(0xFF);
                    break;
                case 30:
                    mask = Convert.ToInt32(0x3FF);
                    break;
                case 36:
                    mask = Convert.ToInt32(0xFFF);
                    break;
                case 48:
                    mask = Convert.ToInt32(0xFFFF);
                    break;
                default:
                    break;
            }
            return mask;
        }

        /// <summary>
        /// Creates a pixel with help from the Getlanedata function
        /// </summary>
        /// <param name="dataBytes"></param>
        /// <param name="width"></param>
        /// <returns name = "ulong"> </returns>
        private void createpixel(ref Bitmap bmp, List<byte[]> pixeldata, int width, int state, int lanes, int remander, ref int w, int h)
        {
            int decoywid = width; //Keep track of original width
            ulong pixel1 = 0; //The pixel
            ulong pixel2 = 0; //The pixel
            ulong pixel3 = 0; //The pixel
            ulong pixel4 = 0; //The pixel
            int component = width / 3;
            List<byte> comp = new List<byte>();
            double bytes = Math.Ceiling(width / 8.00); //Number of bytes that will be required
            int bits = Convert.ToInt32(((bytes - 1) * 8)); //Number of bits - 8, this is used for shifting.
            if (pixeldata.Count - state == 5 && decoywid == 30) //Weird case where the 30 bit width requires 5 bytes instead of the usual 4. 
            {
                bits += 8;
                width += 8;
            }
            while (width > 0)
            {
                width = width - 8;
                comp = Getlanedata(pixeldata[state]);
                if (bits == 0)
                {
                    pixel1 |= comp[0];
                    pixel2 |= comp[1];
                    pixel3 |= comp[2];
                    pixel4 |= comp[3];
                    pixel1 = pixel1 >> remander;
                    pixel1 = pixel1 & (ulong)Mask;
                    pixel2 = pixel2 >> remander;
                    pixel2 = pixel2 & (ulong)Mask;
                    pixel3 = pixel3 >> remander;
                    pixel3 = pixel3 & (ulong)Mask;
                    pixel4 = pixel4 >> remander;
                    pixel4 = pixel4 & (ulong)Mask;
                }
                else
                {
                    pixel1 |= (ulong)(Convert.ToInt64(comp[0]) << bits); //Error where the bits were not shifting when bits == 32. Converted comp to long to fix error.
                    pixel2 |= (ulong)(Convert.ToInt64(comp[1]) << bits);
                    pixel3 |= (ulong)(Convert.ToInt64(comp[2]) << bits);
                    pixel4 |= (ulong)(Convert.ToInt64(comp[3]) << bits);
                    state++;
                    bits -= 8;
                }
            }
            if (lanes == 4)
            {
                placepixel(pixel1, ref bmp, ref w, h);
                placepixel(pixel2, ref bmp, ref w, h);
                placepixel(pixel3, ref bmp, ref w, h);
                placepixel(pixel4, ref bmp, ref w, h);
            }
            else if (lanes == 2)
            {
                placepixel(pixel1, ref bmp, ref w, h);
                placepixel(pixel2, ref bmp, ref w, h);
            }
            else if (lanes == 1)
            {
                placepixel(pixel1, ref bmp, ref w, h);
            }
        }
        /// <summary>
        /// Create pixel list for YCbCr formats
        /// </summary>
        /// <param name="pixeldata"></param>
        /// <param name="width"></param>
        /// <param name="state"></param>
        /// <param name="lanes"></param>
        /// <param name="remander"></param>
        /// <returns></returns>
        private void createpixels(ref Bitmap bmp, List<byte[]> pixeldata, int width, int state, int lanes, int remander, ref int w, ref int h)
        {
            ulong pixel1 = 0;
            ulong pixel = 0;
            List<byte> statedata = new List<byte>();
            int i = 0;
            int k = 0;
            int originalwidth = width;
            ulong mask = 0xFFFFFFFFFFFFFFFF;
            bool flag = true;
            if (lanes > 1)
            {
                k = 1;
                if (lanes == 4) //there are four pixels, we need to loop again. Explained later
                {
                    flag = false;
                }
            }
            if (width == 20) //Weird case
            {
                mask = 0xFFFFF;
                width = 24;
            }
            int lane = 0;
            loop:
            while (width > 0)
            {
                statedata = Getlanedata(pixeldata[state + i]);
                pixel |= (uint)(statedata[lane] << (width - 8));
                width -= 8;
                i++;
                if (width <= 0 && k == 1) //after the Y and Cb in the first are extracted, save it in pixel1 and switch lanes. Set Width back to origanal
                {
                    width = originalwidth;
                    lane++;
                    i = 0;
                    k++;
                    pixel1 = (ulong)pixel;
                    if (remander != 0)
                    {
                        pixel1 = pixel1 >> remander;
                    }
                    pixel1 = pixel1 & mask;
                    pixel = 0;
                }
            }
            if (remander != 0)
            {
                pixel = pixel >> remander;
            }
            pixel |= (ulong)pixel1 << originalwidth; //add pixel1 to the the pixel so that all Y, Cb, Cr, and Y1 components are together.
            if (w == (int)WidthnumericUpDown.Value)
            {
                string e = "More pixels remain for line, width to small";
                Runerror error = new Runerror(e);
                error.Show();
                flag = true;
            }
            placepixel(pixel, ref bmp, ref w, h); //Place pixel
            if (flag == false) //If lanes is 4, Go back and loop again and set the other pixel. Flag will be set to true so that next time this will be skipped over.
            {
                lane++;
                width = originalwidth;
                flag = true;
                pixel = 0;
                pixel1 = 0;
                i = 0;
                k = 1;
                goto loop;
            }
        }

        /// <summary>
        /// Creating a pixel with YCbCr420 formats
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="even"></param>
        /// <param name="odd"></param>
        /// <param name="width"></param>
        /// <param name="state"></param>
        /// <param name="lane"></param>
        /// <param name="remander"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        private void createpixelsYCbCr420(ref Bitmap bmp, List<Byte[]> even, List<Byte[]> odd, int width, int state, int lanes, int remander, ref int w, int h) //Similar to RGB algorithm
        {
            int decoywid = width; //Keep track of original width
            int decoystate = state;
            ulong pixel1 = 0; //The pixel
            ulong pixel2 = 0; //The pixel
            ulong pixel3 = 0; //The pixel
            ulong pixel4 = 0; //The pixel
            ulong epixel1 = 0; //The pixel
            ulong epixel2 = 0; //The pixel
            ulong epixel3 = 0; //The pixel
            ulong epixel4 = 0; //The pixel
            int component = width / 3;
            List<byte> comp = new List<byte>();
            double bytes = Math.Ceiling(width / 8.00); //Number of bytes that will be required
            int bits = Convert.ToInt32(((bytes - 1) * 8)); //Number of bits - 8, this is used for shifting.
            int flag = 0;
            loop:
            while (width > 0)
            {
                width = width - 8;
                if (flag == 0)
                {
                    comp = Getlanedata(even[state]);
                }
                else
                {
                    comp = Getlanedata(odd[state]);
                }
                if (bits == 0)
                {
                    pixel1 |= (ulong)(comp[0]);
                    pixel1 = pixel1 >> remander;
                    //pixel1 = pixel1 & (ulong)Mask;
                    pixel2 |= (ulong)(comp[1]);
                    pixel2 = pixel2 >> remander;
                    //pixel2 = pixel2 & (ulong)Mask;
                    pixel3 |= (ulong)(comp[2]);
                    pixel3 = pixel3 >> remander;
                    //pixel3 = pixel3 & (ulong)Mask;
                    pixel4 |= (ulong)(comp[3]);
                    pixel4 = pixel4 >> remander;
                    //pixel4 = pixel4 & (ulong)Mask;
                }
                else
                {
                    pixel1 |= (uint)(comp[0] << bits);
                    pixel2 |= (uint)(comp[1] << bits);
                    pixel3 |= (uint)(comp[2] << bits);
                    pixel4 |= (uint)(comp[3] << bits);
                    state++;
                    bits -= 8;
                }
            }
            if (flag != 1) // Get data from the other list now
            {
                flag = 1;
                epixel1 = pixel1;
                epixel2 = pixel2;
                epixel3 = pixel3;
                epixel4 = pixel4;
                pixel1 = 0;
                pixel2 = 0;
                pixel3 = 0;
                pixel4 = 0;
                width = decoywid;
                bits = Convert.ToInt32(((bytes - 1) * 8));
                state = decoystate;
                goto loop;
            }
            if (lanes == 4)
            {
                placepixel420(ref bmp, epixel1, pixel1, w, h, decoywid);
                w += 2;
                placepixel420(ref bmp, epixel2, pixel2, w, h, decoywid);
                w += 2;
                placepixel420(ref bmp, epixel3, pixel3, w, h, decoywid);
                w += 2;
                placepixel420(ref bmp, epixel4, pixel4, w, h, decoywid);//pixel = evenpixel, pixel1 = oddpixel 
                w += 2;
            }
            else if (lanes == 2)
            {
                placepixel420(ref bmp, epixel1, pixel1, w, h, decoywid);
                w += 2;
                placepixel420(ref bmp, epixel2, pixel2, w, h, decoywid);
                w += 2;
            }
            else if (lanes == 1)
            {
                placepixel420(ref bmp, epixel1, pixel1, w, h, decoywid);
                w += 2;
            }
        }

        /// <summary>
        /// Recieves pixel and places pixel in the bitmap for YCbCr420
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="evenpixel"></param>
        /// <param name="oddpixel"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="width"></param>
        private void placepixel420(ref Bitmap bmp, ulong evenpixel, ulong oddpixel, int w, int h, int width) //4 pixels are being placed, 2 on current line of bitmap, and 2 on previous.
        {
            int component = width / 3;
            int eY = (int)(evenpixel >> component * 2);
            eY = eY & getbitmask(width);
            int eY1 = (int)(evenpixel >> component);
            eY1 = eY1 & getbitmask(width);
            int Cb = (int)(evenpixel);
            Cb = Cb & getbitmask(width);

            int oY = (int)(evenpixel >> component * 2);
            oY = oY & getbitmask(width);
            int oY1 = (int)(evenpixel >> component);
            oY1 = oY1 & getbitmask(width);
            int Cr = (int)(evenpixel);
            Cr = Cr & getbitmask(width);
            if (Width == 20)
            {
                eY /= 4;
                oY /= 4;
                Cb /= 4;
                Cr /= 4;
                eY1 /= 4;
                oY1 /= 4;
            }

            else if (Width == 24)
            {
                eY /= 16;
                oY /= 16;
                Cb /= 16;
                Cr /= 16;
                eY1 /= 16;
                oY1 /= 16;
            }

            else if (Width == 32)
            {
                eY /= 256;
                oY /= 256;
                Cb /= 256;
                Cr /= 256;
                eY1 /= 256;
                oY1 /= 256;
            }

            YCbCr_to_RGB(ref bmp, eY, Cb, Cr, w, h - 1);
            this.progressBar1.Increment(1);
            YCbCr_to_RGB(ref bmp, eY1, Cb, Cr, w + 1, h - 1);
            this.progressBar1.Increment(1);
            YCbCr_to_RGB(ref bmp, oY, Cb, Cr, w, h);
            this.progressBar1.Increment(1);
            YCbCr_to_RGB(ref bmp, oY1, Cb, Cr, w + 1, h);
            this.progressBar1.Increment(1);

        }

        /// <summary>
        /// Places pixel in bitmap from RGB, YCbCr444, and YCbCr422 formats
        /// </summary>
        /// <param name="pixel"></param>
        /// <param name="bmp"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        private void placepixel(ulong pixel, ref Bitmap bmp, ref int w, int h)
        {
            string format = Format;
            if (format == "RGB")
            {
                placeRGB(ref w, ref h, pixel, ref bmp);
            }
            else if (format == "YCbCr444")
            {
                placeYCbCr444(ref w, ref h, pixel, ref bmp);
            }
            else if (format == "YCbCr422")
            {
                placeYCbCr422(ref w, ref h, pixel, ref bmp);
            }
        }

        private void placeRGB(ref int w, ref int h, ulong pixel, ref Bitmap bmp)
        {
            int component = Width / 3;
            int r = (int)(pixel >> component * 2);
            r = r & getbitmask(Width);
            int g = (int)(pixel >> component);
            g = g & getbitmask(Width);
            int b = (int)(pixel);
            b = b & getbitmask(Width);
            if (Width == 30) //This is to bring RGB widths down to 24
            {
                r = r / 4;
                g = g / 4;
                b = b / 4;
            }
            else if (Width == 36)//This is to bring RGB widths down to 24
            {
                r = r / 16;
                g = g / 16;
                b = b / 16;
            }
            else if (Width == 48)//This is to bring RGB widths down to 24
            {
                r = r / 256;
                g = g / 256;
                b = b / 256;
            }
            if (w != (int)WidthnumericUpDown.Value)
            {
                bmp.SetPixel(w, h, Color.FromArgb(r, g, b));
                w++;
                this.progressBar1.Increment(1);
            }
        }
        private void placeYCbCr444(ref int w, ref int h, ulong pixel, ref Bitmap bmp)
        {
            int component = Width / 3;
            int Cb = (int)(pixel >> component * 2);
            Cb = Cb & (int)getbitmask(Width);
            int Y = (int)(pixel >> component);
            Y = Y & (int)getbitmask(Width);
            int Cr = (int)(pixel);
            Cr = Cr & (int)getbitmask(Width);
            if (Width == 30) //This is to bring RGB widths down to 24
            {
                Cr = Cr / 4;
                Y = Y / 4;
                Cb = Cb / 4;
            }
            else if (Width == 36)//This is to bring RGB widths down to 24
            {
                Cr = Cr / 16;
                Y = Y / 16;
                Cb = Cb / 16;
            }
            else if (Width == 48)//This is to bring RGB widths down to 24
            {
                Cr = Cr / 256;
                Y = Y / 256;
                Cb = Cb / 256;
            }
            YCbCr_to_RGB(ref bmp, Y, Cb, Cr, w, h);
            w++;
            this.progressBar1.Increment(1);
        }
        private void placeYCbCr422(ref int w, ref int h, ulong pixel, ref Bitmap bmp)
        {
            int component = Width / 2;
            int mask = getbitmaskYCbCr();
            int Cb = (int)(pixel >> component * 3);
            Cb = Cb & mask;
            int Y = (int)(pixel >> component * 2);
            Y = Y & mask;
            int Cr = (int)(pixel >> component);
            Cr = Cr & mask;
            int Y1 = (int)pixel;
            Y1 = Y1 & mask;

            if (Width == 20)
            {
                Y /= 4;
                Cb /= 4;
                Cr /= 4;
                Y1 /= 4;
            }

            else if (Width == 24)
            {
                Y /= 16;
                Cb /= 16;
                Cr /= 16;
                Y1 /= 16;
            }

            else if (Width == 32)
            {
                Y /= 256;
                Cb /= 256;
                Cr /= 256;
                Y1 /= 256;
            }

            YCbCr_to_RGB(ref bmp, Y, Cb, Cr, w, h);
            w++;
            this.progressBar1.Increment(1);
            YCbCr_to_RGB(ref bmp, Y1, Cb, Cr, w, h);
            w++;
            this.progressBar1.Increment(1);
        }
        /// <summary>
        /// Organizing the pixel data for the YCbCr format
        /// </summary>
        /// <param name="lanes"></param>
        /// <param name="width"></param>
        private void getpixeldataYCbCr422(int lanes, int width) //This is done with same logic as RGB, just different format and the comment below
        {
            Bitmap bmp = new Bitmap((int)WidthnumericUpDown.Value, (int)HeightnumericUpDown.Value);
            string VBS = "0x4A";
            string HBE = "0x15";
            int w = 0;
            int h = 0;
            if (lanes == 1) //If the width is 16, if there is one lane, the width most be doubled to get Cr and Y1 components.
                width *= 2;
            List<byte[]> pixeldata = new List<byte[]>();
            int vchannel = getvc();
            int i = (int)StateNumberNumericUpDown.Value;
            bool check = false;
            check = checkforframe(ref i, vchannel);
            bool flag = true;
            int tracker = 0;
            byte[] dum = null;
            int lanewidth = 8;
            int dummywidth = 0;
            int count = 0;
            int states = Convert.ToInt32(m_IProbe.GetNumberOfStates(vchannel));
            while (i != states)
            {
                dum = (m_IProbe.GetStateData(vchannel, i));
                StringBuilder sb = Geteventcode(dum);
                if (sb.ToString() == "0x88" || sb.ToString() == "0xC8")
                {
                    if (w == (int)WidthnumericUpDown.Value) //If w equals the inputed width, width most be to small
                    {
                        string e = "More pixels found, width to small";
                        Runerror error = new Runerror(e);
                        error.Show();
                        break;
                    }
                    if (h == (int)HeightnumericUpDown.Value)
                    {
                        string e = "Not enough lines in frame, height to small";
                        Runerror error = new Runerror(e);
                        error.Show();
                        break;
                    }
                    if (flag == true)
                        dummywidth += lanewidth;
                    else
                        flag = true;
                    pixeldata.Add(dum);
                    tracker++;
                    count++;
                    if (dummywidth >= width)
                    {
                        if (dummywidth != width)
                        {
                            i--;
                            dummywidth = dummywidth - width;
                            flag = false;
                        }
                        else if (dummywidth == width)
                            dummywidth = 0;
                        createpixels(ref bmp, pixeldata, width, (tracker - count), lanes, dummywidth, ref w, ref h);
                        count = 0;
                    }
                }
                else if (sb.ToString() == HBE) //HBE
                {
                    if (w < (int)WidthnumericUpDown.Value)
                    {
                        string e = "Width to big, not enough pixels to fill a line";
                        Runerror error = new Runerror(e);
                        error.Show();
                        break;
                    }
                    h++;
                    w = 0;
                }
                else if (sb.ToString() == VBS) //VBE
                {
                    if (h < (HeightnumericUpDown.Value - 1))
                    {
                        string e = "Input height to big, more lines than pixels";
                        Runerror error = new Runerror(e);
                        error.Show();
                    }
                    if (h > (HeightnumericUpDown.Value - 1))
                    {
                        string e = "Input height to small, not enough lines in the frame, or VBS not in Channel";
                        Runerror error = new Runerror(e);
                        error.Show();
                    }
                    break;
                }
                i++;
            }

            PictureBox.Image = bmp;
        }

        /// <summary>
        /// Storing pixel data for YCbCr420, requires two pixel lists
        /// </summary>
        /// <param name="lanes"></param>
        /// <param name="width"></param>
        private void getpixeldataYCbCr420(int lanes, int width)
        {
            Bitmap bmp = new Bitmap((int)WidthnumericUpDown.Value, (int)HeightnumericUpDown.Value);
            string VBS = "0x4A";
            string HBE = "0x15";
            int w = 0;
            int h = 0;
            width *= 2;
            int switchlist = 1;
            List<byte[]> pixeldata_odd = new List<byte[]>(); //List for the odd lines
            List<byte[]> pixeldata_even = new List<byte[]>(); //List for the even lines
            int vchannel = getvc();
            int i = (int)StateNumberNumericUpDown.Value;
            bool check = false;
            check = checkforframe(ref i, vchannel);
            bool flag = true;
            int tracker = 0;
            byte[] dum = null;
            int states = (int)m_IProbe.GetNumberOfStates(vchannel);
            int lanewidth = 8;
            int dummywidth = 0;
            int count = 0;
            while (i != states)
            {
                if (h % 2 == 0) //Switchlist determines which list the state data will be stored in
                    switchlist = 0;
                else
                    switchlist = 1;
                dum = (m_IProbe.GetStateData(vchannel, i));
                StringBuilder sb = Geteventcode(dum);
                if (sb.ToString() == "0x88" || sb.ToString() == "0xC8")
                {
                    if (w == (int)WidthnumericUpDown.Value) //If w equals the inputed width, width most be to small
                    {
                        string e = "More pixels found, width to small";
                        Runerror error = new Runerror(e);
                        error.Show();
                        break;
                    }
                    if (h == (int)HeightnumericUpDown.Value)
                    {
                        string e = "Not enough lines in frame, height to small";
                        Runerror error = new Runerror(e);
                        error.Show();
                        break;
                    }
                    if (flag == true)
                        dummywidth += lanewidth;
                    else
                        flag = true;
                    if (switchlist == 0)
                        pixeldata_even.Add(dum);
                    else
                        pixeldata_odd.Add(dum);
                    tracker++;
                    count++;
                    if (dummywidth >= width)
                    {
                        if (dummywidth != width)
                        {
                            i--;
                            dummywidth = dummywidth - width;
                            flag = false;
                        }
                        else if (dummywidth == width)
                            dummywidth = 0;
                        if (h % 2 == 1) //The even list must be complete, so pixels will only be placed when the odd list is being put together.
                        {
                            if (w != (int)WidthnumericUpDown.Value)
                                createpixelsYCbCr420(ref bmp, pixeldata_even, pixeldata_odd, width, (tracker - count), lanes, dummywidth, ref w, h);
                        }
                        count = 0;
                    }
                }
                else if (sb.ToString() == HBE) //HBE
                {
                    if (h % 2 == 1)
                    {
                        if (w < (int)WidthnumericUpDown.Value)
                        {
                            string e = "Width to big, not enough pixels to fill a line";
                            Runerror error = new Runerror(e);
                            error.Show();
                            break;
                        }
                    }
                    h++;
                    dummywidth = 0;
                    count = 0;
                    tracker = 0;
                    w = 0;
                    if (h % 2 == 0)
                    {
                        pixeldata_even.Clear();
                        pixeldata_odd.Clear();
                    }
                }
                if (sb.ToString() == VBS) //VBE
                {
                    if (h < (HeightnumericUpDown.Value - 1))
                    {
                        string e = "Input height to big, more lines than pixels";
                        Runerror error = new Runerror(e);
                        error.Show();
                    }
                    if (h > (HeightnumericUpDown.Value - 1))
                    {
                        string e = "Input height to small, not enough lines in the frame, or VBS not in Channel";
                        Runerror error = new Runerror(e);
                        error.Show();
                    }
                    break;
                }
                i++;
            }
            PictureBox.Image = bmp;
        }
        /// <summary>
        /// Accquiring the pixel data to create pixels and stores them in a list
        /// </summary>
        /// <param name="lanes"></param>
        /// <param name="width"></param>
        private void getpixeldata(int lanes, int width)
        {
            Bitmap bmp = new Bitmap((int)WidthnumericUpDown.Value, (int)HeightnumericUpDown.Value);
            string VBS = "0x4A";
            string HBE = "0x15";
            int w = 0;
            int h = 0;
            List<byte[]> pixeldata = new List<byte[]>();
            int vchannel = getvc();
            int component = width / 3;
            bool flag = true;
            int i = (int)StateNumberNumericUpDown.Value;
            bool check = false;
            check = checkforframe(ref i, vchannel);
            int tracker = 0;
            byte[] dum = null;
            int states = (int)m_IProbe.GetNumberOfStates(vchannel);
            int lanewidth = 8;
            int dummywidth = 0; //Adding by 8 each time, this is for a check later in the program.
            int count = 0; //counts number of states before enough are found for a pixel state
            while (i != states)
            {
                dum = (m_IProbe.GetStateData(vchannel, i));
                StringBuilder sb = Geteventcode(dum);
                if (sb.ToString() == "0x88" || sb.ToString() == "0xC8") //If pixel state
                {
                    if (w == (int)WidthnumericUpDown.Value) //If w equals the inputed width, width most be to small
                    {
                        string e = "Not enough pixel found to make up line, check MSA, width should equal" + w.ToString();
                        Runerror error = new Runerror(e);
                        error.Show();
                        break;
                    }
                    if (h == (int)HeightnumericUpDown.Value)
                    {
                        string e = "Not enough lines in frame, height to small";
                        Runerror error = new Runerror(e);
                        error.Show();
                        PictureBox.Image = bmp;
                        break;
                    }
                    if (flag == true) //Adding bits to dummywidth
                        dummywidth += lanewidth;
                    else
                        flag = true;
                    pixeldata.Add(dum);
                    tracker++;
                    count++;
                    if (dummywidth >= width)
                    {
                        if (dummywidth != width) //if the dummywidth is more than the width, subtract it. Must be kept track for the reminder.
                        {
                            i--; //This state is going to be put into the statedata list twice, thats why flag will also be false, so dummywidth is not added a second time
                            dummywidth = dummywidth - width;
                            flag = false;
                        }
                        else if (dummywidth == width) //No remander, all the bits will be used and not shift will be nessacary
                            dummywidth = 0;
                        if (w != (int)WidthnumericUpDown.Value) //In case the enough pixels are placed before the lanes are finished
                            createpixel(ref bmp, pixeldata, width, (tracker - count), lanes, dummywidth, ref w, h);
                        count = 0;
                    }
                }
                else if (sb.ToString() == HBE) //If Horizontal Blanking End
                {
                    if (w < (int)WidthnumericUpDown.Value) //If w equals the inputed width, width most be to small
                    {
                        string e = "All pixels found on line found, check MSA correct width should equal" + w.ToString();
                        Runerror error = new Runerror(e);
                        error.Show();
                        PictureBox.Image = bmp;
                        break;
                    }
                    h++;
                    w = 0;
                    tracker = 0;
                    dummywidth = 0;
                    pixeldata.Clear();
                    count = 0;
                }
                else if (sb.ToString() == VBS) //If Vertical Blanking Start
                {
                    if (h < (HeightnumericUpDown.Value - 1))
                    {
                        string e = "Input height to big, more lines than pixels";
                        Runerror error = new Runerror(e);
                        error.Show();
                    }
                    if (h > (HeightnumericUpDown.Value - 1))
                    {
                        string e = "Input height to small, not enough lines in the frame, or VBS not in Channel";
                        Runerror error = new Runerror(e);
                        error.Show();
                    }
                    break;
                }
                i++;
            }
            PictureBox.Image = bmp;
        }


        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods

    }
}

