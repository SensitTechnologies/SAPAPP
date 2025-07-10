using SAPAPP.Configs;
using SAPAPP.Controllers;
using SAPAPP.Util;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static SAPAPP.Util.Logger;


namespace SAPAPP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region instanceVariables

        private readonly bool _BeyondStartup = false;
        private DownloadController DownloadController;
        private FirmwareConfigs configs = new();

        private static readonly string APP_DATA_FOLDER = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SAPAPP");
        private static readonly string PRODUCT_CONFIG_FILE = Path.Combine(APP_DATA_FOLDER, "FirmwareConfigurations.xml");
        private static readonly string PATH_CONFIG_FILE = Path.Combine(APP_DATA_FOLDER, "CLI_configs.json");
        private static readonly string LOGGER_FILE = Path.Combine(APP_DATA_FOLDER, "log.txt");

        /// <summary>
        /// This handles printing informational statements to help for debugging and provide extra
        /// information to the user.
        /// </summary>
        public Logger Logger { get; set; }

        /// <summary>
        /// The name of some folder in the ProductFolder path that contains the Part's specific firmware information
        /// </summary>
        public string SoftwareFolderLocation
        {
            get => configs.SoftwareFolderLocation;
            set
            {
                configs.SoftwareFolderLocation = value;
                configs.ConfigureFullPaths();
                Save_Firmware();
            }
        }

        /// <summary>
        /// This field stores and grabs the CLI integration for STM32 Cube Programmer
        /// </summary>
        public string STM32_PROGRAMMER_CLI
        {
            get => DownloadController.STM32_PROGRAMMER_CLI;
            set
            {
                DownloadController.STM32_PROGRAMMER_CLI = value;
                if (_BeyondStartup)
                {
                    Save_CLIs();
                }
            }
        }

        /// <summary>
        /// This field stores and grabs the CLI integration for avrdude
        /// </summary>
        public string AVRDUDE_CLI
        {
            get => DownloadController.AVRDUDE_CLI;
            set
            {
                DownloadController.AVRDUDE_CLI = value;
                if (_BeyondStartup)
                {
                    Save_CLIs();
                }
            }
        }

        /// <summary>
        /// This field stores and grabs the CLI integrations folder for TI Uniflash
        /// </summary>
        public string TI_UNIFLASH_FOLDER
        {
            get => DownloadController.TI_UNIFLASH_FOLDER;
            set
            {
                DownloadController.TI_UNIFLASH_FOLDER = value;
                if (_BeyondStartup)
                {
                    Save_CLIs();
                }
            }
        }


        #endregion

        /// <summary>
        /// Creates a new instance of the Main application window on app startup
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Logger = new(Log);
            if (File.Exists(LOGGER_FILE))
            {
                Logger.Log("Launching App", LogType.Info);
                Logger.Log("Initializing Startup", LogType.Info);
            }

            DownloadController = new DownloadController(Logger, UpdateFeedbackMessages, UpdateProgressBar);

            ConfigureOnStartup();
            Logger.Log("Startup Complete", LogType.Info);

            _BeyondStartup = true;
        }


        #region Scripts&Configs

        /// <summary>
        /// Specially configures serializable integrations on app startup. 
        /// </summary>
        private void ConfigureOnStartup()
        {

            if (!Directory.Exists(APP_DATA_FOLDER))
            {
                Directory.CreateDirectory(APP_DATA_FOLDER);
            }

            if (!File.Exists(LOGGER_FILE))
            {
                File.Create(LOGGER_FILE);
                Logger.Log("Launching App", LogType.Info);
                Logger.Log("Initializing Startup", LogType.Info);
            }

            // Load pathing configurations for the first time
            if (!File.Exists(PATH_CONFIG_FILE))
            {
                Save_CLIs();
            }
            Load_CLIs(PATH_CONFIG_FILE);

            // Load product configurations for the first time
            if (!File.Exists(PRODUCT_CONFIG_FILE))
            {
                Save_Firmware();
            }
            Load_Product_Configurations(PRODUCT_CONFIG_FILE);
        }

        /// <summary>
        /// Method reads dropdown and gets the selected product from the configuration
        /// </summary>
        /// <returns>The selected Product</returns>
        private Product Get_Current_Product()
        {
            Product currentProduct = new();
            foreach (Product product in configs.Products)
            {
                if (product.ProductName == ProductPicker.Text)
                {
                    currentProduct = product;
                    break;
                }
            }

            return currentProduct;
        }

        /// <summary>
        /// Uses selected product and Drop down menu selection to get the selected part to load
        /// </summary>
        /// <param name="product">The currently selected product</param>
        /// <returns>The currently selected Part</returns>
        private Part Get_Current_Part(Product product)
        {
            Part currentPart = new();
            foreach (Part part in product.Parts)
            {
                if (part.PartName == PartPicker.Text)
                {
                    currentPart = part;
                    break;
                }
            }
            return currentPart;
        }

        /// <summary>
        /// Loads a new list of products and their respective parts from a configuration file in the form of a filename 
        /// </summary>
        /// <param name="filename"></param>
        public void Load_Product_Configurations(string filename)
        {
            configs = Settings.Open_Firmware_Configs(filename);
            Logger.Log("Loaded Firmware Configurations from file: " + filename, LogType.Info);

            DropDownMenuController newContext = new(configs);
            if (filename != PRODUCT_CONFIG_FILE)
            {
                Settings.Save_Firmware_Configs(configs, PRODUCT_CONFIG_FILE);
                newContext.SelectedProduct = "---";
                newContext.SelectedPart = "---";
            }
            DataContext = newContext;

        }

        /// <summary>
        /// Loads the configuration file containing the pathways for the required applications for the function to function
        /// </summary>
        /// <param name="filename"></param>
        public void Load_CLIs(string filename)
        {
            if (File.Exists(filename))
            {
                Dictionary<string, string> selection = Settings.Load_Dictionary_Configs(filename);
                if (selection != null)
                {
                    STM32_PROGRAMMER_CLI = selection.TryGetValue("STM32", out string? value1) ? value1 : "";
                    AVRDUDE_CLI = selection.TryGetValue("AVRDUDE", out string? value2) ? value2 : "";
                    TI_UNIFLASH_FOLDER = selection.TryGetValue("UNIFLASH", out string? value3) ? value3 : "";
                }
                Logger.Log("Loaded Program integration configurations from file: " + filename, LogType.Info);
            }
            if (filename != PATH_CONFIG_FILE)
            {
                Save_CLIs();
            }
        }

        /// <summary>
        /// Saves the current firmware configurations to the xml file
        /// </summary>
        public void Save_Firmware()
        {
            Settings.Save_Firmware_Configs(configs, PRODUCT_CONFIG_FILE);
            Logger.Log("Saved Firmware Configurations to File: " + PRODUCT_CONFIG_FILE, LogType.Info);
        }

        /// <summary>
        /// Saves the configurations for the current pathways of the required applications for this software to function
        /// </summary>
        public void Save_CLIs()
        {
            Dictionary<string, string> selections = new()
            {
                { "STM32", STM32_PROGRAMMER_CLI },
                { "AVRDUDE", AVRDUDE_CLI },
                { "UNIFLASH", TI_UNIFLASH_FOLDER }
            };

            Settings.Save_Dictionary_Configs(selections, PATH_CONFIG_FILE);
            Logger.Log("Saved Program integration configurations to file: " + PATH_CONFIG_FILE, LogType.Info);
        }

        #endregion


        #region Buttons

        /// <summary>
        /// Method for when the start button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            SetButtonAppearance(StartButton, Brushes.Green, Brushes.White);
            SetButtonAppearance(StopButton, Brushes.White, Brushes.Black);
            ResetProgressBar();

            Product currentProduct = Get_Current_Product();
            Part currentPart = Get_Current_Part(currentProduct);

            DownloadController.StartRunning(currentPart);

            if (!DownloadController.AutomaticOn)
            {
                StartButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;

            SetButtonAppearance(StartButton, Brushes.White, Brushes.Black);
            SetButtonAppearance(StopButton, Brushes.Red, Brushes.White);

            DownloadController.StopRunning();

            if (!DownloadController.AutomaticOn)
            {
                StopButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="button"></param>
        /// <param name="background"></param>
        /// <param name="foreground"></param>
        private static void SetButtonAppearance(Button button, Brush background, Brush foreground)
        {
            button.Background = background;
            button.Foreground = foreground;
        }

        #endregion


        #region ToolbarMethods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseFile_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to close?",
                                                      "Confirm", MessageBoxButton.YesNo,
                                                      MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown(); // Closes the entire application
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Preferences_Click(object sender, RoutedEventArgs e)
        {
            StatusMessageDisplay.Text = "Preferences option selected";

            PreferencesDialog preferencesDialog = new(this,
                LOGGER_FILE, PRODUCT_CONFIG_FILE, SoftwareFolderLocation,
                STM32_PROGRAMMER_CLI, TI_UNIFLASH_FOLDER, AVRDUDE_CLI);
            preferencesDialog.ShowDialog();  // Opens it as a modal window
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Wiki_Click(object sender, RoutedEventArgs e)
        {
            StatusMessageDisplay.Text = "Wiki option selected";

            WikiDialog wikiDialog = new()
            {
                Owner = this  // Sets MainWindow as the owner
            };
            wikiDialog.ShowDialog();  // Opens it as a modal window
        }

        #endregion


        #region Feedback

        /// <summary>
        /// 
        /// </summary>
        private void ResetProgressBar()
        {
            progbar.Value = 0;
        }

        /// <summary>
        /// Writes a new log message to the log file
        /// </summary>
        /// <param name="message">The massage being written</param>
        /// <param name="level">The level of a message representing the type of message being logged</param>
        private void Log(string message, LogType level)
        {

            message = message.Trim();
            if (message == "")
            {
                return;
            }

            // format: time level message
            string time = DateTime.Now.ToString(new CultureInfo("en-US"));

            string info = "";
            switch (level)
            {
                case LogType.Info:
                    info = "<INF0>";
                    break;
                case LogType.Warn:
                    info = "<WARNING>";
                    break;
                case LogType.Error:
                    info = "<ERROR>";
                    break;
                case LogType.Pass:
                    info = "<PASS>";
                    break;
                case LogType.Fail:
                    info = "<FAIL>";
                    break;
            }

            string format = "{0} {1} {2}";
            using StreamWriter outputFile = new(LOGGER_FILE, true);
            outputFile.WriteLine(string.Format(format, time, info, message));
            outputFile.Close();
        }

        /// <summary>
        /// Writes a new message to the Message Display Box inside the Status Bar
        /// </summary>
        /// <param name="message">The message to be written</param>
        private void UpdateFeedbackMessages(string message)
        {
            if ((message != null) && (message != ""))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusMessageDisplay.Text = message;
                });
            }
        }

        /// <summary>
        /// Changes the progress display on the Status Bar
        /// </summary>
        /// <param name="progress">The current progress of an Download Process</param>
        private void UpdateProgressBar(double progress)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                progbar.Value = progress;
                progressPercentage.Text = progress.ToString() + '%';
            });
        }

        /// <summary>
        /// Changes the progress display on the Status Bar
        /// </summary>
        /// <param name="progress">The current progress of an Download Process</param>
        private void UpdateProgressBar(int progress)
        {
            UpdateProgressBar((double)progress);
        }

        #endregion


        #region MiscUIMethods

        /// <summary>
        /// Helps serves a click event for closing the overlay. 
        /// Sets the visibility of the overlay container to collapsed
        /// which serves as hiding the overlay altogether. 
        /// </summary>
        /// <param name="sender">
        /// Serves as the source of the event, without the need of a close overlay button
        /// </param>
        /// <param name="e">
        /// The event data associated with clicks on the MainWindow.
        /// </param>
        private void CloseOverlay_Click(object sender, RoutedEventArgs e)
        {
            OverlayContainer.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Serves in handling the click event for toggling the window in
        /// being in the state of always on top.
        /// The property of the Topmost is flipped, which helps in keeping the
        /// window visible at all times.
        /// </summary>
        /// <param name="sender">
        /// The source of an event without the need for a toggle button.
        /// </param>
        /// <param name="e">
        /// The event data associated with clicks and other functionality.
        /// </param>
        private void ToggleStayOnTop_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;
        }

        /// <summary>
        /// Helps in handling the click event for setting the window's font to
        /// small based upon a fixed value. 
        /// </summary>
        /// <param name="sender">
        /// The source of an event without the need for a toggle button.
        /// </param>
        /// <param name="e">
        /// The event data associated with clicks and other functionality.
        /// </param>
        private void FontSizeSmall_Click(object sender, RoutedEventArgs e)
        {
            this.FontSize = 12;
        }

        /// <summary>
        /// Helps in handling the click event for setting the window's font to
        /// medium based upon a fixed value. 
        /// </summary>
        /// <param name="sender">
        /// The source of an event without the need for a toggle button.
        /// </param>
        /// <param name="e">
        /// The event data associated with clicks and other functionality.
        /// </param>
        private void FontSizeMedium_Click(object sender, RoutedEventArgs e)
        {
            this.FontSize = 16;
        }

        /// <summary>
        /// Helps in handling the click event for setting the window's font to
        /// large based upon a fixed value. 
        /// </summary>
        /// <param name="sender">
        /// The source of an event without the need for a toggle button.
        /// </param>
        /// <param name="e">
        /// The event data associated with clicks and other functionality.
        /// </param>
        private void FontSizeLarge_Click(object sender, RoutedEventArgs e)
        {
            this.FontSize = 20;
        }

        /// <summary>
        /// Handles the window size change event through the dynamic 
        /// layout of the canvas items and other UI elements. 
        /// Modifies font sizes, button dimensions and other classifications
        /// based around the aspects of a switch in orientation of the application. 
        /// </summary>
        /// <param name="sender"></param>
        /// The source of an event without the need for a toggle button.
        /// <param name="e"></param>
        /// The event data associated with clicks and other functionality.
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double scaleFactor = e.NewSize.Width / 1366.0;

            ProductLabel.FontSize = 48 * scaleFactor;
            PartLabel.FontSize = 48 * scaleFactor;
            StartButton.FontSize = 40 * scaleFactor;
            StopButton.FontSize = 40 * scaleFactor;

            ProductPicker.Width = 600 * scaleFactor;
            PartPicker.Width = 600 * scaleFactor;

            // Adjust positioning dynamically
            Canvas.SetLeft(ProductLabel, 100 * scaleFactor);
            Canvas.SetTop(ProductLabel, 80 * scaleFactor);
            Canvas.SetLeft(PartLabel, 100 * scaleFactor);
            Canvas.SetTop(PartLabel, 240 * scaleFactor);
            Canvas.SetLeft(StartButton, 100 * scaleFactor);
            Canvas.SetTop(StartButton, 420 * scaleFactor);
            Canvas.SetLeft(StopButton, 400 * scaleFactor);
            Canvas.SetTop(StopButton, 420 * scaleFactor);

            if (GridBackground != null)
            {
                if (GridBackground.Background is ImageBrush bg)
                {
                    // Adjust background stretch based on orientation
                    bg.Stretch = e.NewSize.Width > e.NewSize.Height ? Stretch.UniformToFill : Stretch.Fill;
                }
            }
        }

        #endregion
    }
}