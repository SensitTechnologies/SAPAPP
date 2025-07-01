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
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Log Files (*.txt)|*.txt"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                LogTextBox.Text = openFileDialog.FileName;
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
        private void BrowseConfig_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Config Files (XML-File)|*.xml",
                Title = "Open Config Settings",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            };
            if (openFileDialog.ShowDialog() == true)
            {
                ConfigTextBox.Text = openFileDialog.FileName;
            }
        }

        private void BrowseSharePoint_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFileDialog = new OpenFolderDialog
            {
                Title = "Select Fet Tools Folder",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SharepointSelectionBox.Text = openFileDialog.FolderName;
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
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select STM32 Programmer (STM32_Programmer_CLI.exe)",
                Filter = "Executable Files (*.exe)|*.exe",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                STM32PathTextBox.Text = openFileDialog.FileName;
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
        private void BrowseMSP430_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select Uniflash dslite.bat",
                InitialDirectory = Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles))
            };

            if (openFileDialog.ShowDialog() == true)
            {
                MessageBox.Show(Path.GetDirectoryName(openFileDialog.FileName));
                UniflashTextBox.Text = openFileDialog.FileName;
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
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select ATmega Programmer (atprogram.exe)",
                Filter = "Executable Files (*.exe)|*.exe",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ATmegaPathTextBox.Text = openFileDialog.FileName;
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

            if (ConfigTextBox.Text != "")
            {
                parentWindow.Load_Product_Configurations(ConfigTextBox.Text);
            }
            if (SharepointSelectionBox.Text != "")
            {
                parentWindow.SharePointLocation = SharepointSelectionBox.Text;
            }
            if (STM32PathTextBox.Text != "")
            {
                parentWindow.STM32_PROGRAMMER_CLI = STM32PathTextBox.Text;
            }

            if (UniflashTextBox.Text != "")
            {
                parentWindow.TI_UNIFLASH_FOLDER = Path.GetDirectoryName(UniflashTextBox.Text);
            }

            if (ATmegaPathTextBox.Text != "")
            {
                parentWindow.AVRDUDE_CLI = ATmegaPathTextBox.Text;
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