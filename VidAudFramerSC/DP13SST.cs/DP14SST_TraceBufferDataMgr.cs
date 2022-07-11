using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using SharedProject1;
using FPSProbeMgr_Gen2;

namespace DP14SSTClassLibrary
{
    public enum DP14SST_TBDataType { MainLinkSingleThread, MainLinkMultiThread, AuxiliaryLinkSingleThread, Unknown }

    class DP14SST_TraceBufferDataMgr : ITBObject
    {
        #region Members

        private enum DP14SST_TBDataMgrMode { SingleThreaded, MultiThreaded, Unknown }
        private enum LinkID { Main, Auxiliary, Unknown };

        private class DP14SST_TBParameters
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

        private class DP14SST_AuxParameters
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

        public class DP14SST_TBUploadDescriptor
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

            public DP14SST_TBUploadDescriptor(long startState, long endState, long triggerState, long triggerOffset, long numOfStates, byte wrapped,
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

        public class DP14SST_TBSearchDescriptor
        {
            public DP14SST_TRACE_BUFFER_MODE TBMode;
            public string ColumnName = string.Empty;
            public uint ColumnValue = 0x00;
            public long StateIndex;
            public bool SearchForward = true;

            public DP14SST_TBSearchDescriptor(DP14SST_TRACE_BUFFER_MODE listingType, string cName, uint cValue, long stateIndex, bool searchForward)
            {
                TBMode = listingType;
                ColumnName = cName;
                ColumnValue = cValue;
                StateIndex = stateIndex;
                SearchForward = searchForward;
            }
        }

        private class DP14SST_TaskStateObject
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

            //public DP14SST_TaskStateObject(string path, long trigStateOffset, long startStateIndex, long[] fileStartIndices, long blockSize, long memoryDepth, long numOfCapturedStates, FT60x_IF interfaceRef, HWDataUploadStatusEvent dataUploadStatusEvent, int index, CancellationToken ct)
            //{
            //    FolderPath = path;
            //    TrigStateIndex = trigStateOffset;
            //    StartStateIndex = startStateIndex;
            //    FileStartIndices = new long[fileStartIndices.Length];
            //    Array.Copy(fileStartIndices, FileStartIndices, FileStartIndices.Length);
            //    BlockSize = blockSize;
            //    MemoryDepth = memoryDepth;
            //    NumOfCapturedStates = numOfCapturedStates;
            //    MyFTD2xxIF = interfaceRef;
            //    UploadStatusEvent = dataUploadStatusEvent;
            //    Index = index;
            //    Token = ct;
            //}
        }

        private class DP14SST_ColumnMetaData
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
        private class DP14SST_MultiThread_MetaDataArgs
        {
            private int m_startState = -1;
            public int StartState
            {
                get { return m_startState; }
                set { m_startState = value; }
            }

            private int m_endState = -1;
            public int EndState
            {
                get { return m_endState; }
                set { m_endState = value; }
            }

            private int m_dataOffset = -1;
            public int DataOffset
            {
                get { return m_dataOffset; }
                set { m_dataOffset = value; }
            }
        }

        private const string BIN_FILE_HEADER_INFO_NUM_OF_STATES = "NumOfStates";
        private const string BIN_FILE_HEADER_INFO_TRIG_OFFSET = "TrigOffset";

        private const string BIN_FILE_MULTI_HEADER_START_STATE = "StartState";
        private const string BIN_FILE_MULTI_HEADER_END_STATE = "EndState";
        private const string BIN_FILE_MULTI_HEADER_INFO_TRIG_OFFSET = "TrigOffset";

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

        private const int SMALL_DATA_UPLOAD_NUM_OF_COLUMNS = 2;
        private const int MEDIUM_DATA_UPLOAD_NUM_OF_COLUMNS = 4;
        private const int LARGE_DATA_UPLOAD_NUM_OF_COLUMNS = 4;
        private const int LARGEST_DATA_UPLOAD_NUM_OF_COLUMNS = 4;

        //private const int SMALL_DATA_UPLOAD_NUM_OF_LEVELS = 2;
        //private const int MEDIUM_DATA_UPLOAD_NUM_OF_LEVELS = 4;
        //private const int LARGE_DATA_UPLOAD_NUM_OF_LEVELS = 8;  //4
        //private const int LARGEST_DATA_UPLOAD_NUM_OF_LEVELS = 16;

        private const int TB_MAX_SINGLE_THREAD_SIZE = 0x100000;  // 1M == 0x100000, 2M = 0x200000, 4M = 0x400000, 8M = 0x800000, 16M = 0x1000000
        private const int TB_MAX_THREAD_SIZE_SMALL = 0x40000;   // 8K - 256K 
        private const int TB_MAX_THREAD_SIZE_MEDIUM = 0x1000000;   // 512K - 16M
        private const int TB_MAX_THREAD_SIZE_LARGE = 0x8000000;   // 32M - 128M //512M
        private const int TB_MAX_THREAD_SIZE_VERY_LARGE = 0x40000000;   // 512 - 1G

        private const int TB_NUM_OF_THREADS_SMALL = 1;     // 8K - 256K states
        private const int TB_NUM_OF_THREADS_MEDIUM = 8;     // 512K - 16M states
        private const int TB_NUM_OF_THREADS_LARGE = 16;     // 32m - 512M states
        private const int TB_NUM_OF_THREADS_VERY_LARGE = 32; // 256M - 1G states 

        private CancellationTokenSource tokenSource = null; // tokenSource.Token;
        private CancellationToken token = CancellationToken.None;
        private volatile int m_percentComplete = 0;

        private const int TB_STATE_BYTE_LEN = 16;  // FS4500 TB state size
        //private const int TB_STATE_BYTE_LEN = 39;  // to read the .ALB binary file.  // PNS -- ALB File Change
        private const int TB_STATES_PER_PAGE = 4096;
        private const int TB_PAGE_BYTE_LENGTH = TB_STATES_PER_PAGE * TB_STATE_BYTE_LEN;
        private const int PROGRESS_INDICTOR_TIMER_INTERVAL = 100;

        private const int SEARCH_REPORTING_INTERVAL_SIZE = 4 * 1024;

        //private string m_FS4500_FOLDER_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "FuturePlus";
        //private string m_FS4500_FOLDER_NAME = "FS4500";
        private string m_FS4500_TRACE_FILE_BASE_NAME = "TraceData.bin";
        private string m_FS4500_AUX_TRACE_FILE_BASE_NAME = "AuxTraceData.bin";
        private const string MainLink_COLUMNS_DEFS_XML_FILENAME = "DP14SST_TBDataFormat.xml";
        private const string AuxiliaryLink_COLUMNS_DEFS_XML_FILENAME = "DP14SST_AuxTBDataFormat.xml";
        private string FS4500_FOLDER_PATH = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"FuturePlus\FS4500");
        private const string CONFIG_XML_FILENAME = "Config_DP14SSTTraceBufferDataMgrForm.xml";

        private string m_instanceFolderPath = string.Empty;

        private string m_FS4500_MULTI_THREAD_TRACE_FILE_BASE_NAME = "TraceData_";

        private string m_auxTraceFilePath = string.Empty;
        private string m_traceFilePath = string.Empty;

        private DP14SST_TRACE_BUFFER_MODE m_TBMode = DP14SST_TRACE_BUFFER_MODE.MainLink;        // Main, Auxiliary, Both or Unknown
        private DP14SST_TBDataMgrMode m_TBDataMode = DP14SST_TBDataMgrMode.SingleThreaded;      // singleThreaded, MultiThreaded, unknown

        private List<DP14SST_MultiThread_MetaDataArgs> m_multiThreadFileMetaData = new List<DP14SST_MultiThread_MetaDataArgs>();
        private List<DP14SST_ColumnMetaData> m_columnMetaData_MainLink = new List<DP14SST_ColumnMetaData>();
        private List<DP14SST_ColumnMetaData> m_columnMetaData_AuxiliaryLink = new List<DP14SST_ColumnMetaData>();

        private int m_multiThreadFileSize = 0;
        private int m_numOfTPITasks = 12;

        private long m_dataOffset_mainLink = 0;
        private long m_dataOffset_auxLink = 0;

        //private System.Windows.Forms.Timer m_taskProgressTimer = null;



        // Create an event which the main form can register for... all Trace Buffer
        // objects will raise the same event but with different parameters... the
        // Event class is defined in the DP 1.1a main form.
        //
        public event DP14SSTEvent DisplayPort13SSTEvent;

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
        private DP14SST_TBParameters m_TBParameters = null;

        private string[] m_AUXStatusFields = new string[]  { "Pad:4", "AW4TIN:1", "Pad:1", "TBFIN:1", "TRIG:1", "WRAP:1", "Pad:1", "AWI2NDLVL:1",
                                                            "PRE_TRIG_LINE_COUNT:17", "PST_TRIG_LINE_CNT:17", "TIME_COUNT:50", "TRIG_LINE:17" };
        private byte[] m_AUXStatusFieldWidths = null;
        private DP14SST_AuxParameters m_AUXParameters = null;


        private FieldExtractor m_fldExtractor = null;

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
        private DP14SST_MessageGenerator m_DP14SSTMessageGenerator = null;

        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SST_TraceBufferDataMgr(string path)
        {
            //m_FTD2xxIF = FT60x_IF.GetInstance();
            m_DP14SSTMessageGenerator = DP14SST_MessageGenerator.GetInstance();
            //m_VCFileGenerator = DP14SSTVCFileGenerator.GetInstance();
            //m_VCFileGenerator.VCFileGenStatusEvent += new VCFileGenerationStatusEvent(processVCGenerationProgressEvent);

            // the data files and any loaded configuration files go here
            m_instanceFolderPath = path;

            // all uploads result in creating the same file...
            //m_auxTraceFilePath = Path.Combine(m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME, m_FS4500_AUX_TRACE_FILE_BASE_NAME);
            //m_traceFilePath = Path.Combine(m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME, m_FS4500_TRACE_FILE_BASE_NAME);

            m_auxTraceFilePath = Path.Combine(m_instanceFolderPath, m_FS4500_AUX_TRACE_FILE_BASE_NAME);
            m_traceFilePath = Path.Combine(m_instanceFolderPath, m_FS4500_TRACE_FILE_BASE_NAME);


            //getColumnMetaData(DP14SSTTRACE_BUFFER_MODE.MainLink);
            //getColumnMetaData(DP14SSTTRACE_BUFFER_MODE.AuxiliaryLink);
        }

        #endregion // Constructor(s)

        #region Event Handlers

        ///// <summary>
        ///// Get main link trace buffer data 
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void DoWork_TBUpload(object sender, DoWorkEventArgs e)
        //{
        //    bool status = true;
        //    if (m_worker.CancellationPending)
        //    {
        //        e.Cancel = true;
        //    }
        //    else
        //    {
        //        DP14SST_TBUploadDescriptor parameters = (DP14SST_TBUploadDescriptor)e.Argument;
        //        byte[] pageData = new byte[TB_PAGE_BYTE_LENGTH];
        //        int statesPerPage = TB_STATES_PER_PAGE;

        //        long stateOffset = parameters.StartState;
        //        long pageOffset = 4096;
        //        long maxStateOffset = parameters.MemoryDepth;

        //        long numOfStatesUploaded = 0;
        //        long partialPageSize = 0;
        //        m_metaDataHeaderLength = 0x00;
        //        int retry = 0;

        //        //m_stopWatchTimesSB.Length = 0;

        //        // create a file stream to write too
        //        string traceFilePath = Path.Combine(m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME, m_FS4500_TRACE_FILE_BASE_NAME);
        //        using (BinaryWriter bw = new BinaryWriter(File.Open(traceFilePath, FileMode.Create, FileAccess.Write, FileShare.None)))
        //        {
        //            if (m_generateMetaData)
        //            {
        //                byte[] array = System.Text.Encoding.ASCII.GetBytes("NumOfStates:" + parameters.NumOfStates.ToString() + "\n");
        //                bw.Write(array);
        //                m_metaDataHeaderLength += array.Length;

        //                array = System.Text.Encoding.ASCII.GetBytes("TrigOffset:" + parameters.TriggerOffset.ToString() + "\n");
        //                bw.Write(array);
        //                m_metaDataHeaderLength += array.Length;

        //                array = System.Text.Encoding.ASCII.GetBytes("*****" + "\n");
        //                bw.Write(array);
        //                m_metaDataHeaderLength += array.Length;
        //                bw.Flush();
        //            }

        //            // record where the data starts (for processing individual states).
        //            m_dataOffset_mainLink = bw.BaseStream.Position;

        //            while (status && (!m_worker.CancellationPending) && (numOfStatesUploaded < (parameters.NumOfStates - 1)) && (retry < 10))
        //            {
        //                Array.Clear(pageData, 0, pageData.Length);
        //                if ((stateOffset + pageOffset) <= parameters.MemoryDepth)
        //                {
        //                    status = m_FTD2xxIF.GetTraceBufferPage(stateOffset, ref pageData);
        //                    if (status)
        //                    {
        //                        retry = 0;
        //                        // determine if we want the entire page or just part of it... in terms of states and not bytes
        //                        if ((numOfStatesUploaded + statesPerPage) <= parameters.MemoryDepth)
        //                        {
        //                            if ((numOfStatesUploaded + statesPerPage) <= parameters.NumOfStates)
        //                            {
        //                                bw.Write(pageData);
        //                                stateOffset += pageOffset;
        //                                numOfStatesUploaded += TB_STATES_PER_PAGE;
        //                            }
        //                            else
        //                            {
        //                                partialPageSize = parameters.NumOfStates - numOfStatesUploaded;
        //                                bw.Write(pageData, 0, (int)(partialPageSize * parameters.BytesPerState));
        //                                numOfStatesUploaded += partialPageSize;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (numOfStatesUploaded < parameters.MemoryDepth)
        //                            {
        //                                partialPageSize = parameters.MemoryDepth - numOfStatesUploaded;
        //                                bw.Write(pageData, 0, (int)(partialPageSize * parameters.BytesPerState));
        //                                numOfStatesUploaded += partialPageSize;
        //                            }
        //                            else
        //                            {
        //                                break;
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        retry += 1;
        //                        if (retry < 10)
        //                        {
        //                            status = true;
        //                            Thread.Sleep(100);
        //                        }
        //                        //else
        //                        //    status = false;
        //                    }
        //                }
        //                else // partial buffer scenerio
        //                {
        //                    // get the part of the data that falls before the MemoryDepth Limit...
        //                    partialPageSize = parameters.MemoryDepth - (parameters.StartState + numOfStatesUploaded);

        //                    //m_stopWatch.Reset();
        //                    //m_stopWatch.Start();
        //                    status = m_FTD2xxIF.GetTraceBufferPage(stateOffset, ref pageData);
        //                    //m_stopWatch.Stop();
        //                    //m_stopWatchTimesSB.Append("Request " + requestCount.ToString() + ": " + m_stopWatch.Elapsed.ToString() + Environment.NewLine);

        //                    if (status)
        //                    {
        //                        retry = 0;
        //                        // write the parial page to the binary file
        //                        bw.Write(pageData, 0, (int)(partialPageSize * parameters.BytesPerState));

        //                        // set the stateOffset for the next page
        //                        stateOffset = stateOffset + (partialPageSize * parameters.BytesPerState);
        //                        numOfStatesUploaded += partialPageSize;
        //                        stateOffset = 0;
        //                    }
        //                    else
        //                    {
        //                        retry += 1;
        //                        if (retry < 10)
        //                            status = true;
        //                        else
        //                            status = false;
        //                    }
        //                }

        //                m_worker.ReportProgress((int)((float)((float)numOfStatesUploaded / (float)parameters.MemoryDepth) * 100));
        //            }

        //            bw.Flush();
        //            bw.Close();
        //        }

        //        if (!status)
        //            clearDataUploadFolder();
        //    }

        //    if (status == true)
        //        e.Result = true;
        //    else
        //        e.Result = false;
        //}


        ///// <summary>
        ///// Update the status of uploading main link data
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void ProgressChanged_TBUpload(object sender, ProgressChangedEventArgs e)
        //{
        //    if (TBDataUploadStatusEvent != null)
        //        TBDataUploadStatusEvent(this, new HWDataUploadStatusEventArgs("TB Upload Percentage", true, e.ProgressPercentage));
        //}


        ///// <summary>
        ///// Process the event assoicated with the main link upload Background thread completing.
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void RunWorkerCompleted_TBUpload(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    if (e.Cancelled)
        //    {
        //        clearDataUploadFolder();
        //    }
        //    else if (e.Error != null)
        //    {
        //        clearDataUploadFolder();
        //    }

        //    if ((bool)e.Result)
        //    {
        //        // raise an event  -- informing the top level object that the upload is done!
        //        if ((TBDataUploadStatusEvent != null) && m_ExitRunMode)
        //            TBDataUploadStatusEvent(this, new HWDataUploadStatusEventArgs("Upload Complete", false));

        //        if (DisplayPort12SSTEvent != null)
        //            DisplayPort12SSTEvent(this, new DP14SSTEventArgs((DP14SSTMessage)(m_DP14SSTMessageGenerator.DP14SSTMessageCreate_DataReady())));
        //    }
        //    else
        //    {
        //        if (TBDataUploadStatusEvent != null)
        //            TBDataUploadStatusEvent(this, new HWDataUploadStatusEventArgs("Upload Failed with Error", false));
        //    }


        //    //BackgroundWorker worker = sender as BackgroundWorker;
        //    //worker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(RunWorkerCompleted_TBUpload);
        //    //worker.DoWork -= new DoWorkEventHandler(DoWork_TBUpload);
        //    //worker.Dispose();
        //    //worker = null;

        //    m_worker.Dispose();
        //    m_worker = null;
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
        //        if (((DP14SSTTBSearchDescriptor)e.Argument).SearchForward)
        //        {
        //            stateIndex = searchListing_SingleThread_Forward(((DP14SSTTBSearchDescriptor)e.Argument).TBMode,
        //                                                            ((DP14SSTTBSearchDescriptor)e.Argument).ColumnName,
        //                                                            ((DP14SSTTBSearchDescriptor)e.Argument).ColumnValue,
        //                                                            ((DP14SSTTBSearchDescriptor)e.Argument).StateIndex,
        //                                                            SEARCH_REPORTING_INTERVAL_SIZE);
        //        }
        //        else  // search backwards
        //        {
        //            stateIndex = searchListing_SingleThread_Backward(((DP14SSTTBSearchDescriptor)e.Argument).TBMode,
        //                                                            ((DP14SSTTBSearchDescriptor)e.Argument).ColumnName,
        //                                                            ((DP14SSTTBSearchDescriptor)e.Argument).ColumnValue,
        //                                                            ((DP14SSTTBSearchDescriptor)e.Argument).StateIndex,
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
        //                            new DP12MSTEventArgs(m_DP12MSTMessageGenerator.DP12MSTMessageCreate_SearchListingProgessReport(DP14SSTTRACE_BUFFER_MODE.MainLink, SEARCH_REPORTING_INTERVAL_SIZE)));
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

        //    DP14SSTTRACE_BUFFER_MODE TBMode = DP14SSTTRACE_BUFFER_MODE.MainLink;
        //    if (m_MainLinkSingleThreadSearch == false)
        //        TBMode = DP14SSTTRACE_BUFFER_MODE.AuxiliaryLink;

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
        //        if (((DP14SSTTBSearchDescriptor)e.Argument).SearchForward)
        //        {
        //            stateIndex = searchListing_MultiThread_Forward(((DP14SSTTBSearchDescriptor)e.Argument).TBMode,
        //                                                            ((DP14SSTTBSearchDescriptor)e.Argument).ColumnName,
        //                                                            ((DP14SSTTBSearchDescriptor)e.Argument).ColumnValue,
        //                                                            ((DP14SSTTBSearchDescriptor)e.Argument).VChannelID,
        //                                                            ((DP14SSTTBSearchDescriptor)e.Argument).StateIndex,
        //                                                            SEARCH_REPORTING_INTERVAL_SIZE);
        //        }
        //        else  // search backwards
        //        {
        //            stateIndex = searchListing_MultiThread_Backward(((DP14SSTTBSearchDescriptor)e.Argument).TBMode,
        //                                                            ((DP14SSTTBSearchDescriptor)e.Argument).ColumnName,
        //                                                            ((DP14SSTTBSearchDescriptor)e.Argument).ColumnValue,
        //                                                            ((DP14SSTTBSearchDescriptor)e.Argument).VChannelID,
        //                                                            ((DP14SSTTBSearchDescriptor)e.Argument).StateIndex,
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
        //                            new DP12MSTEventArgs(m_DP12MSTMessageGenerator.DP12MSTMessageCreate_SearchListingProgessReport(DP14SSTTRACE_BUFFER_MODE.MainLink, SEARCH_REPORTING_INTERVAL_SIZE)));
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
        //            DisplayPort12MSTEvent(this, new DP12MSTEventArgs((m_DP12MSTMessageGenerator.DP12MSTMessageCreate_SearchListingResponse(DP14SSTTRACE_BUFFER_MODE.MainLink, 0, true, (long)(e.Result)))));
        //    }
        //    else
        //    {
        //        // raise an event  -- informing the top level object that the upload is done!
        //        if (DisplayPort12MSTEvent != null)
        //            DisplayPort12MSTEvent(this, new DP12MSTEventArgs((m_DP12MSTMessageGenerator.DP12MSTMessageCreate_SearchListingResponse(DP14SSTTRACE_BUFFER_MODE.MainLink, 0, false, (long)(e.Result)))));
        //    }

        //    m_searchWorker_MultiThread.Dispose();
        //    m_searchWorker_MultiThread = null;
        //}


        ///// <summary>
        ///// Get auxiliary link trace buffer data 
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void DoWork_AuxUpload(object sender, DoWorkEventArgs e)
        //{
        //    //bool status = true;
        //    //if (m_auxWorker.CancellationPending)
        //    //{
        //    //    e.Cancel = true;
        //    //}
        //    //else
        //    //{
        //    //    DP14SSTTBUploadDescriptor parameters = (DP14SSTTBUploadDescriptor)e.Argument;
        //    //    byte[] pageData = new byte[AUX_TB_PAGE_BYTE_LENGTH];
        //    //    int statesPerPage = AUX_TB_STATES_PER_PAGE;

        //    //    long stateOffset = parameters.StartState;
        //    //    long pageOffset = AUX_TB_STATES_PER_PAGE;
        //    //    long maxStateOffset = parameters.MemoryDepth;

        //    //    long numOfStatesUploaded = 0;
        //    //    long partialPageSize = 0;
        //    //    m_metaDataHeaderLength = 0x00;
        //    //    int retry = 0;

        //    //    //m_stopWatchTimesSB.Length = 0;

        //    //    // create a file stream to write too
        //    //    string traceFilePath = Path.Combine(m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME, m_FS4500_AUX_TRACE_FILE_BASE_NAME);
        //    //    using (BinaryWriter bw = new BinaryWriter(File.Open(traceFilePath, FileMode.Create, FileAccess.Write, FileShare.None)))
        //    //    {
        //    //        if (m_generateMetaData)
        //    //        {
        //    //            byte[] array = System.Text.Encoding.ASCII.GetBytes("NumOfStates:" + parameters.NumOfStates.ToString() + "\n");
        //    //            bw.Write(array);
        //    //            m_metaDataHeaderLength += array.Length;

        //    //            array = System.Text.Encoding.ASCII.GetBytes("TrigOffset:" + parameters.TriggerOffset.ToString() + "\n");
        //    //            bw.Write(array);
        //    //            m_metaDataHeaderLength += array.Length;

        //    //            array = System.Text.Encoding.ASCII.GetBytes("*****" + "\n");
        //    //            bw.Write(array);
        //    //            m_metaDataHeaderLength += array.Length;
        //    //            bw.Flush();
        //    //        }

        //    //        // record where the data starts (for processing individual states).
        //    //        m_dataOffset_auxLink = bw.BaseStream.Position;

        //    //        while (status && (!m_auxWorker.CancellationPending) && (numOfStatesUploaded < (parameters.NumOfStates - 1)) && (retry < 10))
        //    //        {
        //    //            Array.Clear(pageData, 0, pageData.Length);
        //    //            if ((stateOffset + pageOffset) <= parameters.MemoryDepth)
        //    //            {
        //    //                status = m_FTD2xxIF.GetAuxTraceBufferPage(stateOffset, ref pageData);
        //    //                if (status)
        //    //                {
        //    //                    retry = 0;
        //    //                    // determine if we want the entire page or just part of it... in terms of states and not bytes
        //    //                    if ((numOfStatesUploaded + statesPerPage) <= parameters.MemoryDepth)
        //    //                    {
        //    //                        if ((numOfStatesUploaded + statesPerPage) <= parameters.NumOfStates)
        //    //                        {
        //    //                            bw.Write(pageData);
        //    //                            stateOffset += pageOffset;
        //    //                            numOfStatesUploaded += AUX_TB_STATES_PER_PAGE;
        //    //                        }
        //    //                        else
        //    //                        {
        //    //                            partialPageSize = parameters.NumOfStates - numOfStatesUploaded;
        //    //                            bw.Write(pageData, 0, (int)(partialPageSize * parameters.BytesPerState));
        //    //                            numOfStatesUploaded += partialPageSize;
        //    //                        }
        //    //                    }
        //    //                    else
        //    //                    {
        //    //                        if (numOfStatesUploaded < parameters.MemoryDepth)
        //    //                        {
        //    //                            partialPageSize = parameters.MemoryDepth - numOfStatesUploaded;
        //    //                            bw.Write(pageData, 0, (int)(partialPageSize * parameters.BytesPerState));
        //    //                            numOfStatesUploaded += partialPageSize;
        //    //                        }
        //    //                        else
        //    //                        {
        //    //                            break;
        //    //                        }
        //    //                    }
        //    //                }
        //    //                else
        //    //                {
        //    //                    retry += 1;
        //    //                    if (retry < 10)
        //    //                    {
        //    //                        status = true;
        //    //                        Thread.Sleep(100);
        //    //                    }
        //    //                    //else
        //    //                    //    status = false;
        //    //                }
        //    //            }
        //    //            else // partial buffer scenerio
        //    //            {
        //    //                // get the part of the data that falls before the MemoryDepth Limit...
        //    //                partialPageSize = parameters.MemoryDepth - (parameters.StartState + numOfStatesUploaded);

        //    //                status = m_FTD2xxIF.GetAuxTraceBufferPage(stateOffset, ref pageData);
        //    //                if (status)
        //    //                {
        //    //                    retry = 0;
        //    //                    // write the parial page to the binary file
        //    //                    bw.Write(pageData, 0, (int)(partialPageSize * parameters.BytesPerState));

        //    //                    // set the stateOffset for the next page
        //    //                    stateOffset = stateOffset + (partialPageSize * parameters.BytesPerState);
        //    //                    numOfStatesUploaded += partialPageSize;
        //    //                    stateOffset = 0;
        //    //                }
        //    //                else
        //    //                {
        //    //                    retry += 1;
        //    //                    if (retry < 10)
        //    //                        status = true;
        //    //                    else
        //    //                        status = false;
        //    //                }
        //    //            }

        //    //            m_auxWorker.ReportProgress((int)((float)((float)numOfStatesUploaded / (float)parameters.MemoryDepth) * 100));
        //    //        }

        //    //        bw.Flush();
        //    //        bw.Close();
        //    //    }

        //    //    if (!status)
        //    //        clearDataUploadFolder();
        //    //}

        //    //if (status == true)
        //    //    e.Result = true;
        //    //else
        //    //    e.Result = false;
        //}


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
        //private bool GetTBStatusVariables(List<long> fldValues, DP14SSTTBParameters parameters)
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
        //private bool GetAUXTBStatusVariables(List<long> fldValues, DP14SSTAuxParameters parameters)
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
        //    m_TBParameters = new DP14SSTTBParameters();
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
        //    m_AUXParameters = new DP14SSTAuxParameters();
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
        //private void initSearchBGWorker_SingleThreaded(DP14SSTTBSearchDescriptor threadParameters)
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
        //private void initSearchBGWorker_MultiThreaded(DP14SSTTBSearchDescriptor threadParameters)
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
        //private void initAuxLinkBGWorker(DP14SSTTBUploadDescriptor threadParameters)
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
        //private void uploadData_BGWorkerMode(DP14SSTTBUploadDescriptor threadParameters, bool isMainLinkData = true)
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
        //private bool waitForCascadingTasksToComplete(Task<DP14SSTTaskStateObject>[] tasks,
        //                                                Task<DP14SSTTaskStateObject>[][] continuationTasks)
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
        //private bool waitForCascadingTasksToComplete(Task<DP14SSTTaskStateObject>[] tasks,
        //                                                Task<DP14SSTTaskStateObject>[] continuationTasks,
        //                                                Task<DP14SSTTaskStateObject>[] continuationTasks2,
        //                                                Task<DP14SSTTaskStateObject>[] continuationTasks3,
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
        //private bool waitForCascadingTasksToComplete(Task<DP14SSTTaskStateObject>[] tasks,
        //                                                Task<DP14SSTTaskStateObject>[] continuationTasks,
        //                                                Task<DP14SSTTaskStateObject>[] continuationTasks2,
        //                                                Task<DP14SSTTaskStateObject>[] continuationTasks3,
        //                                                Task<DP14SSTTaskStateObject>[] continuationTasks4,
        //                                                Task<DP14SSTTaskStateObject>[] continuationTasks5,
        //                                                Task<DP14SSTTaskStateObject>[] continuationTasks6,
        //                                                Task<DP14SSTTaskStateObject>[] continuationTasks7,
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
        //private async void uploadDataFromHardware(int numOfColumns, int numOfLevels, DP14SSTTBUploadDescriptor threadParameters)
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
        //    //    ThreadLocal<DP14SSTTaskStateObject> tls = new ThreadLocal<DP14SSTTaskStateObject>();
        //    //    Mutex mutex = new Mutex();

        //    //    string folderPath = m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME + "\\TraceData_";
        //    //    long blockSize = threadParameters.NumOfStates / (numOfColumns * numOfLevels); 
        //    //    long[] fileStartIndices = getfileStartIndices (numOfColumns * numOfLevels, threadParameters.NumOfStates); 

        //    //    Task<DP14SSTTaskStateObject>[] tasks = new Task<DP14SSTTaskStateObject>[numOfColumns];
        //    //    Task<DP14SSTTaskStateObject>[][] continuationTasks = new Task<DP14SSTTaskStateObject>[numOfLevels - 1][];

        //    //    for (int i = 0; i < numOfLevels - 1; i++)
        //    //        continuationTasks[i] = new Task<DP14SSTTaskStateObject>[numOfColumns];


        //    //    Task<DP14SSTTaskStateObject> parentTask;

        //    //    // create the cancellatin token
        //    //    tokenSource = new CancellationTokenSource();
        //    //    token = tokenSource.Token;

        //    //    // numOfColumns is the number of tasks running concurrently...
        //    //    for (int column = 0; column < numOfColumns; column++)
        //    //    {
        //    //        //
        //    //        // setup one of the root task, from which other tasks will be cascaded from.
        //    //        //
        //    //        tasks[column] = new Task<DP14SSTTaskStateObject>((stateObject) => {
        //    //            tls.Value = (DP14SSTTaskStateObject)stateObject;
        //    //            ((DP14SSTTaskStateObject)tls.Value).Token.ThrowIfCancellationRequested();

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

        //    //        }, new DP14SSTTaskStateObject(folderPath, m_FTD2xxIF.TrigOffset, m_FTD2xxIF.StartStateIndex,
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
        //    //                tls.Value = (DP14SSTTaskStateObject)antecedent.Result;
        //    //                tls.Value.Index = tls.Value.Index + 1;
        //    //                ((DP14SSTTaskStateObject)antecedent.Result).Token.ThrowIfCancellationRequested();

        //    //                // get the data...
        //    //                createTraceBufferDataFileII(((DP14SSTTaskStateObject)antecedent.Result).FolderPath + tls.Value.Index.ToString() + ".bin",
        //    //                                            ((DP14SSTTaskStateObject)antecedent.Result).TrigStateIndex,
        //    //                                            ((DP14SSTTaskStateObject)antecedent.Result).StartStateIndex,
        //    //                                            ((DP14SSTTaskStateObject)antecedent.Result).FileStartIndices[tls.Value.Index],
        //    //                                            ((DP14SSTTaskStateObject)antecedent.Result).BlockSize,
        //    //                                            ((DP14SSTTaskStateObject)antecedent.Result).MemoryDepth,
        //    //                                            ((DP14SSTTaskStateObject)antecedent.Result).NumOfCapturedStates,
        //    //                                            ((DP14SSTTaskStateObject)antecedent.Result).MyFTD2xxIF,
        //    //                                            mutex,
        //    //                                            ((DP14SSTTaskStateObject)antecedent.Result).Token,
        //    //                                            ((DP14SSTTaskStateObject)antecedent.Result).UploadStatusEvent);

        //    //                return new DP14SSTTaskStateObject(tls.Value.FolderPath, tls.Value.TrigStateIndex, tls.Value.StartStateIndex,
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

        //    //        removeDataFiles(DP14SSTTRACE_BUFFER_MODE.MainLink);

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


        /// <summary>
        /// returns the number of columns used in the cascading tasks construct (needed for searching)
        /// </summary>
        /// <param name="memDepth"></param>
        /// <returns></returns>
        private int getNumberOfColumns(DP14SST_TRACE_BUFFER_MODE mode, long memoryDepth)
        {
            int numOfColumns = 0;
            if (mode == DP14SST_TRACE_BUFFER_MODE.MainLink)
            {
                if (memoryDepth <= TB_MAX_THREAD_SIZE_SMALL)
                {
                    numOfColumns = SMALL_DATA_UPLOAD_NUM_OF_COLUMNS;
                }
                else if (memoryDepth <= TB_MAX_THREAD_SIZE_MEDIUM)
                {
                    numOfColumns = MEDIUM_DATA_UPLOAD_NUM_OF_COLUMNS;
                }
                else if (memoryDepth <= TB_MAX_THREAD_SIZE_LARGE)
                {
                    numOfColumns = LARGE_DATA_UPLOAD_NUM_OF_COLUMNS;
                }
                else if (memoryDepth <= TB_MAX_THREAD_SIZE_VERY_LARGE)
                {
                    numOfColumns = LARGEST_DATA_UPLOAD_NUM_OF_COLUMNS;
                }
            }
            else if (mode == DP14SST_TRACE_BUFFER_MODE.AuxiliaryLink)
            {
                numOfColumns = 1;  // single thread (BGWorker) 
            }

            return numOfColumns;
        }


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

        ///// <summary>
        ///// Get the data from the probe and store in data file(s)
        ///// </summary>
        ///// m_TBParameters.TRIG, m_TBParameters.TBFIN, m_TBParameters.WRAP,
        ///// m_TBParameters.PRE_TRIG_LINE_COUNT, m_TBParameters.PST_TRIG_LINE_CNT,
        /////                            m_TBParameters.TIME_COUNT, m_TBParameters.TRIG_LINE, memoryDepth
        /////
        ///// http://stackoverflow.com/questions/11500563/winform-multithreading-use-backgroundworker-or-not
        ///// 
        ///// <returns></returns>
        //private bool getDataFromHardware()
        //{
        //    bool status = false; // true;
        //    //DP14SSTTBUploadDescriptor threadParameters = new DP14SSTTBUploadDescriptor(m_FTD2xxIF.StartStateIndex,
        //    //                                                                                m_FTD2xxIF.EndStateIndex,
        //    //                                                                                m_FTD2xxIF.TriggerStateIndex,
        //    //                                                                                m_FTD2xxIF.TrigOffset,
        //    //                                                                                m_FTD2xxIF.NumOfCapturedStates,
        //    //                                                                                m_FTD2xxIF.CaptureWrapped,
        //    //                                                                                m_memoryDepth,
        //    //                                                                                TB_PAGE_BYTE_LENGTH,
        //    //                                                                                TB_STATE_BYTE_LEN,
        //    //                                                                                TBDataUploadStatusEvent);

        //    //// raise an event to the parent form to disable all other forms... we are busy uploading data...
        //    //if (TBDataUploadStatusEvent != null)
        //    //    TBDataUploadStatusEvent(this, new HWDataUploadStatusEventArgs("Data Upload Started", true));  // <<== this caused Gen2.processTBDataUploadEvent() to de-assert ABRUN and TBRUN

        //    //if (MemoryDepth <= TB_MAX_THREAD_SIZE_SMALL)
        //    //{
        //    //    uploadDataFromHardware(SMALL_DATA_UPLOAD_NUM_OF_COLUMNS, SMALL_DATA_UPLOAD_NUM_OF_LEVELS, threadParameters);  //2 Columns, 2 Levels
        //    //}
        //    //else if (MemoryDepth <= TB_MAX_THREAD_SIZE_MEDIUM)
        //    //{
        //    //    uploadDataFromHardware(MEDIUM_DATA_UPLOAD_NUM_OF_COLUMNS, MEDIUM_DATA_UPLOAD_NUM_OF_LEVELS, threadParameters); //4 Columns, 4 Levels);
        //    //}
        //    //else if (MemoryDepth <= TB_MAX_THREAD_SIZE_LARGE)
        //    //{
        //    //    uploadDataFromHardware(LARGE_DATA_UPLOAD_NUM_OF_COLUMNS, LARGE_DATA_UPLOAD_NUM_OF_LEVELS, threadParameters); //4 Columns, 8 Levels);
        //    //}
        //    //else if (MemoryDepth <= TB_MAX_THREAD_SIZE_VERY_LARGE)
        //    //{
        //    //    uploadDataFromHardware(LARGEST_DATA_UPLOAD_NUM_OF_COLUMNS, LARGEST_DATA_UPLOAD_NUM_OF_LEVELS, threadParameters); //8 Columns, 8 Levels);
        //    //}
        //    return status;
        //}


        ///// <summary>
        ///// Get the data from the probe and store in data file(s)
        ///// </summary>
        ///// 
        ///// m_TBParameters.TRIG, m_TBParameters.TBFIN, m_TBParameters.WRAP,
        ///// m_TBParameters.PRE_TRIG_LINE_COUNT, m_TBParameters.PST_TRIG_LINE_CNT,
        /////                            m_TBParameters.TIME_COUNT, m_TBParameters.TRIG_LINE, memoryDepth
        /////
        ///// <returns></returns>
        //private bool getAuxDataFromHardware()
        //{
        //    bool status = false; // true;
        //    //DP14SSTTBUploadDescriptor threadParameters = new DP14SSTTBUploadDescriptor(m_FTD2xxIF.StartStateIndex_Aux,
        //    //                                                                                m_FTD2xxIF.EndStateIndex_Aux,
        //    //                                                                                m_FTD2xxIF.TriggerStateIndex_Aux,
        //    //                                                                                m_FTD2xxIF.TrigOffset_Aux,
        //    //                                                                                m_FTD2xxIF.NumOfCapturedStates_Aux,
        //    //                                                                                m_FTD2xxIF.CaptureWrapped_Aux,
        //    //                                                                                m_auxMemoryDepth);

        //    //// raise an event to the parent form to disable all other forms... we are busy uploading data...
        //    //if (TBDataUploadStatusEvent != null)
        //    //    TBDataUploadStatusEvent(this, new HWDataUploadStatusEventArgs("Auxiliary Upload Started", true));


        //    //// always will be in a back ground work thread... data size is assumed to be 16K states.
        //    //m_TBDataMode = DP14SSTTBDataMgrMode.SingleThreaded;
        //    //uploadData_BGWorkerMode(threadParameters, false);

        //    return status;
        //}


        ///// <summary>
        ///// create the binary file(s).
        ///// </summary>
        ///// <returns></returns>
        //private bool uploadData(long memoryDepth)
        //{
        //    bool status = false;

        //    //// call the method to get the starting and ending indices
        //    //status = createTBDataStatistics(memoryDepth);
        //    //if (status)
        //    //{
        //    //    // remove old/previous trace buffer data
        //    //    clearDataUploadFolder();

        //    //    // get the data and store in file(s)
        //    //    status = getDataFromHardware();
        //    //}

        //    return status;
        //}


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
        private List<string> getDataFileNames()
        {
            List<string> fileNames = new List<string>();

            // clear the folder of all files...
            DirectoryInfo di = new DirectoryInfo(m_instanceFolderPath); // m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME);
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
        //public List<DP14SSTVCFileGenerator_Threads.DP14SSTMultiThread_MetaDataArgs> getMultiThreadedFileMetaData()
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
        private void getRequestedData(DP14SSTMessage DisplayPortEvent)
        {
            bool status = false;

            if (m_TBDataMode == DP14SST_TBDataMgrMode.SingleThreaded)
            {
                // if the TB file exists...
                if (File.Exists(m_traceFilePath))
                {
                    if (m_currentStateNumber != ((DP14SSTMessage_StateDataRequest)DisplayPortEvent).StateNumber)
                    {
                        // clear the previous state data 
                        Array.Clear(m_currentStateData, 0, m_currentStateData.Length);

                        // get the requested state data
                        status = getState(((DP14SSTMessage_StateDataRequest)DisplayPortEvent).StateNumber, (int)m_dataOffset_mainLink);
                        if (!status)
                        {
                            m_currentStateNumber = long.MinValue;
                            Array.Clear(m_currentStateData, 0, m_currentStateData.Length);
                        }
                    }
                }
            }
            else if (m_TBDataMode == DP14SST_TBDataMgrMode.MultiThreaded)
            {
                if (m_multiThreadFileSize != 0)
                {
                    int index = (int)((DP14SSTMessage_StateDataRequest)DisplayPortEvent).StateNumber / m_multiThreadFileSize;
                    if ((index >= 0) && (index <= m_multiThreadFileMetaData.Count))
                    {
                        status = getState(index, ((DP14SSTMessage_StateDataRequest)DisplayPortEvent).StateNumber);
                        if (!status)
                        {
                            m_currentStateNumber = long.MinValue;
                            Array.Clear(m_currentStateData, 0, m_currentStateData.Length);
                        }
                    }
                }
            }
            else
            {
                m_currentStateNumber = long.MinValue;
                Array.Clear(m_currentStateData, 0, m_currentStateData.Length);
            }

            // either way... send the response.
            if (DisplayPort13SSTEvent != null)
            {
                // Raise the response event containing the requested data
                DisplayPort13SSTEvent(this,
                                    new DP14SSTEventArgs(m_DP14SSTMessageGenerator.DP14SSTMessageCreate_StateDataResponse(((DP14SSTMessage_StateDataRequest)DisplayPortEvent).ID,
                                                       ((DP14SSTMessage_StateDataRequest)DisplayPortEvent).StateNumber,
                                                       m_currentStateData)));
            }
        }



        /// <summary>
        /// Get a specified sized chunk of data...
        /// </summary>
        /// <param name="DisplayPortEvent"></param>
        private void getRequestedDataChunk(DP14SSTMessage DisplayPortEvent)
        {
            bool status = false;
            DP14SSTMessage_StateDataChunkRequest msg = DisplayPortEvent as DP14SSTMessage_StateDataChunkRequest;

            if (m_TBDataMode == DP14SST_TBDataMgrMode.SingleThreaded)
            {
                if (File.Exists(m_traceFilePath))
                {
                    msg.ClearDataChunk();
                    status = getStateChunk_SingleThreaded((int)m_dataOffset_mainLink, msg.StateNumber, msg.StateNumber + msg.ChunkSize, msg.DataChunk, msg.ChunkSize);
                }
            }
            else if (m_TBDataMode == DP14SST_TBDataMgrMode.MultiThreaded)
            {
                int index = getFileIndex(msg.StateNumber, m_multiThreadFileMetaData);
                if ((index >= 0) && (index <= m_multiThreadFileMetaData.Count))
                {
                    int fileStartIndex = -1;
                    int fileEndIndex = -1;
                    int dataOffset = -1;
                    string path = generateMultiThreadFileName(index);

                    msg.ClearDataChunk();
                    getMultiTrheadedStateIndices(path, ref fileStartIndex, ref fileEndIndex, ref dataOffset);
                    status = getStateChunk(index, msg.StateNumber, fileEndIndex, msg.DataChunk, msg.ChunkSize);
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
        public bool getRequestedStateData(DP14SSTMessage DisplayPortEvent)
        {
            bool status = false;
            DP14SSTMessage_GetStateData msg = DisplayPortEvent as DP14SSTMessage_GetStateData;
            byte[] dataBytes = new byte[TB_STATE_BYTE_LEN];

            if (DisplayPortEvent is DP14SSTMessage_GetStateData)
            {
                if (m_TBDataMode == DP14SST_TBDataMgrMode.SingleThreaded)
                {
                    // if the TB file exists...
                    if (File.Exists(m_traceFilePath))
                    {
                        // clear the previous state data 
                        Array.Clear(msg.DataBytes, 0, msg.DataBytes.Length);

                        // get the requested state data
                        status = getStateData(msg.StateIndex, (int)m_dataOffset_mainLink, ref dataBytes);
                        if (status)
                            Array.ConstrainedCopy(dataBytes, 0, msg.DataBytes, 0, msg.DataBytes.Length);
                        else
                            Array.Clear(msg.DataBytes, 0, msg.DataBytes.Length);
                    }
                }
                else if (m_TBDataMode == DP14SST_TBDataMgrMode.MultiThreaded)
                {
                    int fileIndex = (int)msg.StateIndex / m_multiThreadFileSize;
                    if ((fileIndex >= 0) && (fileIndex <= m_multiThreadFileMetaData.Count))
                    {
                        status = getStateData(fileIndex, msg.StateIndex, ref dataBytes);
                        if (status)
                            Array.ConstrainedCopy(dataBytes, 0, msg.DataBytes, 0, msg.DataBytes.Length);
                        else
                            Array.Clear(msg.DataBytes, 0, msg.DataBytes.Length);
                    }
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
        public bool getRequestedAuxStateData(DP14SSTMessage DisplayPortEvent)
        {
            bool status = false;
            DP14SSTMessage_AuxGetStateData msg = DisplayPortEvent as DP14SSTMessage_AuxGetStateData;
            byte[] dataBytes = new byte[TB_STATE_BYTE_LEN];

            if (DisplayPortEvent is DP14SSTMessage_AuxGetStateData)
            {
                if (m_TBDataMode == DP14SST_TBDataMgrMode.SingleThreaded)
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
                //else if (m_TBDataMode == DP14SSTTBDataMgrMode.MultiThreaded)
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
        /// Extract a single data entry from the Main Link binary trace buffer  data file.
        /// </summary>
        /// <param name="curIndex"></param>
        /// <returns></returns>
        /// Assumuptions:  Called for single threaded files only.
        /// 
        private bool getState(long stateIndex, int dataOffset)
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


        /// <summary>
        /// Extract a single data entry from the Main Link binary trace buffer data file.
        /// </summary>
        /// <param name="curIndex"></param>
        /// <returns></returns>
        private bool getState(int fileIndex, long stateIndex)
        {
            bool status = false;
            Array.Clear(m_currentStateData, 0, m_currentStateData.Length);

            // Determines which sequencial data file contains the requested state and assembles a file name
            string fileName = generateMultiThreadFileName(fileIndex);


            if (File.Exists(fileName))
            {
                // extract the state data from the specified file.
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    //if (fs.Length > (m_multiThreadFileMetaData[index].DataOffset +
                    //                    (m_multiThreadFileMetaData[index].EndState - m_multiThreadFileMetaData[index].StartState) * TB_STATE_BYTE_LEN))
                    if (fs.Length > (m_multiThreadFileMetaData[fileIndex].DataOffset +
                                            (m_multiThreadFileMetaData[fileIndex].EndState - stateIndex) * TB_STATE_BYTE_LEN))
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

                        int stateOffset = ((m_multiThreadFileMetaData[fileIndex].EndState - m_multiThreadFileMetaData[fileIndex].StartState) - (m_multiThreadFileMetaData[fileIndex].EndState - (int)stateIndex)) * 16;
                        fs.Position = m_multiThreadFileMetaData[fileIndex].DataOffset + stateOffset;
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
        /// Extract a chunk of states from a single threaded trace buffer data file.
        /// </summary>
        /// <param name="dataOffset"></param>
        /// <param name="stateIndex"></param>
        /// <param name="fileEndIndex"></param>
        /// <param name="dataChunk"></param>
        /// <param name="chunkSize"></param>
        /// <returns></returns>
        private bool getStateChunk_SingleThreaded(int dataOffset, long stateIndex, long fileEndIndex, List<byte> dataChunk, int chunkSize)
        {
            bool status = false;
            int stateCount = 0;

            using (FileStream fs = new FileStream(m_traceFilePath, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length > (dataOffset + (stateIndex * TB_STATE_BYTE_LEN)))
                {
                    fs.Position = dataOffset + (stateIndex * TB_STATE_BYTE_LEN);
                    using (BinaryReader br = new BinaryReader(fs, new ASCIIEncoding()))
                    {
                        for (long i = stateIndex; (i <= fileEndIndex) && (stateCount < chunkSize); i++)
                        {
                            if (fs.Position <= (fs.Length - TB_STATE_BYTE_LEN))
                            {
                                dataChunk.AddRange(br.ReadBytes(TB_STATE_BYTE_LEN));
                                stateCount += 1;
                            }
                            else
                            {
                                break;
                            }
                        }
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
        private bool getStateChunk(int fileIndex, long stateIndex, long fileEndIndex, List<byte> dataChunk, int chunkSize)
        {
            bool status = false;
            byte[] stateData = new byte[TB_STATE_BYTE_LEN];
            int stateCount = 0;

            // Determines which sequencial data file contains the requested state and assembles a file name
            string fileName = generateMultiThreadFileName(fileIndex);

            if (((stateIndex + chunkSize) - 1) > fileEndIndex)
            {
                status = false;
            }


            if (File.Exists(fileName))
            {
                // extract the state data from the specified file.
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    if (fs.Length > (m_multiThreadFileMetaData[fileIndex].DataOffset +
                                            (m_multiThreadFileMetaData[fileIndex].EndState - stateIndex) * TB_STATE_BYTE_LEN))
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

                        int stateOffset = ((m_multiThreadFileMetaData[fileIndex].EndState - m_multiThreadFileMetaData[fileIndex].StartState) - (m_multiThreadFileMetaData[fileIndex].EndState - (int)stateIndex)) * 16;
                        fs.Position = m_multiThreadFileMetaData[fileIndex].DataOffset + stateOffset;
                        using (BinaryReader br = new BinaryReader(fs, new ASCIIEncoding()))
                        {
                            status = true;
                            for (long i = stateIndex; (i <= fileEndIndex) && (stateCount < chunkSize); i++)
                            {
                                if (fs.Position <= (fs.Length - TB_STATE_BYTE_LEN))
                                {
                                    //Array.Copy(br.ReadBytes(TB_STATE_BYTE_LEN), stateData, stateData.Length);
                                    //dataChunk.AddRange(stateData);
                                    dataChunk.AddRange(br.ReadBytes(TB_STATE_BYTE_LEN));
                                    stateCount += 1;
                                }
                                else
                                {
                                    break;   // we hit the end of the file and we are done!
                                }
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

        /// <summary>
        /// Extract a single data entry from the Main Link binary trace buffer data file. 
        /// </summary>
        /// <param name="curIndex"></param>
        /// <returns></returns>
        /// Assumuptions:  Called for single threaded files only.
        /// 
        private bool getStateData(long stateIndex, int dataOffset, ref byte[] stateData)
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


        /// <summary>
        /// Get the data for a specific state.
        /// </summary>
        /// <param name="stateIndex"></param>
        /// <param name="dataOffset"></param>
        /// <param name="stateData"></param>
        /// <returns></returns>
        /// Assumuptions:  Called for Multi-threaded files only.
        ///
        private bool getStateData(int fileIndex, long stateIndex, ref byte[] stateData)
        {
            bool status = true;
            // Determines which sequencial data file contains the requested state and assembles a file name
            string fileName = generateMultiThreadFileName(fileIndex);


            if (File.Exists(fileName))
            {
                // extract the state data from the specified file.
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    if (fs.Length > (m_multiThreadFileMetaData[fileIndex].DataOffset +
                                            (m_multiThreadFileMetaData[fileIndex].EndState - stateIndex) * TB_STATE_BYTE_LEN))
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

                        int stateOffset = ((m_multiThreadFileMetaData[fileIndex].EndState - m_multiThreadFileMetaData[fileIndex].StartState) - (m_multiThreadFileMetaData[fileIndex].EndState - (int)stateIndex)) * 16;
                        fs.Position = m_multiThreadFileMetaData[fileIndex].DataOffset + stateOffset;
                        using (BinaryReader br = new BinaryReader(fs, new ASCIIEncoding()))
                        {
                            Array.Copy(br.ReadBytes(TB_STATE_BYTE_LEN), stateData, stateData.Length);
                            status = true;
                        }
                    }
                }
            }

            return status;
        }



        /// <summary>
        /// Get the file index containing the requested state index.
        /// </summary>
        /// <param name="stateIndex"></param>
        /// <returns></returns>
        private int getFileIndex(long stateIndex)
        {
            bool status = true;
            int fileIndex = -1;
            string path = string.Empty;
            int fileStartIndex = 0x00;
            int fileEndIndex = 0x00;
            int dataOffset = 0x00;


            for (int i = 0; i < m_numOfTPITasks; i++)
            {
                path = generateMultiThreadFileName(i);
                if (File.Exists(path))
                {
                    status = getMultiTrheadedStateIndices(path, ref fileStartIndex, ref fileEndIndex, ref dataOffset);
                    if (status)
                    {
                        if (stateIndex >= fileStartIndex)
                        {
                            fileIndex = i;
                            break; // exi tthe for loop
                        }
                    }
                }
            }

            return fileIndex;
        }


        /// <summary>
        /// Get the file index based on the meta data args 
        /// </summary>
        /// <param name="stateIndex"></param>
        /// <param name="metaDataList"></param>
        /// <returns></returns>
        private int getFileIndex(long stateIndex, List<DP14SST_MultiThread_MetaDataArgs> metaDataList)
        {
            int fileIndex = -1;

            for (int i = 0; i < metaDataList.Count; i++)
            {
                if ((stateIndex >= metaDataList[i].StartState) && (stateIndex <= metaDataList[i].EndState))
                {
                    fileIndex = i;
                    break;
                }
            }

            return fileIndex;
        }


        /// <summary>
        /// Get the file index in which the given state Index is located within
        /// </summary>
        /// <param name="stateIndex"></param>
        /// <returns></returns>
        private int getFileIndex_inclusive(DP14SST_TRACE_BUFFER_MODE TBMode, long stateIndex)
        {
            bool status = true;
            int fileIndex = -1;
            string path = string.Empty;
            int fileStartIndex = 0x00;
            int fileEndIndex = 0x00;
            int dataOffset = 0x00;
            int numOfFiles = getNumberOfColumns(TBMode, m_memoryDepth);

            for (int i = 0; i < numOfFiles; i++)
            {
                path = generateMultiThreadFileName(i);
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
        /// Get the Trace buffer meta data that is spread across several files (multi-threaded data)
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        /// Assumption:  this method extracts the meta data for the data files for the specified channel, 
        ///              NOT the meta data for all data files...
        private bool getMultiThreadedFileMetaData(DP14SSTMessage_GetTBBinFileMetaData msg)
        {
            bool status = true;
            bool locatedFile = true;
            string path = string.Empty;
            int delimiterCount = 0;
            int parameterValue = -1;
            int startState = -1;
            int endState = -1;
            int totalStates = 0;
            int triggerOffset = -1;


            for (int i = 0; locatedFile == true; i++)
            {
                delimiterCount = 0;
                parameterValue = -1;
                startState = -1;
                endState = -1;
                path = generateMultiThreadFileName(i);

                if (File.Exists(path))
                {
                    // create an object to hold the meta data for the current file.
                    DP14SST_MultiThread_MetaDataArgs metaDataArgs = new DP14SST_MultiThread_MetaDataArgs();

                    StringBuilder sb = new StringBuilder();
                    sb.Append("FileIndex:" + i.ToString() + ";");

                    // open the binary file and read all lines between the lines that begin with "*****"
                    using (TextReader sr = File.OpenText(path))
                    {
                        sb.Length = 0;
                        string s = "";
                        while (((s = sr.ReadLine()) != null) && (delimiterCount < 1) && (status == true))
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
                                    else if (comps[0] == BIN_FILE_MULTI_HEADER_INFO_TRIG_OFFSET)
                                    {
                                        triggerOffset = parameterValue;
                                    }
                                    else
                                        status = false;
                                }
                                //dataOffset += s.Length + 1;
                            }
                            metaDataArgs.DataOffset += s.Length + 1;
                        }

                        metaDataArgs.DataOffset = metaDataArgs.DataOffset + 1;

                        // store the object holding the meta data into a list... should be sequencial.
                        m_multiThreadFileMetaData.Add(metaDataArgs);

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
                    locatedFile = false;
                }
            }

            // add the global meta data parameters
            msg.TriggerOffset = triggerOffset;
            msg.NumberOfStates = totalStates;
            //msg.DataType = TBDataType.MainLinkMultiThread;

            return status;
        }


        /// <summary>
        /// Extract the binary file's meta data (located at the beginning of the file).
        /// </summary>
        /// <param name="path"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool getSingleThreadedFileMetaData(string path, DP14SSTMessage_GetTBBinFileMetaData msg)
        {
            bool status = true;
            int delimiterCount = 0;
            int parameterValue = -1;


            string filePath = m_traceFilePath;
            if (msg.TBMode == DP14SST_TRACE_BUFFER_MODE.AuxiliaryLink)
                filePath = m_auxTraceFilePath;


            if (msg.TBMode == DP14SST_TRACE_BUFFER_MODE.MainLink)
                m_dataOffset_mainLink = 0;
            else
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

                        if (msg.TBMode == DP14SST_TRACE_BUFFER_MODE.MainLink)
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
        private string generateMultiThreadFileName(int index)
        {
           // return (Path.Combine(m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME, m_FS4500_MULTI_THREAD_TRACE_FILE_BASE_NAME + index.ToString() + ".bin"));
            return (Path.Combine(m_instanceFolderPath, m_FS4500_MULTI_THREAD_TRACE_FILE_BASE_NAME + index.ToString() + ".bin"));
        }


        /// <summary>
        /// Determine the type of data contained in the TB data file(s)
        /// </summary>
        /// <returns></returns>
        private DP14SST_TBDataMgrMode getMainLinkTBDataFormat()
        {
            DP14SST_TBDataMgrMode mode = DP14SST_TBDataMgrMode.Unknown;

            if (File.Exists(m_traceFilePath))
            {
                mode = DP14SST_TBDataMgrMode.SingleThreaded;
            }
            else
            {
                // generate the file name that would be used for the first of several binary files containing the trace buffer data
                string path = generateMultiThreadFileName(0);
                if (File.Exists(path))
                    mode = DP14SST_TBDataMgrMode.MultiThreaded;
            }

            return mode;
        }


        /// <summary>
        /// Get the meta data in the header section of the binary trace file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool getTBMetaData(DP14SSTMessage DisplayPortEvent)
        {
            bool status = true;

            if (((DP14SSTMessage_GetTBBinFileMetaData)DisplayPortEvent).TBMode == DP14SST_TRACE_BUFFER_MODE.MainLink)
            {
                m_TBDataMode = getMainLinkTBDataFormat();
                if (m_TBDataMode == DP14SST_TBDataMgrMode.SingleThreaded)
                {
                    status = getSingleThreadedFileMetaData(m_traceFilePath, ((DP14SSTMessage_GetTBBinFileMetaData)DisplayPortEvent));
                }
                else if (m_TBDataMode == DP14SST_TBDataMgrMode.MultiThreaded)
                {
                    m_multiThreadFileMetaData.Clear();
                    status = getMultiThreadedFileMetaData((DP14SSTMessage_GetTBBinFileMetaData)DisplayPortEvent);
                    if (status)
                        m_multiThreadFileSize = m_multiThreadFileMetaData[0].EndState + 1; // assumes the first file always starts at state # 0
                }
            }
            else if (((DP14SSTMessage_GetTBBinFileMetaData)DisplayPortEvent).TBMode == DP14SST_TRACE_BUFFER_MODE.AuxiliaryLink)
            {
                getSingleThreadedFileMetaData(m_auxTraceFilePath, ((DP14SSTMessage_GetTBBinFileMetaData)DisplayPortEvent));
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
        private bool getFieldParameters(DP14SST_TRACE_BUFFER_MODE TBMode, string fieldName, ref int width, ref int offset)
        {
            bool status = false;

            List<DP14SST_ColumnMetaData> metaDataList = m_columnMetaData_MainLink;
            if (TBMode == DP14SST_TRACE_BUFFER_MODE.AuxiliaryLink)
                metaDataList = m_columnMetaData_AuxiliaryLink;


            foreach (DP14SST_ColumnMetaData metaData in metaDataList)
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
        private bool searchListing_SingleThead(DP14SSTMessage_SearchListingRequest searchMsg, ref long locatedStateIndex)
        {
            bool found = false;
            //bool searchForward = true;

            //if (searchMsg.PreviousStates)
            //    searchForward = false;

            //DP14SST_TBSearchDescriptor threadParameters = new DP14SST_TBSearchDescriptor(searchMsg.TBMode, searchMsg.ColumnName, searchMsg.m_ColumnHexValue, searchMsg.StateIndex, searchForward);

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
        private bool searchListing_MultiThead(DP14SSTMessage_SearchListingRequest searchMsg, ref long locatedStateIndex)
        {
            bool found = false;
            //bool searchForward = true;

            //if (searchMsg.PreviousStates)
            //    searchForward = false;

            //DP14SST_TBSearchDescriptor threadParameters = new DP14SST_TBSearchDescriptor(searchMsg.TBMode, searchMsg.ColumnName, searchMsg.m_ColumnHexValue, searchMsg.StateIndex, searchForward);

            ////initalize the background worker thread and start the background worker
            //initSearchBGWorker_MultiThreaded(threadParameters);


            ////if (!searchMsg.PreviousStates)
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
        private long searchListing_MultiThread_Forward(DP14SST_TRACE_BUFFER_MODE TBMode, string cName, uint cValue, int vChannelID, long stateIndex, int interval = 1024)
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
            int numOfFiles = getNumberOfColumns(TBMode, m_memoryDepth);


            // figure out which file the starting stateindex is in.
            int fileIndex = getFileIndex(curStateIndex);
            if (fileIndex >= 0)
            {
                //status = getFieldParameters(TRACE_BUFFER_MODE.MainLink, searchMsg.ColumnName, ref fldWidth, ref fldOffset);
                status = getFieldParameters(DP14SST_TRACE_BUFFER_MODE.MainLink, cName, ref fldWidth, ref fldOffset);
                if (status)
                    status = LoopOperations.GetFieldLocation(TB_STATE_BYTE_LEN, fldOffset, ref byteID, ref bitID);
            }



            // search the files beginning with the given file assoicated with the fileIndex value.  
            //for (int index = fileIndex; index < m_numOfTPITasks && !found; index++)
            for (int index = fileIndex; index < numOfFiles && !found; index++)
            {
                path = generateMultiThreadFileName(index);
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
                                while (curStateIndex <= endState) // &&  fs.Length > (dataOffset + (curStateIndex * TB_STATE_BYTE_LEN)))
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
        private long searchListing_MultiThread_Backward(DP14SST_TRACE_BUFFER_MODE TBMode, string cName, uint cValue, int vChannelID, long stateIndex, int interval = 1024)
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
            int fileIndex = getFileIndex_inclusive(TBMode, curStateIndex); ;
            if (fileIndex >= 0)
            {
                //status = getFieldParameters(TRACE_BUFFER_MODE.MainLink, searchMsg.ColumnName, ref fldWidth, ref fldOffset);
                status = getFieldParameters(DP14SST_TRACE_BUFFER_MODE.MainLink, cName, ref fldWidth, ref fldOffset);
                if (status)
                    status = LoopOperations.GetFieldLocation(TB_STATE_BYTE_LEN, fldOffset, ref byteID, ref bitID);
            }



            // search the files beginning with the given file assoicated with the fileIndex value.  
            for (int index = fileIndex; (index >= 0) && !found; index--)
            {
                path = generateMultiThreadFileName(index);
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
        private long searchListing_SingleThread_Forward(DP14SST_TRACE_BUFFER_MODE TBMode, string cName, uint cValue, long stateIndex, int interval = 1024)
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
            DP14SST_TRACE_BUFFER_MODE traceBufferMode = DP14SST_TRACE_BUFFER_MODE.MainLink;
            //if (searchMsg.TBMode == TRACE_BUFFER_MODE.AuxiliaryLink)
            if (TBMode == DP14SST_TRACE_BUFFER_MODE.AuxiliaryLink)
            {
                filePath = m_auxTraceFilePath;
                traceBufferMode = DP14SST_TRACE_BUFFER_MODE.AuxiliaryLink;
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
        private long searchListing_SingleThread_Backward(DP14SST_TRACE_BUFFER_MODE TBMode, string cName, uint cValue, long stateIndex, int interval = 1024)
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
            DP14SST_TRACE_BUFFER_MODE traceBufferMode = DP14SST_TRACE_BUFFER_MODE.MainLink;
            //if (searchMsg.TBMode == TRACE_BUFFER_MODE.AuxiliaryLink)
            if (TBMode == DP14SST_TRACE_BUFFER_MODE.AuxiliaryLink)
            {
                filePath = m_auxTraceFilePath;
                traceBufferMode = DP14SST_TRACE_BUFFER_MODE.AuxiliaryLink;
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
        private bool searchTraceBufferData(DP14SSTMessage DisplayPortEvent)
        {
            // initialize a background thread

            // start the thread...

            bool found = false;
            long stateIndex = -1;

            if (DisplayPortEvent is DP14SSTMessage_SearchListingRequest)
            {
                DP14SSTMessage_SearchListingRequest searchMsg = DisplayPortEvent as DP14SSTMessage_SearchListingRequest;
                if (searchMsg.TBMode == DP14SST_TRACE_BUFFER_MODE.MainLink)
                {
                    if (m_TBDataMode == DP14SST_TBDataMgrMode.SingleThreaded)
                    {
                        m_MainLinkSingleThreadSearch = true;
                        found = searchListing_SingleThead(searchMsg, ref stateIndex);
                    }
                    else if (m_TBDataMode == DP14SST_TBDataMgrMode.MultiThreaded)
                    {
                        m_MainLinkSingleThreadSearch = false;
                        found = searchListing_MultiThead(searchMsg, ref stateIndex);
                    }
                    else
                        found = false;
                }
                else if (searchMsg.TBMode == DP14SST_TRACE_BUFFER_MODE.AuxiliaryLink)
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
        private bool removeDataFiles(DP14SST_TRACE_BUFFER_MODE TBModeID)
        {
            bool status = true;
            int maxRetries = 10;
            for (int attempt = 0; attempt < 5; attempt++)
            {
                try
                {
                    if (TBModeID == DP14SST_TRACE_BUFFER_MODE.MainLink)
                    {
                        if (File.Exists(m_traceFilePath))
                            File.Delete(m_traceFilePath);

                        for (int i = 0; i < m_numOfTPITasks; i++)
                        {
                            string path = generateMultiThreadFileName(i);
                            if (File.Exists(path))
                                File.Delete(path);
                        }
                    }
                    else if (TBModeID == DP14SST_TRACE_BUFFER_MODE.AuxiliaryLink)
                    {
                        if (File.Exists(m_auxTraceFilePath))
                            File.Delete(m_auxTraceFilePath);
                    }
                    else if (TBModeID == DP14SST_TRACE_BUFFER_MODE.Both)
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
        private List<string> getDataFileNames(DP14SST_TBDataType linkID)
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
                    if (linkID == DP14SST_TBDataType.AuxiliaryLinkSingleThread)
                    {
                        if (fName == "AuxTraceData.bin")
                        {
                            fileNames.Add(fName);
                            break;
                        }
                    }
                    else // Main link
                    {
                        if (linkID == DP14SST_TBDataType.MainLinkSingleThread)
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
        private bool getColumnMetaInfo(string path, ref List<DP14SST_ColumnMetaData> columnMetaDataList)
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
                            DP14SST_ColumnMetaData cInfo = new DP14SST_ColumnMetaData();

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
        private void getColumnMetaData(DP14SST_TRACE_BUFFER_MODE TBMode)
        {
            string xmlfilePath = "";
            if (TBMode == DP14SST_TRACE_BUFFER_MODE.MainLink)
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
                List<string> mainLinkFileNames_Single = getDataFileNames(DP14SST_TBDataType.MainLinkSingleThread);
                List<string> mainLinkFileNames_Multi = getDataFileNames(DP14SST_TBDataType.MainLinkMultiThread);
                List<string> auxLinkFileNames_Single = getDataFileNames(DP14SST_TBDataType.AuxiliaryLinkSingleThread);

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
            //    }


            //    if (status == false)
            //        MessageBox.Show("Error Reloading Trace Buffer data files", "Error Loading Saved Config", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
            if (DisplayPort13SSTEvent != null)
                DisplayPort13SSTEvent(this, new DP14SSTEventArgs((DP14SSTMessage)(m_DP14SSTMessageGenerator.DP14SSTMessageCreate_Register((ITBObject)this))));
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
        public void ProcessDPMessage(DP14SSTMessage DisplayPortEvent)
        {
            if (DisplayPortEvent is DP14SSTMessage_PMRunning)
            {
                //m_ExitRunMode = false; // true;
                //m_multiThreadFileSize = 0;

                //// call the method to remove the old binary files. -- assumes all files are closed.
                //removeDataFiles(m_TBMode);
            }
            else if (DisplayPortEvent is DP14SSTMessage_PMStopping_Request)
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
            else if (DisplayPortEvent is DP14SSTMessage_PMStopping_Ack)
            {
                //TBD
            }
            else if (DisplayPortEvent is DP14SSTMessage_PMReady)
            {
                // do nothing... the event was generated from this object!
            }
            else if (DisplayPortEvent is DP14SSTMessage_DataAvailable)
            {
                //m_ExitRunMode = ((DP12MSTMessage_DataAvailable)DisplayPortEvent).ExitRunMode;
                //uploadTraceBufferData(DisplayPortEvent);
            }
            else if (DisplayPortEvent is DP14SSTMessage_DataUploaded)
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
            else if (DisplayPortEvent is DP14SSTMessage_StateDataRequest)
            {
                // retain the state for subsequent requests to the same state... saves on overhead.
                getRequestedData(DisplayPortEvent);
            }
            else if (DisplayPortEvent is DP14SSTMessage_StateDataChunkRequest)
            {
                // retain the state for subsequent requests to the same state... saves on overhead.
                getRequestedDataChunk(DisplayPortEvent);
            }
            else if (DisplayPortEvent is DP14SSTMessage_GetStateData)
            {
                getRequestedStateData(DisplayPortEvent);
            }
            else if (DisplayPortEvent is DP14SSTMessage_AuxDataAvailable)
            {
                //m_ExitRunMode = ((DP12MSTMessage_AuxDataAvailable)DisplayPortEvent).ExitRunMode;
                //uploadAuxTraceBufferData(DisplayPortEvent);
            }
            else if (DisplayPortEvent is DP14SSTMessage_AuxStateDataRequest)
            {
                //// retain the state for subsequent requests to the same state... saves on overhead.
                //getAuxRequestedData(DisplayPortEvent);
            }
            else if (DisplayPortEvent is DP14SSTMessage_AuxGetStateData)
            {
                //getRequestedAuxStateData(DisplayPortEvent);
            }
            else if (DisplayPortEvent is DP14SSTMessage_MemoryDepthChanged)
            {
                //// retain the state for subsequent requests to the same state... saves on overhead.
                //m_memoryDepth = ((DP12MSTMessage_MemoryDepthChanged)DisplayPortEvent).MemoryDepth;
                //m_numOfDataFiles = calculateNumOfDataBlocks(m_memoryDepth) * 4;   // capture is made up of blocks of data; each having 4 virtual channels.
            }
            else if (DisplayPortEvent is DP14SSTMessage_TimeSlotSelectionChanged)
            {
                //m_VCFileGenerator.UpdateSelectedSlotsInfo((DP12MSTMessage_TimeSlotSelectionChanged)DisplayPortEvent);
            }
            //else if (DisplayPortEvent is DP12MSTMessage_TriggerChannelChanged)
            //{
            //    m_triggerChannelID = ((DP12MSTMessage_TriggerChannelChanged)DisplayPortEvent).TriggerChannelID;
            //}
            else if (DisplayPortEvent is DP14SSTMessage_TBDataModeChange)
            {
                //m_TBMode = ((DP12MSTMessage_TBDataModeChange)DisplayPortEvent).TBModeID;
            }
            else if (DisplayPortEvent is DP14SSTMessage_GetTBBinFileMetaData)
            {
                getTBMetaData(DisplayPortEvent);
            }
            else if (DisplayPortEvent is DP14SSTMessage_SearchListingRequest)
            {
                //searchTraceBufferData(DisplayPortEvent);
            }
            else if (DisplayPortEvent is DP14SSTMessage_SearchListingCancelRequest)
            {
                //cancelSearch();
            }
            else if (DisplayPortEvent is DP14SSTMessage_Initialize)
            {
                //if (((DP12MSTMessage_Initialize)DisplayPortEvent).RemoveDataFiles)
                //{
                //    removeDataFiles(DP14SSTTRACE_BUFFER_MODE.MainLink);
                //    removeDataFiles(DP14SSTTRACE_BUFFER_MODE.AuxiliaryLink);
                //}
            }
            else if (DisplayPortEvent is DP14SSTMessage_GetNumberOfStates)
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
