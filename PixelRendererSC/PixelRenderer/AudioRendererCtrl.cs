using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DP12MSTClassLibrary;
using DP13MSTClassLibrary;
using DP12SSTClassLibrary;
using DP13SSTClassLibrary;
using FPSProbeMgr_Gen2;
using SharedProject1;
using System.Xml;
using System.Collections;
using System.IO;

namespace PixelRenderer
{
    public partial class AudioRendererCtrl : UserControl
    {
        #region Members
        private DP12SST m_DP12SSTProbe = null;
        private DP12MST m_DP12MSTProbe = null;
        private DP13SST m_DP13SSTProbe = null;
        private DP13MST m_DP13MSTProbe = null;
        IProbeMgrGen2 m_IProbe = null;

        private string m_format = "";
        private string Format { get { return m_format; } set { m_format = value; } }

        private string m_protocol = "";
        private string Protocol { get { return m_protocol; } set { m_protocol = value; } }
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
        #endregion //Members

        #region Ctor

        /// <summary>
        /// Default Constructor(s)
        /// </summary>
        public AudioRendererCtrl()
        {
            InitializeComponent();
            defaultsetup();
        }
        #endregion // Ctor(s)

        #region Event Handlers
        /// <summary>
        /// Disable Virtual Channels in SST mode
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
        /// <summary>
        /// Enable Virtual Channels in MST mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Disable Virtual Channels in SST mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Enable Virtual Channels in MST mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Return the Protocol being used
        /// </summary>
        /// <returns></returns>
        private string getprotocol()
        {
            string protocal = "";
            if (DP1_2SSTbutton.Checked)
            {
                protocal = "SST-1.2";
            }
            else if (DP1_2MSTbutton.Checked)
            {
                protocal = "MST-1.2";
            }
            else if (DP1_4SSTbutton.Checked)
            {
                protocal = "SST-1.4";
            }
            else if (DP1_4MSTbutton.Checked)
            {
                protocal = "MST-1.4";
            }
            else
            {
                protocal = "error";
            }
            return protocal;
        }
        /// <summary>
        /// Return the Virtual Channel being used, if in SST mode, return a 1 and act as if one channel is open
        /// </summary>
        /// <returns></returns>
        private int getvc()
        {
            int vc = 0;
            if (VC1button.Checked)
                vc = 1;
            else if (VC2button.Checked)
                vc = 2;
            else if (VC3button.Checked)
                vc = 3;
            else if (VC4button.Checked)
                vc = 4;
            if (DP1_2SSTbutton.Checked || DP1_4SSTbutton.Checked)
            {
                vc = 1;
            }
            return vc;
        }
        /// <summary>
        /// Open the other 6 channels if channel 1 and 2 are open, if not, close them.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void channel2checkbox_CheckedChanged(object sender, EventArgs e)
        {
            if (channel2checkbox.Checked == true)
            {
                if (channel1checkbox.Checked == false)
                {
                    channel1checkbox.Checked = true;
                    channel3checkbox.Enabled = true;
                    channel4checkbox.Enabled = true;
                    channel5checkbox.Enabled = true;
                    channel6checkbox.Enabled = true;
                    channel7checkbox.Enabled = true;
                    channel8checkbox.Enabled = true;
                }
                if (channel1checkbox.Checked == true)
                {
                    channel3checkbox.Enabled = true;
                    channel4checkbox.Enabled = true;
                    channel5checkbox.Enabled = true;
                    channel6checkbox.Enabled = true;
                    channel7checkbox.Enabled = true;
                    channel8checkbox.Enabled = true;
                }
            }
            if (channel2checkbox.Checked == false)
            {
                channel3checkbox.Enabled = false;
                channel4checkbox.Enabled = false;
                channel5checkbox.Enabled = false;
                channel6checkbox.Enabled = false;
                channel7checkbox.Enabled = false;
                channel8checkbox.Enabled = false;
            }
        }
        /// <summary>
        /// Open the other 6 channels if channel 1 and 2 are open, if not, close them.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void channel1checkbox_CheckedChanged(object sender, EventArgs e)
        {
            if (channel1checkbox.Checked == false)
            {
                if (channel2checkbox.Checked == true)
                {
                    channel2checkbox.Checked = false;
                    channel3checkbox.Enabled = false;
                    channel4checkbox.Enabled = false;
                    channel5checkbox.Enabled = false;
                    channel6checkbox.Enabled = false;
                    channel7checkbox.Enabled = false;
                    channel8checkbox.Enabled = false;
                }
            }
            if (channel1checkbox.Checked == true)
            {
                if (channel2checkbox.Checked == true)
                {
                    channel3checkbox.Enabled = true;
                    channel4checkbox.Enabled = true;
                    channel5checkbox.Enabled = true;
                    channel6checkbox.Enabled = true;
                    channel7checkbox.Enabled = true;
                    channel8checkbox.Enabled = true;
                }
            }
        }
        /// <summary>
        /// Getting info from the form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void maudnaudbutton_Click(object sender, EventArgs e)
        {
            int flag = 0;
            int states = 0;
            this.progressBar1.Value = 0;
            createInterfaceObject();  // assign the m_IProbe variable.
            m_IProbe.Initialize();    // get the trace buffer manager object to register itself
            int lane = 0;
            if (Lane1button.Checked)
                lane = 1;
            else if (Lane2button.Checked)
                lane = 2;
            else if (lane4button.Checked)
                lane = 4;
            Protocol = getprotocol();
            xmlreader();
            if ((int)EndStatenumericUpDown.Value == 0)
            {
                string err = "End State Box can not equal zero";
                Runerror error = new Runerror(err);
                error.Show();
                flag = 1;
            }
            else if ((int)EndStatenumericUpDown.Value < (int)StartStateNumericUpDown.Value)
            {
                string err = "Start State can not be greater than End State";
                Runerror error = new Runerror(err);
                error.Show();
                flag = 1;
            }
            if ((DP1_2SSTbutton.Checked == true || DP1_4SSTbutton.Checked == true) && getvc() == 0)
            {
                string err = "No Virtual Channel selected in MST mode";
                Runerror error = new Runerror(err);
                error.Show();
                flag = 1;
            }
            else
            {
                states = Convert.ToInt32(m_IProbe.GetNumberOfStates(getvc()));
            }
            this.progressBar1.Maximum = (int)EndStatenumericUpDown.Value;
            if (flag == 0)
                getmaudnaud(lane);
        }
        /// <summary>
        /// Reading in all data the user checked in the form and error checking
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void createbutton_Click(object sender, EventArgs e)
        {
            Protocol = getprotocol();
            int flag = 0;
            this.progressBar1.Value = 0;
            createInterfaceObject();  // assign the m_IProbe variable.
            int states = 0;
            createInterfaceObject();  // assign the m_IProbe variable.
            m_IProbe.Initialize();
            if ((DP1_2SSTbutton.Checked == true || DP1_4SSTbutton.Checked == true) && getvc() == 0)
            {
                string err = "No Virtual Channel selected in MST mode";
                Runerror error = new Runerror(err);
                error.Show();
                flag = 1;
            }
            else
            {
                states = Convert.ToInt32(m_IProbe.GetNumberOfStates(getvc()));
            }
            int lane = 0;
            if (Lane1button.Checked)
                lane = 1;
            else if (Lane2button.Checked)
                lane = 2;
            else if (lane4button.Checked)
                lane = 4;
            xmlreader();
            if (string.IsNullOrEmpty(sampleratetext.Text) || sampleratetext.Text == "0")
            {
                string err = "Missing Sample Rate";
                Runerror error = new Runerror(err);
                error.Show();
                flag = 1;
            }
            if (string.IsNullOrEmpty(bitspersampletext.Text) || bitspersampletext.Text == "0")
            {
                string err = "Missing Bits per Sample";
                Runerror error = new Runerror(err);
                error.Show();
                flag = 1;
            }
            if (channel1checkbox.Checked == false && channel2checkbox.Checked == false)
            {
                string err = "Select Audio Channels";
                Runerror error = new Runerror(err);
                error.Show();
                flag = 1;
            }
            if ((int)EndStatenumericUpDown.Value == 0)
            {
                string err = "End State Box can not equal zero";
                Runerror error = new Runerror(err);
                error.Show();
                flag = 1;
            }
            else if ((int)EndStatenumericUpDown.Value < (int)StartStateNumericUpDown.Value)
            {
                string err = "Start State can not be greater than End State";
                Runerror error = new Runerror(err);
                error.Show();
                flag = 1;
            }
            else
            {
                this.progressBar1.Maximum = (int)EndStatenumericUpDown.Value;
            }
            if (flag == 0)
                getaudio(lane);
        }
        #endregion //Event Handlers

        #region Private Methods
        /// <summary>
        /// How the form will first appear
        /// </summary>
        private void defaultsetup()
        {
            DP1_2SSTbutton.Checked = true;
            Lane1button.Checked = true;
            channel3checkbox.Enabled = false;
            channel4checkbox.Enabled = false;
            channel5checkbox.Enabled = false;
            channel6checkbox.Enabled = false;
            channel7checkbox.Enabled = false;
            channel8checkbox.Enabled = false;
        }
        /// <summary>
        /// Creates IProbe object depending on which protocol is selected
        /// </summary>
        /// <returns></returns>
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
                    if (m_DP13SSTProbe != null)
                        m_DP13SSTProbe = null;
                    m_DP13SSTProbe = new DP13SST();
                    m_IProbe = (IProbeMgrGen2)m_DP13SSTProbe;
                    break;
                case "MST-1.4":
                    if (m_DP13MSTProbe != null)
                        m_DP13MSTProbe = null;
                    m_DP13MSTProbe = new DP13MST();
                    m_IProbe = (IProbeMgrGen2)m_DP13MSTProbe;
                    break;
            }
            return status;
        }
        /// <summary>
        /// Returns a bool of which channels are open based on the user input.
        /// </summary>
        /// <returns></returns>
        private List<bool> createchannelarray()
        {
            List<bool> channels = new List<bool> { false, false, false, false, false, false, false, false };
            if (channel1checkbox.Checked == true)
                channels[0] = true;
            if (channel2checkbox.Checked == true)
                channels[1] = true;
            if (channel3checkbox.Checked == true)
                channels[2] = true;
            if (channel4checkbox.Checked == true)
                channels[3] = true;
            if (channel5checkbox.Checked == true)
                channels[4] = true;
            if (channel6checkbox.Checked == true)
                channels[5] = true;
            if (channel7checkbox.Checked == true)
                channels[6] = true;
            if (channel8checkbox.Checked == true)
                channels[7] = true;
            return channels;
        }
        /// <summary>
        /// Returns the number of channels open
        /// </summary>
        /// <returns></returns>
        private int getnumchannels()
        {
            int channels = 0;
            List<bool> channelarray = createchannelarray();
            int i = 0;
            for (i = 0; i < channelarray.Count; i++)
            {
                if (channelarray[i] == true)
                    channels++;
            }
            return channels;
        }
        /// <summary>
        /// Reads pixelrender.xml file
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
                        Lane0sb = Convert.ToInt32(reader.GetAttribute("StartBit"));
                    if (reader.GetAttribute("Name") == "Lane1")
                        Lane1sb = Convert.ToInt32(reader.GetAttribute("StartBit"));
                    if (reader.GetAttribute("Name") == "Lane2")
                        Lane2sb = Convert.ToInt32(reader.GetAttribute("StartBit"));
                    if (reader.GetAttribute("Name") == "Lane3")
                        Lane3sb = Convert.ToInt32(reader.GetAttribute("StartBit"));
                }
            }
        }
        /// <summary>
        /// Return a StringBuilder of the event code
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
        /// Returns a list of lane data for all four lanes
        /// </summary>
        /// <param name="dataBytes"></param>
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
        /// For eventcode ending in 1FD, check if the bit before the FD is a 1 or 0.
        /// </summary>
        /// <param name="dataBytes"></param>
        /// <returns></returns>
        private bool getlanebit(byte[] dataBytes)
        {
            int bit = Lane0sb - 1;
            int v = (15 - (bit / 8));
            byte dum = dataBytes[15 - (bit / 8)];
            dum = Convert.ToByte(dum & 0x40);
            if (dum == 0x00)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private NAudio.Wave.WaveFileReader wave = null;

        private NAudio.Wave.DirectSoundOut output = null;
        /// <summary>
        /// Allows user the open a wave file from their computer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void getfilebutton_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Wave File (*.wav)|*.wav;";
            if (open.ShowDialog() != DialogResult.OK) return;
            DisposeWave();

            wave = new NAudio.Wave.WaveFileReader(open.FileName);

            axWindowsMediaPlayer1.URL = open.FileName;
        }
        private void DisposeWave()
        {
            output = new NAudio.Wave.DirectSoundOut();
            if (output.PlaybackState == NAudio.Wave.PlaybackState.Playing) output.Stop();
            output.Dispose();
            output = null;

            if (wave != null)
            {
                wave.Dispose();
                wave = null;
            }
        }

        private void Form1_FormClosing(object sender, FormClosedEventArgs e)
        {
            DisposeWave();
        }
        /// <summary>
        /// Looping through all the states looking for SDP start
        /// </summary>
        /// <param name="lanes"></param>
        private void getaudio(int lanes)
        {
            int VAS = 0;
            int HAS = 0;
            int bytes = 0;
            string HAS_SDP = "0x20";
            string VAS_SDP = "0x60";
            int vchannel = getvc();
            long mckinley = m_IProbe.GetNumberOfStates(vchannel);
            int i = (int)StartStateNumericUpDown.Value;
            byte[] dum = null;
            int states = (int)EndStatenumericUpDown.Value;
            bool flag = false;
            byte[] audio = new byte[0];
            int lane = 0;
            List<bool> channels = createchannelarray();
            while (i <= states)
            {
                dum = (m_IProbe.GetStateData(vchannel, i));
                this.progressBar1.Increment(1);
                StringBuilder sb = Geteventcode(dum);
                if (sb.ToString() == HAS_SDP || sb.ToString() == VAS_SDP)
                {
                    List<byte> lanedata = Getlanedata(dum);
                    flag = getlanebit(dum);
                    byte b = lanedata[0];
                    if (flag == true && b == 92)
                    {
                        if (sb.ToString() == HAS_SDP)
                        {
                            HAS++;
                            audio = createbytearray(lanes, lane, ref i, ref audio, vchannel, ref bytes,channels);
                        }
                        if (sb.ToString() == VAS_SDP)
                        {
                            VAS++;
                            audio = createbytearray(lanes, lane, ref i, ref audio, vchannel, ref bytes,channels);
                        }
                    }
                }
                i++;
            }
            int size = audio.Length;
            audioSDPtext.Text = Convert.ToString(VAS + HAS);
            audioVSDPtext.Text = Convert.ToString(VAS);
            createwavefile(audio);
        }
        /// <summary>
        /// SDP for been found, going in the audio channels and grabbing audio data if channel is open
        /// </summary>
        /// <param name="lanes"></param>
        /// <param name="lane"></param>
        /// <param name="state"></param>
        /// <param name="bytearray"></param>
        /// <param name="vchannel"></param>
        /// <param name="bytes"></param>
        /// <param name="channels"></param>
        /// <returns></returns>
        private byte[] createbytearray(int lanes, int lane, ref int state, ref byte[] bytearray, int vchannel, ref int bytes, List<bool> channels)
        {
            string HAS_SDP = "0x20";
            string VAS_SDP = "0x60";
            byte[] dum = null;
            int skip_bytes = getbytestoskip(lanes);
            skipbytes(skip_bytes,ref state, vchannel, HAS_SDP, VAS_SDP);
            int i = 0;
            int tracker = 0;
            while (i != channels.Count())
            {
                if (channels[i] == true)
                {
                    tracker = 0;
                    int statetracker = 0;
                    lookinchannel(ref state, ref tracker, ref statetracker, ref bytearray, ref bytes, lane, vchannel, HAS_SDP, VAS_SDP);
                    checklanes(lanes, lane, ref state, ref statetracker);
                }
                else
                {
                    tracker = 0;
                    int statetracker = 0;
                    skipchannel(ref state, ref tracker, ref statetracker, dum, vchannel, HAS_SDP, VAS_SDP);
                    checklanes(lanes, lane, ref state, ref statetracker);
                }
                i++;
            }
            while (i != 0)
            {
                dum = (m_IProbe.GetStateData(vchannel, state));
                StringBuilder sb = Geteventcode(dum);
                if (sb.ToString() == HAS_SDP || sb.ToString() == VAS_SDP)
                {
                    List<byte> lanedata = Getlanedata(dum);
                    {
                        bool flag = getlanebit(dum);
                        byte b = lanedata[0];
                        if (flag == true && b == 253)
                        {
                            break;
                        }
                    }
                }
                state++;
                this.progressBar1.Increment(1);
            }
            return bytearray;
        }
        /// <summary>
        /// Sets the number of bytes to skip
        /// </summary>
        /// <param name="lanes"></param>
        /// <returns></returns>
        private int getbytestoskip(int lanes)
        {
            int bytes = 0;
            if (lanes == 4)
                bytes = 3;
            else if (lanes == 2)
                bytes = 6;
            else if (lanes == 1)
                bytes = 9;
            return bytes;
        }
        /// <summary>
        /// In beginning of SDP, skipping header bytes and parity byte
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="state"></param>
        /// <param name="vchannel"></param>
        /// <param name="HAS_SDP"></param>
        /// <param name="VAS_SDP"></param>
        private void skipbytes(int bytes, ref int state, int vchannel, string HAS_SDP, string VAS_SDP)
        {
            int i = 0;
            while (i != bytes)
            {
                byte[] dum = (m_IProbe.GetStateData(vchannel, state));
                StringBuilder sb = Geteventcode(dum);
                if (sb.ToString() == HAS_SDP || sb.ToString() == VAS_SDP)
                {
                    i++;
                }
                state++;
                this.progressBar1.Increment(1);
            }
        }
        /// <summary>
        /// If the channel is open, look into it and get audio data
        /// </summary>
        /// <param name="state"></param>
        /// <param name="tracker"></param>
        /// <param name="statetracker"></param>
        /// <param name="bytearray"></param>
        /// <param name="bytes"></param>
        /// <param name="lane"></param>
        /// <param name="vchannel"></param>
        /// <param name="HAS_SDP"></param>
        /// <param name="VAS_SDP"></param>
        private void lookinchannel(ref int state, ref int tracker, ref int statetracker, ref byte[] bytearray, ref int bytes, int lane, int vchannel, string HAS_SDP, string VAS_SDP )
        {
            tracker = 0;
            statetracker = 0;
            byte[] dum = null;
            while (tracker != 5)
            {
                dum = (m_IProbe.GetStateData(vchannel, state));
                StringBuilder sb = Geteventcode(dum);
                if (sb.ToString() == HAS_SDP || sb.ToString() == VAS_SDP)
                {
                    tracker++;
                    if (tracker == 2 || tracker == 3)
                    {
                        List<byte> lanedata = Getlanedata(dum);
                        byte[] newarray = null;
                        newarray = new byte[bytearray.Length + 1];
                        bytearray.CopyTo(newarray, 0);
                        newarray[bytes] = lanedata[lane];
                        bytes++;
                        int b = newarray[bytes - 1];
                        bytearray = newarray;
                        b = bytearray[bytes - 1];
                        b = 0;
                    }
                }
                state++;
                statetracker++;
            }
        }
        /// <summary>
        /// If channel is closed, count the states, but dont get any audio data
        /// </summary>
        /// <param name="state"></param>
        /// <param name="tracker"></param>
        /// <param name="statetracker"></param>
        /// <param name="dum"></param>
        /// <param name="vchannel"></param>
        /// <param name="HAS_SDP"></param>
        /// <param name="VAS_SDP"></param>
        private void skipchannel(ref int state, ref int tracker, ref int statetracker, byte[] dum, int vchannel, string HAS_SDP, string VAS_SDP)
        {
            tracker = 0;
            statetracker = 0;
            while (tracker != 5)
            {
                dum = (m_IProbe.GetStateData(vchannel, state));
                StringBuilder sb = Geteventcode(dum);
                if (sb.ToString() == HAS_SDP || sb.ToString() == VAS_SDP)
                {
                    tracker++;
                }
                state++;
                statetracker++;
            }
        }
        /// <summary>
        /// Check to see if another lane with the same states needs to be checked.
        /// </summary>
        /// <param name="lanes"></param>
        /// <param name="lane"></param>
        /// <param name="state"></param>
        /// <param name="statetracker"></param>
        private void checklanes(int lanes, int lane, ref int state, ref int statetracker)
        {
            if (lanes == 1)
            {
                lane = 0;
                this.progressBar1.Increment(statetracker);
            }
            else if (lanes == 2)
            {
                if (lane == 1)
                {
                    lane = 0;
                    this.progressBar1.Increment(statetracker);
                }
                else
                {
                    lane++;
                    state -= statetracker;
                }
            }
            else if (lanes == 4)
            {
                if (lane == 3)
                {
                    lane = 0;
                    this.progressBar1.Increment(statetracker);
                }
                else
                {
                    lane++;
                    state -= statetracker;
                }
            }
        }
        /// <summary>
        /// Creates byte array by adding a riff header and converting byte array to wav file.
        /// </summary>
        /// <param name="audio"></param>
        private void createwavefile(byte[] audio)
        {
            //http://soundfile.sapp.org/doc/WaveFormat/
            int channels = getnumchannels();
            int samplebits = Convert.ToInt32(bitspersampletext.Text);
            int audio_size = audio.Count();
            short block = Convert.ToInt16(channels * (samplebits / 8));
            int sample_rate = Convert.ToInt32(sampleratetext.Text);


            byte[] newarray = null;
            newarray = new byte[audio.Length + 44];
            audio.CopyTo(newarray, 44);
            byte[] chuckID = new byte[4] { 0x52, 0x49, 0x46, 0x46 }; //contain letters RIFF
            chuckID.CopyTo(newarray, 0);
            byte[] chucksize = BitConverter.GetBytes(audio_size + 36);//new byte[4] { 0xD0, 0x13, 0x1D, 0x00 }; //36 + SubChunk2Size or more precisely 4 + (8 + subchunk1size) + (8 + subchunk2size)
            chucksize.CopyTo(newarray, 4);
            byte[] format = new byte[4] { 0x57, 0x41, 0x56, 0x45 }; //contain letters WAVE
            format.CopyTo(newarray, 8);
            byte[] subchunk1ID = new byte[4] { 0x66, 0x6d, 0x74, 0x20 }; //Contains letters fmt
            subchunk1ID.CopyTo(newarray, 12);
            byte[] subchunk1size = new byte[4] { 0x10, 0x00, 0x00, 0x00 }; //16 for PCM
            subchunk1size.CopyTo(newarray, 16);
            byte[] audioformat = new byte[2] { 0x01, 0x00 };//PCM = 1 values other than 1 indicate some form of compression
            audioformat.CopyTo(newarray, 20);
            byte[] numchannels = new byte[2] { 0x02, 0x00 }; //Mono = 1 Stereo = 2
            numchannels.CopyTo(newarray, 22);
            byte[] samplerate = BitConverter.GetBytes(sample_rate);//new byte[4] { 0x80, 0xBB, 0x00, 0x00 }; //8000, 44100, etc  192000
            samplerate.CopyTo(newarray, 24);
            byte[] byterate = BitConverter.GetBytes(sample_rate * block);//new byte[4] { 0x00, 0xEE, 0x02, 0x00 }; // samplerate * blockalign
            byterate.CopyTo(newarray, 28);
            byte[] blockalign = BitConverter.GetBytes(block);//new byte[2] { 0x04, 0x00 }; // numchannels * bits_per_sample/8
            blockalign.CopyTo(newarray, 32);
            byte[] bits_per_sample = new byte[2] { 0x10, 0x00 }; // 8 bits = 8, 16 bits = 16, etc
            bits_per_sample.CopyTo(newarray, 34);
            byte[] subchunk2ID = new byte[4] { 0x64, 0x61, 0x74, 0x61 }; // contain letters "data"
            subchunk2ID.CopyTo(newarray, 36);
            byte[] subchunk2size = BitConverter.GetBytes(audio_size);// new byte[4] { 0xAC, 0x13, 0x1D, 0x00 }; // numsamples * numchannels * bits_per_sample / 8 basically the number of bytes in the data following this number
            subchunk2size.CopyTo(newarray, 40);

            audio = newarray;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "WAV files(*.wav) | *.wav";
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.IO.File.WriteAllBytes(saveFileDialog.FileName, audio);
            }
        }
        /// <summary>
        /// Go through state listing looking for state of Vertical Audio TimeStamp and filling out chart.
        /// </summary>
        /// <param name="lanes"></param>
        private void getmaudnaud(int lanes)
        {
            string V_Audio_TS = "0x64";
            int vchannel = getvc();
            int i = 0;
            byte[] dum = null;
            int states = (int)EndStatenumericUpDown.Value;
            List<uint> mauddata = new List<uint>();
            List<uint> nauddata = new List<uint>();
            int lane = 0;
            while (i <= states)
            {
                dum = m_IProbe.GetStateData(vchannel, i);
                StringBuilder sb = Geteventcode(dum);
                if (sb.ToString() == V_Audio_TS)
                {
                    extractdata(ref i, V_Audio_TS,ref mauddata, ref nauddata);
                }
                i++;
                this.progressBar1.Increment(1);
            }
            if (mauddata.Any() && nauddata.Any())
            {
                this.maudnaudchart.Series["Value"].Points.AddXY("Maud_Max", getmax(mauddata));
                this.maudnaudchart.Series["Value"].Points.AddXY("Maud_Min", getmin(mauddata));
                this.maudnaudchart.Series["Value"].Points.AddXY("Maud_Mean", getmean(mauddata));
                this.maudnaudchart.Series["Value"].Points.AddXY("Naud_Max", getmax(nauddata));
                this.maudnaudchart.Series["Value"].Points.AddXY("Naud_Min", getmin(nauddata));
                this.maudnaudchart.Series["Value"].Points.AddXY("Naud_Mean", getmean(nauddata));

                maudmaxtext.Text = getmax(mauddata).ToString();
                maudmintext.Text = getmin(mauddata).ToString();
                maudaveragetext.Text = getmean(mauddata).ToString();
                naudmaxtext.Text = getmax(nauddata).ToString();
                naudmintext.Text = getmin(nauddata).ToString();
                naudaveragetext.Text = getmean(nauddata).ToString();
            }
            else
            {
                string e = "No Maud or Naud data, Cant find Vertical Audio Time Stamp";
                Runerror error = new Runerror(e);
                error.Show();
            }
        }
        /// <summary>
        /// Skip states and go to function that will extract Naud and Maud values
        /// </summary>
        /// <param name="state"></param>
        /// <param name="eventcode"></param>
        /// <param name="maudlist"></param>
        /// <param name="naudlist"></param>
        private void extractdata(ref int state, string eventcode, ref List<uint> maudlist, ref List<uint> naudlist)
        {
            int i = 0;
            byte[] dum = null;
            int vchannel = getvc();
            while (i != 3)
            {
                dum = m_IProbe.GetStateData(vchannel, state);
                StringBuilder sb = Geteventcode(dum);
                if (sb.ToString() == eventcode)
                {
                    i++;
                }
                state++;
                this.progressBar1.Increment(1);
            }
            maudlist.Add(getmaudornaud(ref state, eventcode, vchannel));
            i = 0;
            while (i != 2)
            {
                dum = m_IProbe.GetStateData(vchannel, state);
                StringBuilder sb = Geteventcode(dum);
                if (sb.ToString() == eventcode)
                {
                    i++;
                }
                state++;
                this.progressBar1.Increment(1);
            }
            naudlist.Add(getmaudornaud(ref state, eventcode, vchannel));

            while (i != 0)
            {
                dum = m_IProbe.GetStateData(vchannel, state);
                StringBuilder sb = Geteventcode(dum);
                bool bit = getlanebit(dum);
                List<byte> lanedata = Getlanedata(dum);
                byte b = lanedata[0];
                if (sb.ToString() == eventcode)
                {
                    if (b == 253 && bit == true)
                    {
                        break;
                    }
                }
                state++;
                this.progressBar1.Increment(1);
            }
        }
        /// <summary>
        /// Grab the Naud or Maud value and return it as an int
        /// </summary>
        /// <param name="state"></param>
        /// <param name="eventcode"></param>
        /// <param name="vchannel"></param>
        /// <returns></returns>
        private uint getmaudornaud(ref int state, string eventcode, int vchannel)
        {
            uint maudornaud = 0;
            byte[] dum = null;
            List<byte> decoy = new List<byte>();
            bool flag = false;
            int i = 0;
            while (flag != true)
            {
                dum = m_IProbe.GetStateData(vchannel, state);
                StringBuilder sb = Geteventcode(dum);
                if (sb.ToString() == eventcode)
                {
                    List<byte> lanedata = Getlanedata(dum);
                    decoy.Add(lanedata[checklanes(lanedata)]);
                    i++;
                }
                state++;
                this.progressBar1.Increment(1);
                if (i == 3)
                {
                    byte[] decoy2 = { 0, decoy[0], decoy[1], decoy[2] };
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(decoy2);
                    maudornaud = BitConverter.ToUInt32(decoy2, 0);
                    flag = true;
                }
            }
            return maudornaud;
        }
        /// <summary>
        /// Check other values in lane.
        /// </summary>
        /// <param name="lanedata"></param>
        /// <returns></returns>
        private int checklanes(List<byte> lanedata)
        {
            int index = 5;
            int byte1 = lanedata[0];
            int byte2 = lanedata[0];
            int byte3 = lanedata[0];
            int byte4 = lanedata[0];
            if ((byte1 == byte2 && byte1 == byte3) || (byte1 == byte2 && byte1 == byte4) 
                || (byte1 == byte3 && byte1 == byte4))
            {
                index = 0;
            }
            else if ((byte2 == byte1 && byte2 == byte3) || (byte2 == byte1 && byte2 == byte4)
                || (byte2 == byte3 && byte2 == byte4))
            {
                index = 1;
            }
            else if ((byte3 == byte2 && byte3 == byte1) || (byte3 == byte2 && byte3 == byte4)
                || (byte3 == byte1 && byte3 == byte4))
            {
                index = 2;
            }
            else if ((byte4 == byte2 && byte4 == byte3) || (byte4 == byte2 && byte4 == byte1)
                || (byte4 == byte3 && byte4 == byte1))
            {
                index = 3;
            }
            return index;
        }
        /// <summary>
        /// returns max of a list
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private double getmax(List<uint> list)
        {
            uint max = 0;
            int i = 0;
            for (i = 0; i < list.Count; i++)
            {
                if (list[i] > max)
                {
                    max = list[i];
                }
            }
            return Convert.ToDouble(max);
        }
        /// <summary>
        /// returns min of a list
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private double getmin(List<uint> list)
        {
            uint min = list[0];
            int i = 0;
            for (i = 0; i < list.Count; i++)
            {
                if (list[i] < min)
                {
                    min = list[i];
                }
            }
            return Convert.ToDouble(min);
        }
        /// <summary>
        /// returns mean of a list
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private double getmean(List<uint> list)
        {
            int i = 0;
            double sum = 0;
            for (i = 0; i < list.Count; i++)
            {
                sum += list[i];
            }
            double mean = (sum / list.Count);
            mean = Math.Round(mean, 2);
            return mean;
        }
        #endregion //Private Methods
    }
}
