using SAPAPP.Configs;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace SAPAPP.AutoScripts
{
    internal class MSP430Script : Script
    {
        private string _uniflashfolder;

        /// <summary>
        /// This field stores and grabs the CLI integrations folder for TI Uniflash
        /// </summary>
        public string TI_UNIFLASH_FOLDER
        {
            get { return _uniflashfolder; }
            set { _uniflashfolder = value; }
        }

        #region Contructors 

        public MSP430Script() : base()
        {
            CompatibleArchitecture = Architecture.MSP430;
            //CapableAutomatic = false;
        }

        public MSP430Script(Logger logger) : base(logger)
        {
            CompatibleArchitecture = Architecture.MSP430;
            //CapableAutomatic = false;
        }

        public MSP430Script(Logger logger, Action<string> updateFeedbackAction, Action<int> updateProgBarAction) : base(logger, updateFeedbackAction, updateProgBarAction)
        {
            CompatibleArchitecture = Architecture.MSP430;
            //CapableAutomatic = false;
        }

        #endregion

        #region Search, and Download algorithms

        public override bool Detect()
        {
            if (!Directory.Exists(TI_UNIFLASH_FOLDER))
            {
                MessageBox.Show("Error: Configure MSP430 Tools before you try to Download");
            }

            string strCmdText = "detect-devices.bat";

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
                    WorkingDirectory = TI_UNIFLASH_FOLDER
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

            if (
                results.ToString().Contains("Error: No device detected") ||
                results.ToString().Contains("Error: no power") ||
                results.ToString().Contains("Could not open the device")
                )
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
            if (!Directory.Exists(TI_UNIFLASH_FOLDER))
            {
                MessageBox.Show("Error: Configure MSP430 Tools before you try to Download");
                return;
            }

            string strCmdText = $"dslite.bat -c \"{currentDownload.FullCCXMLPath()}\" -s VerifyAfterProgramLoad=\"No verification\" -e -f -v \"{currentDownload.FullFirmwarePath()}\"";

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
                    WorkingDirectory = TI_UNIFLASH_FOLDER
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
                    HandleError(eventArgs.Data);
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
            //Cancel();
        }

        protected override void ProcessOutputData(string data)
        {
            string line = data.Trim();
            logger.Log(line, Logger.LogType.Info);

            string DisplayMessage = "";
            int progress = -1;


            string[] words = line.Split(' ');
            if (line.Contains("Finished"))
            {
                progress = 100;
            }
            else if (line.Contains('%'))
            {
                progress = int.Parse(words[^1].Trim('%'));
            }


            if (
                line.Contains("Configuring") ||
                line.Contains("Initializing") ||
                line.Contains("Connecting") ||
                line.Contains("Loading") ||
                line.Contains("Verifying")
                )
            {
                DisplayMessage = line;
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
