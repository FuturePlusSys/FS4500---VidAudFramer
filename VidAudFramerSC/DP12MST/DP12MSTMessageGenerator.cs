using System.Collections.Generic;

namespace DP12MSTClassLibrary
{
    /// <summary>
    /// class that generates an instance of a Display Port Event
    /// </summary>
    public class DP12MSTMessageGenerator
    {
        #region Members

        private static DP12MSTMessageGenerator m_instance = new DP12MSTMessageGenerator();
        private uint m_ID = 0;

        #endregion // Members

        #region Ctor

        // Singleton is constructed when the DLL is created.
        private DP12MSTMessageGenerator()
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
        public static DP12MSTMessageGenerator GetInstance()
        {
            return m_instance;
        }

        /// <summary>
        /// Return an instance of a ITBObject Register DP Event
        /// </summary>
        /// <param name="TBObj"></param>
        /// <returns></returns>
        public DP12MSTMessage_TBObjectRegister DP12MSTMessageCreate_Register(ITBObject TBObjRef)
        {
            return (new DP12MSTMessage_TBObjectRegister(m_ID++, TBObjRef));
        }


        ///// <summary>
        ///// Return an instance of the user selecting a trace buffer data set (ML, Aux, or both)
        ///// </summary>
        //public DP12MSTMessage_TBDataModeChange DP12MSTMessageCreate_TBDataModeChange(DP12MST_TRACE_BUFFER_MODE TBMode)
        //{
        //    return (new DP12MSTMessage_TBDataModeChange(m_ID++, TBMode));
        //}


        ///// <summary>
        ///// Return an instance of the user selecting a trace buffer data set (ML, Aux, or both)
        ///// </summary>
        //public DP12MSTMessage_SpeedChange DP12MSTMessageCreate_SpeedChange(float clkPeriod)
        //{
        //    return (new DP12MSTMessage_SpeedChange(m_ID++, clkPeriod));
        //}


        ///// <summary>
        ///// Returns an instance of a cross link order change event.
        ///// </summary>
        ///// <param name="TBMode"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_CrossLinkTriggerOrderChange DP12MSTMessageCreate_CrossLinkTriggerOrderChange(DP12MST_CROSS_LINK_MODE TBMode)
        //{
        //    return (new DP12MSTMessage_CrossLinkTriggerOrderChange(m_ID++, TBMode));
        //}


        ///// <summary>
        ///// Return an instance of the (Re-)Initializtion message... typically used when loading a saved configuration.
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_Initialize DP12MSTMessageCreate_Initialize(bool removeDataFiles = true)
        //{
        //    return (new DP12MSTMessage_Initialize(m_ID++, removeDataFiles));
        //}


        ///// <summary>
        ///// Return an instance of the Probe Manager Running event
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_SetConfig DP12MSTMessageCreate_SetConfig()
        //{
        //    return (new DP12MSTMessage_SetConfig(m_ID++));
        //}


        ///// <summary>
        ///// Return an instance of the Probe Manager Running event --
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        ///// Assumption:  Event is issued at run-time to get all forms to write their 
        /////              current configuraitons to the HW
        //public DP12MSTMessage_SaveConfig DP12MSTMessageCreate_SaveConfig()
        //{
        //    return (new DP12MSTMessage_SaveConfig(m_ID++));
        //}


        ///// <summary>
        ///// Return an instance of the Probe Manager Running event --
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        ///// Assumption:  Event is issued at run-time to get all forms to write their 
        /////              current configuraitons to the HW
        //public DP12MSTMessage_LoadConfig DP12MSTMessageCreate_LoadConfig()
        //{
        //    return (new DP12MSTMessage_LoadConfig(m_ID++));
        //}


        ///// <summary>
        ///// Return an instance of the Probe Manager Running event --
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        ///// Assumption:  Event is issued at run-time to get all forms to write their 
        /////              current configuraitons to the HW
        //public DP12MSTMessage_LoadConfigModule DP12MSTMessageCreate_LoadConfig_Module(string moduleID)
        //{
        //    return (new DP12MSTMessage_LoadConfigModule(m_ID++, moduleID));
        //}


        ///// <summary>
        ///// rReturn an instance of the Clear PM data request event.
        ///// </summary>
        ///// <param name="TBMode"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_ClearTraceBufferData DP12MSTMessage_ClearTraceBufferData(DP12MST_TRACE_BUFFER_MODE TBMode)
        //{
        //    return (new DP12MSTMessage_ClearTraceBufferData(m_ID++, TBMode));
        //}


        ///// <summary>
        ///// Return an instance of the Probe Manager Running event
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_PMRunning DP12MSTMessageCreate_PMRunning(DP12MST_TRACE_BUFFER_MODE mode)
        //{
        //    return (new DP12MSTMessage_PMRunning(m_ID++, mode));
        //}


        ///// <summary>
        ///// Return an instance of the Data Ready event
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_PMStopping_Request DP12MSTMessageCreate_PMStopping_Request()
        //{
        //    return (new DP12MSTMessage_PMStopping_Request(m_ID++));
        //}

        ///// <summary>
        ///// Return an instance of the Data Ready event
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_PMStopping_Ack DPMessageCreate_PMStopping_Ack()
        //{
        //    return (new DP12MSTMessage_PMStopping_Ack(m_ID++));
        //}


        ///// <summary>
        ///// Return an instance of the Data Ready event
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_PMReady DP12MSTMessageCreate_PMReady()
        //{
        //    return (new DP12MSTMessage_PMReady(m_ID++));
        //}


        ///// <summary>
        ///// Return an instance of the Memory Depth having been changed.
        ///// </summary>
        ///// <returns></returns>
        //public DP12MSTMessage_MemoryDepthChanged DP12MSTMessageCreate_MemoryDepthChanged(long depth)
        //{
        //    return (new DP12MSTMessage_MemoryDepthChanged(m_ID++, depth));
        //}


        ///// <summary>
        ///// Return an instance of time slot selection changed event
        ///// </summary>
        ///// <param name="virtualChannelID"></param>
        ///// <param name="slotIDsList"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_TimeSlotSelectionChanged DP12MSTMessageCreate_TimeSlotSelectionChanged(int virtualChannelID, List<int> slotIDsList)
        //{
        //    return (new DP12MSTMessage_TimeSlotSelectionChanged(virtualChannelID, slotIDsList));
        //}

        ///// <summary>
        ///// Return an instance of the Data Available event
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_DataAvailable DP12MSTMessageCreate_DataAvailable()
        //{
        //    return (new DP12MSTMessage_DataAvailable(m_ID++));
        //}


        ///// <summary>
        ///// Return an instance of the Data Ready event
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_DataUploaded DP12MSTMessageCreate_DataUploaded()
        //{
        //    return (new DP12MSTMessage_DataUploaded(m_ID++));
        //}

        ///// <summary>
        ///// Return an instance of the Data Ready event
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_DataReady DP12MSTMessageCreate_DataReady()
        //{
        //    return (new DP12MSTMessage_DataReady(m_ID++));
        //}


        ///// <summary>
        ///// Indicate an listing data set is ready to be processed
        ///// </summary>
        ///// <returns></returns>
        //public DP12MSTMessage_ListingReady DPMessageCreate_ListingReady()
        //{
        //    return (new DP12MSTMessage_ListingReady(m_ID++));
        //}


        public DP12MSTMessage_GetTBBinFileMetaData DP12MSTMessageCreate_GetTBBinFileMetaData()
        {
            return (new DP12MSTMessage_GetTBBinFileMetaData());
        }

        public DP12MSTMessage_StateDataChunkRequest DP12MSTMessageCreate_StateDataChunkRequest(int listingID, int vChannelID, long stateNumber, int chunkSize, int dataOffset = 0)
        {
            return (new DP12MSTMessage_StateDataChunkRequest(m_ID++, listingID, vChannelID, stateNumber, chunkSize, dataOffset));
        }

        ///// <summary>
        ///// Return an instance of a ITBObject Register DP Event
        ///// </summary>
        ///// <param name="TBObj"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_StateDataRequest DP12MSTMessageCreate_StateDataRequest(int vChannelID, long stateNumber, int dataOffset = 0)
        //{
        //    return (new DP12MSTMessage_StateDataRequest(m_ID++, vChannelID, stateNumber, dataOffset));
        //}


        /// <summary>
        /// Return an instance of a ITBObject Register DP Event
        /// </summary>
        /// <param name="TBObj"></param>
        /// <returns></returns>
        public DP12MSTMessage_StateDataResponse DP12MSTMessageCreate_StateDataResponse(long requestID, int vChannelID, long stateNumber, byte[] data)
        {
            return (new DP12MSTMessage_StateDataResponse(m_ID++, requestID, vChannelID, stateNumber, data));
        }



        /// <summary>
        /// Return an instance of an ITBObject DP event to get the data assoicated with a specific state.
        /// </summary>
        /// <param name="requestID"></param>
        /// <param name="stateNumber"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public DP12MSTMessage_GetStateData DP12MSTMessage_GetStateData(long requestID, long stateNumber)
        {
            return (new DP12MSTMessage_GetStateData(m_ID++, stateNumber));
        }

        ///// <summary>
        ///// Return an instance of the Data Ready event
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_AuxDataAvailable DP12MSTMessageCreate_AuxDataAvailable()
        //{
        //    return (new DP12MSTMessage_AuxDataAvailable(m_ID++));
        //}


        ///// <summary>
        ///// Return an instance of the Data Ready event
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_AuxDataReady DP12MSTMessageCreate_AuxDataReady()
        //{
        //    return (new DP12MSTMessage_AuxDataReady(m_ID++));
        //}

        ///// <summary>
        ///// Return an instance of a ITBObject Register DP Event
        ///// </summary>
        ///// <param name="TBObj"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_AuxStateDataRequest DP12MSTMessageCreate_AuxStateDataRequest(long stateNumber, int dataOffset = 0)
        //{
        //    return (new DP12MSTMessage_AuxStateDataRequest(m_ID++, stateNumber, dataOffset));
        //}


        ///// <summary>
        ///// Return an instance of a ITBObject Register DP Event
        ///// </summary>
        ///// <param name="TBObj"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_AuxStateDataResponse DP12MSTMessageCreate_AuxStateDataResponse(long requestID, long stateNumber, byte[] data)
        //{
        //    return (new DP12MSTMessage_AuxStateDataResponse(m_ID++, requestID, stateNumber, data));
        //}


        ///// <summary>
        ///// Returns an instance of an ITBObject Search Request
        ///// </summary>
        ///// <param name="TBMode"></param>
        ///// <param name="columnName"></param>
        ///// <param name="columnValue"></param>
        ///// <param name="stateIndex"></param>
        ///// <param name="searchBackwards"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_SearchListingRequest DP12MSTMessageCreate_SearchListingRequest(DP12MST_TRACE_BUFFER_MODE TBMode, string columnName, uint columnValue, long stateIndex, bool searchBackwards = false)
        //{
        //    return (new DP12MSTMessage_SearchListingRequest(m_ID++, TBMode, columnName, columnValue, stateIndex, searchBackwards));
        //}


        ///// <summary>
        ///// Returns an instance of an ITBObject Search Response
        ///// </summary>
        ///// <param name="TBMode"></param>
        ///// <param name="requestMsgID"></param>
        ///// <param name="matchFound"></param>
        ///// <param name="stateIndex"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_SearchListingResponse DP12MSTMessageCreate_SearchListingResponse(DP12MST_TRACE_BUFFER_MODE TBMode, long requestMsgID, bool matchFound, long stateIndex)
        //{
        //    return (new DP12MSTMessage_SearchListingResponse(m_ID++, requestMsgID, TBMode, matchFound, stateIndex));
        //}


        ///// <summary>
        ///// Returns an instance of an ITBObject search cancel request
        ///// </summary>
        ///// <param name="TBMode"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_SearchListingCancelRequest DP12MSTMessageCreate_SearchListingCancelRequest(DP12MST_TRACE_BUFFER_MODE TBMode)
        //{
        //    return (new DP12MSTMessage_SearchListingCancelRequest(m_ID++, TBMode));
        //}


        ///// <summary>
        ///// Returns an instance of an ITBObject search cancel respone
        ///// </summary>
        ///// <param name="TBMode"></param>
        ///// <param name="requestMsgID"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_SearchListingCancelResponse DP12MSTMessageCreate_SearchListingCancelResponse(DP12MST_TRACE_BUFFER_MODE TBMode, long requestMsgID)
        //{
        //    return (new DP12MSTMessage_SearchListingCancelResponse(m_ID++, requestMsgID, TBMode));
        //}


        ///// <summary>
        ///// Returns an instance of an ITBObject search listing progress report
        ///// </summary>
        ///// <param name="TBMode"></param>
        ///// <param name="statesSearched"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_SearchListingProgessReport DP12MSTMessageCreate_SearchListingProgessReport(DP12MST_TRACE_BUFFER_MODE TBMode, long statesSearched)
        //{
        //    return (new DP12MSTMessage_SearchListingProgessReport(TBMode, statesSearched));
        //}


        ///// <summary>
        ///// Return an instance of an ITBObject Marker get next marker ID
        ///// </summary>
        ///// <param name="markerType"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_MarkerGetNextID DP12MSTMessageCreate_MarkerGetNextID(DP12MST_MarkerInfoID markerType)
        //{
        //    return (new DP12MSTMessage_MarkerGetNextID(markerType));
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
        //public DP12MSTMessage_MarkerAdd DP12MSTMessageCreate_MarkerAdd(DP12MST_MarkerInfoID markerType, int vChannelID, string title, string markerID, long stateIndex)
        //{
        //    return (new DP12MSTMessage_MarkerAdd(m_ID++, markerType, vChannelID, title, markerID, stateIndex));
        //}


        ///// <summary>
        ///// Return an instance of an ITBObject Marker Add DP Event
        ///// </summary>
        ///// <param name="markerType"></param>
        ///// <param name="title"></param>
        ///// <param name="markerID"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_MarkerRemove DP12MSTMessageCreate_MarkerRemove(DP12MST_MarkerInfoID markerType, int vChannelID, string title, string markerID)
        //{
        //    return (new DP12MSTMessage_MarkerRemove(m_ID++, markerType, vChannelID, title, markerID));
        //}


        ///// <summary>
        ///// Return an instance of an ITBObject Marker Add DP Event
        ///// </summary>
        ///// <param name="markerType"></param>
        ///// <param name="title"></param>
        ///// <param name="markerID"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_MarkerChangeLocation DP12MSTMessageCreate_MarkerChangeLocation(DP12MST_MarkerInfoID markerType, int vChannelID, string title, string markerID, int stateIndex)
        //{
        //    return (new DP12MSTMessage_MarkerChangeLocation(m_ID++, markerType, vChannelID, title, markerID, stateIndex));
        //}


        ///// <summary>
        ///// Get a list of markers from the marker info manager object
        ///// </summary>
        ///// <param name="markerType"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_MarkerGetList DP12MSTMessageCreate_MarkerGetList(DP12MST_MarkerInfoID markerType, int vChannelID = -1)
        //{
        //    return (new DP12MSTMessage_MarkerGetList(markerType, vChannelID));
        //}


        ///// <summary>
        ///// Get a marker list (response to a get marker list request)
        ///// </summary>
        ///// <param name="markerInfoList"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_MarkerGetListResponse_MainLink DP12MSTMessageCreate_MarkerGetListResponse_MainLink(List<DP12MST_MarkerInfo_MainLink> markerInfoList)
        //{
        //    return (new DP12MSTMessage_MarkerGetListResponse_MainLink(markerInfoList));
        //}

        ///// <summary>
        ///// Get a marker list (response to a get marker list request)
        ///// </summary>
        ///// <param name="markerInfoList"></param>
        ///// <returns></returns>
        //public DP12MSTMessage_MarkerGetListResponse_AuxiliaryLink DP12MSTMessageCreate_MarkerGetListResponse_AuxiliaryLink(List<DP12MST_MarkerInfo_AuxiliaryLink> markerInfoList)
        //{
        //    return (new DP12MSTMessage_MarkerGetListResponse_AuxiliaryLink(markerInfoList));
        //}
        #endregion // Public Methods
    }
}