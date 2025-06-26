using SAPAPP.Configs;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;

namespace SAPAPP.Scripts
{
    internal class STMAutomaticTest
    {
        private bool automaic = true;
        private bool running = false;

        BackgroundWorker detectionWorker;

        protected Part currentDownload = new();


        private string _stm32_prog_cli;
        public string STM32_Programmer_CLI
        {
            get { return string.Format("\"{0}\"", _stm32_prog_cli); }
            set { _stm32_prog_cli = value; }
        }
        public STMAutomaticTest()
        {
            detectionWorker = new()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true,
            };

            detectionWorker.DoWork += DetectionWorker_DoWork;

            detectionWorker.RunWorkerAsync();
        }

        private void DetectionWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            BackgroundWorker? worker = sender as BackgroundWorker;

            bool detected = false, readyToDownload = true;

            while (automaic)
            {
                while (!worker.CancellationPending && running)
                {
                    detected = Detect();
                    if (detected && readyToDownload) // new probe was detected
                    {
                        MessageBox.Show("New board detected. staring download");
                        Download(worker);
                        MessageBox.Show("Download Finished");
                        readyToDownload = false;
                    }
                    else if (!detected && !readyToDownload) // Probe was removed from board after previous  download
                    {
                        MessageBox.Show("Probe has been removed. starting search for new board");
                        readyToDownload = true;
                    }
                }
            }
        }

        private bool Detect()
        {
            string connect = "-c port=SWD";

            string strCmdText = STM32_Programmer_CLI + " " + connect;

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
                }
            };

            StringBuilder results = new StringBuilder();

            cmd.OutputDataReceived += new DataReceivedEventHandler((sender, eventArgs) =>
            {
                if (eventArgs.Data != null)
                {
                    results.AppendLine(eventArgs.Data);
                }
            });
            cmd.ErrorDataReceived += new DataReceivedEventHandler((sender, eventArgs) =>
            {
                if (eventArgs.Data != null)
                {
                    results.AppendLine(eventArgs.Data);
                }
            });

            cmd.Start();
            cmd.BeginOutputReadLine();
            cmd.BeginErrorReadLine();
            cmd.WaitForExit();
            cmd.Close();

            if (results.ToString().Contains("Error: No debug probe detected.") || results.ToString().Contains("Error: No STM32 target found"))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void Download(BackgroundWorker worker)
        {
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
                    Arguments = "/c" + strCmdText,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }
            };

            cmd.Start();
            cmd.WaitForExit();
            cmd.Close();
        }

        public void start(Part download)
        {
            if (!running)
            {
                running = true;
                currentDownload = download;
            }
        }

        public void Cancel()
        {
            running = false;
        }
    }
}
