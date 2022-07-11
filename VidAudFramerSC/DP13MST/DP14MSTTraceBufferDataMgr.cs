using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
//using System.Windows.Forms;
//using FTDI_USB3_Probe_Interface;
using SharedProject1;

using FPSProbeMgr_Gen2;

namespace DP14MSTClassLibrary
{
    public enum DP14MSTTBDataType { MainLinkSingleThread, MainLinkMultiThread, AuxiliaryLinkSingleThread, Unknown }

    //[ConfigComponent(1)]
    public class DP14MSTTraceBufferDataMgr : ITBObject
    {
        #region Members

        private enum DP14MSTTBDataMgrMode { SingleThreaded, MultiThreaded, Unknown }
        private enum LinkID { Main, Auxiliary, Unknown };

        private class DP14MSTTBParameters
        {
            public byte W4TIN { get; set; }
            public byte TBFLUSHED { get; set; }
            public byte TBFIN { get; set; }
            public byte TRIG { get; set; }
            public byte WRAP { get; set; }
            public byte ALMOSTTRIG { get; set; }
            public byte WI2NDLVL { get; set; }
            public long PRE_TRIG_LINE_COUNT { get; set; }
            public long PST_TRIG_LINE_CNT { get; set; }
            public long TIME_COUNT { get; set; }
            public long TRIG_LINE { get; set; }
        }

        private class DP14MSTAuxParameters
        {
            public byte AW4TIN { get; set; }
            public byte TBFIN { get; set; }
            public byte TRIG { get; set; }
            public byte WRAP { get; set; }
            public byte AWI2NDLVL { get; set; }
            public long PRE_TRIG_LINE_COUNT { get; set; }
            public long PST_TRIG_LINE_CNT { get; set; }
            public long TIME_COUNT { get; set; }
            public long TRIG_LINE { get; set; }
        }

        public class DP14MSTTBUploadDescriptor
        {
            public long StartState { get; set; }
            public long EndState { get; set; }
            public long TriggerState { get; set; }
            public long TriggerOffset { get; set; }
            public long NumOfStates { get; set; }
            public long MemoryDepth { get; set; }
            public uint BytesPerPage { get; set; }
            public uint BytesPerState { get; set; }
            public byte Wrapped { get; set; }
            public HWDataUploadStatusEvent uploadStatusEvent { get; set; }

            public DP14MSTTBUploadDescriptor(long startState, long endState, long triggerState, long triggerOffset, long numOfStates, byte wrapped,
                                        long memDepth, uint bytesPerPg = 65536, uint bytesPerState = (128 / 8), HWDataUploadStatusEvent dataUploadStatusEvent = null)
            {
                StartState = startState;
                EndState = endState;
                TriggerState = triggerState;
                TriggerOffset = triggerOffset;
                NumOfStates = numOfStates;
                Wrapped = wrapped;
                MemoryDepth = memDepth;
                BytesPerPage = bytesPerPg;
                BytesPerState = bytesPerState; // hard coded for now, but we can use constants in the future.
                uploadStatusEvent = dataUploadStatusEvent;
            }
        }

        public class DP14MSTTBSearchDescriptor
        {
            public DP14MST_TRACE_BUFFER_MODE TBMode;
            public string ColumnName = string.Empty;
            public uint ColumnValue = 0x00;
            public int VChannelID = -1;
            public long StateIndex;
            public bool SearchForward = true;

            public DP14MSTTBSearchDescriptor(DP14MST_TRACE_BUFFER_MODE listingType, string cName, uint cValue, int vChannelID, long stateIndex, bool searchForward)
            {
                TBMode = listingType;
                ColumnName = cName;
                ColumnValue = cValue;
                VChannelID = vChannelID;
                StateIndex = stateIndex;
                SearchForward = searchForward;
            }
        }

        private class DP14MSTTaskStateObject
        {
            public string FolderPath { get; set; }

            public long TrigStateIndex { get; set; }

            public long StartStateIndex { get; set; }

            public long[] FileStartIndices { get; set; }

            public long BlockSize { get; set; }

            public long MemoryDepth { get; set; }

            public long NumOfCapturedStates { get; set; }

            //public FT60x_IF MyFTD2xxIF { get; set; }

            public HWDataUploadStatusEvent UploadStatusEvent { get; set; }

            public int Index { get; set; }

            public CancellationToken Token { get; set; }

            //public DP14MSTTaskStateObject(string path, long trigStateOffset, long startStateIndex, long[] fileStartIndices, long blockSize, long memoryDepth, long numOfCapturedStates, FT60x_IF interfaceRef, HWDataUploadStatusEvent dataUploadStatusEvent, int index, CancellationToken ct)
            //{
            //    FolderPath = path;
            //    TrigStateIndex = trigStateOffset;
            //    StartStateIndex = startStateIndex;
            //    FileStartIndices = new long[fileStartIndices.Length];
            //    Array.Copy(fileStartIndices, FileStartIndices, FileStartIndices.Length);
            //    BlockSize = blockSize;
            //    MemoryDepth = memoryDepth;
            //    NumOfCapturedStates = numOfCapturedStates;
            //    //MyFTD2xxIF = interfaceRef;
            //    UploadStatusEvent = dataUploadStatusEvent;
            //    Index = index;
            //    Token = ct;
            //}
        }

        private class DP14MSTColumnMetaData
        {
            private string m_columnName = "";
            public string ColumnName
            {
                get { return m_columnName; }
                set { m_columnName = value; }
            }

            private int m_columnOffset = 0x00;
            public int ColumnOffset
            {
                get { return m_columnOffset; }
                set { m_columnOffset = value; }
            }

            private int m_columnWidth = 0x00;
            public int ColumnWidth
            {
                get { return m_columnWidth; }
                set { m_columnWidth = value; }
            }
        }

        /// <summary>
        /// Class used to hold meta data assoicated with multi-thread data files.
        /// </summary>
        private class DP14MSTMultiThread_MetaDataArgs
        {
            //    private int m_startState = -1;
            //    public int StartState
            //    {
            //        get { return m_startState; }
            //        set { m_startState = value; }
            //    }


            //    private int m_endState = -1;
            //    public int EndState
            //    {
            //        get { return m_endState; }
            //        set { m_endState = value; }
            //    }


            //    private int m_trigVChannelID = -1;
            //    public int TrigVChannelID
            //    {
            //        get { return m_trigVChannelID; }
            //        set { m_trigVChannelID = value; }
            //    }


            //    private int m_VChannel1ID = -1;
            //    public int VChannel1ID
            //    {
            //        get { return m_VChannel1ID; }
            //        set { m_VChannel1ID = value; }
            //    }


            //    private int m_VChannel2ID = -1;
            //    public int VChannel2ID
            //    {
            //        get { return m_VChannel2ID; }
            //        set { m_VChannel2ID = value; }
            //    }


            //    private int m_VChannel3ID = -1;
            //    public int VChannel3ID
            //    {
            //        get { return m_VChannel3ID; }
            //        set { m_VChannel3ID = value; }
            //    }


            //    private int m_VChannel4ID = -1;
            //    public int VChannel4ID
            //    {
            //        get { return m_VChannel4ID; }
            //        set { m_VChannel4ID = value; }
            //    }


            //    private int m_dataOffset = -1;
            //    public int DataOffset
            //    {
            //        get { return m_dataOffset; }
            //        set { m_dataOffset = value; }
            //    }
        }

        private const string BIN_FILE_HEADER_INFO_NUM_OF_STATES = "NumOfStates";
        private const string BIN_FILE_HEADER_INFO_TRIG_OFFSET = "TrigOffset";

        private const string BIN_FILE_MULTI_HEADER_START_STATE = "StartState";
        private const string BIN_FILE_MULTI_HEADER_END_STATE = "EndState";
        private const string BIN_FILE_MULTI_HEADER_INFO_TRIG_OFFSET = "TrigOffset"; //"TrigState";
        private const string BIN_FILE_TRIG_VCHANNEL_ID = "TrigVChannelID";
        private const string BIN_FILE_VCHANNEL1_TRIG_INDEX = "vChannel1TrigState";
        private const string BIN_FILE_VCHANNEL2_TRIG_INDEX = "vChannel2TrigState";
        private const string BIN_FILE_VCHANNEL3_TRIG_INDEX = "vChannel3TrigState";
        private const string BIN_FILE_VCHANNEL4_TRIG_INDEX = "vChannel4TrigState";

        private const int MAX_VCHANNELS = 4;  // VC1, VC2, VC3 and VC4

        private const int SMALL_DATA_UPLOAD_NUM_OF_COLUMNS   = 2;
        private const int MEDIUM_DATA_UPLOAD_NUM_OF_COLUMNS  = 4;
        private const int LARGE_DATA_UPLOAD_NUM_OF_COLUMNS   = 4;
        private const int LARGEST_DATA_UPLOAD_NUM_OF_COLUMNS = 4;

        private const int SMALL_DATA_UPLOAD_NUM_OF_LEVELS    = 2;
        private const int MEDIUM_DATA_UPLOAD_NUM_OF_LEVELS   = 4;
        private const int LARGE_DATA_UPLOAD_NUM_OF_LEVELS    = 8;  //4
        private const int LARGEST_DATA_UPLOAD_NUM_OF_LEVELS  = 16;

        private const int AUX_STATE_BYTE_LEN = 16;
        private const int AUX_STATUS_LOOP_BYTE_LEN = 14;
        private const int AUX_TRACE_BUFFER_LOOP_ID = 0x47;
        private const int AUX_TRACE_BUFFFER_STATUS_STROBE_REG_ID = 0x47;
        private const int AUX_TB_STATE_BYTE_LEN = 16;
        private const int AUX_TB_STATES_PER_PAGE = 512;
        private const int AUX_TB_PAGE_BYTE_LENGTH = AUX_TB_STATES_PER_PAGE * AUX_TB_STATE_BYTE_LEN;

        private const int MAX_READ_ATTEMTPS = 3;
        private const int TRACE_BUFFER_LOOP_ID = 0x41;
        private const int TB_STATUS_LOOP_BYTE_LEN = 19;         // there are 9 data bytes
        private const int TB_BUF_STATUS_STROBE_REG_ID = 0x41;
        private const int TB_MAX_SINGLE_THREAD_SIZE = 0x100000;  // 1M == 0x100000, 2M = 0x200000, 4M = 0x400000, 8M = 0x800000, 16M = 0x1000000

        private const int TB_MAX_THREAD_SIZE_SMALL      =    0x40000;   // 8K - 256K 
        private const int TB_MAX_THREAD_SIZE_MEDIUM     =  0x1000000;   // 512K - 16M
        //private const int TB_MAX_THREAD_SIZE_LARGE    = 0x10000000;   // 32M - 256M 
        private const int TB_MAX_THREAD_SIZE_LARGE      = 0x04000000;   // 32M - 64M 
        private const int TB_MAX_THREAD_SIZE_VERY_LARGE = 0x40000000;   // 512 - 1G

        private const int TB_NUM_OF_THREADS_SMALL      = 1;     // 8K - 256K states
        private const int TB_NUM_OF_THREADS_MEDIUM     = 8;     // 512K - 16M states
        private const int TB_NUM_OF_THREADS_LARGE      = 16;    // 32m - 256M states
        private const int TB_NUM_OF_THREADS_VERY_LARGE = 32;    // 512M - 1G states 

        private const int DEFAULT_VCHANNEL_ID = 1;

       // private CancellationTokenSource tokenSource = null; // tokenSource.Token;
       // private CancellationToken token = CancellationToken.None;

        private const int TB_STATE_BYTE_LEN = 16;  // FS4500 TB state size
        private const int TB_STATES_PER_PAGE = 4096;
        private const int TB_PAGE_BYTE_LENGTH = TB_STATES_PER_PAGE * TB_STATE_BYTE_LEN;
        private const int PROGRESS_INDICTOR_TIMER_INTERVAL = 100;

        private const int SEARCH_REPORTING_INTERVAL_SIZE = 4 * 1024;

        //private string m_FS4500_FOLDER_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "FuturePlus";
        //private string m_FS4500_FOLDER_NAME = "FS4500";
        private string m_FS4500_TRACE_FILE_BASE_NAME = "TraceData.bin";
        private string m_FS4500_AUX_TRACE_FILE_BASE_NAME = "AuxTraceData.bin";
        private const string MainLink_COLUMNS_DEFS_XML_FILENAME = "DP14MSTTBDataFormat.xml";
        private const string AuxiliaryLink_COLUMNS_DEFS_XML_FILENAME = "DP14MSTAuxTBDataFormat.xml";
        private string FS4500_FOLDER_PATH = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"FuturePlus\FS4500");
        private const string CONFIG_XML_FILENAME = "Config_DP14MSTTraceBufferDataMgrForm.xml";
        private const string TIMESTAMP_FIELD_NAME = "Time";
        private const string VCTAG_FIELD_NAME = "VCTag";

        private string m_instanceFolderPath = string.Empty;

        private string m_FS4500_MULTI_THREAD_TRACE_FILE_BASE_NAME = "TraceData_";

        private string m_auxTraceFilePath = string.Empty;
        private string m_traceFilePath = string.Empty;

        private DP14MST_TRACE_BUFFER_MODE m_TBMode = DP14MST_TRACE_BUFFER_MODE.MainLink;        // Main, Auxiliary, Both or Unknown
        private DP14MSTTBDataMgrMode m_TBDataMode = DP14MSTTBDataMgrMode.MultiThreaded;      // singleThreaded, MultiThreaded, unknown

        // the idea is that only one task will update this variable.
        private volatile string m_triggerBinaryFileName = string.Empty;

        //private List<DP14MSTMultiThread_MetaDataArgs> m_multiThreadFileMetaData_VC1 = new List<DP14MSTMultiThread_MetaDataArgs>();
        //private List<DP14MSTMultiThread_MetaDataArgs> m_multiThreadFileMetaData_VC2 = new List<DP14MSTMultiThread_MetaDataArgs>();
        //private List<DP14MSTMultiThread_MetaDataArgs> m_multiThreadFileMetaData_VC3 = new List<DP14MSTMultiThread_MetaDataArgs>();
        //private List<DP14MSTMultiThread_MetaDataArgs> m_multiThreadFileMetaData_VC4 = new List<DP14MSTMultiThread_MetaDataArgs>();

        private List<DP14MSTVCFileGenerator_Threads.DP14MSTMultiThread_MetaDataArgs> m_multiThreadFileMetaData = new List<DP14MSTVCFileGenerator_Threads.DP14MSTMultiThread_MetaDataArgs>();

        private List<DP14MSTColumnMetaData> m_columnMetaData_MainLink = new List<DP14MSTColumnMetaData>();
        private List<DP14MSTColumnMetaData> m_columnMetaData_AuxiliaryLink = new List<DP14MSTColumnMetaData>();

        private int m_multiThreadFileSize = 0;
        private int m_numOfTPITasks = 12;
        private int m_numOfTaskColumns = 2;
        private int m_numOfDataFiles = 0;

        private long m_dataOffset_mainLink = 0;
        private long m_dataOffset_auxLink = 0;

        // Create an event which the main form can register for... all Trace Buffer
        // objects will raise the same event but with different parameters... the
        // Event class is defined in the DP 1.1a main form.
        //
        public event DP14MSTEvent DisplayPort12MSTEvent;

        // HW data upload is accommplished on a Background worker thread or in multiple TPI tasks..
        private BackgroundWorker m_worker;
        private BackgroundWorker m_auxWorker;
        private BackgroundWorker m_searchWorker_SingleThread;
        private BackgroundWorker m_searchWorker_MultiThread;
        private volatile bool m_MainLinkSingleThreadSearch = true;

        // create an event that can be used to inform the top level object on the upload percentage .
        public HWDataUploadStatusEvent TBDataUploadStatusEvent;
        public StopRequestCompleteEvent StopRequestCompletedEvent;
        public LogMsgEvent LogMsgEvent;

        // bool to control when state data requests will be honored.
        private bool m_dataEnabled = false;

        // trouble shooting/integration capability
        private const bool m_Debug = false; //true;

        private string[] m_TBStatusFields = new string[] { "Pad:2", "W4TIN:1", "TBFLUSHED:1", "TBFIN:1", "TRIG:1", "WRAP:1",
                                                            "ALMOSTTRIG:1", "WI2NDLVL:1", "PRE_TRIG_LINE_COUNT:31", "PST_TRIG_LINE_CNT:31",
                                                            "TIME_COUNT:50", "TRIG_LINE:31" };
        private byte[] m_TBStatusFieldWidths = null;
        private DP14MSTTBParameters m_TBParameters = null;

        private string[] m_AUXStatusFields = new string[]  { "Pad:4", "AW4TIN:1", "Pad:1", "TBFIN:1", "TRIG:1", "WRAP:1", "Pad:1", "AWI2NDLVL:1",
                                                            "PRE_TRIG_LINE_COUNT:17", "PST_TRIG_LINE_CNT:17", "TIME_COUNT:50", "TRIG_LINE:17" };
        private byte[] m_AUXStatusFieldWidths = null;
        private DP14MSTAuxParameters m_AUXParameters = null;


        //private FieldExtractor m_fldExtractor = null;

        private long m_currentStateNumber = long.MinValue;
        private byte[] m_currentStateData = new byte[TB_STATE_BYTE_LEN];

        private long m_auxCurrentStateNumber = long.MinValue;
        private byte[] m_auxCurrentStateData = new byte[AUX_STATE_BYTE_LEN];


        private long m_memoryDepth = 8192;
        public long MemoryDepth
        {
            get { return m_memoryDepth; }
            set { m_memoryDepth = value; }
        }

        private long m_auxMemoryDepth = 128 * 1024; //16 * 1024;
        public long AuxMemoryDepth
        {
            get { return m_auxMemoryDepth; }
            set { m_auxMemoryDepth = value; }
        }

        private int m_metaDataHeaderLength = 0x00;
        private bool m_generateMetaData = true; //false;
        private bool m_ExitRunMode = false;

        //private FT60x_IF m_FTD2xxIF = null;
        private DP14MSTMessageGenerator m_DP12MSTMessageGenerator = null;
        //private DP14MSTVCFileGenerator m_VCFileGenerator = null;

        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14MSTTraceBufferDataMgr(string path)
        {
            //m_FTD2xxIF = FT60x_IF.GetInstance();
            m_DP12MSTMessageGenerator = DP14MSTMessageGenerator.GetInstance();
            //m_VCFileGenerator = DP14MSTVCFileGenerator.GetInstance();
            //m_VCFileGenerator.VCFileGenStatusEvent += new VCFileGenerationStatusEvent(processVCGenerationProgressEvent);

            // all uploads result in creating the same file...
            //m_auxTraceFilePath = Path.Combine(m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME, m_FS4500_AUX_TRACE_FILE_BASE_NAME);
            //m_traceFilePath = Path.Combine(m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME, m_FS4500_TRACE_FILE_BASE_NAME);


            m_instanceFolderPath = path;

            m_auxTraceFilePath = Path.Combine(m_instanceFolderPath, m_FS4500_AUX_TRACE_FILE_BASE_NAME);
            m_traceFilePath = Path.Combine(m_instanceFolderPath, m_FS4500_TRACE_FILE_BASE_NAME);

            //getColumnMetaData(DP14MSTTRACE_BUFFER_MODE.MainLink);
            //getColumnMetaData(DP14MSTTRACE_BUFFER_MODE.AuxiliaryLink);

            m_numOfDataFiles = (getNumberOfDataFiles()/5) * 4;
            m_multiThreadFileMetaData = initVChannelMetaData(m_numOfDataFiles);
        }

        #endregion // Constructor(s)

        #region Event Handlers

        ///// <summary>
        ///// process a progress report from one of the virtual file generation objects.
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void processVCGenerationProgressEvent(object sender, VCFileGenerationStatusEventArgs e)
        //{
        //    // re-used the TB data upload event that reports the percentage uploaded to the main form...
        //    // only this time, we are reporting the percentage of states segregated into the virtual channel files.
        //    if (TBDataUploadStatusEvent != null)
        //    {
        //        if ((e.Title == "Processing MST Data...")       ||
        //            (e.Title == "Processing MST Phase (P1)...") ||
        //            (e.Title == "Processing MST Phase (P2)...") ||
        //            (e.Title == "Processing MST Phase (P3)...") ||
        //            (e.Title == "Processing MST Phase (P4)...") ||
        //            (e.Title == "Processing MST Phase (P5)..."))
        //        {
        //            // raise an event that informs the main form that the post processing has started (Segregation Files).
        //            TBDataUploadStatusEvent(this, new HWDataUploadStatusEventArgs(e.Title, true, 0, 0, e.Parameter));
        //        }
        //        else if (e.Title == "Processing MST Complete (P1)...")
        //        {
        //            // raise an event that informs the main form that the post processing is in progress; locationg trigger in segregation files
        //            TBDataUploadStatusEvent(this, new HWDataUploadStatusEventArgs(e.Title, true, (int)e.Parameter));

        //            // segregation of the data is complete... time to get the trigger state location
        //            getSegregatedTriggerLocation(m_triggerTimeStamp, (int)m_triggerVChannelID);
        //        }
        //        else if (e.Title == "Processing MST Complete (P2)...")
        //        {
        //            // raise an event that informs the main form that the post processing is in progress; gathering Meta data on all segregation files
        //            TBDataUploadStatusEvent(this, new HWDataUploadStatusEventArgs(e.Title, true, (int)e.Parameter));

        //            // trigger has been located... time to assemble the meta data for all data
        //            getSegregatedMetaData(m_triggerTimeStamp, (int)m_triggerVChannelID);
        //        }
        //        else if (e.Title == "Processing MST Complete (P3)...")
        //        {
        //            // raise an event that informs the main form that the post processing is in progress; updating meta data with start/end indices for all files
        //            TBDataUploadStatusEvent(this, new HWDataUploadStatusEventArgs(e.Title, true, (int)e.Parameter));

        //            // Meta data is partially filled, time to finish
        //            setMetaDataTriggerStateIndices(m_triggerTimeStamp, (int)m_triggerVChannelID);
        //        }
        //        else if (e.Title == "Processing MST Complete (P4)...")
        //        {
        //            // raise an event that informs the main form that the post processing has started.
        //            TBDataUploadStatusEvent(this, new HWDataUploadStatusEventArgs(e.Title, true, (int)e.Parameter));

        //            // Segregated Meta Data is ready... time to prepend the meta data to all files
        //            prependSegregatedMetaData(m_triggerTimeStamp, (int)m_triggerVChannelID);
        //        }
        //        else if (e.Title == "Processing MST Data Complete")
        //        {
        //            // raise an event that informs the main form the post processing is in progress;  prepending the meta data to all segregation files.
        //            // which will cause the forms to be re-enabled.
        //            TBDataUploadStatusEvent(this, new HWDataUploadStatusEventArgs("VC File Generation Complete", true, (int)e.Parameter));

        //            // initialize the meta data array from the data files that is used when getting state data.  
        //            m_multiThreadFileMetaData = getMultiThreadedFileMetaData(); // initVChannelMetaData(calculateNumOfDataBlocks(m_memoryDepth) * 4);

        //            // The msg is generated here because of the TPI Tasks used to segregate the data... it is only at this point
        //            // that we should indicate that the data is ready for display

        //            // generate the data ready event that causes the event pump to send the event to all TBObjects.
        //            // which will cause state listing forms to update the IA Control content.
        //            if (DisplayPort12MSTEvent != null)
        //                DisplayPort12MSTEvent(this, new DP12MSTEventArgs((DP12MSTMessage)(m_DP12MSTMessageGenerator.DP12MSTMessageCreate_DataReady())));
        //        }
        //        //else if (e.Title == "VCGenerationInProgress")
        //        //{
        //        //    // raise an event that informs the main form if the percentage of states that have been segregated into the virtual channel files.
        //        //    TBDataUploadStatusEvent(this, new HWDataUploadStatusEventArgs(e.Title, true, (int)e.Parameter, (int)m_FTD2xxIF.NumOfCapturedStates));
        //        //}
        //        else if (e.Title.StartsWith("DebugMsg"))
        //        {
        //            TBDataUploadStatusEvent(this, new HWDataUploadStatusEventArgs(e.Title, true));
        //        }
        //    }
        //}


        ///// <summary>
        ///// Process the event assoicated with a main link search request
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void DoWork_TBSearchSingleThread(object sender, DoWorkEventArgs e)
        //{
        //    if (m_searchWorker_SingleThread.CancellationPending)
        //    {
        //        e.Cancel = true;
        //    }
        //    else
        //    {
        //        long stateIndex = -1;
        //        if (((DP14MSTTBSearchDescriptor)e.Argument).SearchForward)
        //        {
        //            stateIndex = searchListing_SingleThread_Forward(((DP14MSTTBSearchDescriptor)e.Argument).TBMode,
        //                                                            ((DP14MSTTBSearchDescriptor)e.Argument).ColumnName,
        //                                                            ((DP14MSTTBSearchDescriptor)e.Argument).ColumnValue,
        //                                                            ((DP14MSTTBSearchDescriptor)e.Argument).StateIndex,
        //                                                            SEARCH_REPORTING_INTERVAL_SIZE);
        //        }
        //        else  // search backwards
        //        {
        //            stateIndex = searchListing_SingleThread_Backward(((DP14MSTTBSearchDescriptor)e.Argument).TBMode,
        //                                                            ((DP14MSTTBSearchDescriptor)e.Argument).ColumnName,
        //                                                            ((DP14MSTTBSearchDescriptor)e.Argument).ColumnValue,
        //                                                            ((DP14MSTTBSearchDescriptor)e.Argument).StateIndex,
        //                                                            SEARCH_REPORTING_INTERVAL_SIZE);
        //        }

        //        //if (stateIndex == -1)
        //        e.Result = stateIndex;
        //        //else
        //        //    e.Result = true;
        //    }
        //}


        ///// <summary>
        ///// Process the event associated with the main link search progress status changing.
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void ProgressChanged_TBSearchSingleThread(object sender, ProgressChangedEventArgs e)
        //{
        //    // raise an event back to the state listing form to get the progress bar updated...
        //    //  should update evey 1k state...

        //    // either way... send the response.
        //    if (DisplayPort12MSTEvent != null)
        //    {
        //        // Raise the response event containing the requested data
        //        DisplayPort12MSTEvent(this,
        //                            new DP12MSTEventArgs(m_DP12MSTMessageGenerator.DP12MSTMessageCreate_SearchListingProgessReport(DP14MSTTRACE_BUFFER_MODE.MainLink, SEARCH_REPORTING_INTERVAL_SIZE)));
        //    }
        //}


        ///// <summary>
        ///// Process the event assoicated with the main link seach completing
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void RunWorkerCompleted_TBSearchSingleThread(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    if (e.Cancelled)
        //    {
        //        // raise the response event to ensure all forms are re-enabled
        //    }
        //    else if (e.Error != null)
        //    {
        //        // Do something with the error
        //    }

        //    DP14MSTTRACE_BUFFER_MODE TBMode = DP14MSTTRACE_BUFFER_MODE.MainLink;
        //    if (m_MainLinkSingleThreadSearch == false)
        //        TBMode = DP14MSTTRACE_BUFFER_MODE.AuxiliaryLink;

        //    if ((long)(e.Result) > -1) //(bool)e.Result)
        //    {
        //        // raise an event  -- informing the top level object that the upload is done!
        //        if (DisplayPort12MSTEvent != null)
        //            DisplayPort12MSTEvent(this, new DP12MSTEventArgs((m_DP12MSTMessageGenerator.DP12MSTMessageCreate_SearchListingResponse(TBMode, 0, true, (long)(e.Result)))));
        //    }
        //    else
        //    {
        //        // raise an event  -- informing the top level object that the upload is done!
        //        if (DisplayPort12MSTEvent != null)
        //            DisplayPort12MSTEvent(this, new DP12MSTEventArgs((m_DP12MSTMessageGenerator.DP12MSTMessageCreate_SearchListingResponse(TBMode, 0, false, (long)(e.Result)))));
        //    }

        //    m_searchWorker_SingleThread.Dispose();
        //    m_searchWorker_SingleThread = null;
        //}


        ///// <summary>
        ///// Process the event assoicated with a main link search request
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void DoWork_TBSearchMultiThread(object sender, DoWorkEventArgs e)
        //{
        //    if (m_searchWorker_MultiThread.CancellationPending)
        //    {
        //        e.Cancel = true;
        //    }
        //    else
        //    {
        //        long stateIndex = -1;
        //        if (((DP14MSTTBSearchDescriptor)e.Argument).SearchForward)
        //        {
        //            stateIndex = searchListing_MultiThread_Forward(((DP14MSTTBSearchDescriptor)e.Argument).TBMode,
        //                                                            ((DP14MSTTBSearchDescriptor)e.Argument).ColumnName,
        //                                                            ((DP14MSTTBSearchDescriptor)e.Argument).ColumnValue,
        //                                                            ((DP14MSTTBSearchDescriptor)e.Argument).VChannelID,
        //                                                            ((DP14MSTTBSearchDescriptor)e.Argument).StateIndex,
        //                                                            SEARCH_REPORTING_INTERVAL_SIZE);
        //        }
        //        else  // search backwards
        //        {
        //            stateIndex = searchListing_MultiThread_Backward(((DP14MSTTBSearchDescriptor)e.Argument).TBMode,
        //                                                            ((DP14MSTTBSearchDescriptor)e.Argument).ColumnName,
        //                                                            ((DP14MSTTBSearchDescriptor)e.Argument).ColumnValue,
        //                                                            ((DP14MSTTBSearchDescriptor)e.Argument).VChannelID,
        //                                                            ((DP14MSTTBSearchDescriptor)e.Argument).StateIndex,
        //                                                            SEARCH_REPORTING_INTERVAL_SIZE);
        //        }

        //        //if (stateIndex == -1)
        //        e.Result = stateIndex;
        //        //else
        //        //    e.Result = true;
        //    }
        //}


        ///// <summary>
        ///// Process the event associated with the main link search progress status changing.
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void ProgressChanged_TBSearchMultiThread(object sender, ProgressChangedEventArgs e)
        //{
        //    // raise an event back to the state listing form to get the progress bar updated...
        //    //  should update evey 1k state...

        //    // either way... send the response.
        //    if (DisplayPort12MSTEvent != null)
        //    {
        //        // Raise the response event containing the requested data
        //        DisplayPort12MSTEvent(this,
        //                            new DP12MSTEventArgs(m_DP12MSTMessageGenerator.DP12MSTMessageCreate_SearchListingProgessReport(DP14MSTTRACE_BUFFER_MODE.MainLink, SEARCH_REPORTING_INTERVAL_SIZE)));
        //    }
        //}


        ///// <summary>
        ///// Process the event assoicated with the main link seach completing
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void RunWorkerCompleted_TBSearchMultiThread(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    if (e.Cancelled)
        //    {
        //        // raise the response event to ensure all forms are re-enabled
        //    }
        //    else if (e.Error != null)
        //    {
        //        // Do something with the error
        //    }

        //    if ((long)(e.Result) > -1) //(bool)e.Result)
        //    {
        //        // raise an event  -- informing the top level object that the upload is done!
        //        if (DisplayPort12MSTEvent != null)
        //            DisplayPort12MSTEvent(this, new DP12MSTEventArgs((m_DP12MSTMessageGenerator.DP12MSTMessageCreate_SearchListingResponse(DP14MSTTRACE_BUFFER_MODE.MainLink, 0, true, (long)(e.Result)))));
        //    }
        //    else
        //    {
        //        // raise an event  -- informing the top level object that the upload is done!
        //        if (DisplayPort12MSTEvent != null)
        //            DisplayPort12MSTEvent(this, new DP12MSTEventArgs((m_DP12MSTMessageGenerator.DP12MSTMessageCreate_SearchListingResponse(DP14MSTTRACE_BUFFER_MODE.MainLink, 0, false, (long)(e.Result)))));
        //    }

        //    m_searchWorker_MultiThread.Dispose();
        //    m_searchWorker_MultiThread = null;
        //}


        /// <summary>
        /// Get auxiliary link trace buffer data 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoWork_AuxUpload(object sender, DoWorkEventArgs e)
        {
            //bool status = true;
            //if (m_auxWorker.CancellationPending)
            //{
            //    e.Cancel = true;
            //}
            //else
            //{
            //    DP14MSTTBUploadDescriptor parameters = (DP14MSTTBUploadDescriptor)e.Argument;
            //    byte[] pageData = new byte[AUX_TB_PAGE_BYTE_LENGTH];
            //    int statesPerPage = AUX_TB_STATES_PER_PAGE;

            //    long stateOffset = parameters.StartState;
            //    long pageOffset = AUX_TB_STATES_PER_PAGE;
            //    long maxStateOffset = parameters.MemoryDepth;

            //    long numOfStatesUploaded = 0;
            //    long partialPageSize = 0;
            //    m_metaDataHeaderLength = 0x00;
            //    int retry = 0;

            //    //m_stopWatchTimesSB.Length = 0;

            //    // create a file stream to write too
            //    string traceFilePath = Path.Combine(m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME, m_FS4500_AUX_TRACE_FILE_BASE_NAME);
            //    using (BinaryWriter bw = new BinaryWriter(File.Open(traceFilePath, FileMode.Create, FileAccess.Write, FileShare.None)))
            //    {
            //        if (m_generateMetaData)
            //        {
            //            byte[] array = System.Text.Encoding.ASCII.GetBytes("NumOfStates:" + parameters.NumOfStates.ToString() + "\n");
            //            bw.Write(array);
            //            m_metaDataHeaderLength += array.Length;

            //            array = System.Text.Encoding.ASCII.GetBytes("TrigOffset:" + parameters.TriggerOffset.ToString() + "\n");
            //            bw.Write(array);
            //            m_metaDataHeaderLength += array.Length;

            //            array = System.Text.Encoding.ASCII.GetBytes("*****" + "\n");
            //            bw.Write(array);
            //            m_metaDataHeaderLength += array.Length;
            //            bw.Flush();
            //        }

            //        // record where the data starts (for processing individual states).
            //        m_dataOffset_auxLink = bw.BaseStream.Position;

            //        while (status && (!m_auxWorker.CancellationPending) && (numOfStatesUploaded < (parameters.NumOfStates - 1)) && (retry < 10))
            //        {
            //            Array.Clear(pageData, 0, pageData.Length);
            //            if ((stateOffset + pageOffset) <= parameters.MemoryDepth)
            //            {
            //                status = m_FTD2xxIF.GetAuxTraceBufferPage(stateOffset, ref pageData);
            //                if (status)
            //                {
            //                    retry = 0;
            //                    // determine if we want the entire page or just part of it... in terms of states and not bytes
            //                    if ((numOfStatesUploaded + statesPerPage) <= parameters.MemoryDepth)
            //                    {
            //                        if ((numOfStatesUploaded + statesPerPage) <= parameters.NumOfStates)
            //                        {
            //                            bw.Write(pageData);
            //                            stateOffset += pageOffset;
            //                            numOfStatesUploaded += AUX_TB_STATES_PER_PAGE;
            //                        }
            //                        else
            //                        {
            //                            partialPageSize = parameters.NumOfStates - numOfStatesUploaded;
            //                            bw.Write(pageData, 0, (int)(partialPageSize * parameters.BytesPerState));
            //                            numOfStatesUploaded += partialPageSize;
            //                        }
            //                    }
            //                    else
            //                    {
            //                        if (numOfStatesUploaded < parameters.MemoryDepth)
            //                        {
            //                            partialPageSize = parameters.MemoryDepth - numOfStatesUploaded;
            //                            bw.Write(pageData, 0, (int)(partialPageSize * parameters.BytesPerState));
            //                            numOfStatesUploaded += partialPageSize;
            //                        }
            //                        else
            //                        {
            //                            break;
            //                        }
            //                    }
            //                }
            //                else
            //                {
            //                    retry += 1;
            //                    if (retry < 10)
            //                    {
            //                        status = true;
            //                        Thread.Sleep(100);
            //                    }
            //                    //else
            //                    //    status = false;
            //                }
            //            }
            //            else // partial buffer scenerio
            //            {
            //                // get the part of the data that falls before the MemoryDepth Limit...
            //                partialPageSize = parameters.MemoryDepth - (parameters.StartState + numOfStatesUploaded);

            //                status = m_FTD2xxIF.GetAuxTraceBufferPage(stateOffset, ref pageData);
            //                if (status)
            //                {
            //                    retry = 0;
            //                    // write the parial page to the binary file
            //                    bw.Write(pageData, 0, (int)(partialPageSize * parameters.BytesPerState));

            //                    // set the stateOffset for the next page
            //                    stateOffset = stateOffset + (partialPageSize * parameters.BytesPerState);
            //                    numOfStatesUploaded += partialPageSize;
            //                    stateOffset = 0;
            //                }
            //                else
            //                {
            //                    retry += 1;
            //                    if (retry < 10)
            //                        status = true;
            //                    else
            //                        status = false;
            //                }
            //            }

            //            m_auxWorker.ReportProgress((int)((float)((float)numOfStatesUploaded / (float)parameters.MemoryDepth) * 100));
            //        }

            //        bw.Flush();
            //        bw.Close();
            //    }

            //    if (!status)
            //        clearDataUploadFolder();
            //}

            //if (status == true)
            //    e.Result = true;
            //else
            //    e.Result = false;
        }


        ///// <summary>
        ///// Update the status of uploading auxiliary link data
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void ProgressChanged_AuxUpload(object sender, ProgressChangedEventArgs e)
        //{
        //    if (TBDataUploadStatusEvent != null)
        //        TBDataUploadStatusEvent(this, new HWDataUploadStatusEventArgs("Aux TB Upload Percentage", true, e.ProgressPercentage));
        //}


        ///// <summary>
        ///// Process the event assoicated with the auxiliary link upload Background thread completing.
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void RunWorkerCompleted_AuxUpload(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    if (e.Cancelled)
        //    {
        //        // Display some message to the user that task has been
        //        // cancelled
        //    }
        //    else if (e.Error != null)
        //    {
        //        // Do something with the error
        //    }



        //    if ((bool)e.Result)
        //    {
        //        // raise an event  -- informing the top level object that the upload is done!
        //        if (TBDataUploadStatusEvent != null && m_ExitRunMode)
        //            TBDataUploadStatusEvent(this, new HWDataUploadStatusEventArgs("Aux Data Upload Complete", false));

        //        if ((DisplayPort12MSTEvent != null))
        //            DisplayPort12MSTEvent(this, new DP12MSTEventArgs((DP12MSTMessage)(m_DP12MSTMessageGenerator.DP12MSTMessageCreate_AuxDataReady())));
        //    }
        //    else
        //    {
        //        if (TBDataUploadStatusEvent != null)
        //            TBDataUploadStatusEvent(this, new HWDataUploadStatusEventArgs("Aux Data Upload Failed with Error", false));
        //    }

        //    m_auxWorker.Dispose();
        //    m_auxWorker = null;
        //}

        #endregion // Event Handlers

        #region Private Methods

        ///// <summary>
        ///// Initialize the field widths array for the loop
        ///// </summary>
        ///// <param name="fields"></param>
        ///// <param name="fieldWidths"></param>
        //private void initFieldWidthsArray(ref string[] fields, ref byte[] fieldWidths)
        //{
        //    // allocate the array to hold the field widths
        //    fieldWidths = new byte[fields.Length];

        //    // fill in each field width
        //    for (int i = 0; i < fields.Length; i++)
        //    {
        //        string fldID = fields[i];  // each fldID has the format of fieldName:width... "Pad:12"
        //        string[] comps = fldID.Split(new char[] { ':' });
        //        if (comps.Length == 2)
        //        {
        //            try
        //            {
        //                fieldWidths[i] = byte.Parse(comps[1]);
        //            }
        //            catch (Exception ex)
        //            {
        //                throw new Exception("Invalid Trig Field Definition: " + ex.Message);
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception("Invalid Trig Field Definition");
        //        }
        //    }
        //}


        ///// <summary>
        ///// Get the TB Status loop data
        ///// </summary>
        ///// <returns></returns>
        //private bool getTBStatusLoopData(ref byte[] loopData)
        //{
        //    bool status = false;

        //    // try for several attemps before calling it a failure...
        //    for (int attempt = 0; attempt < MAX_READ_ATTEMTPS; attempt++)
        //    {
        //        // Strobe the loop
        //        status = m_FTD2xxIF.SetRegister_FS45xx(TB_BUF_STATUS_STROBE_REG_ID, 0x01);
        //        if (status)
        //        {
        //            // Write the Loop Request pkt across the USB Connection
        //            status = m_FTD2xxIF.WriteLoopReadRequestPacket_FS45xx(TRACE_BUFFER_LOOP_ID);
        //            if (status)
        //            {
        //                // Read the response
        //                status = m_FTD2xxIF.ReadLoopResponse(ref loopData);
        //                if (status)
        //                    break;
        //            }
        //        }
        //    }

        //    return status;
        //}
        

        ///// <summary>
        ///// Get the TB Status loop data
        ///// </summary>
        ///// <returns></returns>
        //private bool getAuxTBStatusLoopData(ref byte[] loopData)
        //{
        //    bool status = false;

        //    // try for several attemps before calling it a failure...
        //    for (int attempt = 0; attempt < MAX_READ_ATTEMTPS; attempt++)
        //    {
        //        // Strobe the loop
        //        status = m_FTD2xxIF.SetRegister_FS45xx(AUX_TRACE_BUFFFER_STATUS_STROBE_REG_ID, 0x01);
        //        if (status)
        //        {
        //            // Write the Loop Request pkt across the USB Connection
        //            status = m_FTD2xxIF.WriteLoopReadRequestPacket_FS45xx(AUX_TRACE_BUFFER_LOOP_ID);
        //            if (status)
        //            {
        //                // Read the response
        //                status = m_FTD2xxIF.ReadLoopResponse(ref loopData);
        //                if (status)
        //                    break;
        //            }
        //        }
        //    }

        //    return status;
        //}


        ///// <summary>
        ///// Returns a data struct/class instance containing all TB status parameters.
        ///// </summary>
        /////         private string[] m_TBStatusFields = new string[] { "Pad:2", "W4TIN:1", "TBFLUSHED:1", "TBFIN:1", "TRIG:1", "WRAP:1",
        /////                                                    "ALMOSTTRIG:1", "WI2NDLVL:1", "PRE_TRIG_LINE_COUNT:31", "PST_TRIG_LINE_CNT:31",
        /////                                                    "TIME_COUNT:50", "TRIG_LINE:31" };
        ///// <returns></returns>
        //private bool GetTBStatusVariables(List<long> fldValues, DP14MSTTBParameters parameters)
        //{
        //    bool status = true;
        //    long pValue = 0x00;

        //    try
        //    {
        //        for (int i = 1; i < m_TBStatusFields.Length; i++)
        //        {
        //            pValue = fldValues[i];
        //            switch (i)
        //            {
        //                case 1:
        //                    parameters.W4TIN = (byte)pValue;
        //                    break;
        //                case 2:
        //                    parameters.TBFLUSHED = (byte)pValue;
        //                    break;
        //                case 3:
        //                    parameters.TBFIN = (byte)pValue;
        //                    break;
        //                case 4:
        //                    parameters.TRIG = (byte)pValue;
        //                    break;
        //                case 5:
        //                    parameters.WRAP = (byte)pValue;
        //                    break;
        //                case 6:
        //                    parameters.ALMOSTTRIG = (byte)pValue;
        //                    break;
        //                case 7:
        //                    parameters.WI2NDLVL = (byte)pValue;
        //                    break;
        //                case 8:
        //                    parameters.PRE_TRIG_LINE_COUNT = pValue;
        //                    break;
        //                case 9:
        //                    parameters.PST_TRIG_LINE_CNT = pValue;
        //                    break;
        //                case 10:
        //                    parameters.TIME_COUNT = pValue;
        //                    break;
        //                case 11:
        //                    parameters.TRIG_LINE = pValue;
        //                    break;

        //                default:
        //                    status = false;
        //                    break;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error getting TB paramter value: Invalid Value Encountered");
        //    }

        //    return status;
        //}


        ///// <summary>
        ///// Returns a data struct/class instance containing all TB status parameters.
        ///// </summary>
        /////         private string[] m_TBStatusFields = new string[] { "Pad:2", "W4TIN:1", "TBFLUSHED:1", "TBFIN:1", "TRIG:1", "WRAP:1",
        /////                                                    "ALMOSTTRIG:1", "WI2NDLVL:1", "PRE_TRIG_LINE_COUNT:31", "PST_TRIG_LINE_CNT:31",
        /////                                                    "TIME_COUNT:50", "TRIG_LINE:31" };
        ///// <returns></returns>
        //private bool GetAUXTBStatusVariables(List<long> fldValues, DP14MSTAuxParameters parameters)
        //{
        //    bool status = true;
        //    long pValue = 0x00;

        //    try
        //    {
        //        for (int i = 1; i < m_AUXStatusFields.Length; i++)
        //        {
        //            pValue = fldValues[i];
        //            switch (i)
        //            {
        //                case 0:
        //                    break;  // Pad:0
        //                case 1:
        //                    parameters.AW4TIN = (byte)pValue;
        //                    break;
        //                case 2:
        //                    break;  // Pad:0
        //                case 3:
        //                    parameters.TBFIN = (byte)pValue;
        //                    break;
        //                case 4:
        //                    parameters.TRIG = (byte)pValue;
        //                    break;
        //                case 5:
        //                    parameters.WRAP = (byte)pValue;
        //                    break;
        //                case 6:
        //                    break;  // Pad:0
        //                case 7:
        //                    parameters.AWI2NDLVL = (byte)pValue;
        //                    break;
        //                case 8:
        //                    parameters.PRE_TRIG_LINE_COUNT = pValue;
        //                    break;
        //                case 9:
        //                    parameters.PST_TRIG_LINE_CNT = pValue;
        //                    break;
        //                case 10:
        //                    parameters.TIME_COUNT = pValue;
        //                    break;
        //                case 11:
        //                    parameters.TRIG_LINE = pValue;
        //                    break;

        //                default:
        //                    status = false;
        //                    break;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error getting TB paramter value: Invalid Value Encountered");
        //    }

        //    return status;
        //}


        ///// <summary>
        ///// Determine the start/stop indices and other pertinent parameters assoicated with the captured data
        ///// </summary>
        ///// <returns></returns>
        //public bool createTBDataStatistics(long memoryDepth)
        //{
        //    bool status = true;
        //    byte[] loopData = new byte[2 + TB_STATUS_LOOP_BYTE_LEN + 2];
        //    List<long> fldValues = new List<long>();

        //    m_fldExtractor = new FieldExtractor();
        //    m_TBParameters = new DP14MSTTBParameters();
        //    initFieldWidthsArray(ref m_TBStatusFields, ref m_TBStatusFieldWidths);
        //    m_fldExtractor.LoopFieldWidths = m_TBStatusFieldWidths;

        //    // get the TB status data loop 
        //    status = getTBStatusLoopData(ref loopData);
        //    if (status)
        //    {
        //        // decode the loop fields
        //        status = m_fldExtractor.(m_TBStatusFieldWidths, loopData, fldValues, 2, loopData.Length - 3);  //startIndex, endIndex);
        //    }

        //    // stuff the fldValue list items into the variables to be used in getting the TB start/end values
        //    if (status)
        //        status = GetTBStatusVariables(fldValues, m_TBParameters);

        //    if (status)
        //    {
        //        // invoke the I/F object to calcualte the TB (row) start and end parameters i.e. TBParameters
        //        m_FTD2xxIF.CalculateTBDataStats(m_TBParameters.TRIG, m_TBParameters.TBFIN, m_TBParameters.WRAP,
        //                            m_TBParameters.PRE_TRIG_LINE_COUNT, m_TBParameters.PST_TRIG_LINE_CNT,
        //                            m_TBParameters.TIME_COUNT, m_TBParameters.TRIG_LINE, memoryDepth);
        //    }

        //    return status;
        //}


        ///// <summary>
        ///// Determine the start/stop indices and other pertinent parameters assoicated with the captured data
        ///// </summary>
        ///// <returns></returns>
        //public bool createAuxTBDataStatistics(long memoryDepth)
        //{
        //    bool status = true;
        //    byte[] loopData = new byte[2 + AUX_STATUS_LOOP_BYTE_LEN + 2];
        //    List<long> fldValues = new List<long>();

        //    m_fldExtractor = new FieldExtractor();
        //    m_AUXParameters = new DP14MSTAuxParameters();
        //    initFieldWidthsArray(ref m_AUXStatusFields, ref m_AUXStatusFieldWidths);
        //    m_fldExtractor.LoopFieldWidths = m_AUXStatusFieldWidths;

        //    // get the TB status data loop 
        //    status = getAuxTBStatusLoopData(ref loopData);
        //    if (status)
        //    {
        //        // decode the loop fields
        //        status = m_fldExtractor.GetloopFields(m_AUXStatusFieldWidths, loopData, fldValues, 2, loopData.Length - 3);
        //    }

        //    // stuff the fldValue list items into the variables to be used in getting the TB start/end values
        //    if (status)
        //        status = GetAUXTBStatusVariables(fldValues, m_AUXParameters);


        //    if (status)
        //    {
        //        // invoke the I/F object to calcualte the TB (row) start and end parameters i.e. TBParameters
        //        m_FTD2xxIF.CalculateAuxDataStats(m_AUXParameters.TRIG, m_AUXParameters.TBFIN, m_AUXParameters.WRAP,
        //                            m_AUXParameters.PRE_TRIG_LINE_COUNT, m_AUXParameters.PST_TRIG_LINE_CNT,
        //                            m_AUXParameters.TIME_COUNT, m_AUXParameters.TRIG_LINE, memoryDepth);
        //    }

        //    return status;
        //}


        ///// <summary>
        ///// Create the FS4500 folder if necessary.  Remove all files is 
        ///// folder exists.
        ///// </summary>
        //private void clearDataUploadFolder_Aux()
        //{
        //    // clear the folder of all files...
        //    DirectoryInfo di = new DirectoryInfo(m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME);
        //    foreach (FileInfo file in di.GetFiles())
        //    {
        //        if (file.Name.StartsWith("AuxTraceData"))
        //            file.Delete();
        //    }
        //}


        ///// <summary>
        ///// Create the FS4500 folder if necessary.  Remove all files is 
        ///// folder exists.
        ///// </summary>
        //private void clearDataUploadFolder()
        //{
        //    // clear the folder of all files...
        //    DirectoryInfo di = new DirectoryInfo(m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME);
        //    foreach (FileInfo file in di.GetFiles())
        //    {
        //        if (file.Name.StartsWith("TraceData"))
        //            file.Delete();
        //    }
        //}


        ///// <summary>
        ///// get a page of trace buffer data within the context of a TPI task.
        ///// </summary>
        ///// <param name="stateOffset"></param>
        ///// <param name="pageData"></param>
        ///// <param name="mutex"></param>
        ///// <returns></returns>
        //private bool getStateData_MultiThread(long stateOffset, ref byte[] pageData, Mutex mutex)
        //{
        //    bool status = true;
        //    bool lockAcquired = false;

        //    try
        //    {
        //        //System.Diagnostics.Debug.WriteLine("Requesting mutex    stateOffset = " + stateOffset.ToString());
        //        lockAcquired = mutex.WaitOne();
        //        if (lockAcquired)
        //        {
        //            //System.Diagnostics.Debug.WriteLine("Acquired mutex    stateOffset = " + stateOffset.ToString());

        //            // get data from the HW
        //            status = m_FTD2xxIF.GetTraceBufferPage(stateOffset, ref pageData);
        //        }
        //    }
        //    finally
        //    {
        //        if (lockAcquired)
        //        {
        //            mutex.ReleaseMutex();
        //            //System.Diagnostics.Debug.WriteLine("Released mutex (" + Thread.CurrentThread.ManagedThreadId.ToString() + ")");
        //        }
        //    }

        //    return status;
        //}


        ///// <summary>
        ///// Get the block uploaded (from the HW) interval at which reporting of the upload percentage occurs/
        ///// </summary>
        ///// <returns></returns>
        //private int getUploadReportInterval()
        //{
        //    int uploadEventInterval = 1;

        //    if (m_numOfTPITasks == 1)
        //        uploadEventInterval = 1;
        //    else if (m_numOfTPITasks <= 8)
        //        uploadEventInterval = 8;
        //    else // m_numOfTPITask <= 16)
        //        uploadEventInterval = 32;

        //    return uploadEventInterval;
        //}


        //private void writeBlockToBinaryFile(string path, byte[] pageData, int count)
        //{
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        using (BinaryWriter bw = new BinaryWriter(ms))
        //        {
        //            bw.Write(pageData, 0, count); // (int)blockSize * TB_STATE_BYTE_LEN);
        //            using (FileStream file = new FileStream(path, FileMode.Append, FileAccess.Write))
        //            {
        //                ms.WriteTo(file);
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// One of several threads each uploading a segment of the trace buffer data and storing
        /// the data in a file on disk. Each thread will be invoking this method repeadily
        /// </summary>
        /// <param name="path"></param>
        /// <param name="startState"></param>
        /// <param name="endState"></param>
        /// <param name="memoryDepth"></param>
        /// <param name="FTD_IF"></param>
        /// <param name="mutex"></param>
        /// 
        /// NOTE:  the method converts from a circular buffer format to a sequencial set of blocks!
        ///        StartStateIndex arg represent the starting state index in the circular buffer
        ///        BlockSize and BlockStartState represent the sequencial trace data, starts at index 0
        /// 
        //private void createTraceBufferDataFileII(string path, long triggerOffset, long startStateIndex, long blockStartState,
        //                                        long blockSize, long memoryDepth, long numOfCapturedStates,
        //                                        FT60x_IF FTD_IF, Mutex mutex, CancellationToken token, HWDataUploadStatusEvent uploadStatusEvent)
        //{
        //    bool status = true;
        //    byte[] pageData = new byte[TB_PAGE_BYTE_LENGTH];
        //    long stateOffset = startStateIndex;
        //    long numOfStatesUploaded = 0;
        //    long curStateIndex = -1;
        //    long partialPageSize = 0;
        //    int headerLength = 0;
        //    int stateCount = 0x00;
        //    int blockUploadCount = 0;
        //    int uploadEventInterval = getUploadReportInterval();
        //    int retry = 0;

        //    //
        //    // NOTE:  BlockStateStart is NOT dependant of the circular buffer... 
        //    //        it is the number of states divided into (16) equal block lengths...
        //    //


        //    // each block starts at a different location relative to the start state of the circular buffer.
        //    // multiple blocks may start AFTER the circular buffer has wrapped...
        //    if ((startStateIndex + blockStartState) < memoryDepth)
        //        curStateIndex = startStateIndex + blockStartState; // +1;
        //    else
        //        curStateIndex = ((startStateIndex + blockStartState) - memoryDepth); // + 1;


        //    //
        //    // fill the file with meta data, followed by the state data.
        //    //
        //    //using (BinaryWriter bw = new BinaryWriter(File.Open(path, FileMode.CreateNew, FileAccess.Write, FileShare.None)))
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        using (BinaryWriter bw = new BinaryWriter(ms))
        //        {
        //            if (m_generateMetaData)
        //            {
        //                byte[] array = System.Text.Encoding.ASCII.GetBytes("StartState:" + blockStartState.ToString() + "\n");
        //                bw.Write(array);
        //                headerLength += array.Length;

        //                array = System.Text.Encoding.ASCII.GetBytes("EndState:" + ((blockStartState + blockSize) - 1).ToString() + "\n");
        //                bw.Write(array);
        //                headerLength += array.Length;

        //                array = System.Text.Encoding.ASCII.GetBytes("TrigOffset:" + triggerOffset.ToString() + "\n");
        //                bw.Write(array);
        //                headerLength += array.Length;

        //                array = System.Text.Encoding.ASCII.GetBytes("EndHdr");
        //                bw.Write(array);
        //                headerLength += array.Length;


        //                string delimiterStr = "";
        //                int padCount = headerLength;
        //                while ((padCount % 16) != 0)
        //                    padCount += 1;

        //                if ((padCount - headerLength - 1) > 0)
        //                    array = System.Text.Encoding.ASCII.GetBytes(delimiterStr.PadLeft(padCount - headerLength - 1, '*'));
        //                else
        //                    array = System.Text.Encoding.ASCII.GetBytes(delimiterStr.PadLeft(0, '*'));

        //                bw.Write(array);
        //                bw.Write('\n');
        //            }

        //            using (FileStream file = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
        //            {
        //                ms.WriteTo(file);
        //            }
        //        }
        //    }



        //    if (blockSize < TB_STATES_PER_PAGE)
        //    {
        //        // At best, a single block is all that is needed
        //        // at worst, two will be needed, both are partial block...

        //        Array.Clear(pageData, 0, pageData.Length);
        //        if ((curStateIndex + blockSize) <= memoryDepth)
        //        {
        //            while (!token.IsCancellationRequested && (retry <= 10))
        //            {
        //                // blocksize is contained in a single block.
        //                status = getStateData_MultiThread(curStateIndex, ref pageData, mutex);
        //                if (status && !token.IsCancellationRequested)
        //                {
        //                    retry = 0;
        //                    if ((curStateIndex + TB_STATES_PER_PAGE) > blockSize)
        //                    {
        //                        writeBlockToBinaryFile(path, pageData, (int)blockSize * TB_STATE_BYTE_LEN);
        //                        break;  // while (... (retry <= 10))
        //                    }
        //                }
        //                else if (!token.IsCancellationRequested && (retry < 10))
        //                {
        //                    retry += 1;
        //                    Thread.Sleep(100);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            while (!token.IsCancellationRequested && (retry <= 10))
        //            {
        //                // need two partial blocks, the partial one abutting against the memory depth limit 
        //                // and the one at the beginning of the circular buffer (e.g. state index 0)...
        //                status = getStateData_MultiThread(curStateIndex, ref pageData, mutex);
        //                if (status)
        //                {
        //                    retry = 0;
        //                    writeBlockToBinaryFile(path, pageData, (int)(memoryDepth - curStateIndex) * TB_STATE_BYTE_LEN);
        //                    int numOfRemainingStates = (int)(blockSize - (memoryDepth - curStateIndex));
        //                    curStateIndex = 0;

        //                    while (!token.IsCancellationRequested && (retry <= 10))
        //                    {
        //                        // get the 2nd partial buffer 
        //                        status = getStateData_MultiThread(curStateIndex, ref pageData, mutex);
        //                        if (status)
        //                        {
        //                            writeBlockToBinaryFile(path, pageData, numOfRemainingStates * TB_STATE_BYTE_LEN);
        //                            break;  // ...(retry <= 10))
        //                        }
        //                        else if (retry < 10)
        //                        {
        //                            retry += 1;
        //                            Thread.Sleep(100);
        //                        }
        //                    }
        //                }
        //                else if (retry < 10)
        //                {
        //                    retry += 1;
        //                    Thread.Sleep(100);
        //                }

        //                if (status || (retry >= 10))
        //                    break;  // ... out while loop
        //            }
        //        }
        //    }
        //    else
        //    {
        //        //
        //        // each file will hold more than 1 page of data... 4096 states
        //        //
        //        while (status && (numOfStatesUploaded < blockSize) && !token.IsCancellationRequested && (retry < 10))
        //        {
        //            Array.Clear(pageData, 0, pageData.Length);
        //            if ((curStateIndex + TB_STATES_PER_PAGE) <= memoryDepth)
        //            {
        //                status = getStateData_MultiThread(curStateIndex, ref pageData, mutex);
        //                if (status && !token.IsCancellationRequested)
        //                {
        //                    retry = 0;
        //                    if ((numOfStatesUploaded + TB_STATES_PER_PAGE) < blockSize)
        //                    {
        //                        writeBlockToBinaryFile(path, pageData, pageData.Length);
        //                        numOfStatesUploaded += TB_STATES_PER_PAGE;
        //                        curStateIndex += TB_STATES_PER_PAGE;
        //                        stateCount += TB_STATES_PER_PAGE;
        //                    }
        //                    else
        //                    {
        //                        partialPageSize = blockSize - numOfStatesUploaded;
        //                        writeBlockToBinaryFile(path, pageData, (int)(partialPageSize * TB_STATE_BYTE_LEN));

        //                        numOfStatesUploaded += partialPageSize;
        //                        curStateIndex += partialPageSize;
        //                        stateCount += (int)partialPageSize;
        //                    }
        //                }
        //                else if (!token.IsCancellationRequested)
        //                {
        //                    retry += 1;
        //                    status = true;
        //                    Thread.Sleep(100);
        //                }
        //            }
        //            else
        //            {
        //                partialPageSize = memoryDepth - curStateIndex;
        //                status = getStateData_MultiThread(curStateIndex, ref pageData, mutex);
        //                if (status && !token.IsCancellationRequested)
        //                {
        //                    retry = 0;
        //                    if (numOfStatesUploaded + partialPageSize < blockSize)
        //                    {
        //                        writeBlockToBinaryFile(path, pageData, (int)(partialPageSize * TB_STATE_BYTE_LEN));
        //                        numOfStatesUploaded += partialPageSize;
        //                        curStateIndex = 0;
        //                    }
        //                    else
        //                    {
        //                        // partial page has enough states to complete the block...
        //                        partialPageSize = blockSize - numOfStatesUploaded;
        //                        writeBlockToBinaryFile(path, pageData, (int)(partialPageSize * TB_STATE_BYTE_LEN));

        //                        curStateIndex += partialPageSize * TB_STATE_BYTE_LEN;
        //                        numOfStatesUploaded += partialPageSize;
        //                        curStateIndex = 0;
        //                    }
        //                }
        //                else if (!token.IsCancellationRequested)
        //                {
        //                    retry += 1;
        //                    status = true;
        //                    Thread.Sleep(100);
        //                }
        //            }

        //            if (!token.IsCancellationRequested)
        //            {
        //                blockUploadCount += 1;
        //                if (blockUploadCount >= uploadEventInterval)
        //                {
        //                    // raise an event to get the progress bar updated...
        //                    if (uploadStatusEvent != null)
        //                        uploadStatusEvent(this, new HWDataUploadStatusEventArgs("Upload Percentage", true, stateCount, (int)numOfCapturedStates));

        //                    stateCount = 0;
        //                    blockUploadCount = 0;
        //                }
        //            }
        //        }
        //    }

        //    if (!token.IsCancellationRequested)
        //    {
        //        try
        //        {
        //            //bw.Flush();
        //            //// now write the memoryStream to a file...
        //            //using (FileStream file = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
        //            //{
        //            //    ms.WriteTo(file);
        //            //}

        //            // update the trigger file name if it's appropriate.
        //            if ((triggerOffset > blockStartState) && (triggerOffset <= (blockStartState + blockSize)))
        //                m_triggerBinaryFileName = path;
        //        }
        //        catch (Exception ex)
        //        {
        //            if (uploadStatusEvent != null)
        //                uploadStatusEvent(this, new HWDataUploadStatusEventArgs("Writing " + path + " failed", true, 0, 0));
        //        }
        //        //} // BinaryWriter bw
        //        //}

        //        //   }  // using (MemoryStream ms 

        //        //// for trouble shooting
        //        //if (uploadStatusEvent != null)
        //        //    uploadStatusEvent(this, new HWDataUploadStatusEventArgs(path + " Closed", true, stateCount, (int)numOfCapturedStates)); 
        //    }
        //}


        ///// <summary>
        ///// One of several threads each uploading a segment of the trace buffer data and storing
        ///// the data in a file on disk. Each thread will be invoking this method repeadily
        ///// </summary>
        ///// <param name="path"></param>
        ///// <param name="startState"></param>
        ///// <param name="endState"></param>
        ///// <param name="memoryDepth"></param>
        ///// <param name="FTD_IF"></param>
        ///// <param name="mutex"></param>
        ///// 
        ///// NOTE:  the method converts from a circular buffer format to a sequencial set of blocks!
        /////        StartStateIndex arg represent the starting state index in the circular buffer
        /////        BlockSize and BlockStartState represent the sequencial trace data, starts at index 0
        ///// 
        //private void createTraceBufferDataFileII(string path, long triggerOffset, long startStateIndex, long blockStartState,
        //                                        long blockSize, long memoryDepth, long numOfCapturedStates,
        //                                        FT60x_IF FTD_IF, Mutex mutex, CancellationToken token, HWDataUploadStatusEvent uploadStatusEvent)
        //{
        //    bool status = true;
        //    byte[] pageData = new byte[TB_PAGE_BYTE_LENGTH];
        //    long stateOffset = startStateIndex;
        //    long numOfStatesUploaded = 0;
        //    long curStateIndex = -1;
        //    long partialPageSize = 0;
        //    int headerLength = 0;
        //    int stateCount = 0x00;
        //    int blockUploadCount = 0;
        //    int uploadEventInterval = getUploadReportInterval();
        //    int retry = 0;

        //    //
        //    // NOTE:  BlockStateStart is NOT dependant of the circular buffer... 
        //    //        it is the number of states divided into (16) equal block lengths...
        //    //


        //    // each block starts at a different location relative to the start state of the circular buffer.
        //    // multiple blocks may start AFTER the circular buffer has wrapped...
        //    if ((startStateIndex + blockStartState) < memoryDepth)
        //        curStateIndex = startStateIndex + blockStartState; // +1;
        //    else
        //        curStateIndex = ((startStateIndex + blockStartState) - memoryDepth); // + 1;


        //    //
        //    // fill the file with meta data, followed by the state data.
        //    //
        //    //using (BinaryWriter bw = new BinaryWriter(File.Open(path, FileMode.CreateNew, FileAccess.Write, FileShare.None)))
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        using (BinaryWriter bw = new BinaryWriter(ms))
        //        {
        //            using(FileStream file = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
        //            {
        //                if (m_generateMetaData)
        //                {
        //                    byte[] array = System.Text.Encoding.ASCII.GetBytes("StartState:" + blockStartState.ToString() + "\n");
        //                    bw.Write(array);
        //                    headerLength += array.Length;

        //                    array = System.Text.Encoding.ASCII.GetBytes("EndState:" + ((blockStartState + blockSize) - 1).ToString() + "\n");
        //                    bw.Write(array);
        //                    headerLength += array.Length;

        //                    array = System.Text.Encoding.ASCII.GetBytes("TrigOffset:" + triggerOffset.ToString() + "\n");
        //                    bw.Write(array);
        //                    headerLength += array.Length;

        //                    array = System.Text.Encoding.ASCII.GetBytes("EndHdr");
        //                    bw.Write(array);
        //                    headerLength += array.Length;


        //                    string delimiterStr = "";
        //                    int padCount = headerLength;
        //                    while ((padCount % 16) != 0)
        //                        padCount += 1;

        //                    if ((padCount - headerLength - 1) > 0)
        //                        array = System.Text.Encoding.ASCII.GetBytes(delimiterStr.PadLeft(padCount - headerLength - 1, '*'));
        //                    else
        //                        array = System.Text.Encoding.ASCII.GetBytes(delimiterStr.PadLeft(0, '*'));

        //                    bw.Write(array);
        //                    bw.Write('\n');

        //                    //ms.WriteTo(file);
        //                }

        //                if (blockSize < TB_STATES_PER_PAGE)
        //                {
        //                    // At best, a single block is all that is needed
        //                    // at worst, two will be needed, both are partial block...

        //                    //Array.Clear(pageData, 0, pageData.Length);
        //                    if ((curStateIndex + blockSize) <= memoryDepth)
        //                    {
        //                        while (!token.IsCancellationRequested && (retry <= 10))
        //                        {
        //                            // blocksize is contained in a single block.
        //                            status = getStateData_MultiThread(curStateIndex, ref pageData, mutex);
        //                            if (status && !token.IsCancellationRequested)
        //                            {
        //                                retry = 0;
        //                                if ((curStateIndex + TB_STATES_PER_PAGE) > blockSize)
        //                                {
        //                                    //writeBlockToBinaryFile(path,  pageData, (int)blockSize * TB_STATE_BYTE_LEN);
        //                                    bw.Write(pageData, 0, (int)blockSize * TB_STATE_BYTE_LEN);
        //                                    //ms.WriteTo(file);
        //                                    break;  // while (... (retry <= 10))
        //                                }
        //                            }
        //                            else if (!token.IsCancellationRequested && (retry < 10))
        //                            {
        //                                retry += 1;
        //                                Thread.Sleep(100);
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        while (!token.IsCancellationRequested && (retry <= 10))
        //                        {
        //                            // need two partial blocks, the partial one abutting against the memory depth limit 
        //                            // and the one at the beginning of the circular buffer (e.g. state index 0)...
        //                            status = getStateData_MultiThread(curStateIndex, ref pageData, mutex);
        //                            if (status)
        //                            {
        //                                retry = 0;
        //                                //writeBlockToBinaryFile(path, pageData, (int)(memoryDepth - curStateIndex) * TB_STATE_BYTE_LEN);
        //                                bw.Write(pageData, 0, (int)(memoryDepth - curStateIndex) * TB_STATE_BYTE_LEN);
        //                                int numOfRemainingStates = (int)(blockSize - (memoryDepth - curStateIndex));
        //                                curStateIndex = 0;

        //                                while (!token.IsCancellationRequested && (retry <= 10))
        //                                {
        //                                    // get the 2nd partial buffer 
        //                                    status = getStateData_MultiThread(curStateIndex, ref pageData, mutex);
        //                                    if (status)
        //                                    {
        //                                        //writeBlockToBinaryFile(path, pageData, numOfRemainingStates * TB_STATE_BYTE_LEN);
        //                                        bw.Write(pageData, 0, numOfRemainingStates * TB_STATE_BYTE_LEN);
        //                                        //ms.WriteTo(file);
        //                                        break;  // ...(retry <= 10))
        //                                    }
        //                                    else if (retry < 10)
        //                                    {
        //                                        retry += 1;
        //                                        Thread.Sleep(100);
        //                                    }
        //                                }
        //                            }
        //                            else if (retry < 10)
        //                            {
        //                                retry += 1;
        //                                Thread.Sleep(100);
        //                            }

        //                            if (status || (retry >= 10))
        //                                break;  // ... out while loop
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    //
        //                    // each file will hold more than 1 page of data... 4096 states
        //                    //
        //                    while (status && (numOfStatesUploaded < blockSize) && !token.IsCancellationRequested && (retry < 10))
        //                    {
        //                        //Array.Clear(pageData, 0, pageData.Length);
        //                        if ((curStateIndex + TB_STATES_PER_PAGE) <= memoryDepth)
        //                        {
        //                            status = getStateData_MultiThread(curStateIndex, ref pageData, mutex);
        //                            if (status && !token.IsCancellationRequested)
        //                            {
        //                                retry = 0;
        //                                if ((numOfStatesUploaded + TB_STATES_PER_PAGE) < blockSize)
        //                                {
        //                                    //writeBlockToBinaryFile(path, pageData, pageData.Length);
        //                                    bw.Write(pageData, 0, pageData.Length);
        //                                    //ms.WriteTo(file);

        //                                    numOfStatesUploaded += TB_STATES_PER_PAGE;
        //                                    curStateIndex += TB_STATES_PER_PAGE;
        //                                    stateCount += TB_STATES_PER_PAGE;
        //                                }
        //                                else
        //                                {
        //                                    partialPageSize = blockSize - numOfStatesUploaded;
        //                                    //writeBlockToBinaryFile(path, pageData, (int)(partialPageSize * TB_STATE_BYTE_LEN));
        //                                    bw.Write(pageData, 0, (int)(partialPageSize * TB_STATE_BYTE_LEN));
        //                                    //ms.WriteTo(file);

        //                                    numOfStatesUploaded += partialPageSize;
        //                                    curStateIndex += partialPageSize;
        //                                    stateCount += (int)partialPageSize;
        //                                }
        //                            }
        //                            else if (!token.IsCancellationRequested)
        //                            {
        //                                retry += 1;
        //                                status = true;
        //                                Thread.Sleep(100);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            partialPageSize = memoryDepth - curStateIndex;
        //                            status = getStateData_MultiThread(curStateIndex, ref pageData, mutex);
        //                            if (status && !token.IsCancellationRequested)
        //                            {
        //                                retry = 0;
        //                                if (numOfStatesUploaded + partialPageSize < blockSize)
        //                                {
        //                                    //writeBlockToBinaryFile(path, pageData, (int)(partialPageSize * TB_STATE_BYTE_LEN));
        //                                    bw.Write(pageData, 0, (int)(partialPageSize * TB_STATE_BYTE_LEN));
        //                                    //ms.WriteTo(file);

        //                                    numOfStatesUploaded += partialPageSize;
        //                                    curStateIndex = 0;
        //                                }
        //                                else
        //                                {
        //                                    // partial page has enough states to complete the block...
        //                                    partialPageSize = blockSize - numOfStatesUploaded;
        //                                    //writeBlockToBinaryFile(path, pageData, (int)(partialPageSize * TB_STATE_BYTE_LEN));
        //                                    bw.Write(pageData, 0, (int)(partialPageSize * TB_STATE_BYTE_LEN));
        //                                    //ms.WriteTo(file);

        //                                    curStateIndex += partialPageSize * TB_STATE_BYTE_LEN;
        //                                    numOfStatesUploaded += partialPageSize;
        //                                    curStateIndex = 0;
        //                                }
        //                            }
        //                            else if (!token.IsCancellationRequested)
        //                            {
        //                                retry += 1;
        //                                status = true;
        //                                Thread.Sleep(100);
        //                            }
        //                        }

        //                        if (!token.IsCancellationRequested)
        //                        {
        //                            blockUploadCount += 1;
        //                            if (blockUploadCount >= uploadEventInterval)
        //                            {
        //                                // raise an event to get the progress bar updated...
        //                                if (uploadStatusEvent != null)
        //                                    uploadStatusEvent(this, new HWDataUploadStatusEventArgs("Upload Percentage", true, stateCount, (int)numOfCapturedStates));

        //                                stateCount = 0;
        //                                blockUploadCount = 0;
        //                            }
        //                        }
        //                    }
        //                }

        //                // write the entire memory stream to the file!
        //                ms.WriteTo(file);
        //            } // using (FileStream...)
        //        } //using (BinaryWriter ...
        //    } // using (MemoryStream)

        //    if (!token.IsCancellationRequested)
        //    {
        //        try
        //        {
        //            // update the trigger file name if it's appropriate.
        //            if ((triggerOffset > blockStartState) && (triggerOffset <= (blockStartState + blockSize)))
        //                m_triggerBinaryFileName = path;
        //        }
        //        catch (Exception ex)
        //        {
        //            if (uploadStatusEvent != null)
        //                uploadStatusEvent(this, new HWDataUploadStatusEventArgs("Writing " + path + " failed", true, 0, 0));
        //        }


        //        //// for trouble shooting
        //        //if (uploadStatusEvent != null)
        //        //    uploadStatusEvent(this, new HWDataUploadStatusEventArgs(path + " Closed", true, stateCount, (int)numOfCapturedStates)); 
        //    }
        //}


        ///// <summary>
        ///// Generate an array of start state indices that are all values that are multiples of 8.
        ///// </summary>
        ///// <returns></returns>
        //private long[] getfileStartIndices(int numOfThreads, long numOfStates)
        //{
        //    long[] indices = new long[numOfThreads];
        //    long fileSize = (numOfStates / numOfThreads);

        //    // initialize the indices
        //    for (int i = 0; i < numOfThreads; i++)
        //        indices[i] = i * fileSize;

        //    // round each index to be a multiple of 8 -- because the HW ignores the 3 LSB address bits.
        //    for (int i = 0; i < indices.Length; i++)
        //    {
        //        if ((indices[i] % 8) != 0)
        //        {
        //            for (long temp = indices[i]; temp > indices[i] - 8; temp--)
        //            {
        //                if ((temp % 8) == 0)
        //                {
        //                    indices[i] = temp;
        //                    break;
        //                }
        //            }
        //        }
        //    }


        //    return indices;
        //}


        ///// <summary>
        ///// Initialize a background worker thread to search single Threaded listings.
        ///// </summary>
        ///// <param name="threadParameters"></param>
        //private void initSearchBGWorker_SingleThreaded(DP14MSTTBSearchDescriptor threadParameters)
        //{
        //    if (m_searchWorker_SingleThread != null)
        //    {
        //        m_worker.Dispose();
        //        m_worker = null;
        //    }

        //    m_searchWorker_SingleThread = new BackgroundWorker
        //    {
        //        WorkerReportsProgress = true,
        //        WorkerSupportsCancellation = true
        //    };

        //    m_searchWorker_SingleThread.DoWork += DoWork_TBSearchSingleThread;
        //    m_searchWorker_SingleThread.ProgressChanged += ProgressChanged_TBSearchSingleThread;
        //    m_searchWorker_SingleThread.RunWorkerCompleted += RunWorkerCompleted_TBSearchSingleThread;

        //    // start the task running
        //    m_searchWorker_SingleThread.RunWorkerAsync(threadParameters);
        //}


        ///// <summary>
        ///// Initialize a background worker thread to search single Threaded listings.
        ///// </summary>
        ///// <param name="threadParameters"></param>
        //private void initSearchBGWorker_MultiThreaded(DP14MSTTBSearchDescriptor threadParameters)
        //{
        //    if (m_searchWorker_MultiThread != null)
        //    {
        //        m_searchWorker_MultiThread.Dispose();
        //        m_searchWorker_MultiThread = null;
        //    }

        //    m_searchWorker_MultiThread = new BackgroundWorker
        //    {
        //        WorkerReportsProgress = true,
        //        WorkerSupportsCancellation = true
        //    };

        //    m_searchWorker_MultiThread.DoWork += DoWork_TBSearchMultiThread;
        //    m_searchWorker_MultiThread.ProgressChanged += ProgressChanged_TBSearchMultiThread;
        //    m_searchWorker_MultiThread.RunWorkerCompleted += RunWorkerCompleted_TBSearchMultiThread;

        //    // start the task running
        //    m_searchWorker_MultiThread.RunWorkerAsync(threadParameters);
        //}


        ///// <summary>
        ///// Background worker thread used to update the 8B10B error counts at regular intervals.
        ///// </summary>
        ///// http://stackoverflow.com/questions/11500563/winform-multithreading-use-backgroundworker-or-not
        ///// 
        //private void initAuxLinkBGWorker(DP14MSTTBUploadDescriptor threadParameters)
        //{
        //    //if (m_auxWorker != null)
        //    //{
        //    //    m_auxWorker.Dispose();
        //    //    m_auxWorker = null;
        //    //}

        //    //m_auxWorker = new BackgroundWorker
        //    //{
        //    //    WorkerReportsProgress = true,
        //    //    WorkerSupportsCancellation = true
        //    //};

        //    //m_auxWorker.DoWork += DoWork_AuxUpload;
        //    //m_auxWorker.ProgressChanged += ProgressChanged_AuxUpload;
        //    //m_auxWorker.RunWorkerCompleted += RunWorkerCompleted_AuxUpload;

        //    //// start the task running
        //    //m_auxWorker.RunWorkerAsync(threadParameters);
        //}


        ///// <summary>
        ///// Upload the trace buffer data using a single background worker thread
        ///// </summary>
        ///// <param name="threadParameters"></param>
        //private void uploadData_BGWorkerMode(DP14MSTTBUploadDescriptor threadParameters, bool isMainLinkData = true)
        //{
        //    ////// raise an event to the parent form
        //    ////if (TBDataUploadStatusEvent != null)
        //    ////    TBDataUploadStatusEvent(this, new HWDataUploadStatusEventArgs("Data Upload Started", true));

        //    //initAuxLinkBGWorker(threadParameters);
        //}


        ///// <summary>
        ///// Wait for all uploading TPI Task to finish or exit.
        ///// </summary>
        ///// <param name="tasks"></param>
        ///// <param name="continuationTasks"></param>
        ///// <returns></returns>
        //private bool waitForCascadingTasksToComplete(Task<DP14MSTTaskStateObject>[] tasks,
        //                                                Task<DP14MSTTaskStateObject>[][] continuationTasks)
        //{
        //    bool waitToExit = true;
        //    int exitAttempts = 0;
        //    int numOfLevels = 0;

        //    numOfLevels = continuationTasks[0].Length + 1;

        //    while (waitToExit && (exitAttempts < 20))
        //    {
        //        waitToExit = false;
        //        for (int outterLoop = 0; outterLoop < numOfLevels; outterLoop++)
        //        {
        //            Task[] tasksRef = null;
        //            switch (outterLoop)
        //            {
        //                case 0:
        //                    tasksRef = tasks;
        //                    break;
        //                case 1:
        //                    tasksRef = continuationTasks[1];
        //                    break;
        //                case 2:
        //                    tasksRef = continuationTasks[2];
        //                    break;
        //                case 3:
        //                    tasksRef = continuationTasks[3];
        //                    break;
        //            }

        //            for (int i = 0; i < tasksRef.Length; i++)
        //            {
        //                if ((tasksRef[i].Status != TaskStatus.RanToCompletion) && (tasksRef[i].Status != TaskStatus.Canceled) && (tasksRef[i].Status != TaskStatus.WaitingForActivation))
        //                {
        //                    waitToExit = true;
        //                    break;
        //                }
        //                else
        //                {
        //                    waitToExit = false;
        //                }
        //            }
        //        }

        //        if (waitToExit)
        //            Thread.Sleep(200); //waitToExit = false;
        //        else
        //            exitAttempts += 1;
        //    }

        //    return true;
        //}


        ///// <summary>
        ///// Wait for all tasks (in all four levels) to stop processing
        ///// </summary>
        ///// <param name="tasks"></param>
        ///// <param name="continuationTasks"></param>
        ///// <param name="continuationTasks2"></param>
        ///// <param name="continuationTasks3"></param>
        ///// <param name="numOfLevels"></param>
        ///// <param name="numOfTaskPerGroup"></param>
        ///// <returns></returns>
        //private bool waitForCascadingTasksToComplete(Task<DP14MSTTaskStateObject>[] tasks,
        //                                                Task<DP14MSTTaskStateObject>[] continuationTasks,
        //                                                Task<DP14MSTTaskStateObject>[] continuationTasks2,
        //                                                Task<DP14MSTTaskStateObject>[] continuationTasks3,
        //                                                int numOfLevels = 4,
        //                                                int numOfTaskPerGroup = 4)
        //{
        //    bool waitToExit = true;
        //    int exitAttempts = 0;

        //    while (waitToExit && (exitAttempts < 20))
        //    {
        //        waitToExit = false;
        //        for (int outterLoop = 0; outterLoop < numOfLevels; outterLoop++)
        //        {
        //            Task[] tasksRef = null;
        //            switch (outterLoop)
        //            {
        //                case 0:
        //                    tasksRef = tasks;
        //                    break;
        //                case 1:
        //                    tasksRef = continuationTasks;
        //                    break;
        //                case 2:
        //                    tasksRef = continuationTasks2;
        //                    break;
        //                case 3:
        //                    tasksRef = continuationTasks3;
        //                    break;
        //            }

        //            for (int i = 0; i < tasksRef.Length; i++)
        //            {
        //                if ((tasksRef[i].Status != TaskStatus.RanToCompletion) && (tasksRef[i].Status != TaskStatus.Canceled) && (tasksRef[i].Status != TaskStatus.WaitingForActivation))
        //                {
        //                    waitToExit = true;
        //                    break;
        //                }
        //                else
        //                {
        //                    waitToExit = false;
        //                }
        //            }
        //        }

        //        if (waitToExit)
        //            Thread.Sleep(200); //waitToExit = false;
        //        else
        //            exitAttempts += 1;
        //    }

        //    return true;
        //}


        ///// <summary>
        ///// Wait for all tasks (in all four levels) to stop processing
        ///// </summary>
        ///// <param name="tasks"></param>
        ///// <param name="continuationTasks"></param>
        ///// <param name="continuationTasks2"></param>
        ///// <param name="continuationTasks3"></param>
        ///// <param name="numOfLevels"></param>
        ///// <param name="numOfTaskPerGroup"></param>
        ///// <returns></returns>
        //private bool waitForCascadingTasksToComplete(Task<DP14MSTTaskStateObject>[] tasks,
        //                                                Task<DP14MSTTaskStateObject>[] continuationTasks,
        //                                                Task<DP14MSTTaskStateObject>[] continuationTasks2,
        //                                                Task<DP14MSTTaskStateObject>[] continuationTasks3,
        //                                                Task<DP14MSTTaskStateObject>[] continuationTasks4,
        //                                                Task<DP14MSTTaskStateObject>[] continuationTasks5,
        //                                                Task<DP14MSTTaskStateObject>[] continuationTasks6,
        //                                                Task<DP14MSTTaskStateObject>[] continuationTasks7,
        //                                                int numOfLevels = 8,
        //                                                int numOfTaskPerGroup = 4)
        //{
        //    bool waitToExit = true;
        //    int exitAttempts = 0;

        //    while (waitToExit && (exitAttempts < 20))
        //    {
        //        waitToExit = false;
        //        for (int outterLoop = 0; outterLoop < numOfLevels; outterLoop++)
        //        {
        //            Task[] tasksRef = null;
        //            switch (outterLoop)
        //            {
        //                case 0:
        //                    tasksRef = tasks;
        //                    break;
        //                case 1:
        //                    tasksRef = continuationTasks;
        //                    break;
        //                case 2:
        //                    tasksRef = continuationTasks2;
        //                    break;
        //                case 3:
        //                    tasksRef = continuationTasks3;
        //                    break;
        //                case 4:
        //                    tasksRef = continuationTasks4;
        //                    break;
        //                case 5:
        //                    tasksRef = continuationTasks5;
        //                    break;
        //                case 6:
        //                    tasksRef = continuationTasks6;
        //                    break;
        //                case 7:
        //                    tasksRef = continuationTasks7;
        //                    break;
        //            }

        //            for (int i = 0; i < tasksRef.Length; i++)
        //            {
        //                if ((tasksRef[i].Status != TaskStatus.RanToCompletion) && (tasksRef[i].Status != TaskStatus.Canceled) && (tasksRef[i].Status != TaskStatus.WaitingForActivation))
        //                {
        //                    waitToExit = true;
        //                    break;
        //                }
        //                else
        //                {
        //                    waitToExit = false;
        //                }
        //            }
        //        }

        //        if (waitToExit)
        //            Thread.Sleep(100); //waitToExit = false;
        //    }

        //    return true;
        //}

        ///// <summary>
        ///// Get data from the probe hardware
        ///// </summary>
        ///// <param name="numOfColumns"></param>
        ///// <param name="numOfLevels"></param>
        ///// <param name="threadParameters"></param>
        //private async void uploadDataFromHardware(int numOfColumns, int numOfLevels, DP14MSTTBUploadDescriptor threadParameters)
        //{
        //    ////
        //    //// Multiple threads will create multiple files (1 per thread) using a 
        //    //// Mutex to gain acess to the HW USB bus (critial section).
        //    ////

        //    ////int workerThreads, complete;
        //    ////ThreadPool.GetMinThreads(out workerThreads, out complete);
        //    ////ThreadPool.SetMinThreads(64, complete);


        //    //try
        //    //{
        //    //    ThreadLocal<DP14MSTTaskStateObject> tls = new ThreadLocal<DP14MSTTaskStateObject>();
        //    //    Mutex mutex = new Mutex();

        //    //    string folderPath = m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME + "\\TraceData_";
        //    //    long blockSize = threadParameters.NumOfStates / (numOfColumns * numOfLevels); 
        //    //    long[] fileStartIndices = getfileStartIndices (numOfColumns * numOfLevels, threadParameters.NumOfStates); 

        //    //    Task<DP14MSTTaskStateObject>[] tasks = new Task<DP14MSTTaskStateObject>[numOfColumns];
        //    //    Task<DP14MSTTaskStateObject>[][] continuationTasks = new Task<DP14MSTTaskStateObject>[numOfLevels - 1][];

        //    //    for (int i = 0; i < numOfLevels - 1; i++)
        //    //        continuationTasks[i] = new Task<DP14MSTTaskStateObject>[numOfColumns];


        //    //    Task<DP14MSTTaskStateObject> parentTask;

        //    //    // create the cancellatin token
        //    //    tokenSource = new CancellationTokenSource();
        //    //    token = tokenSource.Token;

        //    //    // numOfColumns is the number of tasks running concurrently...
        //    //    for (int column = 0; column < numOfColumns; column++)
        //    //    {
        //    //        //
        //    //        // setup one of the root task, from which other tasks will be cascaded from.
        //    //        //
        //    //        tasks[column] = new Task<DP14MSTTaskStateObject>((stateObject) => {
        //    //            tls.Value = (DP14MSTTaskStateObject)stateObject;
        //    //            ((DP14MSTTaskStateObject)tls.Value).Token.ThrowIfCancellationRequested();

        //    //            // get the data...
        //    //            createTraceBufferDataFileII(tls.Value.FolderPath + tls.Value.Index.ToString() + ".bin",
        //    //                                        tls.Value.TrigStateIndex,
        //    //                                        tls.Value.StartStateIndex,
        //    //                                        tls.Value.FileStartIndices[tls.Value.Index],
        //    //                                        tls.Value.BlockSize,
        //    //                                        tls.Value.MemoryDepth,
        //    //                                        tls.Value.NumOfCapturedStates,
        //    //                                        tls.Value.MyFTD2xxIF,
        //    //                                        mutex,
        //    //                                        tls.Value.Token,
        //    //                                        tls.Value.UploadStatusEvent);

        //    //            return tls.Value;

        //    //        }, new DP14MSTTaskStateObject(folderPath, m_FTD2xxIF.TrigOffset, m_FTD2xxIF.StartStateIndex,
        //    //                                            fileStartIndices, blockSize, threadParameters.MemoryDepth,
        //    //                                            threadParameters.NumOfStates, m_FTD2xxIF, threadParameters.uploadStatusEvent, column * numOfLevels, token)); //, token, TaskCreationOptions.LongRunning);


        //    //        //
        //    //        // retain the top level task  -- cascading from top task to the continuation levels
        //    //        //
        //    //        parentTask = tasks[column];


        //    //        //
        //    //        // setup the cascading tasks -- level is used as an array index, thus it is zero based.
        //    //        //
        //    //        for (int level = 0; level < (numOfLevels - 1); level++)
        //    //        {
        //    //            continuationTasks[level][column] = parentTask.ContinueWith(antecedent => {
        //    //                tls.Value = (DP14MSTTaskStateObject)antecedent.Result;
        //    //                tls.Value.Index = tls.Value.Index + 1;
        //    //                ((DP14MSTTaskStateObject)antecedent.Result).Token.ThrowIfCancellationRequested();

        //    //                // get the data...
        //    //                createTraceBufferDataFileII(((DP14MSTTaskStateObject)antecedent.Result).FolderPath + tls.Value.Index.ToString() + ".bin",
        //    //                                            ((DP14MSTTaskStateObject)antecedent.Result).TrigStateIndex,
        //    //                                            ((DP14MSTTaskStateObject)antecedent.Result).StartStateIndex,
        //    //                                            ((DP14MSTTaskStateObject)antecedent.Result).FileStartIndices[tls.Value.Index],
        //    //                                            ((DP14MSTTaskStateObject)antecedent.Result).BlockSize,
        //    //                                            ((DP14MSTTaskStateObject)antecedent.Result).MemoryDepth,
        //    //                                            ((DP14MSTTaskStateObject)antecedent.Result).NumOfCapturedStates,
        //    //                                            ((DP14MSTTaskStateObject)antecedent.Result).MyFTD2xxIF,
        //    //                                            mutex,
        //    //                                            ((DP14MSTTaskStateObject)antecedent.Result).Token,
        //    //                                            ((DP14MSTTaskStateObject)antecedent.Result).UploadStatusEvent);

        //    //                return new DP14MSTTaskStateObject(tls.Value.FolderPath, tls.Value.TrigStateIndex, tls.Value.StartStateIndex,
        //    //                                            tls.Value.FileStartIndices, tls.Value.BlockSize, tls.Value.MemoryDepth,
        //    //                                            tls.Value.NumOfCapturedStates, tls.Value.MyFTD2xxIF, tls.Value.UploadStatusEvent,
        //    //                                            tls.Value.Index, tls.Value.Token);
        //    //            }, token); //, TaskContinuationOptions.LongRunning, TaskScheduler.FromCurrentSynchronizationContext());

        //    //            parentTask = continuationTasks[level][column];
        //    //        }
        //    //    }



        //    //    //// Start all tasks doing the VC File Creation(s)
        //    //    //foreach (Task t in tasks)
        //    //    //    t.Start();

        //    //    try
        //    //    {
        //    //        Task[] waitTasks = new Task[numOfColumns];
        //    //        for (int i = 0; i < numOfColumns; i++)
        //    //            waitTasks[i] = continuationTasks[numOfLevels - 2][i];

        //    //        foreach (Task t in tasks)
        //    //            t.Start();

        //    //        // await keeps the GUI thread active while we are waiting..
        //    //        await Task.WhenAll(waitTasks);


        //    //        foreach (Task T in tasks)
        //    //        {
        //    //            T.Dispose();
        //    //        }

        //    //        for (int outterIndex = 0; outterIndex < numOfLevels-1; outterIndex++)
        //    //        {
        //    //            for (int innerIndex = 0; innerIndex < numOfColumns; innerIndex++)
        //    //                continuationTasks[outterIndex][innerIndex].Dispose();
        //    //        }
        //    //    }
        //    //    catch (OperationCanceledException) { }
        //    //    catch (Exception ex)
        //    //    {
        //    //        using (MessageDlg dlg = new MessageDlg(ex.Message, ex.Message))
        //    //        {
        //    //            dlg.ShowDialog();
        //    //        }
        //    //    }

        //    //    if (token.IsCancellationRequested)
        //    //    {
        //    //        //
        //    //        // Wait for everyone to stop processing (if needed)
        //    //        waitForCascadingTasksToComplete(tasks, continuationTasks); //continuationTasks, continuationTasks2, continuationTasks3, 4, tasks.Length);

        //    //        //if (TBDataUploadStatusEvent != null)
        //    //        //    TBDataUploadStatusEvent(this, new HWDataUploadStatusEventArgs("One monent please, as we remove Uploaded data", true, 0));

        //    //        removeDataFiles(DP14MSTTRACE_BUFFER_MODE.MainLink);

        //    //        //if (exitAttempts > 0)
        //    //        //    TBDataUploadStatusEvent(this, new HWDataUploadStatusEventArgs("Exit Attempts = " + exitAttempts.ToString(), true, 0));
        //    //        //else {
        //    //        if (TBDataUploadStatusEvent != null)
        //    //            TBDataUploadStatusEvent(this, new HWDataUploadStatusEventArgs("Upload Canceled", true, 0));
        //    //        //}           
        //    //    }
        //    //    else
        //    //    {
        //    //        // clear the progess indicator
        //    //        if (TBDataUploadStatusEvent != null)
        //    //            TBDataUploadStatusEvent(this, new HWDataUploadStatusEventArgs("Upload Complete", false));

        //    //    }

        //    //    mutex.Dispose();
        //    //    tokenSource.Dispose();
        //    //    tokenSource = null;


        //    //    // tell the ITBObjects that the data is ready to be processed... especially the State Listing form(s)
        //    //    if (DisplayPort12MSTEvent != null)
        //    //        DisplayPort12MSTEvent(this, new DP12MSTEventArgs((DP12MSTMessage)(m_DP12MSTMessageGenerator.DP12MSTMessageCreate_DataUploaded())));  // Changed; DataReady is sent by the 2nd processing phase
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    MessageBox.Show(ex.Message.ToString());
        //    //}
        //}


        ///// <summary>
        ///// cancel any on-going threads; backworker or TPI
        ///// </summary>
        //private void CancelThreadProcessing()
        //{
        //    if (MemoryDepth > TB_MAX_THREAD_SIZE_SMALL)
        //    {
        //        if (tokenSource != null)
        //            tokenSource.Cancel();
        //    }
        //    else
        //    {
        //        if (m_worker != null)
        //            m_worker.CancelAsync();
        //    }
        //}


        ///// <summary>
        ///// Get the timestamp of the trigger state.
        ///// </summary>
        ///// NOTE:  path is the file path of the original uploaded data file e.g. TraceData_0.bin
        ///// <returns></returns>
        //private bool getTriggerTimeStamp(string path, ref long triggerTimeStamp, ref long triggerStateVCTag)
        //{
        //    triggerTimeStamp = -1;
        //    triggerStateVCTag = -1;

        //    int startStateIndex = 0x00;
        //    int endStateIndex = 0x00;
        //    int triggerStateIndex = 0x00; // m_FTD2xxIF.TriggerStateIndex;
        //    int dataOffset = 0x00;


        //    // path is the file that should/must contain the trigger state...
        //    // uses the meta data to get the trigger offset/index... 
        //    bool status = getMultiTrheadedStateIndices(path, ref startStateIndex, ref endStateIndex, ref triggerStateIndex, ref dataOffset);
        //    if (status)
        //    {
        //        if ((triggerStateIndex >= startStateIndex) && (triggerStateIndex <= endStateIndex))
        //        {
        //            long triggerStateOffset = dataOffset + ((triggerStateIndex - startStateIndex) * TB_STATE_BYTE_LEN);
        //            byte[] stateData = getState(path, triggerStateOffset);
        //            if (stateData != null)
        //            {
        //                int byteID = 0x00;
        //                int bitID = 0x00;
        //                int fldWidth = 0x00;
        //                int fldOffset = 0x00;

        //                // get the time stamp field from the state data
        //                status = getFieldParameters(DP14MSTTRACE_BUFFER_MODE.MainLink, TIMESTAMP_FIELD_NAME, ref fldWidth, ref fldOffset);
        //                if (status)
        //                    status = LoopOperations.GetFieldLocation(TB_STATE_BYTE_LEN, fldOffset, ref byteID, ref bitID);
        //                if (status)
        //                    triggerTimeStamp = LoopOperations.GetFieldValue(byteID, bitID, fldWidth, stateData);


        //                // get the VCTag of the trigger state...
        //                status = getFieldParameters(DP14MSTTRACE_BUFFER_MODE.MainLink, VCTAG_FIELD_NAME, ref fldWidth, ref fldOffset);
        //                if (status)
        //                    status = LoopOperations.GetFieldLocation(TB_STATE_BYTE_LEN, fldOffset, ref byteID, ref bitID);
        //                if (status)
        //                    triggerStateVCTag = LoopOperations.GetFieldValue(byteID, bitID, fldWidth, stateData);
        //            }
        //        }
        //    }

        //    return status;
        //}


        ///// <summary>
        ///// returns the number of columns used in the cascading tasks construct (needed for searching)
        ///// </summary>
        ///// <param name="memDepth"></param>
        ///// <returns></returns>
        //private int getNumberOfColumns(DP14MSTTRACE_BUFFER_MODE mode, long memoryDepth)
        //{
        //    int numOfColumns = 0;
        //    if (mode == DP14MSTTRACE_BUFFER_MODE.MainLink)
        //    {
        //        if (memoryDepth <= TB_MAX_THREAD_SIZE_SMALL)
        //        {
        //            numOfColumns = SMALL_DATA_UPLOAD_NUM_OF_COLUMNS;
        //        }
        //        else if (memoryDepth <= TB_MAX_THREAD_SIZE_MEDIUM)
        //        {
        //            numOfColumns = MEDIUM_DATA_UPLOAD_NUM_OF_COLUMNS;
        //        }
        //        else if (memoryDepth <= TB_MAX_THREAD_SIZE_LARGE)
        //        {
        //            numOfColumns = LARGE_DATA_UPLOAD_NUM_OF_COLUMNS;
        //        }
        //        else if (memoryDepth <= TB_MAX_THREAD_SIZE_VERY_LARGE)
        //        {
        //            numOfColumns = LARGEST_DATA_UPLOAD_NUM_OF_COLUMNS;
        //        }
        //    }
        //    else if (mode == DP14MSTTRACE_BUFFER_MODE.AuxiliaryLink)
        //    {
        //        numOfColumns = 1;  // single thread (BGWorker) 
        //    }

        //    return numOfColumns;
        //}



        ///// <summary>
        ///// Get the number of data files that will be created on each capture.
        ///// </summary>
        ///// <param name="memoryDepth"></param>
        ///// <returns></returns>
        //private int calculateNumOfDataBlocks(long memoryDepth)
        //{
        //    int numOfFiles = 0;

        //    if (MemoryDepth <= TB_MAX_THREAD_SIZE_SMALL)
        //    {
        //        numOfFiles = SMALL_DATA_UPLOAD_NUM_OF_COLUMNS * SMALL_DATA_UPLOAD_NUM_OF_LEVELS;        // 2 Columns, 2 Levels
        //    }
        //    else if (MemoryDepth <= TB_MAX_THREAD_SIZE_MEDIUM)
        //    {
        //        numOfFiles = MEDIUM_DATA_UPLOAD_NUM_OF_COLUMNS * MEDIUM_DATA_UPLOAD_NUM_OF_LEVELS;      // 4 Columns, 4 Levels);
        //    }
        //    else if (MemoryDepth <= TB_MAX_THREAD_SIZE_LARGE)
        //    {
        //        numOfFiles = LARGE_DATA_UPLOAD_NUM_OF_COLUMNS * LARGE_DATA_UPLOAD_NUM_OF_LEVELS;        // 4 Columns, 8 Levels);
        //    }
        //    else if (MemoryDepth <= TB_MAX_THREAD_SIZE_VERY_LARGE)
        //    {
        //        numOfFiles = LARGEST_DATA_UPLOAD_NUM_OF_COLUMNS * LARGEST_DATA_UPLOAD_NUM_OF_LEVELS;    // 8 Columns, 8 Levels);
        //    }

        //    return numOfFiles;
        //}

        /// <summary>
        /// Get the data from the probe and store in data file(s)
        /// </summary>
        /// m_TBParameters.TRIG, m_TBParameters.TBFIN, m_TBParameters.WRAP,
        /// m_TBParameters.PRE_TRIG_LINE_COUNT, m_TBParameters.PST_TRIG_LINE_CNT,
        ///                            m_TBParameters.TIME_COUNT, m_TBParameters.TRIG_LINE, memoryDepth
        ///
        /// http://stackoverflow.com/questions/11500563/winform-multithreading-use-backgroundworker-or-not
        /// 
        /// <returns></returns>
        private bool getDataFromHardware()
        {
            bool status = false; // true;
            //DP14MSTTBUploadDescriptor threadParameters = new DP14MSTTBUploadDescriptor(m_FTD2xxIF.StartStateIndex,
            //                                                                                m_FTD2xxIF.EndStateIndex,
            //                                                                                m_FTD2xxIF.TriggerStateIndex,
            //                                                                                m_FTD2xxIF.TrigOffset,
            //                                                                                m_FTD2xxIF.NumOfCapturedStates,
            //                                                                                m_FTD2xxIF.CaptureWrapped,
            //                                                                                m_memoryDepth,
            //                                                                                TB_PAGE_BYTE_LENGTH,
            //                                                                                TB_STATE_BYTE_LEN,
            //                                                                                TBDataUploadStatusEvent);

            //// raise an event to the parent form to disable all other forms... we are busy uploading data...
            //if (TBDataUploadStatusEvent != null)
            //    TBDataUploadStatusEvent(this, new HWDataUploadStatusEventArgs("Data Upload Started", true));  // <<== this caused Gen2.processTBDataUploadEvent() to de-assert ABRUN and TBRUN

            //if (MemoryDepth <= TB_MAX_THREAD_SIZE_SMALL)
            //{
            //    uploadDataFromHardware(SMALL_DATA_UPLOAD_NUM_OF_COLUMNS, SMALL_DATA_UPLOAD_NUM_OF_LEVELS, threadParameters);  //2 Columns, 2 Levels
            //}
            //else if (MemoryDepth <= TB_MAX_THREAD_SIZE_MEDIUM)
            //{
            //    uploadDataFromHardware(MEDIUM_DATA_UPLOAD_NUM_OF_COLUMNS, MEDIUM_DATA_UPLOAD_NUM_OF_LEVELS, threadParameters); //4 Columns, 4 Levels);
            //}
            //else if (MemoryDepth <= TB_MAX_THREAD_SIZE_LARGE)
            //{
            //    uploadDataFromHardware(LARGE_DATA_UPLOAD_NUM_OF_COLUMNS, LARGE_DATA_UPLOAD_NUM_OF_LEVELS, threadParameters); //4 Columns, 8 Levels);
            //}
            //else if (MemoryDepth <= TB_MAX_THREAD_SIZE_VERY_LARGE)
            //{
            //    uploadDataFromHardware(LARGEST_DATA_UPLOAD_NUM_OF_COLUMNS, LARGEST_DATA_UPLOAD_NUM_OF_LEVELS, threadParameters); //8 Columns, 8 Levels);
            //}
            return status;
        }



        /// <summary>
        /// Get the data from the probe and store in data file(s)
        /// </summary>
        /// 
        /// m_TBParameters.TRIG, m_TBParameters.TBFIN, m_TBParameters.WRAP,
        /// m_TBParameters.PRE_TRIG_LINE_COUNT, m_TBParameters.PST_TRIG_LINE_CNT,
        ///                            m_TBParameters.TIME_COUNT, m_TBParameters.TRIG_LINE, memoryDepth
        ///
        /// <returns></returns>
        private bool getAuxDataFromHardware()
        {
            bool status = false; // true;
            //DP14MSTTBUploadDescriptor threadParameters = new DP14MSTTBUploadDescriptor(m_FTD2xxIF.StartStateIndex_Aux,
            //                                                                                m_FTD2xxIF.EndStateIndex_Aux,
            //                                                                                m_FTD2xxIF.TriggerStateIndex_Aux,
            //                                                                                m_FTD2xxIF.TrigOffset_Aux,
            //                                                                                m_FTD2xxIF.NumOfCapturedStates_Aux,
            //                                                                                m_FTD2xxIF.CaptureWrapped_Aux,
            //                                                                                m_auxMemoryDepth);

            //// raise an event to the parent form to disable all other forms... we are busy uploading data...
            //if (TBDataUploadStatusEvent != null)
            //    TBDataUploadStatusEvent(this, new HWDataUploadStatusEventArgs("Auxiliary Upload Started", true));


            //// always will be in a back ground work thread... data size is assumed to be 16K states.
            //m_TBDataMode = DP14MSTTBDataMgrMode.SingleThreaded;
            //uploadData_BGWorkerMode(threadParameters, false);

            return status;
        }


        /// <summary>
        /// create the binary file(s).
        /// </summary>
        /// <returns></returns>
        private bool uploadData(long memoryDepth)
        {
            bool status = false;

            //// call the method to get the starting and ending indices
            //status = createTBDataStatistics(memoryDepth);
            //if (status)
            //{
            //    // remove old/previous trace buffer data
            //    clearDataUploadFolder();

            //    // get the data and store in file(s)
            //    status = getDataFromHardware();
            //}

            return status;
        }


        ///// <summary>
        ///// Get the data from the Probe Hardware and store in a binary file.
        ///// </summary>
        ///// <param name="memoryDepth"></param>
        ///// <returns></returns>
        //private bool uploadTraceBufferData(DP12MSTMessage DisplayPortEvent)
        //{
        //    bool status = true;

        //    if (m_memoryDepth <= TB_MAX_THREAD_SIZE_SMALL)
        //    {
        //        m_numOfTPITasks = TB_NUM_OF_THREADS_SMALL;
        //        m_numOfTaskColumns = SMALL_DATA_UPLOAD_NUM_OF_COLUMNS;
        //    }
        //    else if (m_memoryDepth <= TB_MAX_THREAD_SIZE_MEDIUM)
        //    {
        //        m_numOfTPITasks = TB_NUM_OF_THREADS_MEDIUM;
        //        m_numOfTaskColumns = LARGE_DATA_UPLOAD_NUM_OF_COLUMNS;
        //    }
        //    else if (m_memoryDepth <= TB_MAX_THREAD_SIZE_LARGE)
        //    {
        //        m_numOfTPITasks = TB_NUM_OF_THREADS_LARGE;
        //        m_numOfTaskColumns = LARGE_DATA_UPLOAD_NUM_OF_COLUMNS;
        //    }
        //    else if (m_memoryDepth <= TB_MAX_THREAD_SIZE_VERY_LARGE)
        //    {
        //        m_numOfTPITasks = TB_NUM_OF_THREADS_VERY_LARGE;
        //        m_numOfTaskColumns = LARGE_DATA_UPLOAD_NUM_OF_COLUMNS;
        //    }
        //    else
        //    {
        //        m_numOfTPITasks = TB_NUM_OF_THREADS_SMALL;
        //        m_numOfTaskColumns = SMALL_DATA_UPLOAD_NUM_OF_COLUMNS;
        //    }

        //    m_numOfDataFiles = calculateNumOfDataBlocks(m_memoryDepth) * 4;  // e.g. 16 data blocks with each block having 4 virtual channels
        //    status = uploadData(m_memoryDepth);

        //    return status;
        //}


        /// <summary>
        /// Examine the FS4500 folder and determine the number of data files it contains.
        /// </summary>
        /// <returns></returns>
        private int getNumberOfDataFiles()
        {
            int numOfFiles = 0;

            // clear the folder of all files...
            //DirectoryInfo di = new DirectoryInfo(m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME);
            DirectoryInfo di = new DirectoryInfo(m_instanceFolderPath);
            foreach (FileInfo file in di.GetFiles())
            {
                if (file.Name.StartsWith("TraceData_"))
                    numOfFiles += 1;
            }

            return numOfFiles;
        }


        /// <summary>
        /// Examine the FS4500 folder and determine the number of data files it contains.
        /// </summary>
        /// <returns></returns>
        private List<string> getDataFileNames()
        {
            List<string> fileNames = new List<string>();

            // clear the folder of all files...
            //DirectoryInfo di = new DirectoryInfo(m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME);
            DirectoryInfo di = new DirectoryInfo(m_instanceFolderPath);
            foreach (FileInfo file in di.GetFiles())
            {
                if (file.Name.StartsWith("TraceData"))
                    fileNames.Add(file.Name);
            }

            return fileNames;
        }


        ///// <summary>
        ///// Segregate the data file into virtual channel data files.
        ///// </summary>
        ///// <returns></returns>
        //private bool generateVirtualChannelFiles(long triggerTimeStamp, int trigChannelID)
        //{
        //    bool status = true;

        //    m_VCFileGenerator.InitializeProcessingMembers();

        //    // get the number of data files
        //    status = m_VCFileGenerator.CreateVirtualChannelDataFiles(getDataFileNames(), triggerTimeStamp, trigChannelID);
            
        //    return status;
        //}


        ///// <summary>
        ///// ???
        ///// </summary>
        ///// <param name="triggerTimeStamp"></param>
        ///// <param name="trigChannelID"></param>
        ///// <returns></returns>
        //private bool getSegregatedTriggerLocation(long triggerTimeStamp, int trigChannelID)
        //{
        //    bool status = true;

        //    status = m_VCFileGenerator.GetSegregatedTriggerLocation(triggerTimeStamp, trigChannelID);

        //    return status;
        //}


        ///// <summary>
        ///// ???
        ///// </summary>
        ///// <param name="triggerTimeStamp"></param>
        ///// <param name="trigChannelID"></param>
        ///// <returns></returns>
        //private bool getSegregatedMetaData(long triggerTimeStamp, int trigChannelID)
        //{
        //    bool status = true;

        //    status = m_VCFileGenerator.GetSegregatedMetaData(triggerTimeStamp, trigChannelID);

        //    return status;
        //}


        ///// <summary>
        ///// ???
        ///// </summary>
        ///// <param name="triggerTimeStamp"></param>
        ///// <param name="trigChannelID"></param>
        ///// <returns></returns>
        //private bool setMetaDataTriggerStateIndices(long triggerTimeStamp, int trigChannelID)
        //{
        //    bool status = true;

        //    status = m_VCFileGenerator.SetMetaDataTriggerStateIndices(triggerTimeStamp, trigChannelID);
        //    return status;
        //}


        ///// <summary>
        ///// ???
        ///// </summary>
        ///// <param name="m_triggerTimeStamp"></param>
        ///// <param name="m_triggerVChannelID"></param>
        ///// <returns></returns>
        //public bool prependSegregatedMetaData(long m_triggerTimeStamp, int m_triggerVChannelID)
        //{
        //    return m_VCFileGenerator.PrependSegregatedMetaData(m_triggerTimeStamp, m_triggerVChannelID);
        //}



        ///// <summary>
        ///// ???
        ///// </summary>
        ///// <returns></returns>
        //public List<DP14MSTVCFileGenerator_Threads.DP14MSTMultiThread_MetaDataArgs> getMultiThreadedFileMetaData()
        //{
        //    return m_VCFileGenerator.GetMultiThreadedFileMetaData();
        //}

        ///// <summary>
        ///// Get the auxiliary data from the Probe Hardware and store in a binary file.
        ///// </summary>
        ///// <param name=""></param>
        ///// <returns></returns>
        //private bool uploadAuxTraceBufferData(DP12MSTMessage DisplayPortEvent)
        //{
        //    bool status = false;

        //    //// call the method to get the starting and ending indices
        //    //status = createAuxTBDataStatistics(m_auxMemoryDepth);
        //    //if (status)
        //    //{
        //    //    // remove old/previous trace buffer data
        //    //    clearDataUploadFolder_Aux();

        //    //    // get the data and store in file(s)
        //    //    status = getAuxDataFromHardware();
        //    //}

        //    return status;
        //}


        ///// <summary>
        ///// Extract, if possible, state data from the uploaded binary file.
        ///// </summary>
        ///// <param name="DisplayPortEvent"></param>
        //private void getAuxRequestedData(DP12MSTMessage DisplayPortEvent)
        //{
        //    bool status = false;

        //    // if the TB file exists...
        //    if (File.Exists(m_auxTraceFilePath))
        //    {
        //        if (m_auxCurrentStateNumber != ((DP12MSTMessage_AuxStateDataRequest)DisplayPortEvent).StateNumber)
        //        {
        //            // clear the previous state data 
        //            Array.Clear(m_auxCurrentStateData, 0, m_auxCurrentStateData.Length);

        //            // get the requested state data
        //            status = getAuxState(((DP12MSTMessage_AuxStateDataRequest)DisplayPortEvent).StateNumber, (int)m_dataOffset_auxLink);
        //            if (!status)
        //            {
        //                m_auxCurrentStateNumber = long.MinValue;
        //                Array.Clear(m_auxCurrentStateData, 0, m_auxCurrentStateData.Length);
        //            }
        //        }
        //    }

        //    // either way... send the response.
        //    if (DisplayPortEvent != null)
        //    {
        //        // Raise the response event containing the requested data
        //        DisplayPort12MSTEvent(this,
        //                            new DP12MSTEventArgs(m_DP12MSTMessageGenerator.DP12MSTMessageCreate_AuxStateDataResponse(((DP12MSTMessage_AuxStateDataRequest)DisplayPortEvent).ID,
        //                                               ((DP12MSTMessage_AuxStateDataRequest)DisplayPortEvent).StateNumber,
        //                                               m_auxCurrentStateData)));
        //    }
        //}


        /// <summary>
        /// Extract, if possible, state data from the uploaded binary file.
        /// </summary>
        /// <param name="DisplayPortEvent"></param>
        private void getRequestedData(DP14MSTMessage DisplayPortEvent)
        {
            bool status = false;

            if (m_TBDataMode == DP14MSTTBDataMgrMode.SingleThreaded)
            {
                // if the TB file exists...
                if (File.Exists(m_traceFilePath))
                {
                    if (m_currentStateNumber != ((DP14MSTMessage_StateDataRequest)DisplayPortEvent).StateNumber)
                    {
                        // clear the previous state data 
                        Array.Clear(m_currentStateData, 0, m_currentStateData.Length);

                        // get the requested state data
                        status = getState(((DP14MSTMessage_StateDataRequest)DisplayPortEvent).VChannelID, ((DP14MSTMessage_StateDataRequest)DisplayPortEvent).StateNumber, (int)m_dataOffset_mainLink);
                        if (!status)
                        {
                            m_currentStateNumber = long.MinValue;
                            Array.Clear(m_currentStateData, 0, m_currentStateData.Length);
                        }
                    }
                }
            }
            else if (m_TBDataMode == DP14MSTTBDataMgrMode.MultiThreaded)
            {
                status = getState(((DP14MSTMessage_StateDataRequest)DisplayPortEvent).VChannelID, ((DP14MSTMessage_StateDataRequest)DisplayPortEvent).StateNumber);
                if (!status)
                {
                    m_currentStateNumber = long.MinValue;
                    Array.Clear(m_currentStateData, 0, m_currentStateData.Length);
                }
            }
            else
            {
                m_currentStateNumber = long.MinValue;
                Array.Clear(m_currentStateData, 0, m_currentStateData.Length);
            }

            // either way... send the response.
            if (DisplayPort12MSTEvent != null)
            {
                // Raise the response event containing the requested data
                DisplayPort12MSTEvent(this,
                                    new DP14MSTEventArgs(m_DP12MSTMessageGenerator.DP14MSTMessageCreate_StateDataResponse(
                                                            ((DP14MSTMessage_StateDataRequest)DisplayPortEvent).ID,
                                                            ((DP14MSTMessage_StateDataRequest)DisplayPortEvent).VChannelID,
                                                            ((DP14MSTMessage_StateDataRequest)DisplayPortEvent).StateNumber,
                                                            m_currentStateData)));
            }
        }


        /// <summary>
        /// Get a specified sized chunk of data...
        /// </summary>
        /// <param name="DisplayPortEvent"></param>
        private void getRequestedDataChunk(DP14MSTMessage DisplayPortEvent)
        {
            bool status = false;
            DP14MSTMessage_StateDataChunkRequest msg = DisplayPortEvent as DP14MSTMessage_StateDataChunkRequest;


            if (m_TBDataMode == DP14MSTTBDataMgrMode.SingleThreaded)
            {
                //// if the TB file exists...
                //if (File.Exists(m_traceFilePath))
                //{
                //    if (m_currentStateNumber != ((DP14SSTMessage_StateDataRequest)DisplayPortEvent).StateNumber)
                //    {
                //        // clear the previous state data 
                //        Array.Clear(m_currentStateData, 0, m_currentStateData.Length);

                //        // get the requested state data
                //        status = getState(((DP14SSTMessage_StateDataRequest)DisplayPortEvent).StateNumber, (int)m_dataOffset_mainLink);
                //        if (!status)
                //        {
                //            m_currentStateNumber = long.MinValue;
                //            Array.Clear(m_currentStateData, 0, m_currentStateData.Length);
                //        }
                //    }
                //}
            }
            else if (m_TBDataMode == DP14MSTTBDataMgrMode.MultiThreaded)
            {
                status = getStateChunk((DP14MSTMessage_StateDataChunkRequest)DisplayPortEvent);
                if (!status)
                {
                    m_currentStateNumber = long.MinValue;
                    Array.Clear(m_currentStateData, 0, m_currentStateData.Length);
                }
            }
            else
            {
                m_currentStateNumber = long.MinValue;
                Array.Clear(m_currentStateData, 0, m_currentStateData.Length);
                msg.ClearDataChunk();
            }
        }


        /// <summary>
        /// Get the state data assoicated with a specific state index and return the data as a message property
        /// </summary>
        /// <param name="DisplayPortEvent"></param>
        /// <returns></returns>
        public bool getRequestedStateData(DP14MSTMessage DisplayPortEvent)
        {
            bool status = false;
            DP14MSTMessage_GetStateData msg = DisplayPortEvent as DP14MSTMessage_GetStateData;
            byte[] dataBytes = new byte[TB_STATE_BYTE_LEN];

            if (DisplayPortEvent is DP14MSTMessage_GetStateData)
            {
                if (m_TBDataMode == DP14MSTTBDataMgrMode.SingleThreaded)
                {
                    // if the TB file exists...
                    if (File.Exists(m_traceFilePath))
                    {
                        // clear the previous state data 
                        Array.Clear(msg.DataBytes, 0, msg.DataBytes.Length);

                        // get the requested state data
                        status = getStateData(msg.VChannelID, msg.StateIndex, (int)m_dataOffset_mainLink, ref dataBytes);
                        if (status)
                            Array.ConstrainedCopy(dataBytes, 0, msg.DataBytes, 0, msg.DataBytes.Length);
                        else
                            Array.Clear(msg.DataBytes, 0, msg.DataBytes.Length);
                    }
                }
                else if (m_TBDataMode == DP14MSTTBDataMgrMode.MultiThreaded)
                {
                    //
                    // search the meta data list for the given channel and determine the file index 
                    //

                    status = getState(msg.VChannelID, msg.StateIndex);
                    if (status)
                        Array.ConstrainedCopy(m_currentStateData, 0, msg.DataBytes, 0, msg.DataBytes.Length);
                    else
                        Array.Clear(msg.DataBytes, 0, msg.DataBytes.Length);
                }
                else
                {
                    Array.Clear(msg.DataBytes, 0, msg.DataBytes.Length);
                }
            }

            return status;
        }


        /// <summary>
        /// Get the state data assoicated with a specific state index and return the data as a message property
        /// </summary>
        /// <param name="DisplayPortEvent"></param>
        /// <returns></returns>
        public bool getRequestedAuxStateData(DP14MSTMessage DisplayPortEvent)
        {
            bool status = false;
            DP14MSTMessage_AuxGetStateData msg = DisplayPortEvent as DP14MSTMessage_AuxGetStateData;
            byte[] dataBytes = new byte[TB_STATE_BYTE_LEN];

            if (DisplayPortEvent is DP14MSTMessage_AuxGetStateData)
            {
                if (m_TBDataMode == DP14MSTTBDataMgrMode.SingleThreaded)
                {
                    // if the TB file exists...
                    if (File.Exists(m_auxTraceFilePath))
                    {
                        // clear the previous state data 
                        Array.Clear(msg.AuxDataBytes, 0, msg.AuxDataBytes.Length);

                        // get the requested state data
                        status = getAuxStateData(msg.StateIndex, (int)m_dataOffset_auxLink, ref dataBytes); // getStateData( msg.StateIndex, (int)m_dataOffset_mainLink, ref dataBytes);
                        if (status)
                            Array.ConstrainedCopy(dataBytes, 0, msg.AuxDataBytes, 0, msg.AuxDataBytes.Length);
                        else
                            Array.Clear(msg.AuxDataBytes, 0, msg.AuxDataBytes.Length);
                    }
                }
                //else if (m_TBDataMode == DP14MSTTBDataMgrMode.MultiThreaded)
                //{
                //    //
                //    // search the meta data list for the given channel and determine the file index 
                //    //
                //    int fileIndex = getFileIndex(msg.VChannelID, msg.StateIndex);
                //    if (fileIndex != -1)
                //    {
                //        status = getStateData(fileIndex, msg.VChannelID, msg.StateIndex, ref dataBytes);
                //        if (status)
                //            Array.ConstrainedCopy(dataBytes, 0, msg.DataBytes, 0, msg.DataBytes.Length);
                //        else
                //            Array.Clear(msg.DataBytes, 0, msg.DataBytes.Length);
                //    }
                //}
                else
                {
                    Array.Clear(msg.AuxDataBytes, 0, msg.AuxDataBytes.Length);
                }
            }

            return status;
        }


        /// <summary>
        /// Extract a single data entry from the auxiliary binary trace buffer data file.
        /// </summary>
        /// <param name="stateIndex"></param>
        /// <param name="dataOffset"></param>
        /// <returns></returns>
        private bool getAuxState(long stateIndex, int dataOffset)
        {
            bool status = false;
            Array.Clear(m_auxCurrentStateData, 0, m_auxCurrentStateData.Length);

            using (FileStream fs = new FileStream(m_auxTraceFilePath, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length > (dataOffset + (stateIndex * AUX_STATE_BYTE_LEN)))
                {
                    fs.Position = dataOffset + (stateIndex * AUX_STATE_BYTE_LEN);
                    using (BinaryReader br = new BinaryReader(fs, new ASCIIEncoding()))
                    {
                        Array.Copy(br.ReadBytes(AUX_STATE_BYTE_LEN), m_auxCurrentStateData, m_auxCurrentStateData.Length);
                        status = true;
                    }
                }
            }

            return status;
        }


        /// <summary>
        /// Get a state at a pre-determined location 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="stateOffset"></param>
        /// <returns></returns>
        private byte[] getState(string path, long stateOffset)
        {
            byte[] stateData = null; 
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length >= (stateOffset + TB_STATE_BYTE_LEN))
                {
                    fs.Position = stateOffset;
                    using (BinaryReader br = new BinaryReader(fs, new ASCIIEncoding()))
                    {
                        stateData = new byte[TB_STATE_BYTE_LEN];
                        Array.Copy(br.ReadBytes(TB_STATE_BYTE_LEN), stateData, stateData.Length);
                    }
                }
            }

            return stateData;
        }


        /// <summary>
        /// Extract a single data entry from the Main Link binary trace buffer  data file.
        /// </summary>
        /// <param name="curIndex"></param>
        /// <returns></returns>
        /// Assumuptions:  Called for single threaded files only.
        /// 
        private bool getState(int vChannelID, long stateIndex, int dataOffset)
        {
            bool status = false;
            Array.Clear(m_currentStateData, 0, m_currentStateData.Length);

            using (FileStream fs = new FileStream(m_traceFilePath, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length > (dataOffset + (stateIndex * TB_STATE_BYTE_LEN)))
                {
                    fs.Position = dataOffset + (stateIndex * TB_STATE_BYTE_LEN);
                    using (BinaryReader br = new BinaryReader(fs, new ASCIIEncoding()))
                    {
                        Array.Copy(br.ReadBytes(TB_STATE_BYTE_LEN), m_currentStateData, m_currentStateData.Length);
                        status = true;
                    }
                }
            }

            return status;
        }


        ///// <summary>
        ///// Get a refernece to the list of meta data
        ///// </summary>
        ///// <param name="vChannelID"></param>
        ///// <returns></returns>
        //private List<DP14MSTMultiThread_MetaDataArgs> getMultiThreadFileDataRef(int vChannelID)
        //{
        //    List<DP14MSTMultiThread_MetaDataArgs> multiThreadFileMetaData = null;

        //    switch (vChannelID)
        //    {
        //        case 1:
        //            multiThreadFileMetaData = m_multiThreadFileMetaData_VC1;
        //            break;
        //        case 2:
        //            multiThreadFileMetaData = m_multiThreadFileMetaData_VC2;
        //            break;
        //        case 3:
        //            multiThreadFileMetaData = m_multiThreadFileMetaData_VC3;
        //            break;
        //        case 4:
        //            multiThreadFileMetaData = m_multiThreadFileMetaData_VC4;
        //            break;
        //        default:
        //            break;
        //    }

        //    return multiThreadFileMetaData;
        //}


        /// <summary>
        /// Search through the list of meta data structures/objects to get the file in which
        /// the requested state resides.
        /// </summary>
        /// <param name="stateIndex"></param>
        /// <param name="vChannelID"></param>
        /// <returns></returns>
        /// assumes:     list of meta data is in the following order
        /// 
        ///                     block                                                      index
        ///                 ----------------------------------------------------------------------------------------
        ///                 TraceData_0_VC_1.bin  |  (blockID * 4) + (vchannel -1 )  ==>  (0*4) - (1-1)   =   0 + 0 = 0
        ///                 TraceData_0_VC_2.bin  |  (blockID * 4) + (vchannel -1 )  ==>  (0*4) - (2-1)   =   0 + 1 = 1
        ///                 TraceData_0_VC_3.bin  |  (blockID * 4) + (vchannel -1 )  ==>  (0*4) - (3-1)   =   0 + 2 = 2
        ///                 TraceData_0_VC_4.bin  |  (blockID * 4) + (vchannel -1 )  ==>  (0*4) - (4-1)   =   0 + 3 = 3
        ///                 
        ///                 TraceData_1_VC_1.bin  |  (blockID * 4) + (vchannel -1 )  ==>  (1*4) - (1-1)   =   4 + 0 = 4
        ///                 TraceData_1_VC_2.bin  |  (blockID * 4) + (vchannel -1 )  ==>  (1*4) - (2-1)   =   4 + 1 = 5
        ///                 TraceData_1_VC_3.bin  |  (blockID * 4) + (vchannel -1 )  ==>  (1*4) - (3-1)   =   4 + 2 = 6
        ///                 TraceData_1_VC_4.bin  |  (blockID * 4) + (vchannel -1 )  ==>  (1*4) - (4-1)   =   4 + 3 = 7
        ///                     :
        ///                     :
        private string getMultiThreadedFileName(long stateIndex, int vChannelID)
        {
            string fileName = string.Empty;
            int index = -1;

            for (int blockID = 0; blockID < (m_numOfDataFiles / 4); blockID++)
            {
                index = (blockID * 4) + (vChannelID - 1);
                if ((stateIndex >= m_multiThreadFileMetaData[index].StartState) && (stateIndex <= m_multiThreadFileMetaData[index].EndState))
                {
                    fileName = generateMultiThreadFileName(index/4, vChannelID);
                    break;
                }
            }

            return fileName;
        }


        /// <summary>
        /// Get the meta data list index for the given filename
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// Assumes:  file name has the format of TraceData_0_VC_1.bin
        ///                                                 |    |
        ///                                                 |    VChannel ID
        ///                                                 |
        ///                                                 BlockID
        private int getMultiThreadedMetaDataIndex(string fileName)
        {
            string[] comps = fileName.Split(new char[] { '_' , '.' });
            int index = -1;

            if (comps.Length == 5)
            {
                int blockID = int.Parse(comps[1]);
                int channelID = int.Parse(comps[3]);

                index = (blockID * 4) + (channelID - 1);
            }

            return index;
        }


        /// <summary>
        /// Extract a single data entry from the Main Link binary trace buffer data file.
        /// </summary>
        /// <param name="curIndex"></param>
        /// <returns></returns>
        private bool getState(int vChannelID, long stateIndex)
        {
            bool status = false;
            Array.Clear(m_currentStateData, 0, m_currentStateData.Length);

            string fileName = getMultiThreadedFileName(stateIndex, vChannelID);
            if (File.Exists(fileName))
            {
                string fName = Path.GetFileName(fileName);
                int index = getMultiThreadedMetaDataIndex(fName); //fileName);

                // extract the state data from the specified file.
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    if (fs.Length > (m_multiThreadFileMetaData[index].DataOffset + 16))                                      
                    {
                        //
                        // NOTE: on the stateOffset Calculation....
                        //          ((EndState - StartState) - (EndState - StateIndex)) * 16 yeilds the byte ID of the start of the requested state.
                        //                     |                            |
                        //                     |                            |
                        //                     |                          nth state in the file.
                        //             # of states in the file
                        //
                        //      Example in terms of millions of states:
                        //  
                        //              stateOffset = ((1,400,000 - 1,300,000) - (1,400,000 - 1,350,000)) = 
                        //                          = 100,000 - 50,000 
                        //                          = 50,000 th state in the file contains the requested state data
                        //

                        int stateOffset = ((int)stateIndex - m_multiThreadFileMetaData[index].StartState) * 16;
                        fs.Position = m_multiThreadFileMetaData[index].DataOffset + stateOffset;
                        using (BinaryReader br = new BinaryReader(fs, new ASCIIEncoding()))
                        {
                            Array.Copy(br.ReadBytes(TB_STATE_BYTE_LEN), m_currentStateData, m_currentStateData.Length);
                            status = true;
                        }
                    }
                }
            }

            return status;
        }


        /// <summary>
        /// Extract a chunk of states.
        /// </summary>
        /// <param name="VChannelID"></param>
        /// <param name="StateNumber"></param>
        /// <returns></returns>
        private bool getStateChunk(DP14MSTMessage_StateDataChunkRequest msg)
        {
            bool status = false;
            //((DP14MSTMessage_StateDataRequest)DisplayPortEvent).VChannelID, ((DP14MSTMessage_StateDataRequest)DisplayPortEvent).StateNumber);

            if ((msg.VChannelID >= 1) && (msg.VChannelID <= 4))
            {
                string fileName = getMultiThreadedFileName(msg.StateNumber, msg.VChannelID);
                if (File.Exists(fileName))
                {
                    int index = getMultiThreadedMetaDataIndex(fileName);
                    msg.ClearDataChunk();

                    // extract the state data from the specified file.
                    using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                    {
                        if (fs.Length > (m_multiThreadFileMetaData[index].DataOffset + 16))
                        {
                            //
                            // NOTE: on the stateOffset Calculation....
                            //          ((EndState - StartState) - (EndState - StateIndex)) * 16 yeilds the byte ID of the start of the requested state.
                            //                     |                            |
                            //                     |                            |
                            //                     |                          nth state in the file.
                            //             # of states in the file
                            //
                            //      Example in terms of millions of states:
                            //  
                            //              stateOffset = ((1,400,000 - 1,300,000) - (1,400,000 - 1,350,000)) = 
                            //                          = 100,000 - 50,000 
                            //                          = 50,000 th state in the file contains the requested state data
                            //

                            //int stateOffset = ((int)stateIndex - m_multiThreadFileMetaData[index].StartState) * 16;
                            int stateOffset = ((int)msg.StateNumber - m_multiThreadFileMetaData[index].StartState) * 16;
                            fs.Position = m_multiThreadFileMetaData[index].DataOffset + stateOffset;
                            using (BinaryReader br = new BinaryReader(fs, new ASCIIEncoding()))
                            {
                                for (int i = 0; (i < msg.ChunkSize) && (fs.Position <= (fs.Length - TB_STATE_BYTE_LEN)); i++)
                                {
                                    //Array.Copy(br.ReadBytes(TB_STATE_BYTE_LEN), m_currentStateData, m_currentStateData.Length);
                                    msg.DataChunk.AddRange(br.ReadBytes(TB_STATE_BYTE_LEN));
                                }
                                status = true;
                            }
                        }
                    }
                }
            }

            return status;
        }


        /// <summary>
        /// Get a single state of auxiliary link data
        /// </summary>
        /// <param name="stateIndex"></param>
        /// <param name="dataOffset"></param>
        /// <param name="stateData"></param>
        /// <returns></returns>
        private bool getAuxStateData(long stateIndex, int dataOffset, ref byte[] stateData)
        {
            bool status = false;
            Array.Clear(m_currentStateData, 0, m_currentStateData.Length);

            using (FileStream fs = new FileStream(m_auxTraceFilePath, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length > (dataOffset + (stateIndex* TB_STATE_BYTE_LEN)))
                {
                    fs.Position = dataOffset + (stateIndex* TB_STATE_BYTE_LEN);
                    using (BinaryReader br = new BinaryReader(fs, new ASCIIEncoding()))
                    {
                        Array.Copy(br.ReadBytes(TB_STATE_BYTE_LEN), stateData, stateData.Length);
                        status = true;
                    }
                }
            }

            return status;
        }


        /// <summary>
        /// Extract a single data entry from the Main Link binary trace buffer data file. 
        /// </summary>
        /// <param name="curIndex"></param>
        /// <returns></returns>
        /// Assumuptions:  Called for single threaded files only.
        /// 
        private bool getStateData(int vChannelID, long stateIndex, int dataOffset, ref byte[] stateData)
        {
            bool status = false;
            Array.Clear(m_currentStateData, 0, m_currentStateData.Length);

            using (FileStream fs = new FileStream(m_traceFilePath, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length > (dataOffset + (stateIndex * TB_STATE_BYTE_LEN)))
                {
                    fs.Position = dataOffset + (stateIndex * TB_STATE_BYTE_LEN);
                    using (BinaryReader br = new BinaryReader(fs, new ASCIIEncoding()))
                    {
                        Array.Copy(br.ReadBytes(TB_STATE_BYTE_LEN), stateData, stateData.Length);
                        status = true;
                    }
                }
            }

            return status;
        }


        ///// <summary>
        ///// Get the data for a specific state.
        ///// </summary>
        ///// <param name="stateIndex"></param>
        ///// <param name="dataOffset"></param>
        ///// <param name="stateData"></param>
        ///// <returns></returns>
        ///// Assumuptions:  Called for Multi-threaded files only.
        /////
        //private bool getStateData(int fileIndex, int vChannelID, long stateIndex, ref byte[] stateData)
        //{
        //    bool status = true;

        //    // Determines which sequencial data file contains the requested state and assembles a file name
        //    string fileName = generateMultiThreadFileName(fileIndex, vChannelID);

        //    if (File.Exists(fileName))
        //    {
        //        List<DP14MSTMultiThread_MetaDataArgs> multiThreadFileMetaData = getMultiThreadFileDataRef(vChannelID);
        //        long stateOffset = (multiThreadFileMetaData[fileIndex].EndState - stateIndex) * TB_STATE_BYTE_LEN;

        //        // extract the state data from the specified file.
        //        using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        //        {               
        //            if (fs.Length > (multiThreadFileMetaData[fileIndex].DataOffset + stateOffset))
        //            {
        //                fs.Position = multiThreadFileMetaData[fileIndex].DataOffset + stateOffset;
        //                using (BinaryReader br = new BinaryReader(fs, new ASCIIEncoding()))
        //                {
        //                    Array.Copy(br.ReadBytes(TB_STATE_BYTE_LEN), stateData, stateData.Length);
        //                    status = true;
        //                }
        //            }
        //        }
        //    }

        //    return status;
        //}


        /// <summary>
        /// Get the file index containing the requested state index.
        /// </summary>
        /// <param name="stateIndex"></param>
        /// <returns></returns>
        private int getFileIndex(int vChannelID, long stateIndex)
        {
            bool status = true;
            int fileIndex = -1;
            string path = string.Empty;
            int fileStartIndex = 0x00;
            int fileEndIndex = 0x00;
            int dataOffset = 0x00;

            for (int i = 0; i < m_numOfDataFiles; i++)  
            {
                path = generateMultiThreadFileName(i, vChannelID);
                if (File.Exists(path))
                {
                    status = getMultiTrheadedStateIndices(path, ref fileStartIndex, ref fileEndIndex, ref dataOffset); 
                    if (status)
                    {
                        if ((stateIndex >= fileStartIndex) && (stateIndex <= fileEndIndex))
                        {
                            fileIndex = i;
                            break;
                        }
                    }
                }
            }

            return fileIndex;
        }


        /// <summary>
        /// Get the file index in which the given state Index is located within
        /// </summary>
        /// <param name="stateIndex"></param>
        /// <returns></returns>
        private int getFileIndex_inclusive(DP14MST_TRACE_BUFFER_MODE TBMode, long stateIndex, int vChannelID)
        {
            bool status = true;
            int fileIndex = -1;
            string path = string.Empty;
            int fileStartIndex = 0x00;
            int fileEndIndex = 0x00;
            int dataOffset = 0x00;
            //int numOfFiles = getNumberOfColumns(TBMode, m_memoryDepth);

            for (int i = 0; i < m_numOfDataFiles; i++)
            {
                path = generateMultiThreadFileName(i, vChannelID);
                if (File.Exists(path))
                {
                    status = getMultiTrheadedStateIndices(path, ref fileStartIndex, ref fileEndIndex, ref dataOffset);
                    if (status)
                    {
                        if ((stateIndex >= fileStartIndex) && (stateIndex <= fileEndIndex))
                        {
                            fileIndex = i;
                            break; // exit the for loop
                        }
                    }
                }
            }

            return fileIndex;
        }


        /// <summary>
        /// Get the start and end indices of a specified file.
        /// </summary>
        /// <param name="fileIndex"></param>
        /// <returns></returns>
        private bool getMultiTrheadedStateIndices(string path, ref int startStateIndex, ref int endStateIndex, ref int dataOffset)
        {
            bool status = false;
            int delimiterCount = 0;
            int parameterValue = 0x00;
            int parameterCount = 0x00;
            int triggerOffset = 0x00;


            dataOffset = 0x00;
            using (TextReader sr = File.OpenText(path))
            {
                string s = "";
                while (((s = sr.ReadLine()) != null) && (delimiterCount < 1))
                {
                    if (s.StartsWith("EndHdr"))
                        delimiterCount += 1;

                    // do it this way... will only process lines in-between the delimited lines.
                    if (delimiterCount == 0)
                    {
                        // get the line, parse it out
                        string[] comps = s.Split(new char[] { ':' });
                        if (int.TryParse(comps[1], out parameterValue))
                        {
                            if (comps[0] == BIN_FILE_MULTI_HEADER_START_STATE)
                            {
                                startStateIndex = parameterValue;
                                parameterCount += 1;
                            }
                            else if (comps[0] == BIN_FILE_MULTI_HEADER_END_STATE)
                            {
                                endStateIndex = parameterValue;
                                parameterCount += 1;
                            }
                            else if (comps[0] == BIN_FILE_MULTI_HEADER_INFO_TRIG_OFFSET)
                            {
                                triggerOffset = parameterValue;
                            }
                        }
                    }

                    dataOffset += s.Length + 1;
                }

                if (parameterCount == 2)
                    status = true;
            }

            return status;
        }



        /// <summary>
        /// Get the start and end indices of a specified file.
        /// </summary>
        /// <param name="fileIndex"></param>
        /// <returns></returns>
        private bool getMultiTrheadedStateIndices(string path, ref int startStateIndex, ref int endStateIndex, ref int triggerOffset, ref int dataOffset)
        {
            bool status = false;
            int delimiterCount = 0;
            int parameterValue = 0x00;
            int parameterCount = 0x00;

            dataOffset = 0x00;
            using (TextReader sr = File.OpenText(path))
            {
                string s = "";
                while (((s = sr.ReadLine()) != null) && (delimiterCount < 1))
                {
                    if (s.StartsWith("EndHdr"))
                        delimiterCount += 1;

                    // do it this way... will only process lines in-between the delimited lines.
                    if (delimiterCount == 0)
                    {
                        // get the line, parse it out
                        string[] comps = s.Split(new char[] { ':' });
                        if (int.TryParse(comps[1], out parameterValue))
                        {
                            if (comps[0] == BIN_FILE_MULTI_HEADER_START_STATE)
                            {
                                startStateIndex = parameterValue;
                                parameterCount += 1;
                            }
                            else if (comps[0] == BIN_FILE_MULTI_HEADER_END_STATE)
                            {
                                endStateIndex = parameterValue;
                                parameterCount += 1;
                            }
                            else if (comps[0] == BIN_FILE_MULTI_HEADER_INFO_TRIG_OFFSET)
                            {
                                triggerOffset = parameterValue;
                            }
                        }
                    }

                    dataOffset += s.Length + 1;
                }

                if (parameterCount == 2)
                    status = true;
            }

            return status;
        }



        /// <summary>
        /// Initialize the meta data for the captured data.
        /// </summary>
        /// <returns></returns>
        private List<DP14MSTVCFileGenerator_Threads.DP14MSTMultiThread_MetaDataArgs> initVChannelMetaData(int numOfDataFiles)
        {
            bool status = true;

            List<DP14MSTVCFileGenerator_Threads.DP14MSTMultiThread_MetaDataArgs> vChannelMetaData = new List<DP14MSTVCFileGenerator_Threads.DP14MSTMultiThread_MetaDataArgs>();
            string path = string.Empty;
            int delimiterCount = 0;
            int parameterValue = -1;
            int startState = -1;
            int triggerOffset = -1;


            for (int i = 0; i < numOfDataFiles / 4; i++)                    // 1 Millon states has 16 blocks, each with 4 channels ==> (16 * 4) = 64 files need to be processed
            {
                for (int channelID = 1; channelID <= 4; channelID++)        // each column should have four channels
                {
                    delimiterCount = 0;
                    parameterValue = -1;
                    startState = -1;
                    //path = Path.Combine(m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME, m_FS4500_MULTI_THREAD_TRACE_FILE_BASE_NAME + i.ToString() + "_VC_" + channelID.ToString() + ".bin");
                    path = Path.Combine(m_instanceFolderPath, m_FS4500_MULTI_THREAD_TRACE_FILE_BASE_NAME + i.ToString() + "_VC_" + channelID.ToString() + ".bin");
                    if (File.Exists(path))
                    {
                        // create an object to hold the meta data for the current file.
                        DP14MSTVCFileGenerator_Threads.DP14MSTMultiThread_MetaDataArgs metaDataArgs = new DP14MSTVCFileGenerator_Threads.DP14MSTMultiThread_MetaDataArgs();

                        // open the binary file and read all lines between the lines that begin with "*****"
                        using (TextReader sr = File.OpenText(path))
                        {
                            string s = "";
                            string s2 = "";
                            while (((s = sr.ReadLine()) != null) && (delimiterCount < 1) && (status == true))
                            {
                                if (s.StartsWith("EndHdr"))
                                {
                                    s2 = sr.ReadLine();
                                    if (s2 != null)
                                    {
                                        if (s2.StartsWith("*"))
                                            metaDataArgs.DataOffset += s2.Length + 1;
                                    }
                                    delimiterCount += 1;
                                }

                                // do it this way... will only process lines in-between the delimited lines.
                                if (delimiterCount == 0)
                                {
                                    // get the line, parse it out
                                    string[] comps = s.Split(new char[] { ':' });
                                    if (int.TryParse(comps[1], out parameterValue))
                                    {
                                        if (comps[0] == BIN_FILE_MULTI_HEADER_START_STATE)
                                        {
                                            //sb.Append("StartState:" + parameterValue.ToString() + ";");
                                            metaDataArgs.StartState = parameterValue;
                                            startState = parameterValue;
                                        }
                                        else if (comps[0] == BIN_FILE_MULTI_HEADER_END_STATE)
                                        {
                                            metaDataArgs.EndState = parameterValue;
                                        }
                                        else if (comps[0] == BIN_FILE_TRIG_VCHANNEL_ID)
                                        {
                                            metaDataArgs.TrigVChannelID = parameterValue;
                                        }
                                        else if (comps[0] == "TrigState")
                                        { // BIN_FILE_MULTI_HEADER_INFO_TRIG_OFFSET) {
                                            triggerOffset = parameterValue;
                                        }
                                        else if (comps[0] == BIN_FILE_VCHANNEL1_TRIG_INDEX)
                                        {
                                            metaDataArgs.VChannel1ID = parameterValue;
                                        }
                                        else if (comps[0] == BIN_FILE_VCHANNEL2_TRIG_INDEX)
                                        {
                                            metaDataArgs.VChannel2ID = parameterValue;
                                        }
                                        else if (comps[0] == BIN_FILE_VCHANNEL3_TRIG_INDEX)
                                        {
                                            metaDataArgs.VChannel3ID = parameterValue;
                                        }
                                        else if (comps[0] == BIN_FILE_VCHANNEL4_TRIG_INDEX)
                                        {
                                            metaDataArgs.VChannel4ID = parameterValue;
                                        }
                                        else {
                                            status = false;
                                        }
                                    }
                                }
                                metaDataArgs.DataOffset += s.Length + 1;
                            }

                            metaDataArgs.DataOffset = metaDataArgs.DataOffset + 1;
                            vChannelMetaData.Add(metaDataArgs);
                        }
                    }
                }
            }

            return vChannelMetaData;
        }


        ///// <summary>
        ///// Get the Trace buffer meta data that is spread across several files (multi-threaded data)
        ///// </summary>
        ///// <param name="msg"></param>
        ///// <returns></returns>
        ///// Assumption:  this method extracts the meta data for the data files for the specified channel, 
        /////              NOT the meta data for all data files...
        //private List<DP14MSTMultiThread_MetaDataArgs> initVChannelMetaData(int vChannelID, List<DP14MSTMultiThread_MetaDataArgs> vChannelMetaData)
        //{
        //    bool status = true;
        //    string path = string.Empty;
        //    int delimiterCount = 0;
        //    int parameterValue = -1;
        //    int startState = -1;
        //    int triggerOffset = -1;
        //    bool locatedFile = true;

        //    vChannelMetaData.Clear();
        //    for (int columnID = 0; locatedFile && (columnID < m_numOfTaskColumns); columnID++)
        //    {
        //        delimiterCount = 0;
        //        parameterValue = -1;
        //        startState = -1;
        //        path = generateMultiThreadFileName(columnID, vChannelID);

        //        if (File.Exists(path))
        //        {
        //            // create an object to hold the meta data for the current file.
        //            DP14MSTMultiThread_MetaDataArgs metaDataArgs = new DP14MSTMultiThread_MetaDataArgs();

        //            StringBuilder sb = new StringBuilder();
        //            sb.Append("FileIndex:" + columnID.ToString() + ";");

        //            // open the binary file and read all lines between the lines that begin with "*****"
        //            using (TextReader sr = File.OpenText(path))
        //            {
        //                sb.Length = 0;
        //                string s = "";
        //                string s2 = "";
        //                while (((s = sr.ReadLine()) != null) && (delimiterCount < 1) && (status == true))
        //                {
        //                    if (s.StartsWith("EndHdr"))
        //                    {
        //                        s2 = sr.ReadLine();
        //                        if (s2 != null)
        //                        {
        //                            if (s2.StartsWith("*"))
        //                                metaDataArgs.DataOffset += s2.Length + 1;
        //                        }
        //                        delimiterCount += 1;
        //                    }

        //                    // do it this way... will only process lines in-between the delimited lines.
        //                    if (delimiterCount == 0)
        //                    {
        //                        // get the line, parse it out
        //                        string[] comps = s.Split(new char[] { ':' });
        //                        if (int.TryParse(comps[1], out parameterValue))
        //                        {
        //                            if (comps[0] == BIN_FILE_MULTI_HEADER_START_STATE)
        //                            {
        //                                //sb.Append("StartState:" + parameterValue.ToString() + ";");
        //                                metaDataArgs.StartState = parameterValue;
        //                                startState = parameterValue;
        //                            }
        //                            else if (comps[0] == BIN_FILE_MULTI_HEADER_END_STATE)
        //                            {
        //                                metaDataArgs.EndState = parameterValue;
        //                            }
        //                            else if (comps[0] == BIN_FILE_TRIG_VCHANNEL_ID)
        //                            {
        //                                metaDataArgs.TrigVChannelID = parameterValue;
        //                            }
        //                            else if (comps[0] == "TrigState")
        //                            { // BIN_FILE_MULTI_HEADER_INFO_TRIG_OFFSET) {
        //                                triggerOffset = parameterValue;
        //                            }
        //                            else if (comps[0] == BIN_FILE_VCHANNEL1_TRIG_INDEX)
        //                            {
        //                                metaDataArgs.VChannel1ID = parameterValue;
        //                            }
        //                            else if (comps[0] == BIN_FILE_VCHANNEL2_TRIG_INDEX)
        //                            {
        //                                metaDataArgs.VChannel2ID = parameterValue;
        //                            }
        //                            else if (comps[0] == BIN_FILE_VCHANNEL3_TRIG_INDEX)
        //                            {
        //                                metaDataArgs.VChannel3ID = parameterValue;
        //                            }
        //                            else if (comps[0] == BIN_FILE_VCHANNEL4_TRIG_INDEX)
        //                            {
        //                                metaDataArgs.VChannel4ID = parameterValue;
        //                            }
        //                            else {
        //                                status = false;
        //                            }
        //                        }
        //                    }
        //                    metaDataArgs.DataOffset += s.Length + 1;
        //                }

        //                metaDataArgs.DataOffset = metaDataArgs.DataOffset + 1;
        //                vChannelMetaData.Add(metaDataArgs);
        //            }
        //        }
        //    }

        //    return vChannelMetaData;
        //}


        ///// <summary>
        ///// initialize the meta data assoicated with a set of captured data files.
        ///// </summary>
        ///// NOTE:  m_multiThreadFileMetaData_VC1 documents the column (cascading tasks)
        /////        start and end states for virtual channel 1.  
        /////        
        /////        The same idea is repeated for all four virtual channels.
        //private void initalizeDataFilesMetaData()
        //{
        //    for (int i = 1; i <= 4; i++)
        //    {
        //        switch (i)
        //        {
        //            case 1:
        //                initVChannelMetaData(i, m_multiThreadFileMetaData_VC1);
        //                break;
        //            case 2:
        //                initVChannelMetaData(i, m_multiThreadFileMetaData_VC2);
        //                break;
        //            case 3:
        //                initVChannelMetaData(i, m_multiThreadFileMetaData_VC3);
        //                break;
        //            case 4:
        //                initVChannelMetaData(i, m_multiThreadFileMetaData_VC4);
        //                break;

        //            default:
        //                break;
        //        }
        //    }
        //}


        /// <summary>
        /// Get the Trace buffer meta data that is spread across several files (multi-threaded data)
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        /// Assumption:  this method extracts the meta data for the data files for the specified channel, 
        ///              NOT the meta data for all data files...
        private bool getMultiThreadedFileMetaData(DP14MSTMessage_GetTBBinFileMetaData msg)
        {
            bool status = true;
            string path = string.Empty;
            int delimiterCount = 0;
            int parameterValue = -1;
            int startState = -1;
            int endState = -1;
            int totalStates = 0;
            int triggerOffset = -1;
            int vChannelID = -1;
            int vChannel1TrigIndex = -1;
            int vChannel2TrigIndex = -1;
            int vChannel3TrigIndex = -1;
            int vChannel4TrigIndex = -1;



            //for (int columnID = 0; locatedFile && (columnID < m_numOfTaskColumns); columnID++)
            for (int blockID = 0; blockID < (m_numOfDataFiles / 4); blockID++)
            {
                delimiterCount = 0;
                parameterValue = -1;
                startState = -1;
                endState = -1;
                path = generateMultiThreadFileName(blockID, msg.VChannelID);

                if (File.Exists(path))
                {
                    // create an object to hold the meta data for the current file.
                    DP14MSTVCFileGenerator_Threads.DP14MSTMultiThread_MetaDataArgs metaDataArgs = new DP14MSTVCFileGenerator_Threads.DP14MSTMultiThread_MetaDataArgs();

                    // open the binary file and read all lines between the lines that begin with "*****"
                    using (TextReader sr = File.OpenText(path))
                    {
                        string s = "";
                        string s2 = "";
                        while (((s = sr.ReadLine()) != null) && (delimiterCount < 1) && (status == true))
                        {
                            if (s.StartsWith("EndHdr"))
                            {
                                s2 = sr.ReadLine();
                                if (s2 != null)
                                {
                                    if (s2.StartsWith("*"))
                                        metaDataArgs.DataOffset += s2.Length + 1;
                                }
                                delimiterCount += 1;
                            }

                            // do it this way... will only process lines in-between the delimited lines.
                            if (delimiterCount == 0)
                            {
                                // get the line, parse it out
                                string[] comps = s.Split(new char[] { ':' });
                                if (int.TryParse(comps[1], out parameterValue))
                                {
                                    if (comps[0] == BIN_FILE_MULTI_HEADER_START_STATE)
                                    {
                                        //sb.Append("StartState:" + parameterValue.ToString() + ";");
                                        metaDataArgs.StartState = parameterValue;
                                        startState = parameterValue;
                                    }
                                    else if (comps[0] == BIN_FILE_MULTI_HEADER_END_STATE)
                                    {
                                        //sb.Append("EndState:" + parameterValue.ToString() + ";");
                                        metaDataArgs.EndState = parameterValue;
                                        endState = parameterValue;
                                    }
                                    else if (comps[0] == BIN_FILE_TRIG_VCHANNEL_ID)
                                    {
                                        metaDataArgs.TrigVChannelID = parameterValue;
                                        vChannelID = parameterValue;
                                    }
                                    else if (comps[0] == "TrigState") // BIN_FILE_MULTI_HEADER_INFO_TRIG_OFFSET)
                                    {
                                        triggerOffset = parameterValue;
                                    }
                                    else if (comps[0] == BIN_FILE_VCHANNEL1_TRIG_INDEX)
                                    {
                                        metaDataArgs.VChannel1ID = parameterValue;
                                        vChannel1TrigIndex = parameterValue;
                                    }
                                    else if (comps[0] == BIN_FILE_VCHANNEL2_TRIG_INDEX)
                                    {
                                        metaDataArgs.VChannel2ID = parameterValue;
                                        vChannel2TrigIndex = parameterValue;
                                    }
                                    else if (comps[0] == BIN_FILE_VCHANNEL3_TRIG_INDEX)
                                    {
                                        metaDataArgs.VChannel3ID = parameterValue;
                                        vChannel3TrigIndex = parameterValue;
                                    }
                                    else if (comps[0] == BIN_FILE_VCHANNEL4_TRIG_INDEX)
                                    {
                                        metaDataArgs.VChannel4ID = parameterValue;
                                        vChannel4TrigIndex = parameterValue;
                                    }
                                    else
                                    {
                                        status = false;
                                    }
                                }
                                //dataOffset += s.Length + 1;
                            }
                            metaDataArgs.DataOffset += s.Length + 1;
                        }

                        metaDataArgs.DataOffset = metaDataArgs.DataOffset + 1;

                        // store the object holding the meta data into a list... should be sequencial.
                        //m_multiThreadFileMetaData.Add(metaDataArgs);

                        // add the dataOffset as the last data point as it is dependent
                        // on the length of all of the meta data values.
                        // sb.Append("DataOffset:" + dataOffset.ToString());

                        // add the file meta data to a list (one entry per file)
                        //msg.MultiThreadedMetaData.Add(sb.ToString());


                        // calculate the total number of states and store the value in the msg args 
                        totalStates += endState - startState + 1;
                        //msg.NumberOfStates = totalStates;

                        // all of the files should have had the same trigger offset... strore
                        // the value into the msg args
                        //msg.TriggerOffset = triggerOffset;
                    }
                }
                else
                {
                    break; // exit the loop as we are done! // locatedFile = false;
                }
            }

            // add the global meta data parameters
            msg.TriggerOffset = triggerOffset;
            msg.NumberOfStates = totalStates;
            msg.VChannelID = vChannelID;

            msg.VChannel1TrigIndex = vChannel1TrigIndex;
            msg.VChannel2TrigIndex = vChannel2TrigIndex;
            msg.VChannel3TrigIndex = vChannel3TrigIndex;
            msg.VChannel4TrigIndex = vChannel4TrigIndex;

            return status;
        }


        /// <summary>
        /// Extract the binary file's meta data (located at the beginning of the file).
        /// </summary>
        /// <param name="path"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool getSingleThreadedFileMetaData(string path, DP14MSTMessage_GetTBBinFileMetaData msg)
        {
            bool status = true;
            int delimiterCount = 0;
            int parameterValue = -1;


            string filePath = m_traceFilePath;
            if (msg.TBMode == DP14MST_TRACE_BUFFER_MODE.AuxiliaryLink)
                filePath = m_auxTraceFilePath;

            m_dataOffset_mainLink = 0;
            m_dataOffset_auxLink = 0;


            if (File.Exists(filePath))
            {
                //    // open the binary file and read all lines between the lines that begin with "*****"
                using (TextReader sr = File.OpenText(path))
                {
                    string s = String.Empty;
                    while (((s = sr.ReadLine()) != null) && (delimiterCount < 1) && (status == true))
                    {
                        if (s.StartsWith("*****"))
                            delimiterCount += 1;

                        // do it this way... will only process lines in-between the delimited lines.
                        if (delimiterCount == 0)
                        {
                            // get the line, parse is out
                            string[] comps = s.Split(new char[] { ':' });
                            if (int.TryParse(comps[1], out parameterValue))
                            {
                                if (comps[0] == BIN_FILE_HEADER_INFO_NUM_OF_STATES)
                                    msg.NumberOfStates = parameterValue;
                                else if (comps[0] == BIN_FILE_HEADER_INFO_TRIG_OFFSET)
                                    msg.TriggerOffset = parameterValue;
                                else
                                    status = false;
                            }
                        }

                        //msg.DataType = TBDataType.MainLinkSingleThread;
                        //msg.DataOffset += s.Length + 1;

                        if (msg.TBMode == DP14MST_TRACE_BUFFER_MODE.MainLink)
                            m_dataOffset_mainLink += s.Length + 1;
                        else
                            m_dataOffset_auxLink += s.Length + 1;
                    }
                }
            }

            return status;
        }

        /// <summary>
        /// Extract the binary file's meta data (located at the beginning of the file).
        /// </summary>
        /// <param name="path"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool getSingleThreadedFileMetaData(string path, ref int numOfStates, ref int triggerOffset, ref int dataOffset)
        {
            bool status = true;
            int delimiterCount = 0;
            int parameterValue = -1;


            if (File.Exists(path))
            {
                //    // open the binary file and read all lines between the lines that begin with "*****"
                using (TextReader sr = File.OpenText(path))
                {
                    string s = String.Empty;
                    while (((s = sr.ReadLine()) != null) && (delimiterCount < 1) && (status == true))
                    {
                        if (s.StartsWith("*****"))
                            delimiterCount += 1;

                        // do it this way... will only process lines in-between the delimited lines.
                        if (delimiterCount == 0)
                        {
                            // get the line, parse is out
                            string[] comps = s.Split(new char[] { ':' });
                            if (int.TryParse(comps[1], out parameterValue))
                            {
                                if (comps[0] == BIN_FILE_HEADER_INFO_NUM_OF_STATES)
                                    numOfStates = parameterValue;
                                else if (comps[0] == BIN_FILE_HEADER_INFO_TRIG_OFFSET)
                                    triggerOffset = parameterValue;
                                else if (comps[0] == BIN_FILE_HEADER_INFO_TRIG_OFFSET)
                                    dataOffset = parameterValue;
                                else
                                    status = false;
                            }
                        }

                        //msg.DataType = TBDataType.MainLinkSingleThread;
                        //msg.DataOffset += s.Length + 1;

                        dataOffset += s.Length + 1;
                    }
                }
            }

            return status;
        }


        /// <summary>
        ///  Generate a file path for a specified multi-threaded Trace Buffer binary file.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private string generateMultiThreadFileName(int index, int vChannelID)
        {
            //return (Path.Combine(m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME, m_FS4500_MULTI_THREAD_TRACE_FILE_BASE_NAME + index.ToString() + "_VC_" + vChannelID.ToString() + ".bin"));
            return (Path.Combine(m_instanceFolderPath, m_FS4500_MULTI_THREAD_TRACE_FILE_BASE_NAME + index.ToString() + "_VC_" + vChannelID.ToString() + ".bin"));
        }


        /// <summary>
        /// Determine the type of data contained in the TB data file(s)
        /// </summary>
        /// <returns></returns>
        private DP14MSTTBDataMgrMode getMainLinkTBDataFormat()
        {
           DP14MSTTBDataMgrMode mode = DP14MSTTBDataMgrMode.Unknown;

            if (File.Exists(m_traceFilePath))
            {
                mode = DP14MSTTBDataMgrMode.SingleThreaded;
            }
            else
            {
                // generate the file name that would be used for the first of several binary files containing the trace buffer data
                string path = generateMultiThreadFileName(0, DEFAULT_VCHANNEL_ID);
                if (File.Exists(path))
                    mode = DP14MSTTBDataMgrMode.MultiThreaded;
            }

            return mode;
        }


        /// <summary>
        /// Get the meta data in the header section of the binary trace file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool getTBMetaData(DP14MSTMessage DisplayPortEvent)
        {
            bool status = true;

            if (((DP14MSTMessage_GetTBBinFileMetaData)DisplayPortEvent).TBMode == DP14MST_TRACE_BUFFER_MODE.MainLink)
            {
                m_TBDataMode = getMainLinkTBDataFormat();
                if (m_TBDataMode == DP14MSTTBDataMgrMode.SingleThreaded)
                {
                    status = getSingleThreadedFileMetaData(m_traceFilePath, ((DP14MSTMessage_GetTBBinFileMetaData)DisplayPortEvent));
                }
                else if (m_TBDataMode == DP14MSTTBDataMgrMode.MultiThreaded)
                {
                    status = getMultiThreadedFileMetaData((DP14MSTMessage_GetTBBinFileMetaData)DisplayPortEvent);
                }
            }
            else if (((DP14MSTMessage_GetTBBinFileMetaData)DisplayPortEvent).TBMode == DP14MST_TRACE_BUFFER_MODE.AuxiliaryLink)
            {
                getSingleThreadedFileMetaData(m_auxTraceFilePath, ((DP14MSTMessage_GetTBBinFileMetaData)DisplayPortEvent));
            }

            return status;
        }


        /// <summary>
        /// Extract a field value at the specified location in the given state data 
        /// </summary>
        /// <param name="fieldOffset"></param>
        /// <param name="FieldWidth"></param>
        /// <param name="dataBytes"></param>
        /// <returns></returns>
        public long GetLoopFieldData(int fieldOffset, int fieldWidth, byte[] dataBytes)
        {
            long fldValue = -1;
            int byteID = -1;
            int bitID = -1;

            // get the location, in terms of byte ID and Byte bit ID, of the MSBit of the field
            LoopOperations.GetFieldLocation(dataBytes.Length, fieldOffset, ref byteID, ref bitID);
            fldValue = LoopOperations.GetFieldValue(byteID, bitID, fieldWidth, dataBytes);

            return fldValue;
        }


        /// <summary>
        /// Get the column / field meta data.
        /// </summary>
        /// <param name="TBMode"></param>
        /// <param name="fieldName"></param>
        /// <param name="width"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private bool getFieldParameters(DP14MST_TRACE_BUFFER_MODE TBMode, string fieldName, ref int width, ref int offset)
        {
            bool status = false;

            List<DP14MSTColumnMetaData> metaDataList = m_columnMetaData_MainLink;
            if (TBMode == DP14MST_TRACE_BUFFER_MODE.AuxiliaryLink)
                metaDataList = m_columnMetaData_AuxiliaryLink;


            foreach (DP14MSTColumnMetaData metaData in metaDataList)
            {
                if (metaData.ColumnName == fieldName)
                {
                    width = metaData.ColumnWidth;
                    offset = metaData.ColumnOffset;
                    status = true;
                    break;
                }
            }

            return status;
        }


        /// <summary>
        /// Search multiple threaded binary file(s) for the given column value.
        /// </summary>
        /// <param name="searchMsg"></param>
        /// <param name="locatedStateIndex"></param>
        /// <returns></returns>
        private bool searchListing_SingleThead(DP14MSTMessage_SearchListingRequest searchMsg, ref long locatedStateIndex)
        {
            bool found = false;
            //bool searchForward = true;

            //if (searchMsg.PreviousStates)
            //    searchForward = false;

            //DP14MSTTBSearchDescriptor threadParameters = new DP14MSTTBSearchDescriptor(searchMsg.TBMode, searchMsg.ColumnName, searchMsg.m_ColumnHexValue, 
            //                                                                                searchMsg.VChannelID, searchMsg.StateIndex, searchForward);
            ////initalize the background worker thread and start the background worker
            //initSearchBGWorker_SingleThreaded(threadParameters);

            return found;
        }


        /// <summary>
        /// Search multiple threaded binary file(s) for the given column value.
        /// </summary>
        /// <param name="searchMsg"></param>
        /// <param name="locatedStateIndex"></param>
        /// <returns></returns>
        private bool searchListing_MultiThead(DP14MSTMessage_SearchListingRequest searchMsg, ref long locatedStateIndex)
        {
            bool found = false;
            //bool searchForward = true;

            //if (searchMsg.PreviousStates)
            //    searchForward = false;

            //DP14MSTTBSearchDescriptor threadParameters = new DP14MSTTBSearchDescriptor(searchMsg.TBMode, searchMsg.ColumnName, searchMsg.m_ColumnHexValue, 
            //                                                                                searchMsg.VChannelID, searchMsg.StateIndex, searchForward);
            ////initalize the background worker thread and start the background worker
            //initSearchBGWorker_MultiThreaded(threadParameters);


            ////if (!searchMsg.PreviouMSTates)
            ////    found = SearchListing_MultiThread_Forward(searchMsg, ref locatedStateIndex);
            ////else
            ////    found = SearchListing_MultiThread_Backward(searchMsg, ref locatedStateIndex);

            return found;
        }


        /// <summary>
        /// Search forward through the listing for a specific column value.
        /// </summary>
        /// <param name="searchMsg"></param>
        /// <param name="locatedStateIndex"></param>
        /// <returns></returns>
        private long searchListing_MultiThread_Forward(DP14MST_TRACE_BUFFER_MODE TBMode, string cName, uint cValue, int vChannelID, long stateIndex, int interval = 1024)
        {
            bool status = false;
            bool found = false;
            int fldWidth = 0x00;
            int fldOffset = 0x00;
            long curStateIndex = stateIndex; // searchMsg.StateIndex;
            long locatedStateIndex = -1;
            byte[] stateData = new byte[TB_STATE_BYTE_LEN];
            string path = string.Empty;
            int startState = 0x00;
            int endState = 0x00;
            int dataOffset = 0x00;
            int byteID = 0x00;
            int bitID = 0x00;
            uint fldValue = 0x00;
            long count = 0;
            //int numOfFiles = getNumberOfColumns(TBMode, m_memoryDepth);


            // figure out which file the starting stateindex is in.
            int fileIndex = getFileIndex(vChannelID, curStateIndex);
            if (fileIndex >= 0)
            {
                //status = getFieldParameters(TRACE_BUFFER_MODE.MainLink, searchMsg.ColumnName, ref fldWidth, ref fldOffset);
                status = getFieldParameters(DP14MST_TRACE_BUFFER_MODE.MainLink, cName, ref fldWidth, ref fldOffset);
                if (status)
                    status = LoopOperations.GetFieldLocation(TB_STATE_BYTE_LEN, fldOffset, ref byteID, ref bitID);
            }



            // search the files beginning with the given file assoicated with the fileIndex value.  
            //for (int index = fileIndex; index < m_numOfTPITasks && !found; index++)
            for (int index = fileIndex; index < m_numOfDataFiles && !found; index++)
            {
                path = generateMultiThreadFileName(index, vChannelID);
                if (File.Exists(path))
                {
                    status = getMultiTrheadedStateIndices(path, ref startState, ref endState, ref dataOffset);
                    if (status)
                    {
                        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                        {
                            //loop until found or end of file is encountered
                            using (BinaryReader br = new BinaryReader(fs, new ASCIIEncoding()))
                            {
                                //while (!found && (fs.Length > (dataOffset + (curStateIndex * TB_STATE_BYTE_LEN))))
                                while ((curStateIndex <= endState)) //  &&  (fs.Length > (dataOffset + (curStateIndex * TB_STATE_BYTE_LEN))))
                                {
                                    Array.Clear(stateData, 0, stateData.Length);
                                    fs.Position = dataOffset + ((curStateIndex - startState) * TB_STATE_BYTE_LEN);
                                    Array.Copy(br.ReadBytes(TB_STATE_BYTE_LEN), stateData, stateData.Length);

                                    //if (searchMsg.m_ColumnHexValue == GetLoopFieldData(fldOffset, fldWidth, stateData))
                                    fldValue = LoopOperations.GetFieldValueII(byteID, bitID, fldWidth, stateData);
                                    //if (searchMsg.m_ColumnHexValue == fldValue)
                                    if (cValue == fldValue)
                                    {
                                        found = true;
                                        locatedStateIndex = curStateIndex;
                                        break;
                                    }
                                    else
                                    {
                                        //curStateIndex += 1;
                                        count += 1;
                                        curStateIndex += 1;
                                        if ((count % interval) == 0)
                                        {
                                            m_searchWorker_MultiThread.ReportProgress(0);       // we don't care about the percentage
                                            count = 0;                                          // the calling form will update the progress bar base on interval size...
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return locatedStateIndex;
        }


        /// <summary>
        /// Search backward through the listing for a specific column value.
        /// </summary>
        /// <param name="searchMsg"></param>
        /// <param name="locatedStateIndex"></param>
        /// <returns></returns>
        private long searchListing_MultiThread_Backward(DP14MST_TRACE_BUFFER_MODE TBMode, string cName, uint cValue, int vChannelID, long stateIndex, int interval = 1024)
        {
            bool status = false;
            bool found = false;
            int fldWidth = 0x00;
            int fldOffset = 0x00;
            long curStateIndex = stateIndex; //searchMsg.StateIndex;
            long locatedStateIndex = -1;
            byte[] stateData = new byte[TB_STATE_BYTE_LEN];
            string path = string.Empty;
            int startState = 0x00;
            int endState = 0x00;
            int dataOffset = 0x00;
            int byteID = 0x00;
            int bitID = 0x00;
            uint fldValue = 0x00;
            long count = 0;
            
            // figure out which file the starting stateindex is in.
            int fileIndex = getFileIndex_inclusive(TBMode, curStateIndex, vChannelID); 
            if (fileIndex >= 0)
            {
                //status = getFieldParameters(TRACE_BUFFER_MODE.MainLink, searchMsg.ColumnName, ref fldWidth, ref fldOffset);
                status = getFieldParameters(DP14MST_TRACE_BUFFER_MODE.MainLink, cName, ref fldWidth, ref fldOffset);
                if (status)
                    status = LoopOperations.GetFieldLocation(TB_STATE_BYTE_LEN, fldOffset, ref byteID, ref bitID);
            }



            // search the files beginning with the given file assoicated with the fileIndex value.  
            for (int index = fileIndex; (index >= 0) && !found; index--)
            {
                path = generateMultiThreadFileName(index, vChannelID);
                status = getMultiTrheadedStateIndices(path, ref startState, ref endState, ref dataOffset);
                if (status)
                {
                    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        //loop until found or end of file is encountered
                        using (BinaryReader br = new BinaryReader(fs, new ASCIIEncoding()))
                        {
                            while (curStateIndex >= startState) // &&  fs.Length > (dataOffset + (curStateIndex * TB_STATE_BYTE_LEN)))
                            {
                                Array.Clear(stateData, 0, stateData.Length);
                                fs.Position = dataOffset + ((curStateIndex - startState) * TB_STATE_BYTE_LEN);
                                Array.Copy(br.ReadBytes(TB_STATE_BYTE_LEN), stateData, stateData.Length);

                                //if (searchMsg.m_ColumnHexValue == GetLoopFieldData(fldOffset, fldWidth, stateData))
                                fldValue = LoopOperations.GetFieldValueII(byteID, bitID, fldWidth, stateData);
                                //if (searchMsg.m_ColumnHexValue == fldValue)
                                if (cValue == fldValue)
                                {
                                    found = true;
                                    locatedStateIndex = curStateIndex;
                                    break;
                                }
                                else
                                {
                                    count += 1;
                                    curStateIndex -= 1;
                                    if ((count % interval) == 0)
                                    {
                                        m_searchWorker_MultiThread.ReportProgress(0);  // we don't care about the percentage
                                        count = 0;                         // the calling form will update the progress bar base on interval size...
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return locatedStateIndex;
        }


        /// <summary>
        /// Search looking forward from a given state index.
        /// </summary>
        /// <param name="searchMsg"></param>
        /// <param name="locatedStateIndex"></param>
        /// <returns></returns>
        // private bool searchListing_SingleThead_Forward(DPMessage_SearchListingRequest searchMsg, ref long locatedStateIndex)
        private long searchListing_SingleThread_Forward(DP14MST_TRACE_BUFFER_MODE TBMode, string cName, uint cValue, long stateIndex, int interval = 1024)
        {
            bool found = false;
            int fldWidth = 0x00;
            int fldOffset = 0x00;
            long curStateIndex = stateIndex; // searchMsg.StateIndex;
            long locatedStateIndex = -1;
            byte[] stateData = new byte[TB_STATE_BYTE_LEN];

            // meta data 
            int numOfStates = 0x00;
            int triggerOffset = 0x00;
            int dataOffset = 0x00;
            long count = 0;

            string filePath = m_traceFilePath;
            DP14MST_TRACE_BUFFER_MODE traceBufferMode = DP14MST_TRACE_BUFFER_MODE.MainLink;
            //if (searchMsg.TBMode == TRACE_BUFFER_MODE.AuxiliaryLink)
            if (TBMode == DP14MST_TRACE_BUFFER_MODE.AuxiliaryLink)
            {
                filePath = m_auxTraceFilePath;
                traceBufferMode = DP14MST_TRACE_BUFFER_MODE.AuxiliaryLink;
            }


            if (File.Exists(filePath))
            {
                bool status = getSingleThreadedFileMetaData(filePath, ref numOfStates, ref triggerOffset, ref dataOffset);
                if (status)
                {
                    //if (getFieldParameters(traceBufferMode, searchMsg.ColumnName, ref fldWidth, ref fldOffset))
                    if (getFieldParameters(traceBufferMode, cName, ref fldWidth, ref fldOffset))
                    {
                        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            //loop until found or end of file is encountered
                            using (BinaryReader br = new BinaryReader(fs, new ASCIIEncoding()))
                            {
                                while (!found && (fs.Length > (dataOffset + (curStateIndex * TB_STATE_BYTE_LEN))))
                                {
                                    Array.Clear(stateData, 0, stateData.Length);
                                    fs.Position = dataOffset + (curStateIndex * TB_STATE_BYTE_LEN);
                                    Array.Copy(br.ReadBytes(TB_STATE_BYTE_LEN), stateData, stateData.Length);

                                    //if (searchMsg.m_ColumnHexValue == GetLoopFieldData(fldOffset, fldWidth, stateData))
                                    if (cValue == GetLoopFieldData(fldOffset, fldWidth, stateData))
                                    {
                                        found = true;
                                        locatedStateIndex = curStateIndex;
                                        break;
                                    }
                                    else
                                    {
                                        count += 1;
                                        curStateIndex += 1;

                                        if ((count % interval) == 0)
                                        {
                                            m_searchWorker_SingleThread.ReportProgress(0);  // we don't care about the percentage
                                            count = 0;                         // the calling form will update the progress bar base on interval size...
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return locatedStateIndex;
        }


        /// <summary>
        /// Search looking forward from a given state index.
        /// </summary>
        /// <param name="searchMsg"></param>
        /// <param name="locatedStateIndex"></param>
        /// <returns></returns>
        //private bool searchListing_SingleThead_Backward(DPMessage_SearchListingRequest searchMsg, ref long locatedStateIndex)
        private long searchListing_SingleThread_Backward(DP14MST_TRACE_BUFFER_MODE TBMode, string cName, uint cValue, long stateIndex, int interval = 1024)
        {
            bool found = false;
            int fldWidth = 0x00;
            int fldOffset = 0x00;
            long curStateIndex = stateIndex; // searchMsg.StateIndex;
            long locatedStateIndex = -1;
            byte[] stateData = new byte[TB_STATE_BYTE_LEN];

            // meta data 
            int numOfStates = 0x00;
            int triggerOffset = 0x00;
            int dataOffset = 0x00;
            long count = 0;

            string filePath = m_traceFilePath;
            DP14MST_TRACE_BUFFER_MODE traceBufferMode = DP14MST_TRACE_BUFFER_MODE.MainLink;
            //if (searchMsg.TBMode == TRACE_BUFFER_MODE.AuxiliaryLink)
            if (TBMode == DP14MST_TRACE_BUFFER_MODE.AuxiliaryLink)
            {
                filePath = m_auxTraceFilePath;
                traceBufferMode = DP14MST_TRACE_BUFFER_MODE.AuxiliaryLink;
            }


            if (File.Exists(filePath))
            {
                bool status = getSingleThreadedFileMetaData(filePath, ref numOfStates, ref triggerOffset, ref dataOffset);
                if (status)
                {
                    //if (getFieldParameters(traceBufferMode, searchMsg.ColumnName, ref fldWidth, ref fldOffset))
                    if (getFieldParameters(traceBufferMode, cName, ref fldWidth, ref fldOffset))
                    {
                        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            //loop until found or end of file is encountered
                            using (BinaryReader br = new BinaryReader(fs, new ASCIIEncoding()))
                            {
                                while (!found && (curStateIndex >= 0) && (fs.Length > (dataOffset + (curStateIndex * TB_STATE_BYTE_LEN))))
                                {
                                    Array.Clear(stateData, 0, stateData.Length);
                                    fs.Position = dataOffset + (curStateIndex * TB_STATE_BYTE_LEN);
                                    Array.Copy(br.ReadBytes(TB_STATE_BYTE_LEN), stateData, stateData.Length);

                                    //if (searchMsg.m_ColumnHexValue == GetLoopFieldData(fldOffset, fldWidth, stateData))
                                    if (cValue == GetLoopFieldData(fldOffset, fldWidth, stateData))
                                    {
                                        found = true;
                                        locatedStateIndex = curStateIndex;
                                        break;
                                    }
                                    else
                                    {
                                        //curStateIndex -= 1;
                                        count += 1;
                                        curStateIndex -= 1;

                                        if ((count % interval) == 0)
                                        {
                                            m_searchWorker_SingleThread.ReportProgress(0);  // we don't care about the percentage
                                            count = 0;                         // the calling form will update the progress bar base on interval size...
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return locatedStateIndex; // found;
        }


        /// <summary>
        /// Search the trace buffer data 
        /// </summary>
        /// <param name="DisplayPortEvent"></param>
        /// <returns></returns>
        private bool searchTraceBufferData(DP14MSTMessage DisplayPortEvent)
        {
            bool found = false;
            long stateIndex = -1;

            if (DisplayPortEvent is DP14MSTMessage_SearchListingRequest)
            {
                DP14MSTMessage_SearchListingRequest searchMsg = DisplayPortEvent as DP14MSTMessage_SearchListingRequest;
                if (searchMsg.TBMode == DP14MST_TRACE_BUFFER_MODE.MainLink)
                {
                    if (m_TBDataMode == DP14MSTTBDataMgrMode.SingleThreaded)
                    {
                        m_MainLinkSingleThreadSearch = true;
                        found = searchListing_SingleThead(searchMsg, ref stateIndex);
                    }
                    else if (m_TBDataMode == DP14MSTTBDataMgrMode.MultiThreaded)
                    {
                        m_MainLinkSingleThreadSearch = false;
                        found = searchListing_MultiThead(searchMsg, ref stateIndex);
                    }
                    else
                        found = false;
                }
                else if (searchMsg.TBMode == DP14MST_TRACE_BUFFER_MODE.AuxiliaryLink)
                {
                    m_MainLinkSingleThreadSearch = false;
                    found = searchListing_SingleThead(searchMsg, ref stateIndex);
                }
                else
                {
                    found = false;
                }

                if (found)
                {
                    searchMsg.MatchLocation = stateIndex;
                    searchMsg.MatchLocated = found;
                }
            }

            return found;
        }

        /// <summary>
        /// Cancel a search request
        /// </summary>
        private void cancelSearch()
        {
            // cancel the on-going search that is on the Background Work Thread

            // raise and response event 
        }


        /// <summary>
        /// Remove the trace data binary files on request.
        /// </summary>
        /// <param name="DisplayPortEvent"></param>
        /// <returns></returns>
        private bool removeDataFiles(DP14MST_TRACE_BUFFER_MODE TBModeID)
        {
            bool status = true;
            int maxRetries = 10;
            for (int attempt = 0; attempt < 5; attempt++)
            {
                try
                {
                    if (TBModeID == DP14MST_TRACE_BUFFER_MODE.MainLink)
                    {
                        //DirectoryInfo di = new DirectoryInfo(m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME);
                        DirectoryInfo di = new DirectoryInfo(m_instanceFolderPath);
                        FileInfo[] dFiles = di.GetFiles("TraceData*.bin");
                        foreach (FileInfo file in dFiles)
                            file.Delete();

                        dFiles = di.GetFiles("VChannel*.bin");
                        foreach (FileInfo file in dFiles)
                            file.Delete();
                    }
                    else if (TBModeID == DP14MST_TRACE_BUFFER_MODE.AuxiliaryLink)
                    {
                        if (File.Exists(m_auxTraceFilePath))
                            File.Delete(m_auxTraceFilePath);
                    }
                    else if (TBModeID == DP14MST_TRACE_BUFFER_MODE.Both)
                    {
                        // remove main link files (if they exist)
                        //DirectoryInfo di = new DirectoryInfo(m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME);
                        DirectoryInfo di = new DirectoryInfo(m_instanceFolderPath);
                        FileInfo[] dFiles = di.GetFiles("TraceData*.bin");
                        foreach (FileInfo file in dFiles)
                            file.Delete();

                        dFiles = di.GetFiles("VChannel*.bin");
                        foreach (FileInfo file in dFiles)
                            file.Delete();


                        // remove auxiliary link files
                        if (File.Exists(m_auxTraceFilePath))
                            File.Delete(m_auxTraceFilePath);
                    }

                    break;
                }
                catch (IOException ex)
                {
                    if (attempt == maxRetries)
                        throw;
                    Thread.Sleep(250);
                }
            }


            return status;
        }


        /// <summary>
        /// generate a list of existing data file names 
        /// </summary>
        /// <param name="linkID"></param>
        /// <returns></returns>
        private List<string> getDataFileNames(DP14MSTTBDataType linkID)
        {
            List<string> fileNames = new List<string>();

            // get the file names of all the files in the folder
            string[] names = Directory.GetFiles(FS4500_FOLDER_PATH);

            // search through the filenames to get the data files
            foreach (string name in names)
            {
                string fName = Path.GetFileName(name);

                if (fName.Contains("TraceData") && fName.Contains(".bin"))
                {
                    if (linkID == DP14MSTTBDataType.AuxiliaryLinkSingleThread)
                    {
                        if (fName == "AuxTraceData.bin")
                        {
                            fileNames.Add(fName);
                            break;
                        }
                    }
                    else // Main link
                    {
                        if (linkID == DP14MSTTBDataType.MainLinkSingleThread)
                        {
                            if (fName == "TraceData.bin")
                            {
                                fileNames.Add(fName);
                                break;
                            }
                        }
                        else // multi-thread, meaning multiple files.
                        {
                            if (fName.Contains("TraceData_"))
                            {
                                fileNames.Add(fName);
                            }
                        }
                    }
                }
            }

            return fileNames;
        }

        /// <summary>
        /// Extract the column meta data for the binary file.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        private bool getColumnMetaInfo(string path, ref List<DP14MSTColumnMetaData> columnMetaDataList)
        {
            bool status = true;
            string columnName = string.Empty;
            string columnWidth = string.Empty;
            int offset = 0x00;

            // open a xml reader
            using (XmlReader reader = XmlReader.Create(path))
            {
                // remove any old data
                columnMetaDataList.Clear();

                // skip version ID (and any other) comments
                reader.MoveToContent();

                // move to the DataFormat section
                reader.ReadToFollowing("DataFormat");

                // get all the columns form the dataformat section
                while (reader.Read())
                {
                    reader.MoveToContent();
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "Signal")
                        {
                            DP14MSTColumnMetaData cInfo = new DP14MSTColumnMetaData();

                            // extract the meta data from the xml tags
                            cInfo.ColumnName = reader.GetAttribute("Name");
                            cInfo.ColumnWidth = int.Parse(reader.GetAttribute("Width"));
                            cInfo.ColumnOffset = offset;

                            // add the meta data to the list
                            columnMetaDataList.Add(cInfo);

                            // keep track of the number of bits into the loop
                            offset += cInfo.ColumnWidth;
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        break;
                    }
                }
            }  // closes the file 

            return status;
        }


        /// <summary>
        /// Get the column offsets for either the main or auxiliary link
        /// </summary>
        /// <param name="TBMode"></param>
        private void getColumnMetaData(DP14MST_TRACE_BUFFER_MODE TBMode)
        {
            string xmlfilePath = "";
            if (TBMode == DP14MST_TRACE_BUFFER_MODE.MainLink)
            {
                xmlfilePath = Path.Combine(FS4500_FOLDER_PATH, MainLink_COLUMNS_DEFS_XML_FILENAME);
                getColumnMetaInfo(xmlfilePath, ref m_columnMetaData_MainLink);
            }
            else
            {
                xmlfilePath = Path.Combine(FS4500_FOLDER_PATH, AuxiliaryLink_COLUMNS_DEFS_XML_FILENAME);
                getColumnMetaInfo(xmlfilePath, ref m_columnMetaData_AuxiliaryLink);
            }
        }



        /// <summary>
        /// Creates an xml file used to re-load a saved configuration.  
        /// </summary>
        /// <returns></returns>
        /// Assumptions:  All data files for 'other' protocol versions have been removed
        ///               when the probe is selected in the main form.
        private bool createConfigXMLFile(string filePath)
        {
            bool status = true;

            if (File.Exists(filePath))
                File.Delete(filePath);

            try
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.NewLineOnAttributes = false;

                // get a list of data files
                List<string> mainLinkFileNames_Single = getDataFileNames(DP14MSTTBDataType.MainLinkSingleThread);
                List<string> mainLinkFileNames_Multi = getDataFileNames(DP14MSTTBDataType.MainLinkMultiThread);
                List<string> auxLinkFileNames_Single = getDataFileNames(DP14MSTTBDataType.AuxiliaryLinkSingleThread);

                using (XmlWriter writer = XmlWriter.Create(filePath, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Config_Settings_DPTraceBufferDataMgr");



                    // Get the main link data file names
                    writer.WriteStartElement("Main_Link_Data_Files");

                    if (mainLinkFileNames_Multi.Count > 0)
                    {
                        foreach (string name in mainLinkFileNames_Multi)
                        {
                            writer.WriteStartElement("File");
                            writer.WriteAttributeString("value", name);
                            writer.WriteEndElement();
                        }
                    }
                    else if (mainLinkFileNames_Single.Count > 0)
                    {
                        foreach (string name in mainLinkFileNames_Single)
                        {
                            writer.WriteStartElement("File");
                            writer.WriteAttributeString("value", name);
                            writer.WriteEndElement();
                        }
                    }

                    //writer.WriteAttributeString("Setting_TBD", "TBD");
                    writer.WriteEndElement();




                    // Get the Auxiliary link data file names
                    writer.WriteStartElement("Aux_Link_Data_Files");

                    if (auxLinkFileNames_Single.Count > 0)
                    {
                        foreach (string name in auxLinkFileNames_Single)
                        {
                            writer.WriteStartElement("File");
                            writer.WriteAttributeString("value", name);
                            writer.WriteEndElement();
                        }
                    }

                    writer.WriteEndElement();

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
            catch (Exception ex)
            {
                status = false;
                //MessageBox.Show(ex.Message, "Error Saving Configuration", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            return status;
        }


        /// <summary>
        /// Get the data file names contained in an xml file
        /// </summary>
        /// <param name="xmlFilePath"></param>
        /// <param name="dataFileNames"></param>
        /// <returns></returns>
        private bool getDataFileNames(string xmlFilePath, ref List<string> dataFileNames)
        {
            bool status = true;
            dataFileNames = new List<string>();

            if (File.Exists(xmlFilePath))
            {
                try
                {
                    // open a xml reader
                    using (XmlReader reader = XmlReader.Create(xmlFilePath))
                    {
                        // skip version ID (and any other) comments
                        reader.MoveToContent();

                        // move to the DataFormat section
                        reader.ReadToFollowing("Main_Link_Data_Files");

                        // get all the columns form the dataformat section
                        while (reader.Read())
                        {
                            reader.MoveToContent();
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                if (reader.Name == "File")
                                    dataFileNames.Add(reader.GetAttribute("value"));
                            }
                            else if (reader.NodeType == XmlNodeType.EndElement)
                            {
                                break;
                            }
                        }
                    }  // closes the file 
                }
                catch (Exception ex)
                {
                    dataFileNames = null;
                    status = false;
                }
            }
            return status;
        }

        #endregion // Private Methods

        #region Public Methods

        /// <summary>
        /// Set the binary data file folder path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool SetDataFolderPath(string path)
        {
            bool status = true;

            if (Directory.Exists(path))
            {
                m_instanceFolderPath = path;

                m_auxTraceFilePath = Path.Combine(path, m_FS4500_AUX_TRACE_FILE_BASE_NAME);
                m_traceFilePath = Path.Combine(path, m_FS4500_TRACE_FILE_BASE_NAME);
            }
            else
            {
                status = false;
            }

            return status;
        }


        /// <summary>
        /// Load a saved configuration module
        /// </summary>
        /// <param name="moduleID"></param>
        /// <returns></returns>
        public bool LoadModuleConfig(string moduleID)
        {
            bool status = true;

            //if (moduleID == CONFIG_XML_FILENAME)
            //{
            //    string xmlFilePath = Path.Combine(FS4500_FOLDER_PATH, CONFIG_XML_FILENAME);
            //    List<string> dataFileNames = null;

            //    status = getDataFileNames(xmlFilePath, ref dataFileNames);
            //    if (status && (dataFileNames != null))
            //    {
            //        foreach (string name in dataFileNames)
            //        {
            //            if (!File.Exists(Path.Combine(FS4500_FOLDER_PATH, name)))
            //            {
            //                status = false;
            //                break;
            //            }
            //        }

            //        if (status)
            //        {
            //            //// set the number of cascading columns.... 
            //            //m_numOfTaskColumns = dataFileNames.Count / MAX_VCHANNELS;
            //            m_numOfDataFiles = dataFileNames.Count;
            //            m_multiThreadFileMetaData = initVChannelMetaData(dataFileNames.Count);   //initalizeDataFilesMetaData();
            //        }
            //    }


            //    //if (status == false)
            //    //    MessageBox.Show("Error Reloading Trace Buffer data files", "Error Loading Saved Config", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //}

            return status;
        }


        /// <summary>
        /// Assemble an XML string representing the current configuration
        /// </summary>
        /// <returns></returns>
        public string GetConfig()
        {
            //// create the xml file 
            //string xmlfilePath = Path.Combine(FS4500_FOLDER_PATH, CONFIG_XML_FILENAME);


            //// Create an XML file containing the current settings...
            //createConfigXMLFile(xmlfilePath);


            //// return the filePath (so the top level can archive the file).
            //return xmlfilePath;
            return string.Empty;
        }

        #endregion // Public Methods

        #region ITBObject Methods

        /// <summary>
        /// once created, send a DPEvent to inform other objects of our existance.
        /// </summary>
        public void RegisterAsTBObject()
        {
            if (DisplayPort12MSTEvent != null)
                DisplayPort12MSTEvent(this, new DP14MSTEventArgs((DP14MSTMessage)(m_DP12MSTMessageGenerator.DP14MSTMessageCreate_Register((ITBObject)this))));
        }


        /// <summary>
        /// Initialize the TB Object
        /// </summary>
        public void Initialize()
        {
        }


        private long m_triggerTimeStamp = -1;
        private long m_triggerVChannelID = 0x01;  // defaults to Probe Stream 1

        /// <summary>
        /// determine if the event is of interest to us and act on it
        /// </summary>
        /// <param name="DisplayPortEvent"></param>
        public void ProcessDPMessage(DP14MSTMessage DisplayPortEvent)
        {
            if (DisplayPortEvent is DP14MSTMessage_PMRunning)
            {
                //m_ExitRunMode = false; // true;
                //m_multiThreadFileSize = 0;

                //// call the method to remove the old binary files. -- assumes all files are closed.
                //removeDataFiles(m_TBMode);
            }
            else if (DisplayPortEvent is DP14MSTMessage_PMStopping_Request)
            {
                //if (tokenSource != null)
                //    CancelThreadProcessing();
                //else
                //{
                //    m_dataEnabled = false;  // indicate there is no data available.
                //    m_ExitRunMode = false;

                //    // tell the PM Gen 2 object that we are done with cancelation... as none was needed.
                //    if (StopRequestCompletedEvent != null)
                //        StopRequestCompletedEvent(this, new StopRequestCompleteEventArgs());
                //}
            }
            else if (DisplayPortEvent is DP14MSTMessage_PMStopping_Ack)
            {
                //TBD
            }
            else if (DisplayPortEvent is DP14MSTMessage_PMReady)
            {
                // do nothing... the event was generated from this object!
            }
            else if (DisplayPortEvent is DP14MSTMessage_DataAvailable)
            {
                //m_ExitRunMode = ((DP12MSTMessage_DataAvailable)DisplayPortEvent).ExitRunMode;
                //uploadTraceBufferData(DisplayPortEvent);
            }
            else if (DisplayPortEvent is DP14MSTMessage_DataUploaded)
            {
                //m_triggerTimeStamp = -1;
                //m_triggerVChannelID = -1;

                //// m_triggerBinaryFileName is set when the data is uploaded from the HW... e.g. TraceData_0.bin
                //// we need to get the trigger time stamps and the VCTag of that state in order to segrated the 
                //// data into virtual channel data files.
                //if (getTriggerTimeStamp(m_triggerBinaryFileName, ref m_triggerTimeStamp, ref m_triggerVChannelID))
                //    generateVirtualChannelFiles(m_triggerTimeStamp, (int)m_triggerVChannelID);
                //else
                //    clearDataUploadFolder();
            }
            else if (DisplayPortEvent is DP14MSTMessage_StateDataRequest)
            {
                // retain the state for subsequent requests to the same state... saves on overhead.
                getRequestedData(DisplayPortEvent);
            }
            else if (DisplayPortEvent is DP14MSTMessage_StateDataChunkRequest)
            {
                // retain the state for subsequent requests to the same state... saves on overhead.
                getRequestedDataChunk(DisplayPortEvent);
            }
            else if (DisplayPortEvent is DP14MSTMessage_GetStateData)
            {
                getRequestedStateData(DisplayPortEvent);
            }
            else if (DisplayPortEvent is DP14MSTMessage_AuxDataAvailable)
            {
                //m_ExitRunMode = ((DP12MSTMessage_AuxDataAvailable)DisplayPortEvent).ExitRunMode;
                //uploadAuxTraceBufferData(DisplayPortEvent);
            }
            else if (DisplayPortEvent is DP14MSTMessage_AuxStateDataRequest)
            {
                //// retain the state for subsequent requests to the same state... saves on overhead.
                //getAuxRequestedData(DisplayPortEvent);
            }
            else if (DisplayPortEvent is DP14MSTMessage_AuxGetStateData)
            {
                //getRequestedAuxStateData(DisplayPortEvent);
            }
            else if (DisplayPortEvent is DP14MSTMessage_MemoryDepthChanged)
            {
                //// retain the state for subsequent requests to the same state... saves on overhead.
                //m_memoryDepth = ((DP12MSTMessage_MemoryDepthChanged)DisplayPortEvent).MemoryDepth;
                //m_numOfDataFiles = calculateNumOfDataBlocks(m_memoryDepth) * 4;   // capture is made up of blocks of data; each having 4 virtual channels.
            }
            else if (DisplayPortEvent is DP14MSTMessage_TimeSlotSelectionChanged)
            {
                //m_VCFileGenerator.UpdateSelectedSlotsInfo((DP12MSTMessage_TimeSlotSelectionChanged)DisplayPortEvent);
            }
            //else if (DisplayPortEvent is DP12MSTMessage_TriggerChannelChanged)
            //{
            //    m_triggerChannelID = ((DP12MSTMessage_TriggerChannelChanged)DisplayPortEvent).TriggerChannelID;
            //}
            else if (DisplayPortEvent is DP14MSTMessage_TBDataModeChange)
            {
                //m_TBMode = ((DP12MSTMessage_TBDataModeChange)DisplayPortEvent).TBModeID;
            }
            else if (DisplayPortEvent is DP14MSTMessage_GetTBBinFileMetaData)
            {
                getTBMetaData(DisplayPortEvent);
            }
            else if (DisplayPortEvent is DP14MSTMessage_SearchListingRequest)
            {
                //searchTraceBufferData(DisplayPortEvent);
            }
            else if (DisplayPortEvent is DP14MSTMessage_SearchListingCancelRequest)
            {
                //cancelSearch();
            }
            else if (DisplayPortEvent is DP14MSTMessage_Initialize)
            {
                //if (((DP12MSTMessage_Initialize)DisplayPortEvent).RemoveDataFiles)
                //{
                //    removeDataFiles(DP14MSTTRACE_BUFFER_MODE.MainLink);
                //    removeDataFiles(DP14MSTTRACE_BUFFER_MODE.AuxiliaryLink);
                //}
            }
            else if (DisplayPortEvent is DP14MSTMessage_GetNumberOfStates)
            {
                // get the number of states contained in the uploaded binary data.
            }
        }

        #endregion // ITBObject Methods
    }


    // An event delegate used by various user controls to inform the top level control
    // that a back ground task is running... probably should disable the main form's Enabled property 
    public delegate void HWDataUploadStatusEvent(object sender, HWDataUploadStatusEventArgs e);

    //Event Args used to identify who has a background task running...
    public class HWDataUploadStatusEventArgs : EventArgs
    {
        public string ID;
        public bool IsRunning;
        public int numOfStatesProcessed = 0;
        public int numOfCapturedStates = 0;
        public float ProgressPercentage = 0;

        public HWDataUploadStatusEventArgs(string ID, bool taskIsRunning, int statesProcessed = 0, int capturedStates = 0, float progressPercentage = 0)
        {
            this.ID = ID;
            this.IsRunning = taskIsRunning;
            this.numOfStatesProcessed = statesProcessed;
            this.numOfCapturedStates = capturedStates;
            this.ProgressPercentage = progressPercentage;
        }
    }


    public delegate void StopRequestCompleteEvent(object sender, StopRequestCompleteEventArgs e);


    //Event Args used to identify who has a background task running...
    public class StopRequestCompleteEventArgs : EventArgs
    {
        public string Parameter;

        public StopRequestCompleteEventArgs(string parameter = "")
        {
            this.Parameter = parameter;
        }
    }
}
