using SAPAPP.Configs;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace SAPAPP.AutoScripts
{
    internal class MegaScript : Script
    {
        private string _avrdudeCLI;
        public string AVRDUDE_CLI
        {
            get => _avrdudeCLI; set => _avrdudeCLI = value;
        }

        private string boardType = "m2560";


        #region Constructors

        public MegaScript() : base()
        {
            CompatibleArchitecture = Architecture.ATMEGA;
            //CapableAutomatic = false;
        }

        public MegaScript(Logger logger) : base(logger)
        {
            CompatibleArchitecture = Architecture.ATMEGA;
            //CapableAutomatic = false;
        }

        public MegaScript(Logger logger, Action<string> updateFeedbackAction, Action<int> updateProgBarAction) : base(logger, updateFeedbackAction, updateProgBarAction)
        {
            CompatibleArchitecture = Architecture.ATMEGA;
            //CapableAutomatic = false;
        }

        #endregion

        #region Search and Download algorithms

        public override bool Detect()
        {
            if (!File.Exists(AVRDUDE_CLI))
            {
                MessageBox.Show("Error: Configure STM32 Cube Programmer before you try to Download");
                return false;
            }

            string connect = GetConnection();
            string board = $"-p {boardType}";
            string verbose = "-vb 3";
            string strCmdText = $"\"{AVRDUDE_CLI}\" {connect} {board} {verbose}";

            Process cmd = new()
            {
                StartInfo = new()
                {
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    Arguments = $"/c {strCmdText}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }
            };

            StringBuilder results = new();
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

            if (results.ToString().Contains("AVR device initialized and ready to accept instructions"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Download(Part currentDownload)
        {
            if (!File.Exists(AVRDUDE_CLI))
            {
                MessageBox.Show("Error: Configure STM32 Cube Programmer before you try to Download");
                return;
            }


            string connect = GetConnection();
            string board = $"-p {boardType}";
            string write = $"-U flash:w:{currentDownload.FirmwareFile}:i";
            string verbose = "-vb 3";
            string strCmdText = $"\"{AVRDUDE_CLI}\" {connect} {board} {write} {verbose}";

            MessageBox.Show(strCmdText);


            Process cmd = new()
            {
                StartInfo = new()
                {
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    Arguments = $"/k {strCmdText}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = currentDownload.FullPath()
                }
            };

            cmd.OutputDataReceived += new DataReceivedEventHandler((sender, eventArgs) =>
            {
                if (eventArgs.Data != null)
                {
                    ProcessOutputData(eventArgs.Data);
                }
            });
            cmd.ErrorDataReceived += new DataReceivedEventHandler((sender, eventArgs) =>
            {
                if (eventArgs.Data != null)
                {
                    ProcessOutputData(eventArgs.Data);
                }
            });

            cmd.Start();
            cmd.BeginErrorReadLine();
            cmd.BeginOutputReadLine();
            cmd.WaitForExit();
            cmd.Close();
        }

        #endregion


        #region Helper methods

        private string GetConnection()
        {
            string strCmdText = $"\"{AVRDUDE_CLI}\" -c avrispmkII -P usb:{"xxx"} -v";

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
                },
            };

            StringBuilder results = new();
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


            string lastFour = "";
            string[] lines = results.ToString().Split("\n");
            foreach (string line in lines)
            {
                if (line.ToLower().Contains("found"))
                {
                    string[] words = line.Split(' ');
                    string serno = words[words.Length - 1];

                    lastFour = $"{serno[serno.Length - 5]}{serno[serno.Length - 4]}:{serno[serno.Length - 3]}{serno[serno.Length - 2]}";
                }
            }
            return $"-c avrispmkII -p usb:{lastFour}";
        }


        #endregion

        #region Feedback FIltering

        protected override void HandleError(string line)
        {
            line = line.Trim();
            string message, header;

            message = line;
            header = "Error";

            MessageBox.Show(message, header, MessageBoxButton.OK, MessageBoxImage.Error);
            //Cancel();
        }

        protected override void ProcessOutputData(string data)
        {
            string line = data.Trim();

            logger.Log(line, Logger.LogType.Info);

            string DisplayMessage = "";
            int progress = -1;


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
                UpdateProgbarFeedback(progress);
            }
            UpdateMessageFeedback(DisplayMessage);
        }

        #endregion
    }
}
