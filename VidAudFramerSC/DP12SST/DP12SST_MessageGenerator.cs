using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP12SSTClassLibrary
{
    class DP12SST_MessageGenerator
    {
        #region Members

        private static DP12SST_MessageGenerator m_instance = new DP12SST_MessageGenerator();
        private uint m_ID = 0;

        #endregion // Members

        #region Ctor

        // Singleton is constructed when the DLL is created.
        private DP12SST_MessageGenerator()
        {
        }

        #endregion // Ctor

        #region Private Methods
        #endregion // Private Methods

        #region Public Methods

        /// <summary>
        /// Returns a single thread safe instance of the class object.
        /// </summary>
        /// <returns></returns>
        public static DP12SST_MessageGenerator GetInstance()
        {
            return m_instance;
        }

        /// <summary>
        /// Return an instance of a ITBObject Register DP Event
        /// </summary>
        /// <param name="TBObj"></param>
        /// <returns></returns>
        public DP12SSTMessage_TBObjectRegister DP12SSTMessageCreate_Register(ITBObject TBObjRef)
        {
            return (new DP12SSTMessage_TBObjectRegister(m_ID++, TBObjRef));
        }


        /// <summary>
        /// Return an instance of the user selecting a trace buffer data set (ML, Aux, or both)
        /// </summary>
        public DP12SSTMessage_TBDataModeChange DP12SSTMessageCreate_TBDataModeChange(DP12SST_TRACE_BUFFER_MODE TBMode)
        {
            return (new DP12SSTMessage_TBDataModeChange(m_ID++, TBMode));
        }

        /// <summary>
        /// Return an instance of the user selecting a trace buffer data set (ML, Aux, or both)
        /// </summary>
        public DP12SSTMessage_SpeedChange DP12SSTMessageCreate_SpeedChange(float clkPeriod)
        {
            return (new DP12SSTMessage_SpeedChange(m_ID++, clkPeriod));
        }


        /// <summary>
        /// Returns an instance of a cross link order change event.
        /// </summary>
        /// <param name="TBMode"></param>
        /// <returns></returns>
        public DP12SSTMessage_CrossLinkTriggerOrderChange DP12SSTMessageCreate_CrossLinkTriggerOrderChange(DP12SST_CROSS_LINK_MODE TBMode)
        {
            return (new DP12SSTMessage_CrossLinkTriggerOrderChange(m_ID++, TBMode));
        }


        /// <summary>
        /// Return an instance of the (Re-)Initializtion message... typically used when loading a saved configuration.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DP12SSTMessage_Initialize DP12SSTMessageCreate_Initialize(bool removeDataFiles = true)
        {
            return (new DP12SSTMessage_Initialize(m_ID++, removeDataFiles));
        }


        /// <summary>
        /// Return an instance of the Probe Manager Running event
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DP12SSTMessage_SetConfig DP12SSTMessageCreate_SetConfig()
        {
            return (new DP12SSTMessage_SetConfig(m_ID++));
        }


        /// <summary>
        /// Return an instance of the Probe Manager Running event --
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// Assumption:  Event is issued at run-time to get all forms to write their 
        ///              current configuraitons to the HW
        public DP12SSTMessage_SaveConfig DP12SSTMessageCreate_SaveConfig()
        {
            return (new DP12SSTMessage_SaveConfig(m_ID++));
        }


        /// <summary>
        /// Return an instance of the Probe Manager Running event --
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// Assumption:  Event is issued at run-time to get all forms to write their 
        ///              current configuraitons to the HW
        public DP12SSTMessage_LoadConfig DP12SSTMessageCreate_LoadConfig()
        {
            return (new DP12SSTMessage_LoadConfig(m_ID++));
        }


        /// <summary>
        /// Return an instance of the Probe Manager Running event --
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// Assumption:  Event is issued at run-time to get all forms to write their 
        ///              current configuraitons to the HW
        public DP12SSTMessage_LoadConfigModule DP12SSTMessageCreate_LoadConfig_Module(string moduleID)
        {
            return (new DP12SSTMessage_LoadConfigModule(m_ID++, moduleID));
        }

        /// <summary>
        /// rReturn an instance of the Clear PM data request event.
        /// </summary>
        /// <param name="TBMode"></param>
        /// <returns></returns>
        public DP12SSTMessage_ClearTraceBufferData DP12SSTMessage_ClearTraceBufferData(DP12SST_TRACE_BUFFER_MODE TBMode)
        {
            return (new DP12SSTMessage_ClearTraceBufferData(m_ID++, TBMode));
        }


        /// <summary>
        /// Return an instance of the Probe Manager Running event
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DP12SSTMessage_PMRunning DP12SSTMessageCreate_PMRunning(DP12SST_TRACE_BUFFER_MODE mode)
        {
            return (new DP12SSTMessage_PMRunning(m_ID++, mode));
        }



        /// <summary>
        /// Return an instance of the Data Ready event
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DP12SSTMessage_PMStopping_Request DP12SSTMessageCreate_PMStopping_Request()
        {
            return (new DP12SSTMessage_PMStopping_Request(m_ID++));
        }

        /// <summary>
        /// Return an instance of the Data Ready event
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DP12SSTMessage_PMStopping_Ack DPMessageCreate_PMStopping_Ack()
        {
            return (new DP12SSTMessage_PMStopping_Ack(m_ID++));
        }


        /// <summary>
        /// Return an instance of the Data Ready event
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DP12SSTMessage_PMReady DP12SSTMessageCreate_PMReady()
        {
            return (new DP12SSTMessage_PMReady(m_ID++));
        }


        /// <summary>
        /// Return an instance of the Memory Depth having been changed.
        /// </summary>
        /// <returns></returns>
        public DP12SSTMessage_MemoryDepthChanged DP12SSTMessageCreate_MemoryDepthChanged(long depth)
        {
            return (new DP12SSTMessage_MemoryDepthChanged(m_ID++, depth));
        }


        /// <summary>
        /// Return an instance of the Data Available event
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DP12SSTMessage_DataAvailable DP12SSTMessageCreate_DataAvailable()
        {
            return (new DP12SSTMessage_DataAvailable(m_ID++));
        }

        /// <summary>
        /// Return an instance of the Data Ready event
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DP12SSTMessage_DataReady DP12SSTMessageCreate_DataReady()
        {
            return (new DP12SSTMessage_DataReady(m_ID++));
        }


        /// <summary>
        /// Indicate an listing data set is ready to be processed
        /// </summary>
        /// <returns></returns>
        public DP12SSTMessage_ListingReady DPMessageCreate_ListingReady()
        {
            return (new DP12SSTMessage_ListingReady(m_ID++));
        }


        public DP12SSTMessage_GetTBBinFileMetaData DP12SSTMessageCreate_GetTBBinFileMetaData()
        {
            return (new DP12SSTMessage_GetTBBinFileMetaData());
        }


        /// <summary>
        /// Return an instance of a ITBObject Register DP Event
        /// </summary>
        /// <param name="TBObj"></param>
        /// <returns></returns>
        public DP12SSTMessage_StateDataChunkRequest DP12SSTMessageCreate_StateDataChunkRequest(int listingID, long stateNumber, int chunkSize, int dataOffset = 0)
        {
            return (new DP12SSTMessage_StateDataChunkRequest(m_ID++, listingID, stateNumber,chunkSize, dataOffset));
        }


        /// <summary>
        /// Return an instance of a ITBObject Register DP Event
        /// </summary>
        /// <param name="TBObj"></param>
        /// <returns></returns>
        public DP12SSTMessage_StateDataResponse DP12SSTMessageCreate_StateDataResponse(long requestID, long stateNumber, byte[] data)
        {
            return (new DP12SSTMessage_StateDataResponse(m_ID++, requestID, stateNumber, data));
        }



        /// <summary>
        /// Return an instance of an ITBObject DP event to get the data assoicated with a specific state.
        /// </summary>
        /// <param name="requestID"></param>
        /// <param name="stateNumber"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public DP12SSTMessage_GetStateData DP12SSTMessage_GetStateData(long requestID, long stateNumber)
        {
            return (new DP12SSTMessage_GetStateData(m_ID++, stateNumber));
        }

        /// <summary>
        /// Return an instance of the Data Ready event
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DP12SSTMessage_AuxDataAvailable DP12SSTMessageCreate_AuxDataAvailable()
        {
            return (new DP12SSTMessage_AuxDataAvailable(m_ID++));
        }


        /// <summary>
        /// Return an instance of the Data Ready event
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DP12SSTMessage_AuxDataReady DP12SSTMessageCreate_AuxDataReady()
        {
            return (new DP12SSTMessage_AuxDataReady(m_ID++));
        }

        /// <summary>
        /// Return an instance of a ITBObject Register DP Event
        /// </summary>
        /// <param name="TBObj"></param>
        /// <returns></returns>
        public DP12SSTMessage_AuxStateDataRequest DP12SSTMessageCreate_AuxStateDataRequest(long stateNumber, int dataOffset = 0)
        {
            return (new DP12SSTMessage_AuxStateDataRequest(m_ID++, stateNumber, dataOffset));
        }


        /// <summary>
        /// Return an instance of a ITBObject Register DP Event
        /// </summary>
        /// <param name="TBObj"></param>
        /// <returns></returns>
        public DP12SSTMessage_AuxStateDataResponse DP12SSTMessageCreate_AuxStateDataResponse(long requestID, long stateNumber, byte[] data)
        {
            return (new DP12SSTMessage_AuxStateDataResponse(m_ID++, requestID, stateNumber, data));
        }


        /// <summary>
        /// Returns an instance of an ITBObject Search Request
        /// </summary>
        /// <param name="TBMode"></param>
        /// <param name="columnName"></param>
        /// <param name="columnValue"></param>
        /// <param name="stateIndex"></param>
        /// <param name="searchBackwards"></param>
        /// <returns></returns>
        public DP12SSTMessage_SearchListingRequest DP12SSTMessageCreate_SearchListingRequest(DP12SST_TRACE_BUFFER_MODE TBMode, string columnName, uint columnValue, long stateIndex, bool searchBackwards = false)
        {
            return (new DP12SSTMessage_SearchListingRequest(m_ID++, TBMode, columnName, columnValue, stateIndex, searchBackwards));
        }


        /// <summary>
        /// Returns an instance of an ITBObject Search Response
        /// </summary>
        /// <param name="TBMode"></param>
        /// <param name="requestMsgID"></param>
        /// <param name="matchFound"></param>
        /// <param name="stateIndex"></param>
        /// <returns></returns>
        public DP12SSTMessage_SearchListingResponse DP12SSTMessageCreate_SearchListingResponse(DP12SST_TRACE_BUFFER_MODE TBMode, long requestMsgID, bool matchFound, long stateIndex)
        {
            return (new DP12SSTMessage_SearchListingResponse(m_ID++, requestMsgID, TBMode, matchFound, stateIndex));
        }


        /// <summary>
        /// Returns an instance of an ITBObject search cancel request
        /// </summary>
        /// <param name="TBMode"></param>
        /// <returns></returns>
        public DP12SSTMessage_SearchListingCancelRequest DP12SSTMessageCreate_SearchListingCancelRequest(DP12SST_TRACE_BUFFER_MODE TBMode)
        {
            return (new DP12SSTMessage_SearchListingCancelRequest(m_ID++, TBMode));
        }


        /// <summary>
        /// Returns an instance of an ITBObject search cancel respone
        /// </summary>
        /// <param name="TBMode"></param>
        /// <param name="requestMsgID"></param>
        /// <returns></returns>
        public DP12SSTMessage_SearchListingCancelResponse DP12SSTMessageCreate_SearchListingCancelResponse(DP12SST_TRACE_BUFFER_MODE TBMode, long requestMsgID)
        {
            return (new DP12SSTMessage_SearchListingCancelResponse(m_ID++, requestMsgID, TBMode));
        }


        /// <summary>
        /// Returns an instance of an ITBObject search listing progress report
        /// </summary>
        /// <param name="TBMode"></param>
        /// <param name="statesSearched"></param>
        /// <returns></returns>
        public DP12SSTMessage_SearchListingProgessReport DP12SSTMessageCreate_SearchListingProgessReport(DP12SST_TRACE_BUFFER_MODE TBMode, long statesSearched)
        {
            return (new DP12SSTMessage_SearchListingProgessReport(TBMode, statesSearched));
        }


        ///// <summary>
        ///// Return an instance of an ITBObject Marker get next marker ID
        ///// </summary>
        ///// <param name="markerType"></param>
        ///// <returns></returns>
        //public DP12SSTMessage_MarkerGetNextID DP12SSTMessageCreate_MarkerGetNextID(DP12SST_MarkerInfoID markerType)
        //{
        //    return (new DP12SSTMessage_MarkerGetNextID(markerType));
        //}


        ///// <summary>
        ///// Return an instance of an ITBObject Marker Add DP Event
        ///// </summary>
        ///// <param name="markerType"></param>
        ///// <param name="title"></param>
        ///// <param name="markerID"></param>
        ///// <param name="stateIndex"></param>
        ///// <param name="barPosition"></param>
        ///// <param name="timeStamp"></param>
        ///// <param name="auxTimeStamp"></param>
        ///// <returns></returns>
        //public DP12SSTMessage_MarkerAdd DP12SSTMessageCreate_MarkerAdd(DP12SST_MarkerInfoID markerType, string title, string markerID, long stateIndex)
        //{
        //    return (new DP12SSTMessage_MarkerAdd(m_ID++, markerType, title, markerID, stateIndex));
        //}


        ///// <summary>
        ///// Return an instance of an ITBObject Marker Add DP Event
        ///// </summary>
        ///// <param name="markerType"></param>
        ///// <param name="title"></param>
        ///// <param name="markerID"></param>
        ///// <returns></returns>
        //public DP12SSTMessage_MarkerRemove DP12SSTMessageCreate_MarkerRemove(DP12SST_MarkerInfoID markerType, string title, string markerID)
        //{
        //    return (new DP12SSTMessage_MarkerRemove(m_ID++, markerType, title, markerID));
        //}


        ///// <summary>
        ///// Return an instance of an ITBObject Marker Add DP Event
        ///// </summary>
        ///// <param name="markerType"></param>
        ///// <param name="title"></param>
        ///// <param name="markerID"></param>
        ///// <returns></returns>
        //public DP12SSTMessage_MarkerChangeLocation DP12SSTMessageCreate_MarkerChangeLocation(DP12SST_MarkerInfoID markerType, string title, string markerID, int stateIndex)
        //{
        //    return (new DP12SSTMessage_MarkerChangeLocation(m_ID++, markerType, title, markerID, stateIndex));
        //}


        ///// <summary>
        ///// Get a list of markers from the marker info manager object
        ///// </summary>
        ///// <param name="markerType"></param>
        ///// <returns></returns>
        //public DP12SSTMessage_MarkerGetList DP12SSTMessageCreate_MarkerGetList(DP12SST_MarkerInfoID markerType)
        //{
        //    return (new DP12SSTMessage_MarkerGetList(markerType));
        //}


        ///// <summary>
        ///// Get a marker list (response to a get marker list request)
        ///// </summary>
        ///// <param name="markerInfoList"></param>
        ///// <returns></returns>
        //public DP12SSTMessage_MarkerGetListResponse_MainLink DP12SSTMessageCreate_MarkerGetListResponse_MainLink(List<DP12SST_MarkerInfo_MainLink> markerInfoList)
        //{
        //    return (new DP12SSTMessage_MarkerGetListResponse_MainLink(markerInfoList));
        //}

        ///// <summary>
        ///// Get a marker list (response to a get marker list request)
        ///// </summary>
        ///// <param name="markerInfoList"></param>
        ///// <returns></returns>
        //public DP12SSTMessage_MarkerGetListResponse_AuxiliaryLink DP12SSTMessageCreate_MarkerGetListResponse_AuxiliaryLink(List<DP12SST_MarkerInfo_AuxiliaryLink> markerInfoList)
        //{
        //    return (new DP12SSTMessage_MarkerGetListResponse_AuxiliaryLink(markerInfoList));
        //}
        #endregion // Public Methods
    }
}
