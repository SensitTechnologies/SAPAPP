using SAPAPP.Configs;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace SAPAPP.AutoScripts
{
    internal class STMScript : Script
    {
        private string _stm32_prog_cli;
        public string STM32_Programmer_CLI
        {
            get { return _stm32_prog_cli; }
            set { _stm32_prog_cli = value; }
        }

        #region Constructors
        public STMScript() : base() 
        {
            CompatibleArchitecture = Architecture.STM32;
        }

        public STMScript(Logger logger) : base(logger)
        {
            CompatibleArchitecture = Architecture.STM32;
        }

        public STMScript(Logger logger, Action<string> updateFeedbackAction, Action<int> updateProgBarAction) : base(logger, updateFeedbackAction, updateProgBarAction)
        {
            CompatibleArchitecture = Architecture.STM32;
        }

        #endregion

        #region Search, and Download algorithms

        public override bool Detect()
        {
            if (!File.Exists(STM32_Programmer_CLI))
            {
                MessageBox.Show("Error: Configure STM32 Cube Programmer before you try to Download");
                return false;
            }

            string connect = "-c port=SWD";
            string strCmdText = $"\"{STM32_Programmer_CLI}\" {connect}";

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

        public override void Download(Part currentDownload)
        {
            //extra data needed
            string writeHead = "0x08000000";

            // basic commands
            string connect = "-c port=SWD";                                                 // connect to board command
            string write = $"-w {currentDownload.FullFirmwarePath()} {writeHead}";          // write command

            string strCmdText = $"\"{STM32_Programmer_CLI}\" {connect} {write}";
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

            cmd.OutputDataReceived += new DataReceivedEventHandler((sender, eventArgs) =>
            {
                if (eventArgs.Data != null)
                {
                    ProcessOutputData(eventArgs.Data);
                    /*
                    if (line.Contains("Error:"))
                    {
                        //e.Result = eventArgs.Data;
                        logger.Log(eventArgs.Data, Logger.LogType.Error);
                        HandleError(eventArgs.Data);
                    }
                    else
                    {
                        logger.Log(eventArgs.Data, Logger.LogType.Info);
                        //UpdateProgress(eventArgs.Data);
                    }
                    */
                }
            });
            cmd.ErrorDataReceived += new DataReceivedEventHandler((sender, eventArgs) =>
            {
                if (eventArgs.Data != null)
                {
                    ProcessOutputData(eventArgs.Data);
                    //e.Result = eventArgs.Data;
                    //logger.Log(eventArgs.Data, Logger.LogType.Error);
                    //HandleError(eventArgs.Data);
                }
            });

            cmd.Start();
            cmd.BeginErrorReadLine();
            cmd.BeginOutputReadLine();
            cmd.WaitForExit();
            cmd.Close();
        }

        #endregion

        #region Feedback Filtering

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
            if (line.Contains("Error"))
            {
                logger.Log(line, Logger.LogType.Error);
                HandleError(line);
            }
            else
            {
                logger.Log(line, Logger.LogType.Info);

                string DisplayMessage = "";
                int progress = -1;

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

                if (progress > 0)
                {
                    UpdateProgbarFeedback(progress);
                }
                UpdateMessageFeedback(DisplayMessage);
            }
        }

        #endregion
    }
}