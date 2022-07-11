using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
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
using System.Xml;
using System.Collections;
using System.IO;

namespace FrameVideoRendererClassLibrary
{
    public partial class PictureUserControl : UserControl
    {
        /// <summary>
        /// Cancellation Token for the cancel button
        /// </summary>
        private class GetAudioDataStateObject
        {
            public CancellationToken Token { get; set; }

            public GetAudioDataStateObject(CancellationToken ct)
            {
                Token = ct;
            }
        }

        private class PictureLineData
        {
            private int Line { get; set; }
            public int getLine()
            {
                return Line;
            }
            private int Pixels { get; set; }
            public int getPixels()
            {
                return Pixels;
            }

            private string[] Error { get; set; }
            public string[] getError()
            {
                return Error;
            }

            private int Startstate { get; set; }
            public int getStartstate()
            {
                return Startstate;
            }

            private int Endstate { get; set; }
            public int getEndstate()
            {
                return Endstate;
            }

            public PictureLineData(int line, int pixels, string[] error, int start, int end)
            {
                Line = line;
                Pixels = pixels;
                Error = error;
                Startstate = start;
                Endstate = end;
            }
        }

        #region Members
        private DP12SST m_DP12SSTProbe = null;
        private DP12MST m_DP12MSTProbe = null;
        private DP14SST m_DP14SSTProbe = null;
        private DP14MST m_DP14MSTProbe = null;
        IProbeMgrGen2 m_IProbe = null;              // this sets us up for polymorphism...  because DP12MST inherits the FPSProbeMgrGen2 interface,
                                                    // then all the different probe versions can be assigned to this one base type.
        private List<PixelData> pixelList = new List<PixelData>();


        private int m_width = 0;
        private int PixelWidth { get { return m_width; } set { m_width = value; } }

        private string m_format = "";
        private string Format { get { return m_format; } set { m_format = value; } }

        private string m_protocol = "";
        private string Protocol { get { return m_protocol; } set { m_protocol = value; } }

        private long m_mask = 0;
        private long Mask { get { return m_mask; } set { m_mask = value; } }
        private int m_lanes = 0;
        private int Lanes { get { return m_lanes; } set { m_lanes = value; } }
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
        private int m_startstate = 0;
        public int Startstate { get { return m_startstate; } set { m_startstate = value; } }
        private int m_frameid = 0;
        public int Frameid { get { return m_frameid; } set { m_frameid = value; } }
        private int m_endstate = 0;
        public int Endstate { get { return m_endstate; } set { m_endstate = value; } }
        private int m_pixelstate = 0;
        public int Pixelstate { get { return m_pixelstate; } set { m_pixelstate = value; } }
        private int m_statesbeforetrigger = 0;
        public int StatesBeforeTrigger { get { return m_statesbeforetrigger; } set { m_statesbeforetrigger = value; } }
        private string m_framestring = "";
        public string Framestring { get { return m_framestring; } set { m_framestring = value; } }
        private int m_virtualchannel = 0;
        public int Virtualchannel { get { return m_virtualchannel; } set { m_virtualchannel = value; } }
        private int m_lanewidth = 0;
        public int Lanewidth { get { return m_lanewidth; } set { m_lanewidth = value; } }
        private string m_file = "";
        public string PathFile { get { return m_file; } set { m_file = value; } }

        private string m_test = "";
        public string Test { get { return m_test; } set { m_test = value; } }

        private Metadata m_data = null;
        public Metadata Data { get { return m_data; } set { m_data = value; } }

        private int m_paintedpixels = 0;
        public int PaintedPixels { get { return m_paintedpixels; } set { m_paintedpixels = value; } }

        public event PaintingEvent PaintingPicture;
        public event GetPixelData GettingPixelData;
        public event GetMetaData GettingMetaData;
        public event RequestPictureEvent SendingPicture;
        public event CheckReferenceEvent CheckReference;
        public event CheckPictureEvent CheckPicture;

        private CancellationTokenSource m_tokenSource = null;
        private CancellationToken m_token = CancellationToken.None;

        #endregion
        public PictureUserControl()
        {
            InitializeComponent();
            DefaultSetup();
        }

        #region Eventhandlers
        /// <summary>
        /// Gathers information from Meta data and User inputed button. Starts painting process.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Paintbutton_Click(object sender, EventArgs e)
        {
            this.CancelButton.Enabled = true;
            SaveMetaData();
            this.toolStripProgressBar1.Value = 0;
            Bitmap picture;
            PixelWidth = getpixelwidth();
            Format = getformat();
            Mask = getmask(PixelWidth);
            xmlreader();
            if ((int)WidthnumericUpDown.Value == 0 || (int)HeightnumericUpDown.Value == 0) //If nothing set for NumericUpDowns return error
            {
                string err = "Set bitmap Dimentions";
                Runerror error = new Runerror(err);
                error.Show();
                Format = " ";
            }
            else
            {
                picture = new Bitmap((int)WidthnumericUpDown.Value, (int)HeightnumericUpDown.Value);
                this.toolStripProgressBar1.Maximum = ((int)WidthnumericUpDown.Value * (int)HeightnumericUpDown.Value);
                this.toolStripStatusLabel1.Text = "Gathering Pixel Data";
                this.statusStrip1.Update();
            }
            if (ReferencecheckBox.Checked == true)
            {
                CheckReferenceArgs args = null;
                if (CheckReference != null)
                {
                    args = new CheckReferenceArgs(true);
                    CheckReference(this, args);
                    if (args.ReferenceCheck == false)
                    {
                        string err = "Already a Reference Frame Set";
                        Runerror error = new Runerror(err);
                        error.Show();
                        Format = " ";
                    }
                }
            }
            if (CheckPicture != null && Format != " ")
            {
                CheckPictureArgs args = null;
                args = new CheckPictureArgs(true,Frameid);
                CheckPicture(this, args);
            }
            if (Format == "RGB" || Format == "YCbCr444" || Format == "YCbCr422" || Format == "YCbCr420")
            {
                getpicture(Format, Lanes, PixelWidth, Startstate, Pixelstate, Endstate);
            }
        }
        /// <summary>
        /// Will go through the frame, count pixels on each line, paint them, return with data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnalyseLinesButton_Click(object sender, EventArgs e)
        {
            this.CancelButton.Enabled = true;
            Metadata data = null;
            GettingMetaDataArgs args = null;
            if (GettingMetaData != null) //Raise Event to collect meta data
            {
                args = new GettingMetaDataArgs(data);
                GettingMetaData(this, args);
            }
            Data = args.MetaData;
            Protocol = args.MetaData.Protocol; //Saving Metadata to variables 
            Virtualchannel = args.MetaData.VirtualChannel;
            Lanes = args.MetaData.Lanes;
            Frameid = args.MetaData.Frameid;
            StatesBeforeTrigger = args.MetaData.StatesBeforeTrig;
            int pixelstate = args.MetaData.FirstPixelState;
            int startstate = args.MetaData.StartState;
            int endstate = args.MetaData.EndState;
            this.toolStripProgressBar1.Value = 0;
            this.toolStripProgressBar1.Maximum = endstate - startstate;
            PictureBox.Image = null;
            Bitmap picture;
            PixelWidth = getpixelwidth();
            Format = getformat();
            Mask = getmask(PixelWidth);
            xmlreader();
            if (Format == "RGB" || Format == "YCbCr444" || Format == "YCbCr422" || Format == "YCbCr420")
            {
                this.toolStripProgressBar1.Maximum = ((int)WidthnumericUpDown.Value * (int)HeightnumericUpDown.Value);
                this.toolStripStatusLabel1.Text = "Analyzing Picture";
                this.statusStrip1.Update();
                AnalysePicture(Format, Lanes, PixelWidth, startstate, pixelstate, endstate);
            }
        }

        /// <summary>
        /// Allows user to save picture to anywhere in their computer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Savebutton_Click(object sender, EventArgs e)
        {
            if (PictureBox.Image != null)
            {
                SaveFileDialog sfile = new SaveFileDialog();
                sfile.Filter = "JPEG files(*.jpeg)|*.jpeg| PNG files(*.png)|*.png";
                if (DialogResult.OK == sfile.ShowDialog())
                {
                    this.PictureBox.Image.Save(sfile.FileName);
                }
            }
        }
        /// <summary>
        /// Clears the picture box, everything goes back to default
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        }
        /// <summary>
        /// For canceling the task, cancel painting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (m_tokenSource != null)
                m_tokenSource.Cancel();
        }
        /// <summary>
        /// If RGB is Checked, disable YCbCr formats
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RGBButton_CheckedChanged(object sender, EventArgs e)
        {
            if (RGBButton.Checked == false && YCbCr444Button.Checked == false)
            {
                RGBGroupbox.Enabled = false;
                RGB18button.Checked = false;
                RGB24button.Checked = false;
                RGB30button.Checked = false;
                RGB36button.Checked = false;
                RGB48button.Checked = false;
            }
            else if (RGBButton.Checked == true || YCbCr444Button.Checked == true)
            {
                RGBGroupbox.Enabled = true;
            }
            if (YCbCr422Button.Checked == true)
            {
                YCbCr422groupbox.Enabled = true;
            }
            else if (YCbCr420Button.Checked == true)
            {
                YCbCr420groupbox.Enabled = true;
            }
        }
        /// <summary>
        /// Same as RGBButton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void YCbCr444Button_CheckedChanged(object sender, EventArgs e)
        {
            if (RGBButton.Checked == false && YCbCr444Button.Checked == false)
            {
                RGBGroupbox.Enabled = false;
                RGB18button.Checked = false;
                RGB24button.Checked = false;
                RGB30button.Checked = false;
                RGB36button.Checked = false;
                RGB48button.Checked = false;
            }
            else if (RGBButton.Checked == true || YCbCr444Button.Checked == true)
            {
                RGBGroupbox.Enabled = true;
            }
            if (YCbCr422Button.Checked == true)
            {
                YCbCr422groupbox.Enabled = true;
            }
            else if (YCbCr420Button.Checked == true)
            {
                YCbCr420groupbox.Enabled = true;
            }
        }
        /// <summary>
        /// Enable User YCbCr options
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void YCbCr422Button_CheckedChanged(object sender, EventArgs e)
        {
            if (RGBButton.Checked == true || YCbCr444Button.Checked == true || YCbCr420Button.Checked == true)
            {
                YCbCr422groupbox.Enabled = false;
                YCbCr16button.Checked = false;
                YCbCr20button.Checked = false;
                YCbCr24button.Checked = false;
                YCbCr32button.Checked = false;
            }
            else if (YCbCr422Button.Checked == true)
            {
                YCbCr422groupbox.Enabled = true;
            }
        }
        /// <summary>
        /// Enable User YCbCr options
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void YCbCr420Button_CheckedChanged(object sender, EventArgs e)
        {
            if (RGBButton.Checked == true || YCbCr444Button.Checked == true || YCbCr422Button.Checked == true)
            {
                YCbCr420groupbox.Enabled = false;
                YCbCr15button.Checked = false;
                YCbCr18button.Checked = false;
                YCbCr420_24button.Checked = false;
            }
            else if (YCbCr420Button.Checked == true)
            {
                YCbCr420groupbox.Enabled = true;
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Disable or enable all controls in user control while painting, except cancel button.
        /// </summary>
        private void enable(bool flag)
        {
            foreach (Control control in this.Controls)
            {
                //Ignore the following controls
                if (control.Name == "CancelButton" || control.Name == "toolStripStatusLabel1" || control.Name == "toolStripProgressBar1" || control.Name == "statusStrip1")
                {
                    //Skip these controls
                }
                else
                {
                    control.Enabled = flag;
                }
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
        /// <summary>
        /// Save Tracebuffer data for use later
        /// </summary>
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
        /// Returns the pixel format being used.
        /// </summary>
        /// <returns></returns>
        private string getformat()
        {
            string format = "";
            if (RGBButton.Checked)
                format = "RGB";
            else if (YCbCr444Button.Checked)
                format = "YCbCr444";
            else if (YCbCr422Button.Checked)
                format = "YCbCr422";
            else if (YCbCr420Button.Checked)
                format = "YCbCr420";
            return format;
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
        /// The DefaultSetup that first appears and when the clear button is pushed.
        /// </summary>
        private void DefaultSetup()
        {
            RGBButton.Checked = true;
            RGB24button.Checked = true;
            YCbCr422groupbox.Enabled = false;
            YCbCr420groupbox.Enabled = false;

        }

        private void SaveMetaData()
        {
            Metadata data = null;
            GettingMetaDataArgs args = null;
            if (GettingMetaData != null) //Raise Event to collect meta data
            {
                args = new GettingMetaDataArgs(data);
                GettingMetaData(this, args);
            }
            Protocol = args.MetaData.Protocol; //Saving Metadata to variables 
            Virtualchannel = args.MetaData.VirtualChannel;
            Lanes = args.MetaData.Lanes;
            Frameid = args.MetaData.Frameid;
            Pixelstate = args.MetaData.FirstPixelState;
            Startstate = args.MetaData.StartState;
            Endstate = args.MetaData.EndState;
            StatesBeforeTrigger = args.MetaData.StatesBeforeTrig;
        }
        /// <summary>
        /// Return tracebuffer data of single state
        /// </summary>
        /// <param name="index"></param>
        /// <param name="statesChunk"></param>
        /// <param name="stateData"></param>
        /// <returns></returns>
        private byte[] getStateDataFromChunk(int index, List<byte> statesChunk, ref byte[] stateData)
        {

            for (int i = 0; i < stateData.Length; i++)
                stateData[i] = statesChunk[index + i];

            return stateData;
        }
        /// <summary>
        /// Convert YCbCr to RGB for picturebox
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="y"></param>
        /// <param name="cb"></param>
        /// <param name="cr"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        private void YCbCr_to_RGB(ref Bitmap bmp, int y, int cb, int cr, int w, int h, int pixelstart, int pixelend)
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

            if (w < (int)(WidthnumericUpDown.Value))
            {
                if (h < (int)(HeightnumericUpDown.Value))
                {
                    bmp.SetPixel(w, h, Color.FromArgb(Convert.ToInt32(r), Convert.ToInt32(g), Convert.ToInt32(b)));
                    PixelData p = new PixelData(h, w, pixelstart, pixelend, Convert.ToInt32(r), Convert.ToInt32(g), Convert.ToInt32(b), true);
                    pixelList.Add(p);
                    PaintedPixels++;
                }
            }
        }

        private void YCbCr_to_RGB_Analyze(int y, int cb, int cr, int w, int h)
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

            PixelData p = new PixelData(h, w, 0, 0, Convert.ToInt32(r), Convert.ToInt32(g), Convert.ToInt32(b), true);
            pixelList.Add(p);
            PaintedPixels++;

        }
        /// <summary>
        /// Async function setting task to collect pixel data.
        /// </summary>
        /// <param name="lanes"></param>
        /// <param name="width"></param>
        /// <param name="startstate"></param>
        /// <param name="pixelstate"></param>
        /// <param name="endstate"></param>
        private async void getpicture(string format, int lanes, int width, int startstate, int pixelstate, int endstate)
        {
            if (PaintingPicture != null) //Raise event, disable all other forms
                PaintingPicture(this, new PaintingEventArgs(true, Frameid));
            enable(false);

            m_tokenSource = new CancellationTokenSource();
            m_token = m_tokenSource.Token;

            Task<Bitmap>[] tasks = new Task<Bitmap>[1];
            ThreadLocal<GetAudioDataStateObject> tls = new ThreadLocal<GetAudioDataStateObject>();

            //Three different paths that the task could go down.
            if (format == "RGB" || Format == "YCbCr444")
            {
                tasks[0] = new Task<Bitmap>((stateObject) =>
                {
                    tls.Value = (GetAudioDataStateObject)stateObject;
                    ((GetAudioDataStateObject)tls.Value).Token.ThrowIfCancellationRequested();
                    return getpixeldata(lanes, width, startstate, pixelstate, endstate, tls.Value.Token);
                }, new GetAudioDataStateObject(m_token));
            }
            else if (format == "YCbCr422")
            {
                tasks[0] = new Task<Bitmap>((stateObject) =>
                {
                    tls.Value = (GetAudioDataStateObject)stateObject;
                    ((GetAudioDataStateObject)tls.Value).Token.ThrowIfCancellationRequested();
                    return getpixeldataYCbCr422(lanes, width, startstate, pixelstate, endstate, tls.Value.Token);
                }, new GetAudioDataStateObject(m_token));

            }
            else if (format == "YCbCr420")
            {
                tasks[0] = new Task<Bitmap>((stateObject) =>
                {
                    tls.Value = (GetAudioDataStateObject)stateObject;
                    ((GetAudioDataStateObject)tls.Value).Token.ThrowIfCancellationRequested();
                    return getpixeldataYCbCr420(lanes, width, startstate, pixelstate, endstate, tls.Value.Token);
                }, new GetAudioDataStateObject(m_token));
            }

            foreach (Task t in tasks)
                t.Start();
            await Task.WhenAll(tasks);

            if (PaintingPicture != null) //Raise event, enable other forms since task done
                PaintingPicture(this, new PaintingEventArgs(false, Frameid));
            enable(true);
            this.PictureBox.Enabled = true;
            this.PictureBox.Image = tasks[0].Result; //Bitmap from task put in picturebox
            this.toolStripStatusLabel1.Text = "Ready";
            this.toolStripProgressBar1.Value = 0;
            this.statusStrip1.Update();
            pixelList.Clear();
            PaintedPixels = 0;
            //pixelList.Clear(); //After Event is raised
            //foreach (PixelData pixel in pixelList)
            //{
            //    string text = "Y:" + pixel.YLocation.ToString() + " X:" + pixel.XLocation.ToString() + " R:" + pixel.RValue.ToString() + " G:" + pixel.GValue.ToString() + " B:" + pixel.BValue.ToString() + "\n";
            //    PixelLineDataRichText.AppendText(text);
            //}
        }
        /// <summary>
        /// Async function setting task count number of pixels per line
        /// </summary>
        /// <param name="format"></param>
        /// <param name="lanes"></param>
        /// <param name="width"></param>
        /// <param name="startstate"></param>
        /// <param name="pixelstate"></param>
        /// <param name="endstate"></param>
        private async void AnalysePicture(string format, int lanes, int width, int startstate, int pixelstate, int endstate)
        {
            if (PaintingPicture != null) //Raise event, disable all other forms
                PaintingPicture(this, new PaintingEventArgs(true, Frameid));
            enable(false);

            m_tokenSource = new CancellationTokenSource();
            m_token = m_tokenSource.Token;

            Task<List<PictureLineData>>[] tasks = new Task<List<PictureLineData>>[1];
            ThreadLocal<GetAudioDataStateObject> tls = new ThreadLocal<GetAudioDataStateObject>();

            //Three different paths that the task could go down.
            if (format == "RGB" || format == "YCbCr444")
            {
                tasks[0] = new Task<List<PictureLineData>>((stateObject) =>
               {
                   tls.Value = (GetAudioDataStateObject)stateObject;
                   ((GetAudioDataStateObject)tls.Value).Token.ThrowIfCancellationRequested();
                   return AnalysePixels(lanes, width, startstate, pixelstate, endstate, tls.Value.Token);
               }, new GetAudioDataStateObject(m_token));
            }
            else if (format == "YCbCr422")
            {
                tasks[0] = new Task<List<PictureLineData>>((stateObject) =>
               {
                   tls.Value = (GetAudioDataStateObject)stateObject;
                   ((GetAudioDataStateObject)tls.Value).Token.ThrowIfCancellationRequested();
                   return AnalysePixels422(lanes, width, startstate, pixelstate, endstate, tls.Value.Token);
               }, new GetAudioDataStateObject(m_token));

            }
            else if (format == "YCbCr420")
            {
                tasks[0] = new Task<List<PictureLineData>>((stateObject) =>
               {
                   tls.Value = (GetAudioDataStateObject)stateObject;
                   ((GetAudioDataStateObject)tls.Value).Token.ThrowIfCancellationRequested();
                   return AnalysePixels420(lanes, width, startstate, pixelstate, endstate, tls.Value.Token);
               }, new GetAudioDataStateObject(m_token));
            }

            foreach (Task t in tasks)
                t.Start();
            await Task.WhenAll(tasks);

            if (PaintingPicture != null) //Raise event, enable other forms since task done
                PaintingPicture(this, new PaintingEventArgs(false, Frameid));
            enable(true);

            string text = StorePictureLaneData(tasks[0].Result);
            AnalyzeForm Analyze = new AnalyzeForm(text);
            //this.Controls.Add(Analyze);
            Analyze.Show();
            this.toolStripStatusLabel1.Text = "Ready";
            this.toolStripProgressBar1.Value = 0;
            this.statusStrip1.Update();
            pixelList.Clear();
            PaintedPixels = 0;
        }

        private string StorePictureLaneData(List<PictureLineData> data)
        {
            PixelLineDataRichText.Text = "";
            string linedata = "";
            foreach (PictureLineData line in data)
            {
                string text = "Line " + line.getLine().ToString() + ":\n   Number of Pixels:" + line.getPixels().ToString() + "\n";
                if (line.getError().Length == 0)
                {
                    text += "   Error: None" + "\n   StateRange:" + (line.getStartstate() - StatesBeforeTrigger).ToString() + " - " + (line.getEndstate() - StatesBeforeTrigger).ToString() + "\n";
                }
                else
                {
                    int count = 1;
                    foreach (string error in line.getError())
                    {
                        text += "   Error" + count.ToString() + ": " + error + "\n";
                        count++;
                    }
                    text += "   StateRange:" + (line.getStartstate() - StatesBeforeTrigger).ToString() + " - " + (line.getEndstate() - StatesBeforeTrigger).ToString() + "\n";
                }
                PixelLineDataRichText.AppendText(text);
                linedata += text;
            }
            return linedata;
        }

        /// <summary>
        /// Accquiring the pixel data to create pixels and stores them in a list
        /// </summary>
        /// <param name="lanes"></param>
        /// <param name="width"></param>
        private Bitmap getpixeldata(int lanes, int width, int startstate, int pixelstate, int endstate, CancellationToken token)
        {
            //Check if Cancel before even starting
            if (token.IsCancellationRequested == true)
            {
                token.ThrowIfCancellationRequested();
            }
            int pixels = 0;
            Bitmap bmp = new Bitmap((int)WidthnumericUpDown.Value, (int)HeightnumericUpDown.Value); //Returning with this
            string VBS = "0x4A";
            string HBE = "0x15";
            int w = 0; //Bitmap height
            int h = 0; //Bitmap width
            List<byte[]> pixeldata = new List<byte[]>();
            int vchannel = Virtualchannel;
            List<byte> Statechunk = new List<byte>();
            int component = width / 3;
            bool flag = true;
            int stateindex = pixelstate;
            bool check = false;
            int tracker = 0;
            int states = (endstate - startstate); //Number of states to get frame
            int lanewidth = 8;
            int dummywidth = 0; //Adding by 8 each time, this is for a check later in the program.
            int count = 0; //counts number of states before enough are found for a pixel state
            int chunkSize = 4096;
            int stateDataLength = 16;
            byte[] stateData = new byte[stateDataLength];
            bool errorcheck = false;
            bool bmpshow = true;
            int pixelstart = 0;
            int pixelend = 0;
            int HDCPCheck = 0;
            List<PixelData> picturelist = null;
            for (int s = 0; s < states; stateindex += chunkSize, s += chunkSize) //s is just a counter for the section of data this frame is assigned too
            {
                GettingPixelDataArgs args = null;
                if (GettingPixelData != null) //Raise event, get data from m_IProbe
                {
                    args = new GettingPixelDataArgs(4096, vchannel, stateindex, Statechunk);
                    GettingPixelData(this, args);
                }
                Statechunk = args.Statedata;
                chunkSize = Statechunk.Count / stateDataLength;
                if (chunkSize > 0) //Should only be 0 if no more states
                {
                    for (int chunkIndex = 0; (chunkIndex < chunkSize) && (stateindex + chunkIndex) < endstate + chunkIndex; chunkIndex += 1) //Get state data from statechunk
                    {
                        getStateDataFromChunk(chunkIndex * 16, Statechunk, ref stateData); //Get individual state from statechunk
                        StringBuilder sb = Geteventcode(stateData);
                        if (sb.ToString() == "0x88" || sb.ToString() == "0xC8") //If pixel state
                        {
                            if (w >= (int)WidthnumericUpDown.Value) //If w equals the inputed width, width most be to small
                            {
                                string e = "Not enough pixel found to fill a line, check MSA, width should equal MSA width.";
                                Runerror error = new Runerror(e);
                                this.Invoke(new Action(() =>
                                {
                                    error.Show();
                                }));
                                errorcheck = true;
                                bmpshow = false;
                                break;
                            }
                            if (h >= (int)HeightnumericUpDown.Value) //If h equals the inputed height, the height must be to small
                            {
                                string e = "Not enough lines in frame, height to small, check the MSA, Height in the MSA must be same as input Height";
                                Runerror error = new Runerror(e);
                                this.Invoke(new Action(() =>
                                {
                                    error.Show();
                                }));
                                errorcheck = true;
                                break;
                            }
                            if (flag == true) //Adding bits to dummywidth
                            {
                                if (dummywidth < lanewidth)
                                {
                                    if (pixelstart != pixelend || pixelstart == 0) //if the pixelstart is set in the else statement, then that means that it will be equal to the pixelend. 
                                        pixelstart = (chunkIndex + stateindex) - StatesBeforeTrigger; // +1 to include trigger state
                                }
                                dummywidth += lanewidth;
                            }
                            else
                            {
                                pixelstart = (chunkIndex + stateindex) - StatesBeforeTrigger; // +1 to include trigger state
                                flag = true;
                            }
                            pixeldata.Add(stateData);
                            stateData = new byte[stateDataLength];
                            tracker++;
                            count++;
                            if (dummywidth >= width)
                            {
                                if (dummywidth != width) //if the dummywidth is more than the width, subtract it. Must be kept track for the reminder.
                                {
                                    pixelend = (chunkIndex + stateindex) - StatesBeforeTrigger; // +1 to include trigger state
                                    chunkIndex--; //This state is going to be put into the statedata list twice, thats why flag will also be false, so dummywidth is not added a second time
                                    dummywidth = dummywidth - width;
                                    flag = false;
                                }
                                else if (dummywidth == width) //No remander, all the bits will be used and not shift will be nessacary
                                {
                                    pixelend = (chunkIndex + stateindex) - StatesBeforeTrigger; // +1 to include trigger state
                                    dummywidth = 0;
                                }
                                if (w != (int)WidthnumericUpDown.Value) //In case the enough pixels are placed before the lanes are finished
                                    createpixel(ref bmp, pixeldata, width, (tracker - count), lanes, dummywidth, ref w, h, ref pixels, pixelstart, pixelend);
                                count = 0;
                            }
                        }
                        else if (sb.ToString() == HBE) //If Horizontal Blanking End
                        {
                            if (w < (int)WidthnumericUpDown.Value) //If w equals the inputed width, width most be to small
                            {
                                string e = "Width to big, All pixels found on line found. Try " + w.ToString() + " as the Width."; 
                                Runerror error = new Runerror(e);
                                this.Invoke(new Action(() =>
                                {
                                    error.Show();
                                }));
                                errorcheck = true;
                                bmpshow = false;
                                break;
                            }
                            if (w > (int)WidthnumericUpDown.Value) //If w equals the inputed width, width most be to small
                            {
                                string e = "Width to small, more pixels found on line found";
                                Runerror error = new Runerror(e);
                                this.Invoke(new Action(() =>
                                {
                                    error.Show();
                                }));
                                errorcheck = true;
                                bmpshow = false;
                                break;
                            }
                            h++;
                            w = 0;
                            tracker = 0;
                            dummywidth = 0;
                            pixeldata.Clear();
                            count = 0;
                            if (h % 10 == 0) //Update Progress bar every 10 lines. Limit switching threads
                            {
                                this.Invoke(new Action(() =>
                                {
                                    this.toolStripProgressBar1.Increment((int)WidthnumericUpDown.Value * 10);
                                    this.statusStrip1.Update();
                                }));
                            }
                        }
                        else if (sb.ToString() == VBS) //If Vertical Blanking Start
                        {
                            h++;
                            if (h < (HeightnumericUpDown.Value))
                            {
                                string e = "Input height too big, only found " + (h).ToString() + " lines";
                                Runerror error = new Runerror(e);
                                this.Invoke(new Action(() =>
                                {
                                    error.Show();
                                }));
                                errorcheck = true;
                            }
                            if (h > (HeightnumericUpDown.Value))
                            {
                                string e = "Input height too small, not enough lines in the frame, check the MSA, Height in the MSA must be same as input Height";
                                Runerror error = new Runerror(e);
                                this.Invoke(new Action(() =>
                                {
                                    error.Show();
                                }));
                                errorcheck = true;
                            }
                            s = states; //have frame no need to search through states, set equal to break out of outer forloop
                            break; //break from inner
                        }
                    }
                    if (errorcheck == true) //There was an error, break out of the forloop
                    {
                        break;
                    }
                }
                Statechunk.Clear();
                if (token.IsCancellationRequested)
                {
                    break;
                }
            }
            if (errorcheck != true && !token.IsCancellationRequested)
            {
                if (SendingPicture != null)
                {
                    string reference = "";
                    if (ReferencecheckBox.Checked == true)
                        reference = "Reference";
                    picturelist = new List<PixelData>(pixelList.ToArray());
                    this.Invoke(new Action(() =>
                    {
                        SendingPicture(this, new SendingPictureArgs(picturelist, Frameid, reference));
                    }));
                }
            }
            return bmp;
        }
        /// <summary>
        /// Acquiring Pixeldata and return bitmap, very similar to RGB
        /// </summary>
        /// <param name="lanes"></param>
        /// <param name="width"></param>
        /// <param name="startstate"></param>
        /// <param name="pixelstate"></param>
        /// <param name="endstate"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private Bitmap getpixeldataYCbCr422(int lanes, int width, int startstate, int pixelstate, int endstate, CancellationToken token)
        {
            if (token.IsCancellationRequested == true)
            {
                token.ThrowIfCancellationRequested();
            }
            int pixels = 0;
            Bitmap bmp = new Bitmap((int)WidthnumericUpDown.Value, (int)HeightnumericUpDown.Value);
            if (lanes == 1) //If the width is 16, if there is one lane, the width most be doubled to get Cr and Y1 components.
                width *= 2;
            string VBS = "0x4A";
            string HBE = "0x15";
            int w = 0;
            int h = 0;
            int update = 0;
            List<byte[]> pixeldata = new List<byte[]>();
            int vchannel = Virtualchannel;
            List<byte> Statechunk = new List<byte>();
            int component = width / 3;
            bool flag = true;
            int stateindex = pixelstate;
            bool check = false;
            int tracker = 0;
            int states = (endstate - startstate); //If this doesn't work could just get state listing
            int lanewidth = 8;
            int dummywidth = 0; //Adding by 8 each time, this is for a check later in the program.
            int count = 0; //counts number of states before enough are found for a pixel state
            int chunkSize = 4096;
            int stateDataLength = 16;
            byte[] stateData = new byte[stateDataLength];
            bool errorcheck = false;
            bool bmpshow = true;
            int pixelstart = -1;
            int pixelend = -1;
            List<PixelData> picturelist = null;
            for (int s = 0; s < states; stateindex += chunkSize, s += chunkSize)
            {
                GettingPixelDataArgs args = null;
                if (GettingPixelData != null)
                {
                    args = new GettingPixelDataArgs(4096, vchannel, stateindex, Statechunk);
                    GettingPixelData(this, args);
                }
                Statechunk = args.Statedata;
                chunkSize = Statechunk.Count / stateDataLength;
                if (chunkSize > 0)
                {
                    for (int chunkIndex = 0; (chunkIndex < chunkSize) && (stateindex + chunkIndex) < endstate + chunkIndex; chunkIndex += 1)
                    {
                        getStateDataFromChunk(chunkIndex * 16, Statechunk, ref stateData);
                        StringBuilder sb = Geteventcode(stateData);
                        if (sb.ToString() == "0x88" || sb.ToString() == "0xC8") //If pixel state
                        {
                            if (w >= (int)WidthnumericUpDown.Value) //If w equals the inputed width, width most be to small
                            {
                                string e = "Not enough pixel found to fill a line, check MSA, width should equal MSA width.";
                                Runerror error = new Runerror(e);
                                this.Invoke(new Action(() =>
                                {
                                    error.Show();
                                }));
                                errorcheck = true;
                                break;
                            }
                            if (h >= (int)HeightnumericUpDown.Value)
                            {
                                string e = "Not enough lines in frame, height to small, check the MSA, Height in the MSA must be same as input Height.";
                                Runerror error = new Runerror(e);
                                this.Invoke(new Action(() =>
                                {
                                    error.Show();
                                }));
                                errorcheck = true;
                                break;
                            }
                            if (flag == true) //Adding bits to dummywidth
                            {
                                if (dummywidth < lanewidth)
                                {
                                    if (pixelstart != pixelend || pixelstart < 0)
                                        pixelstart = (chunkIndex + stateindex) - StatesBeforeTrigger;
                                }
                                dummywidth += lanewidth;
                            }
                            else
                            {
                                pixelstart = (chunkIndex + stateindex) - StatesBeforeTrigger;
                                flag = true;
                            }
                            pixeldata.Add(stateData);
                            stateData = new byte[stateDataLength];
                            tracker++;
                            count++;
                            if (dummywidth >= width)
                            {
                                if (dummywidth != width) //if the dummywidth is more than the width, subtract it. Must be kept track for the reminder.
                                {
                                    pixelend = (chunkIndex + stateindex) - StatesBeforeTrigger;
                                    chunkIndex--; //This state is going to be put into the statedata list twice, thats why flag will also be false, so dummywidth is not added a second time
                                    dummywidth = dummywidth - width;
                                    flag = false;
                                }
                                else if (dummywidth == width) //No remander, all the bits will be used and not shift will be nessacary
                                {
                                    pixelend = (chunkIndex + stateindex) - StatesBeforeTrigger;
                                    dummywidth = 0;
                                }
                                if (w != (int)WidthnumericUpDown.Value) //In case the enough pixels are placed before the lanes are finished
                                    createpixelYCbCr422(ref bmp, pixeldata, width, (tracker - count), lanes, dummywidth, ref w, h, ref pixels, pixelstart, pixelend);
                                count = 0;
                            }
                        }
                        else if (sb.ToString() == HBE) //If Horizontal Blanking End
                        {
                            if (w < (int)WidthnumericUpDown.Value) //If w equals the inputed width, width most be to small
                            {
                                string e = "Width to big, All pixels found on line found. Try " + w.ToString() + " as the Width.";
                                Runerror error = new Runerror(e);
                                this.Invoke(new Action(() =>
                                {
                                    error.Show();
                                }));
                                errorcheck = true;
                                bmpshow = false;
                                break;
                            }
                            if (w > (int)WidthnumericUpDown.Value) //If w equals the inputed width, width most be to small
                            {
                                string e = "Width to small, more pixels found on line found.";
                                Runerror error = new Runerror(e);
                                this.Invoke(new Action(() =>
                                {
                                    error.Show();
                                }));
                                errorcheck = true;
                                bmpshow = false;
                                break;
                            }
                            h++;
                            w = 0;
                            tracker = 0;
                            dummywidth = 0;
                            pixeldata.Clear();
                            count = 0;
                            if (h % 10 == 0)
                            {
                                this.Invoke(new Action(() =>
                                {
                                    this.toolStripProgressBar1.Increment((int)WidthnumericUpDown.Value * 10);
                                    this.statusStrip1.Update();
                                }));
                            }
                        }
                        else if (sb.ToString() == VBS) //If Vertical Blanking Start
                        {
                            if (h < (HeightnumericUpDown.Value - 1))
                            {
                                string e = "Input height too big, only found " + (h + 1).ToString() + " lines";
                                Runerror error = new Runerror(e);
                                this.Invoke(new Action(() =>
                                {
                                    error.Show();
                                }));
                                errorcheck = true;
                                bmpshow = false;
                            }
                            if (h > (HeightnumericUpDown.Value - 1))
                            {
                                string e = "Input height to small, not enough lines in the frame, or VBS not in Channel, check the MSA, Height in the MSA must be same as input Height";
                                Runerror error = new Runerror(e);
                                this.Invoke(new Action(() =>
                                {
                                    error.Show();
                                }));
                                errorcheck = true;
                                bmpshow = false;
                            }
                            s = states; //have frame no need to search through states, set equal to break out of outer forloop
                            break; //break from inner
                        }
                    }
                    if (errorcheck == true) //There was an error, break out of the forloop
                    {
                        break;
                    }
                }
                Statechunk.Clear();
                if (token.IsCancellationRequested)
                {
                    break;
                }
            }
            if (errorcheck != true)
            {
                if (SendingPicture != null)
                {
                    string reference = "";
                    if (ReferencecheckBox.Checked == true)
                        reference = "Reference";
                    picturelist = new List<PixelData>(pixelList.ToArray());
                    this.Invoke(new Action(() =>
                    {
                        SendingPicture(this, new SendingPictureArgs(picturelist, Frameid, reference));
                    }));
                }
            }
            return bmp;
        }
        /// <summary>
        /// Aquiring Pixeldata and return bitmap, must have two pixel lists because of how 420 is read.
        /// </summary>
        /// <param name="lanes"></param>
        /// <param name="width"></param>
        /// <param name="startstate"></param>
        /// <param name="pixelstate"></param>
        /// <param name="endstate"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private Bitmap getpixeldataYCbCr420(int lanes, int width, int startstate, int pixelstate, int endstate, CancellationToken token)
        {
            if (token.IsCancellationRequested == true)
            {
                token.ThrowIfCancellationRequested();
            }
            int pixels = 0;
            Bitmap bmp = new Bitmap((int)WidthnumericUpDown.Value, (int)HeightnumericUpDown.Value);
            string VBS = "0x4A";
            string HBE = "0x15";
            int w = 0;
            int h = 0;
            int switchlist = 1;
            int update = 0;
            width *= 2;
            List<byte[]> pixeldata_odd = new List<byte[]>();
            List<byte[]> pixeldata_even = new List<byte[]>();
            int vchannel = Virtualchannel;
            List<byte> Statechunk = new List<byte>();
            int component = width / 3;
            bool flag = true;
            int stateindex = pixelstate;
            bool check = false;
            int tracker = 0;
            int states = (endstate - startstate); //If this doesn't work could just get state listing
            int lanewidth = 8;
            int dummywidth = 0; //Adding by 8 each time, this is for a check later in the program.
            int count = 0; //counts number of states before enough are found for a pixel state
            int chunkSize = 4096;
            int stateDataLength = 16;
            byte[] stateData = new byte[stateDataLength];
            bool errorcheck = false;
            bool bmpshow = true;
            List<PixelData> picturelist = null;
            for (int s = 0; s < states; stateindex += chunkSize, s += chunkSize)
            {
                GettingPixelDataArgs args = null;
                if (GettingPixelData != null)
                {
                    args = new GettingPixelDataArgs(4096, vchannel, stateindex, Statechunk);
                    GettingPixelData(this, args);
                }
                Statechunk = args.Statedata;
                chunkSize = Statechunk.Count / stateDataLength;
                if (chunkSize > 0)
                {
                    for (int chunkIndex = 0; (chunkIndex < chunkSize) && (stateindex + chunkIndex) < endstate + chunkIndex; chunkIndex += 1)
                    {
                        if (h % 2 == 0) //Switchlist determines which list the state data will be stored in
                            switchlist = 0;
                        else
                            switchlist = 1;
                        getStateDataFromChunk(chunkIndex * 16, Statechunk, ref stateData);
                        StringBuilder sb = Geteventcode(stateData);
                        if (sb.ToString() == "0x88" || sb.ToString() == "0xC8") //If pixel state
                        {
                            if (w == (int)WidthnumericUpDown.Value) //If w equals the inputed width, width most be to small
                            {
                                string e = "Not enough pixel found to fill a line, check MSA, width should equal MSA width.";
                                Runerror error = new Runerror(e);
                                this.Invoke(new Action(() =>
                                {
                                    error.Show();
                                }));
                                errorcheck = true;
                                bmpshow = false;
                                break;
                            }
                            if (h == (int)HeightnumericUpDown.Value)
                            {
                                string e = "Not enough lines in frame, height to small, check the MSA, Height in the MSA must be same as input Height";
                                Runerror error = new Runerror(e);
                                this.Invoke(new Action(() =>
                                {
                                    error.Show();
                                }));
                                errorcheck = true;
                                break;
                            }
                            if (flag == true) //Adding bits to dummywidth
                                dummywidth += lanewidth;
                            else
                                flag = true;
                            if (switchlist == 0)
                                pixeldata_even.Add(stateData);
                            else
                                pixeldata_odd.Add(stateData);
                            stateData = new byte[stateDataLength];
                            tracker++;
                            count++;
                            if (dummywidth >= width)
                            {
                                if (dummywidth != width) //if the dummywidth is more than the width, subtract it. Must be kept track for the reminder.
                                {
                                    chunkIndex--; //This state is going to be put into the statedata list twice, thats why flag will also be false, so dummywidth is not added a second time
                                    dummywidth = dummywidth - width;
                                    flag = false;
                                }
                                else if (dummywidth == width) //No remander, all the bits will be used and not shift will be nessacary
                                    dummywidth = 0;
                                if (h % 2 == 1)
                                {
                                    if (w != (int)WidthnumericUpDown.Value) //In case the enough pixels are placed before the lanes are finished
                                        createpixelYCbCr420(ref bmp, pixeldata_even, pixeldata_odd, width, (tracker - count), lanes, dummywidth, ref w, h);
                                }
                                count = 0;
                            }
                        }
                        else if (sb.ToString() == HBE) //If Horizontal Blanking End
                        {
                            if (h % 2 == 1)
                            {
                                if (w < (int)WidthnumericUpDown.Value) //If w equals the inputed width, width most be to small
                                {
                                    string e = "All pixels found on line found, check MSA correct width should equal" + w.ToString();
                                    Runerror error = new Runerror(e);
                                    this.Invoke(new Action(() =>
                                    {
                                        error.Show();
                                    }));
                                    errorcheck = true;
                                    bmpshow = false;
                                    break;
                                }
                            }
                            h++;
                            if (h % 10 == 0)
                            {
                                this.Invoke(new Action(() =>
                                {
                                    this.toolStripProgressBar1.Increment((int)WidthnumericUpDown.Value * 10);
                                    this.statusStrip1.Update();
                                }));
                            }
                            w = 0;
                            tracker = 0;
                            dummywidth = 0;
                            if (h % 2 == 0)
                            {
                                pixeldata_even.Clear();
                                pixeldata_odd.Clear();
                            }
                            count = 0;
                        }
                        else if (sb.ToString() == VBS) //If Vertical Blanking Start
                        {
                            if (h < (HeightnumericUpDown.Value - 1))
                            {
                                string e = "Input height to big, more lines than pixels, check the MSA, Height in the MSA must be same as input Height";
                                Runerror error = new Runerror(e);
                                this.Invoke(new Action(() =>
                                {
                                    error.Show();
                                }));
                                errorcheck = true;
                                bmpshow = false;
                            }
                            if (h > (HeightnumericUpDown.Value - 1))
                            {
                                string e = "Input height to small, not enough lines in the frame, or VBS not in Channel, check the MSA, Height in the MSA must be same as input Height";
                                Runerror error = new Runerror(e);
                                this.Invoke(new Action(() =>
                                {
                                    error.Show();
                                }));
                                errorcheck = true;
                                bmpshow = false;
                            }
                            s = states; //have frame no need to search through states, set equal to break out of outer forloop
                            break; //break from inner
                        }
                    }
                    if (errorcheck == true) //There was an error, break out of the forloop
                    {
                        break;
                    }
                }
                Statechunk.Clear();
                if (token.IsCancellationRequested)
                {
                    break;
                }
            }
            if (errorcheck != true)
            {
                if (SendingPicture != null)
                {
                    string reference = "";
                    if (ReferencecheckBox.Checked == true)
                        reference = "Reference";
                    picturelist = new List<PixelData>(pixelList.ToArray());
                    this.Invoke(new Action(() =>
                    {
                        SendingPicture(this, new SendingPictureArgs(pixelList, Frameid, reference));
                    }));
                }
            }
            return bmp;
        }

        /// <summary>
        /// Creates a pixel with help from the Getlanedata function
        /// </summary>
        /// <param name="dataBytes"></param>
        /// <param name="width"></param>
        /// <returns name = "ulong"> </returns>
        /// 
        private List<PictureLineData> AnalysePixels(int lanes, int width, int startstate, int pixelstate, int endstate, CancellationToken token)
        {
            List<PictureLineData> LineData = new List<PictureLineData>();
            if (token.IsCancellationRequested == true)
            {
                token.ThrowIfCancellationRequested();
            }
            int pixels = 0;
            string VBS = "0x4A";
            string HBE = "0x15";
            int w = 0; //Bitmap height
            int h = 0; //Bitmap width
            List<byte[]> pixeldata = new List<byte[]>();
            int vchannel = Virtualchannel;
            List<byte> Statechunk = new List<byte>();
            int component = width / 3;
            bool flag = true;
            int stateindex = pixelstate;
            bool check = false;
            int tracker = 0;
            int states = (endstate - startstate); //Number of states to get frame
            int lanewidth = 8;
            int dummywidth = 0; //Adding by 8 each time, this is for a check later in the program.
            int count = 0; //counts number of states before enough are found for a pixel state
            int chunkSize = 4096;
            int stateDataLength = 16;
            byte[] stateData = new byte[stateDataLength];
            string error = "";
            List<string> errorlist = new List<string>();
            bool firstpixel = false;
            int startofline = 0;
            int maxwidth = 0;
            for (int s = 0; s < states; stateindex += chunkSize, s += chunkSize) //s is just a counter for the section of data this frame is assigned too
            {
                GettingPixelDataArgs args = null;
                if (GettingPixelData != null) //Raise event, get data from m_IProbe
                {
                    args = new GettingPixelDataArgs(4096, vchannel, stateindex, Statechunk);
                    GettingPixelData(this, args);
                }
                Statechunk = args.Statedata;
                chunkSize = Statechunk.Count / stateDataLength;
                if (chunkSize > 0) //Should only be 0 if no more states
                {
                    for (int chunkIndex = 0; (chunkIndex < chunkSize) && (stateindex + chunkIndex) < endstate + chunkIndex; chunkIndex += 1) //Get state data from statechunk
                    {
                        getStateDataFromChunk(chunkIndex * 16, Statechunk, ref stateData); //Get individual state from statechunk
                        StringBuilder sb = Geteventcode(stateData);
                        if (sb.ToString() == "0x88" || sb.ToString() == "0xC8") //If pixel state
                        {
                            if (firstpixel == false)
                            {
                                startofline = stateindex + chunkIndex;
                                firstpixel = true;
                            }
                            if (flag == true) //Adding bits to dummywidth
                                dummywidth += lanewidth;
                            else
                                flag = true;
                            pixeldata.Add(stateData);
                            stateData = new byte[stateDataLength];
                            tracker++;
                            count++;
                            if (dummywidth >= width)
                            {
                                if (dummywidth != width) //if the dummywidth is more than the width, subtract it. Must be kept track for the reminder.
                                {
                                    chunkIndex--; //This state is going to be put into the statedata list twice, thats why flag will also be false, so dummywidth is not added a second time
                                    dummywidth = dummywidth - width;
                                    flag = false;
                                }
                                else if (dummywidth == width) //No remander, all the bits will be used and not shift will be nessacary
                                    dummywidth = 0; 
                                createpixel_analyze(pixeldata, width, (tracker - count), lanes, dummywidth, ref w, h, ref pixels);
                                count = 0;
                            }
                        }
                        else if (sb.ToString() == HBE) //If Horizontal Blanking End
                        {
                            if (w > Data.MSAData.Width) //If w equals the inputed width, width most be to small
                            {
                                error = "Painted " + PaintedPixels.ToString() + " pixels. Found more pixels on line.";
                                errorlist.Add(error);
                            }
                            if (w < Data.MSAData.Width) //If w equals the inputed width, width most be to small
                            {
                                error = "Painted " + PaintedPixels.ToString() + " pixels. Asked to paint more when they do not exist.";
                                errorlist.Add(error);
                            }
                            if (maxwidth < w)
                            {
                                maxwidth = w;
                            }
                            h++;
                            string[] copy = new string [errorlist.Count];
                            errorlist.CopyTo(copy);
                            PictureLineData linedata = new PictureLineData(h, w, copy, startofline, stateindex + chunkIndex);
                            errorlist.Clear();
                            LineData.Add(linedata);
                            firstpixel = false;
                            startofline = 0;
                            PaintedPixels = 0;
                            w = 0;
                            error = "";
                            tracker = 0;
                            dummywidth = 0;
                            pixeldata.Clear();
                            count = 0;
                            if (h % 10 == 0) //Update Progress bar every 10 lines. Limit switching threads
                            {
                                this.Invoke(new Action(() =>
                                {
                                    this.toolStripProgressBar1.Increment((int)WidthnumericUpDown.Value * 10);
                                    this.statusStrip1.Update();
                                }));
                            }
                        }
                        else if (sb.ToString() == VBS) //If Vertical Blanking Start
                        {
                            h++;
                            if (h < Data.MSAData.Height)
                            {
                                error = "Input height too big";
                                errorlist.Add(error);
                            }
                            if (h > Data.MSAData.Height)
                            {
                                error = "Input height too small";
                                errorlist.Add(error);
                            }
                            if (w > Data.MSAData.Width) //If w equals the inputed width, width most be to small
                            {
                                error = "Painted " + w.ToString() + " pixels. Found more pixels on line.";
                                errorlist.Add(error);
                            }
                            if (w < Data.MSAData.Width) //If w equals the inputed width, width most be to small
                            {
                                error = "Painted " + w.ToString() + " pixels. Asked to paint more when they do not exist.";
                                errorlist.Add(error);
                            }
                            string[] copy = new string[errorlist.Count];
                            errorlist.CopyTo(copy);
                            PictureLineData linedata = new PictureLineData(h, w, copy, startofline, stateindex + chunkIndex);
                            errorlist.Clear();
                            PaintedPixels = 0;
                            LineData.Add(linedata);
                            s = states; //have frame no need to search through states, set equal to break out of outer forloop
                            break; //break from inner
                        }
                    }
                }
                Statechunk.Clear();
                if (token.IsCancellationRequested)
                {
                    break;
                }
            }
            Bitmap bmp = new Bitmap(maxwidth, h); //Returning with this
            createpicture(ref bmp, pixelList);
            this.Invoke(new Action(() =>
            {
                this.PictureBox.Enabled = true;
                this.PictureBox.Image = bmp;
            }));
            return LineData;
        }

        private Bitmap createpicture(ref Bitmap bmp, List<PixelData> pixels)
        {
            foreach (PixelData pixel in pixels)
            {
                bmp.SetPixel(pixel.XLocation, pixel.YLocation, Color.FromArgb(pixel.RValue, pixel.GValue, pixel.BValue));
            }
            return bmp;
        }
        private List<PictureLineData> AnalysePixels422(int lanes, int width, int startstate, int pixelstate, int endstate, CancellationToken token)
        {
            List<PictureLineData> LineData = new List<PictureLineData>();
            if (token.IsCancellationRequested == true)
            {
                token.ThrowIfCancellationRequested();
            }
            int pixels = 0;
            if (lanes == 1)
                width *= 2;
            string VBS = "0x4A";
            string HBE = "0x15";
            int w = 0; //Bitmap height
            int h = 0; //Bitmap width
            List<byte[]> pixeldata = new List<byte[]>();
            int vchannel = Virtualchannel;
            List<byte> Statechunk = new List<byte>();
            int component = width / 3;
            bool flag = true;
            int stateindex = pixelstate;
            bool check = false;
            int tracker = 0;
            int states = (endstate - startstate); //Number of states to get frame
            int lanewidth = 8;
            int dummywidth = 0; //Adding by 8 each time, this is for a check later in the program.
            int count = 0; //counts number of states before enough are found for a pixel state
            int chunkSize = 4096;
            int stateDataLength = 16;
            byte[] stateData = new byte[stateDataLength];
            string error = "";
            List<string> errorlist = new List<string>();
            bool firstpixel = false;
            int startofline = 0;
            int maxwidth = 0;
            for (int s = 0; s < states; stateindex += chunkSize, s += chunkSize) //s is just a counter for the section of data this frame is assigned too
            {
                GettingPixelDataArgs args = null;
                if (GettingPixelData != null) //Raise event, get data from m_IProbe
                {
                    args = new GettingPixelDataArgs(4096, vchannel, stateindex, Statechunk);
                    GettingPixelData(this, args);
                }
                Statechunk = args.Statedata;
                chunkSize = Statechunk.Count / stateDataLength;
                if (chunkSize > 0) //Should only be 0 if no more states
                {
                    for (int chunkIndex = 0; (chunkIndex < chunkSize) && (stateindex + chunkIndex) < endstate + chunkIndex; chunkIndex += 1) //Get state data from statechunk
                    {
                        getStateDataFromChunk(chunkIndex * 16, Statechunk, ref stateData); //Get individual state from statechunk
                        StringBuilder sb = Geteventcode(stateData);
                        if (sb.ToString() == "0x88" || sb.ToString() == "0xC8") //If pixel state
                        {
                            if (firstpixel == false)
                            {
                                startofline = stateindex + chunkIndex;
                                firstpixel = true;
                            }
                            if (flag == true) //Adding bits to dummywidth
                                dummywidth += lanewidth;
                            else
                                flag = true;
                            pixeldata.Add(stateData);
                            stateData = new byte[stateDataLength];
                            tracker++;
                            count++;
                            if (dummywidth >= width)
                            {
                                if (dummywidth != width) //if the dummywidth is more than the width, subtract it. Must be kept track for the reminder.
                                {
                                    chunkIndex--; //This state is going to be put into the statedata list twice, thats why flag will also be false, so dummywidth is not added a second time
                                    dummywidth = dummywidth - width;
                                    flag = false;
                                }
                                else if (dummywidth == width) //No remander, all the bits will be used and not shift will be nessacary
                                    dummywidth = 0;
                                createpixelYCbCr422_analyze(pixeldata, width, (tracker - count), lanes, dummywidth, ref w, h, ref pixels);
                                count = 0;
                            }
                        }
                        else if (sb.ToString() == HBE) //If Horizontal Blanking End
                        {
                            if (w > Data.MSAData.Width) //If w equals the inputed width, width most be to small
                            {
                                error = "Painted " + PaintedPixels.ToString() + " pixels. Found more pixels on line.";
                                errorlist.Add(error);
                            }
                            if (w < Data.MSAData.Width) //If w equals the inputed width, width most be to small
                            {
                                error = "Painted " + PaintedPixels.ToString() + " pixels. Asked to paint more when they do not exist.";
                                errorlist.Add(error);
                            }
                            if (maxwidth < w)
                            {
                                maxwidth = w;
                            }
                            h++;
                            string[] copy = new string[errorlist.Count];
                            errorlist.CopyTo(copy);
                            PictureLineData linedata = new PictureLineData(h, w, copy, startofline, stateindex + chunkIndex);
                            errorlist.Clear();
                            LineData.Add(linedata);
                            firstpixel = false;
                            startofline = 0;
                            w = 0;
                            error = "";
                            tracker = 0;
                            dummywidth = 0;
                            pixeldata.Clear();
                            count = 0;
                            PaintedPixels = 0;
                            if (h % 10 == 0) //Update Progress bar every 10 lines. Limit switching threads
                            {
                                this.Invoke(new Action(() =>
                                {
                                    this.toolStripProgressBar1.Increment((int)WidthnumericUpDown.Value * 10);
                                    this.statusStrip1.Update();
                                }));
                            }
                        }
                        else if (sb.ToString() == VBS) //If Vertical Blanking Start
                        {
                            h++;
                            if (h < Data.MSAData.Height)
                            {
                                error = "Input height too big";
                                errorlist.Add(error);
                            }
                            if (h > Data.MSAData.Height)
                            {
                                error = "Input height too small";
                                errorlist.Add(error);
                            }
                            if (w > Data.MSAData.Width) //If w equals the inputed width, width most be to small
                            {
                                error = "Painted " + w.ToString() + " pixels. Found more pixels on line.";
                                errorlist.Add(error);
                            }
                            if (w < Data.MSAData.Width) //If w equals the inputed width, width most be to small
                            {
                                error = "Painted " + w.ToString() + " pixels. Asked to paint more when they do not exist.";
                                errorlist.Add(error);
                            }
                            string[] copy = new string[errorlist.Count];
                            errorlist.CopyTo(copy);
                            PictureLineData linedata = new PictureLineData(h, w, copy, startofline, stateindex + chunkIndex);
                            errorlist.Clear();
                            LineData.Add(linedata);
                            s = states; //have frame no need to search through states, set equal to break out of outer forloop
                            break; //break from inner
                        }
                    }
                }
                Statechunk.Clear();
                if (token.IsCancellationRequested)
                {
                    break;
                }
            }
            Bitmap bmp = new Bitmap(maxwidth, h); //Returning with this
            createpicture(ref bmp, pixelList);
            this.Invoke(new Action(() =>
            {
                this.PictureBox.Enabled = true;
                this.PictureBox.Image = bmp;
            }));
            return LineData;
        }


        private List<PictureLineData> AnalysePixels420(int lanes, int width, int startstate, int pixelstate, int endstate, CancellationToken token)
        {
            List<PictureLineData> LineData = new List<PictureLineData>();
            if (token.IsCancellationRequested == true)
            {
                token.ThrowIfCancellationRequested();
            }
            int pixels = 0;
            Bitmap bmp = new Bitmap((int)WidthnumericUpDown.Value, (int)HeightnumericUpDown.Value); //Returning with this
            string VBS = "0x4A";
            string HBE = "0x15";
            int w = 0; //Bitmap height
            int h = 0; //Bitmap width
            int switchlist = 1;
            List<byte[]> pixeldata_odd = new List<byte[]>();
            List<byte[]> pixeldata_even = new List<byte[]>();
            int vchannel = Virtualchannel;
            List<byte> Statechunk = new List<byte>();
            int component = width / 3;
            bool flag = true;
            int stateindex = pixelstate;
            bool check = false;
            int tracker = 0;
            int states = (endstate - startstate); //Number of states to get frame
            int lanewidth = 8;
            int dummywidth = 0; //Adding by 8 each time, this is for a check later in the program.
            int count = 0; //counts number of states before enough are found for a pixel state
            int chunkSize = 4096;
            int stateDataLength = 16;
            byte[] stateData = new byte[stateDataLength];
            string error = "";
            List<string> errorlist = new List<string>();
            bool firstpixel = false;
            int startofline = 0;
            for (int s = 0; s < states; stateindex += chunkSize, s += chunkSize) //s is just a counter for the section of data this frame is assigned too
            {
                GettingPixelDataArgs args = null;
                if (GettingPixelData != null) //Raise event, get data from m_IProbe
                {
                    args = new GettingPixelDataArgs(4096, vchannel, stateindex, Statechunk);
                    GettingPixelData(this, args);
                }
                Statechunk = args.Statedata;
                chunkSize = Statechunk.Count / stateDataLength;
                if (chunkSize > 0) //Should only be 0 if no more states
                {
                    for (int chunkIndex = 0; (chunkIndex < chunkSize) && (stateindex + chunkIndex) < endstate + chunkIndex; chunkIndex += 1) //Get state data from statechunk
                    {
                        if (h % 2 == 0) //Switchlist determines which list the state data will be stored in
                            switchlist = 0;
                        else
                            switchlist = 1;
                        getStateDataFromChunk(chunkIndex * 16, Statechunk, ref stateData); //Get individual state from statechunk
                        StringBuilder sb = Geteventcode(stateData);
                        if (sb.ToString() == "0x88" || sb.ToString() == "0xC8") //If pixel state
                        {
                            if (firstpixel == false)
                            {
                                startofline = stateindex + chunkIndex;
                                firstpixel = true;
                            }
                            if (w == (int)WidthnumericUpDown.Value) //If w equals the inputed width, width most be to small
                            {
                                error = "Width to small, more pixels found on line.";
                                errorlist.Add(error);
                            }
                            if (h == (int)HeightnumericUpDown.Value) //If h equals the inputed height, the height must be to small
                            {
                                error = "Not enough lines in frame, height to small, check the MSA, Height in the MSA must be same as input Height";
                                errorlist.Add(error);
                            }
                            if (flag == true) //Adding bits to dummywidth
                                dummywidth += lanewidth;
                            else
                                flag = true;
                            if (switchlist == 0)
                                pixeldata_even.Add(stateData);
                            else
                                pixeldata_odd.Add(stateData);
                            stateData = new byte[stateDataLength];
                            tracker++;
                            count++;
                            if (dummywidth >= width)
                            {
                                if (dummywidth != width) //if the dummywidth is more than the width, subtract it. Must be kept track for the reminder.
                                {
                                    chunkIndex--; //This state is going to be put into the statedata list twice, thats why flag will also be false, so dummywidth is not added a second time
                                    dummywidth = dummywidth - width;
                                    flag = false;
                                }
                                else if (dummywidth == width) //No remander, all the bits will be used and not shift will be nessacary
                                    dummywidth = 0;
                                if (h % 2 == 1)
                                {
                                    if (w != (int)WidthnumericUpDown.Value) //In case the enough pixels are placed before the lanes are finished
                                        createpixelYCbCr420(ref bmp, pixeldata_even, pixeldata_odd, width, (tracker - count), lanes, dummywidth, ref w, h);
                                }
                                count = 0;
                            }
                        }
                        else if (sb.ToString() == HBE) //If Horizontal Blanking End
                        {
                            if (h % 2 == 1)
                            {
                                if (w < (int)WidthnumericUpDown.Value) //If w equals the inputed width, width most be to small
                                {
                                    error = "All pixels found on line found, check MSA correct width should equal";
                                    errorlist.Add(error);
                                }
                            }
                            h++;
                            if (h % 10 == 0) //Update Progress bar every 10 lines. Limit switching threads
                            {
                                this.Invoke(new Action(() =>
                                {
                                    this.toolStripProgressBar1.Increment((int)WidthnumericUpDown.Value * 10);
                                    this.statusStrip1.Update();
                                }));
                            }
                            if (error == "")
                            {
                                error = "None";
                            }
                            string[] copy = new string[errorlist.Count];
                            errorlist.CopyTo(copy);
                            PictureLineData linedata = new PictureLineData(h, w, copy, startofline, stateindex + chunkIndex);
                            errorlist.Clear();
                            LineData.Add(linedata);
                            firstpixel = false;
                            startofline = 0;
                            w = 0;
                            error = "";
                            tracker = 0;
                            dummywidth = 0;
                            if (h % 2 == 0)
                            {
                                pixeldata_even.Clear();
                                pixeldata_odd.Clear();
                            }
                            count = 0;
                        }
                        else if (sb.ToString() == VBS) //If Vertical Blanking Start
                        {
                            if (h < (HeightnumericUpDown.Value - 1))
                            {
                                error = "Input height to big, more lines than pixels, check the MSA, Height in the MSA must be same as input Height";
                                errorlist.Add(error);
                            }
                            if (h > (HeightnumericUpDown.Value - 1))
                            {
                                error = "Input height to small, not enough lines in the frame, or VBS not in Channel, check the MSA, Height in the MSA must be same as input Height";
                                errorlist.Add(error);
                            }
                            h++;
                            if (error == "")
                            {
                                error = "None";
                            }
                            string[] copy = new string[errorlist.Count];
                            errorlist.CopyTo(copy);
                            PictureLineData linedata = new PictureLineData(h, w, copy, startofline, stateindex + chunkIndex);
                            errorlist.Clear();
                            LineData.Add(linedata);
                            s = states; //have frame no need to search through states, set equal to break out of outer forloop
                            break; //break from inner
                        }
                    }
                }
                Statechunk.Clear();
                if (token.IsCancellationRequested)
                {
                    break;
                }
            }
            this.Invoke(new Action(() =>
            {
                this.PictureBox.Enabled = true;
                this.PictureBox.Image = bmp;
            }));
            return LineData;
        }
        private void createpixel(ref Bitmap bmp, List<byte[]> pixeldata, int width, int state, int lanes, int remander, ref int w, int h, ref int pixels, int pixelstart, int pixelend)
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
                placepixel(pixel1, ref bmp, ref w, h, ref pixels, pixelstart, pixelend);
                placepixel(pixel2, ref bmp, ref w, h, ref pixels, pixelstart, pixelend);
                placepixel(pixel3, ref bmp, ref w, h, ref pixels, pixelstart, pixelend);
                placepixel(pixel4, ref bmp, ref w, h, ref pixels, pixelstart, pixelend);
            }
            else if (lanes == 2)
            {
                placepixel(pixel1, ref bmp, ref w, h, ref pixels, pixelstart, pixelend);
                placepixel(pixel2, ref bmp, ref w, h, ref pixels, pixelstart, pixelend);
            }
            else if (lanes == 1)
            {
                placepixel(pixel1, ref bmp, ref w, h, ref pixels, pixelstart, pixelend);
            }
        }

        private void createpixel_analyze(List<byte[]> pixeldata, int width, int state, int lanes, int remander, ref int w, int h, ref int pixels)
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
                placepixel_analyze(pixel1, ref w, h, ref pixels);
                placepixel_analyze(pixel2, ref w, h, ref pixels);
                placepixel_analyze(pixel3, ref w, h, ref pixels);
                placepixel_analyze(pixel4, ref w, h, ref pixels);
            }
            else if (lanes == 2)
            {
                placepixel_analyze(pixel1, ref w, h, ref pixels);
                placepixel_analyze(pixel2, ref w, h, ref pixels);
            }
            else if (lanes == 1)
            {
                placepixel_analyze(pixel1, ref w, h, ref pixels);
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
        private void createpixelYCbCr422(ref Bitmap bmp, List<byte[]> pixeldata, int width, int state, int lanes, int remander, ref int w, int h, ref int pixels, int pixelstart, int pixelend)
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
            placepixel(pixel, ref bmp, ref w, h, ref pixels, pixelstart, pixelend); //Place pixel
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

        private void createpixelYCbCr422_analyze(List<byte[]> pixeldata, int width, int state, int lanes, int remander, ref int w, int h, ref int pixels)
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
            placepixel_analyze(pixel, ref w, h, ref pixels); //Place pixel
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
        private void createpixelYCbCr420(ref Bitmap bmp, List<Byte[]> even, List<Byte[]> odd, int width, int state, int lanes, int remander, ref int w, int h) //Similar to RGB algorithm
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
        /// Places pixel in bitmap from RGB, YCbCr444, and YCbCr422 formats
        /// </summary>
        /// <param name="pixel"></param>
        /// <param name="bmp"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        private void placepixel(ulong pixel, ref Bitmap bmp, ref int w, int h, ref int pixels, int pixelstart, int pixelend)
        {
            string format = Format;
            if (format == "RGB")
            {
                placeRGB(ref w, ref h, pixel, ref bmp, ref pixels, pixelstart, pixelend);
            }
            else if (format == "YCbCr444")
            {
                placeYCbCr444(ref w, ref h, pixel, ref bmp, pixelstart, pixelend);
            }
            else if (format == "YCbCr422")
            {
                placeYCbCr422(ref w, ref h, pixel, ref bmp, pixelstart, pixelend);
            }
        }

        private void placepixel_analyze(ulong pixel, ref int w, int h, ref int pixels)
        {
            string format = Format;
            if (format == "RGB")
            {
                AddRGBPixel(ref w, ref h, pixel, ref pixels);
            }
            else if (format == "YCbCr444")
            {
                AddYCbCr444Pixel(ref w, ref h, pixel);
            }
            else if (format == "YCbCr422")
            {
                AddYCbCr422Pixel(ref w, ref h, pixel);
            }
        }


        private void AddRGBPixel(ref int w, ref int h, ulong pixel, ref int pixels)
        {
            int component = PixelWidth / 3;
            int r = (int)(pixel >> component * 2);
            r = r & getbitmask(PixelWidth);
            int g = (int)(pixel >> component);
            g = g & getbitmask(PixelWidth);
            int b = (int)(pixel);
            b = b & getbitmask(PixelWidth);
            if (PixelWidth == 30) //This is to bring RGB widths down to 24
            {
                r = r / 4;
                g = g / 4;
                b = b / 4;
            }
            else if (PixelWidth == 36)//This is to bring RGB widths down to 24
            {
                r = r / 16;
                g = g / 16;
                b = b / 16;
            }
            else if (PixelWidth == 48)//This is to bring RGB widths down to 24
            {
                r = r / 256;
                g = g / 256;
                b = b / 256;
            }
            if (TestPixelsCheck.Checked == true)
            {
                if (w == 12)
                {
                    r = 0;
                    g = 0;
                    b = 0;
                }
            }
            PixelData p = new PixelData(h, w, 0, 0, r, g, b, true);
            pixelList.Add(p);
            PaintedPixels++;
            w++;
        }
        /// <summary>
        /// Place RGB pixel
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="pixel"></param>
        /// <param name="bmp"></param>
        /// <param name="pixels"></param>
        private void placeRGB(ref int w, ref int h, ulong pixel, ref Bitmap bmp, ref int pixels, int pixelstart, int pixelend)
        {
            int component = PixelWidth / 3;
            int r = (int)(pixel >> component * 2);
            r = r & getbitmask(PixelWidth);
            int g = (int)(pixel >> component);
            g = g & getbitmask(PixelWidth);
            int b = (int)(pixel);
            b = b & getbitmask(PixelWidth);
            if (PixelWidth == 30) //This is to bring RGB widths down to 24
            {
                r = r / 4;
                g = g / 4;
                b = b / 4;
            }
            else if (PixelWidth == 36)//This is to bring RGB widths down to 24
            {
                r = r / 16;
                g = g / 16;
                b = b / 16;
            }
            else if (PixelWidth == 48)//This is to bring RGB widths down to 24
            {
                r = r / 256;
                g = g / 256;
                b = b / 256;
            }
            if (w < (int)WidthnumericUpDown.Value)
            {
                if (h < (int)HeightnumericUpDown.Value)
                {
                    if (TestPixelsCheck.Checked == true)
                    {
                        if (w == 12)
                        {
                            r = 0;
                            g = 0;
                            b = 0;
                        }
                    }
                    bmp.SetPixel(w, h, Color.FromArgb(r, g, b));
                    PixelData p = new PixelData(h, w, pixelstart, pixelend, r, g, b, true);
                    pixelList.Add(p);
                    PaintedPixels++;
                }
            }
            w++;
        }

        private void AddYCbCr444Pixel(ref int w, ref int h, ulong pixel)
        {
            int component = PixelWidth / 3;
            int Cb = (int)(pixel >> component * 2);
            Cb = Cb & (int)getbitmask(PixelWidth);
            int Y = (int)(pixel >> component);
            Y = Y & (int)getbitmask(PixelWidth);
            int Cr = (int)(pixel);
            Cr = Cr & (int)getbitmask(PixelWidth);
            if (PixelWidth == 30) //This is to bring RGB widths down to 24
            {
                Cr = Cr / 4;
                Y = Y / 4;
                Cb = Cb / 4;
            }
            else if (PixelWidth == 36)//This is to bring RGB widths down to 24
            {
                Cr = Cr / 16;
                Y = Y / 16;
                Cb = Cb / 16;
            }
            else if (PixelWidth == 48)//This is to bring RGB widths down to 24
            {
                Cr = Cr / 256;
                Y = Y / 256;
                Cb = Cb / 256;
            }
            YCbCr_to_RGB_Analyze(Y, Cb, Cr, w, h);
            w++;
        }
        /// <summary>
        /// Convert to RGB, then place
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="pixel"></param>
        /// <param name="bmp"></param>
        private void placeYCbCr444(ref int w, ref int h, ulong pixel, ref Bitmap bmp, int pixelstart, int pixelend)
        {
            int component = PixelWidth / 3;
            int Cb = (int)(pixel >> component * 2);
            Cb = Cb & (int)getbitmask(PixelWidth);
            int Y = (int)(pixel >> component);
            Y = Y & (int)getbitmask(PixelWidth);
            int Cr = (int)(pixel);
            Cr = Cr & (int)getbitmask(PixelWidth);
            if (PixelWidth == 30) //This is to bring RGB widths down to 24
            {
                Cr = Cr / 4;
                Y = Y / 4;
                Cb = Cb / 4;
            }
            else if (PixelWidth == 36)//This is to bring RGB widths down to 24
            {
                Cr = Cr / 16;
                Y = Y / 16;
                Cb = Cb / 16;
            }
            else if (PixelWidth == 48)//This is to bring RGB widths down to 24
            {
                Cr = Cr / 256;
                Y = Y / 256;
                Cb = Cb / 256;
            }
            YCbCr_to_RGB(ref bmp, Y, Cb, Cr, w, h, pixelstart, pixelend);
            w++;
        }
        /// <summary>
        /// Convert to RGB, then place
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="pixel"></param>
        /// <param name="bmp"></param>
        private void placeYCbCr422(ref int w, ref int h, ulong pixel, ref Bitmap bmp, int pixelstart, int pixelend)
        {
            int component = PixelWidth / 2;
            int mask = getbitmaskYCbCr();
            int Cb = (int)(pixel >> component * 3);
            Cb = Cb & mask;
            int Y = (int)(pixel >> component * 2);
            Y = Y & mask;
            int Cr = (int)(pixel >> component);
            Cr = Cr & mask;
            int Y1 = (int)pixel;
            Y1 = Y1 & mask;

            if (PixelWidth == 20)
            {
                Y /= 4;
                Cb /= 4;
                Cr /= 4;
                Y1 /= 4;
            }

            else if (PixelWidth == 24)
            {
                Y /= 16;
                Cb /= 16;
                Cr /= 16;
                Y1 /= 16;
            }

            else if (PixelWidth == 32)
            {
                Y /= 256;
                Cb /= 256;
                Cr /= 256;
                Y1 /= 256;
            }

            YCbCr_to_RGB(ref bmp, Y, Cb, Cr, w, h, pixelstart, pixelend);
            w++;
            YCbCr_to_RGB(ref bmp, Y1, Cb, Cr, w, h, pixelstart, pixelend);
            w++;
        }

        private void AddYCbCr422Pixel(ref int w, ref int h, ulong pixel)
        {
            int component = PixelWidth / 2;
            int mask = getbitmaskYCbCr();
            int Cb = (int)(pixel >> component * 3);
            Cb = Cb & mask;
            int Y = (int)(pixel >> component * 2);
            Y = Y & mask;
            int Cr = (int)(pixel >> component);
            Cr = Cr & mask;
            int Y1 = (int)pixel;
            Y1 = Y1 & mask;

            if (PixelWidth == 20)
            {
                Y /= 4;
                Cb /= 4;
                Cr /= 4;
                Y1 /= 4;
            }

            else if (PixelWidth == 24)
            {
                Y /= 16;
                Cb /= 16;
                Cr /= 16;
                Y1 /= 16;
            }

            else if (PixelWidth == 32)
            {
                Y /= 256;
                Cb /= 256;
                Cr /= 256;
                Y1 /= 256;
            }

            YCbCr_to_RGB_Analyze(Y, Cb, Cr, w, h);
            w++;
            YCbCr_to_RGB_Analyze(Y1, Cb, Cr, w, h);
            w++;
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
            if (PixelWidth == 20)
            {
                eY /= 4;
                oY /= 4;
                Cb /= 4;
                Cr /= 4;
                eY1 /= 4;
                oY1 /= 4;
            }

            else if (PixelWidth == 24)
            {
                eY /= 16;
                oY /= 16;
                Cb /= 16;
                Cr /= 16;
                eY1 /= 16;
                oY1 /= 16;
            }

            else if (PixelWidth == 32)
            {
                eY /= 256;
                oY /= 256;
                Cb /= 256;
                Cr /= 256;
                eY1 /= 256;
                oY1 /= 256;
            }

            YCbCr_to_RGB(ref bmp, eY, Cb, Cr, w, h - 1, 0, 0);
            YCbCr_to_RGB(ref bmp, eY1, Cb, Cr, w + 1, h - 1, 0, 0);
            YCbCr_to_RGB(ref bmp, oY, Cb, Cr, w, h, 0, 0);
            YCbCr_to_RGB(ref bmp, oY1, Cb, Cr, w + 1, h, 0, 0);
        }
        #endregion

        #region Public Methods
        #endregion

    }
    public class PixelData
    {
        private int m_ylocation;
        public int YLocation { get { return m_ylocation; } set { m_ylocation = value; } }
        private int m_xlocation;
        public int XLocation { get { return m_xlocation; } set { m_xlocation = value; } }
        private int m_startstate;
        public int StartState { get { return m_startstate; } set { m_startstate = value; } }
        private int m_endstate;
        public int EndState { get { return m_endstate; } set { m_endstate = value; } }
        private int m_rvalue;
        public int RValue { get { return m_rvalue; } set { m_rvalue = value; } }
        private int m_gvalue;
        public int GValue { get { return m_gvalue; } set { m_gvalue = value; } }
        private int m_bvalue;
        public int BValue { get { return m_bvalue; } set { m_bvalue = value; } }
        private bool m_compare;
        public bool Compare { get { return m_compare; } set { m_compare = value; } }
        public PixelData(int ylocation, int xlocation, int startstate, int endstate, int rvalue, int gvalue, int bvalue, bool compare)
        {
            YLocation = ylocation;
            XLocation = xlocation;
            StartState = startstate;
            EndState = endstate;
            RValue = rvalue;
            GValue = gvalue;
            BValue = bvalue;
            Compare = compare;
        }
    }
    //Delegates used to pass events up to other forms.
    public delegate void PaintingEvent(object sender, PaintingEventArgs e);

    public class PaintingEventArgs : EventArgs
    {
        public bool Painting = false;
        public int Frameid = 0;
        public PaintingEventArgs(bool painting, int frameid)
        {
            Painting = painting;
            Frameid = frameid;
        }
    }

    public delegate void GetPixelData(object sender, GettingPixelDataArgs e);

    public class GettingPixelDataArgs : EventArgs
    {
        public int Statechunk = 0;
        public int VC = 0;
        public int Stateindex = 0;
        public List<byte> Statedata = null;
        public GettingPixelDataArgs(int statechunk, int vc, int stateindex, List<byte> statedata)
        {
            this.Statechunk = statechunk;
            this.VC = vc;
            this.Stateindex = stateindex;
            this.Statedata = statedata;
        }
    }

    public delegate void GetMetaData(object sender, GettingMetaDataArgs e);

    public class GettingMetaDataArgs : EventArgs
    {
        public Metadata MetaData = null;
        //private Metadata Data { get { return m_data; } set { m_data = value; } } 
        public GettingMetaDataArgs(Metadata data)
        {
            this.MetaData = data;
        }
    }

    public delegate void RequestPictureEvent(object sender, SendingPictureArgs e);

    public class SendingPictureArgs : EventArgs
    {
        public List<PixelData> Pixels = new List<PixelData>();
        public int Frameid = 0;
        public string Reference = "";
        public SendingPictureArgs(List<PixelData> pixels, int id, string reference)
        {
            Pixels = pixels;
            Frameid = id;
            Reference = reference;
        }
    }

    public delegate void CheckReferenceEvent(object sender, CheckReferenceArgs e);

    public class CheckReferenceArgs : EventArgs
    {
        public bool ReferenceCheck;

        public CheckReferenceArgs(bool reference)
        {
            ReferenceCheck = reference;
        }
    }

    public delegate void CheckPictureEvent(object sender, CheckPictureArgs e);

    public class CheckPictureArgs : EventArgs
    {
        public bool PictureCheck;
        public int FrameID = 0;

        public CheckPictureArgs(bool picturecheck, int frameid)
        {
            PictureCheck = picturecheck;
            FrameID = frameid;
        }
    }
}


