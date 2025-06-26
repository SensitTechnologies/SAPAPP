using SAPAPP.Configs;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace SAPAPP.Scripts
{
    internal class TestScript
    {
        protected BackgroundWorker backgroundWorker;
        protected const bool testing = false;
        protected const int delay = 0; // delay time in milliseconds

        protected Part currentDownload = new();

        // Feedback Devices
        protected Logger logger;
        protected TextBlock FeedbackDisplay;
        protected TextBlock progressPercentage;
        protected ProgressBar progbar;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lg"></param>
        /// <param name="fd"></param>
        /// <param name="pp"></param>
        /// <param name="pb"></param>
        public TestScript(Logger lg, TextBlock fd, TextBlock pp, ProgressBar pb)
        {
            logger = lg;
            FeedbackDisplay = fd;
            progressPercentage = pp;
            progbar = pb;

            backgroundWorker = InitializeBackgroundWorker();
        }

        private BackgroundWorker InitializeBackgroundWorker()
        {
            BackgroundWorker worker = new()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true,
            };

            worker.DoWork +=
                new DoWorkEventHandler(BackgroundWorker_DoWork);
            worker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(BackgroundWorker_RunWorkerCompleted);
            worker.ProgressChanged +=
                new ProgressChangedEventHandler(BackgroundWorker_ProgressChanged);

            return worker;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="download"></param>
        public void Download(Part download)
        {
            if (!backgroundWorker.IsBusy)
            {
                logger.Log("Starting Download", Logger.LogType.Info);
                currentDownload = download;
                backgroundWorker.RunWorkerAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Cancel()
        {
            if (backgroundWorker.IsBusy)
            {
                backgroundWorker.CancelAsync();
            }
        }

        // This event handler is where the time-consuming work is done.
        protected void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            string strCmdText = "dir";
            string firmwareDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;


            Process cmd = new()
            {
                StartInfo = new()
                {
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    Arguments = "/c" + strCmdText,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = firmwareDir,
                }
            };
            cmd.OutputDataReceived += new DataReceivedEventHandler((sender, eventArgs) =>
            {
                if (eventArgs.Data != null)
                {
                    logger.Log(eventArgs.Data, Logger.LogType.Info);
                    UpdateProgress(eventArgs.Data);
                }
            });
            cmd.ErrorDataReceived += new DataReceivedEventHandler((sender, eventArgs) =>
            {
                if (eventArgs.Data != null)
                {
                    e.Result = eventArgs.Data;
                    HandleError(eventArgs.Data);
                }
            });

            cmd.Start();
            if (!testing)
            {
                cmd.BeginErrorReadLine();
                cmd.BeginOutputReadLine();
                cmd.WaitForExitAsync();

                while ((worker.IsBusy) && (!cmd.HasExited))
                {
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        cmd.CancelErrorRead();
                        cmd.CancelOutputRead();
                        cmd.Kill();
                    }
                }
            }

            if (!cmd.HasExited)
            {
                cmd.Close();
            }
        }


        // This event handler updates the progress.
        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                progbar.Value = (double)e.ProgressPercentage;
                progressPercentage.Text = e.ProgressPercentage.ToString() + '%';
            });
        }

        // This event handler deals with the results of the background operation.
        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (e.Result != null)
                {
                    logger.Log(e.Result.ToString(), Logger.LogType.Error);
                    FeedbackDisplay.Text = e.Result.ToString();
                }
                else if (e.Cancelled)
                {
                    FeedbackDisplay.Text = "Canceled!";
                }
                else if (e.Error != null)
                {
                    logger.Log(e.Error.Message, Logger.LogType.Error);
                    FeedbackDisplay.Text = "Error: " + e.Error.Message;
                }
                else
                {
                    logger.Log("Download Finished Successfully!", Logger.LogType.Pass);
                    MessageBox.Show("Download Finished Successfully!", "Download Finished", MessageBoxButton.OK, MessageBoxImage.Information);
                    FeedbackDisplay.Text = "Done!";
                }
            }));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="line"></param>
        protected void HandleError(string line)
        {
            MessageBox.Show(line);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        protected void UpdateProgress(string line)
        {
            UpdateProgressFeedback(line);
        }

        protected void UpdateProgressFeedback(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if ((message != null) && (message != ""))
                {
                    FeedbackDisplay.Text = message;
                }
            });
            System.Threading.Thread.Sleep(delay);
        }
    }

}
