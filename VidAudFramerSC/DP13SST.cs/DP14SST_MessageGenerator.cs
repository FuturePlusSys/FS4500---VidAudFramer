using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP14SSTClassLibrary
{
    class DP14SST_MessageGenerator
    {
        #region Members

        private static DP14SST_MessageGenerator m_instance = new DP14SST_MessageGenerator();
        private uint m_ID = 0;

        #endregion // Members

        #region Ctor

        // Singleton is constructed when the DLL is created.
        private DP14SST_MessageGenerator()
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
        public static DP14SST_MessageGenerator GetInstance()
        {
            return m_instance;
        }

        /// <summary>
        /// Return an instance of a ITBObject Register DP Event
        /// </summary>
        /// <param name="TBObj"></param>
        /// <returns></returns>
        public DP14SSTMessage_TBObjectRegister DP14SSTMessageCreate_Register(ITBObject TBObjRef)
        {
            return (new DP14SSTMessage_TBObjectRegister(m_ID++, TBObjRef));
        }


        /// <summary>
        /// Return an instance of the user selecting a trace buffer data set (ML, Aux, or both)
        /// </summary>
        public DP14SSTMessage_TBDataModeChange DP14SSTMessageCreate_TBDataModeChange(DP14SST_TRACE_BUFFER_MODE TBMode)
        {
            return (new DP14SSTMessage_TBDataModeChange(m_ID++, TBMode));
        }

        /// <summary>
        /// Return an instance of the user selecting a trace buffer data set (ML, Aux, or both)
        /// </summary>
        public DP14SSTMessage_SpeedChange DP14SSTMessageCreate_SpeedChange(float clkPeriod)
        {
            return (new DP14SSTMessage_SpeedChange(m_ID++, clkPeriod));
        }


        /// <summary>
        /// Returns an instance of a cross link order change event.
        /// </summary>
        /// <param name="TBMode"></param>
        /// <returns></returns>
        public DP14SSTMessage_CrossLinkTriggerOrderChange DP14SSTMessageCreate_CrossLinkTriggerOrderChange(DP14SST_CROSS_LINK_MODE TBMode)
        {
            return (new DP14SSTMessage_CrossLinkTriggerOrderChange(m_ID++, TBMode));
        }


        /// <summary>
        /// Return an instance of the (Re-)Initializtion message... typically used when loading a saved configuration.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DP14SSTMessage_Initialize DP14SSTMessageCreate_Initialize(bool removeDataFiles = true)
        {
            return (new DP14SSTMessage_Initialize(m_ID++, removeDataFiles));
        }


        /// <summary>
        /// Return an instance of the Probe Manager Running event
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DP14SSTMessage_SetConfig DP14SSTMessageCreate_SetConfig()
        {
            return (new DP14SSTMessage_SetConfig(m_ID++));
        }


        /// <summary>
        /// Return an instance of the Probe Manager Running event --
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// Assumption:  Event is issued at run-time to get all forms to write their 
        ///              current configuraitons to the HW
        public DP14SSTMessage_SaveConfig DP14SSTMessageCreate_SaveConfig()
        {
            return (new DP14SSTMessage_SaveConfig(m_ID++));
        }


        /// <summary>
        /// Return an instance of the Probe Manager Running event --
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// Assumption:  Event is issued at run-time to get all forms to write their 
        ///              current configuraitons to the HW
        public DP14SSTMessage_LoadConfig DP14SSTMessageCreate_LoadConfig()
        {
            return (new DP14SSTMessage_LoadConfig(m_ID++));
        }


        /// <summary>
        /// Return an instance of the Probe Manager Running event --
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// Assumption:  Event is issued at run-time to get all forms to write their 
        ///              current configuraitons to the HW
        public DP14SSTMessage_LoadConfigModule DP14SSTMessageCreate_LoadConfig_Module(string moduleID)
        {
            return (new DP14SSTMessage_LoadConfigModule(m_ID++, moduleID));
        }

        /// <summary>
        /// rReturn an instance of the Clear PM data request event.
        /// </summary>
        /// <param name="TBMode"></param>
        /// <returns></returns>
        public DP14SSTMessage_ClearTraceBufferData DP14SSTMessage_ClearTraceBufferData(DP14SST_TRACE_BUFFER_MODE TBMode)
        {
            return (new DP14SSTMessage_ClearTraceBufferData(m_ID++, TBMode));
        }


        /// <summary>
        /// Return an instance of the Probe Manager Running event
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DP14SSTMessage_PMRunning DP14SSTMessageCreate_PMRunning(DP14SST_TRACE_BUFFER_MODE mode)
        {
            return (new DP14SSTMessage_PMRunning(m_ID++, mode));
        }



        /// <summary>
        /// Return an instance of the Data Ready event
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DP14SSTMessage_PMStopping_Request DP14SSTMessageCreate_PMStopping_Request()
        {
            return (new DP14SSTMessage_PMStopping_Request(m_ID++));
        }

        /// <summary>
        /// Return an instance of the Data Ready event
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DP14SSTMessage_PMStopping_Ack DPMessageCreate_PMStopping_Ack()
        {
            return (new DP14SSTMessage_PMStopping_Ack(m_ID++));
        }


        /// <summary>
        /// Return an instance of the Data Ready event
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DP14SSTMessage_PMReady DP14SSTMessageCreate_PMReady()
        {
            return (new DP14SSTMessage_PMReady(m_ID++));
        }


        /// <summary>
        /// Return an instance of the Memory Depth having been changed.
        /// </summary>
        /// <returns></returns>
        public DP14SSTMessage_MemoryDepthChanged DP14SSTMessageCreate_MemoryDepthChanged(long depth)
        {
            return (new DP14SSTMessage_MemoryDepthChanged(m_ID++, depth));
        }


        /// <summary>
        /// Return an instance of the Data Available event
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DP14SSTMessage_DataAvailable DP14SSTMessageCreate_DataAvailable()
        {
            return (new DP14SSTMessage_DataAvailable(m_ID++));
        }

        /// <summary>
        /// Return an instance of the Data Ready event
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DP14SSTMessage_DataReady DP14SSTMessageCreate_DataReady()
        {
            return (new DP14SSTMessage_DataReady(m_ID++));
        }


        /// <summary>
        /// Indicate an listing data set is ready to be processed
        /// </summary>
        /// <returns></returns>
        public DP14SSTMessage_ListingReady DPMessageCreate_ListingReady()
        {
            return (new DP14SSTMessage_ListingReady(m_ID++));
        }


        public DP14SSTMessage_GetTBBinFileMetaData DP14SSTMessageCreate_GetTBBinFileMetaData()
        {
            return (new DP14SSTMessage_GetTBBinFileMetaData());
        }


        /// <summary>
        /// Get a defined set of states
        /// </summary>
        /// <param name="listingID"></param>
        /// <param name="stateNumber"></param>
        /// <param name="dataOffset"></param>
        /// <returns></returns>
        public DP14SSTMessage_StateDataChunkRequest DP14SSTMessageCreate_StateDataChunkRequest(int listingID, long stateNumber, int chunkSize, int dataOffset = 0)
        {
            return (new DP14SSTMessage_StateDataChunkRequest(m_ID++, listingID, stateNumber, chunkSize, dataOffset));
        }


        /// <summary>
        /// Return an instance of a ITBObject Register DP Event
        /// </summary>
        /// <param name="TBObj"></param>
        /// <returns></returns>
        public DP14SSTMessage_StateDataRequest DP14SSTMessageCreate_StateDataRequest(long stateNumber, int dataOffset = 0)
        {
            return (new DP14SSTMessage_StateDataRequest(m_ID++, stateNumber, dataOffset));
        }


        /// <summary>
        /// Return an instance of a ITBObject Register DP Event
        /// </summary>
        /// <param name="TBObj"></param>
        /// <returns></returns>
        public DP14SSTMessage_StateDataResponse DP14SSTMessageCreate_StateDataResponse(long requestID, long stateNumber, byte[] data)
        {
            return (new DP14SSTMessage_StateDataResponse(m_ID++, requestID, stateNumber, data));
        }



        /// <summary>
        /// Return an instance of an ITBObject DP event to get the data assoicated with a specific state.
        /// </summary>
        /// <param name="requestID"></param>
        /// <param name="stateNumber"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public DP14SSTMessage_GetStateData DP14SSTMessage_GetStateData(long requestID, long stateNumber)
        {
            return (new DP14SSTMessage_GetStateData(m_ID++, stateNumber));
        }

        /// <summary>
        /// Return an instance of the Data Ready event
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DP14SSTMessage_AuxDataAvailable DP14SSTMessageCreate_AuxDataAvailable()
        {
            return (new DP14SSTMessage_AuxDataAvailable(m_ID++));
        }


        /// <summary>
        /// Return an instance of the Data Ready event
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DP14SSTMessage_AuxDataReady DP14SSTMessageCreate_AuxDataReady()
        {
            return (new DP14SSTMessage_AuxDataReady(m_ID++));
        }

        /// <summary>
        /// Return an instance of a ITBObject Register DP Event
        /// </summary>
        /// <param name="TBObj"></param>
        /// <returns></returns>
        public DP14SSTMessage_AuxStateDataRequest DP14SSTMessageCreate_AuxStateDataRequest(long stateNumber, int dataOffset = 0)
        {
            return (new DP14SSTMessage_AuxStateDataRequest(m_ID++, stateNumber, dataOffset));
        }


        /// <summary>
        /// Return an instance of a ITBObject Register DP Event
        /// </summary>
        /// <param name="TBObj"></param>
        /// <returns></returns>
        public DP14SSTMessage_AuxStateDataResponse DP14SSTMessageCreate_AuxStateDataResponse(long requestID, long stateNumber, byte[] data)
        {
            return (new DP14SSTMessage_AuxStateDataResponse(m_ID++, requestID, stateNumber, data));
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
        public DP14SSTMessage_SearchListingRequest DP14SSTMessageCreate_SearchListingRequest(DP14SST_TRACE_BUFFER_MODE TBMode, string columnName, uint columnValue, long stateIndex, bool searchBackwards = false)
        {
            return (new DP14SSTMessage_SearchListingRequest(m_ID++, TBMode, columnName, columnValue, stateIndex, searchBackwards));
        }


        /// <summary>
        /// Returns an instance of an ITBObject Search Response
        /// </summary>
        /// <param name="TBMode"></param>
        /// <param name="requestMsgID"></param>
        /// <param name="matchFound"></param>
        /// <param name="stateIndex"></param>
        /// <returns></returns>
        public DP14SSTMessage_SearchListingResponse DP14SSTMessageCreate_SearchListingResponse(DP14SST_TRACE_BUFFER_MODE TBMode, long requestMsgID, bool matchFound, long stateIndex)
        {
            return (new DP14SSTMessage_SearchListingResponse(m_ID++, requestMsgID, TBMode, matchFound, stateIndex));
        }


        /// <summary>
        /// Returns an instance of an ITBObject search cancel request
        /// </summary>
        /// <param name="TBMode"></param>
        /// <returns></returns>
        public DP14SSTMessage_SearchListingCancelRequest DP14SSTMessageCreate_SearchListingCancelRequest(DP14SST_TRACE_BUFFER_MODE TBMode)
        {
            return (new DP14SSTMessage_SearchListingCancelRequest(m_ID++, TBMode));
        }


        /// <summary>
        /// Returns an instance of an ITBObject search cancel respone
        /// </summary>
        /// <param name="TBMode"></param>
        /// <param name="requestMsgID"></param>
        /// <returns></returns>
        public DP14SSTMessage_SearchListingCancelResponse DP14SSTMessageCreate_SearchListingCancelResponse(DP14SST_TRACE_BUFFER_MODE TBMode, long requestMsgID)
        {
            return (new DP14SSTMessage_SearchListingCancelResponse(m_ID++, requestMsgID, TBMode));
        }


        /// <summary>
        /// Returns an instance of an ITBObject search listing progress report
        /// </summary>
        /// <param name="TBMode"></param>
        /// <param name="statesSearched"></param>
        /// <returns></returns>
        public DP14SSTMessage_SearchListingProgessReport DP14SSTMessageCreate_SearchListingProgessReport(DP14SST_TRACE_BUFFER_MODE TBMode, long statesSearched)
        {
            return (new DP14SSTMessage_SearchListingProgessReport(TBMode, statesSearched));
        }


        ///// <summary>
        ///// Return an instance of an ITBObject Marker get next marker ID
        ///// </summary>
        ///// <param name="markerType"></param>
        ///// <returns></returns>
        //public DP14SSTMessage_MarkerGetNextID DP14SSTMessageCreate_MarkerGetNextID(DP14SST_MarkerInfoID markerType)
        //{
        //    return (new DP14SSTMessage_MarkerGetNextID(markerType));
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
        //public DP14SSTMessage_MarkerAdd DP14SSTMessageCreate_MarkerAdd(DP14SST_MarkerInfoID markerType, string title, string markerID, long stateIndex)
        //{
        //    return (new DP14SSTMessage_MarkerAdd(m_ID++, markerType, title, markerID, stateIndex));
        //}


        ///// <summary>
        ///// Return an instance of an ITBObject Marker Add DP Event
        ///// </summary>
        ///// <param name="markerType"></param>
        ///// <param name="title"></param>
        ///// <param name="markerID"></param>
        ///// <returns></returns>
        //public DP14SSTMessage_MarkerRemove DP14SSTMessageCreate_MarkerRemove(DP14SST_MarkerInfoID markerType, string title, string markerID)
        //{
        //    return (new DP14SSTMessage_MarkerRemove(m_ID++, markerType, title, markerID));
        //}


        ///// <summary>
        ///// Return an instance of an ITBObject Marker Add DP Event
        ///// </summary>
        ///// <param name="markerType"></param>
        ///// <param name="title"></param>
        ///// <param name="markerID"></param>
        ///// <returns></returns>
        //public DP14SSTMessage_MarkerChangeLocation DP14SSTMessageCreate_MarkerChangeLocation(DP14SST_MarkerInfoID markerType, string title, string markerID, int stateIndex)
        //{
        //    return (new DP14SSTMessage_MarkerChangeLocation(m_ID++, markerType, title, markerID, stateIndex));
        //}


        ///// <summary>
        ///// Get a list of markers from the marker info manager object
        ///// </summary>
        ///// <param name="markerType"></param>
        ///// <returns></returns>
        //public DP14SSTMessage_MarkerGetList DP14SSTMessageCreate_MarkerGetList(DP14SST_MarkerInfoID markerType)
        //{
        //    return (new DP14SSTMessage_MarkerGetList(markerType));
        //}


        ///// <summary>
        ///// Get a marker list (response to a get marker list request)
        ///// </summary>
        ///// <param name="markerInfoList"></param>
        ///// <returns></returns>
        //public DP14SSTMessage_MarkerGetListResponse_MainLink DP14SSTMessageCreate_MarkerGetListResponse_MainLink(List<DP14SST_MarkerInfo_MainLink> markerInfoList)
        //{
        //    return (new DP14SSTMessage_MarkerGetListResponse_MainLink(markerInfoList));
        //}

        ///// <summary>
        ///// Get a marker list (response to a get marker list request)
        ///// </summary>
        ///// <param name="markerInfoList"></param>
        ///// <returns></returns>
        //public DP14SSTMessage_MarkerGetListResponse_AuxiliaryLink DP14SSTMessageCreate_MarkerGetListResponse_AuxiliaryLink(List<DP14SST_MarkerInfo_AuxiliaryLink> markerInfoList)
        //{
        //    return (new DP14SSTMessage_MarkerGetListResponse_AuxiliaryLink(markerInfoList));
        //}
        #endregion // Public Methods
    }
}
