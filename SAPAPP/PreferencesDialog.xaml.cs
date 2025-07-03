using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace SAPAPP
{

    /// <summary>
    /// Window designed for managing and configuring settings
    /// that are related to the implementation of products
    /// and various scripts, providing a useful and easy
    /// application for operators to follow.
    /// </summary>
    public partial class PreferencesDialog : Window
    {
        MainWindow parentWindow;

        string log_location = "";
        string Firmware_Config_Location = "";
        string Software_Folder_Location = "";
        string STM32_Location = "";
        string Uniflash_Location = "";
        string AVRDUDE_Location = "";

        /// <summary>
        /// Represents a dialog window that can be useful
        /// for managing different user preferences and
        /// allowing configuration settings to be adjusted
        /// based on the level of need and use case.
        /// </summary>
        /// <param name="parentWindow"></param>
        /// Highlights the way the main window serves as a
        /// parent to the child window of the preference dialog.
        /// </summary>
        public PreferencesDialog(MainWindow parentWindow)
        {
            InitializeComponent();
            this.parentWindow = parentWindow;

        }

        public PreferencesDialog(MainWindow parentWindow,
            string log_location, string firmware_config_location, string software_folder_location, string STM32_focation, string uniflash_location, string AVRDUDE_location)
            : this(parentWindow)
        {
            this.log_location = log_location;
            this.Firmware_Config_Location = firmware_config_location;
            this.Software_Folder_Location = software_folder_location;
            this.STM32_Location = STM32_focation;
            this.Uniflash_Location = uniflash_location;
            this.AVRDUDE_Location = AVRDUDE_location;

            ConfigureTextBoxes();
        }


        private void ConfigureTextBoxes()
        {
            Log_Location_TextBox.Text = log_location;
            Firmware_Config_Location_TextBox.Text = Firmware_Config_Location;
            Software_Folder_Location_TextBox.Text = Software_Folder_Location;
            STM32_Location_TextBox.Text = STM32_Location;
            Uniflash_Location_TextBox.Text = Uniflash_Location;
            AVRDUDE_Location_TextBox.Text = AVRDUDE_Location;
        }


        /// <summary>
        /// Handles the different levels of a click event for browsing
        /// and selecting a log file.
        /// Opens a file dialog window that can allow the user to choose
        /// a text-based log file or .txt, and upload it with the selected
        /// file type and path.
        /// </summary>
        /// <param name="sender"></param>
        /// Serves as the source of an click event through the Browse Log button.
        /// <param name="e"></param>
        /// Whereas this serves as the event data that is associated with a
        /// click.
        /// </summary>
        private void BrowseLog_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Log Files (*.txt)|*.txt"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                Log_Location_TextBox.Text = openFileDialog.FileName;
            }
        }

        /// <summary>
        /// Handles the different levels of a click event for browsing
        /// and selecting a configuration file.
        /// Opens a file dialog window that can allow the user to choose
        /// a XML-based config file, and upload it with the selected
        /// file type and path.
        /// </summary>
        /// <param name="sender"></param>
        /// Serves as the source of an click event through the Browse Config button.
        /// <param name="e"></param>
        /// Whereas this serves as the event data that is associated with a
        /// click.
        /// </summary>
        private void BrowseFirmwareConfig_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Config Files (XML-File)|*.xml",
                Title = "Open Firmware Configs",
                FileName = Firmware_Config_Location_TextBox.Text,
                InitialDirectory = Firmware_Config_Location_TextBox.Text == "" ?
                        Environment.GetFolderPath(Environment.SpecialFolder.Recent) :
                        Path.GetDirectoryName(Firmware_Config_Location_TextBox.Text),
            };

            if (openFileDialog.ShowDialog() == true)
            {
                Firmware_Config_Location_TextBox.Text = openFileDialog.FileName;
            }
        }

        private void BrowseSoftwareFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new()
            {
                Title = "Select Firmware Folder",
                FolderName = Software_Folder_Location_TextBox.Text,
                InitialDirectory = Software_Folder_Location_TextBox.Text == "" ?
                        Environment.GetFolderPath(Environment.SpecialFolder.Recent) :
                        Path.GetDirectoryName(Software_Folder_Location_TextBox.Text),
            };

            if (openFolderDialog.ShowDialog() == true)
            {
                Software_Folder_Location_TextBox.Text = openFolderDialog.FolderName;
            }
        }

        /// <summary>
        /// Handles the different levels of a click event for browsing
        /// and selecting the STM32 programmer executable file.
        /// Opens a file dialog window that can allow the user to choose
        /// the STM32 programmer tool, and upload it with the selected
        /// executable file and path.
        /// </summary>
        /// <param name="sender"></param>
        /// Serves as the source of an click event through the Browse STM32 button.
        /// <param name="e"></param>
        /// Whereas this serves as the event data that is associated with a
        /// click.
        /// </summary>
        private void BrowseSTM32_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "Select STM32 Programmer (STM32_Programmer_CLI.exe)",
                Filter = "Executable Files (*.exe)|*.exe",
                FileName = STM32_Location_TextBox.Text,
                InitialDirectory = STM32_Location_TextBox.Text == "" ?
                        Environment.GetFolderPath(Environment.SpecialFolder.Recent) :
                        Path.GetDirectoryName(STM32_Location_TextBox.Text),
            };

            if (openFileDialog.ShowDialog() == true)
            {
                STM32_Location_TextBox.Text = openFileDialog.FileName;
            }
        }

        /// <summary>
        /// Handles the different levels of a click event for browsing
        /// and selecting the Fet Batch Script file.
        /// Opens a file dialog window that can allow the user to choose
        /// the Fet Batch Script, and upload it with the selected
        /// executable file and path.
        /// </summary>
        /// <param name="sender"></param>
        /// Serves as the source of an click event through the Browse STM32 button.
        /// <param name="e"></param>
        /// Whereas this serves as the event data that is associated with a
        /// click.
        /// </summary>
        private void BrowseUniflashLocation_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new()
            {
                Title = "Select Uniflash Install Folder (uniflash_x.x.x)",
                Multiselect = false,
                FolderName = Uniflash_Location_TextBox.Text,
                InitialDirectory = Uniflash_Location_TextBox.Text == "" ?
                        Environment.GetFolderPath(Environment.SpecialFolder.Recent) :
                        Path.GetDirectoryName(Uniflash_Location_TextBox.Text),
            };

            if (openFolderDialog.ShowDialog() == true)
            {
                Uniflash_Location_TextBox.Text = openFolderDialog.FolderName;
            }
        }

        /// <summary>
        /// Handles the different levels of a click event for browsing
        /// and selecting the ATmega programmer executable file.
        /// Opens a file dialog window that can allow the user to choose
        /// the ATmega programmer tool, and upload it with the selected
        /// executable file and path.
        /// </summary>
        /// <param name="sender"></param>
        /// Serves as the source of an click event through the Browse ATmega button.
        /// <param name="e"></param>
        /// Whereas this serves as the event data that is associated with a
        /// click.
        /// </summary>
        private void BrowseATmega_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "Select ATmega Programmer (atprogram.exe)",
                Filter = "Executable Files (*.exe)|*.exe",
                FileName = AVRDUDE_Location_TextBox.Text,
                InitialDirectory = AVRDUDE_Location_TextBox.Text == "" ?
                        Environment.GetFolderPath(Environment.SpecialFolder.Recent) :
                        Path.GetDirectoryName(AVRDUDE_Location_TextBox.Text),
            };

            if (openFileDialog.ShowDialog() == true)
            {
                AVRDUDE_Location_TextBox.Text = openFileDialog.FileName;
            }
        }

        /// <summary>
        /// Serves as the way in which the OK button has a click event,
        /// which serves as a dialog result and updates the configuration
        /// paths through the apply functionality of pushing it to the
        /// application, as well as assigning paths.
        /// </summary>
        /// <param name="sender"></param>
        /// Serves as the source of a click event through the OK button.
        /// <param name="e"></param>
        /// This serves as the event data that is associated with a click.
        /// </summary>
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;

            if (Firmware_Config_Location_TextBox.Text != Firmware_Config_Location)
            {
                parentWindow.Load_Product_Configurations(Firmware_Config_Location_TextBox.Text);
            }

            if (Software_Folder_Location_TextBox.Text != Software_Folder_Location)
            {
                parentWindow.SoftwareFolderLocation = Software_Folder_Location_TextBox.Text;
            }

            if (STM32_Location_TextBox.Text != STM32_Location)
            {
                parentWindow.STM32_PROGRAMMER_CLI = STM32_Location_TextBox.Text;
            }

            if (Uniflash_Location_TextBox.Text != Uniflash_Location)
            {
                parentWindow.TI_UNIFLASH_FOLDER = Uniflash_Location_TextBox.Text;
            }

            if (AVRDUDE_Location_TextBox.Text != AVRDUDE_Location)
            {
                parentWindow.AVRDUDE_CLI = AVRDUDE_Location_TextBox.Text;
            }

            this.Close();
        }

        /// <summary>
        /// Handles the click event for the Cancel button, setting the aspect
        /// of a dialog result to be set to false and close the window.
        /// </summary>
        /// <param name="sender"></param>
        /// Serves as the source of an event, as the cancel functionality.
        /// <param name="e"></param>
        /// The event data that is associated with each cancel button click. 
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}