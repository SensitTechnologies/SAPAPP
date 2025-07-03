using SAPAPP.Configs;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SAPAPP.Scripts
{
    internal class MegaScript(Logger lg, TextBlock fd, TextBlock pp, ProgressBar pb) : Script(lg, fd, pp, pb)
    {

        private string _avrdudeCLI;
        public string AVRDUDE_CLI
        {
            get => string.Format("\"{0}\"", _avrdudeCLI); set => _avrdudeCLI = value;
        }

        private string boardType = "m2560";

        public override void Download(Part download)
        {
            if (!backgroundWorker.IsBusy)
            {
                currentDownload = download;
                backgroundWorker.RunWorkerAsync();
            }
            //await UpdateProgressBar();
        }

        protected override void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            //string fullCommand = "avrdude -c avrispmkII -p m2560 -P usb:20:38 -U flash:w:\"C:\\Users\\nbealsla\\Downloads\\Blink\\build\\arduino.avr.megaADK\\Blink.ino.hex\":i";

            string connect = GetConnection();
            string write = "-U flash:w:" + currentDownload.FullFirmwarePath() + ":i";
            string verbose = "-v 3";
            string strCmdText = AVRDUDE_CLI + " " + connect + " " + write + " " + verbose;

            /*
            Process cmd = new()
            {
                StartInfo = new()
                {
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    Arguments = "/k" + strCmdText,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    CreateNoWindow = false,
                    WorkingDirectory = currentDownload.FirmwarePath,
                }
            };


            cmd.Start();
            cmd.WaitForExit();

            //string results = cmd.StandardOutput.ReadToEnd() + cmd.StandardError.ReadToEnd();

            cmd.Close();

            //MessageBox.Show(results);

            /*
            StringBuilder sb = new StringBuilder();
            int i = 0;

            string[] lines = results.Split('\n');
            foreach (string line in lines)
            {
                sb.Append(i + ": ");
                sb.Append(line);
                sb.Append("\n");
                i++;
            }

            MessageBox.Show(sb.ToString());
            
            /*/

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
                    WorkingDirectory = currentDownload.FullPath()
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

        private string GetConnection()
        {
            string fullformat = "-c avrispmkII -p {0} -P {1}";
            string usbformat = "usb:{0}";

            string strCmdText = AVRDUDE_CLI + " " + string.Format(fullformat, boardType, string.Format(usbformat, "xxx")) + " -v";

            ProcessStartInfo processStartInfo = new()
            {
                FileName = "cmd.exe",
                UseShellExecute = false,
                Arguments = "/c" + strCmdText,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            Process cmd = new()
            {
                StartInfo = processStartInfo,
            };

            cmd.Start();
            cmd.WaitForExit();

            string results = cmd.StandardOutput.ReadToEnd();
            results += "\n";
            results = cmd.StandardError.ReadToEnd();
            cmd.Close();


            string lastFour = "";
            string[] lines = results.Split("\n");
            foreach (string line in lines)
            {
                if (line.ToLower().Contains("found"))
                {
                    string[] words = line.Split(' ');
                    string serno = words[words.Length - 1];

                    string digitformat = "{0}{1}";
                    string firstTwo = string.Format(digitformat, serno[serno.Length - 5], serno[serno.Length - 4]);
                    string lastTwo = string.Format(digitformat, serno[serno.Length - 3], serno[serno.Length - 2]);

                    lastFour = string.Format("{0}:{1}", firstTwo, lastTwo);
                }
            }
            string serial = string.Format(usbformat, lastFour);

            return string.Format(fullformat, boardType, serial);
        }

        protected override void HandleError(string line)
        {
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

            if (line.Contains("Writing"))
            {
                DisplayMessage = "Writing...";
            }
            else if (line.Contains("Reading"))
            {
                DisplayMessage = "Reading...";
            }


            if (line.Contains("done"))
            {
                progress = 100;
            }
            else if (line.Contains('%'))
            {
                progress = int.Parse(words[^2].Trim('%'));
                DisplayMessage = "";
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
