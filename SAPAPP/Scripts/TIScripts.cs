using SAPAPP.Configs;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;

namespace SAPAPP.Scripts
{
    internal class TIScript : Script
    {
        private readonly SerialPort serialPort;
        private const int BaudRate = 115200;

        public TIScript(Logger lg, TextBlock fd, TextBlock pp, ProgressBar pb, string portName) : base(lg, fd, pp, pb)
        {
            serialPort = new SerialPort(portName, BaudRate);
            serialPort.DataReceived += SerialPort_DataReceived;
        }

        public void Initialize()
        {
            try
            {
                serialPort.Open();
                //LogMessage("Serial port opened successfully.");
            }
            catch (Exception ex)
            {
                //LogMessage($"Error opening serial port: {ex.Message}");
                Console.WriteLine($"Failed to open serial port: {ex.Message}");
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            char inByte = (char)serialPort.ReadByte();
            if (inByte == 'f')
            {
                //LogMessage("Firmware programming initiated.");
                backgroundWorker.RunWorkerAsync();
            }
        }

        public override void Download(Part download)
        {
            Initialize();

            if (!backgroundWorker.IsBusy)
            {
                backgroundWorker.RunWorkerAsync();
            }
        }

        protected override void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            string strCmdText = "";
            string firmwareDir = currentDownload.FirmwareFolder;

            ProcessStartInfo processStartInfo = new()
            {
                FileName = "cmd.exe",
                UseShellExecute = false,
                Arguments = testing ? "/k " + strCmdText : "/c " + strCmdText,
                RedirectStandardOutput = !testing,
                RedirectStandardError = !testing,
                CreateNoWindow = !testing,
                WorkingDirectory = firmwareDir
            };

            try
            {
                Process cmd = new() { StartInfo = processStartInfo };
                cmd.Start();
                cmd.WaitForExit();

                if (!testing)
                {
                    string line;
                    while (!cmd.StandardOutput.EndOfStream)
                    {
                        if (worker.CancellationPending)
                        {
                            e.Cancel = true;
                            break;
                        }

                        line = cmd.StandardError.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            HandleError(line);
                            break;
                        }

                        line = cmd.StandardOutput.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            Application.Current.Dispatcher.Invoke(() => Console.WriteLine(line));
                            Thread.Sleep(delay);
                        }
                    }
                }

                cmd.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        protected override void HandleError(string line)
        {
            string message, header;
            line = line.Trim();

            if (line.Contains("system cannot find the path specified", StringComparison.CurrentCultureIgnoreCase))
            {
                message = line + "\nFirmware file missing or incorrectly named.";
                header = "Firmware File Error";
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
            throw new NotImplementedException();
        }
    }
}