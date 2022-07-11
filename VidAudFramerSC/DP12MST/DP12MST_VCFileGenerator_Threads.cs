using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharedProject1;

namespace DP12MSTClassLibrary
{
    public class DP12MST_VCFileGenerator_Threads
    {
        #region Members

        private class DP12MST_TaskStateObject
        {
            public string FolderPath { get; set; }

            public VCFileGenerationStatusEvent FileGenerationStatusEvent { get; set; }

            public float ProgressBarPercentage { get; set; }

            public int Index { get; set; }

            public CancellationToken Token { get; set; }

            public DP12MST_TaskStateObject(string path, VCFileGenerationStatusEvent fileGenStatusEvent, float percentage, int index, CancellationToken ct)
            {
                FolderPath = path;
                FileGenerationStatusEvent = fileGenStatusEvent;
                ProgressBarPercentage = percentage;
                Index = index;
                Token = ct;
            }

            public DP12MST_TaskStateObject()
            {
            }
        }


        private class DP12MST_TrigLocationTaskStateObject
        {
            public VCFileGenerationStatusEvent FileGenerationStatusEvent { get; set; }

            public float ProgressBarPercentage { get; set; }

            public long TriggerTimeStamp { get; set; }

            public int TriggerChannelID { get; set; }

            public CancellationToken Token { get; set; }

            public DP12MST_TrigLocationTaskStateObject(long trigTimeStamp,  int trigChannelID, VCFileGenerationStatusEvent fileGenStatusEvent, float percentage, CancellationToken ct)
            {
                TriggerTimeStamp = trigTimeStamp;
                TriggerChannelID = trigChannelID;
                FileGenerationStatusEvent = fileGenStatusEvent;
                ProgressBarPercentage = percentage;
                Token = ct;
            }
        }


        private class DP12MST_GetMetaDataTaskStateObject
        {
            public int NumberOfColumns { get; set; }

            public int NumberOfLevels { get; set; }

            public int NumberOfChannels { get; set; }

            public int TriggerIndex{ get; set; }

            public int TriggerVChannelID { get; set; }

            public VCFileGenerationStatusEvent FileGenerationStatusEvent { get; set; }

            public float ProgressBarPercentage { get; set; }

            public CancellationToken Token { get; set; }

            public DP12MST_GetMetaDataTaskStateObject(int numOfColumns, int numOfLevels, int numOfChannels, int triggerIndex, int triggerVChannelID, VCFileGenerationStatusEvent fileGenStatusEvent, float percentage, CancellationToken ct)
            {
                NumberOfColumns = numOfColumns;
                NumberOfLevels = numOfLevels;
                NumberOfChannels = numOfChannels; ;
                TriggerIndex = triggerIndex;
                TriggerVChannelID = triggerVChannelID;
                FileGenerationStatusEvent = fileGenStatusEvent;
                ProgressBarPercentage = percentage;
                Token = ct;
            }
        }
        


        private class DP12MST_GetTrigStateIndicesTaskStateObject
        {
            public UploadMetaData[] MetaData { get; set; }

            public long TriggerTimeStamp { get; set; }

            public CancellationToken Token { get; set; }

            public VCFileGenerationStatusEvent FileGenerationStatusEvent { get; set; }

            public float ProgressBarPercentage { get; set; }

            public DP12MST_GetTrigStateIndicesTaskStateObject(UploadMetaData[] metaDataRef, long trigTimeStamp, VCFileGenerationStatusEvent fileGenStatusEvent, float percentage, CancellationToken ct)
            {
                MetaData = metaDataRef;
                TriggerTimeStamp = trigTimeStamp;
                FileGenerationStatusEvent = fileGenStatusEvent;
                ProgressBarPercentage = percentage;
                Token = ct;
            }
        }


        private class DP12MST_PrependMetaTaskStateObject
        {
            public int ChannelID { get; set; }

            public UploadMetaData[] MetaData { get; set; }

            public VCFileGenerationStatusEvent FileGenerationStatusEvent { get; set; }

            public float ProgressBarPercentage { get; set; }

            public CancellationToken Token { get; set; }

            public DP12MST_PrependMetaTaskStateObject(int channelID, UploadMetaData[] metaDataRef, VCFileGenerationStatusEvent fileGenStatusEvent, float percentage, CancellationToken ct)
            {
                ChannelID = channelID;
                MetaData = metaDataRef;
                FileGenerationStatusEvent = fileGenStatusEvent;
                ProgressBarPercentage = percentage;
                Token = ct;
            }
        }


        /// <summary>
        /// Repository class object used during the initialize of the segregated data files.
        /// </summary>
        private class UploadMetaData
        {
            public const string BIN_FILE_HEADER_START_STATE = "StartState";
            public const string BIN_FILE_HEADER_END_STATE = "EndState";
            public const string BIN_FILE_HEADER_TRIG_STATE = "TrigOffset";

            public int StartStateIndex { get; set; }
            public int EndStateIndex { get; set; }
            public int TriggerVChannelID { get; set; }
            public int TrigStateIndex { get; set; }
            public int DataOffset { get; set; }

            public int BlockID = -1;
            public int VirtualChannelID = -1;

            // these hold the state index that following immediately after the trigger state
            // on the virtual channels that do not contain the trigger state... one of these
            // will be set to 0
            public int VChannel1TrigStateIndex { get; set; }
            public int VChannel2TrigStateIndex { get; set; }
            public int VChannel3TrigStateIndex { get; set; }
            public int VChannel4TrigStateIndex { get; set; }


            public UploadMetaData()
            {
                StartStateIndex = -1;
                EndStateIndex = -1;
                TriggerVChannelID = -1;
                TrigStateIndex = -1;
                DataOffset = 0;
            }

            public UploadMetaData(int blockID, int virtualChannelID, int startStateIndex= -1, int endStateIndex = -1, int trigVChannelID = -1, int trigStateIndex = -1, int offset = 0)
            {
                BlockID = blockID;
                VirtualChannelID = virtualChannelID;
                StartStateIndex = startStateIndex;
                EndStateIndex = endStateIndex;
                TriggerVChannelID = trigVChannelID;
                TrigStateIndex = trigStateIndex;
                 DataOffset = offset;
            }


            public void Copy(DP12MST_MultiThread_MetaDataArgs destMetaData)
            {
                destMetaData.StartState = StartStateIndex;
                destMetaData.EndState = EndStateIndex;
                destMetaData.TrigVChannelID = TriggerVChannelID;
                destMetaData.VChannel1ID = VChannel1TrigStateIndex;
                destMetaData.VChannel2ID = VChannel2TrigStateIndex;
                destMetaData.VChannel3ID = VChannel3TrigStateIndex;
                destMetaData.VChannel4ID = VChannel4TrigStateIndex;
                destMetaData.DataOffset = DataOffset;
            }
        }



        /// <summary>
        /// Repository object used to hold meta data assoicated with multi-thread data files 
        /// </summary>
        public class DP12MST_MultiThread_MetaDataArgs
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


            private int m_trigVChannelID = -1;
            public int TrigVChannelID
            {
                get { return m_trigVChannelID; }
                set { m_trigVChannelID = value; }
            }


            private int m_VChannel1ID = -1;
            public int VChannel1ID
            {
                get { return m_VChannel1ID; }
                set { m_VChannel1ID = value; }
            }


            private int m_VChannel2ID = -1;
            public int VChannel2ID
            {
                get { return m_VChannel2ID; }
                set { m_VChannel2ID = value; }
            }


            private int m_VChannel3ID = -1;
            public int VChannel3ID
            {
                get { return m_VChannel3ID; }
                set { m_VChannel3ID = value; }
            }


            private int m_VChannel4ID = -1;
            public int VChannel4ID
            {
                get { return m_VChannel4ID; }
                set { m_VChannel4ID = value; }
            }


            private int m_dataOffset = -1;
            public int DataOffset
            {
                get { return m_dataOffset; }
                set { m_dataOffset = value; }
            }
        }

        private string m_FS4500_FOLDER_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "FuturePlus";
        private const string m_FS4500_FOLDER_NAME = "FS4500";
        private const string m_FS4500_TRACE_FILE_BASE_NAME = "TraceData.bin";
        private const string m_FS4500_VC_BASE_FILE_NAME = "VirtualChannel";

        private CancellationTokenSource m_tokenSource = null; 
        private CancellationToken m_token = CancellationToken.None;

        public VCFileGenerationStatusEvent VCFileGenStatusEvent = null;

        private const int STATE_BYTE_LENGTH = 16;
        private const int VC_CHANNEL_COUNT = 4;
        private const int VC_TAG_BYTE_ID = 0x08;
        private const int VC_TAG_FIELD_MASK = 0x38;
        private const int VC_TAG_FIELD_SHIFT = 0x03;
        private const string VC_FILE_PREFIX = "VChannelData_Block";
        private const string BLOCK_VC_FILENAME_BASE = "TraceData_";
        private const string BLOCK_0_UPLOAD_DATA_FILENAME = "TraceData_0.bin";

        private const int NUMBER_OF_TIME_SLOTS = 64;
        private const int TB_STATE_BYTE_LEN = 16;
        private const int VCTAGS_BTYE_ID = 8;
        private const int VCTAGS_FIELD_MASK = 0x38;
        private const int VCTAGS_FIELD_SHIFT = 0x3;

        private int m_VC1_timeSlotCount = 14; //0;
        private int m_VC2_timeSlotCount = 14; //0;
        private int m_VC3_timeSlotCount = 0;
        private int m_VC4_timeSlotCount = 0;
        private int m_TriggerVCTimeSlotCount = 0;

        private bool m_debugDataFiles = false;  //true


        private int m_numOfColumns = 0x00;
        private int m_numOfLevels = 0x00;
        private int m_numOfChannels = 4;
        private long m_triggerTimeStamp = 0x00;
        private int m_trigChannelID = 0x00;
        private int m_segregatedTriggerStateIndex = 0x00;
        private UploadMetaData[] m_uploadMetaData = null;

        #endregion // Members

        #region Ctor

        public DP12MST_VCFileGenerator_Threads()
        {
        }
        #endregion // Ctor

        #region Private Methods

        /// <summary>
        /// Segregates data files into virtual channel data files.
        /// </summary>
        /// <param name="dataFileNames"></param>
        /// <param name="numOfColumns"></param>
        /// <param name="numOfLevels"></param>
        /// <returns></returns>
        private bool generateVirtualChannelFiles(List<string> dataFileNames, int numOfColumns, int numOfLevels)
        {
            bool status = true;

            return status;
        }


        /// <summary>
        /// Clear all data files (from previous runs).
        /// </summary>
        private void clearDataFiles()
        {
            string[] filePaths = Directory.GetFiles(m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME);
            foreach (string path in filePaths)
            {
                string fileName = Path.GetFileName(path);
                if (fileName.StartsWith("TraceData") || fileName.StartsWith(m_FS4500_VC_BASE_FILE_NAME) || fileName.StartsWith("VChannelData"))
                {
                    File.Delete(path);
                }
            }
        }


        /// <summary>
        /// Clear a specified foldef of all files and sub-directories
        /// </summary>
        /// <param name="FolderName"></param>
        private void clearFolder(string FolderName)
        {
            DirectoryInfo dir = new DirectoryInfo(FolderName);

            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.Delete();
            }

            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                clearFolder(di.FullName);
                di.Delete();
            }
        }


        /// <summary>
        /// Clear all data files (from previous runs).
        /// </summary>
        private void clearVCDataFiles()
        {
            if (m_debugDataFiles)  // for testing
                clearFolder(m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME + "\\debug\\");


            string[] filePaths = Directory.GetFiles(m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME);
            foreach (string path in filePaths)
            {
                string fileName = Path.GetFileName(path);
                if (fileName.StartsWith("TraceData")) 
                {
                    if (!fileName.Contains("_VC_"))
                    {
                        if (m_debugDataFiles)  // for testing and trouble shooting
                            File.Move(path, m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME + "\\debug\\" + fileName);

                        File.Delete(path);
                    }
                }
                else if (fileName.StartsWith("VChannelData_Block"))
                {
                    File.Delete(path);
                }
            }
        }



        /// <summary>
        /// Get all the virtual channel file names for the given probe stream channel.
        /// </summary>
        /// <param name="vChannel"></param>
        /// <returns></returns>
        private List<string> getVCDataFileName(int vChannel)
        {
            List<string> vchannelFileNames = new List<string>();

            string[] filePaths = Directory.GetFiles(m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME);
            foreach (string path in filePaths)
            {
                string fileName = Path.GetFileName(path);
                if (fileName.StartsWith("TraceData"))
                {
                    if (fileName.Contains("_VC_"))
                    {
                        string[] comps = fileName.Split(new char[] { '_', '.' }); // e.g. TraceData_0_VC_1.bin
                        if (comps.Length >= 4)
                        {
                            if (vChannel == int.Parse(comps[3]))
                                vchannelFileNames.Add(path);
                        }
                    }
                }
                else if (fileName.StartsWith("VChannelData_Block"))
                {
                    File.Delete(path);
                }
            }

            return vchannelFileNames;
        }



        /// <summary>
        /// Write a state to a binary file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="pageData"></param>
        /// <param name="count"></param>
        private void writeBlockToBinaryFile(string path, byte[] stateData)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(stateData, 0, stateData.Length); // (int)blockSize * TB_STATE_BYTE_LEN);
                    using (FileStream file = new FileStream(path, FileMode.Append, FileAccess.Write))
                    {
                        ms.WriteTo(file);
                    }
                }
            }
        }


        /// <summary>
        /// convert the extracted VCTag bits 5:3 to a tag id.
        /// </summary>
        /// <param name="stateData"></param>
        /// <returns></returns>
        private byte getVCTagID(byte[] stateData)
        {
            byte vcTagID = 0;
            byte tagBits = (byte)((stateData[VC_TAG_BYTE_ID] & VC_TAG_FIELD_MASK));

            // bits[--xx x---]
            switch (tagBits)
            {
                case 0x08:
                    vcTagID = 0x01;
                    break;
                case 0x10:
                    vcTagID = 0x02;
                    break;
                case 0x18:
                    vcTagID = 0x03;
                    break;
                case 0x20:
                    vcTagID = 0x04;
                    break;
                case 0x21:
                    vcTagID = 0x05;
                    break;
                case 0x30:
                    vcTagID = 0x06;
                    break;
                case 0x31:
                    vcTagID = 0x07;
                    break;

                default:
                    break;
            }

            return vcTagID;
        }



        /// <summary>
        /// Segregate the virtual channel data into individual files.
        /// </summary>
        /// <param name="binaryFilePath"></param>
        /// <param name="blockIndex"></param>
        /// <param name="token"></param>
        /// <param name="fileGenerationStatusEvent"></param>
        private void createVirtualChannelFiles(string binaryFilePath, int blockIndex, CancellationToken token, VCFileGenerationStatusEvent fileGenerationStatusEvent, float progressPercentage)
        {
            using (FileStream fs = new FileStream(binaryFilePath, FileMode.Open))
            {
                int stateCount = 0;
                string vc1_FilePath = m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME + "\\TraceData_" + blockIndex.ToString() + "_VC_1.bin";
                string vc2_FilePath = m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME + "\\TraceData_" + blockIndex.ToString() + "_VC_2.bin";
                string vc3_FilePath = m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME + "\\TraceData_" + blockIndex.ToString() + "_VC_3.bin";
                string vc4_FilePath = m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME + "\\TraceData_" + blockIndex.ToString() + "_VC_4.bin";


                using (FileStream ms_VC1 = new FileStream(vc1_FilePath, FileMode.CreateNew, FileAccess.Write))
                {
                    using (FileStream ms_VC2 = new FileStream(vc2_FilePath, FileMode.CreateNew, FileAccess.Write))
                    {
                        using (FileStream ms_VC3 = new FileStream(vc3_FilePath, FileMode.CreateNew, FileAccess.Write))
                        {
                            using (FileStream ms_VC4 = new FileStream(vc4_FilePath, FileMode.CreateNew, FileAccess.Write))
                            {

                                byte[] stateData = new byte[STATE_BYTE_LENGTH];
                                //byte vcTagValue = 0x00;

                                // read the contents of the uploaded binary data file.
                                using (BinaryReader br = new BinaryReader(fs))
                                {
                                    bool done = false;
                                    byte bValue = 0x00;
                                    int newLineCount = 0x00;

                                    // read past the header...
                                    while (!done)
                                    {
                                        bValue = br.ReadByte();
                                        if (bValue == 0x0A)
                                            newLineCount += 1;

                                        if (newLineCount == 4)
                                            done = true;
                                    }


                                    // segregate the binary file data into virtual channel memory streams.
                                    while (br.BaseStream.Position <= br.BaseStream.Length - TB_STATE_BYTE_LEN)
                                    {
                                        //vcTagValue = 0x00;
                                        //Array.Clear(stateData, 0, stateData.Length);
                                        if (br.Read(stateData, 0, TB_STATE_BYTE_LEN) == TB_STATE_BYTE_LEN)
                                        {
                                            stateCount += 1;
                                            //vcTagValue = getVCTagID(stateData); //(byte)((stateData[VC_TAG_BYTE_ID] & VC_TAG_FIELD_MASK) >> VC_TAG_FIELD_SHIFT);
                                            //switch (vcTagValue)
                                            switch (getVCTagID(stateData))
                                            {
                                                case 1:
                                                    ms_VC1.Write(stateData, 0, stateData.Length);
                                                    break;
                                                case 2:
                                                    ms_VC2.Write(stateData, 0, stateData.Length);
                                                    break;
                                                case 3:
                                                    ms_VC3.Write(stateData, 0, stateData.Length);
                                                    break;
                                                case 4:
                                                    ms_VC4.Write(stateData, 0, stateData.Length);
                                                    break;

                                                default:
                                                    break;
                                            }

                                            if ((stateCount % 8192) == 0)
                                            {
                                                ////update the progress bar on the main form
                                                //if (VCFileGenStatusEvent != null)
                                                //    VCFileGenStatusEvent(this, new VCFileGenerationStatusEventArgs("VCGenerationInProgress", stateCount));
                                                stateCount = 0;
                                            }
                                        }
                                        else
                                        {
                                            break; // while  (br.BaseStream.Position...
                                        }
                                    }  // While (...
                                } // using (Binary Reader ...
                            } //using (FileStream ms_VC4
                        }//using (FileStream ms_VC3
                    }//using (FileStream ms_VC2
                }//using (FileStream ms_VC1
            } // using (FileStream fs ... 


            // issue an event to increment the progress bar
            if (fileGenerationStatusEvent != null)
                fileGenerationStatusEvent(this, new VCFileGenerationStatusEventArgs("Processing MST Phase (P1)...", progressPercentage));
        }


        ///// <summary>
        ///// Combine the files generated in each task column.  Each column produces four files, VC1-VC4.
        ///// </summary>
        //private void consolidateVCFiles(int numOfColumns, int numOfLevels, int numOfVirtualChannels)
        //{
        //    string blockVCFileNameBase = m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME + "\\" + BLOCK_VC_FILENAME_BASE;
        //    List<string> blockFilePaths = new List<string>();
        //    int condensedBlockID = 0;

        //    //for (int columnID = 0; columnID < numOfColumns; columnID++) // += numOfLevels)
        //    for (int columnID = 0; columnID < (numOfColumns * numOfLevels); columnID += numOfLevels)  
        //    {
        //        for (int vcID = 0; vcID < numOfVirtualChannels; vcID++)
        //        {
        //            string destFileName = blockVCFileNameBase + condensedBlockID.ToString() + "_VC_" + (vcID + 1).ToString() + ".bin";
        //            using (Stream destStream = File.OpenWrite(destFileName))
        //            {
        //                for (int levelID = 0; levelID < numOfLevels; levelID++)
        //                {
        //                    string path = m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME + "\\" + VC_FILE_PREFIX + "_" + 
        //                                                (columnID + levelID).ToString() +  "_VC" + (vcID + 1).ToString() + ".bin";
        //                    if (File.Exists(path))
        //                    {
        //                        using (Stream srcStream = File.OpenRead(path)) 
        //                        {
        //                            srcStream.CopyTo(destStream);
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        condensedBlockID += 1;
        //        blockFilePaths.Clear();
        //    }
        //}


        /// <summary>
        /// Get the timestamp of the trigger state.
        /// </summary>
        /// <returns></returns>
        private long getStateTimeStamp(byte[] stateData)
        {
            long triggerTimeStamp = -1;
            int timeStampMSBit_ByteID = 1;
            int timeStampMSBitID = 2;
            int timeStampFieldWidth = 50; // 50 bits wide

            //
            // TBD -- this is hardcoded for now... should work for all combinations as long as the loop is not changed!
            //
             triggerTimeStamp = LoopOperations.GetFieldValue(timeStampMSBit_ByteID, timeStampMSBitID, timeStampFieldWidth, stateData); 

            // get the meta data from the start of the file...
            // loop through the uploaded data files... TraceData_X.bin... get the length of each one.

            return triggerTimeStamp;
        }


        /// <summary>
        /// Get the timestamp of the trigger state.
        /// </summary>
        /// <returns></returns>
        private long getStateTimeStamp(BinaryReader br, long stateOffset, byte[] stateData)
        {
            long triggerTimeStamp = -1;
            int timeStampMSBit_ByteID = 1;
            int timeStampMSBitID = 2;
            int timeStampFieldWidth = 50; // 50 bits wide

            //
            // TBD -- this is hardcoded for now... should work for all combinations as long as the loop is not changed!
            //
            triggerTimeStamp = LoopOperations.GetFieldValue(timeStampMSBit_ByteID, timeStampMSBitID, timeStampFieldWidth, stateData);

            // get the meta data from the start of the file...
            // loop through the uploaded data files... TraceData_X.bin... get the length of each one.

            return triggerTimeStamp;
        }


        /// <summary>
        /// Find the state with the given time stamp value.
        /// </summary>
        /// <param name="triggerTimeStamp"></param>
        /// <returns></returns>
        /// Assumptions:  Works on segrated state data for a specified virtual channel
        ///               Files do not contain meta data (for which this method is being invoked)
        private int locateTriggerStateIndex(long triggerTimeStamp, int trigChannelID, VCFileGenerationStatusEvent fileGenerationStatusEvent)
        {
            long triggerIndex = 0x00;

            if (triggerTimeStamp > -1)  // there is an issue that needs to be tracked down where the trigger index is -1
            {
                long fileStartStateIndex = 0x00;

                // get the list of files containing the trigger channel data
                List<string> filePaths = getVCDataFileName(trigChannelID);
                float percentagePerFile = (1.0f / (float)(filePaths.Count)) * 100.0f;

                // loop through the list of files that could contain the trigger
                foreach (string path in filePaths)
                {
                    // get the first and last time stamps...
                    using (BinaryReader br = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read)))
                    {
                        long topTimeStamp = 0x00;
                        long bottomTimeStamp = 0x00;
                        long middleTimeStamp = 0x00;

                        byte[] topStateData = new byte[TB_STATE_BYTE_LEN];
                        byte[] bottomStateData = new byte[TB_STATE_BYTE_LEN];
                        byte[] middleStateData = new byte[TB_STATE_BYTE_LEN];

                        try
                        {
                            // read the 1st state in the file
                            br.BaseStream.Position = 0x00;
                            br.Read(topStateData, 0, TB_STATE_BYTE_LEN);

                            // read the 2nd state in the file
                            br.BaseStream.Position = br.BaseStream.Length - TB_STATE_BYTE_LEN;
                            br.Read(bottomStateData, 0, TB_STATE_BYTE_LEN);

                            // extract the time stamps from the two states
                            topTimeStamp = getStateTimeStamp(topStateData);
                            bottomTimeStamp = getStateTimeStamp(bottomStateData);

                            if (triggerTimeStamp == topTimeStamp)
                            {
                                triggerIndex = fileStartStateIndex;  // this is a work around... binary search algorithm is not finding a trigger in the very first state.
                            }
                            else
                            {
                                // determine if the trigger time stamp is contained in the current file
                                if ((triggerTimeStamp >= topTimeStamp) && (triggerTimeStamp <= bottomTimeStamp))
                                {
                                    int minIndex = 0;
                                    int maxIndex = (int)((br.BaseStream.Length - TB_STATE_BYTE_LEN) / TB_STATE_BYTE_LEN);
                                    int fileMaxIndex = maxIndex;
                                    int midIndex = -1;

                                    while (minIndex <= maxIndex)
                                    {
                                        midIndex = (minIndex + maxIndex) / 2;
                                        br.BaseStream.Position = midIndex * TB_STATE_BYTE_LEN;
                                        br.Read(middleStateData, 0, TB_STATE_BYTE_LEN);
                                        middleTimeStamp = getStateTimeStamp(middleStateData);

                                        if (triggerTimeStamp == middleTimeStamp)
                                        {
                                            triggerIndex = fileStartStateIndex + midIndex;
                                            break;//done = true;
                                        }
                                        else if (triggerTimeStamp < middleTimeStamp)
                                        {
                                            maxIndex = midIndex - 1;
                                        }
                                        else
                                        {
                                            minIndex = midIndex + 1;
                                        }
                                    }

                                    break;  // exit the foreach loop
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            triggerIndex = 0x00;
                        }

                        fileStartStateIndex += br.BaseStream.Length / TB_STATE_BYTE_LEN;
                    }

                    // issue an event to increment the progress bar
                    if (fileGenerationStatusEvent != null)
                        fileGenerationStatusEvent(this, new VCFileGenerationStatusEventArgs("Processing MST Phase (P2)...", percentagePerFile));
                }
            }
            else
            {
                triggerIndex = 0;
            }

            return (int)triggerIndex;
        }


        ///// <summary>
        ///// Extract the binary file's meta data (located at the beginning of the file).
        ///// </summary>
        ///// <param name="path"></param>
        ///// <param name="msg"></param>
        ///// <returns></returns>
        //private bool getuploadDataFileMetaData(string filePath, UploadMetaData metaData)
        //{
        //    bool status = true;
        //    int delimiterCount = 0;
        //    int parameterValue = -1;

        //    if (File.Exists(filePath))
        //    {
        //        //    // open the binary file and read all lines between the lines that begin with "*****"
        //        using (TextReader sr = File.OpenText(filePath))
        //        {
        //            string s = String.Empty;
        //            while (((s = sr.ReadLine()) != null) && (delimiterCount < 1) && (status == true))
        //            {
        //                if (s.StartsWith("EndHdr"))
        //                    delimiterCount += 1;

        //                // do it this way... will only process lines in-between the delimited lines.
        //                if (delimiterCount == 0)
        //                {
        //                    // get the line, parse is out
        //                    string[] comps = s.Split(new char[] { ':' });
        //                    if (int.TryParse(comps[1], out parameterValue))
        //                    {
        //                        if (comps[0] == UploadMetaData.BIN_FILE_HEADER_START_STATE)
        //                            metaData.StartStateIndex = parameterValue;
        //                        else if (comps[0] == UploadMetaData.BIN_FILE_HEADER_END_STATE)
        //                            metaData.EndStateIndex = parameterValue;
        //                        else if (comps[0] == UploadMetaData.BIN_FILE_HEADER_TRIG_STATE)
        //                            metaData.TrigStateIndex = parameterValue;
        //                        else
        //                            status = false;
        //                    }
        //                }
        //                metaData.DataOffset += s.Length + 1;
        //            }
        //        }
        //    }

        //    return status;
        //}


        /// <summary>
        /// Search for a given timestamp in the opened binary file.
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="br"></param>
        /// <param name="trigTimeStamp"></param>
        /// <returns></returns>
        private long searchTriggerStateIndex(FileStream fs, BinaryReader br, uint trigTimeStamp)
        {
            long index = -1;
            long topStateIndex = 0;
            long middleStateIndex = fs.Length / 2;
            long bottomStateIndex = fs.Length - TB_STATE_BYTE_LEN;
            uint topTimeStamp = 0;
            uint bottomTimeStamp = 0;

            // get the timestamp that is located half way through the file
            byte[] stateData = new byte[TB_STATE_BYTE_LEN];
            uint middleTimeStamp = 0;


            // calculate the state in the virtual channel at which the trigger is located.


            while (index == -1)
            {
                // get the time stamp at the top index
                fs.Position = topStateIndex;
                Array.Copy(br.ReadBytes(TB_STATE_BYTE_LEN), stateData, stateData.Length);
                topTimeStamp = SharedProject1.LoopOperations.GetFieldValueII(0x01, 0x2, 50, stateData);

                // get the time stame at the bottom index 
                fs.Position = middleStateIndex;
                Array.Copy(br.ReadBytes(TB_STATE_BYTE_LEN), stateData, stateData.Length);
                middleTimeStamp = SharedProject1.LoopOperations.GetFieldValueII(0x01, 0x2, 50, stateData);


                // get the time stame at the bottom index 
                fs.Position = bottomStateIndex;
                Array.Copy(br.ReadBytes(TB_STATE_BYTE_LEN), stateData, stateData.Length);
                bottomTimeStamp = SharedProject1.LoopOperations.GetFieldValueII(0x01, 0x2, 50, stateData);

                if (trigTimeStamp == topTimeStamp)
                {
                    index = topStateIndex;
                }
                else if (trigTimeStamp == middleTimeStamp)
                {
                    index = middleStateIndex;
                }
                else if (trigTimeStamp == bottomTimeStamp)
                {
                    index = bottomStateIndex;
                }
                else
                {
                    if (trigTimeStamp < middleTimeStamp)    // discard the bottom half
                    {
                        bottomStateIndex = middleStateIndex;
                    }
                    else                                    // discard the top half
                    {
                        topStateIndex = middleStateIndex;
                    }

                    // calculate the middle state index
                    middleStateIndex = (bottomStateIndex - topStateIndex) / 2;
                }

                if (topStateIndex == bottomStateIndex)
                    break; // exit the search while loop as we have failed!
            }

            return index;
        }


        /// <summary>
        /// Add the meta data to each consolidated column virtual channel file.
        /// </summary>
        /// <param name="metaDataList"></param>
        /// <returns></returns>
        private bool prependMetaData(int channelID, UploadMetaData[] metaDataList, VCFileGenerationStatusEvent fileGenerationStatusEvent, float progressPercentage)
        {
            bool status = true;
            string consolidatedDataFileName = string.Empty;
            int metaDataLength = 0;

            progressPercentage = (1.0f / (float)(metaDataList.Length)) * 100.0f;

            //for (int i = 0; i < metaDataList.Length; i += 4)  // loop for the number of columns
            for (int i = channelID; i < metaDataList.Length; i += 4)  // loop for the number of columns
            {
                //// each column should have four channels
                //for (int channelID = 1; channelID <= 4; channelID++)
                //{
                metaDataLength = 0;
                consolidatedDataFileName = m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME + "\\" + BLOCK_VC_FILENAME_BASE + (i / 4).ToString() + "_VC_" + (channelID + 1).ToString() + ".bin";
                string tempFile = m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME + "\\" + BLOCK_VC_FILENAME_BASE + channelID.ToString() + "_temp.bin";


                for (int x = 0; x < 5; x++)
                {
                    try
                    {
                        // rename the binary file
                        File.Move(consolidatedDataFileName, tempFile);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Thread.Sleep(1);
                    }
                }


                //using (BinaryWriter bw = new BinaryWriter(File.Open(tempFile, FileMode.Create, FileAccess.Write, FileShare.None)))
                using (BinaryWriter bw = new BinaryWriter(File.Open(consolidatedDataFileName, FileMode.Create, FileAccess.Write, FileShare.None)))
                {
                    //byte[] array = System.Text.Encoding.ASCII.GetBytes("StartState:" + metaDataList[i + (channelID - 1)].StartStateIndex.ToString() + "\n");
                    byte[] array = System.Text.Encoding.ASCII.GetBytes("StartState:" + metaDataList[i].StartStateIndex.ToString() + "\n");
                    bw.Write(array);
                    metaDataLength += array.Length;

                    //array = System.Text.Encoding.ASCII.GetBytes("EndState:" + metaDataList[i + (channelID + 1)].EndStateIndex.ToString() + "\n");
                    array = System.Text.Encoding.ASCII.GetBytes("EndState:" + metaDataList[i].EndStateIndex.ToString() + "\n");
                    bw.Write(array);
                    metaDataLength += array.Length;

                    //array = System.Text.Encoding.ASCII.GetBytes("TrigVChannelID:" + metaDataList[i + (channelID + 1)].TriggerVChannelID.ToString() + "\n");
                    array = System.Text.Encoding.ASCII.GetBytes("TrigVChannelID:" + metaDataList[i].TriggerVChannelID.ToString() + "\n");
                    bw.Write(array);
                    metaDataLength += array.Length;

                    //array = System.Text.Encoding.ASCII.GetBytes("TrigState:" + metaDataList[i + (channelID + 1)].TrigStateIndex.ToString() + "\n");
                    array = System.Text.Encoding.ASCII.GetBytes("TrigState:" + metaDataList[i].TrigStateIndex.ToString() + "\n");
                    bw.Write(array);
                    metaDataLength += array.Length;

                    //array = System.Text.Encoding.ASCII.GetBytes("vChannel1TrigState:" + metaDataList[i].VChannel1TrigStateIndex.ToString() + "\n");
                    array = System.Text.Encoding.ASCII.GetBytes("vChannel1TrigState:" + metaDataList[i].VChannel1TrigStateIndex.ToString() + "\n");
                    bw.Write(array);
                    metaDataLength += array.Length;

                    //array = System.Text.Encoding.ASCII.GetBytes("vChannel2TrigState:" + metaDataList[i + (channelID + 1)].VChannel2TrigStateIndex.ToString() + "\n");
                    array = System.Text.Encoding.ASCII.GetBytes("vChannel2TrigState:" + metaDataList[i].VChannel2TrigStateIndex.ToString() + "\n");
                    bw.Write(array);
                    metaDataLength += array.Length;

                    //array = System.Text.Encoding.ASCII.GetBytes("vChannel3TrigState:" + metaDataList[i + (channelID + 1)].VChannel3TrigStateIndex.ToString() + "\n");
                    array = System.Text.Encoding.ASCII.GetBytes("vChannel3TrigState:" + metaDataList[i].VChannel3TrigStateIndex.ToString() + "\n");
                    bw.Write(array);
                    metaDataLength += array.Length;

                    //array = System.Text.Encoding.ASCII.GetBytes("vChannel4TrigState:" + metaDataList[i + (channelID - +1)].VChannel4TrigStateIndex.ToString() + "\n");
                    array = System.Text.Encoding.ASCII.GetBytes("vChannel4TrigState:" + metaDataList[i].VChannel4TrigStateIndex.ToString() + "\n");
                    bw.Write(array);
                    metaDataLength += array.Length;

                    array = System.Text.Encoding.ASCII.GetBytes("EndHdr***");
                    bw.Write(array);
                    metaDataLength += array.Length;

                    string delimiterStr = "";
                    int padCount = metaDataLength;
                    while (((padCount) % 16) != 0)
                        padCount += 1;
                    padCount -= metaDataLength;

                    if (padCount > 2)
                        array = System.Text.Encoding.ASCII.GetBytes(delimiterStr.PadLeft(padCount - 1, '*'));
                    else
                        array = System.Text.Encoding.ASCII.GetBytes(delimiterStr.PadLeft(0, '*'));

                    bw.Write(array);
                    bw.Write('\n');
                    metaDataLength += array.Length + 1;


                    // update the meta data offset value to reflect what is contained in the binary file.
                    metaDataList[i].DataOffset = metaDataLength;

                    https://social.msdn.microsoft.com/Forums/vstudio/en-US/70050550-ba85-4ead-8854-e22a7746ca72/how-to-append-two-binary-files?forum=netfxbcl
                    //using (BinaryWriter writer = new BinaryWriter(File.OpenForWriting(< path >))
                    //{
                    //Skip to end
                    //consolidatedDataFileName = m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME + "\\" + BLOCK_VC_FILENAME_BASE + (i / 4).ToString() + "_VC_" + (channelID+1).ToString() + ".bin";

                    //bw.Seek(0, SeekOrigin.End);
                    using (BinaryReader reader = new BinaryReader(File.Open(tempFile, FileMode.Open, FileAccess.Read, FileShare.None))) //File.OpenForReading(consolidatedDataFileName))
                    {
                        byte[] buffer = new byte[1024];
                        while (reader.BaseStream.Position < reader.BaseStream.Length)
                        {
                            int count = reader.Read(buffer, 0, buffer.Length);
                            bw.Write(buffer, 0, count);
                        }
                    }
                }



                for (int x = 0; x < 5; x++)
                {
                    try
                    {
                        // remove the intermediate file.
                        File.Delete(tempFile);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Thread.Sleep(1);
                    }
                }


                // issue an event to increment the progress bar
                if (fileGenerationStatusEvent != null)
                    fileGenerationStatusEvent(this, new VCFileGenerationStatusEventArgs("Processing MST Phase (P5)...", progressPercentage));

                ////
                //// append the consolidated data onto the end of the temp file (that has the meta data header).
                ////

                //byte[] buffer = new byte[8 * (1024 * 1024)];
                //                using (BinaryReader br = new BinaryReader(File.Open(consolidatedDataFileName, FileMode.Open, FileAccess.Read)))
                //                {
                //                    while (br.BaseStream.Position < br.BaseStream.Length)
                //                    {
                //                        int count = br.Read(buffer, 0, buffer.Length);
                //                        bw.Write(buffer, 0, count);
                //                    };
                //                }
                //          //}
                //        }

                //        File.Copy(tempFile, consolidatedDataFileName, true);
            }

            return status;
        }


        /// <summary>
        /// Get the meta data for the VC-1 through VC-4 for all consolidated column files; typically 2 or 4 columns.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        /// Asssumptions:  Each cascading task column will have a list.
        ///                Each list (in the array) will have four elements, one for each virtual channel.
        ///                
        /// Assumption:    Meta data array is in order of VC1-VC4
        /// Assumption:    MetaDataList is pre-allocation before this method is invoked.
        /// 
        ///                    B0_VC1
        ///                    |    B0_VC2 
        ///                    |    |    B0_VC3                                        
        ///                    |    |    |    B0_VC4 
        ///                    |    |    |    |
        ///                    |    |    |    |      B1_VC1                               
        ///                    |    |    |    |      |    B1_VC2  
        ///                    |    |    |    |      |    |    B1_VC3 
        ///                    |    |    |    |      |    |    |    B1_VC4  
        ///                    |    |    |    |      |    |    |    |     
        ///                    |    |    |    |      |    |    |    |      B2_VC1
        ///                    |    |    |    |      |    |    |    |      |    B2_VC2
        ///                    |    |    |    |      |    |    |    |      |    |            
        ///                    |    |    |    |      |    |    |    |      |    |      ...   V15_VC4
        ///                    |    |    |    |      |    |    |    |      |    |            |
        /// metaDataArray[] = [0], [1], [2], [3],   [4], [5], [6], [7],   [8], [9],    ...  [63] 
        ///                    
        //private bool getConsolidatedFilesMetaData(int numOfColumns, int numOfLevels, int numOfChannels, UploadMetaData[] metaDataArray, int triggerVChannelID, int triggerIndex)
        private UploadMetaData[]  getConsolidatedFilesMetaData(int numOfColumns, int numOfLevels, int numOfChannels, int triggerVChannelID, int triggerIndex, VCFileGenerationStatusEvent fileGenerationStatusEvent, float progressPercentage)
        {
            //bool status = true;
            int numOfFiles = numOfColumns * numOfLevels * numOfChannels;
            int numOfBlocks = numOfColumns * numOfLevels;
            string consolidatedDataFileName = string.Empty;
            int startIndex = 0;
            int blockIndex = 0;
            UploadMetaData[] metaDataArray = new UploadMetaData[m_numOfColumns * m_numOfLevels * m_numOfChannels];


            // data have segreated such that each uploaded blocks have been divided into four sub-blocks, one for each virtual channel...
            //   TraceData_0_VC_1.bin
            //   TraceData_0_VC_2.bin
            //   TraceData_0_VC_3.bin
            //   TraceData_0_VC_4.bin
            //
            //   TraceData_1_VC_1.bin
            //   TraceData_1_VC_2.bin
            //   TraceData_1_VC_3.bin
            //   TraceData_01_VC_4.bin
            //
            // etc...

            // this method loop through all blocks processing channel 1 blocks in sequencial order and then 
            // repeats for channels 2, 3, and 4.
            //

            progressPercentage = (1.0f / (float)(numOfChannels * numOfBlocks)) * 100.0f;
            for (int channelID = 1; channelID <= numOfChannels; channelID++)
            {
                startIndex = 0;
                blockIndex += (channelID - 1);
                for (int blockID = 0; blockID < numOfBlocks; blockID++)
                {
                    consolidatedDataFileName = m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME + "\\" + BLOCK_VC_FILENAME_BASE + blockID.ToString() + "_VC_" + channelID.ToString() + ".bin";
                    using (FileStream fs = new FileStream(consolidatedDataFileName, FileMode.Open, FileAccess.Read))
                    {
                        if (fs.Length == 0)
                        {
                            UploadMetaData metaData = new UploadMetaData(0, 0);
                            metaDataArray[blockIndex] = new UploadMetaData(blockID, channelID, 0, 0, triggerVChannelID, triggerIndex);
                        }
                        else
                        {
                            metaDataArray[blockIndex] = new UploadMetaData(blockID, channelID, startIndex, startIndex + (int)((fs.Length / TB_STATE_BYTE_LEN)-1), triggerVChannelID, triggerIndex);
                            startIndex += (int)(fs.Length / TB_STATE_BYTE_LEN);
                        }
                    }

                    blockIndex += 4;

                    // issue an event to increment the progress bar
                    if (fileGenerationStatusEvent != null)
                        fileGenerationStatusEvent(this, new VCFileGenerationStatusEventArgs("Processing MST Phase (P3)...", progressPercentage));
                }

                blockIndex = 0;
            }

            return metaDataArray; // status;
        }


        /// <summary>
        /// Initialize vChannel Index
        /// </summary>
        /// <param name="triggerVChannelID"></param>
        /// <param name="triggerIndex"></param>
        /// <param name="vChannel1TriggerIndex"></param>
        /// <param name="vChannel2TriggerIndex"></param>
        /// <param name="vChannel3TriggerIndex"></param>
        /// <param name="vChannel4TriggerIndex"></param>
        private void  initTriggerChannelIndex(int triggerVChannelID, int triggerIndex, ref int vChannel1TriggerIndex, ref int vChannel2TriggerIndex, ref int vChannel3TriggerIndex, ref int vChannel4TriggerIndex)
        {
            switch (triggerVChannelID)
            {
                case 1:
                    vChannel1TriggerIndex = triggerIndex;  
                    break;
                case 2:
                    vChannel2TriggerIndex = triggerIndex;
                    break;
                case 3:
                    vChannel3TriggerIndex = triggerIndex;
                    break;
                case 4:
                    vChannel4TriggerIndex = triggerIndex;
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// Init current channel trigger index
        /// </summary>
        /// <param name="channelID"></param>
        /// <param name="vChannel1TriggerIndex"></param>
        /// <param name="vChannel2TriggerIndex"></param>
        /// <param name="vChannel3TriggerIndex"></param>
        /// <param name="vChannel4TriggerIndex"></param>
        /// <returns></returns>
        private int getCurrentTriggerChannelIndex(int channelID, int vChannel1TriggerIndex, int vChannel2TriggerIndex, int vChannel3TriggerIndex, int vChannel4TriggerIndex)
        {
            int currentChannelTriggerIndex = -1;

            //
            // reset the current trigger index variable.
            //
            switch (channelID)
            {
                case 1:
                    currentChannelTriggerIndex = vChannel1TriggerIndex;
                    break;
                case 2:
                    currentChannelTriggerIndex = vChannel2TriggerIndex;
                    break;
                case 3:
                    currentChannelTriggerIndex = vChannel3TriggerIndex;
                    break;
                case 4:
                    currentChannelTriggerIndex = vChannel4TriggerIndex;
                    break;

                default:
                    break;
            }

            return currentChannelTriggerIndex;
        }


        /// <summary>
        /// Init current channel trigger index
        /// </summary>
        /// <param name="channelID"></param>
        /// <param name="vChannel1TriggerIndex"></param>
        /// <param name="vChannel2TriggerIndex"></param>
        /// <param name="vChannel3TriggerIndex"></param>
        /// <param name="vChannel4TriggerIndex"></param>
        /// <returns></returns>
        private void setChannelTriggerIndex(int channelID, int index, ref int vChannel1TriggerIndex, ref int vChannel2TriggerIndex, ref int vChannel3TriggerIndex, ref int vChannel4TriggerIndex)
        {
            //
            // reset the current trigger index variable.
            //
            switch (channelID)
            {
                case 1:
                    vChannel1TriggerIndex = index;
                    break;
                case 2:
                    vChannel2TriggerIndex = index;
                    break;
                case 3:
                    vChannel3TriggerIndex = index;
                    break;
                case 4:
                    vChannel4TriggerIndex = index;
                    break;

                default:
                    break;
            }
        }



        /// <summary>
        /// set current channel previous state index
        /// </summary>
        /// <param name="channelID"></param>
        /// <param name="vChannel1TriggerIndex"></param>
        /// <param name="vChannel2TriggerIndex"></param>
        /// <param name="vChannel3TriggerIndex"></param>
        /// <param name="vChannel4TriggerIndex"></param>
        /// <returns></returns>
        private void setChannelPreviousIndex(int channelID, int index, ref int vChannel1PrevIndex, ref int vChannel2PrevIndex, ref int vChannel3PrevIndex, ref int vChannel4PrevIndex)
        {
            //
            // reset the current trigger index variable.
            //
            switch (channelID)
            {
                case 1:
                    vChannel1PrevIndex = index;
                    break;
                case 2:
                    vChannel2PrevIndex = index;
                    break;
                case 3:
                    vChannel3PrevIndex = index;
                    break;
                case 4:
                    vChannel4PrevIndex = index;
                    break;

                default:
                    break;
            }
        }


        /// <summary>
        /// get current channel previous state index
        /// </summary>
        /// <param name="channelID"></param>
        /// <param name="vChannel1TriggerIndex"></param>
        /// <param name="vChannel2TriggerIndex"></param>
        /// <param name="vChannel3TriggerIndex"></param>
        /// <param name="vChannel4TriggerIndex"></param>
        /// <returns></returns>
        private int getChannelPreviousIndex(int channelID, int vChannel1PrevIndex, int vChannel2PrevIndex, int vChannel3PrevIndex, int vChannel4PrevIndex)
        {
            int index = 0;

            //
            // reset the current trigger index variable.
            //
            switch (channelID)
            {
                case 1:
                    index = vChannel1PrevIndex;
                    break;
                case 2:
                    index = vChannel2PrevIndex;
                    break;
                case 3:
                    index = vChannel3PrevIndex;
                    break;
                case 4:
                    index = vChannel4PrevIndex;
                    break;

                default:
                    break;
            }

            return index;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="br"></param>
        /// <param name="stateIndex"></param>
        /// <returns></returns>
        ///    br.BaseStream.Position = topIndex * TB_STATE_BYTE_LEN;
        ///    bCount = br.Read(topStateData, 0, TB_STATE_BYTE_LEN);
        ///    topTS = SharedProject1.LoopOperations.GetFieldValueII(0x01, 0x2, 50, topStateData);
        ///    
        ///    br.BaseStream.Position = (int)Math.Ceiling((float)(bottomIndex - topIndex) / 2) * TB_STATE_BYTE_LEN;
        ///    bCount = br.Read(middleStateData, 0, TB_STATE_BYTE_LEN);
        ///    middleTS = SharedProject1.LoopOperations.GetFieldValueII(0x01, 0x2, 50, middleStateData);
        ///    
        ///    br.BaseStream.Position = (bottomIndex - 1) * TB_STATE_BYTE_LEN; 
        ///    bCount = br.Read(bottomStateData, 0, TB_STATE_BYTE_LEN);
        ///    bottomTS = SharedProject1.LoopOperations.GetFieldValueII(0x01, 0x2, 50, bottomStateData);
        ///
        private long getStateTimeStamp(BinaryReader br, int stateIndex)
        {
            byte[] stateData = new byte[TB_STATE_BYTE_LEN];
            long stateTS = 0;
            int bCount = -1;

            try
            {
                br.BaseStream.Position = stateIndex * TB_STATE_BYTE_LEN;
                bCount = br.Read(stateData, 0, TB_STATE_BYTE_LEN);
                if (bCount == TB_STATE_BYTE_LEN)
                    stateTS = SharedProject1.LoopOperations.GetFieldValueII(0x01, 0x2, 50, stateData);
                else
                    stateTS = -1;
            }
            catch (Exception ex)
            {
                stateTS = -1; // typically, stateIndex is less than 0 or beyond the end of the file.
            }

            return stateTS;
        }


        /// <summary>
        /// Get the state index of the timestamp closest to the trigger state using a binary search methodology.
        /// </summary>
        /// <param name="metaDataList"></param>
        /// <param name="triggerVChannelID"></param>
        /// <param name="triggerIndex"></param>
        /// <returns></returns>
        /// Assumptions:  The TPI Task Columns have been consolidated before the invokation of this method.
        ///               The data file does not contain any meta data (ascii text) info.
        ///               The search will never find an exact timestamp match (only the channel with the trigger will match)
        private UploadMetaData[] setMetaDataTriggerStateIndices(UploadMetaData[] metaDataList, long triggerTimeStamp, VCFileGenerationStatusEvent fileGenerationStatusEvent, float progressPercentage)
        {
            string consolidatedDataFileName = string.Empty;
            int vChannel1TriggerIndex = -1;
            int vChannel2TriggerIndex = -1;
            int vChannel3TriggerIndex = -1;
            int vChannel4TriggerIndex = -1;
            bool done = false;
            int numOfStates = 0;
            int topIndex = 0;
            int middleIndex = 0;
            int bottomIndex = 0;
            long topTS = 0;
            long middleTS = 0;
            long bottomTS = 0;
            int bCount = -1;
            int prevIndex = -1;

            progressPercentage = (1.0f / (float)(metaDataList.Length)) * 100.0f;
            for (int channelID = 1; channelID <= 4; channelID++)        // each column should have four channels
            {
                prevIndex = 0;
                for (int i = 0; i < metaDataList.Length / 4; i++)       // 1 Millon states has 16 blocks, each with 4 channels ==> (16 * 4) = 64 files need to be processed
                {
                    done = false;
                    consolidatedDataFileName = m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME + "\\" + BLOCK_VC_FILENAME_BASE + i.ToString() + "_VC_" + channelID.ToString() + ".bin";
                    using (BinaryReader br = new BinaryReader(File.Open(consolidatedDataFileName, FileMode.Open, FileAccess.Read)))
                    {
                        if (br.BaseStream.Length > 0)
                        {
                            numOfStates = (int)(br.BaseStream.Length / TB_STATE_BYTE_LEN);
                            if (numOfStates == 1)
                            {
                                done = true;  // exit, leave the channel index to be -1... for now
                            }
                            else
                            {
                                topIndex = 0;
                                bottomIndex = numOfStates - 1;
                                middleIndex = (int)Math.Ceiling((float)((bottomIndex - topIndex) / 2)) - 1;
                                topTS = 0;
                                middleTS = 0;
                                bottomTS = 0;
                                bCount = -1;
                                done = false;

                                while (!done)
                                {
                                    topTS = getStateTimeStamp(br, topIndex);
                                    middleTS = getStateTimeStamp(br, middleIndex);
                                    bottomTS = getStateTimeStamp(br, bottomIndex);

                                    if (bottomTS < triggerTimeStamp)        // file has only data BEFORE the trigger.
                                    {
                                        prevIndex += numOfStates;
                                        break;  // exit the while loop
                                    }
                                    else                                    // file contains the trigger state.
                                    {
                                        if (topTS == triggerTimeStamp)
                                        {
                                            setChannelTriggerIndex(channelID, topIndex, ref vChannel1TriggerIndex, ref vChannel2TriggerIndex, ref vChannel3TriggerIndex, ref vChannel4TriggerIndex);
                                            done = true;
                                            break;  // exit the 'while' loop
                                        }
                                        else
                                        {
                                            if (middleTS <= triggerTimeStamp)
                                                topIndex = middleIndex;
                                            else
                                                bottomIndex = middleIndex;
                                            middleIndex = topIndex + (int)Math.Ceiling((float)((bottomIndex - topIndex) / 2)) - 1;
                                        }

                                        //
                                        // do a sequenical search of the timestamp match if the delta between the two indices is less than 64
                                        if ((bottomIndex - topIndex) <= NUMBER_OF_TIME_SLOTS)  // 
                                        {
                                            byte[] topStateData = new byte[TB_STATE_BYTE_LEN];
                                            for (int stateIndex = topIndex; stateIndex <= bottomIndex; stateIndex++)
                                            {
                                                br.BaseStream.Position = stateIndex * TB_STATE_BYTE_LEN;
                                                bCount = br.Read(topStateData, 0, TB_STATE_BYTE_LEN);
                                                topTS = SharedProject1.LoopOperations.GetFieldValueII(0x01, 0x2, 50, topStateData);
                                                if (topTS >= triggerTimeStamp)
                                                {
                                                    setChannelTriggerIndex(channelID, prevIndex + stateIndex, ref vChannel1TriggerIndex, ref vChannel2TriggerIndex, ref vChannel3TriggerIndex, ref vChannel4TriggerIndex);
                                                    done = true;
                                                    break;      // exit the 'for' loop and onto the next virtual channel
                                                }
                                            }

                                            if (done == true)
                                                break;          // exit the 'while' loop; this channel is done being processed.
                                        }
                                    }
                                }  // while (!done)
                            }  // number of states > 1
                        }
                        else  // if (br.BaseStream.Length > 0)
                        {
                            // there are no states in the file... 
                            // assumption is there can be no gaps... 
                            // thus no more states in this channel should follow  -- channel increase will remain as -1.
                            done = true; 
                        }
                    }  // using (BinaryReader...

                    if (done == true)
                    {
                        break;  // exit the inner' for loop
                    }
                    else
                    {
                        // issue an event to increment the progress bar
                        if (fileGenerationStatusEvent != null)
                            fileGenerationStatusEvent(this, new VCFileGenerationStatusEventArgs("Processing MST Phase (P4)...", progressPercentage));
                    }
                }  // for (i=0...
            } // for (channel...


            //
            // update the meta data 2D array to have the channel trigger state indices.
            for (int i = 0; i < metaDataList.Length; i++)
            {
                ((UploadMetaData)(metaDataList[i])).VChannel1TrigStateIndex = vChannel1TriggerIndex;
                ((UploadMetaData)(metaDataList[i])).VChannel2TrigStateIndex = vChannel2TriggerIndex;
                ((UploadMetaData)(metaDataList[i])).VChannel3TrigStateIndex = vChannel3TriggerIndex;
                ((UploadMetaData)(metaDataList[i])).VChannel4TrigStateIndex = vChannel4TriggerIndex;
            }

            return metaDataList;
        }


        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="fileNames"></param>
        ///// <param name="fileNameIndex"></param>
        ///// <param name="fileStartIndex"></param>
        ///// <param name="vcTriggerIndex"></param>
        ///// <param name="triggerIndex"></param>
        ///// <param name="triggerTimeStamp"></param>
        ///// <param name="EOFEncountered"></param>
        ///// <returns></returns>
        ///// Assumptions: fileStartIndex is the state index of the first state contained in the file.
        /////              vcTriggerIndex is the state index for which we will starting looling for a state with the trigger bit set to 1.
        //private bool locateTriggerState(string[] fileNames, int fileNameIndex, long fileStartIndex, long startSearchStateIndex, int trigVChannelID, ref int triggerIndex, ref long triggerTimeStamp)
        //{
        //    bool located = false;
        //    int count = 0;
        //    byte[] stateData = new byte[TB_STATE_BYTE_LEN];
        //    uint triggeredFieldValue = 0x00;
        //    long curIndex = startSearchStateIndex - fileStartIndex;
        //    string consolidatedDataFileName = string.Empty;

        //    for (int fileIndex = fileNameIndex; !located && (fileIndex < fileNames.Length); fileIndex++)
        //    {
        //        consolidatedDataFileName = m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME + "\\" + BLOCK_VC_FILENAME_BASE + fileIndex.ToString() + "_VC_" + trigVChannelID.ToString() + ".bin";
        //        using (FileStream fs = new FileStream(consolidatedDataFileName, FileMode.Open, FileAccess.Read))
        //        {
        //            using (BinaryReader br = new BinaryReader(fs, new ASCIIEncoding()))
        //            {
        //                fs.Position = curIndex * TB_STATE_BYTE_LEN;
        //                if (fs.Position <= fs.Length - TB_STATE_BYTE_LEN)
        //                {
        //                    // search forward in the file (until found or EOF encountered)
        //                    for (int i = 0; i < NUMBER_OF_TIME_SLOTS * 2; i++)
        //                    {
        //                        // get the state data
        //                        Array.Copy(br.ReadBytes(TB_STATE_BYTE_LEN), stateData, stateData.Length);
        //                        triggeredFieldValue = SharedProject1.LoopOperations.GetFieldValueII(0x01, 0x3, 1, stateData);
        //                        if (triggeredFieldValue == 0x01)
        //                        {
        //                            triggerIndex = (int)(startSearchStateIndex + count) - 1;
        //                            triggerTimeStamp = SharedProject1.LoopOperations.GetFieldValueII(0x01, 0x2, 50, stateData);
        //                            located = true;
        //                            break;  // exit the loop iterating through the number of time slots
        //                        }
        //                        else
        //                        {
        //                            count += 1;

        //                            // we've gone through to the end of the current file... 
        //                            if (fs.Position > (fs.Length - TB_STATE_BYTE_LEN))
        //                            {
        //                                curIndex = 0;  // the start of the next file.
        //                                break; // // exit the loop iterating through the number of time slots
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    break;
        //                }
        //            }  // using binary reader
        //        } // using file stream
        //    }

        //    return located;
        //}


        ///// <summary>
        ///// Generate a list of file names for a specific virtual channel
        ///// </summary>
        ///// <param name="numOfColumns"></param>
        ///// <param name="trigVChannelID"></param>
        ///// <returns></returns>
        //private string[] getConsolidatedFileNames(int numOfColumns, int trigVChannelID)
        //{
        //    string[] fileNames = new string[numOfColumns];

        //    for (int i = 0; i < numOfColumns; i++)
        //        fileNames[i] =  m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME + "\\" + BLOCK_VC_FILENAME_BASE + i.ToString() + "_VC_" + trigVChannelID.ToString() + ".bin";

        //    return fileNames;
        //}


        ///// <summary>
        ///// Locate the trigger's consolated data file's name (TraceData_ColumnID_VC_vChannelID.bin file, e.g. TraceData_0_VC_1.bin).
        ///// </summary>
        ///// <param name="trigVChannelID"></param>
        ///// <param name="timeStamp"></param>
        ///// <returns></returns>
        //private bool getTriggerConsolidatedFileID(int numOfDataFiles, int numOfColumns, 
        //                                             int trigVChannelID, int numOfChannelSlots, ref string fileName, 
        //                                             ref int triggerIndex, ref long triggerTimeStamp)
        //{
        //    bool status = false;
        //    bool triggerLocated = false;
        //    string consolidatedDataFileName = string.Empty;
        //    string uploadedDataFilePath = m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME + "\\" + BLOCK_VC_FILENAME_BASE + "0.bin";
        //    UploadMetaData uploadedDataFileMetaData = new UploadMetaData();

        //    fileName = string.Empty;
        //    triggerIndex = -1;
        //    triggerTimeStamp = -1;

        //    // get the trigger state index (of the original uploaded data file).
        //    status = getuploadDataFileMetaData(uploadedDataFilePath, uploadedDataFileMetaData);
        //    if (status)
        //    {
        //        long vcTriggerIndex = (long)Math.Ceiling(((float)(uploadedDataFileMetaData.TrigStateIndex / NUMBER_OF_TIME_SLOTS) * (float)numOfChannelSlots));
        //        if (vcTriggerIndex > NUMBER_OF_TIME_SLOTS)
        //            vcTriggerIndex -= NUMBER_OF_TIME_SLOTS;

        //        string[] fileNames = getConsolidatedFileNames(numOfColumns, trigVChannelID);
        //        long fileStartIndex = -1;
        //        long fileEndIndex = 0;
        //        for (int x = 0; x < fileNames.Length; x++)
        //        {
        //            FileInfo fInfo = new FileInfo(fileNames[x]);
        //            fileStartIndex = fileEndIndex + 1;
        //            fileEndIndex = fileStartIndex + (fInfo.Length / TB_STATE_BYTE_LEN) - 1;

        //            if ((fileStartIndex <= vcTriggerIndex) && ((fileEndIndex >= vcTriggerIndex)))
        //                triggerLocated = locateTriggerState(fileNames, x, fileStartIndex, vcTriggerIndex, trigVChannelID, ref triggerIndex, ref triggerTimeStamp);

        //            if (triggerLocated == false)
        //            {
        //                triggerIndex = 0x00;
        //                triggerTimeStamp = 0x00;
        //                triggerLocated = true;
        //            }
        //        }
        //    }

        //    return triggerLocated; 
        //}


        /// <summary>
        /// Extract the VCTag field from the state data array.
        /// </summary>
        /// <param name="triggerStateData"></param>
        /// <returns></returns>
        private int getVChannelFields(ref byte[] triggerStateData)
        {
            int VChannelID = -1;

            if (triggerStateData.Length == TB_STATE_BYTE_LEN)
            {
                VChannelID = (triggerStateData[VCTAGS_BTYE_ID] & VCTAGS_FIELD_MASK) >> VCTAGS_FIELD_SHIFT;
            }

            return VChannelID;
        }


        ///// <summary>
        ///// Get the state data from the uploaded state data capture file
        ///// </summary>
        ///// <param name="numOfDataFiles"></param>
        ///// <returns></returns>
        //private byte[] getTriggerStateData(int numOfDataFiles)
        //{
        //    byte[] triggerStateData = new byte[TB_STATE_BYTE_LEN];
        //    UploadMetaData metaData = new UploadMetaData();

        //    //
        //    // read the meta data of the first files TraceData_0.bin... 
        //    // that will tell us trigger state state number... thus we
        //    // can avoid reading every state of every file until we get
        //    // to the trigger state...
        //    // 
        //    bool status = getuploadDataFileMetaData(m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME + "\\" + BLOCK_0_UPLOAD_DATA_FILENAME, metaData);
        //    if (status)
        //    {
        //        //
        //        // locate the file containing the trigger state.  -- I could do a calculation to determine which file contains
        //        //                                                   the trigger, but that assumes the header is the same length
        //        //                                                   and/or each uploaded data file is the same length... Two
        //        //                                                   assumptions I am not willing to make... so we'll do it the hard 
        //        //                                                   way of of opening each file, extracting the meta data and comparing 
        //        //                                                   the trigger offset to the start and end state indices of the file.
        //        //
        //        // List<string> uploadDataFilePaths = getUploadDataFilePaths(m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME);
        //        for (int i = 0; i < numOfDataFiles; i++)
        //        {
        //            UploadMetaData dataFileMetaData = new UploadMetaData();
        //            string currentPath = m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME + "\\" + BLOCK_VC_FILENAME_BASE + i.ToString() + ".bin";

        //            getuploadDataFileMetaData(currentPath, dataFileMetaData);
        //            if ((dataFileMetaData.StartStateIndex <= metaData.TrigStateIndex) && (dataFileMetaData.EndStateIndex >= metaData.TrigStateIndex))
        //            {
        //                int stateIndex = metaData.TrigStateIndex - dataFileMetaData.StartStateIndex;
        //                using (FileStream fs = new FileStream(currentPath, FileMode.Open, FileAccess.Read))
        //                {
        //                    if (fs.Length > (metaData.DataOffset + (stateIndex * TB_STATE_BYTE_LEN)))
        //                    {
        //                        using (BinaryReader br = new BinaryReader(fs, new ASCIIEncoding()))
        //                        {
        //                            fs.Position = dataFileMetaData.DataOffset + (stateIndex * TB_STATE_BYTE_LEN);
        //                            Array.Copy(br.ReadBytes(TB_STATE_BYTE_LEN), triggerStateData, triggerStateData.Length);
        //                        }
        //                    }

        //                    break; // exit the for loop as we have the trigger information
        //                }
        //            }
        //        }
        //    }

        //    return triggerStateData;
        //}


        ///// <summary>
        ///// Insert the Meta data into the Virtual Channel binary files
        ///// </summary>
        //// private bool insertVCMetaData(int numOfDataFiles, int numOfColumns, int triggerChannelID, int triggerStateIndex, long triggerTimeStamp)
        //private bool insertVCMetaData(int numOfColumns, int numOfLevels, int numOfChannels, int triggerChannelID, int triggerStateIndex, long triggerTimeStamp)
        //{
        //    bool status = false;
        //    int numOfDataFiles = numOfColumns * numOfLevels * numOfChannels;

        //    //  Allocate an array capable of holding the meta data for all blocks of segreated data...
        //    //  all blocks means the original uploaded blocks multiple by the number of virtual channels.
        //    //  blocks will be filled in in sequencial order of BlockID + VirtualChannelID.
        //    UploadMetaData[] metaDataArray = new UploadMetaData[numOfColumns * numOfLevels * numOfChannels];
        //    //VCFileGenStatusEvent(this, new VCFileGenerationStatusEventArgs("Processing MST Data (P1)", 0));

        //    //assemble the meta data that will be inserted into all the TraceData_1_VC_1.bin files
        //    status = getConsolidatedFilesMetaData(numOfColumns, numOfLevels, numOfChannels, metaDataArray, triggerChannelID, triggerStateIndex);
        //    if (status)
        //    {
        //        //VCFileGenStatusEvent(this, new VCFileGenerationStatusEventArgs("Processing MST Data (P2)", 0));
        //        status = setMetaDataTriggerStateIndices(metaDataArray, triggerChannelID, triggerStateIndex, triggerTimeStamp);
        //        if (status)
        //        {
        //            // prepend the meta data info to all the TraceData_1_VC_1.bin files
        //            //VCFileGenStatusEvent(this, new VCFileGenerationStatusEventArgs("Processing MST Data (P3)", 0));
        //            status = prependMetaData(metaDataArray);
        //        }
        //    }

        //    return status;
        //}


        /// <summary>
        /// Create the Virtual Channel files form the set of data files uploaded 
        /// from the HW using a cascading task methodology.
        /// 
        /// </summary>
        /// 2d-array explanation: https://msdn.microsoft.com/en-us/library/2s05feca.aspx
        /// int[][] jaggedArray = new int[3][];
        /// 
        /// jaggedArray[0] = new int[5];
        /// jaggedArray[1] = new int[4];
        /// jaggedArray[2] = new int[2];
        /// 
        /// jaggedArray[0] = new int[] { 1, 3, 5, 7, 9 };
        /// jaggedArray[1] = new int[] { 0, 2, 4, 6 };
        /// jaggedArray[2] = new int[] { 11, 22 };
        /// 
        /// What we want...
        /// 
        /// tasks[]              = new Task[] { column0, column1};     or   new Task[] { column0, column1, column2, column3 }; 
        /// ContinuationTasks[0] = new Task[] { column0, column1 };    or   new Task[] { column0, column1, column2, column3 }; 
        /// ContinuationTasks[1] = new Task[] { column0, column1 };    or   new Task[] { column0, column1, column2, column3 }; 
        /// ContinuationTasks[2] = new Task[] { column0, column1 };    or   new Task[] { column0, column1, column2, column3 }; 
        /// 
        /// <param name="numOfColumns"></param>
        /// <param name="numOfLevels"></param>
        /// <returns></returns>
        private async void segregateDataFile(int numOfColumns, int numOfLevels, long triggerTimeStamp, int trigChannelID)
        {
            try
            {
                string folderPath = m_FS4500_FOLDER_PATH + "\\" + m_FS4500_FOLDER_NAME + "\\TraceData_";
                ThreadLocal<DP12MST_TaskStateObject> tls = new ThreadLocal<DP12MST_TaskStateObject>();
                Task<DP12MST_TaskStateObject>[] tasks = new Task<DP12MST_TaskStateObject>[numOfColumns];
                Task<DP12MST_TaskStateObject>[][] continuationTasks = new Task<DP12MST_TaskStateObject>[numOfLevels - 1][];

                for (int i = 0; i < numOfLevels - 1; i++)
                    continuationTasks[i] = new Task<DP12MST_TaskStateObject>[numOfColumns];

                Task<DP12MST_TaskStateObject> parentTask;

                //
                // Tell whoever cares that we are post processing the uploaded data
                //
                if (VCFileGenStatusEvent != null)
                    VCFileGenStatusEvent(this, new VCFileGenerationStatusEventArgs("Processing MST Data...", 0));

                float percentagePerFile = (1.0f / (float)(numOfColumns * numOfLevels)) * 100.0f;

                // create the cancellatin token
                m_tokenSource = new CancellationTokenSource();
                m_token = m_tokenSource.Token;

                // numOfColumns is the number of tasks running concurrently...
                for (int column = 0; column < numOfColumns; column++)
                {
                    //
                    // setup one of the root task, from which other tasks will be cascaded from.
                    //
                    tasks[column] = new Task<DP12MST_TaskStateObject>((stateObject) => {
                        tls.Value = (DP12MST_TaskStateObject)stateObject;
                        ((DP12MST_TaskStateObject)tls.Value).Token.ThrowIfCancellationRequested();
                        createVirtualChannelFiles(tls.Value.FolderPath + tls.Value.Index.ToString() + ".bin", tls.Value.Index, tls.Value.Token, tls.Value.FileGenerationStatusEvent, tls.Value.ProgressBarPercentage);

                        return tls.Value;
                    }, new DP12MST_TaskStateObject(folderPath, VCFileGenStatusEvent, percentagePerFile, column * numOfLevels, m_token), m_token);


                    //
                    // retain the top level task  -- cascading from top task to the continuation levels
                    //
                    parentTask = tasks[column];


                    //
                    // setup the cascading tasks -- level is used as an array index, thus it is zero based.
                    //
                    for (int level = 0; level < (numOfLevels - 1); level++)
                    {
                        continuationTasks[level][column] = parentTask.ContinueWith(antecedent => {
                            tls.Value = (DP12MST_TaskStateObject)antecedent.Result;
                            ((DP12MST_TaskStateObject)tls.Value).Token.ThrowIfCancellationRequested();
                            tls.Value.Index = tls.Value.Index + 1;
                            createVirtualChannelFiles(tls.Value.FolderPath + tls.Value.Index.ToString() + ".bin", tls.Value.Index, tls.Value.Token, tls.Value.FileGenerationStatusEvent, tls.Value.ProgressBarPercentage);

                            return new DP12MST_TaskStateObject(tls.Value.FolderPath, tls.Value.FileGenerationStatusEvent, percentagePerFile, tls.Value.Index, tls.Value.Token);
                        }, m_token);

                        parentTask = continuationTasks[level][column];
                    }
                }

                try
                {
                    Task[] waitTasks = new Task[numOfColumns];
                    for (int i = 0; i < numOfColumns; i++)
                        waitTasks[i] = continuationTasks[numOfLevels - 2][i];

                    // Start all tasks doing the VC File Creation(s)
                    foreach (Task t in tasks)
                        t.Start();

                    // await keeps the GUI thread active while we are waiting..
                    await Task.WhenAll(waitTasks);

                    foreach (Task T in tasks)
                    {
                        T.Dispose();
                    }

                    for (int outterIndex = 0; outterIndex < numOfLevels - 1; outterIndex++)
                    {
                        for (int innerIndex = 0; innerIndex < numOfColumns; innerIndex++)
                            continuationTasks[outterIndex][innerIndex].Dispose();
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    //using (MessageDlg dlg = new MessageDlg(ex.Message, ex.Message))
                    //{
                    //    dlg.ShowDialog();
                    //}
                }


                if (m_token.IsCancellationRequested)
                {
                    // Wait for everyone to stop processing (if needed)
                    //waitForCascadingTasksToComplete(tasks, continuationTasks);

                    //if (TBDataUploadStatusEvent != null)
                    //    TBDataUploadStatusEvent(this, new HWDataUploadStatusEventArgs("One monent please, as we remove Uploaded data", true, 0));

                    clearDataFiles();

                    if (VCFileGenStatusEvent != null)
                        VCFileGenStatusEvent(this, new VCFileGenerationStatusEventArgs("Upload Canceled", 0));
                }
                else
                {
                    //// get the trigger state index in the segregated data files -- uses a binary search for the time stamp
                    //int triggerStateIndex =  locateTriggerStateIndex(triggerTimeStamp, trigChannelID);

                    //// set the meta data on the segrated data files (4 columns & 4 levels * 4 virtual channels)
                    //insertVCMetaData(numOfColumns, numOfLevels, 4, trigChannelID, triggerStateIndex, triggerTimeStamp);

                    //// remove the unused data files.
                    //clearVCDataFiles();
                }

                m_tokenSource.Dispose();
                m_tokenSource = null;

                //
                // Tell whoever cares that we are done post processing the uploaded data
                //
                if (!m_token.IsCancellationRequested)
                {
                    if (VCFileGenStatusEvent != null)
                        VCFileGenStatusEvent(this, new VCFileGenerationStatusEventArgs("Processing MST Complete (P1)...", 0));
                }
            }
            catch ( Exception ex)
            {
                if (VCFileGenStatusEvent != null)
                    VCFileGenStatusEvent(this, new VCFileGenerationStatusEventArgs("Processing MST Phase 1 --- Error!", 0));
            }
        }


        /// <summary>
        /// Located the trigger state in the segregated data 
        /// </summary>
        /// <param name="triggerTimeStamp"></param>
        /// <param name="trigChannelID"></param>
        private async void getSegregatedTriggerLocation(long triggerTimeStamp, int trigChannelID)
        {           
            Task<int>[] tasks = new Task<int>[1];
            ThreadLocal<DP12MST_TrigLocationTaskStateObject> tls = new ThreadLocal<DP12MST_TrigLocationTaskStateObject>();

            tasks[0] = new Task<int>((stateObject) => {
                tls.Value = (DP12MST_TrigLocationTaskStateObject)stateObject;
                ((DP12MST_TrigLocationTaskStateObject)tls.Value).Token.ThrowIfCancellationRequested();

                return locateTriggerStateIndex(tls.Value.TriggerTimeStamp, tls.Value.TriggerChannelID, tls.Value.FileGenerationStatusEvent);
            }, new DP12MST_TrigLocationTaskStateObject(triggerTimeStamp, trigChannelID, VCFileGenStatusEvent, 0.0f, m_token));


            // start the task to get the trigger index
            foreach (Task t in tasks)
                t.Start();

            // wait for the task to finish;  await keeps the GUI thread active while we are waiting..
            await Task.WhenAll(tasks);


            // store the located trigger index for subsequent processing steps.
            m_segregatedTriggerStateIndex = tasks[0].Result;

            // get rid of the task 
            foreach (Task T in tasks)
            {
                T.Dispose();
            }


            //
            // Tell whoever cares that we are done post processing the uploaded data
            //
            if (!m_token.IsCancellationRequested)
            {
                if (VCFileGenStatusEvent != null)
                    VCFileGenStatusEvent(this, new VCFileGenerationStatusEventArgs("Processing MST Complete (P2)...", 0));
            }
            else
            {
                clearDataFiles();
                if (VCFileGenStatusEvent != null)
                    VCFileGenStatusEvent(this, new VCFileGenerationStatusEventArgs("Upload Canceled", 0));
            }
        }


        /// <summary>
        /// Extract the meta data that summaries the binary data.
        /// </summary>
        /// <param name="triggerTimeStamp"></param>
        /// <param name="trigChannelID"></param>
        private async void getSegregatedMetaData(long triggerTimeStamp, int trigChannelID)
        {
            Task<UploadMetaData[]>[] tasks = new Task<UploadMetaData[]>[1];
            ThreadLocal<DP12MST_GetMetaDataTaskStateObject> tls = new ThreadLocal<DP12MST_GetMetaDataTaskStateObject>();

            tasks[0] = new Task<UploadMetaData[]>((stateObject) => {
                tls.Value = (DP12MST_GetMetaDataTaskStateObject)stateObject;
                ((DP12MST_GetMetaDataTaskStateObject)tls.Value).Token.ThrowIfCancellationRequested();

                return getConsolidatedFilesMetaData(tls.Value.NumberOfColumns, tls.Value.NumberOfLevels, tls.Value.NumberOfChannels, tls.Value.TriggerVChannelID, tls.Value.TriggerIndex, tls.Value.FileGenerationStatusEvent, tls.Value.ProgressBarPercentage);

                //return locateTriggerStateIndex(tls.Value.TriggerTimeStamp, tls.Value.TriggerVChannelID);
            }, new DP12MST_GetMetaDataTaskStateObject(m_numOfColumns, m_numOfLevels, m_numOfChannels, m_segregatedTriggerStateIndex, trigChannelID, VCFileGenStatusEvent, 0.0f, m_token));


            // start the task to get the trigger index
            foreach (Task t in tasks)
                t.Start();

            // wait for the task to finish;  await keeps the GUI thread active while we are waiting..
            await Task.WhenAll(tasks);

            // store the located trigger index for subsequent processing steps.
            m_uploadMetaData = tasks[0].Result;

            // get rid of the task 
            foreach (Task T in tasks)
            {
                T.Dispose();
            }


            //
            // Tell whoever cares that we are done post processing the uploaded data
            //
            if (!m_token.IsCancellationRequested)
            {
                if (VCFileGenStatusEvent != null)
                    VCFileGenStatusEvent(this, new VCFileGenerationStatusEventArgs("Processing MST Complete (P3)...", 0));
            }
            else
            {
                clearDataFiles();
                if (VCFileGenStatusEvent != null)
                    VCFileGenStatusEvent(this, new VCFileGenerationStatusEventArgs("Upload Canceled", 0));
            }
        }


        /// <summary>
        /// Get Trigger State Indices
        /// </summary>
        /// <param name="triggerTimeStamp"></param>
        /// <param name="trigChannelID"></param>
        private async void setMetaDataTriggerStateIndices(long triggerTimeStamp, int trigChannelID)
        {
            Task<UploadMetaData[]>[] tasks = new Task<UploadMetaData[]>[1];
            ThreadLocal<DP12MST_GetTrigStateIndicesTaskStateObject> tls = new ThreadLocal<DP12MST_GetTrigStateIndicesTaskStateObject>();

            tasks[0] = new Task<UploadMetaData[]>((stateObject) => {
                tls.Value = (DP12MST_GetTrigStateIndicesTaskStateObject)stateObject;
                ((DP12MST_GetTrigStateIndicesTaskStateObject)tls.Value).Token.ThrowIfCancellationRequested();

            return setMetaDataTriggerStateIndices(tls.Value.MetaData, tls.Value.TriggerTimeStamp, tls.Value.FileGenerationStatusEvent, tls.Value.ProgressBarPercentage); 

                //return locateTriggerStateIndex(tls.Value.TriggerTimeStamp, tls.Value.TriggerVChannelID);
            }, new DP12MST_GetTrigStateIndicesTaskStateObject(m_uploadMetaData, triggerTimeStamp, VCFileGenStatusEvent, 0.0f, m_token));


            // start the task to get the trigger index
            foreach (Task t in tasks)
                t.Start();

            // wait for the task to finish;  await keeps the GUI thread active while we are waiting..
            await Task.WhenAll(tasks);

            // store the located trigger index for subsequent processing steps.
            m_uploadMetaData = tasks[0].Result;

            // get rid of the task 
            foreach (Task T in tasks)
            {
                T.Dispose();
            }


            //
            // Tell whoever cares that we are done post processing the uploaded data
            //
            if (!m_token.IsCancellationRequested)
            {
                if (VCFileGenStatusEvent != null)
                    VCFileGenStatusEvent(this, new VCFileGenerationStatusEventArgs("Processing MST Complete (P4)...", 0));
            }
            else
            {
                clearDataFiles();
                if (VCFileGenStatusEvent != null)
                    VCFileGenStatusEvent(this, new VCFileGenerationStatusEventArgs("Upload Canceled", 0));
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="triggerTimeStamp"></param>
        /// <param name="trigChannelID"></param>
        private async void prependSegregatedMetaData(long triggerTimeStamp, int trigChannelID)
        {
            bool status = false;
            Task<bool>[] tasks = new Task<bool>[VC_CHANNEL_COUNT];  // one task per virtual channel...
            ThreadLocal<DP12MST_PrependMetaTaskStateObject> tls = new ThreadLocal<DP12MST_PrependMetaTaskStateObject>();


            for (int channel = 0; channel < VC_CHANNEL_COUNT; channel++)
            {
                tasks[channel] = new Task<bool>((stateObject) => {
                    tls.Value = (DP12MST_PrependMetaTaskStateObject)stateObject;
                    ((DP12MST_PrependMetaTaskStateObject)tls.Value).Token.ThrowIfCancellationRequested();

                    return prependMetaData(tls.Value.ChannelID, tls.Value.MetaData, tls.Value.FileGenerationStatusEvent, tls.Value.ProgressBarPercentage);

                    //return locateTriggerStateIndex(tls.Value.TriggerTimeStamp, tls.Value.TriggerVChannelID);
                }, new DP12MST_PrependMetaTaskStateObject(channel, m_uploadMetaData, VCFileGenStatusEvent, 0.0f, m_token));
            }


            // start the task to get the trigger index
            foreach (Task t in tasks)
                t.Start();

            // wait for the task to finish;  await keeps the GUI thread active while we are waiting..
            await Task.WhenAll(tasks);

            // store the located trigger index for subsequent processing steps.
            status = tasks[0].Result;

            // get rid of the task 
            foreach (Task T in tasks)
            {
                T.Dispose();
            }


            //
            // Tell whoever cares that we are done post processing the uploaded data
            //
            if (!m_token.IsCancellationRequested)
            {
                // remove un-needed binary files.
                clearVCDataFiles();

                if (VCFileGenStatusEvent != null)
                    VCFileGenStatusEvent(this, new VCFileGenerationStatusEventArgs("Processing MST Data Complete", 0));
            }
            else
            {
                clearDataFiles();
                if (VCFileGenStatusEvent != null)
                    VCFileGenStatusEvent(this, new VCFileGenerationStatusEventArgs("Upload Canceled", 0));
            }
        }

        #endregion // Private Methods

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        public void InitializeProcessingMembers()
        {
            m_numOfColumns = 0x00;
            m_numOfLevels = 0x00;
            m_numOfChannels = 4;
            m_triggerTimeStamp = 0x00;
            m_trigChannelID = 0x00;
            m_segregatedTriggerStateIndex = 0x00;
            m_uploadMetaData = null;
        }


        /// <summary>
        /// Update the virtual channel selected slot information. 
        /// </summary>
        /// <param name="msg"></param>
        public void UpdateSelectedSlotsInfo(DP12MSTMessage_TimeSlotSelectionChanged msg)
        {
            switch (msg.VChannelID)
            {
                case 1:
                    m_VC1_timeSlotCount = msg.SlotIDsList.Count;
                    break;
                case 2:
                    m_VC2_timeSlotCount = msg.SlotIDsList.Count;
                    break;
                case 3:
                    m_VC3_timeSlotCount = msg.SlotIDsList.Count;
                    break;
                case 4:
                    m_VC4_timeSlotCount = msg.SlotIDsList.Count;
                    break;

                default:
                    break;
            } 
        }


        /// <summary>
        /// Convert the uploaded data files into the Virtual Channel data files.
        /// </summary>
        /// <param name="dataFileNames"></param>
        /// <returns></returns>
        public bool GenerateDataFiles(List<string> dataFileNames, long triggerTimeStamp, int trigChannelID)
        {
            bool status = true;

            // to be used in the next step of processing.
            m_triggerTimeStamp = triggerTimeStamp;
            m_trigChannelID = trigChannelID;

            if (dataFileNames.Count == 4) //Capacity == 4)
            {
                m_numOfColumns = 2;
                m_numOfLevels = 2;
                segregateDataFile(m_numOfColumns, m_numOfLevels, triggerTimeStamp, trigChannelID);
            }
            else if (dataFileNames.Count == 8)  // 2 columns, 4 levels
            {
                m_numOfColumns = 2;
                m_numOfLevels = 4;
                segregateDataFile(m_numOfColumns, m_numOfLevels, triggerTimeStamp, trigChannelID);
            }
            else if (dataFileNames.Count == 16)  // 4 columns, 4 levels
            {
                m_numOfColumns = 4;
                m_numOfLevels = 4;
                segregateDataFile(m_numOfColumns, m_numOfLevels, triggerTimeStamp, trigChannelID);
            }
            else if (dataFileNames.Count == 32) // 4 columns, 8 levels
            {
                m_numOfColumns = 4;
                m_numOfLevels = 8;
                segregateDataFile(m_numOfColumns, m_numOfLevels, triggerTimeStamp,trigChannelID);
            }
            else if (dataFileNames.Count == 64) // 4 columns, 16 levels
            {
                m_numOfColumns = 4;
                m_numOfLevels = 16;
                segregateDataFile(m_numOfColumns, m_numOfLevels, triggerTimeStamp,trigChannelID);
            }
            else
            {
                // error setup.. do nothing...
                clearVCDataFiles();
            }

            if (!status)
                clearVCDataFiles();

            return status;
        }


        /// <summary>
        /// Get trigger location
        /// </summary>
        /// <param name="triggerTimeStamp"></param>
        /// <param name="trigChannelID"></param>
        /// <returns></returns>
        public void GetSegregatedTriggerLocation(long triggerTimeStamp, int trigChannelID)
        {
            getSegregatedTriggerLocation(triggerTimeStamp, trigChannelID);
        }


        /// <summary>
        /// ???
        /// </summary>
        /// <param name="triggerTimeStamp"></param>
        /// <param name="trigChannelID"></param>
        public void GetSegregatedMetaData(long triggerTimeStamp, int trigChannelID)
        {
            getSegregatedMetaData(triggerTimeStamp, trigChannelID);
        }


        /// <summary>
        /// ???
        /// </summary>
        /// <param name="triggerTimeStamp"></param>
        /// <param name="trigChannelID"></param>
        public void SetMetaDataTriggerStateIndices(long triggerTimeStamp, int trigChannelID)
        {
            setMetaDataTriggerStateIndices(triggerTimeStamp, trigChannelID);
        }


        /// <summary>
        /// ???
        /// </summary>
        /// <param name="triggerTimeStamp"></param>
        /// <param name="trigChannelID"></param>
        public void PrependSegregatedMetaData(long triggerTimeStamp, int trigChannelID)
        {
            prependSegregatedMetaData(triggerTimeStamp, trigChannelID);
        }


        /// <summary>
        /// returns a copy of the meta data parameters array
        /// </summary>
        /// <returns></returns>
        public DP12MST_MultiThread_MetaDataArgs[] GetMultiThreadedFileMetaData()
        {
            // return (DP12MST_MultiThread_MetaDataArgs[])m_uploadMetaData.Clone();

            DP12MST_MultiThread_MetaDataArgs[] metaDataArgs = new DP12MST_MultiThread_MetaDataArgs[m_uploadMetaData.Length];
            for(int i=0; i<metaDataArgs.Length; i++)
            {
                DP12MST_MultiThread_MetaDataArgs currentArgs = new DP12MST_MultiThread_MetaDataArgs();
                m_uploadMetaData[i].Copy(currentArgs);

                metaDataArgs[i] = currentArgs;
            }

            return metaDataArgs;
        }
        #endregion // Public Methods
    }
}
