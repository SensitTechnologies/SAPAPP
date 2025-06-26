using SAPAPP.AutoScripts;
using SAPAPP.Configs;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SAPAPP.Controllers
{
    internal class DownloadController
    {

        #region Instance Variables

        //scripts
        private STMScript STMScript = new();
        public string STM32_Programmer_CLI
        {
            get { return STMScript.STM32_Programmer_CLI; }
            set { STMScript.STM32_Programmer_CLI = value;}
        }

        // current firmware to flash
        private Part currentDownload = new();

        // Asynchronous worker
        private BackgroundWorker worker;

        // signals when to search and download
        private bool Running = false;

        // tells whether automatic mode is on or off
        public bool AutomaticOn {get; } = true;

        // Feedback Devices
        private Logger logger;
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

            worker.DoWork += worker_Run;
            worker.RunWorkerAsync();
        }

        public DownloadController(Logger lg) : this()
        {
            logger = lg;

            STMScript = new STMScript(logger, UpdateMessageFeedback, UpdateProgbarFeedback);
        }

        public DownloadController(Logger logger, Action<string> updateFeedbackAction, Action<int> updateProgBarAction) : this(logger)
        {
            UpdateMessageAction = updateFeedbackAction;
            UpdateProgbarAction = updateProgBarAction;
        }

        #endregion

        #region Run, Search, and Download algorithms

        private void worker_Run(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker? worker = sender as BackgroundWorker;

            bool ReadyToDownload = true, detected = false;

            while (true)
            {
                while (!worker.CancellationPending && Running)
                {
                    detected = Detect();
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

                    if (!AutomaticOn)
                    {
                        Running = false;
                    }
                }
            }
        }

        private bool Detect()
        {
            return STMScript.Detect();
        }

        private void Download()
        {
            STMScript.Download(currentDownload);
        }

        #endregion

        #region ButtonReactions

        public void StartRunning(Part download)
        {
            if (!Running)
            {
                LogNUpdate_Info($"Starting search for Board...");
                currentDownload = download;
                Running = true;
            }
        }

        public void StopRunning() 
        {
            LogNUpdate_Info($"Stopping search for Board...");
            Running = false;
        }

        #endregion

        #region Feedback and friendly functions for logging

        private void LogNUpdate_Info(string message)
        {
            logger.Log(message, Logger.LogType.Info);
            UpdateMessageFeedback(message);
        }

        private void UpdateMessageFeedback(string message) => UpdateMessageAction(message);
        private void UpdateProgbarFeedback(int progress) => UpdateProgbarAction(progress);
        

        #endregion
    }
}
