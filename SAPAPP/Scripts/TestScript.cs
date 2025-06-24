using SAPAPP.Configs;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace SAPAPP.Scripts
{
    internal class TestScript(Logger lg, TextBlock fd, TextBlock pp, ProgressBar pb) : Script(lg, fd, pp, pb)
    {
        public override void Download(Part download)
        {
            if (!backgroundWorker.IsBusy)
            {
                currentDownload = download;
                backgroundWorker.RunWorkerAsync();
            }
        }

        // This event handler is where the time-consuming work is done.
        protected override void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
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


        protected override void HandleError(string line)
        {
            MessageBox.Show(line);
        }

        protected override void UpdateProgress(string line)
        {
            UpdateProgressFeedback(line);
        }
    }
}
