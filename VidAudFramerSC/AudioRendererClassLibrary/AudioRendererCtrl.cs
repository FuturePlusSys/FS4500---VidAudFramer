using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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
using AxWMPLib;
using WMPLib;

namespace AudioRendererClassLibrary
{
    public partial class AudioRendererCtrl : UserControl
    {
        #region Encapsulated Classes

        private class GetAudioDataStateObject
        {
            public int linkWidth { get; set; }
            public int vchannel { get; set; }
            public long numOfStates { get; set; }
            public long startState { get; set; }
            public long endState { get; set; }
            public int bitsPerSample { get; set; }

            public CancellationToken Token { get; set; }

            public GetAudioDataStateObject(int LinkWidth, int VChannel, long NumOfStates, long StartState, long EndState, int BitsPerSample, CancellationToken ct)
            {
                linkWidth = LinkWidth;
                vchannel = VChannel;
                numOfStates = NumOfStates;
                startState = StartState;
                endState = EndState;
                bitsPerSample = BitsPerSample;
                Token = ct;
            }
        }

        private class GetMaudNaudDataStateObject
        {
            public int linkWidth { get; set; }
            public int vchannel { get; set; }
            public long numOfStates { get; set; }
            public long startState { get; set; }
            public long endState { get; set; }

            public CancellationToken Token { get; set; }

            public GetMaudNaudDataStateObject(int LinkWidth, int VChannel, long NumOfStates, long StartState, long EndState, CancellationToken ct)
            {
                linkWidth = LinkWidth;
                vchannel = VChannel;
                numOfStates = NumOfStates;
                startState = StartState;
                endState = EndState;
                Token = ct;
            }
        }

        private class MaudNaudStatistics
        {
            public uint MaudMin { get; set; }
            public uint MaudMax { get; set; }
            public uint MaudAverage { get; set; }
            public long MaudSum { get; set; }

            public uint NaudMin { get; set; }
            public uint NaudMax { get; set; }
            public uint NaudAverage { get; set; }
            public long NaudSum { get; set; }

            public int NumOfTimeStampSDPs { get; set; }
            public int NumOfVerticalTimeStampSDPs { get; set; }
        }

        #endregion // Encapsulated Classes

        #region Members

        private enum ProcessModeID { OutOfPkt, StartOfPkt, InPkt, EndOfPkt, Unknown }
        private const byte Horizontal_AudioPkt = 0x20;
        private const byte Vertical_AudioPkt = 0x60;
        private const byte Horizontal_AudioTSPkt = 0x24;
        private const byte Vertical_AudioTSPkt = 0x64;
        private const ushort SS_CTRL_CHAR = 0x15C;
        private const ushort SE_CTRL_CHAR = 0x1FD;


        //
        // Audio Data Parameters
        //
        private byte m_S0_Ch1_B0 = 0x00;
        private byte m_S0_Ch1_B1 = 0x00;
        private byte m_S0_Ch1_B2 = 0x00;
        private byte m_S0_Ch1_B3 = 0x00;

        private byte m_S0_Ch2_B0 = 0x00;
        private byte m_S0_Ch2_B1 = 0x00;
        private byte m_S0_Ch2_B2 = 0x00;
        private byte m_S0_Ch2_B3 = 0x00;

        private byte m_S0_Ch3_B0 = 0x00;
        private byte m_S0_Ch3_B1 = 0x00;
        private byte m_S0_Ch3_B2 = 0x00;
        private byte m_S0_Ch3_B3 = 0x00;

        private byte m_S0_Ch4_B0 = 0x00;
        private byte m_S0_Ch4_B1 = 0x00;
        private byte m_S0_Ch4_B2 = 0x00;
        private byte m_S0_Ch4_B3 = 0x00;

        private byte m_S0_Ch5_B0 = 0x00;
        private byte m_S0_Ch5_B1 = 0x00;
        private byte m_S0_Ch5_B2 = 0x00;
        private byte m_S0_Ch5_B3 = 0x00;

        private byte m_S0_Ch6_B0 = 0x00;
        private byte m_S0_Ch6_B1 = 0x00;
        private byte m_S0_Ch6_B2 = 0x00;
        private byte m_S0_Ch6_B3 = 0x00;

        private byte m_S0_Ch7_B0 = 0x00;
        private byte m_S0_Ch7_B1 = 0x00;
        private byte m_S0_Ch7_B2 = 0x00;
        private byte m_S0_Ch7_B3 = 0x00;

        private byte m_S0_Ch8_B0 = 0x00;
        private byte m_S0_Ch8_B1 = 0x00;
        private byte m_S0_Ch8_B2 = 0x00;
        private byte m_S0_Ch8_B3 = 0x00;


        private int m_S0_Ch1_B0_pktStateID = 0;
        private int m_S0_Ch1_B1_pktStateID = 0;
        private int m_S0_Ch1_B2_pktStateID = 0;
        private int m_S0_Ch1_B3_pktStateID = 0;

        private int m_S0_Ch2_B0_pktStateID = 0;
        private int m_S0_Ch2_B1_pktStateID = 0;
        private int m_S0_Ch2_B2_pktStateID = 0;
        private int m_S0_Ch2_B3_pktStateID = 0;

        private int m_S0_Ch3_B0_pktStateID = 0;
        private int m_S0_Ch3_B1_pktStateID = 0;
        private int m_S0_Ch3_B2_pktStateID = 0;
        private int m_S0_Ch3_B3_pktStateID = 0;

        private int m_S0_Ch4_B0_pktStateID = 0;
        private int m_S0_Ch4_B1_pktStateID = 0;
        private int m_S0_Ch4_B2_pktStateID = 0;
        private int m_S0_Ch4_B3_pktStateID = 0;

        private int m_S0_Ch5_B0_pktStateID = 0;
        private int m_S0_Ch5_B1_pktStateID = 0;
        private int m_S0_Ch5_B2_pktStateID = 0;
        private int m_S0_Ch5_B3_pktStateID = 0;

        private int m_S0_Ch6_B0_pktStateID = 0;
        private int m_S0_Ch6_B1_pktStateID = 0;
        private int m_S0_Ch6_B2_pktStateID = 0;
        private int m_S0_Ch6_B3_pktStateID = 0;

        private int m_S0_Ch7_B0_pktStateID = 0;
        private int m_S0_Ch7_B1_pktStateID = 0;
        private int m_S0_Ch7_B2_pktStateID = 0;
        private int m_S0_Ch7_B3_pktStateID = 0;

        private int m_S0_Ch8_B0_pktStateID = 0;
        private int m_S0_Ch8_B1_pktStateID = 0;
        private int m_S0_Ch8_B2_pktStateID = 0;
        private int m_S0_Ch8_B3_pktStateID = 0;


        //
        // Maud/Naud Data Parameters
        //
        private byte m_Maud_1_B2 = 0x00;      // Mand #1
        private byte m_Maud_1_B1 = 0x00;
        private byte m_Maud_1_B0 = 0x00;

        private byte m_Maud_2_B2 = 0x00;      // Mand #2
        private byte m_Maud_2_B1 = 0x00;
        private byte m_Maud_2_B0 = 0x00;

        private byte m_Maud_3_B2 = 0x00;      // Mand #3
        private byte m_Maud_3_B1 = 0x00;
        private byte m_Maud_3_B0 = 0x00;

        private byte m_Maud_4_B2 = 0x00;      // Mand #4
        private byte m_Maud_4_B1 = 0x00;
        private byte m_Maud_4_B0 = 0x00;


        private byte m_Naud_1_B2 = 0x00;      // Nand #1
        private byte m_Naud_1_B1 = 0x00;
        private byte m_Naud_1_B0 = 0x00;

        private byte m_Naud_2_B2 = 0x00;      // Nand #2
        private byte m_Naud_2_B1 = 0x00;
        private byte m_Naud_2_B0 = 0x00;

        private byte m_Naud_3_B2 = 0x00;      // Nand #3
        private byte m_Naud_3_B1 = 0x00;
        private byte m_Naud_3_B0 = 0x00;

        private byte m_Naud_4_B2 = 0x00;      // Nand #4
        private byte m_Naud_4_B1 = 0x00;
        private byte m_Naud_4_B0 = 0x00;



        private int m_Maud_1_B2_pktStateID = 0;
        private int m_Maud_1_B1_pktStateID = 0;
        private int m_Maud_1_B0_pktStateID = 0;

        private int m_Maud_2_B2_pktStateID = 0;
        private int m_Maud_2_B1_pktStateID = 0;
        private int m_Maud_2_B0_pktStateID = 0;

        private int m_Maud_3_B2_pktStateID = 0;
        private int m_Maud_3_B1_pktStateID = 0;
        private int m_Maud_3_B0_pktStateID = 0;

        private int m_Maud_4_B2_pktStateID = 0;
        private int m_Maud_4_B1_pktStateID = 0;
        private int m_Maud_4_B0_pktStateID = 0;


        private int m_Naud_1_B2_pktStateID = 0;
        private int m_Naud_1_B1_pktStateID = 0;
        private int m_Naud_1_B0_pktStateID = 0;

        private int m_Naud_2_B2_pktStateID = 0;
        private int m_Naud_2_B1_pktStateID = 0;
        private int m_Naud_2_B0_pktStateID = 0;

        private int m_Naud_3_B2_pktStateID = 0;
        private int m_Naud_3_B1_pktStateID = 0;
        private int m_Naud_3_B0_pktStateID = 0;

        private int m_Naud_4_B2_pktStateID = 0;
        private int m_Naud_4_B1_pktStateID = 0;
        private int m_Naud_4_B0_pktStateID = 0;



        IProbeMgrGen2 m_IProbe = null;  

        private DP12SST m_DP12SSTProbe = null;
        private DP12MST m_DP12MSTProbe = null;
        private DP14SST m_DP14SSTProbe = null;
        private DP14MST m_DP14MSTProbe = null;

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

        private string m_defaultFolderPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"FuturePlus\FS4500\Instance1");
        private NAudio.Wave.WaveFileReader m_waveRdr = null;
        private NAudio.Wave.DirectSoundOut m_output = null;


        private CancellationTokenSource m_tokenSource = null;
        private CancellationToken m_token = CancellationToken.None;

        #endregion //Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public AudioRendererCtrl()
        {
            InitializeComponent();

            InitializeSampleRateComboBox();
            InitializeBitsPerSample_ComboBox();
            defaultsetup();
        }

        #endregion // Constructor

        #region Event Handlers

        /// <summary>
        /// Disable Virtual Channels in SST mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DP1_2SSTbutton_CheckedChanged(object sender, EventArgs e)
        {
            if (DP1_2SST_Button.Checked == true)
            {
                resetDPSelection();

                VC1_Button.Enabled = false;
                VC2_Button.Enabled = false;
                VC3_Button.Enabled = false;
                VC4_Button.Enabled = false;
                VC1_Button.Checked = false;
                VC2_Button.Checked = false;
                VC3_Button.Checked = false;
                VC4_Button.Checked = false;
            }
        }


        /// <summary>
        /// Enable Virtual Channels in MST mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DP1_2MSTbutton_CheckedChanged(object sender, EventArgs e)
        {
            if (DP1_2MST_Button.Checked == true)
            {
                resetDPSelection();

                VC1_Button.Enabled = true;
                VC2_Button.Enabled = true;
                VC3_Button.Enabled = true;
                VC4_Button.Enabled = true;
            }
        }


        /// <summary>
        /// Disable Virtual Channels in SST mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DP1_3SSTbutton_CheckedChanged(object sender, EventArgs e)
        {
            if (DP1_4SST_Button.Checked == true)
            {
                resetDPSelection();

                VC1_Button.Enabled = false;
                VC2_Button.Enabled = false;
                VC3_Button.Enabled = false;
                VC4_Button.Enabled = false;
                VC1_Button.Checked = false;
                VC2_Button.Checked = false;
                VC3_Button.Checked = false;
                VC4_Button.Checked = false;
            }
        }


        /// <summary>
        /// Enable Virtual Channels in MST mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DP1_3MSTbutton_CheckedChanged(object sender, EventArgs e)
        {
            if (DP1_4MST_Button.Checked == true)
            {
                resetDPSelection();

                VC1_Button.Enabled = true;
                VC2_Button.Enabled = true;
                VC3_Button.Enabled = true;
                VC4_Button.Enabled = true;
            }
        }


        /// <summary>
        /// Return the Protocol being used
        /// </summary>
        /// <returns></returns>
        private string getprotocol()
        {
            string protocal = "";
            if (DP1_2SST_Button.Checked)
            {
                protocal = "SST-1.2";
            }
            else if (DP1_2MST_Button.Checked)
            {
                protocal = "MST-1.2";
            }
            else if (DP1_4SST_Button.Checked)
            {
                protocal = "SST-1.4";
            }
            else if (DP1_4MST_Button.Checked)
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
        private int getVChannelID()
        {
            int vc = 0;

            if (DP1_2SST_Button.Checked || DP1_4SST_Button.Checked)
            {
                vc = 1;
            }
            else
            {
                if (VC1_Button.Checked)
                    vc = 1;
                else if (VC2_Button.Checked)
                    vc = 2;
                else if (VC3_Button.Checked)
                    vc = 3;
                else if (VC4_Button.Checked)
                    vc = 4;
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
            if (channel2_Checkbox.Checked == true)
            {
                if (channel1_Checkbox.Checked == false)
                {
                    channel1_Checkbox.Checked = true;
                    channel3_Checkbox.Enabled = true;
                    channel4_Checkbox.Enabled = true;
                    channel5_Checkbox.Enabled = true;
                    channel6_Checkbox.Enabled = true;
                    channel7_Checkbox.Enabled = true;
                    channel8_Checkbox.Enabled = true;
                }
                if (channel1_Checkbox.Checked == true)
                {
                    channel3_Checkbox.Enabled = true;
                    channel4_Checkbox.Enabled = true;
                    channel5_Checkbox.Enabled = true;
                    channel6_Checkbox.Enabled = true;
                    channel7_Checkbox.Enabled = true;
                    channel8_Checkbox.Enabled = true;
                }
            }
            if (channel2_Checkbox.Checked == false)
            {
                channel3_Checkbox.Enabled = false;
                channel4_Checkbox.Enabled = false;
                channel5_Checkbox.Enabled = false;
                channel6_Checkbox.Enabled = false;
                channel7_Checkbox.Enabled = false;
                channel8_Checkbox.Enabled = false;
            }
        }


        /// <summary>
        /// Open the other 6 channels if channel 1 and 2 are open, if not, close them.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void channel1checkbox_CheckedChanged(object sender, EventArgs e)
        {
            if (channel1_Checkbox.Checked == false)
            {
                if (channel2_Checkbox.Checked == true)
                {
                    channel2_Checkbox.Checked = false;
                    channel3_Checkbox.Enabled = false;
                    channel4_Checkbox.Enabled = false;
                    channel5_Checkbox.Enabled = false;
                    channel6_Checkbox.Enabled = false;
                    channel7_Checkbox.Enabled = false;
                    channel8_Checkbox.Enabled = false;
                }
            }
            if (channel1_Checkbox.Checked == true)
            {
                if (channel2_Checkbox.Checked == true)
                {
                    channel3_Checkbox.Enabled = true;
                    channel4_Checkbox.Enabled = true;
                    channel5_Checkbox.Enabled = true;
                    channel6_Checkbox.Enabled = true;
                    channel7_Checkbox.Enabled = true;
                    channel8_Checkbox.Enabled = true;
                }
            }
        }


        /// <summary>
        /// Assemble the Maud/Naud statistics
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaudNaudButton_Click(object sender, EventArgs e)
        {
            bool status = true;
            string errorMsg = string.Empty;

            //createInterfaceObject();  // assign the m_IProbe variable.
            //m_IProbe.Initialize();    // get the trace buffer manager object to register itself

            int linkWidth = 0;
            if (LinkWidth_1_Lane_Button.Checked)
                linkWidth = 1;
            else if (LinkWidth_2_Lane_Button.Checked)
                linkWidth = 2;
            else if (LinkWidth_4_Lane_Button.Checked)
                linkWidth = 4;

            Protocol = getprotocol();
            xmlreader();

            if ((int)EndState_NumericUpDown.Value == 0)
            {
                errorMsg = "End State Box can not equal zero";
                status = false;
            }
            else if ((int)EndState_NumericUpDown.Value < (int)StartState_NumericUpDown.Value)
            {
                errorMsg = "Start State can not be greater than End State";
                status = false;
            }

            if (status == false)
                audioRendererStatusStripLabel.Text = errorMsg;
            else
                getMaudNaud(linkWidth);


            if (errorMsg == string.Empty)
                audioRendererStatusStripLabel.Text = "Ready...";
        }


        /// <summary>
        /// Reading in all data the user checked in the form and error checking
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void createbutton_Click(object sender, EventArgs e)
        {
            bool status = true;

            string errorMsg = string.Empty;
            int states = 0;

            CancelButton.Enabled = true;
            audioRendererStatusStripLabel.Text = "Creating File...";

            // PNS modified... will return VC=1 when SST is choosen.
            if ((DP1_2SST_Button.Checked == true || DP1_4SST_Button.Checked == true) && getVChannelID() == 0)
            {
                MessageBox.Show("Setup Error", "Invalid Virtual Channel Selection", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                states = Convert.ToInt32(m_IProbe.GetNumberOfStates(getVChannelID()));
            }


            int linkWidth = 0;
            if (LinkWidth_1_Lane_Button.Checked)
                linkWidth = 1;
            else if (LinkWidth_2_Lane_Button.Checked)
                linkWidth = 2;
            else if (LinkWidth_4_Lane_Button.Checked)
                linkWidth = 4;


            xmlreader();
            errorMsg = string.Empty;

            if (channel1_Checkbox.Checked == false && channel2_Checkbox.Checked == false)
            {
                errorMsg = "Select Audio Channels";
                status = false;
            }

            if ((long)EndState_NumericUpDown.Value == 0)
            {
                errorMsg = "End State Box can not equal zero";
                status = false;
            }

            else if ((long)EndState_NumericUpDown.Value < (int)StartState_NumericUpDown.Value)
            {
                errorMsg = "Start State can not be greater than End State";
                status = false;
            }

            else
            {
                int numOfStates = (int)EndState_NumericUpDown.Value;
                if (numOfStates > states)
                {
                    errorMsg = "Invalid End State Value";
                    status = false;
                }
            }

            if (status == false)
                audioRendererStatusStripLabel.Text = errorMsg;
            else
                getAudio(linkWidth);


            if (errorMsg == string.Empty)
                audioRendererStatusStripLabel.Text = "Ready...";
        }


        /// <summary>
        /// cancel a file create task.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (m_tokenSource != null)
                m_tokenSource.Cancel();
        }


        /// <summary>
        /// Set Path to data file location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Browse_Button_Click(object sender, EventArgs e)
        {
            //SaveFileDialog openFileDialog1 = new SaveFileDialog();
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                DataFolderPath_TextBox.Text = folderDialog.SelectedPath;
            }
        }


        /// <summary>
        /// update the data folder path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetPath_Button_Click(object sender, EventArgs e)
        {
            if (m_IProbe == null)
            {
                Protocol = getprotocol();
                createInterfaceObject();
                m_IProbe.Initialize();

                if (m_IProbe.SetDataFolderPath(DataFolderPath_TextBox.Text) == true)
                {
                    NumOfStates_Button.Enabled = true;
                    Create_Button.Enabled = true;
                    NumOfStates_TextBox.Text = m_IProbe.GetNumberOfStates(getVChannelID()).ToString();
                    EndState_NumericUpDown.Value = m_IProbe.GetNumberOfStates(getVChannelID());
                    MaudNaud_Button.Enabled = true;
                }
            }
            else
            {
                if (m_IProbe.SetDataFolderPath(DataFolderPath_TextBox.Text) == true)
                {
                    NumOfStates_Button.Enabled = true;
                    Create_Button.Enabled = true;
                    NumOfStates_TextBox.Text = m_IProbe.GetNumberOfStates(getVChannelID()).ToString();
                    EndState_NumericUpDown.Value = m_IProbe.GetNumberOfStates(getVChannelID());
                    MaudNaud_Button.Enabled = true;
                }
            }
        }


        /// <summary>
        /// Display the number of states in a read only textbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumOfStates_Button_Click(object sender, EventArgs e)
        {
            if (m_IProbe == null)
            {
                Protocol = getprotocol();
                createInterfaceObject();  // assign the m_IProbe variable.
                m_IProbe.Initialize();
            }

            // set the data folder path for the current probe setup...
            SetPath_Button.PerformClick();

            NumOfStates_TextBox.Text = m_IProbe.GetNumberOfStates(getVChannelID()).ToString();
        }

        /// <summary>
        /// Set the data folder path when the user presses the Return key.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataFolderPath_TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                SetPath_Button.PerformClick();
            }
        }

        #endregion //Event Handlers

        #region Private Methods

        /// <summary>
        /// Initialize the sample rate combo box control
        /// </summary>
        private void InitializeSampleRateComboBox()
        {
            SampleRate_ComboBox.Items.Clear();
            SampleRate_ComboBox.Items.Add("44100");
            SampleRate_ComboBox.Items.Add("48000");
            SampleRate_ComboBox.Items.Add("192000");

            SampleRate_ComboBox.SelectedIndex = 1;
        }


        /// <summary>
        /// Initialize the bits per sample combox box control
        /// </summary>
        private void InitializeBitsPerSample_ComboBox()
        {
            BitsPerSample_ComboBox.Items.Clear();
            BitsPerSample_ComboBox.Items.Add("16");
            BitsPerSample_ComboBox.Items.Add("24");
            //BitsPerSample_ComboBox.Items.Add("32");

            BitsPerSample_ComboBox.SelectedIndex = 0;
        }


        /// <summary>
        /// How the form will first appear
        /// </summary>
        private void defaultsetup()
        {
            DP1_4SST_Button.Checked = true;
            LinkWidth_4_Lane_Button.Checked = true;
            VC1_Button.Checked = true;
            VC1_Button.Enabled = false;
            VC2_Button.Enabled = false;
            VC3_Button.Enabled = false;
            VC4_Button.Enabled = false;


            channel1_Checkbox.Checked = true;
            channel2_Checkbox.Checked = true;
            channel3_Checkbox.Checked = false;
            channel4_Checkbox.Checked = false;
            channel5_Checkbox.Checked = false;
            channel6_Checkbox.Checked = false;
            channel7_Checkbox.Checked = false;
            channel8_Checkbox.Checked = false;

            channel1_Checkbox.Enabled = true;
            channel2_Checkbox.Enabled = true;
            channel3_Checkbox.Enabled = true;
            channel4_Checkbox.Enabled = true;
            channel5_Checkbox.Enabled = false;
            channel6_Checkbox.Enabled = false;
            channel7_Checkbox.Enabled = false;
            channel8_Checkbox.Enabled = false;

            //sampleRate_Textbox.Text = "44100";
            //bitsPerSample_TextBox.Text = "16";

            //MaudNaud_Chart.Visible = true;
            //MaudNaud_Chart.BringToFront();

            //this.MaudNaud_Chart.Series["Value"].Points.AddXY("Maud_Max", 1);
            //this.MaudNaud_Chart.Series["Value"].Points.AddXY("Maud_Min", 2);
            //this.MaudNaud_Chart.Series["Value"].Points.AddXY("Maud_Mean", 3);
            //this.MaudNaud_Chart.Series["Value"].Points.AddXY("Naud_Max", 4);
            //this.MaudNaud_Chart.Series["Value"].Points.AddXY("Naud_Min", 5);
            //this.MaudNaud_Chart.Series["Value"].Points.AddXY("Naud_Mean", 6);

            MaudMax_TextBox.Text = "---"; // getmax(mauddata).ToString();
            MaudMin_TextBox.Text = "---"; // getmin(mauddata).ToString();
            MaudAverage_TextBox.Text = "---"; // getmean(mauddata).ToString();
            NaudMax_TextBox.Text = "---"; // getmax(nauddata).ToString();
            NaudMin_TextBox.Text = "---"; // getmin(nauddata).ToString();
            NaudAverage_TextBox.Text = "---"; // getmean(nauddata).ToString();
            AvgMOverN_TextBox.Text = "---";
            AudioSDP_TextBox.Text = "---";
            AudioVerticalSDP_TextBox.Text = "---";

            //MaudNaud_Chart.Series["Value"].Legend = "Legend1";
            //MaudNaud_Chart.Series["Value"].IsVisibleInLegend = true;


            DataFolderPath_TextBox.Text = m_defaultFolderPath;
            CancelButton.Enabled = false;
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
                    m_DP12SSTProbe = new DP12SST(DataFolderPath_TextBox.Text);
                    m_IProbe = (IProbeMgrGen2)m_DP12SSTProbe;
                    break;
                case "MST-1.2":
                    if (m_DP12MSTProbe != null)
                        m_DP12MSTProbe = null;
                    m_DP12MSTProbe = new DP12MST(DataFolderPath_TextBox.Text);
                    m_IProbe = (IProbeMgrGen2)m_DP12MSTProbe;
                    break;
                case "SST-1.4":
                    if (m_DP14SSTProbe != null)
                        m_DP14SSTProbe = null;
                    m_DP14SSTProbe = new DP14SST(DataFolderPath_TextBox.Text);
                    m_IProbe = m_DP14SSTProbe;
                    break;
                case "MST-1.4":
                    if (m_DP14MSTProbe != null)
                        m_DP14MSTProbe = null;
                    m_DP14MSTProbe = new DP14MST(DataFolderPath_TextBox.Text);
                    m_IProbe = (IProbeMgrGen2)m_DP14MSTProbe;
                    break;
            }
            return status;
        }


        /// <summary>
        /// Reset user selection inputs
        /// </summary>
        private void resetDPSelection()
        {
            m_IProbe = null;

            NumOfStates_TextBox.Text = string.Empty;
            DataFolderPath_TextBox.Text = m_defaultFolderPath;
            NumOfStates_Button.Enabled = false;
            Create_Button.Enabled = false;
            CancelButton.Enabled = false;
            MaudNaud_Button.Enabled = false;

            StartState_NumericUpDown.Value = 0;
            EndState_NumericUpDown.Value = 0;
        }


        /// <summary>
        /// Returns a bool of which channels are open based on the user input.
        /// </summary>
        /// <returns></returns>
        private List<bool> createchannelarray()
        {
            List<bool> channels = new List<bool> { false, false, false, false, false, false, false, false };
            if (channel1_Checkbox.Checked == true)
                channels[0] = true;
            if (channel2_Checkbox.Checked == true)
                channels[1] = true;
            if (channel3_Checkbox.Checked == true)
                channels[2] = true;
            if (channel4_Checkbox.Checked == true)
                channels[3] = true;
            //if (channel5_Checkbox.Checked == true)
            //    channels[4] = true;
            //if (channel6_Checkbox.Checked == true)
            //    channels[5] = true;
            //if (channel7_Checkbox.Checked == true)
            //    channels[6] = true;
            //if (channel8_Checkbox.Checked == true)
            //    channels[7] = true;
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
        private StringBuilder GetEventCode(byte[] dataBytes)
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
        /// Return a StringBuilder of the event code
        /// </summary>
        /// <param name="dataBytes"></param>
        /// <returns></returns>
        private byte GetEventCodeII(byte[] dataBytes)
        {
            byte result = 0x00;
            if (dataBytes != null)
            {
                StringBuilder sb = new StringBuilder();
                byte bits = dataBytes[(15 - Eventsb / EventWidth)];
                byte bits2 = dataBytes[(15 - (Eventsb - 7) / EventWidth)];
                result = (byte)(bits << EventWidth - ((Eventsb % EventWidth) + 1) | (bits2 >> (Eventsb - 7) % EventWidth));
                //sb.Append("0x" + result.ToString("X2"));
                //return sb;
            }

            // return null;
            return result;
        }



        /// <summary>
        /// Returns a list of lane data for all four lanes
        /// </summary>
        /// <param name="dataBytes"></param>
        /// <returns></returns>
        private List<byte> GetLaneData(byte[] dataBytes)
        {
            List<byte> result = new List<Byte>();

            result.Add((byte)(((dataBytes[11] & 0x3F) << 2) | ((dataBytes[12] & 0xC0) >> 6)));
            result.Add((byte)(((dataBytes[12] & 0x0F) << 4) | ((dataBytes[13] & 0xF0) >> 4)));
            result.Add((byte)(((dataBytes[13] & 0x03) << 6) | ((dataBytes[14] & 0xFC) >> 2)));
            result.Add((byte)(dataBytes[15]));

            return result;
            //List<byte> result = new List<Byte>();
            //int startbit = 0;
            //int i = 0;
            //while (i != 4)
            //{
            //    switch (i)
            //    {
            //        case 0:
            //            startbit = Lane0sb;
            //            break;
            //        case 1:
            //            startbit = Lane1sb;
            //            break;
            //        case 2:
            //            startbit = Lane2sb;
            //            break;
            //        case 3:
            //            startbit = Lane3sb;
            //            break;

            //        default:
            //            break;
            //    }

            //    //StringBuilder sb = new StringBuilder();
            //    byte bits = dataBytes[15 - (startbit / 8)];
            //    byte bits2 = dataBytes[(15 - (startbit - 7) / 8)];
            //    result.Add((byte)(bits << 8 - ((startbit % 8) + 1) | (bits2 >> (startbit - 7) % 8)));
            //    i++;
            //}

            //return result;
        }
        /// <summary>
        /// For eventcode ending in 1FD, check if the bit before the FD is a 1 or 0.
        /// </summary>
        /// <param name="dataBytes"></param>
        /// <returns></returns>
        private bool getLaneBit(byte[] dataBytes)
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
        /// <summary>
        /// Allows user the open a wave file from their computer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void getfilebutton_Click(object sender, EventArgs e)
        {
            disposeWave();
            SelectedWaveFile_TextBox.Text = string.Empty;


            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Wave File (*.wav)|*.wav;";
            if (open.ShowDialog() == DialogResult.OK)
            {
                //m_waveRdr = new NAudio.Wave.WaveFileReader(open.FileName);
                SelectedWaveFile_TextBox.Text = open.FileName;
                wave = new NAudio.Wave.WaveFileReader(open.FileName);
                axWindowsMediaPlayer1.URL = open.FileName;
            }

            return;
        }


        /// <summary>
        /// De-allocation the resourses assoicate with a .Wav file that has been selected.
        /// </summary>
        private void disposeWave()
        {
            m_output = new NAudio.Wave.DirectSoundOut();
            if (m_output.PlaybackState == NAudio.Wave.PlaybackState.Playing)
                m_output.Stop();

            m_output.Dispose();
            m_output = null;

            //if (m_waveRdr != null)
            //{
            //    m_waveRdr.Dispose();
            //    m_waveRdr = null;
            //}
        }


        /// <summary>
        /// Process closing the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosedEventArgs e)
        {
            disposeWave();
        }


        /// <summary>
        /// Disable various controls when the audio data is being gathered on a seperate task
        /// </summary>
        /// <param name="isEnabled"></param>
        private void setAudioRenderedEnabledProperty(bool isEnabled)
        {
            foreach (Control control in DataPath_Panel.Controls)
            {
                if (control.Name == "CancelButton")
                {
                    if (isEnabled == false)
                    {
                        CancelButton.Enabled = true;
                    }
                    else
                    {
                        CancelButton.Enabled = false;
                    }
                }
                else
                {
                    control.Enabled = isEnabled;
                }
            }
            MaudNaud_Button.Enabled = isEnabled;
            Create_Button.Enabled = isEnabled;
            MaudNaud_Button.Enabled = isEnabled;

            //axWindowsMediaPlayer1.Ctlenabled = isEnabled;
            getFile_Button.Enabled = isEnabled;
            NumOfStates_Button.Enabled = isEnabled;
        }


        /// <summary>
        /// Reset the channel sample data
        /// </summary>
        private void resetChannelDataBytes()
        {
            m_S0_Ch1_B0 = 0x00;
            m_S0_Ch1_B1 = 0x00;
            m_S0_Ch1_B2 = 0x00;
            m_S0_Ch1_B3 = 0x00;

            m_S0_Ch2_B0 = 0x00;
            m_S0_Ch2_B1 = 0x00;
            m_S0_Ch2_B2 = 0x00;
            m_S0_Ch2_B3 = 0x00;

            m_S0_Ch3_B0 = 0x00;
            m_S0_Ch3_B1 = 0x00;
            m_S0_Ch3_B2 = 0x00;
            m_S0_Ch3_B3 = 0x00;

            m_S0_Ch4_B0 = 0x00;
            m_S0_Ch4_B1 = 0x00;
            m_S0_Ch4_B2 = 0x00;
            m_S0_Ch4_B3 = 0x00;

            m_S0_Ch5_B0 = 0x00;
            m_S0_Ch5_B1 = 0x00;
            m_S0_Ch5_B2 = 0x00;
            m_S0_Ch5_B3 = 0x00;

            m_S0_Ch6_B0 = 0x00;
            m_S0_Ch6_B1 = 0x00;
            m_S0_Ch6_B2 = 0x00;
            m_S0_Ch6_B3 = 0x00;

            m_S0_Ch7_B0 = 0x00;
            m_S0_Ch7_B1 = 0x00;
            m_S0_Ch7_B2 = 0x00;
            m_S0_Ch7_B3 = 0x00;

            m_S0_Ch8_B0 = 0x00;
            m_S0_Ch8_B1 = 0x00;
            m_S0_Ch8_B2 = 0x00;
            m_S0_Ch8_B3 = 0x00;
        }

        /// <summary>
        /// Get the InvalidBit | CtrlCharBit | 8bit of data 
        /// </summary>
        /// Assumes the data is always at the same place in the TB state data...
        /// <param name="laneID"></param>
        /// <param name="stateData"></param>
        /// <returns></returns>
        private ushort getLaneData(int laneID, byte[] stateData)
        {
            ushort laneValue = 0x00;
            switch (laneID)
            {
                case 0:
                    laneValue = (ushort)(stateData[11] << 2 | ((stateData[12] & 0xC0) >> 6));
                    break;

                case 1:
                    laneValue = (ushort)(((stateData[12] & 0x3F) << 4) | ((stateData[13] & 0xF0) >> 4));
                    break;

                case 2:
                    laneValue = (ushort)(((stateData[13] & 0x0F) << 6) | ((stateData[14] & 0xFC) >> 2));
                    break;

                case 3:
                    laneValue = (ushort)(((stateData[14] & 0x03) << 9) | (stateData[15] & 0xFF));
                    break;

                default:
                    break;
            }

            return laneValue;
        }


        /// <summary>
        /// Get the lane byte value.
        /// </summary>
        /// <param name="laneID"></param>
        /// <param name="stateData"></param>
        /// <returns></returns>
        private byte getLaneDataByte(int laneID, byte[] stateData)
        {
            byte laneValue = 0x00;
            switch (laneID)
            {
                case 0:
                    laneValue = (byte)(((stateData[11] & 0x3F) << 2) | ((stateData[12] & 0xC0) >> 6));
                    break;

                case 1:
                    laneValue = (byte)(((stateData[12] & 0x0F) << 4) | ((stateData[13] & 0xF0) >> 4));
                    break;

                case 2:
                    laneValue = (byte)(((stateData[13] & 0x03) << 6) | ((stateData[14] & 0xFC) >> 2));
                    break;

                case 3:
                    laneValue = (byte)(stateData[15]);
                    break;

                default:
                    break;
            }

            return laneValue;
        }


        /// <summary>
        /// set the variables that idenify the pkt state index of the two data bytes of interest.
        /// </summary>
        /// <param name="linkWidth"></param>
        private void setLaneDataLocations(int linkWidth)
        {
            switch (linkWidth)
            {
                case 1:
                    m_S0_Ch1_B0_pktStateID = 9;
                    m_S0_Ch1_B1_pktStateID = 10;
                    m_S0_Ch1_B2_pktStateID = 11;
                    m_S0_Ch1_B3_pktStateID = 12;

                    m_S0_Ch2_B0_pktStateID = 14;
                    m_S0_Ch2_B1_pktStateID = 15;
                    m_S0_Ch2_B2_pktStateID = 16;
                    m_S0_Ch2_B3_pktStateID = 17;

                    m_S0_Ch3_B0_pktStateID = 19;
                    m_S0_Ch3_B1_pktStateID = 20;
                    m_S0_Ch3_B2_pktStateID = 21;
                    m_S0_Ch3_B3_pktStateID = 22;

                    m_S0_Ch4_B0_pktStateID = 24;
                    m_S0_Ch4_B1_pktStateID = 25;
                    m_S0_Ch4_B2_pktStateID = 26;
                    m_S0_Ch4_B3_pktStateID = 27;

                    m_S0_Ch5_B0_pktStateID = 29;
                    m_S0_Ch5_B1_pktStateID = 30;
                    m_S0_Ch5_B2_pktStateID = 31;
                    m_S0_Ch5_B3_pktStateID = 32;

                    m_S0_Ch6_B0_pktStateID = 34;
                    m_S0_Ch6_B1_pktStateID = 35;
                    m_S0_Ch6_B2_pktStateID = 36;
                    m_S0_Ch6_B3_pktStateID = 37;

                    m_S0_Ch7_B0_pktStateID = 39;
                    m_S0_Ch7_B1_pktStateID = 40;
                    m_S0_Ch7_B2_pktStateID = 41;
                    m_S0_Ch7_B3_pktStateID = 42;

                    m_S0_Ch8_B0_pktStateID = 44;
                    m_S0_Ch8_B1_pktStateID = 45;
                    m_S0_Ch8_B2_pktStateID = 46;
                    m_S0_Ch8_B3_pktStateID = 47;
                    break;

                case 2:
                    m_S0_Ch1_B0_pktStateID = 5;
                    m_S0_Ch1_B1_pktStateID = 6;
                    m_S0_Ch1_B2_pktStateID = 7;
                    m_S0_Ch1_B3_pktStateID = 8;

                    m_S0_Ch2_B0_pktStateID = 5;
                    m_S0_Ch2_B1_pktStateID = 6;
                    m_S0_Ch2_B2_pktStateID = 7;
                    m_S0_Ch2_B3_pktStateID = 8;

                    m_S0_Ch3_B0_pktStateID = 10;
                    m_S0_Ch3_B1_pktStateID = 11;
                    m_S0_Ch3_B2_pktStateID = 12;
                    m_S0_Ch3_B3_pktStateID = 13;

                    m_S0_Ch4_B0_pktStateID = 10;
                    m_S0_Ch4_B1_pktStateID = 11;
                    m_S0_Ch4_B2_pktStateID = 12;
                    m_S0_Ch4_B3_pktStateID = 13;

                    m_S0_Ch5_B0_pktStateID = 15;
                    m_S0_Ch5_B1_pktStateID = 16;
                    m_S0_Ch5_B2_pktStateID = 17;
                    m_S0_Ch5_B3_pktStateID = 18;

                    m_S0_Ch6_B0_pktStateID = 15;
                    m_S0_Ch6_B1_pktStateID = 16;
                    m_S0_Ch6_B2_pktStateID = 17;
                    m_S0_Ch6_B3_pktStateID = 18;

                    m_S0_Ch7_B0_pktStateID = 20;
                    m_S0_Ch7_B1_pktStateID = 21;
                    m_S0_Ch7_B2_pktStateID = 22;
                    m_S0_Ch7_B3_pktStateID = 23;

                    m_S0_Ch8_B0_pktStateID = 20;
                    m_S0_Ch8_B1_pktStateID = 21;
                    m_S0_Ch8_B2_pktStateID = 22;
                    m_S0_Ch8_B3_pktStateID = 23;
                    break;

                case 4:
                    m_S0_Ch1_B0_pktStateID = 3;
                    m_S0_Ch1_B1_pktStateID = 4;
                    m_S0_Ch1_B2_pktStateID = 5;
                    m_S0_Ch1_B3_pktStateID = 6;

                    m_S0_Ch2_B0_pktStateID = 3;
                    m_S0_Ch2_B1_pktStateID = 4;
                    m_S0_Ch2_B2_pktStateID = 5;
                    m_S0_Ch2_B3_pktStateID = 6;

                    m_S0_Ch3_B0_pktStateID = 3;
                    m_S0_Ch3_B1_pktStateID = 4;
                    m_S0_Ch3_B2_pktStateID = 5;
                    m_S0_Ch3_B3_pktStateID = 6;

                    m_S0_Ch4_B0_pktStateID = 3;
                    m_S0_Ch4_B1_pktStateID = 4;
                    m_S0_Ch4_B2_pktStateID = 5;
                    m_S0_Ch4_B3_pktStateID = 6;

                    m_S0_Ch5_B0_pktStateID = 8;
                    m_S0_Ch5_B1_pktStateID = 9;
                    m_S0_Ch5_B2_pktStateID = 10;
                    m_S0_Ch5_B3_pktStateID = 11;

                    m_S0_Ch6_B0_pktStateID = 8;
                    m_S0_Ch6_B1_pktStateID = 9;
                    m_S0_Ch6_B2_pktStateID = 10;
                    m_S0_Ch6_B3_pktStateID = 11;

                    m_S0_Ch7_B0_pktStateID = 8;
                    m_S0_Ch7_B1_pktStateID = 9;
                    m_S0_Ch7_B2_pktStateID = 10;
                    m_S0_Ch7_B3_pktStateID = 11;

                    m_S0_Ch8_B0_pktStateID = 8;
                    m_S0_Ch8_B1_pktStateID = 9;
                    m_S0_Ch8_B2_pktStateID = 10;
                    m_S0_Ch8_B3_pktStateID = 11;
                    break;

                default:
                    m_S0_Ch1_B0_pktStateID = -1;
                    m_S0_Ch1_B1_pktStateID = -1;
                    m_S0_Ch1_B2_pktStateID = -1;
                    m_S0_Ch1_B3_pktStateID = -1;

                    m_S0_Ch2_B0_pktStateID = -1;
                    m_S0_Ch2_B1_pktStateID = -1;
                    m_S0_Ch2_B2_pktStateID = -1;
                    m_S0_Ch2_B3_pktStateID = -1;

                    m_S0_Ch3_B0_pktStateID = -1;
                    m_S0_Ch3_B1_pktStateID = -1;
                    m_S0_Ch3_B2_pktStateID = -1;
                    m_S0_Ch3_B3_pktStateID = -1;

                    m_S0_Ch4_B0_pktStateID = -1;
                    m_S0_Ch4_B1_pktStateID = -1;
                    m_S0_Ch4_B2_pktStateID = -1;
                    m_S0_Ch4_B3_pktStateID = -1;

                    m_S0_Ch5_B0_pktStateID = -1;
                    m_S0_Ch5_B1_pktStateID = -1;
                    m_S0_Ch5_B2_pktStateID = -1;
                    m_S0_Ch5_B3_pktStateID = -1;

                    m_S0_Ch6_B0_pktStateID = -1;
                    m_S0_Ch6_B1_pktStateID = -1;
                    m_S0_Ch6_B2_pktStateID = -1;
                    m_S0_Ch6_B3_pktStateID = -1;

                    m_S0_Ch7_B0_pktStateID = -1;
                    m_S0_Ch7_B1_pktStateID = -1;
                    m_S0_Ch7_B2_pktStateID = -1;
                    m_S0_Ch7_B3_pktStateID = -1;

                    m_S0_Ch8_B0_pktStateID = -1;
                    m_S0_Ch8_B1_pktStateID = -1;
                    m_S0_Ch8_B2_pktStateID = -1;
                    m_S0_Ch8_B3_pktStateID = -1;
                    break;
            }
        }


        /// <summary>
        /// Append a sample/channel data byte if appopriate.
        /// </summary>
        /// <param name="linkWidth"></param>
        /// <param name="pktStateIndex"></param>
        /// <param name="stateData"></param>
        private void updateChannelDataValues(int linkWidth, int pktStateIndex, byte[] stateData)
        {
            switch (linkWidth)
            {
                case 1:
                    if (pktStateIndex == m_S0_Ch1_B0_pktStateID)
                        m_S0_Ch1_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch1_B1_pktStateID)
                        m_S0_Ch1_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch1_B2_pktStateID)
                        m_S0_Ch1_B2 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch1_B3_pktStateID)
                        m_S0_Ch1_B3 = getLaneDataByte(0, stateData);

                    else if (pktStateIndex == m_S0_Ch2_B0_pktStateID)
                        m_S0_Ch2_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch2_B1_pktStateID)
                        m_S0_Ch2_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch2_B2_pktStateID)
                        m_S0_Ch2_B2 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch2_B3_pktStateID)
                        m_S0_Ch2_B3 = getLaneDataByte(0, stateData);

                    else if (pktStateIndex == m_S0_Ch3_B0_pktStateID)
                        m_S0_Ch3_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch3_B1_pktStateID)
                        m_S0_Ch3_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch3_B2_pktStateID)
                        m_S0_Ch3_B2 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch3_B3_pktStateID)
                        m_S0_Ch3_B3 = getLaneDataByte(0, stateData);

                    else if (pktStateIndex == m_S0_Ch4_B0_pktStateID)
                        m_S0_Ch4_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch4_B1_pktStateID)
                        m_S0_Ch4_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch4_B2_pktStateID)
                        m_S0_Ch4_B2 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch4_B3_pktStateID)
                        m_S0_Ch4_B3 = getLaneDataByte(0, stateData);



                    if (pktStateIndex == m_S0_Ch5_B0_pktStateID)
                        m_S0_Ch5_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch5_B1_pktStateID)
                        m_S0_Ch5_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch5_B2_pktStateID)
                        m_S0_Ch5_B2 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch5_B3_pktStateID)
                        m_S0_Ch5_B3 = getLaneDataByte(0, stateData);

                    else if (pktStateIndex == m_S0_Ch6_B0_pktStateID)
                        m_S0_Ch6_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch6_B1_pktStateID)
                        m_S0_Ch6_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch6_B2_pktStateID)
                        m_S0_Ch6_B2 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch6_B3_pktStateID)
                        m_S0_Ch6_B3 = getLaneDataByte(0, stateData);

                    else if (pktStateIndex == m_S0_Ch7_B0_pktStateID)
                        m_S0_Ch7_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch7_B1_pktStateID)
                        m_S0_Ch7_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch7_B2_pktStateID)
                        m_S0_Ch7_B2 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch7_B3_pktStateID)
                        m_S0_Ch7_B3 = getLaneDataByte(0, stateData);

                    else if (pktStateIndex == m_S0_Ch8_B0_pktStateID)
                        m_S0_Ch8_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch8_B1_pktStateID)
                        m_S0_Ch8_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch8_B2_pktStateID)
                        m_S0_Ch8_B2 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch8_B3_pktStateID)
                        m_S0_Ch8_B3 = getLaneDataByte(0, stateData);

                    break;

                case 2:
                    if (pktStateIndex == m_S0_Ch1_B0_pktStateID)
                        m_S0_Ch1_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch1_B1_pktStateID)
                        m_S0_Ch1_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch1_B2_pktStateID)
                        m_S0_Ch1_B2 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch1_B3_pktStateID)
                        m_S0_Ch1_B3 = getLaneDataByte(0, stateData);


                    if (pktStateIndex == m_S0_Ch2_B0_pktStateID)
                        m_S0_Ch2_B0 = getLaneDataByte(1, stateData);
                    else if (pktStateIndex == m_S0_Ch2_B1_pktStateID)
                        m_S0_Ch2_B1 = getLaneDataByte(1, stateData);
                    else if (pktStateIndex == m_S0_Ch2_B2_pktStateID)
                        m_S0_Ch2_B2 = getLaneDataByte(1, stateData);
                    else if (pktStateIndex == m_S0_Ch2_B3_pktStateID)
                        m_S0_Ch2_B3 = getLaneDataByte(1, stateData);


                    if (pktStateIndex == m_S0_Ch3_B0_pktStateID)
                        m_S0_Ch3_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch3_B1_pktStateID)
                        m_S0_Ch3_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch3_B2_pktStateID)
                        m_S0_Ch3_B2 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch3_B3_pktStateID)
                        m_S0_Ch3_B3 = getLaneDataByte(0, stateData);


                    if (pktStateIndex == m_S0_Ch2_B0_pktStateID)
                        m_S0_Ch4_B0 = getLaneDataByte(1, stateData);
                    else if (pktStateIndex == m_S0_Ch2_B1_pktStateID)
                        m_S0_Ch4_B1 = getLaneDataByte(1, stateData);
                    else if (pktStateIndex == m_S0_Ch2_B2_pktStateID)
                        m_S0_Ch4_B2 = getLaneDataByte(1, stateData);
                    else if (pktStateIndex == m_S0_Ch2_B3_pktStateID)
                        m_S0_Ch4_B3 = getLaneDataByte(1, stateData);



                    if (pktStateIndex == m_S0_Ch5_B0_pktStateID)
                        m_S0_Ch5_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch5_B1_pktStateID)
                        m_S0_Ch5_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch5_B2_pktStateID)
                        m_S0_Ch5_B2 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch5_B3_pktStateID)
                        m_S0_Ch5_B3 = getLaneDataByte(0, stateData);


                    if (pktStateIndex == m_S0_Ch6_B0_pktStateID)
                        m_S0_Ch6_B0 = getLaneDataByte(1, stateData);
                    else if (pktStateIndex == m_S0_Ch6_B1_pktStateID)
                        m_S0_Ch6_B1 = getLaneDataByte(1, stateData);
                    else if (pktStateIndex == m_S0_Ch6_B2_pktStateID)
                        m_S0_Ch6_B2 = getLaneDataByte(1, stateData);
                    else if (pktStateIndex == m_S0_Ch6_B3_pktStateID)
                        m_S0_Ch6_B3 = getLaneDataByte(1, stateData);


                    if (pktStateIndex == m_S0_Ch7_B0_pktStateID)
                        m_S0_Ch7_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch7_B1_pktStateID)
                        m_S0_Ch7_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch7_B2_pktStateID)
                        m_S0_Ch7_B2 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch7_B3_pktStateID)
                        m_S0_Ch7_B3 = getLaneDataByte(0, stateData);


                    if (pktStateIndex == m_S0_Ch8_B0_pktStateID)
                        m_S0_Ch8_B0 = getLaneDataByte(1, stateData);
                    else if (pktStateIndex == m_S0_Ch8_B1_pktStateID)
                        m_S0_Ch8_B1 = getLaneDataByte(1, stateData);
                    else if (pktStateIndex == m_S0_Ch8_B2_pktStateID)
                        m_S0_Ch8_B2 = getLaneDataByte(1, stateData);
                    else if (pktStateIndex == m_S0_Ch8_B3_pktStateID)
                        m_S0_Ch8_B3 = getLaneDataByte(1, stateData);

                    break;

                case 4:
                    if (pktStateIndex == m_S0_Ch1_B0_pktStateID)
                        m_S0_Ch1_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch1_B1_pktStateID)
                        m_S0_Ch1_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch1_B2_pktStateID)
                        m_S0_Ch1_B2 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch1_B3_pktStateID)
                        m_S0_Ch1_B3 = getLaneDataByte(0, stateData);


                    if (pktStateIndex == m_S0_Ch2_B0_pktStateID)
                        m_S0_Ch2_B0 = getLaneDataByte(1, stateData);
                    else if (pktStateIndex == m_S0_Ch2_B1_pktStateID)
                        m_S0_Ch2_B1 = getLaneDataByte(1, stateData);
                    else if (pktStateIndex == m_S0_Ch2_B2_pktStateID)
                        m_S0_Ch2_B2 = getLaneDataByte(1, stateData);
                    else if (pktStateIndex == m_S0_Ch2_B3_pktStateID)
                        m_S0_Ch2_B3 = getLaneDataByte(1, stateData);


                    if (pktStateIndex == m_S0_Ch3_B0_pktStateID)
                        m_S0_Ch3_B0 = getLaneDataByte(2, stateData);
                    else if (pktStateIndex == m_S0_Ch3_B1_pktStateID)
                        m_S0_Ch3_B1 = getLaneDataByte(2, stateData);
                    else if (pktStateIndex == m_S0_Ch3_B2_pktStateID)
                        m_S0_Ch3_B2 = getLaneDataByte(2, stateData);
                    else if (pktStateIndex == m_S0_Ch3_B3_pktStateID)
                        m_S0_Ch3_B3 = getLaneDataByte(2, stateData);


                    if (pktStateIndex == m_S0_Ch2_B0_pktStateID)
                        m_S0_Ch4_B0 = getLaneDataByte(3, stateData);
                    else if (pktStateIndex == m_S0_Ch2_B1_pktStateID)
                        m_S0_Ch4_B1 = getLaneDataByte(3, stateData);
                    else if (pktStateIndex == m_S0_Ch2_B2_pktStateID)
                        m_S0_Ch4_B2 = getLaneDataByte(3, stateData);
                    else if (pktStateIndex == m_S0_Ch2_B3_pktStateID)
                        m_S0_Ch4_B3 = getLaneDataByte(3, stateData);



                    if (pktStateIndex == m_S0_Ch5_B0_pktStateID)
                        m_S0_Ch5_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch5_B1_pktStateID)
                        m_S0_Ch5_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch5_B2_pktStateID)
                        m_S0_Ch5_B2 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_S0_Ch5_B3_pktStateID)
                        m_S0_Ch5_B3 = getLaneDataByte(0, stateData);


                    if (pktStateIndex == m_S0_Ch6_B0_pktStateID)
                        m_S0_Ch6_B0 = getLaneDataByte(1, stateData);
                    else if (pktStateIndex == m_S0_Ch6_B1_pktStateID)
                        m_S0_Ch6_B1 = getLaneDataByte(1, stateData);
                    else if (pktStateIndex == m_S0_Ch6_B2_pktStateID)
                        m_S0_Ch6_B2 = getLaneDataByte(1, stateData);
                    else if (pktStateIndex == m_S0_Ch6_B3_pktStateID)
                        m_S0_Ch6_B3 = getLaneDataByte(1, stateData);


                    if (pktStateIndex == m_S0_Ch7_B0_pktStateID)
                        m_S0_Ch7_B0 = getLaneDataByte(2, stateData);
                    else if (pktStateIndex == m_S0_Ch7_B1_pktStateID)
                        m_S0_Ch7_B1 = getLaneDataByte(2, stateData);
                    else if (pktStateIndex == m_S0_Ch7_B2_pktStateID)
                        m_S0_Ch7_B2 = getLaneDataByte(2, stateData);
                    else if (pktStateIndex == m_S0_Ch7_B3_pktStateID)
                        m_S0_Ch7_B3 = getLaneDataByte(2, stateData);


                    if (pktStateIndex == m_S0_Ch8_B0_pktStateID)
                        m_S0_Ch8_B0 = getLaneDataByte(3, stateData);
                    else if (pktStateIndex == m_S0_Ch8_B1_pktStateID)
                        m_S0_Ch8_B1 = getLaneDataByte(3, stateData);
                    else if (pktStateIndex == m_S0_Ch8_B2_pktStateID)
                        m_S0_Ch8_B2 = getLaneDataByte(3, stateData);
                    else if (pktStateIndex == m_S0_Ch8_B3_pktStateID)
                        m_S0_Ch8_B3 = getLaneDataByte(3, stateData);

                    break;

                default:
                    break;
            }
        }


        /// <summary>
        /// Append the appropriate data bytes to the audio byte collection
        /// </summary>
        /// <param name="linkWidth"></param>
        /// <param name="audio"></param>
        private void updateAudioData(int linkWidth, int bitsPerSample, List<byte> audio)
        {
            if (channel1_Checkbox.Checked)
            {
                if (bitsPerSample == 16)
                {
                    audio.Add(m_S0_Ch1_B1);
                    audio.Add(m_S0_Ch1_B2);
                }
                else if (bitsPerSample == 24)
                {
                    audio.Add(m_S0_Ch1_B0);
                    audio.Add(m_S0_Ch1_B1);
                    audio.Add(m_S0_Ch1_B2);
                }
            }


            if (channel2_Checkbox.Checked)
            {
                if (bitsPerSample == 16)
                {
                    audio.Add(m_S0_Ch2_B1);
                    audio.Add(m_S0_Ch2_B2);
                }
                else if (bitsPerSample == 24)
                {
                    audio.Add(m_S0_Ch2_B0);
                    audio.Add(m_S0_Ch2_B1);
                    audio.Add(m_S0_Ch2_B2);
                }
            }


            if (channel3_Checkbox.Checked)
            {
                if (bitsPerSample == 16)
                {
                    audio.Add(m_S0_Ch3_B2);
                    audio.Add(m_S0_Ch3_B1);
                }
                else if (bitsPerSample == 24)
                {
                    audio.Add(m_S0_Ch3_B0);
                    audio.Add(m_S0_Ch3_B1);
                    audio.Add(m_S0_Ch3_B2);
                }
            }


            if (channel4_Checkbox.Checked)
            {
                if (bitsPerSample == 16)
                {
                    audio.Add(m_S0_Ch4_B2);
                    audio.Add(m_S0_Ch4_B1);
                }
                else if (bitsPerSample == 24)
                {
                    audio.Add(m_S0_Ch4_B0);
                    audio.Add(m_S0_Ch4_B1);
                    audio.Add(m_S0_Ch4_B2);
                }
            }


            if (channel5_Checkbox.Checked)
            {
                if (bitsPerSample == 16)
                {
                    audio.Add(m_S0_Ch5_B2);
                    audio.Add(m_S0_Ch5_B1);
                }
                else if (bitsPerSample == 24)
                {
                    audio.Add(m_S0_Ch5_B0);
                    audio.Add(m_S0_Ch5_B1);
                    audio.Add(m_S0_Ch5_B2);
                }
            }


            if (channel6_Checkbox.Checked)
            {
                if (bitsPerSample == 16)
                {
                    audio.Add(m_S0_Ch6_B2);
                    audio.Add(m_S0_Ch6_B1);
                }
                else if (bitsPerSample == 24)
                {
                    audio.Add(m_S0_Ch6_B0);
                    audio.Add(m_S0_Ch6_B1);
                    audio.Add(m_S0_Ch6_B2);
                }
            }


            if (channel7_Checkbox.Checked)
            {
                if (bitsPerSample == 16)
                {
                    audio.Add(m_S0_Ch7_B2);
                    audio.Add(m_S0_Ch7_B1);
                }
                else if (bitsPerSample == 24)
                {
                    audio.Add(m_S0_Ch7_B0);
                    audio.Add(m_S0_Ch7_B1);
                    audio.Add(m_S0_Ch7_B2);
                }
            }


            if (channel8_Checkbox.Checked)
            {
                if (bitsPerSample == 16)
                {
                    audio.Add(m_S0_Ch8_B2);
                    audio.Add(m_S0_Ch8_B1);
                }
                else if (bitsPerSample == 24)
                {
                    audio.Add(m_S0_Ch8_B0);
                    audio.Add(m_S0_Ch8_B1);
                    audio.Add(m_S0_Ch8_B2);
                }
            }
        }


        /// <summary>
        /// Get the 16 bytes of data assoicated with the specified state
        /// </summary>
        /// <param name="index"></param>
        /// <param name="statesChunk"></param>
        /// <returns></returns>
        private byte[] getStateDataFromChunk(int index, List<byte> statesChunk, ref byte[] stateData)
        {
            Array.Clear(stateData, 0, stateData.Length); // byte[] stateData = new byte[16];

            for (int i = 0; i < stateData.Length; i++)
                stateData[i] = statesChunk[index + i];

            return stateData;
        }

        /// <summary>
        /// TPI Task function to extract the lower two bytes of audio data for each Audio SDP pkt.
        /// </summary>
        /// <param name="linkWidth"></param>
        /// <param name="vchannel"></param>
        /// <param name="numOfStates"></param>
        /// <param name="startState"></param>
        /// <param name="endState"></param>
        /// <returns></returns>
        private List<byte> getAudioData(int linkWidth, int vchannel, long numOfStates, long startState, long endState, int bitsPerSample, CancellationToken token)
        {
            bool status = true;
            int stateDataLength = 16;  // Assumes that all Trace Buffer data sets have a 16 byte state length.
            byte[] stateData = new byte[stateDataLength];
            int skip_bytes = getbytestoskip(linkWidth);
            int pktStateIndex = 0;
            List<byte> audio = new List<byte>();
            int chunkSize = 4096;

            // check if "Task {0} was cancelled before it got started.",
            if (token.IsCancellationRequested == true)
            {
                token.ThrowIfCancellationRequested();
            }

            this.Invoke(new Action(() =>
            {
                this.audioRendererStatusStripLabel.Text = "Gathering Audio Data...";
                this.statusStrip1.Update();
            }));


            if ((startState < 0) || (endState < 0) || (endState < startState))
                status = false;


            if (status == true)
            {
                long numOfProcessedStates = endState - startState;
                setLaneDataLocations(linkWidth);

                List<byte> statesChunk = null; // m_IProbe.GetStateDataChunk((int)startState, chunkSize);

                // loop through the requested states; one at a time...
                for (int stateIndex = (int)startState; stateIndex < endState; stateIndex += chunkSize)
                {
                    statesChunk = m_IProbe.GetStateDataChunk((int)stateIndex, 4096, vchannel);  // << always request the same amount of states... may not always get the request number of states...
                    chunkSize = statesChunk.Count / stateDataLength;

                    if (chunkSize > 0)
                    {
                        // process the states contained in the current chunk of data
                        for (int chunkIndex = 0; (chunkIndex < chunkSize) && (stateIndex + chunkIndex) < endState; chunkIndex += 1)
                        {
                            getStateDataFromChunk(chunkIndex * 16, statesChunk, ref stateData);
                            if (stateData.Length == 16)  // each state should be sixteen bytes in length
                            {
                                byte eventCode = GetEventCodeII(stateData);
                                if (eventCode == Horizontal_AudioPkt || eventCode == Vertical_AudioPkt)  // 0x20 or 0x60  respectively
                                {
                                    ushort lane0Value = getLaneData(0, stateData);  // Invalid Bit | CtrlChar Bit | Data Byte
                                    if (lane0Value == SS_CTRL_CHAR)  // 0x15c
                                    {
                                        pktStateIndex = 0;
                                        resetChannelDataBytes();  // set everything to zero
                                    }
                                    else if (lane0Value == SE_CTRL_CHAR)  // 0x1FD
                                    {
                                        // write the audio data bytes to the list...
                                        updateAudioData(linkWidth, bitsPerSample, audio);
                                    }
                                    else
                                    {
                                        pktStateIndex += 1;
                                        updateChannelDataValues(linkWidth, pktStateIndex, stateData);
                                    }
                                }
                            }
                            else  // state did not have 16 bytes... shouldn't happen
                            {
                                status = false;
                                break;
                            }

                            Array.Clear(stateData, 0, stateData.Length);
                        }
                    }

                    statesChunk.Clear();    // clear the chunk of states... 

                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                    else
                    {
                        this.Invoke(new Action(() =>
                        {
                            //this.audioRendererStatusStripLabel.Text = stateIndex.ToString();
                            this.audioRendererStatusStripProgressBar.Value = (int)(((float)(stateIndex - startState) / (float)numOfProcessedStates) * 100);
                            this.statusStrip1.Update();
                        }));
                    }
                }
            }


            this.Invoke(new Action(() =>
            {
                this.audioRendererStatusStripLabel.Text = "Done";
                this.audioRendererStatusStripProgressBar.Value = 0;
                this.statusStrip1.Update();
            }));

            return audio;
        }


        /// <summary>
        /// Asyncrohnous task used to assemble the audio data file.
        /// </summary>
        /// <param name="linkWidth"></param>
        private async void generateAudioFile(int linkWidth)
        {
            int vchannel = getVChannelID();
            long numOfStates = Convert.ToInt64(NumOfStates_TextBox.Text);
            long startState = (int)StartState_NumericUpDown.Value;
            long endState = (int)EndState_NumericUpDown.Value;
            int bitsPerSample = int.Parse(BitsPerSample_ComboBox.Text);

            // create the cancellatin token
            m_tokenSource = new CancellationTokenSource();
            m_token = m_tokenSource.Token;


            //Task<byte[]>[] tasks = new Task<byte[]>[1];
            Task<List<byte>>[] tasks = new Task<List<byte>>[1];
            ThreadLocal<GetAudioDataStateObject> tls = new ThreadLocal<GetAudioDataStateObject>();

            //tasks[0] = new Task<byte[]>((stateObject) => {
            tasks[0] = new Task<List<byte>>((stateObject) => {
                tls.Value = (GetAudioDataStateObject)stateObject;
                ((GetAudioDataStateObject)tls.Value).Token.ThrowIfCancellationRequested();

                return getAudioData(tls.Value.linkWidth, tls.Value.vchannel, tls.Value.numOfStates, tls.Value.startState, tls.Value.endState, tls.Value.bitsPerSample, tls.Value.Token);
            }, new GetAudioDataStateObject(linkWidth, vchannel, numOfStates, startState, endState, bitsPerSample, m_token));


            // start the task to get the trigger index
            foreach (Task t in tasks)
                t.Start();

            // wait for the task to finish;  await keeps the GUI thread active while we are waiting...
            //if (m_remoteAPIMode == false)
            await Task.WhenAll(tasks);
            //else
            //    Task.WaitAll(tasks);


            // store the located trigger index for subsequent processing steps.
            List<byte> audio = tasks[0].Result; //byte[] audio = tasks[0].Result;  //m_segregatedTriggerStateIndex = tasks[0].Result;

            //// get rid of the task 
            //foreach (Task T in tasks)
            //{
            //    T.Dispose();
            //}


            //
            // Tell whoever cares that we are done post processing the uploaded data
            //
            if (!m_token.IsCancellationRequested)
            {
                //audioRendererStatusStripLabel.Text = "Creating Wave File";
                this.Invoke(new Action(() =>
                {
                    this.audioRendererStatusStripLabel.Text = "Creating Wave File";
                    this.statusStrip1.Update();
                }));
                createWaveFile(audio);
            }


            this.Invoke(new Action(() =>
            {
                this.audioRendererStatusStripLabel.Text = "Ready";
                this.statusStrip1.Update();
            }));

            setAudioRenderedEnabledProperty(true);
        }



        /// <summary>
        /// Looping through all the states looking for SDP start
        /// </summary>
        /// <param name="lanes"></param>
        private void getAudio(int linkWidth)
        {
            //
            //NOTE: https://stackoverflow.com/questions/29417166/out-of-memory-exception-when-working-with-large-images 
            //      shows a solution to an out-of-memory issue... build in "Any CPU" mode and unclick the "Prefer 32-bit" 
            //      configuration build option (which I couldn't located, but the DLL projects has the Build Property checkbox
            //      disabled and unchecked... I believe we are good to go!
            //      PNS
            //
            setAudioRenderedEnabledProperty(false);
            generateAudioFile(linkWidth);  // generate the audio file on a seperate task.  
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
        private void createByteArray(int linkWidth, int lane, ref int state, ref List<byte> bytearray, int vchannel, ref int bytes, List<bool> channels, long endState)
        {
            byte HAS_SDP = 0x20;  //string HAS_SDP = "0x20";
            byte VAS_SDP = 0x60;  //string VAS_SDP = "0x60";
            byte[] stateBytes = null;
            int skip_bytes = getbytestoskip(linkWidth); // lanes);


            skipbytes(skip_bytes, ref state, vchannel, HAS_SDP, VAS_SDP, endState);

            int i = 0;
            int tracker = 0;


            int incomingStateNumber = state;  // PNS change

            //
            // loop through all eight channels..
            //
            while (i != channels.Count())
            {
                if (channels[i] == true)
                {
                    //
                    // extracts the audio data for a selected channel
                    //

                    state = incomingStateNumber;
                    tracker = 0;
                    int statetracker = 0;
                    lookinchannel(ref state, ref tracker, ref statetracker, ref bytearray, ref bytes, lane + i, vchannel, HAS_SDP, VAS_SDP, endState);
                }

                i++;
            }


            //
            // locate the send of the SDP packet.
            // 
            while (state < endState)   // search for the next SE control character.
            {
                stateBytes = (m_IProbe.GetStateData(vchannel, state));
                byte eventCode = GetEventCodeII(stateBytes);
                if (eventCode == HAS_SDP || eventCode == VAS_SDP)
                {
                    List<byte> lanedata = GetLaneData(stateBytes);
                    bool flag = getLaneBit(stateBytes);
                    byte b = lanedata[0];
                    if (((flag == true) && (b == 253)) || (state >= endState))  // 253 == 0xFD, which is SE control character 
                    {
                        break;  // exits the while loop... 
                    }
                    else
                    {
                        state++;
                        Array.Clear(stateBytes, 0, stateBytes.Length);
                    }
                }
            }
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
       // private void skipbytes(int bytes, ref int state, int vchannel, string HAS_SDP, string VAS_SDP)
        private void skipbytes(int bytes, ref int state, int vchannel, byte HAS_SDP, byte VAS_SDP, long endstate)
        {
            int i = 0;
            while (i != bytes)
            {
                byte[] dum = (m_IProbe.GetStateData(vchannel, state));
                byte eventCode = GetEventCodeII(dum); // StringBuilder sb = GetEventCode(dum);
                if (eventCode == HAS_SDP || eventCode == VAS_SDP)   //if (sb.ToString() == HAS_SDP || sb.ToString() == VAS_SDP)
                    i++;

                if (state < endstate)
                    state++;
                else
                    break;
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
        private void lookinchannel(ref int state, ref int tracker, ref int statetracker, ref List<byte> bytearray, ref int bytes, int lane, int vchannel, byte HAS_SDP, byte VAS_SDP, long endState)
        {
            tracker = 0;
            statetracker = 0;
            byte[] dum = null;
            while ((tracker != 5) && (state < endState))
            {
                dum = (m_IProbe.GetStateData(vchannel, state));
                byte eventCode = GetEventCodeII(dum); //StringBuilder sb = GetEventCode(dum);
                if (eventCode == HAS_SDP || eventCode == VAS_SDP)  //if (sb.ToString() == HAS_SDP || sb.ToString() == VAS_SDP)
                {
                    tracker++;
                    if (tracker == 2 || tracker == 3)
                    {
                        List<byte> lanedata = GetLaneData(dum);
                        bytearray.Add(lanedata[lane]);
                    }
                }
                state++;
                statetracker++;

                //if (state > endState)
                //    break; // exit the while loop
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
        private void skipchannel(ref int state, ref int tracker, ref int statetracker, byte[] dum, int vchannel, string HAS_SDP, string VAS_SDP, long endState)
        {
            tracker = 0;
            statetracker = 0;
            while (tracker != 5)
            {
                dum = (m_IProbe.GetStateData(vchannel, state));
                StringBuilder sb = GetEventCode(dum);
                if (sb.ToString() == HAS_SDP || sb.ToString() == VAS_SDP)
                {
                    tracker++;
                }
                state++;
                statetracker++;

                if (state > endState)
                    break;
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
                //this.progressBar1.Increment(statetracker);
            }
            else if (lanes == 2)
            {
                if (lane == 1)
                {
                    lane = 0;
                    //this.progressBar1.Increment(statetracker);
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
                    //this.progressBar1.Increment(statetracker);
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
        /// https://msdn.microsoft.com/en-us/library/he2s3bh7(v=vs.110).aspx  -- good example of linked list usage
        private void createWaveFile(List<byte> audio)
        {
            //http://soundfile.sapp.org/doc/WaveFormat/
            int channels = getnumchannels();
            int samplebits = Convert.ToInt32(BitsPerSample_ComboBox.Text); //Convert.ToInt32(bitsPerSample_TextBox.Text);
            int audio_size = audio.Count();
            short block = Convert.ToInt16(channels * (samplebits / 8));
            int sample_rate = Convert.ToInt32(SampleRate_ComboBox.Text);  //Convert.ToInt32(sampleRate_Textbox.Text);

            // insert chunkID
            audio.Insert(0, 0x52); // letter 'R'
            audio.Insert(1, 0x49); // letter 'I'
            audio.Insert(2, 0x46); //letter 'F'
            audio.Insert(3, 0x46); // letter 'F'

            // insert chunk Size
            byte[] chucksize = BitConverter.GetBytes(audio_size + 36);//new byte[4] { 0xD0, 0x13, 0x1D, 0x00 }; //36 + SubChunk2Size or more precisely 4 + (8 + subchunk1size) + (8 + subchunk2size)
            audio.Insert(4, chucksize[0]);
            audio.Insert(5, chucksize[1]);
            audio.Insert(6, chucksize[2]);
            audio.Insert(7, chucksize[3]);

            // insert format ID
            audio.Insert(8, 0x57); // letter 'W'
            audio.Insert(9, 0x41); // letter 'A'
            audio.Insert(10, 0x56); // letter 'V'
            audio.Insert(11, 0x45); // letter 'E'

            // insert subchuck ID
            audio.Insert(12, 0x66); // letter 'f'
            audio.Insert(13, 0x6d); // letter 'm'
            audio.Insert(14, 0x74); // letter 't'
            audio.Insert(15, 0x20); // letter ' '

            // insert subchunk1Size  -- 16 for PCM
            audio.Insert(16, 0x10);
            audio.Insert(17, 0x00);
            audio.Insert(18, 0x00);
            audio.Insert(19, 0x00);

            // insert audio format  PCM == 1, other than 1 indicate some form of compression
            audio.Insert(20, 0x01);
            audio.Insert(21, 0x00);

            // insert # of channels, Mono = 1 Stereo = 2
            audio.Insert(22, (byte)(channels & 0x00FF));
            audio.Insert(23, (byte)((channels & 0xFF00) >> 8));
            //audio.Insert(22, 0x02);
            //audio.Insert(23, 0x00);

            // insert sample rate
            byte[] samplerate = BitConverter.GetBytes(sample_rate);
            audio.Insert(24, samplerate[0]);
            audio.Insert(25, samplerate[1]);
            audio.Insert(26, samplerate[2]);
            audio.Insert(27, samplerate[3]);

            // insert byte rate
            byte[] byterate = BitConverter.GetBytes(sample_rate * block);//new byte[4] { 0x00, 0xEE, 0x02, 0x00 }; // samplerate * blockalign
            audio.Insert(28, byterate[0]);
            audio.Insert(29, byterate[1]);
            audio.Insert(30, byterate[2]);
            audio.Insert(31, byterate[3]);


            // insert block align
            byte[] blockalign = BitConverter.GetBytes(block);//new byte[2] { 0x04, 0x00 }; // numchannels * bits_per_sample/8
            audio.Insert(32, blockalign[0]);
            audio.Insert(33, blockalign[1]);

            //bits per sample
            //byte[] bits_per_sample = new byte[2] { 0x10, 0x00 }; // 8 bits = 8, 16 bits = 16, etc
            audio.Insert(34, 0x10);
            audio.Insert(35, 0x00);

            // insert "data"
            //byte[] subchunk2ID = new byte[4] { 0x64, 0x61, 0x74, 0x61 }; // contain letters "data"
            audio.Insert(36, 0x64);
            audio.Insert(37, 0x61);
            audio.Insert(38, 0x74);
            audio.Insert(39, 0x61);

            // insert numSample * numChannels * bitsPerSample
            byte[] subchunk2size = BitConverter.GetBytes(audio_size);
            string hexvalue = audio_size.ToString("X");
            byte fourthbyte = (byte)((audio_size >> 24) & 0xff);
            byte thirdbyte = (byte)((audio_size >> 16) & 0xff);
            byte secondbyte = (byte)((audio_size >> 8) & 0xff);
            byte firstbyte = (byte)((audio_size) & 0xff);
            audio.Insert(40, firstbyte);
            audio.Insert(41, secondbyte);
            audio.Insert(42, thirdbyte);
            audio.Insert(43, fourthbyte);

            if (audio_size == 0)
            {
                string error = "No Audio Found";
                ErrorForm form = new ErrorForm(error);
                form.Show();
            }
            else
            {
                //audio = newarray;
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "WAV files(*.wav) | *.wav";
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //System.IO.File.WriteAllBytes(saveFileDialog.FileName, audio.ToArray());
                    using (FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.ReadWrite))
                    {
                        fileStream.Write(audio.ToArray(), 0, audio.Count);
                        fileStream.Close();
                    }
                }
            }
        }


        /// <summary>
        ///  update the chart
        /// </summary>
        /// <param name="MaudNaudStats"></param>
        private void displayMaudNaudStats(MaudNaudStatistics MaudNaudStats)
        {
            long MaudAvg = MaudNaudStats.MaudSum / MaudNaudStats.NumOfTimeStampSDPs;
            long NaudAvg = MaudNaudStats.NaudSum / MaudNaudStats.NumOfTimeStampSDPs;

            // update the min/max/mean Maud Values
            MaudMax_TextBox.Text = "0x" + MaudNaudStats.MaudMax.ToString("X6");
            MaudMin_TextBox.Text = "0x" + MaudNaudStats.MaudMin.ToString("X6");
            MaudAverage_TextBox.Text = "0x" + MaudAvg.ToString("X6");


            // update the min/max/mean Naud Values
            NaudMax_TextBox.Text = "0x" + MaudNaudStats.NaudMax.ToString("X6");
            NaudMin_TextBox.Text = "0x" + MaudNaudStats.NaudMin.ToString("X6");
            NaudAverage_TextBox.Text = "0x" + NaudAvg.ToString("X6");


            // Avg Maud/Naud Stat
            AvgMOverN_TextBox.Text = ((float)MaudAvg / (float)NaudAvg).ToString();

            // update the SDP counts
            AudioSDP_TextBox.Text = MaudNaudStats.NumOfTimeStampSDPs.ToString();
            AudioVerticalSDP_TextBox.Text = MaudNaudStats.NumOfVerticalTimeStampSDPs.ToString();
        }



        /// <summary>
        /// Update the chart and counts.
        /// </summary>
        /// <param name="MaudNaudStats"></param>
        private void displayMaudNaudChart(MaudNaudStatistics MaudNaudStats)
        {
            // update the min/max and other counts
            if (MaudNaudStats.MaudAverage == 0)
            {
                ErrorForm form = new ErrorForm("No Maud or Naud values found. Check Probe Manager for any Timestamp packets.");
                form.Show();
            }
            displayMaudNaudStats(MaudNaudStats);

            //// update the chart
            //this.MaudNaud_Chart.Series["Value"].Points.AddXY("Maud_Max", MaudNaudStats.MaudMax); // getmax(mauddata));
            //this.MaudNaud_Chart.Series["Value"].Points.AddXY("Maud_Min", MaudNaudStats.MaudMin); //  getmin(mauddata));
            //this.MaudNaud_Chart.Series["Value"].Points.AddXY("Maud_Mean", MaudNaudStats.MaudAverage); //  getmean(mauddata));

            //this.MaudNaud_Chart.Series["Value"].Points.AddXY("Naud_Max", MaudNaudStats.NaudMax); //  getmax(nauddata));
            //this.MaudNaud_Chart.Series["Value"].Points.AddXY("Naud_Min", MaudNaudStats.NaudMin); //  getmin(nauddata));
            //this.MaudNaud_Chart.Series["Value"].Points.AddXY("Naud_Mean", MaudNaudStats.NaudAverage); //  getmean(nauddata));
        }


        /// <summary>
        /// Assemble the Maud bytes into an interger format
        /// </summary>
        /// <param name="useMajorityRuling"></param>
        /// <returns></returns>
        private uint getMaudValue(bool useMajorityRuling)
        {
            uint pValue = 0x00;

            if (useMajorityRuling == false)
            {
                pValue = (uint)((m_Maud_1_B0 << 16) | (m_Maud_1_B1 << 8) | m_Maud_1_B2);
            }
            else
            {
                // TBD...
            }

            return pValue;
        }


        /// <summary>
        /// Assemble the Naud bytes into an interger format
        /// </summary>
        /// <param name="useMajorityRuling"></param>
        /// <returns></returns>
        private uint getNaudValue(bool useMajorityRuling)
        {
            uint pValue = 0x00;

            if (useMajorityRuling == false)
            {
                pValue = (uint)((m_Naud_1_B0 << 16) | (m_Naud_1_B1 << 8) | m_Naud_1_B2);
            }
            else
            {
                // TBD...
            }

            return pValue;
        }



        /// <summary>
        /// Update the Maud/Naud Min/Max statistics.
        /// </summary>
        /// <param name="MaudValue"></param>
        /// <param name="NaudValue"></param>
        /// <param name="stats"></param>
        private void setMaudNaudMinMaxStats(uint MaudValue, uint NaudValue, MaudNaudStatistics stats)
        {
            // update Maud Min/Max statistic
            if (stats.MaudMin < MaudValue)
                stats.MaudMin = MaudValue;

            if (MaudValue > stats.MaudMax)
                stats.MaudMax = MaudValue;





            // update Naud Min/Max statistic
            if (stats.NaudMin < NaudValue)
                stats.NaudMin = NaudValue;

            if (NaudValue > stats.NaudMax)
                stats.NaudMax = NaudValue;
        }


        /// <summary>
        /// Keep a running summation of all Maud and Naud values.
        /// </summary>
        /// <param name="MaudValue"></param>
        /// <param name="NaudValue"></param>
        /// <param name="stats"></param>
        private void setMaudNaudSumStats(uint MaudValue, uint NaudValue, MaudNaudStatistics stats)
        {
            stats.MaudSum = stats.MaudSum + MaudValue;
            stats.NaudSum = stats.NaudSum + NaudValue;
        }


        /// <summary>
        /// update the min/max/mean statistics.
        /// </summary>
        /// <param name="linkWidth"></param>
        /// <param name="stats"></param>
        private void updateMaudNaudStats(byte eventCode, int linkWidth, MaudNaudStatistics stats)
        {
            uint MaudValue = getMaudValue(false);
            uint NaudValue = getNaudValue(false);

            // for now... just take the Maud/Naud from lane 0.... will do majority voting in a bit...
            setMaudNaudMinMaxStats(MaudValue, NaudValue, stats);
            setMaudNaudSumStats(MaudValue, NaudValue, stats);

            // update the Time Stamp SDP counts
            stats.NumOfTimeStampSDPs = stats.NumOfTimeStampSDPs + 1;
            if (eventCode == Vertical_AudioTSPkt)
            {
                stats.NumOfVerticalTimeStampSDPs = stats.NumOfVerticalTimeStampSDPs + 1;
            }
        }


        /// <summary>
        /// set the variables that idenify the pkt state index of the two data bytes of interest.
        /// </summary>
        /// <param name="linkWidth"></param>
        private void setMaudNauDataByteLocations(int linkWidth)
        {
            switch (linkWidth)
            {
                case 1:
                    m_Maud_1_B0_pktStateID = 9;
                    m_Maud_1_B1_pktStateID = 10;
                    m_Maud_1_B2_pktStateID = 11;

                    m_Maud_2_B0_pktStateID = 14;
                    m_Maud_2_B1_pktStateID = 15;
                    m_Maud_2_B2_pktStateID = 16;

                    m_Maud_3_B0_pktStateID = 19;
                    m_Maud_3_B1_pktStateID = 20;
                    m_Maud_3_B2_pktStateID = 21;

                    m_Maud_4_B0_pktStateID = 24;
                    m_Maud_4_B1_pktStateID = 25;
                    m_Maud_4_B2_pktStateID = 26;


                    m_Naud_1_B0_pktStateID = 29;
                    m_Naud_1_B1_pktStateID = 30;
                    m_Naud_1_B2_pktStateID = 31;

                    m_Naud_2_B0_pktStateID = 34;
                    m_Naud_2_B1_pktStateID = 35;
                    m_Naud_2_B2_pktStateID = 36;

                    m_Naud_3_B0_pktStateID = 39;
                    m_Naud_3_B1_pktStateID = 40;
                    m_Naud_3_B2_pktStateID = 41;

                    m_Naud_4_B0_pktStateID = 44;
                    m_Naud_4_B1_pktStateID = 45;
                    m_Naud_4_B2_pktStateID = 46;
                    break;

                case 2:
                    m_Maud_1_B0_pktStateID = 5;
                    m_Maud_1_B1_pktStateID = 6;
                    m_Maud_1_B2_pktStateID = 7;

                    m_Maud_2_B0_pktStateID = 5;
                    m_Maud_2_B1_pktStateID = 6;
                    m_Maud_2_B2_pktStateID = 7;

                    m_Maud_3_B0_pktStateID = 10;
                    m_Maud_3_B1_pktStateID = 11;
                    m_Maud_3_B2_pktStateID = 12;

                    m_Maud_4_B0_pktStateID = 10;
                    m_Maud_4_B1_pktStateID = 11;
                    m_Maud_4_B2_pktStateID = 12;


                    m_Naud_1_B0_pktStateID = 15;
                    m_Naud_1_B1_pktStateID = 16;
                    m_Naud_1_B2_pktStateID = 17;

                    m_Naud_2_B0_pktStateID = 15;
                    m_Naud_2_B1_pktStateID = 16;
                    m_Naud_2_B2_pktStateID = 17;

                    m_Naud_3_B0_pktStateID = 20;
                    m_Naud_3_B1_pktStateID = 21;
                    m_Naud_3_B2_pktStateID = 22;

                    m_Naud_4_B0_pktStateID = 20;
                    m_Naud_4_B1_pktStateID = 21;
                    m_Naud_4_B2_pktStateID = 22;
                    break;

                case 4:
                    m_Maud_1_B0_pktStateID = 3;
                    m_Maud_1_B1_pktStateID = 4;
                    m_Maud_1_B2_pktStateID = 5;

                    m_Maud_2_B0_pktStateID = 3;
                    m_Maud_2_B1_pktStateID = 4;
                    m_Maud_2_B2_pktStateID = 5;

                    m_Maud_3_B0_pktStateID = 3;
                    m_Maud_3_B1_pktStateID = 4;
                    m_Maud_3_B2_pktStateID = 5;

                    m_Maud_4_B0_pktStateID = 3;
                    m_Maud_4_B1_pktStateID = 4;
                    m_Maud_4_B2_pktStateID = 5;


                    m_Naud_1_B0_pktStateID = 8;
                    m_Naud_1_B1_pktStateID = 9;
                    m_Naud_1_B2_pktStateID = 10;

                    m_Naud_2_B0_pktStateID = 8;
                    m_Naud_2_B1_pktStateID = 9;
                    m_Naud_2_B2_pktStateID = 10;

                    m_Naud_3_B0_pktStateID = 8;
                    m_Naud_3_B1_pktStateID = 9;
                    m_Naud_3_B2_pktStateID = 10;

                    m_Naud_4_B0_pktStateID = 8;
                    m_Naud_4_B1_pktStateID = 9;
                    m_Naud_4_B2_pktStateID = 10;
                    break;

                default:
                    m_Maud_1_B2_pktStateID = 9;
                    m_Maud_1_B1_pktStateID = 10;
                    m_Maud_1_B0_pktStateID = 11;

                    m_Maud_2_B2_pktStateID = 14;
                    m_Maud_2_B1_pktStateID = 15;
                    m_Maud_2_B0_pktStateID = 16;

                    m_Maud_3_B2_pktStateID = 19;
                    m_Maud_3_B1_pktStateID = 20;
                    m_Maud_3_B0_pktStateID = 21;

                    m_Maud_4_B2_pktStateID = 24;
                    m_Maud_4_B1_pktStateID = 25;
                    m_Maud_4_B0_pktStateID = 26;


                    m_Naud_1_B2_pktStateID = 29;
                    m_Naud_1_B1_pktStateID = 30;
                    m_Naud_1_B0_pktStateID = 31;

                    m_Naud_2_B2_pktStateID = 34;
                    m_Naud_2_B1_pktStateID = 35;
                    m_Naud_2_B0_pktStateID = 36;

                    m_Naud_3_B2_pktStateID = 39;
                    m_Naud_3_B1_pktStateID = 40;
                    m_Naud_3_B0_pktStateID = 41;

                    m_Naud_4_B2_pktStateID = 44;
                    m_Naud_4_B1_pktStateID = 45;
                    m_Naud_4_B0_pktStateID = 46;
                    break;
            }
        }

        /// <summary>
        /// Reset the maud/naud data
        /// </summary>
        private void resetMaudNaudDataBytes()
        {
            m_Maud_1_B2 = 0;
            m_Maud_1_B1 = 0;
            m_Maud_1_B0 = 0;

            m_Maud_2_B2 = 0;
            m_Maud_2_B1 = 0;
            m_Maud_2_B0 = 0;

            m_Maud_3_B2 = 0;
            m_Maud_3_B1 = 0;
            m_Maud_3_B0 = 0;

            m_Maud_4_B2 = 0;
            m_Maud_4_B1 = 0;
            m_Maud_4_B0 = 0;


            m_Naud_1_B2 = 0;
            m_Naud_1_B1 = 0;
            m_Naud_1_B0 = 0;

            m_Naud_2_B2 = 0;
            m_Naud_2_B1 = 0;
            m_Naud_2_B0 = 0;

            m_Naud_3_B2 = 0;
            m_Naud_3_B1 = 0;
            m_Naud_3_B0 = 0;

            m_Naud_4_B2 = 0;
            m_Naud_4_B1 = 0;
            m_Naud_4_B0 = 0;
        }

        /// <summary>
        /// Get the Maud/Naud member values with the data contained in the current state
        /// </summary>
        /// <param name="linkWidth"></param>
        /// <param name="stats"></param>
        private void updateManuNaudDataValues(int linkWidth, int pktStateIndex, byte[] stateData)
        {
            switch (linkWidth)
            {
                case 1:
                    if (pktStateIndex == m_Maud_1_B0_pktStateID)
                        m_Maud_1_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Maud_1_B1_pktStateID)
                        m_Maud_1_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Maud_1_B2_pktStateID)
                        m_Maud_1_B2 = getLaneDataByte(0, stateData);

                    else if (pktStateIndex == m_Maud_2_B0_pktStateID)
                        m_Maud_2_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Maud_2_B1_pktStateID)
                        m_Maud_2_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Maud_2_B2_pktStateID)
                        m_Maud_2_B2 = getLaneDataByte(0, stateData);

                    else if (pktStateIndex == m_Maud_3_B0_pktStateID)
                        m_Maud_3_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Maud_3_B1_pktStateID)
                        m_Maud_2_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Maud_3_B2_pktStateID)
                        m_Maud_3_B2 = getLaneDataByte(0, stateData);

                    else if (pktStateIndex == m_Maud_4_B0_pktStateID)
                        m_Maud_4_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Maud_4_B1_pktStateID)
                        m_Maud_4_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Maud_4_B2_pktStateID)
                        m_Maud_4_B2 = getLaneDataByte(0, stateData);


                    else if (pktStateIndex == m_Naud_1_B0_pktStateID)
                        m_Naud_1_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Naud_1_B1_pktStateID)
                        m_Naud_1_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Naud_1_B2_pktStateID)
                        m_Naud_1_B2 = getLaneDataByte(0, stateData);

                    else if (pktStateIndex == m_Naud_2_B0_pktStateID)
                        m_Naud_2_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Naud_2_B1_pktStateID)
                        m_Naud_2_B2 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Naud_2_B2_pktStateID)
                        m_Naud_2_B2 = getLaneDataByte(0, stateData);

                    else if (pktStateIndex == m_Naud_3_B0_pktStateID)
                        m_Naud_3_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Naud_3_B1_pktStateID)
                        m_Naud_3_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Naud_3_B2_pktStateID)
                        m_Naud_3_B2 = getLaneDataByte(0, stateData);

                    else if (pktStateIndex == m_Naud_4_B0_pktStateID)
                        m_Naud_4_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Naud_4_B1_pktStateID)
                        m_Naud_4_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Naud_4_B2_pktStateID)
                        m_Naud_4_B2 = getLaneDataByte(0, stateData);
                    break;

                case 2:
                    if (pktStateIndex == m_Maud_1_B0_pktStateID)
                        m_Maud_1_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Maud_1_B1_pktStateID)
                        m_Maud_1_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Maud_1_B2_pktStateID)
                        m_Maud_1_B2 = getLaneDataByte(0, stateData);

                    if (pktStateIndex == m_Maud_2_B0_pktStateID)
                        m_Maud_2_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Maud_2_B1_pktStateID)
                        m_Maud_2_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Maud_2_B2_pktStateID)
                        m_Maud_2_B2 = getLaneDataByte(0, stateData);

                    if (pktStateIndex == m_Maud_3_B0_pktStateID)
                        m_Maud_3_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Maud_3_B1_pktStateID)
                        m_Maud_2_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Maud_3_B2_pktStateID)
                        m_Maud_3_B2 = getLaneDataByte(0, stateData);

                    if (pktStateIndex == m_Maud_4_B0_pktStateID)
                        m_Maud_4_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Maud_4_B1_pktStateID)
                        m_Maud_4_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Maud_4_B2_pktStateID)
                        m_Maud_4_B2 = getLaneDataByte(0, stateData);


                    if (pktStateIndex == m_Naud_1_B0_pktStateID)
                        m_Naud_1_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Naud_1_B1_pktStateID)
                        m_Naud_1_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Naud_1_B2_pktStateID)
                        m_Naud_1_B2 = getLaneDataByte(0, stateData);

                    if (pktStateIndex == m_Naud_2_B0_pktStateID)
                        m_Naud_2_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Naud_2_B1_pktStateID)
                        m_Naud_2_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Naud_2_B2_pktStateID)
                        m_Naud_2_B2 = getLaneDataByte(0, stateData);

                    if (pktStateIndex == m_Naud_3_B0_pktStateID)
                        m_Naud_3_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Naud_3_B1_pktStateID)
                        m_Naud_3_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Naud_3_B2_pktStateID)
                        m_Naud_3_B2 = getLaneDataByte(0, stateData);

                    if (pktStateIndex == m_Naud_4_B0_pktStateID)
                        m_Naud_4_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Naud_4_B1_pktStateID)
                        m_Naud_4_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Naud_4_B2_pktStateID)
                        m_Naud_4_B2 = getLaneDataByte(0, stateData);
                    break;

                case 4:
                    if (pktStateIndex == m_Maud_1_B0_pktStateID)
                        m_Maud_1_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Maud_1_B1_pktStateID)
                        m_Maud_1_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Maud_1_B2_pktStateID)
                        m_Maud_1_B2 = getLaneDataByte(0, stateData);

                    if (pktStateIndex == m_Maud_2_B0_pktStateID)
                        m_Maud_2_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Maud_2_B1_pktStateID)
                        m_Maud_2_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Maud_2_B2_pktStateID)
                        m_Maud_2_B2 = getLaneDataByte(0, stateData);

                    if (pktStateIndex == m_Maud_3_B0_pktStateID)
                        m_Maud_3_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Maud_3_B1_pktStateID)
                        m_Maud_3_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Maud_3_B2_pktStateID)
                        m_Maud_3_B2 = getLaneDataByte(0, stateData);

                    if (pktStateIndex == m_Maud_4_B0_pktStateID)
                        m_Maud_4_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Maud_4_B1_pktStateID)
                        m_Maud_4_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Maud_4_B2_pktStateID)
                        m_Maud_4_B2 = getLaneDataByte(0, stateData);


                    if (pktStateIndex == m_Naud_1_B0_pktStateID)
                        m_Naud_1_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Naud_1_B1_pktStateID)
                        m_Naud_1_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Naud_1_B2_pktStateID)
                        m_Naud_1_B2 = getLaneDataByte(0, stateData);

                    if (pktStateIndex == m_Naud_2_B0_pktStateID)
                        m_Naud_2_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Naud_2_B1_pktStateID)
                        m_Naud_2_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Naud_2_B2_pktStateID)
                        m_Naud_2_B2 = getLaneDataByte(0, stateData);

                    if (pktStateIndex == m_Naud_3_B0_pktStateID)
                        m_Naud_3_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Naud_3_B1_pktStateID)
                        m_Naud_3_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Naud_3_B2_pktStateID)
                        m_Naud_3_B2 = getLaneDataByte(0, stateData);

                    if (pktStateIndex == m_Naud_4_B0_pktStateID)
                        m_Naud_4_B0 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Naud_4_B1_pktStateID)
                        m_Naud_4_B1 = getLaneDataByte(0, stateData);
                    else if (pktStateIndex == m_Naud_4_B2_pktStateID)
                        m_Naud_4_B2 = getLaneDataByte(0, stateData);
                    break;
            }
        }


        /// <summary>
        /// TPI Task function to extract the lower two bytes of audio data for each Audio SDP pkt.
        /// </summary>
        /// <param name="linkWidth"></param>
        /// <param name="vchannel"></param>
        /// <param name="numOfStates"></param>
        /// <param name="startState"></param>
        /// <param name="endState"></param>
        /// <returns></returns>
        private MaudNaudStatistics getMaudNaudData(int linkWidth, int vchannel, long numOfStates, long startState, long endState, CancellationToken token)
        {
            bool status = true;
            int stateDataLength = 16;  // Assumes that all Trace Buffer data sets have a 16 byte state length.
            byte[] stateData = new byte[stateDataLength];
            int skip_bytes = getbytestoskip(linkWidth);
            int pktStateIndex = 0;
            MaudNaudStatistics stats = new MaudNaudStatistics(); // List<byte> audio = new List<byte>();
            int chunkSize = 4096;

            // check if "Task {0} was cancelled before it got started.",
            if (token.IsCancellationRequested == true)
            {
                token.ThrowIfCancellationRequested();
            }

            this.Invoke(new Action(() =>
            {
                this.audioRendererStatusStripLabel.Text = "Gathering Maud/Naud Statistics...";
                this.statusStrip1.Update();
            }));


            if ((startState < 0) || (endState < 0) || (endState < startState))
                status = false;


            if (status == true)
            {
                long numOfProcessedStates = endState - startState;
                setMaudNauDataByteLocations(linkWidth);

                List<byte> statesChunk = null; // m_IProbe.GetStateDataChunk((int)startState, chunkSize);

                // loop through the requested states; one at a time...
                for (int stateIndex = (int)startState; stateIndex < endState; stateIndex += chunkSize)
                {
                    statesChunk = m_IProbe.GetStateDataChunk((int)stateIndex, 4096, vchannel);  // << always request the same amount of states... may not always get the request number of states...
                    chunkSize = statesChunk.Count / stateDataLength;

                    if (chunkSize > 0)
                    {
                        // process the states contained in the current chunk of data
                        for (int chunkIndex = 0; (chunkIndex < chunkSize) && (stateIndex + chunkIndex) < endState; chunkIndex += 1)
                        {
                            getStateDataFromChunk(chunkIndex * 16, statesChunk, ref stateData);
                            if (stateData.Length == 16)  // each state should be sixteen bytes in length
                            {
                                byte eventCode = GetEventCodeII(stateData);
                                if (eventCode == Horizontal_AudioTSPkt || eventCode == Vertical_AudioTSPkt)  // 0x24 or 0x64  respectively
                                {
                                    ushort lane0Value = getLaneData(0, stateData);  // Invalid Bit | CtrlChar Bit | Data Byte
                                    if (lane0Value == SS_CTRL_CHAR)  // 0x15c
                                    {
                                        pktStateIndex = 0;
                                        resetMaudNaudDataBytes();  // set everything to zero
                                    }
                                    else if (lane0Value == SE_CTRL_CHAR)  // 0x1FD
                                    {
                                        // write the audio data bytes to the list...
                                        updateMaudNaudStats(eventCode, linkWidth, stats);
                                    }
                                    else
                                    {
                                        pktStateIndex += 1;
                                        updateManuNaudDataValues(linkWidth, pktStateIndex, stateData);
                                    }
                                }
                            }
                            else  // state did not have 16 bytes... shouldn't happen
                            {
                                status = false;
                                break;
                            }

                            Array.Clear(stateData, 0, stateData.Length);
                        }
                    }

                    statesChunk.Clear();    // clear the chunk of states... 

                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                    else
                    {
                        this.Invoke(new Action(() =>
                        {
                            //this.audioRendererStatusStripLabel.Text = stateIndex.ToString();
                            this.audioRendererStatusStripProgressBar.Value = (int)(((float)(stateIndex - startState) / (float)numOfProcessedStates) * 100);
                            this.statusStrip1.Update();
                        }));
                    }
                }
            }


            this.Invoke(new Action(() =>
            {
                this.audioRendererStatusStripLabel.Text = "Done";
                this.audioRendererStatusStripProgressBar.Value = 0;
                this.statusStrip1.Update();
            }));

            return stats;
        }


        /// <summary>
        /// Assemble the Maud/Naud statistics
        /// </summary>
        /// <param name="linkWidth"></param>
        private async void generateMaudNaudStats(int linkWidth)
        {
            int vchannel = getVChannelID();
            long numOfStates = m_IProbe.GetNumberOfStates(vchannel);
            long startState = (int)StartState_NumericUpDown.Value;
            long endState = (int)EndState_NumericUpDown.Value;
            int bitsPerSample = int.Parse(BitsPerSample_ComboBox.Text);

            // create the cancellatin token
            m_tokenSource = new CancellationTokenSource();
            m_token = m_tokenSource.Token;


            Task<MaudNaudStatistics>[] tasks = new Task<MaudNaudStatistics>[1];
            ThreadLocal<GetMaudNaudDataStateObject> tls = new ThreadLocal<GetMaudNaudDataStateObject>();

            //tasks[0] = new Task<byte[]>((stateObject) => {
            tasks[0] = new Task<MaudNaudStatistics>((stateObject) => {
                tls.Value = (GetMaudNaudDataStateObject)stateObject;
                ((GetMaudNaudDataStateObject)tls.Value).Token.ThrowIfCancellationRequested();

                return getMaudNaudData(tls.Value.linkWidth, tls.Value.vchannel, tls.Value.numOfStates, tls.Value.startState, tls.Value.endState, tls.Value.Token);
            }, new GetMaudNaudDataStateObject(linkWidth, vchannel, numOfStates, startState, endState, m_token));


            // start the task to get the trigger index
            foreach (Task t in tasks)
                t.Start();

            // wait for the task to finish;  await keeps the GUI thread active while we are waiting...
            //if (m_remoteAPIMode == false)
            await Task.WhenAll(tasks);
            //else
            //    Task.WaitAll(tasks);


            // store the located trigger index for subsequent processing steps.
            MaudNaudStatistics MaudNaudStats = tasks[0].Result; //byte[] audio = tasks[0].Result;  //m_segregatedTriggerStateIndex = tasks[0].Result;

            //// get rid of the task 
            //foreach (Task T in tasks)
            //{
            //    T.Dispose();
            //}


            //
            // Tell whoever cares that we are done post processing the uploaded data
            //
            if (!m_token.IsCancellationRequested)
            {
                //audioRendererStatusStripLabel.Text = "Creating Wave File";
                this.Invoke(new Action(() =>
                {
                    this.audioRendererStatusStripLabel.Text = "Calculating Maud/Naud Statistics...";
                    this.statusStrip1.Update();

                    try
                    {
                        displayMaudNaudChart(MaudNaudStats);
                    }
                    catch (Exception ex)
                    {
                        audioRendererStatusStripLabel.Text = "Error calculating M/N statistics...";
                    }
                }));
            }


            this.Invoke(new Action(() =>
            {
                this.audioRendererStatusStripLabel.Text = "Ready";
                this.statusStrip1.Update();
            }));

            setAudioRenderedEnabledProperty(true);
        }


        /// <summary>
        /// Go through state listing looking for state of Vertical Audio TimeStamp and filling out chart.
        /// </summary>
        /// <param name="lanes"></param>
        private void getMaudNaud(int linkWidth)
        {
            setAudioRenderedEnabledProperty(false);
            generateMaudNaudStats(linkWidth);  // generate the audio file on a seperate task.  
        }

        //string V_Audio_TS = "0x64";
        //int vchannel = getVChannelID();
        //int i = 0;
        //byte[] dum = null;
        //long numofstates = Convert.ToInt32(m_IProbe.GetNumberOfStates(getVChannelID()));
        //int states = (int)EndState_NumericUpDown.Value;

        //if (states > numofstates)
        //{
        //    states = Convert.ToInt32(numofstates - 1);
        //}

        //List<uint> mauddata = new List<uint>();
        //List<uint> nauddata = new List<uint>();

        //int lane = 0;

        //while (i <= states)
        //{
        //    dum = m_IProbe.GetStateData(vchannel, i);
        //    StringBuilder sb = GetEventCode(dum);
        //    if (sb.ToString() == V_Audio_TS)
        //    {
        //        extractdata(ref i, V_Audio_TS,ref mauddata, ref nauddata);
        //    }
        //    i++;
        //    //this.progressBar1.Increment(1);
        //}

        //if (mauddata.Any() && nauddata.Any())
        //{
        //    this.MaudNaud_Chart.Series["Value"].Points.AddXY("Maud_Max", getmax(mauddata));
        //    this.MaudNaud_Chart.Series["Value"].Points.AddXY("Maud_Min", getmin(mauddata));
        //    this.MaudNaud_Chart.Series["Value"].Points.AddXY("Maud_Mean", getmean(mauddata));
        //    this.MaudNaud_Chart.Series["Value"].Points.AddXY("Naud_Max", getmax(nauddata));
        //    this.MaudNaud_Chart.Series["Value"].Points.AddXY("Naud_Min", getmin(nauddata));
        //    this.MaudNaud_Chart.Series["Value"].Points.AddXY("Naud_Mean", getmean(nauddata));

        //    maudmaxtext.Text = getmax(mauddata).ToString();
        //    maudmintext.Text = getmin(mauddata).ToString();
        //    maudaveragetext.Text = getmean(mauddata).ToString();
        //    naudmaxtext.Text = getmax(nauddata).ToString();
        //    naudmintext.Text = getmin(nauddata).ToString();
        //    naudaveragetext.Text = getmean(nauddata).ToString();
        //}
        //else
        //{
        //    string e = "No Maud or Naud data, Cant find Vertical Audio Time Stamp";
        //    Runerror error = new Runerror(e);
        //    error.Show();
        //}
        //}
        #endregion //Private Methods

    }
}
