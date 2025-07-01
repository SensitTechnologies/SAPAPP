using SAPAPP.Configs;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SAPAPP.Scripts
{
    internal class FetScript(Logger lg, TextBlock fd, TextBlock pp, ProgressBar pb) : Script(lg, fd, pp, pb)
    {
        public string ToolsFolder { get; set; }

        public override void Download(Part download)
        {
            if (!backgroundWorker.IsBusy)
            {
                currentDownload = download;
                backgroundWorker.RunWorkerAsync();
            }
        }

        protected override void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker? worker = sender as BackgroundWorker;

            string strCmdText = string.Format(
                "FetExecutor.bat \"{0}\" \"user_files\\configs\\{1}.ccxml\"",
                currentDownload.FullFirmwarePath(), currentDownload.CCXML_Config_File);

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
                    WorkingDirectory = ToolsFolder,
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
            if (line.Contains("system cannot find the path specified", StringComparison.CurrentCultureIgnoreCase)) // package needs to be recompiled
            {
                message = line + "\nRecompile Package and try again.";
                header = "Package Error";
            }
            else if (line.Contains("no usb fet", StringComparison.CurrentCultureIgnoreCase))
            {
                message = line;
                header = "Connection Error";
            }
            else
            {
                message = line;
                header = "Error";
            }

            MessageBox.Show(message, header, MessageBoxButton.OK, MessageBoxImage.Error);
            Cancel();
        }

        protected override void UpdateProgress(string line)
        {
            int progress = -1;
            string DisplayMessage = "";


            line = line.Trim();
            string[] words = line.Split(' ');
            if (line.Contains("Finished"))
            {
                progress = 100;
            }
            else if (line.Contains('%'))
            {
                progress = int.Parse(words[^1].Trim('%'));
            }

            if (line.Contains("Configuring"))
            {
                DisplayMessage = line;
            }
            else if (line.Contains("Initializing"))
            {
                DisplayMessage = line + "...";
            }
            else if (line.Contains("Connecting"))
            {
                DisplayMessage = line;
            }
            else if (line.Contains("Loading"))
            {
                DisplayMessage = words[0] + ' ' + words[1] + ' ' + currentDownload.FullFirmwarePath();
            }
            else if (line.Contains("Verifying"))
            {
                DisplayMessage = words[0] + ' ' + words[1] + ' ' + currentDownload.FullFirmwarePath();
            }

            if (progress > 100)
            {
                progress = 100;
            }
            if (progress > 0)
            {
                backgroundWorker.ReportProgress(progress);
            }
            UpdateProgressFeedback(DisplayMessage);
        }
    }
}
