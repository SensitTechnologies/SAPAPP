using SAPAPP.AutoScripts;
using SAPAPP.Configs;
using System.ComponentModel;
using System.Diagnostics;

namespace SAPAPP.Controllers
{
    internal class DownloadController
    {

        #region Instance Variables

        //scripts
        private Script[] scripts = [];
        private STMScript STMScript = new();
        private MegaScript MegaScript = new();
        private MSP430Script MSPScript = new();
        private FuelGaugeScript FuelGaugeScript = new();

        /// <summary>
        /// This field stores and grabs the CLI integration for STM32 Cube Programmer
        /// </summary>
        public string STM32_PROGRAMMER_CLI
        {
            get { return STMScript.STM32_Programmer_CLI; }
            set { STMScript.STM32_Programmer_CLI = value; }
        }

        /// <summary>
        /// This field stores and grabs the CLI integration for avrdude
        /// </summary>
        public string AVRDUDE_CLI
        {
            get { return MegaScript.AVRDUDE_CLI; }
            set { MegaScript.AVRDUDE_CLI = value; }
        }

        /// <summary>
        /// This field stores and grabs the CLI integrations folder for TI Uniflash
        /// </summary>
        public string TI_UNIFLASH_FOLDER
        {
            get { return MSPScript.TI_UNIFLASH_FOLDER; }
            set { MSPScript.TI_UNIFLASH_FOLDER = value; }
        }

        // current firmware to flash
        private Part currentDownload = new();

        // Asynchronous worker
        private BackgroundWorker worker;

        // signals when to search and download
        private bool Running = false;

        // tells whether automatic mode is on or off
        public bool AutomaticOn { get => ScriptHasAutomatic(); }

        // Feedback Devices
        private Logger logger = new();
        private Action<string> UpdateMessageAction { get; set; } = (message) =>
        {
            Debug.WriteLine(message);
        };

        private Action<int> UpdateProgbarAction { get; set; } = (progress) =>
        {
            Debug.WriteLine(progress);
        };

        #endregion

        #region Constructors

        public DownloadController()
        {

            worker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            worker.DoWork += Worker_Run;
            worker.RunWorkerAsync();
        }

        public DownloadController(Logger lg) : this()
        {
            logger = lg;

            STMScript = new STMScript(logger, UpdateMessageFeedback, UpdateProgbarFeedback);
            MegaScript = new MegaScript(logger, UpdateMessageFeedback, UpdateProgbarFeedback);
            MSPScript = new MSP430Script(logger, UpdateMessageFeedback, UpdateProgbarFeedback);
            FuelGaugeScript = new FuelGaugeScript(logger, UpdateMessageFeedback, UpdateProgbarFeedback);

            scripts = [STMScript, MegaScript, MSPScript, FuelGaugeScript];
        }

        public DownloadController(Logger logger, Action<string> updateFeedbackAction, Action<int> updateProgBarAction) : this(logger)
        {
            UpdateMessageAction = updateFeedbackAction;
            UpdateProgbarAction = updateProgBarAction;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Determines which Script to be used with the current download
        /// </summary>
        /// <returns></returns>
        private Script ActiveScript()
        {
            foreach (Script script in scripts)
            {
                if (script.CompatibleArchitecture == currentDownload.Architecture)
                {
                    return script;
                }
            }
            return null;
        }

        /// <summary>
        /// Determines whether The active script can be used in Automatic mode or not
        /// </summary>
        /// <returns></returns>
        private bool ScriptHasAutomatic()
        {
            return ActiveScript().GetCapableAutomatic();
        }

        #endregion

        #region Run, Search, and Download algorithms

        /// <summary>
        /// Loops for the duration of the program and and controlls the work control of the Download Controller
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worker_Run(object? sender, DoWorkEventArgs e)
        {
            BackgroundWorker? worker = sender as BackgroundWorker;
            bool ReadyToDownload = true;

            while (true)
            {
                while (!worker.CancellationPending && Running)
                {
                    if (!ScriptHasAutomatic())
                    {
                        LogNUpdate_Info($"Automatic mode not available for {currentDownload.Architecture}... Starting ");
                        Download();
                        LogNUpdate_Info("Download Finished");
                        Running = false;
                        break;
                    }

                    bool detected = Detect();
                    if (detected && ReadyToDownload)
                    {
                        LogNUpdate_Info("New board detected. staring download");
                        Download();
                        LogNUpdate_Info("Download Finished");
                        ReadyToDownload = false;
                    }
                    else if (!(detected || ReadyToDownload))
                    {
                        LogNUpdate_Info("Probe has been removed. starting search for new board");
                        UpdateProgbarFeedback(0);
                        ReadyToDownload = true;
                    }
                }
            }
        }

        /// <summary>
        /// Detects if a microcontroller compatible with the current script is connected to the computer
        /// </summary>
        /// <returns>Whether a board has been detected for the active script</returns>
        private bool Detect() => ActiveScript().Detect();

        /// <summary>
        /// Downloads the current firmware configuration
        /// </summary>
        private void Download() => ActiveScript().Download(currentDownload);

        #endregion

        #region ButtonReactions

        /// <summary>
        /// Sets download as the current download and turns the search and download algorithm on.
        /// </summary>
        /// <param name="download"></param>
        public void StartRunning(Part download)
        {
            if (!Running)
            {
                LogNUpdate_Info($"Starting search for Board...");
                currentDownload = download;
                Running = true;
            }
        }

        /// <summary>
        /// Stops the search and download 
        /// </summary>
        public void StopRunning()
        {
            LogNUpdate_Info($"Stopping search for Board...");
            Running = false;
        }

        #endregion

        #region Feedback and friendly functions for logging

        /// <summary>
        /// Helpful method to Log and Display a feedback message to the log file and UI respectively
        /// </summary>
        /// <param name="message"></param>
        private void LogNUpdate_Info(string message)
        {
            logger.Log(message, Logger.LogType.Info);
            UpdateMessageFeedback(message);
        }

        /// <summary>
        /// Writes a new message to the Message Display Box inside the Status Bar
        /// </summary>
        /// <param name="message">The message to be written</param>
        private void UpdateMessageFeedback(string message) => UpdateMessageAction(message);

        /// <summary>
        /// Changes the progress display on the Status Bar
        /// </summary>
        /// <param name="progress">The current progress of an Download Process</param>
        private void UpdateProgbarFeedback(int progress) => UpdateProgbarAction(progress);

        #endregion
    }
}
