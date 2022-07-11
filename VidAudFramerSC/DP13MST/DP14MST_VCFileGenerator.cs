using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DP14MSTClassLibrary
{
    public class DP14MST_VCFileGenerator
    {
        #region Members


        private static DP14MST_VCFileGenerator m_instance = new DP14MST_VCFileGenerator();
        //private DP14MSTVCFileGenerator_BGWorker m_VCFileGenerator_BGWorker = null;
        private DP14MSTVCFileGenerator_Threads m_VCFileGenerator_Threads = null;

        //private string m_FS4500_FOLDER_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "FuturePlus";
        //private string m_FS4500_FOLDER_NAME = "FS4500";
        private string m_instanceFolderPath = string.Empty;

        private const string m_FS4500_VC_BASE_FILE_NAME = "VirtualChannel";

        public VCFileGenerationStatusEvent VCFileGenStatusEvent = null;

        #endregion // Members

        #region Ctor

        /// <summary>
        /// Default Constructor
        /// </summary>
        private DP14MST_VCFileGenerator()
        {
            //m_VCFileGenerator_BGWorker = new DP14MSTVCFileGenerator_BGWorker();
            m_VCFileGenerator_Threads = new DP14MSTVCFileGenerator_Threads();

            //m_VCFileGenerator_BGWorker.VCFileGenStatusEvent += new VCFileGenerationStatusEvent(processVCFileGenEvent);
            m_VCFileGenerator_Threads.VCFileGenStatusEvent += new VCFileGenerationStatusEvent(processVCFileGenEvent);
        }

        #endregion // Ctor

        #region Event Handlers

        /// <summary>
        /// Bubble events up to the Trace Data Mgr object
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void processVCFileGenEvent(object sender, VCFileGenerationStatusEventArgs e)
        {
            // bubble the event up to the Trace Data Mgr object
            if (VCFileGenStatusEvent != null)
                VCFileGenStatusEvent(this, e);
        }

        #endregion // Event Handlers

        #region Private Methods

        ///// <summary>
        ///// Create four virtual channel files (form a single data file)
        ///// </summary>
        ///// <param name="fileName"></param>
        ///// <returns></returns>
        //private bool createVirtualChannelDataFiles_BGWorker(string dataFileName)
        //{
        //    bool status = true;

        //    string dataFilePath = Path.Combine(m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME, dataFileName);
        //    if (File.Exists(dataFilePath))
        //    {
        //        // process the single data file
        //        m_VCFileGenerator_BGWorker.GenerateDataFiles(dataFileName);
        //    }
        //    else
        //    {
        //        status = false;
        //    }

        //    return status;
        //}


        /// <summary>
        /// Create four virtual channel files for every data file.
        /// </summary>
        /// <param name="dataFileNames"></param>
        /// <returns></returns>
        private bool createVirtualChannelDataFiles_Tasks(List<string> dataFileNames, long triggerTimeStamp, int trigChannelID)
        {
            bool status = true;

            // create the VC file generator that uses cascading threads
            m_VCFileGenerator_Threads.GenerateDataFiles(dataFileNames, triggerTimeStamp, trigChannelID);

            return status;
        }


        /// <summary>
        /// Get the trigger location in the segregated data
        /// </summary>
        /// <param name="triggerTimeStamp"></param>
        /// <param name="trigChannelID"></param>
        /// <returns></returns>
        private void getSegregatedTriggerLocation(long triggerTimeStamp, int trigChannelID)
        {
            m_VCFileGenerator_Threads.GetSegregatedTriggerLocation(triggerTimeStamp, trigChannelID);
        }


        /// <summary>
        /// ???
        /// </summary>
        /// <param name="triggerTimeStamp"></param>
        /// <param name="trigChannelID"></param>
        private void getSegregatedMetaData(long triggerTimeStamp, int trigChannelID)
        {
            m_VCFileGenerator_Threads.GetSegregatedMetaData(triggerTimeStamp, trigChannelID);
        }


        private void setMetaDataTriggerStateIndices(long triggerTimeStamp, int trigChannelID)
        {
            m_VCFileGenerator_Threads.SetMetaDataTriggerStateIndices(triggerTimeStamp, trigChannelID);
        }


        /// <summary>
        /// ???
        /// </summary>
        /// <param name="triggerTimeStamp"></param>
        /// <param name="trigChannelID"></param>
        private void prependSegregatedMetaData(long triggerTimeStamp, int trigChannelID)
        {
            m_VCFileGenerator_Threads.PrependSegregatedMetaData(triggerTimeStamp, trigChannelID);
        }

        #endregion // Private Methods

        #region Public Methods

        /// <summary>
        /// Returns a single thread safe instance of the class object.
        /// </summary>
        /// <returns></returns>
        public static DP14MST_VCFileGenerator GetInstance()
        {
            return m_instance;
        }

        public bool SetDataFolderPath(string path)
        {
            bool status = true;
            m_instanceFolderPath = path;
            m_VCFileGenerator_Threads.SetDataFolderPath(m_instanceFolderPath);

            return status;
        }


        /// <summary>
        /// Update the virtual channel selected slot information. 
        /// </summary>
        /// <param name="msg"></param>
        public void UpdateSelectedSlotsInfo(DP14MSTMessage_TimeSlotSelectionChanged msg)
        {
            m_VCFileGenerator_Threads.UpdateSelectedSlotsInfo(msg);
        }

        public void InitializeProcessingMembers()
        {
            m_VCFileGenerator_Threads.InitializeProcessingMembers();
        }

        /// <summary>
        /// Segrate the data into virtural channel data files.
        /// </summary>
        /// <param name="dataFileNames"></param>
        /// <returns></returns>
        /// Assumptions:  DataFileNames list contains one or more names. 
        ///               DataFileNames do not specify the path
        public bool CreateVirtualChannelDataFiles(List<string> dataFileNames, long triggerTimeStamp, int trigChannelID)
        {
            bool status = true;

            if (dataFileNames.Count > 0)
            {
                status = createVirtualChannelDataFiles_Tasks(dataFileNames, triggerTimeStamp, trigChannelID);
            }
            else
            {
                // raise the data ready event?  which would unlock the forms...
            }
            return status;
        }


        /// <summary>
        /// ???
        /// </summary>
        /// <param name="triggerTimeStamp"></param>
        /// <param name="trigChannelID"></param>
        /// <returns></returns>
        public bool GetSegregatedTriggerLocation(long triggerTimeStamp, int trigChannelID)
        {
            bool status = true;

            getSegregatedTriggerLocation(triggerTimeStamp, trigChannelID);

            return status;
        }


        /// <summary>
        /// ???
        /// </summary>
        /// <param name="triggerTimeStamp"></param>
        /// <param name="trigChannelID"></param>
        /// <returns></returns>
        public bool GetSegregatedMetaData(long triggerTimeStamp, int trigChannelID)
        {
            bool status = true;

            getSegregatedMetaData(triggerTimeStamp, trigChannelID);

            return status;
        }


        /// <summary>
        /// ???
        /// </summary>
        /// <param name="triggerTimeStamp"></param>
        /// <param name="trigChannelID"></param>
        /// <returns></returns>
        public bool SetMetaDataTriggerStateIndices(long triggerTimeStamp, int trigChannelID)
        {
            bool status = true;

            setMetaDataTriggerStateIndices(triggerTimeStamp, trigChannelID);

            return status;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="triggerTimeStamp"></param>
        /// <param name="trigChannelID"></param>
        /// <returns></returns>
        public bool PrependSegregatedMetaData(long triggerTimeStamp, int trigChannelID)
        {
            bool status = true;

            prependSegregatedMetaData(triggerTimeStamp, trigChannelID);

            return status;
        }


        /// <summary>
        /// ???
        /// </summary>
        /// <returns></returns>
        public List<DP14MSTVCFileGenerator_Threads.DP14MSTMultiThread_MetaDataArgs>  GetMultiThreadedFileMetaData()
        {
            return m_VCFileGenerator_Threads.GetMultiThreadedFileMetaData().ToList();
        }
        #endregion // Public Methods
    }


    //
    // VCFileGenerationStatusEvent... used by both the VC File Generator BGWoker and Threads objects... 
    //
    public delegate void VCFileGenerationStatusEvent(object sender, VCFileGenerationStatusEventArgs e);

    //Event Args used to identify who has a background task running...
    public class VCFileGenerationStatusEventArgs : EventArgs
    {
        public string Title;   // VCGenerationInProgress and VCGenerationComplete
        public float Parameter;  // most probably percentage

        public VCFileGenerationStatusEventArgs(string title, float parameter = 0.0f)
        {
            this.Title = title;
            this.Parameter = parameter;
        }
    }
}
