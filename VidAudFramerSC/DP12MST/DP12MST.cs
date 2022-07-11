using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FPSProbeMgr_Gen2;

namespace DP12MSTClassLibrary
{
    public enum DP12MST_TRACE_BUFFER_MODE { MainLink, AuxiliaryLink, Both, Unknown }

    public enum DP12MST_CROSS_LINK_MODE { AuxiliaryLink, MainLink, Unknown }

    /// <summary>
    /// Data extractor for Display Port 1.2 MST
    /// </summary>
    public class DP12MST : IProbeMgrGen2
    {

        #region Members

        private const int TB_STATE_LENTH = 16;

        IProbeMgrGen2 m_IProbe = null;

        private static DP12MST_MessagePump m_eventPump;
        private static DP12MSTTraceBufferDataMgr m_TBDataMgr;
        private DP12MSTMessageGenerator m_DP12MSTMessageGenerator = null;

        private byte[] m_stateData = new byte[TB_STATE_LENTH]; 


        public event LogMsgEvent LogMsgEvent2;
        public event TBUploadEvent TBUploadEvent2;
        public event ProbeCommEvent ProbeCommEvent2;

        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP12MST(string path)
        {
            m_eventPump = new DP12MST_MessagePump();

            m_TBDataMgr = new DP12MSTTraceBufferDataMgr(path);
            m_TBDataMgr.DisplayPort12MSTEvent += new DP12MSTEvent(processDPEvent);
            m_DP12MSTMessageGenerator = DP12MSTMessageGenerator.GetInstance();
        }

        #endregion // Constructor(s)

        #region Event Handlers

        /// <summary>
        /// Forward the event to the event pump object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void processDPEvent(object sender, DP12MSTEventArgs e)
        {
            if (e.Message is DP12MSTMessage_TBObjectRegister)
            {
                DP12MSTMessage_TBObjectRegister regEvent = e.Message as DP12MSTMessage_TBObjectRegister;
                m_eventPump.AddTBObjectRef(regEvent.TBObj);
            }
            else if (e.Message is DP12MSTMessage_StateDataResponse)
            {
                extractStateDataFromResponseMessage(e.Message);
            }
            else
            {
                m_eventPump.ForwardEvent(e.Message); 
            }
        }


        #endregion // Event Handlers

        #region Private Methods

        /// <summary>
        /// Get the state data from the DP Message containing a state data response message
        /// </summary>
        /// <param name="DisplayPortEvent"></param>
        /// <returns></returns>
        private bool extractStateDataFromResponseMessage(DP12MSTMessage DisplayPortEvent)
        {
            bool status = true;

            // initiate the state byte array
            if (m_stateData == null)
            {
                m_stateData = new byte[((DP12MSTMessage_StateDataResponse)DisplayPortEvent).DataBytes.Length];
            }
            else if ((m_stateData.Length == ((DP12MSTMessage_StateDataResponse)DisplayPortEvent).DataBytes.Length))
            {
                Array.ConstrainedCopy(((DP12MSTMessage_StateDataResponse)DisplayPortEvent).DataBytes, 0, m_stateData, 0, ((DP12MSTMessage_StateDataResponse)DisplayPortEvent).DataBytes.Length);
            }
            else
            {
                m_stateData = new byte[((DP12MSTMessage_StateDataResponse)DisplayPortEvent).DataBytes.Length];
                Array.ConstrainedCopy(((DP12MSTMessage_StateDataResponse)DisplayPortEvent).DataBytes, 0, m_stateData, 0, ((DP12MSTMessage_StateDataResponse)DisplayPortEvent).DataBytes.Length);
            }

            return status;
        }
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods

        #region IProbeMgrGen2 interface methods

        /// <summary>
        ///  Associate the IProbeMgrGen2's event -- OnLogMsgEvent -- with LogMsgEvent2
        /// </summary>
        event LogMsgEvent IProbeMgrGen2.OnLogMsgEvent
        {
            add
            {
                LogMsgEvent2 += value;
            }
            remove
            {
                LogMsgEvent2 -= value;
            }
        }


        /// <summary>
        ///  Associate the IProbeMgrGen2's event -- OnTBUploadEvent -- with OnTBUploadEvent2
        /// </summary>
        event TBUploadEvent IProbeMgrGen2.OnTBUploadEvent
        {
            add
            {
                TBUploadEvent2 += value;
            }
            remove
            {
                TBUploadEvent2 -= value;
            }
        }


        /// <summary>
        ///  Associate the IProbeMgrGen2's event -- OnTBUploadEvent -- with OnTBUploadEvent2
        /// </summary>
        event ProbeCommEvent IProbeMgrGen2.OnProbeCommEvent
        {
            add
            {
                ProbeCommEvent2 += value;
            }
            remove
            {
                ProbeCommEvent2 -= value;
            }
        }


        /// <summary>
        /// Configure the probe
        /// </summary>
        /// <param name="deviceNum"></param>
        /// <param name="serialNumberStr"></param>
        /// <param name="probeIsRunning"></param>
        /// <param name="inDemoMode"></param>
        /// <param name="flashAddr"></param>
        /// <returns></returns>
        public bool Configure(int deviceNum, string serialNumberStr, bool inDemoMode)
        {
            //m_deviceNum = deviceNum;
            //m_serialNumber = serialNumberStr;
            //m_probeIsRunning = false;
            //m_demoMode = inDemoMode;
            //analyzerType = DP12MST_LA_TYPE.agilent;

            return true;
        }


        /// <summary>
        /// Initialize the Display Port Probe Mgr object
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            bool status = true;

            m_TBDataMgr.RegisterAsTBObject();
            m_TBDataMgr.LoadModuleConfig("Config_DP12MSTTraceBufferDataMgrForm.xml");
            return status;
        }


        /// <summary>
        /// Set the probe to a default configuration
        /// </summary>
        /// <returns></returns>
        public bool SetDefaultConfiguration()
        {
            return true;
        }


        /// <summary>
        /// Set the probe to a saved configuration
        /// </summary>
        /// <returns></returns>
        public bool SetStoredConfiguration(string configFileName, int selectedProtocolIndex)
        {
            bool status = true;

            // this may be handy to implement

            return status;
        }


        /// <summary>
        /// Save the current configuration
        /// </summary>
        /// <param name="configFileName"></param>
        /// <returns></returns>
        public bool SaveConfiguration(string configFileName, int protocolComboBoxIndex)
        {
            bool status = true;

            // this may be handy to implement

            return status;
        }


        /// <summary>
        /// Get the title string for the probe
        /// </summary>
        /// <returns></returns>
        public string GetTitleString()
        {
            return "Display Port Pixel Renderer 1.0";
        }


        /// <summary>
        /// Return the assembly version 
        /// </summary>
        /// <returns></returns>
        public string GetAssemblyVersion()
        {
            ////Assembly ThisAssembly = Assembly.GetExecutingAssembly();
            ////AssemblyName ThisAssemblyName = ThisAssembly.GetName();
            ////return (string.Format("{0:D}.{1:D2}.{2:D4}",
            ////                        ThisAssemblyName.Version.Major,
            ////                        ThisAssemblyName.Version.Minor,
            return "TBD";
        }


        /// <summary>
        /// Returns the FPGA reversion information
        /// </summary>
        /// <returns></returns>
        public string GetFPGAVersion()
        {
            return string.Empty;
        }


        /// <summary>
        /// Run the probe and disable the appropriate controls on various forms
        /// </summary>
        /// <returns></returns>
        public bool Run()
        {
            bool status = true;

            return status;
        }


        /// <summary>
        /// Part 1 of Stopping the probe and enable the appropriate controls on various forms
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            bool status = true;

            return status;
        }


        /// <summary>
        /// Part II of stopping the HW... 
        /// </summary>
        /// <returns></returns>
        public bool Stopped()
        {
            bool status = true;

            return status;
        }


        /// <summary>
        /// Close all forms currently being used.
        /// </summary>
        /// <returns></returns>
        public bool ShutDown()
        {
            bool status = true;
       
            return status;
        }


        /// <summary>
        /// The program is being terminated.  Close everything that needs to be 
        /// gracefully exited.
        /// </summary>
        public void CloseProbe()
        {
        
        }


        /// <summary>
        /// Display the requested form.
        /// </summary>
        /// <param name="FormName"></param>
        /// <returns></returns>
        public bool DisplayForm(string FormName)
        {
            bool status = true;

            return status;
        }



        /// <summary>
        /// Timer Thread the expires once per second.  Examines various registers, both
        /// probe e.g. STAT and SERDES registes (for SA and SB), and makes a determine 
        /// if an error has occurred.  If so, a message is written to a msg log that is
        /// printed out in the error log dialog, if error msg'ing is enabled.
        /// 
        /// The process is based on a collection of bits that we've termed syndrome bits.
        /// There are two sets of bits, one for SERDES A and a second for SERDES B.  The
        /// messaging scheme produces one msg per SERDES.
        /// 
        /// The current syndrome bits are compared to the previous bits, if there are the
        /// same, no messages are produced. 
        ///
        /// </summary>
        public bool ProcessTimerTick(RunTimeParameters parameters)
        {
            bool status = true;

            return status;
        }


        /// <summary>
        /// Cycle through the objects and collect any revelant log messages to be printed.
        /// </summary>
        /// <returns></returns>
        public string GetLogMsgs()
        {
            StringBuilder sb = new StringBuilder();

            return sb.ToString();
        }


        /// <summary>
        /// Process a request from the main/parent form.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="parameters"></param>
        public void MiscOperation(string title, object parameters = null)
        {
        }



        //
        // Pixel Renderer Additions


        /// <summary>
        /// returns the vchannel ID
        /// </summary>
        /// <returns></returns>
        public int GetTriggerChannelID()
        {
            DP12MSTMessage msg = m_DP12MSTMessageGenerator.DP12MSTMessageCreate_GetTBBinFileMetaData();
            ((DP12MSTMessage_GetTBBinFileMetaData)msg).TBMode = DP12MST_TRACE_BUFFER_MODE.MainLink;  // main or auxiliary link
            ((DP12MSTMessage_GetTBBinFileMetaData)msg).VChannelID = 1;

            m_eventPump.ForwardEvent(msg);      // the answer will be in the next statement before this one ends...
                                                // that is because .NET events are synchronous and blocking...

            return ((DP12MSTMessage_GetTBBinFileMetaData)msg).VChannelID;
        }


        /// <summary>
        /// Get the trigger state index 
        /// </summary>
        /// <param name="virtualChannelID"></param>
        /// <returns></returns>
        public long GetTriggerStateIndex(int virtualChannelID)
        {
            int index = -1;

            DP12MSTMessage msg = m_DP12MSTMessageGenerator.DP12MSTMessageCreate_GetTBBinFileMetaData();
            ((DP12MSTMessage_GetTBBinFileMetaData)msg).TBMode = DP12MST_TRACE_BUFFER_MODE.MainLink;  // main or auxiliary link
            ((DP12MSTMessage_GetTBBinFileMetaData)msg).VChannelID = virtualChannelID;

            m_eventPump.ForwardEvent(msg);      // the answer will be in the next statement before this one ends...
                                                // that is because .NET events are synchronous and blocking...

            switch (virtualChannelID)
            {
                case 0:
                    index = ((DP12MSTMessage_GetTBBinFileMetaData)msg).TriggerOffset;
                    break;

                case 1:
                    index = ((DP12MSTMessage_GetTBBinFileMetaData)msg).VChannel1TrigIndex;
                    break;

                case 2:
                    index = ((DP12MSTMessage_GetTBBinFileMetaData)msg).VChannel2TrigIndex;
                    break;

                case 3:
                    index = ((DP12MSTMessage_GetTBBinFileMetaData)msg).VChannel3TrigIndex;
                    break;

                case 4:
                    index = ((DP12MSTMessage_GetTBBinFileMetaData)msg).VChannel4TrigIndex;
                    break;

                default:
                    break;
            }

            return (index);
        }


        /// <summary>
        /// Get the number of states (-1) contained in the uploaded data.
        /// </summary>
        /// <param name="virtualChannelID"></param>
        /// <returns></returns>
        public long GetNumberOfStates(int virtualChannelID)
        {
            //m_TBDataMgr.LoadModuleConfig( "Config_DP12MSTTraceBufferDataMgrForm.xml");  // should we make an additional IProbeMgr I/F method to load the module?

            DP12MSTMessage msg = m_DP12MSTMessageGenerator.DP12MSTMessageCreate_GetTBBinFileMetaData();
            ((DP12MSTMessage_GetTBBinFileMetaData)msg).TBMode = DP12MST_TRACE_BUFFER_MODE.MainLink;  // main or auxiliary link
            ((DP12MSTMessage_GetTBBinFileMetaData)msg).VChannelID = virtualChannelID;


            m_eventPump.ForwardEvent(msg);      // the answer will be in the next statement before this one ends...
                                                // that is because .NET events are synchronous and blocking...

            return (((DP12MSTMessage_GetTBBinFileMetaData)msg).NumberOfStates);
        }


        /// <summary>
        /// Get the byte data for the requested state.
        /// </summary>
        /// <param name="virtualChannelID"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public byte[] GetStateData(int virtualChannelID, int index)
        {
            // push a new message through the event pump to get the requested state data.

            DP12MSTMessage_GetStateData msg = new DP12MSTMessage_GetStateData(0, index);
            msg.VChannelID = virtualChannelID;

            m_eventPump.ForwardEvent(msg);    // <== heads up here.... the data is copied into the msg's state data byte []
                                              // before it returns... so, in effect, you get your data before the statement exits!
                                              // if an error occurs, the byte[] is cleared and contains only zeros.

            // As an alternative, you could request a state, and handle the state data response message
            // in the processDPEvent(), that forces a design decision on the higher pixel renderer
            // code... as it would would not block on the call to get the state data and you'd have to
            // encode polling or waiting on the data... not the easiest coding... 

            // I like this approach better, as I can return the data to the called in the same methods...
            // making things easier to comprehend!

            return msg.DataBytes;
        }


        /// <summary>
        /// Request a chuck of data... data copied into the request message.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="chunkSize"></param>
        /// <returns></returns>
        public List<byte> GetStateDataChunk(int startIndex, int chunkSize, int VChannelID = 1)
        {
            DP12MSTMessage_StateDataChunkRequest msg = m_DP12MSTMessageGenerator.DP12MSTMessageCreate_StateDataChunkRequest(0, VChannelID, startIndex, chunkSize);
            m_eventPump.ForwardEvent(msg);

            return msg.DataChunk;  // stubbed code...return true;
        }


        /// <summary>
        /// Set the path in which the binary data file reside.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool SetDataFolderPath(string path)
        {
            bool status = true;

            status = m_TBDataMgr.SetDataFolderPath(path);

            return status;
        }

        #endregion // IProbeMgr Interface methods
    }

    #region DPEvent Message Definition

    // .NET event used for TraceBuffer objects communication.
    // Typically, the Gen2 object simply forwards the event to the 
    // message pump so that the event is seen by all the objects 
    // behind the ITBObject I/F.
    //

    public delegate void DP12MSTEvent(object sender, DP12MSTEventArgs e);

    /// <summary>
    /// Derived Event Args used to contain DP Message objects 
    /// </summary>
    public class DP12MSTEventArgs : EventArgs
    {
        public DP12MSTMessage Message;

        public DP12MSTEventArgs(DP12MSTMessage message)
        {
            this.Message = message;
        }
    }

    #endregion // DPEvent Message Definition
}
