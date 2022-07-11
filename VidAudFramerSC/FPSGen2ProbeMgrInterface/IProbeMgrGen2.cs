using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;


namespace FPSProbeMgr_Gen2
{
    public struct RunTimeParameters
    {
        public bool DumpRegisters;
        public bool VerboseErrorReporting;

        public RunTimeParameters( bool dumpRegisters, bool verboseReporting )
        {
            DumpRegisters = dumpRegisters;
            VerboseErrorReporting = verboseReporting; 
        }
    } 

    public interface IProbeMgrGen2
    {
        string GetAssemblyVersion();
        string GetFPGAVersion();
        string GetTitleString();

        bool Configure(int deviceNum, string serialNumberStr, bool inDemoMode);
        bool Initialize();
        void CloseProbe();

        bool Run();
        bool Stop();
        bool Stopped();
        bool ShutDown();

        bool ProcessTimerTick( RunTimeParameters runtimeParameters);
        string GetLogMsgs();

        bool SetDefaultConfiguration();

        // Load a configuration from a file
        bool SetStoredConfiguration(string fileName, int selectedProtocolIndex);

        // Read a configuration to be written to a file
        bool SaveConfiguration(string fileName, int selectedProtocolIndex);

        // Load a user interface window
        bool DisplayForm(string FormName);

        void MiscOperation(string title, object parameters = null);

        event LogMsgEvent OnLogMsgEvent;
        event TBUploadEvent OnTBUploadEvent;
        event ProbeCommEvent OnProbeCommEvent;


        //
        // Pixel Renderer Additions   -- this should probably be moved into its own interface definition and added to the DP12MST class (inheriting an additional I/F).
        //

        int GetTriggerChannelID();

        long GetNumberOfStates(int virtualChannelID);

        long GetTriggerStateIndex(int virtualChannelID);

        byte[] GetStateData(int virtualChannelID, int index);

        List<byte> GetStateDataChunk(int startStart, int chunkSize, int VChannelID = 1);

        bool SetDataFolderPath(string path);
    }

    //Create a new event handler delegate, so that information can be passed back to a form. 
    // The standard EventHandler could be used, but then information can't be passed.
    public delegate void LogMsgEvent(object sender, LogMsgEventArgs e);

    //Class that used to hold  info that gets passed back. It has to decend from EventArgs.
    public class LogMsgEventArgs : EventArgs
    {
        public StringBuilder msgs;

        public LogMsgEventArgs(StringBuilder logMsgs)
        {
            this.msgs = logMsgs;
        }
    }

    //Create a new event handler delegate, so that information can be passed back to a form. 
    // The standard EventHandler could be used, but then information can't be passed.
    public delegate void TBUploadEvent(object sender, TBUploadEventArgs e);

    //Class that used to hold  info that gets passed back. It has to decend from EventArgs.
    public class TBUploadEventArgs : EventArgs
    {
        public string eventTitle;
        public bool BGTaskRunning;
        public int NumOfStatesProcessed;
        public int NumOfStatesCaptured;
        public float Parameter;

        public TBUploadEventArgs(string title,  bool running, int statesProcessed, int statesCaptured, float parameter = 0)
        {
            this.eventTitle = title;
            this.BGTaskRunning = running;
            this.NumOfStatesProcessed = statesProcessed;
            this.NumOfStatesCaptured = statesCaptured;
            this.Parameter = parameter;  // used in processing uploaded data (segregating data files)
        }
    }


    public delegate void ProbeCommEvent(object sender, ProbeCommEventArgs e);

    public class ProbeCommEventArgs : EventArgs
    {
        public string EventTitle;

        public ProbeCommEventArgs(string title)
        {
            this.EventTitle = title;
        }
    }
}
