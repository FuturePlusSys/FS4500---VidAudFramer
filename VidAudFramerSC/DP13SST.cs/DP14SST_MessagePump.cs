﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP14SSTClassLibrary
{
    public enum DP14SSTMessageID
    {
        PMReady = 1,
        PMRunning = 2,
        PMStopping_Request = 3,
        PMStopping_Ack = 4,
        DataAvailable = 5,
        DataReady = 6,
        StateDataRequest = 7,
        StateDateResponse = 8,
        AuxDataAvailable = 9,
        AuxStateDataRequest = 10,
        AuxStateDataResponse = 11,
        ListingReady = 12,
        TBObjectRegister = 13,
        MemoryDepthChanged = 14,
        TBModeChanged = 15
    };


    /// <summary>
    /// Display Port Event (Base) Class
    /// </summary>
    public class DP14SSTMessage
    {
        #region Members

        protected string m_Name = string.Empty;
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        protected long m_ID = 0;
        public long ID
        {
            get { return m_ID; }
            set { m_ID = value; }
        }

        public object DPEventArgs { get; set; }

        #endregion // Members

        #region Constructors

        /// <summary>
        /// Overloaded Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="stateNumber"></param>
        public DP14SSTMessage()
        {
        }

        #endregion Constructors

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }


    /// <summary>
    /// Derived Display Port event containing an PM Running Event
    /// </summary>
    public class DP14SSTMessage_ClearTraceBufferData : DP14SSTMessage
    {
        #region Members
        private DP14SST_TRACE_BUFFER_MODE m_TBModeID = DP14SST_TRACE_BUFFER_MODE.MainLink;

        public DP14SST_TRACE_BUFFER_MODE TBModeID
        {
            get { return m_TBModeID; }
            set { m_TBModeID = value; }
        }

        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_ClearTraceBufferData(long id, DP14SST_TRACE_BUFFER_MODE TBMode)
        {
            m_Name = "Clear Trace Buffer Data";
            m_ID = id;
            m_TBModeID = TBMode;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }


    /// <summary>
    /// Derived Display Port event containing an PM Running Event
    /// </summary>
    public class DP14SSTMessage_PMRunning : DP14SSTMessage
    {
        #region Members

        private DP14SST_TRACE_BUFFER_MODE m_runMode = DP14SST_TRACE_BUFFER_MODE.MainLink;
        public DP14SST_TRACE_BUFFER_MODE RunMode { get { return m_runMode; } set { m_runMode = value; } }

        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_PMRunning(long id = 0, DP14SST_TRACE_BUFFER_MODE mode = DP14SST_TRACE_BUFFER_MODE.MainLink)
        {
            m_Name = "PMRunning";
            m_ID = id;
            m_runMode = mode;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }


    /// <summary>
    /// Derived Display Port event containing an PM Ready Event
    /// </summary>
    public class DP14SSTMessage_PMStopping_Request : DP14SSTMessage
    {
        #region Members
        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_PMStopping_Request(long id = 0)
        {
            m_Name = "PMStoppingRequest";
            m_ID = id;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }


    /// <summary>
    /// Derived Display Port event containing an PM Ready Event
    /// </summary>
    public class DP14SSTMessage_PMStopping_Ack : DP14SSTMessage
    {
        #region Members
        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_PMStopping_Ack(long id = 0)
        {
            m_Name = "PMStoppingAck";
            m_ID = id;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }

    /// <summary>
    /// Derived Display Port event containing an PM Ready Event
    /// </summary>
    public class DP14SSTMessage_PMReady : DP14SSTMessage
    {
        #region Members
        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_PMReady(long id = 0)
        {
            m_Name = "PMReady";
            m_ID = id;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }


    /// <summary>
    /// Derived Display Port event containing an PM Ready Event
    /// </summary>
    public class DP14SSTMessage_MemoryDepthChanged : DP14SSTMessage
    {
        #region Members

        private long m_memDepth = 0;
        public long MemoryDepth
        {
            get { return m_memDepth; }
            set { m_memDepth = value; }
        }

        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_MemoryDepthChanged(long id = 0, long depth = 8192)
        {
            m_ID = id;
            m_memDepth = depth;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }



    /// <summary>
    /// Derived Display Port event containing an virtual channel slot selection change Event
    /// </summary>
    public class DP14SSTMessage_TimeSlotSelectionChanged : DP14SSTMessage
    {
        #region Members
        private int m_VChannelID = 0;

        public int VChannelID
        {
            get { return m_VChannelID; }
            set { m_VChannelID = value; }
        }


        private List<int> m_slotIDs = null;

        public List<int> SlotIDsList
        {
            get { return m_slotIDs; }
            set { m_slotIDs = value; }
        }


        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Overloaded Constructor
        /// </summary>
        /// <param name="vChannelID"></param>
        /// <param name="slotIDs"></param>
        public DP14SSTMessage_TimeSlotSelectionChanged(int vChannelID, List<int> slotIDs)
        {
            m_VChannelID = vChannelID;
            m_slotIDs = slotIDs;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }


    /// <summary>
    /// Derived Display Port event containing an Data Available Event
    /// </summary>
    public class DP14SSTMessage_DataAvailable : DP14SSTMessage
    {
        #region Members

        private long m_memoryDepth = 8 * 1024;  // 8K
        public long MemoryDepth
        {
            get { return m_memoryDepth; }
            set { m_memoryDepth = value; }
        }


        private bool m_ExitRunMode = false;
        public bool ExitRunMode
        {
            get { return m_ExitRunMode; }
            set { m_ExitRunMode = value; }
        }
        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_DataAvailable(long id = 0)
        {
            m_Name = "Data Available";
            m_ID = id;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }


    /// <summary>
    /// Derived Display Port event containing an Data Ready Event
    /// </summary>
    public class DP14SSTMessage_DataUploaded : DP14SSTMessage
    {
        #region Members
        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_DataUploaded(long id = 0)
        {
            m_Name = "Data Uploaded";
            m_ID = id;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }

    /// <summary>
    /// Derived Display Port event containing an Data Ready Event
    /// </summary>
    public class DP14SSTMessage_DataReady : DP14SSTMessage
    {
        #region Members
        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_DataReady(long id = 0)
        {
            m_Name = "Data Ready";
            m_ID = id;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }



    /// <summary>
    /// Derived Display Port event containing an State Data Request Event
    /// </summary>
    public class DP14SSTMessage_StateDataChunkRequest : DP14SSTMessage
    {
        #region Members

        private long m_stateNumber = 0;
        public long StateNumber
        {
            get { return m_stateNumber; }
            set { m_stateNumber = value; }
        }

        private int m_chunkSize = 0;
        public int ChunkSize
        {
            get { return m_chunkSize; }
            set { m_chunkSize = value; }
        }

        private List<byte> m_dataChunk = new List<byte>();
        public List<byte> DataChunk
        {
            get { return m_dataChunk; }
            set { m_dataChunk = value; }
        }

        private int m_dataOffset = 0;
        public int DataOffset
        {
            get { return m_dataOffset; }
            set { m_dataOffset = value; }
        }

        private int m_listingID = 0;
        public int ListingID
        {
            get { return m_listingID; }
            set { m_listingID = value; }
        }

        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_StateDataChunkRequest(long id = 0, int listingID = 0, long stateNumber = 0, int chunkSize = 4096, int dataOffset = 0)
        {
            m_Name = "State Data Request";
            m_ID = id;
            m_listingID = listingID;
            m_stateNumber = stateNumber;
            m_chunkSize = chunkSize;
            m_dataOffset = dataOffset;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods


        /// <summary>
        /// Add a list of bytes (hopefully on state data length boundries) data chuck byte list.
        /// </summary>
        /// <param name="stateData"></param>
        public void AppendStateToChunk(byte[] stateData)
        {
            m_dataChunk.AddRange(stateData);
        }


        /// <summary>
        /// Add a list of bytes (hopefully on state data length boundries) data chuck byte list.
        /// </summary>
        /// <param name="stateData"></param>
        public void ClearDataChunk()
        {
            m_dataChunk.Clear();
        }

        #endregion // Public Methods
    }

    /// <summary>
    /// Derived Display Port event containing an State Data Request Event
    /// </summary>
    public class DP14SSTMessage_StateDataRequest : DP14SSTMessage
    {
        #region Members

        private long m_stateNumber = 0;
        public long StateNumber
        {
            get { return m_stateNumber; }
            set { m_stateNumber = value; }
        }

        private int m_dataOffset = 0;
        public int DataOffset
        {
            get { return m_dataOffset; }
            set { m_dataOffset = value; }
        }

        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_StateDataRequest(long id = 0, long stateNumber = 0, int dataOffset = 0)
        {
            m_Name = "State Data Request";
            m_ID = id;
            m_stateNumber = stateNumber;
            m_dataOffset = dataOffset;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }


    /// <summary>
    /// Derived Display Port event containing an State Data Response Event
    /// </summary>
    public class DP14SSTMessage_StateDataResponse : DP14SSTMessage
    {
        #region Members

        private const int TB_STATE_LENTH = 16; //39;  // PNS -- ALB File Change

        // retain the original message ID (for pairing of messages -- request/response)
        private long m_requestID = 0;
        public long RequestID
        {
            get { return m_requestID; }
            set { m_requestID = value; }
        }


        private long m_stateNumber = 0;
        public long StateNumber
        {
            get { return m_stateNumber; }
            set { m_stateNumber = value; }
        }


        private byte[] m_dataBytes = new byte[TB_STATE_LENTH];
        public byte[] DataBytes
        {
            get { return m_dataBytes; }
            set { m_dataBytes = value; }
        }

        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_StateDataResponse(long responseEventID, long requestEventID, long stateNumber = 0, byte[] data = null)
        {
            m_Name = "State Data Response";
            m_ID = responseEventID;
            m_requestID = requestEventID;
            m_stateNumber = stateNumber;

            Array.Copy(data, m_dataBytes, m_dataBytes.Length);
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods

        /// <summary>
        /// Set the state data bytes to zero
        /// </summary>
        public void ClearData()
        {
            Array.Clear(m_dataBytes, 0, m_dataBytes.Length);
        }


        /// <summary>
        /// Returns the data byte array (really just the reference, which can be 
        /// used to store the data retreieved from the disk file).
        /// </summary>
        /// <returns></returns>
        public byte[] GetDataArrayRef()
        {
            return m_dataBytes;
        }


        /// <summary>
        /// Copy an array of bytes into the array member 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool SetData(byte[] data)
        {
            bool status = false;
            if (data.Length == TB_STATE_LENTH)
            {
                Array.Copy(data, m_dataBytes, m_dataBytes.Length);
                status = true;
            }

            return status;
        }
        #endregion // Public Methods
    }



    /// <summary>
    /// Get the data assoicated state and return the data as a class property 
    /// </summary>
    public class DP14SSTMessage_GetStateData : DP14SSTMessage
    {
        #region Members

        private const int TB_STATE_LENTH = 16; //39;  // PNS -- ALB File Change


        private long m_stateIndex = 0;
        public long StateIndex
        {
            get { return m_stateIndex; }
            set { m_stateIndex = value; }
        }


        private byte[] m_dataBytes = new byte[TB_STATE_LENTH];
        public byte[] DataBytes
        {
            get { return m_dataBytes; }
            set { m_dataBytes = value; }
        }

        #endregion // Members


        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_GetStateData(long id = 0, long stateIndex = 0)
        {
            m_Name = "Get State Data";
            m_ID = id;
            m_stateIndex = stateIndex;

            Array.Clear(m_dataBytes, 0, m_dataBytes.Length);
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods

        /// <summary>
        /// Copy an array of bytes into the array member 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool SetData(byte[] data)
        {
            bool status = false;
            if (data.Length == TB_STATE_LENTH)
            {
                Array.Copy(data, m_dataBytes, m_dataBytes.Length);
                status = true;
            }

            return status;
        }

        #endregion // Public Methods
    }


    /// <summary>
    /// Derived Display Port event containing an Listing Ready Event
    /// </summary>
    public class DP14SSTMessage_ListingReady : DP14SSTMessage
    {
        #region Members
        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_ListingReady(long id = 0)
        {
            m_Name = "Listing Ready";
            m_ID = id;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }



    /// <summary>
    /// Derived Display Port event containing the binary trace file meta data.
    /// </summary>
    public class DP14SSTMessage_GetTBBinFileMetaData : DP14SSTMessage
    {
        #region Members

        // type of data being requested; mail or auxiliary
        private DP14SST_TRACE_BUFFER_MODE m_TBMode = DP14SST_TRACE_BUFFER_MODE.Unknown;
        public DP14SST_TRACE_BUFFER_MODE TBMode
        {
            get { return m_TBMode; }
            set { m_TBMode = value; }
        }


        //// data format found; single or multi threaded
        //private TBDataType m_dataType = TBDataType.Unknown;
        //public TBDataType DataType
        //{
        //    get { return m_dataType; }
        //    set { m_dataType = value; }
        //}


        private int m_numberOfStates = 0x00;
        public int NumberOfStates
        {
            get { return m_numberOfStates; }
            set { m_numberOfStates = value; }
        }


        private int m_triggerOffset = 0;
        public int TriggerOffset
        {
            get { return m_triggerOffset; }
            set { m_triggerOffset = value; }
        }


        private int m_dataOffset = 0;
        public int DataOffset
        {
            get { return m_dataOffset; }
            set { m_dataOffset = value; }
        }


        // CSV string containin the meta data for one or more multi-theaded upload binary files.
        // fileIndex:XX;StartState:XX;EndState:XX;DataOffset:XX
        public List<string> MultiThreadedMetaData = new List<string>();
        //public List<string> MultiThreadedMetaData
        //{
        //    get { return m_MultiThreadedMetaData; }
        //    set { }
        //}

        #endregion

        #region Constructor(s)
        ///// <summary>
        ///// Overloaded Constructor
        ///// </summary>
        //public DPMessage_GetTBBinFileMetaData(TBDataType type)
        //{
        //    m_dataType = type;
        //}

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_GetTBBinFileMetaData()
        {
        }

        ///// <summary>
        ///// Default Constructor
        ///// </summary>
        //public DPMessage_GetTBBinFileMetaData(int numOfStates, int trigOffset)
        //{
        //    m_numberOfStates = numOfStates;
        //    m_triggerOffset = trigOffset;
        //}

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }


    /// <summary>
    /// Derived Display Port event containing an Data Available Event
    /// </summary>
    public class DP14SSTMessage_AuxDataAvailable : DP14SSTMessage
    {
        #region Members

        private long m_auxMemoryDepth = 128 * 1024; //16 * 1024;
        public long AuxMemoryDepth
        {
            get { return m_auxMemoryDepth; }
            set { m_auxMemoryDepth = value; }
        }

        private bool m_ExitRunMode = false;
        public bool ExitRunMode
        {
            get { return m_ExitRunMode; }
            set { m_ExitRunMode = value; }
        }

        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_AuxDataAvailable(long id = 0)
        {
            m_Name = "Aux Data Available";
            m_ID = id;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }


    /// <summary>
    /// Derived Display Port event containing an Data Ready Event
    /// </summary>
    public class DP14SSTMessage_AuxDataReady : DP14SSTMessage
    {
        #region Members
        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_AuxDataReady(long id = 0)
        {
            m_Name = "Aux Data Ready";
            m_ID = id;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }


    /// <summary>
    /// Derived Display Port event containing an State Data Request Event
    /// </summary>
    public class DP14SSTMessage_AuxStateDataRequest : DP14SSTMessage
    {
        #region Members

        private long m_stateNumber = 0;
        public long StateNumber
        {
            get { return m_stateNumber; }
            set { m_stateNumber = value; }
        }

        private int m_dataOffset = 0;
        public int DataOffset
        {
            get { return m_dataOffset; }
            set { m_dataOffset = value; }
        }

        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_AuxStateDataRequest(long id = 0, long stateNumber = 0, int dataOffset = 0)
        {
            m_Name = "State Data Request";
            m_ID = id;
            m_stateNumber = stateNumber;
            m_dataOffset = dataOffset;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }


    /// <summary>
    /// Derived Display Port event containing an State Data Response Event
    /// </summary>
    public class DP14SSTMessage_AuxStateDataResponse : DP14SSTMessage
    {
        #region Members

        private const int AUX_STATE_LENGTH = 16;

        // retain the original message ID (for pairing of messages -- request/response)
        private long m_requestID = 0;
        public long RequestID
        {
            get { return m_requestID; }
            set { m_requestID = value; }
        }


        private long m_stateNumber = 0;
        public long StateNumber
        {
            get { return m_stateNumber; }
            set { m_stateNumber = value; }
        }


        private byte[] m_dataBytes = new byte[AUX_STATE_LENGTH];
        public byte[] DataBytes
        {
            get { return m_dataBytes; }
            set { m_dataBytes = value; }
        }

        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_AuxStateDataResponse(long responseEventID, long requestEventID, long stateNumber = 0, byte[] data = null)
        {
            m_Name = "State Data Response";
            m_ID = responseEventID;
            m_requestID = requestEventID;
            m_stateNumber = stateNumber;

            Array.Copy(data, m_dataBytes, m_dataBytes.Length);
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods

        /// <summary>
        /// Set the state data bytes to zero
        /// </summary>
        public void ClearData()
        {
            Array.Clear(m_dataBytes, 0, m_dataBytes.Length);
        }


        /// <summary>
        /// Returns the data byte array (really just the reference, which can be 
        /// used to store the data retreieved from the disk file).
        /// </summary>
        /// <returns></returns>
        public byte[] GetDataArrayRef()
        {
            return m_dataBytes;
        }


        /// <summary>
        /// Copy an array of bytes into the array member 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool SetData(byte[] data)
        {
            bool status = false;
            if (data.Length == AUX_STATE_LENGTH)
            {
                Array.Copy(data, m_dataBytes, m_dataBytes.Length);
                status = true;
            }

            return status;
        }
        #endregion // Public Methods
    }



    /// <summary>
    /// Get the data assoicated state and return the data as a class property 
    /// </summary>
    public class DP14SSTMessage_AuxGetStateData : DP14SSTMessage
    {
        #region Members

        private const int TB_STATE_LENTH = 16; //39;  // PNS -- ALB File Change

        private long m_stateIndex = 0;
        public long StateIndex
        {
            get { return m_stateIndex; }
            set { m_stateIndex = value; }
        }


        private byte[] m_auxDataBytes = new byte[TB_STATE_LENTH];
        public byte[] AuxDataBytes
        {
            get { return m_auxDataBytes; }
            set { m_auxDataBytes = value; }
        }

        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_AuxGetStateData(long id = 0, long stateIndex = 0)
        {
            m_Name = "Get Aux State Data";
            m_ID = id;
            m_stateIndex = stateIndex;

            Array.Clear(m_auxDataBytes, 0, m_auxDataBytes.Length);
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods

        /// <summary>
        /// Copy an array of bytes into the array member 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool SetData(byte[] data)
        {
            bool status = false;
            if (data.Length == TB_STATE_LENTH)
            {
                Array.Copy(data, m_auxDataBytes, m_auxDataBytes.Length);
                status = true;
            }

            return status;
        }

        #endregion // Public Methods
    }


    /// <summary>
    /// Derived Display Port event containing a search request
    /// </summary>
    public class DP14SSTMessage_SearchListingRequest : DP14SSTMessage
    {
        #region Members

        // type of data being requested; mail or auxiliary
        private DP14SST_TRACE_BUFFER_MODE m_TBMode = DP14SST_TRACE_BUFFER_MODE.Unknown;
        public DP14SST_TRACE_BUFFER_MODE TBMode
        {
            get { return m_TBMode; }
            set { m_TBMode = value; }
        }

        private string m_columnName = "";
        public string ColumnName
        {
            get { return m_columnName; }
            set { m_columnName = value; }
        }


        private uint m_columnHexValue = 0x00;
        public uint m_ColumnHexValue
        {
            get { return m_columnHexValue; }
            set { m_columnHexValue = value; }
        }


        private long m_stateIndex = 0;
        public long StateIndex
        {
            get { return m_stateIndex; }
            set { m_stateIndex = value; }
        }



        private bool m_matchLocated = false;
        public bool MatchLocated
        {
            get { return m_matchLocated; }
            set { m_matchLocated = value; }
        }


        private long m_matchLocation = 0;
        public long MatchLocation
        {
            get { return m_matchLocation; }
            set { m_matchLocation = value; }
        }

        private bool m_searchPreviousStates = false;
        public bool PreviousStates
        {
            get { return m_searchPreviousStates; }
            set { m_searchPreviousStates = value; }
        }

        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_SearchListingRequest(long id, DP14SST_TRACE_BUFFER_MODE TBMode, string columnName, uint columnValue, long stateIndex, bool searchBackwards)
        {
            m_Name = "Search Listing Request";
            m_ID = id;

            m_TBMode = TBMode;
            m_columnName = columnName;
            m_columnHexValue = columnValue;
            m_stateIndex = stateIndex;
            m_searchPreviousStates = searchBackwards;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }


    /// <summary>
    /// Derived Display Port event containing a search response
    /// </summary>
    public class DP14SSTMessage_SearchListingResponse : DP14SSTMessage
    {
        #region Members

        // type of data being requested; mail or auxiliary
        private DP14SST_TRACE_BUFFER_MODE m_TBMode = DP14SST_TRACE_BUFFER_MODE.Unknown;
        public DP14SST_TRACE_BUFFER_MODE TBMode
        {
            get { return m_TBMode; }
            set { m_TBMode = value; }
        }

        private long m_RequestMsgID = -1;
        public long RequestMsgID
        {
            get { return m_RequestMsgID; }
            set { m_RequestMsgID = value; }
        }

        private bool m_matchLocated = false;
        public bool MatchLocated
        {
            get { return m_matchLocated; }
            set { m_matchLocated = value; }
        }

        private long m_matchLocation = 0;
        public long MatchLocation
        {
            get { return m_matchLocation; }
            set { m_matchLocation = value; }
        }

        #endregion Members

        #region Constructor(s)

        /// <summary>
        /// Overloaded Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="TBMode"></param>
        /// <param name="stateIndex"></param>
        public DP14SSTMessage_SearchListingResponse(long id, long requestMsgID, DP14SST_TRACE_BUFFER_MODE TBMode, bool matchFound, long stateIndex)
        {
            m_Name = "Search Listing Response";
            m_ID = id;

            m_RequestMsgID = requestMsgID;
            m_TBMode = TBMode;
            m_matchLocated = matchFound;
            m_matchLocation = stateIndex;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }


    /// <summary>
    /// Derived Display Port event containing a search cancel request
    /// </summary>
    public class DP14SSTMessage_SearchListingCancelRequest : DP14SSTMessage
    {
        #region Members

        // type of data being requested; mail or auxiliary
        private DP14SST_TRACE_BUFFER_MODE m_TBMode = DP14SST_TRACE_BUFFER_MODE.Unknown;
        public DP14SST_TRACE_BUFFER_MODE TBMode
        {
            get { return m_TBMode; }
            set { m_TBMode = value; }
        }

        #endregion Members

        #region Constructor(s)

        /// <summary>
        /// Overloaded Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="TBMode"></param>
        /// <param name="stateIndex"></param>
        public DP14SSTMessage_SearchListingCancelRequest(long id, DP14SST_TRACE_BUFFER_MODE TBMode)
        {
            m_Name = "Cancel Search Listing Request";
            m_ID = id;
        }
        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }


    /// <summary>
    /// Derived Display Port event containing a search cancel response
    /// </summary>
    public class DP14SSTMessage_SearchListingCancelResponse : DP14SSTMessage
    {
        #region Members

        // type of data being requested; mail or auxiliary
        private DP14SST_TRACE_BUFFER_MODE m_TBMode = DP14SST_TRACE_BUFFER_MODE.Unknown;
        public DP14SST_TRACE_BUFFER_MODE TBMode
        {
            get { return m_TBMode; }
            set { m_TBMode = value; }
        }

        private long m_RequestMsgID = -1;
        public long RequestMsgID
        {
            get { return m_RequestMsgID; }
            set { m_RequestMsgID = value; }
        }

        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestMsgID"></param>
        /// <param name="TBMode"></param>
        public DP14SSTMessage_SearchListingCancelResponse(long id, long requestMsgID, DP14SST_TRACE_BUFFER_MODE TBMode)
        {
            m_Name = "Cancel Search Listing Response";
            m_ID = id;
            m_RequestMsgID = requestMsgID;
            m_TBMode = TBMode;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }


    /// <summary>
    /// Derived Display Port event containing a search progress report
    /// </summary>
    public class DP14SSTMessage_SearchListingProgessReport : DP14SSTMessage
    {
        #region Members

        // type of data being requested; mail or auxiliary
        private DP14SST_TRACE_BUFFER_MODE m_TBMode = DP14SST_TRACE_BUFFER_MODE.Unknown;
        public DP14SST_TRACE_BUFFER_MODE TBMode
        {
            get { return m_TBMode; }
            set { m_TBMode = value; }
        }

        private long m_statesSeached = -1;
        public long StatesSearched
        {
            get { return m_statesSeached; }
            set { m_statesSeached = value; }
        }

        #endregion // members

        #region Constructor(s)

        /// <summary>
        /// Overloaded Constructor(s)
        /// </summary>
        /// <param name="TBMode"></param>
        /// <param name="chunkSize"></param>
        public DP14SSTMessage_SearchListingProgessReport(DP14SST_TRACE_BUFFER_MODE TBMode, long statesSearched)
        {
            m_Name = "Search Listing Progress Report";
            m_TBMode = TBMode;
            m_statesSeached = statesSearched;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }


    ///// <summary>
    ///// Derived Display Port event containing the next marker ID to be used.
    ///// </summary>
    //public class DP14SSTMessage_MarkerGetNextID : DP14SSTMessage
    //{
    //    #region Members

    //    private DP14SST_MarkerInfoID m_markerType = DP14SST_MarkerInfoID.Unknown;
    //    public DP14SST_MarkerInfoID MarkerType
    //    {
    //        get { return m_markerType; }
    //        set { m_markerType = value; }
    //    }

    //    private int m_markerID = -1;
    //    public int MarkerID
    //    {
    //        get { return m_markerID; }
    //        set { m_markerID = value; }
    //    }

    //    #endregion

    //    #region Constructor(s)

    //    /// <summary>
    //    /// Default Constructor
    //    /// </summary>
    //    public DP14SSTMessage_MarkerGetNextID(DP14SST_MarkerInfoID type)
    //    {
    //        m_Name = "TB Marker Get Next";
    //        m_markerType = type;
    //    }

    //    #endregion // Constructor(s)

    //    #region Private Methods
    //    #endregion // Private Methods

    //    #region Public Methods
    //    #endregion // Public Methods
    //}

    ///// <summary>
    ///// Derived Display Port event containing a trace buffer marker remove event
    ///// </summary>
    //public class DP14SSTMessage_MarkerAdd : DP14SSTMessage
    //{
    //    #region Members

    //    private DP14SST_MarkerInfoID m_markerType = DP14SST_MarkerInfoID.Unknown;
    //    public DP14SST_MarkerInfoID MarkerType
    //    {
    //        get { return m_markerType; }
    //        set { m_markerType = value; }
    //    }

    //    private string m_title = "";
    //    public string Title
    //    {
    //        get { return m_title; }
    //        set { m_title = value; }
    //    }


    //    private string m_markerID = "";
    //    public string MarkerID
    //    {
    //        get { return m_markerID; }
    //        set { m_markerID = value; }
    //    }


    //    private long m_stateIndex = 0x00;
    //    public long StateIndex
    //    {
    //        get { return m_stateIndex; }
    //        set { m_stateIndex = value; }
    //    }

    //    #endregion // Members

    //    #region Constructor(s)

    //    /// <summary>
    //    /// Default Constructor
    //    /// </summary>
    //    public DP14SSTMessage_MarkerAdd(long id = 0)
    //    {
    //        m_Name = "TB Marker Add";
    //        m_ID = id;
    //    }

    //    /// <summary>
    //    /// Overloaded constructor used to create an auxiliary link marker
    //    /// </summary>
    //    /// <param name="title"></param>
    //    /// <param name="markerID"></param>
    //    /// <param name="stateIndex"></param>
    //    /// <param name="barPosition"></param>
    //    /// <param name="timeStamp"></param>
    //    /// <param name="auxTimeStamp"></param>
    //    public DP14SSTMessage_MarkerAdd(long id, DP14SST_MarkerInfoID markerType, string title, string markerID, long stateIndex)
    //    {
    //        m_Name = "TB Marker Add";
    //        m_ID = id;

    //        m_markerType = markerType;
    //        m_title = title;
    //        m_markerID = markerID;
    //        m_stateIndex = stateIndex;
    //    }

    //    #endregion // Constructor(s)

    //    #region Private Methods
    //    #endregion // Private Methods

    //    #region Public Methods
    //    #endregion // Public Methods
    //}


    ///// <summary>
    ///// Derived Display Port event containing a trace buffer marker add event
    ///// </summary>
    //public class DP14SSTMessage_MarkerRemove : DP14SSTMessage
    //{
    //    #region Members

    //    private DP14SST_MarkerInfoID m_markerType = DP14SST_MarkerInfoID.Unknown;
    //    public DP14SST_MarkerInfoID MarkerType
    //    {
    //        get { return m_markerType; }
    //        set { m_markerType = value; }
    //    }

    //    private string m_title = "";
    //    public string Title
    //    {
    //        get { return m_title; }
    //        set { m_title = value; }
    //    }


    //    private string m_markerID = "";
    //    public string MarkerID
    //    {
    //        get { return m_markerID; }
    //        set { m_markerID = value; }
    //    }

    //    #endregion // Members

    //    #region Constructor(s)

    //    /// <summary>
    //    /// Default Constructor
    //    /// </summary>
    //    public DP14SSTMessage_MarkerRemove(long id)
    //    {
    //        m_Name = "TB Marker Remove";
    //        m_ID = id;
    //    }


    //    /// <summary>
    //    /// Overloaded Constructor 
    //    /// </summary>
    //    /// <param name="title"></param>
    //    /// <param name="markerID"></param>
    //    public DP14SSTMessage_MarkerRemove(long id, DP14SST_MarkerInfoID markerType, string title, string markerID)
    //    {
    //        m_Name = "TB Marker Remove";
    //        m_ID = id;

    //        m_markerType = markerType;
    //        m_title = title;
    //        m_markerID = markerID;
    //    }


    //    #endregion // Constructor(s)

    //    #region Private Methods
    //    #endregion // Private Methods

    //    #region Public Methods
    //    #endregion // Public Methods
    //}


    ///// <summary>
    ///// Derived Display Port event containing a trace buffer marker add event
    ///// </summary>
    //public class DP14SSTMessage_MarkerChangeLocation : DP14SSTMessage
    //{
    //    #region Members

    //    private DP14SST_MarkerInfoID m_markerType = DP14SST_MarkerInfoID.Unknown;
    //    public DP14SST_MarkerInfoID MarkerType
    //    {
    //        get { return m_markerType; }
    //        set { m_markerType = value; }
    //    }

    //    private string m_title = "";
    //    public string Title
    //    {
    //        get { return m_title; }
    //        set { m_title = value; }
    //    }


    //    private string m_markerID = "";
    //    public string MarkerID
    //    {
    //        get { return m_markerID; }
    //        set { m_markerID = value; }
    //    }


    //    private int m_stateIndex = 0x00;
    //    public int StateIndex
    //    {
    //        get { return m_stateIndex; }
    //        set { m_stateIndex = value; }
    //    }
    //    #endregion // Members

    //    #region Constructor(s)

    //    /// <summary>
    //    /// Overloaded Constructor 
    //    /// </summary>
    //    /// <param name="title"></param>
    //    /// <param name="markerID"></param>
    //    public DP14SSTMessage_MarkerChangeLocation(long id, DP14SST_MarkerInfoID markerType, string title, string markerID, int stateIndex)
    //    {
    //        m_Name = "TB Marker Change Location";
    //        m_ID = id;

    //        m_markerType = markerType;
    //        m_title = title;
    //        m_markerID = markerID;
    //        m_stateIndex = stateIndex;
    //    }

    //    #endregion // Constructor(s)

    //    #region Private Methods
    //    #endregion // Private Methods

    //    #region Public Methods
    //    #endregion // Public Methods
    //}


    ///// <summary>
    ///// Derived Display Port event containing a marker list request event
    ///// </summary>
    //public class DP14SSTMessage_MarkerGetList : DP14SSTMessage
    //{
    //    #region Members

    //    private DP14SST_MarkerInfoID m_markerType = DP14SST_MarkerInfoID.Unknown;

    //    public DP14SST_MarkerInfoID MarkerType
    //    {
    //        get { return m_markerType; }
    //        set { m_markerType = value; }
    //    }

    //    private List<DP14SST_MarkerInfo_MainLink> m_markerInfoList_mainLink = new List<DP14SST_MarkerInfo_MainLink>();
    //    public List<DP14SST_MarkerInfo_MainLink> MarkerInfoList_mainLink
    //    {
    //        get { return m_markerInfoList_mainLink; }
    //        set { m_markerInfoList_mainLink = value; }
    //    }

    //    private List<DP14SST_MarkerInfo_AuxiliaryLink> m_markerInfoList_auxiliaryLink = new List<DP14SST_MarkerInfo_AuxiliaryLink>();
    //    public List<DP14SST_MarkerInfo_AuxiliaryLink> MarkerInfoList_auxiliaryLink
    //    {
    //        get { return m_markerInfoList_auxiliaryLink; }
    //        set { m_markerInfoList_auxiliaryLink = value; }
    //    }

    //    #endregion // Members

    //    #region Constructor(s)

    //    /// <summary>
    //    /// Overloaded Constructor 
    //    /// </summary>
    //    /// <param name="title"></param>
    //    /// <param name="markerID"></param>
    //    public DP14SSTMessage_MarkerGetList(DP14SST_MarkerInfoID markerType)
    //    {
    //        m_Name = "TB Marker Get List Request";
    //        m_markerType = markerType;
    //    }

    //    #endregion // Constructor(s)

    //    #region Private Methods
    //    #endregion // Private Methods

    //    #region Public Methods
    //    #endregion // Public Methods
    //}


    ///// <summary>
    ///// Derived Display Port event containing a marker list response event
    ///// </summary>
    //public class DP14SSTMessage_MarkerGetListResponse_MainLink : DP14SSTMessage
    //{
    //    #region Members

    //    private List<DP14SST_MarkerInfo_MainLink> m_markerList = new List<DP14SST_MarkerInfo_MainLink>();

    //    public List<DP14SST_MarkerInfo_MainLink> MarkerList
    //    {
    //        get { return m_markerList; }
    //        set { m_markerList = value; }
    //    }

    //    #endregion // Members

    //    #region Constructor(s)

    //    public DP14SSTMessage_MarkerGetListResponse_MainLink(List<DP14SST_MarkerInfo_MainLink> markerList)
    //    {
    //        m_Name = "TB Marker Get List Response";
    //        m_markerList = markerList;
    //    }

    //    #endregion //Constructor(s)

    //    #region Private Methods
    //    #endregion // Private Methods

    //    #region Public Methods
    //    #endregion // Public Methods
    //}


    ///// <summary>
    ///// Derived Display Port event containing a marker list response event
    ///// </summary>
    //public class DP14SSTMessage_MarkerGetListResponse_AuxiliaryLink : DP14SSTMessage
    //{
    //    #region Members

    //    private List<DP14SST_MarkerInfo_AuxiliaryLink> m_markerList = new List<DP14SST_MarkerInfo_AuxiliaryLink>();

    //    public List<DP14SST_MarkerInfo_AuxiliaryLink> MarkerList
    //    {
    //        get { return m_markerList; }
    //        set { m_markerList = value; }
    //    }

    //    #endregion // Members

    //    #region Constructor(s)

    //    /// <summary>
    //    /// Overaloaded Constructor
    //    /// </summary>
    //    /// <param name="markerList"></param>
    //    public DP14SSTMessage_MarkerGetListResponse_AuxiliaryLink(List<DP14SST_MarkerInfo_AuxiliaryLink> markerList)
    //    {
    //        m_Name = "Aux TB Marker Get List Response";
    //        m_markerList = markerList;
    //    }

    //    #endregion // Constructor(s)

    //    #region Private Methods
    //    #endregion // Private Methods

    //    #region Public Methods
    //    #endregion // Public Methods
    //}


    /// <summary>
    /// Derived Display Port event containing a trace buffer mode change event
    /// </summary>
    public class DP14SSTMessage_TBDataModeChange : DP14SSTMessage
    {
        #region Members
        private DP14SST_TRACE_BUFFER_MODE m_TBModeID = DP14SST_TRACE_BUFFER_MODE.MainLink;

        public DP14SST_TRACE_BUFFER_MODE TBModeID
        {
            get { return m_TBModeID; }
            set { m_TBModeID = value; }
        }

        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_TBDataModeChange(long id, DP14SST_TRACE_BUFFER_MODE TBMode)
        {
            m_Name = "TB Data Mode Change";
            m_ID = id;
            m_TBModeID = TBMode;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }

    /// <summary>
    /// Derived Display Port event containing a trace buffer mode change event
    /// </summary>
    public class DP14SSTMessage_SpeedChange : DP14SSTMessage
    {
        #region Members
        private float m_ClkPeriod = 0x00;

        public float ClockPeriod
        {
            get { return m_ClkPeriod; }
            set { m_ClkPeriod = value; }
        }

        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_SpeedChange(long id, float clkPeriod)
        {
            m_Name = "Speed Change";
            m_ID = id;
            m_ClkPeriod = clkPeriod;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }


    /// <summary>
    /// Derived Display Port event containing a cross link order change event
    /// </summary>
    public class DP14SSTMessage_CrossLinkTriggerOrderChange : DP14SSTMessage
    {
        #region Members

        private DP14SST_CROSS_LINK_MODE m_mode = 0x00;

        public DP14SST_CROSS_LINK_MODE Mode
        {
            get { return m_mode; }
            set { m_mode = value; }
        }

        #endregion // Members

        #region Constructor(s)
        public DP14SSTMessage_CrossLinkTriggerOrderChange(long id, DP14SST_CROSS_LINK_MODE mode)
        {
            m_Name = "Cross Link Order Change";
            m_ID = id;
            m_mode = mode;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }


    /// <summary>
    /// Derived Display Port event containing an Initialize Message.
    /// </summary>
    public class DP14SSTMessage_Initialize : DP14SSTMessage
    {
        #region Members

        private bool m_RemoveDataFiles = false;

        public bool RemoveDataFiles
        {
            get { return m_RemoveDataFiles; }
            set { m_RemoveDataFiles = value; }
        }

        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_Initialize(long id = 0, bool removeDataFiles = false)
        {
            m_Name = "Initialize";
            m_ID = id;
            m_RemoveDataFiles = removeDataFiles;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }


    /// <summary>
    /// Derived Display Port event containing an Set Configuration Message
    /// </summary>
    public class DP14SSTMessage_SetConfig : DP14SSTMessage
    {
        #region Members
        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_SetConfig(long id = 0)
        {
            m_Name = "Set Config";
            m_ID = id;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }



    /// <summary>
    /// Derived Display Port event containing an Set Configuration Message
    /// </summary>
    public class DP14SSTMessage_SaveConfig : DP14SSTMessage
    {
        #region Members
        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_SaveConfig(long id = 0)
        {
            m_Name = "Save Config";
            m_ID = id;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }



    /// <summary>
    /// Derived Display Port event containing an Set Configuration Message
    /// </summary>
    public class DP14SSTMessage_LoadConfig : DP14SSTMessage
    {
        #region Members
        private string m_filePath = "";
        public string FilePath
        {
            get { return m_filePath; }
            set { m_filePath = value; }
        }
        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_LoadConfig(long id = 0, string filePath = "")
        {
            m_Name = "Load Config";
            m_ID = id;

            m_filePath = filePath;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }


    /// <summary>
    /// Load a saved configuration for a specfic program module (class/form object)
    /// </summary>
    public class DP14SSTMessage_LoadConfigModule : DP14SSTMessage
    {
        #region Members
        private string m_moduleID = "";
        public string ModuleID
        {
            get { return m_moduleID; }
            set { m_moduleID = value; }
        }
        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_LoadConfigModule(long id = 0, string moduleID = "")
        {
            m_Name = "Load Config Module";
            m_ID = id;

            m_moduleID = moduleID;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }


    /// <summary>
    /// Derived Display Port event containing an Listing Ready Event
    /// </summary>
    public class DP14SSTMessage_TBObjectRegister : DP14SSTMessage
    {
        #region Members
        private ITBObject m_TBObj = null;

        public ITBObject TBObj
        {
            get { return m_TBObj; }
            set { m_TBObj = value; }
        }

        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_TBObjectRegister(long id = 0, ITBObject TraceBufferObj = null)
        {
            m_Name = "TB Object Register";
            m_ID = id;

            m_TBObj = TraceBufferObj;
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods
        #endregion // Public Methods
    }


    //
    // New Pixel Renderer Class(es)
    //


    /// <summary>
    /// Get the number of states contained in the uploaded binary file
    /// </summary>
    public class DP14SSTMessage_GetNumberOfStates : DP14SSTMessage
    {
        private int m_virtualChannelID = -1;

        private long m_numOfStates = -1;
        public long NumberOfStates
        {
            get { return m_numOfStates; }
            set { m_numOfStates = value; }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SSTMessage_GetNumberOfStates(int virtualChannelID)
        {
            m_virtualChannelID = virtualChannelID;
        }
    }


    /// <summary>
    /// Display Event Pump class -- 
    /// </summary>
    public class DP14SST_MessagePump
    {
        #region Members

        public List<ITBObject> m_TBObjectsList = new List<ITBObject>();

        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DP14SST_MessagePump()
        {
        }

        #endregion // Constructor(s)

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods

        /// <summary>
        /// Cycle through all TBObjects and ask them to raise the DPEvent for registering.
        /// </summary>
        /// Assumption:  this call is held off at startup time until all objects exist.
        public void Initialize()
        {
            foreach (ITBObject tbObj in m_TBObjectsList)
            {
                tbObj.Initialize();
            }
        }


        /// <summary>
        /// Maintain a distinct list of TBObjects
        /// </summary>
        /// <param name="TBObjectRef"></param>
        public void AddTBObjectRef(ITBObject TBObjectRef)
        {
            if (!m_TBObjectsList.Contains(TBObjectRef))
                m_TBObjectsList.Add(TBObjectRef);
        }


        /// <summary>
        /// Push the event to all TBObjects.
        /// </summary>
        /// <param name="DisplayPortEvent"></param>
        public void ForwardEvent(DP14SSTMessage e)
        {
            if ((e is DP14SSTMessage_StateDataRequest) ||
                (e is DP14SSTMessage_AuxStateDataRequest) ||
                (e is DP14SSTMessage_AuxGetStateData) ||
                (e is DP14SSTMessage_GetTBBinFileMetaData) ||
                (e is DP14SSTMessage_SearchListingRequest) ||
                (e is DP14SSTMessage_GetStateData))
            {
                // send this to the TBDataMgr Object
                foreach (ITBObject obj in m_TBObjectsList)
                {
                    if (obj is DP14SST_TraceBufferDataMgr)
                    {
                        obj.ProcessDPMessage(e);
                        break;
                    }
                }
            }
            //else if ((e is DP14SSTMessage_MarkerAdd) ||
            //         (e is DP14SSTMessage_MarkerRemove) ||
            //         (e is DP14SSTMessage_MarkerGetList))
            //{
            //    foreach (ITBObject obj in m_TBObjectsList)
            //    {
            //        if (obj is DP14SST_MarkerInfoMgr)
            //        {
            //            obj.ProcessDPMessage(e);
            //            break;
            //        }
            //    }
            //}
            //else if (e is DP14SSTMessage_SpeedChange)
            //{
            //    foreach (ITBObject obj in m_TBObjectsList)
            //    {
            //        if (obj is DP14SSTStateListingForm)
            //        {
            //            obj.ProcessDPMessage(e);
            //            break;
            //        }
            //    }
            //}
            //else if (e is DP14SSTMessage_CrossLinkTriggerOrderChange)
            //{
            //    foreach (ITBObject obj in m_TBObjectsList)
            //    {
            //        if ((obj is DP14SSTProbeTrigForm) ||
            //            (obj is DP14SSTProbeAuxTrigForm))
            //        {
            //            obj.ProcessDPMessage(e);
            //        }
            //    }
            //}
            //else if (e is DP14SSTMessage_MarkerGetListResponse_MainLink)
            //{
            //    foreach (ITBObject obj in m_TBObjectsList)
            //    {
            //        if (obj is DP14SSTStateListingForm)
            //        {
            //            obj.ProcessDPMessage(e);
            //            break;
            //        }
            //    }
            //}
            //else if (e is DP14SSTMessage_SearchListingResponse)
            //{
            //    if (((DP14SSTMessage_SearchListingResponse)e).TBMode == DP14SST_TRACE_BUFFER_MODE.MainLink)
            //    {
            //        foreach (ITBObject obj in m_TBObjectsList)
            //        {
            //            if (obj is DP14SSTStateListingForm)
            //            {
            //                obj.ProcessDPMessage(e);
            //                break;
            //            }
            //        }
            //    }
            //    else if (((DP14SSTMessage_SearchListingResponse)e).TBMode == DP14SST_TRACE_BUFFER_MODE.AuxiliaryLink)
            //    {
            //        foreach (ITBObject obj in m_TBObjectsList)
            //        {
            //            if (obj is DP14SSTAuxStateListingForm)
            //            {
            //                obj.ProcessDPMessage(e);
            //                break;
            //            }
            //        }
            //    }
            //}
            //else if (e is DP14SSTMessage_SearchListingCancelResponse)
            //{
            //    if (((DP14SSTMessage_SearchListingCancelResponse)e).TBMode == DP14SST_TRACE_BUFFER_MODE.MainLink)
            //    {
            //        foreach (ITBObject obj in m_TBObjectsList)
            //        {
            //            if (obj is DP14SSTStateListingForm)
            //            {
            //                obj.ProcessDPMessage(e);
            //                break;
            //            }
            //        }
            //    }
            //    else if (((DP14SSTMessage_SearchListingCancelResponse)e).TBMode == DP14SST_TRACE_BUFFER_MODE.AuxiliaryLink)
            //    {
            //        foreach (ITBObject obj in m_TBObjectsList)
            //        {
            //            if (obj is DP14SSTAuxStateListingForm)
            //            {
            //                obj.ProcessDPMessage(e);
            //                break;
            //            }
            //        }
            //    }
            //}
            //else if (e is DP14SSTMessage_SearchListingProgessReport)
            //{
            //    if (((DP14SSTMessage_SearchListingProgessReport)e).TBMode == DP14SST_TRACE_BUFFER_MODE.MainLink)
            //    {
            //        foreach (ITBObject obj in m_TBObjectsList)
            //        {
            //            if (obj is DP14SSTStateListingForm)
            //            {
            //                obj.ProcessDPMessage(e);
            //                break;
            //            }
            //        }
            //    }
            //    else if (((DP14SSTMessage_SearchListingProgessReport)e).TBMode == DP14SST_TRACE_BUFFER_MODE.AuxiliaryLink)
            //    {
            //        foreach (ITBObject obj in m_TBObjectsList)
            //        {
            //            if (obj is DP14SSTAuxStateListingForm)
            //            {
            //                obj.ProcessDPMessage(e);
            //                break;
            //            }
            //        }
            //    }
            //}
            //else if (e is DP14SSTMessage_MarkerGetListResponse_AuxiliaryLink)
            //{
            //    foreach (ITBObject obj in m_TBObjectsList)
            //    {
            //        if (obj is DP14SSTAuxStateListingForm)
            //        {
            //            obj.ProcessDPMessage(e);
            //            break;
            //        }
            //    }
            //}
            else
            {
                foreach (ITBObject obj in m_TBObjectsList)
                    obj.ProcessDPMessage(e);
            }
        }

        #endregion // Public Methods
    }
}
