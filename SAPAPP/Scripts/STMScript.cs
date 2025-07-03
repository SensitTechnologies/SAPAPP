using SAPAPP.Configs;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SAPAPP.Scripts
{
    internal class STMScript(Logger lg, TextBlock fd, TextBlock pp, ProgressBar pb) : Script(lg, fd, pp, pb)
    {
        private string _stm32_prog_cli;
        public string STM32_Programmer_CLI
        {
            get => string.Format("\"{0}\"", _stm32_prog_cli); set => _stm32_prog_cli = value;
        }

        public override void Download(Part download)
        {
            if (!backgroundWorker.IsBusy)
            {
                logger.Log("Starting Download", Logger.LogType.Info);
                currentDownload = download;
                backgroundWorker.RunWorkerAsync();
            }
        }

        protected override void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker? worker = sender as BackgroundWorker;

            //extra data needed
            string writeHead = "0x08000000";

            // basic commands
            string connect = "-c port=SWD";                                                 // connect to board command
            string write = "-w " + currentDownload.FullFirmwarePath() + " " + writeHead;    // write command

            string strCmdText = STM32_Programmer_CLI + " " + connect + " " + write;
            string firmwareDir = currentDownload.FirmwareFolder;


            Process cmd = new()
            {
                StartInfo = new()
                {
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    Arguments = testing ? "/k" + strCmdText : "/c" + strCmdText,
                    RedirectStandardOutput = !testing,
                    RedirectStandardError = !testing,
                    CreateNoWindow = !testing,
                }
            };

            cmd.OutputDataReceived += new DataReceivedEventHandler((sender, eventArgs) =>
            {
                if (eventArgs.Data != null)
                {
                    string line = eventArgs.Data;
                    if (line.Contains("Error:"))
                    {
                        e.Result = eventArgs.Data;
                        logger.Log(eventArgs.Data, Logger.LogType.Error);
                        HandleError(eventArgs.Data);
                    }
                    else
                    {
                        logger.Log(eventArgs.Data, Logger.LogType.Info);
                        UpdateProgress(eventArgs.Data);
                    }
                }
            });
            cmd.ErrorDataReceived += new DataReceivedEventHandler((sender, eventArgs) =>
            {
                if (eventArgs.Data != null)
                {
                    e.Result = eventArgs.Data;
                    logger.Log(eventArgs.Data, Logger.LogType.Error);
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

        protected override void HandleError(string line)
        {
            line = line.Trim();
            string message, header;

            message = line;
            header = "Error";

            MessageBox.Show(message, header, MessageBoxButton.OK, MessageBoxImage.Error);
            Cancel();
        }

        protected override void UpdateProgress(string line)
        {
            int progress = -1;
            string DisplayMessage = "";

            line = line.Trim();
            string[] words = line.Split(' ');

            if (!line.Contains("--"))
            {
                DisplayMessage = line;
            }


            if (line.Contains("download complete"))
            {
                progress = 100;
            }
            else if (line.Contains('%'))
            {
                progress = int.Parse(words[^1].Trim('%'));
                DisplayMessage = "";
            }

            if (progress > 100)
            {
                progress = 100;
            }
            if (progress > 0)
            {
                backgroundWorker.ReportProgress((int)progress);
            }
            UpdateProgressFeedback(DisplayMessage);
        }
    }
}
