using SAPAPP.Configs;
using SAPAPP.Scripts;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static SAPAPP.Logger;


namespace SAPAPP
{
    public partial class MainWindow : Window
    {
        #region instanceVariables

        private TestScript TestScript;
        private FetScript FetScript;
        private MegaScript MegaScript;
        private STMScript STMScript;

        private FirmwareConfigs configs = new();

        private static string APP_DATA_FOLDER = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SAPAPP");
        private static string PRODUCT_CONFIG_FILE = Path.Combine(APP_DATA_FOLDER, "FirmwareConfigurations.xml");
        private static string PATH_CONFIG_FILE = Path.Combine(APP_DATA_FOLDER, "CLI_configs.json");
        private static string LOGGER_FILE = Path.Combine(APP_DATA_FOLDER, "log.txt");

        /// <summary>
        /// This handles printing informational statements to help for debugging and provide extra
        /// information to the user.
        /// </summary>
        public Logger logger { get; set; }

        private string _SharePointLocation;
        public string SharePointLocation
        {
            get { return _SharePointLocation; }
            set
            {
                _SharePointLocation = value;
                configs.DriveLocation = value;
                Save_Firmware();
            }
        }

        private string _AVRDUDE_CLI;
        public string AVRDUDE_CLI
        {
            get => _AVRDUDE_CLI;
            set
            {
                _AVRDUDE_CLI = value;
                MegaScript.AVRDUDE_CLI = value;
                Save_CLIs();
            }
        }

        private string _STM32_Programmer_CLI;
        public string STM32_Programmer_CLI
        {
            get => _STM32_Programmer_CLI;
            set
            {
                _STM32_Programmer_CLI = value;
                STMScript.STM32_Programmer_CLI = value;
                Save_CLIs();
            }
        }

        private string _fetTools;
        public string FetTools
        {
            get => _fetTools;
            set
            {
                _fetTools = value;
                FetScript.ToolsFolder = value;
                Save_CLIs();
            }
        }


        #endregion

        public MainWindow()
        {
            InitializeComponent();
            logger = new(Log);
            if (File.Exists(LOGGER_FILE))
            {
                logger.Log("Launching App", LogType.Info);
                logger.Log("Initializing Startup", LogType.Info);
            }

            InitializeScripts();
            ConfigureOnStartup();
            logger.Log("Startup Complete", LogType.Info);
        }


        #region Scripts&Configs

        private void ConfigureOnStartup()
        {

            if (!Directory.Exists(APP_DATA_FOLDER))
            {
                Directory.CreateDirectory(APP_DATA_FOLDER);
            }

            if (!File.Exists(LOGGER_FILE))
            {
                File.Create(LOGGER_FILE);
                logger.Log("Launching App", LogType.Info);
                logger.Log("Initializing Startup", LogType.Info);
            }

            // Load pathing configurations for the first time
            Dictionary<string, string> selections = new Dictionary<string, string>();
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
        /// Initializes all loader scripts
        /// </summary>
        private void InitializeScripts()
        {
            TestScript = new TestScript(logger, StatusMessageDisplay, progressPercentage, progbar);
            FetScript = new FetScript(logger, StatusMessageDisplay, progressPercentage, progbar);
            MegaScript = new MegaScript(logger, StatusMessageDisplay, progressPercentage, progbar);
            STMScript = new STMScript(logger, StatusMessageDisplay, progressPercentage, progbar);
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
            logger.Log("Loaded Firmware Configurations from file: " + filename, LogType.Info);

            SelectionViewModel newContext = new SelectionViewModel(configs);
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
                    STM32_Programmer_CLI = selection.TryGetValue("STM32", out string? value1) ? value1 : "";
                    AVRDUDE_CLI = selection.TryGetValue("AVRDUDE", out string? value2) ? value2 : "";
                    FetTools = selection.TryGetValue("FETTOOLS", out string? value3) ? value3 : "";
                }
                logger.Log("Loaded Program integration configurations from file: " + filename, LogType.Info);
            }
            if (filename != PATH_CONFIG_FILE)
            {
                Save_CLIs();
            }
        }

        public void Save_Firmware()
        {
            Settings.Save_Firmware_Configs(configs, PRODUCT_CONFIG_FILE);
            logger.Log("Saved Firmware Configurations to File: " + PRODUCT_CONFIG_FILE, LogType.Info);
        }

        /// <summary>
        /// Saves the configurations for the current pathways of the required applications for this software to function
        /// </summary>
        public void Save_CLIs()
        {
            Dictionary<string, string> selections = new()
            {
                { "STM32", STM32_Programmer_CLI },
                { "AVRDUDE", AVRDUDE_CLI },
                { "FETTOOLS", FetTools }
            };

            Settings.Save_Dictionary_Configs(selections, PATH_CONFIG_FILE);
            logger.Log("Saved Program integration configurations to file: " + PATH_CONFIG_FILE, LogType.Info);
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
            SetButtonAppearance(StartButton, Brushes.Green, Brushes.White);
            SetButtonAppearance(StopButton, Brushes.White, Brushes.Black);
            ResetProgressBar();

            StatusMessageDisplay.Text = "Starting Download";

            Product currentProduct = Get_Current_Product();
            Part currentPart = Get_Current_Part(currentProduct);

            switch (currentPart.Architecture.ToLower())
            {
                //case "---": TestScript.Download(currentPart); break;
                //case "msp430": FetScript.Download(currentPart); break;
                case "atmega": MegaScript.Download(currentPart); break;
                case "stm32": STMScript.Download(currentPart); break;
                //case "laird": break;
                //case "fuel gauge": break;
                default: break;

            }

            StartButton.IsEnabled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            SetButtonAppearance(StartButton, Brushes.White, Brushes.Black);
            SetButtonAppearance(StopButton, Brushes.Red, Brushes.White);


            Product currentProduct = Get_Current_Product();
            Part currentPart = Get_Current_Part(currentProduct);
            switch (currentPart.Architecture)
            {
                case "---": TestScript.Cancel(); break;
                case "MSP430": FetScript.Cancel(); break;
                case "ATmega": MegaScript.Cancel(); break;
                case "STM32": STMScript.Cancel(); break;
                default: break;
            }

            StatusMessageDisplay.Text = "Download Canceled";
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

            PreferencesDialog preferencesDialog = new(this)
            {
                Owner = this  // Sets MainWindow as the owner
            };
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
            progbar.IsIndeterminate = false;
            progbar.Value = 0;
        }


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
            using (StreamWriter outputFile = new StreamWriter(LOGGER_FILE, true))
            {
                outputFile.WriteLine(string.Format(format, time, info, message));
                outputFile.Close();
            }
        }

        #endregion


        #region MiscUIMethods

        /// <summary>
        /// Helps serves a click event for closing the overlay. 
        /// Sets the visibility of the overlay container to collapsed
        /// which serves as hiding the overlay altogether. 
        /// </summary>
        /// <param name="sender"></param>
        /// Serves as the source of the event, without the need of a close overlay button
        /// <param name="e"></param>
        /// The event data associated with clicks on the MainWindow.
        /// </summary>
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
        /// <param name="sender"></param>
        /// The source of an event without the need for a toggle button.
        /// <param name="e"></param>
        /// The event data associated with clicks and other functionality.
        /// </summary>
        private void ToggleStayOnTop_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;
        }

        /// <summary>
        /// Helps in handling the click event for setting the window's font to
        /// small based upon a fixed value. 
        /// </summary>
        /// <param name="sender"></param>
        /// The source of an event without the need for a toggle button.
        /// <param name="e"></param>
        /// The event data associated with clicks and other functionality.
        /// </summary>
        private void FontSizeSmall_Click(object sender, RoutedEventArgs e)
        {
            this.FontSize = 12;
        }

        /// <summary>
        /// Helps in handling the click event for setting the window's font to
        /// medium based upon a fixed value. 
        /// </summary>
        /// <param name="sender"></param>
        /// The source of an event without the need for a toggle button.
        /// <param name="e"></param>
        /// The event data associated with clicks and other functionality.
        /// </summary>
        private void FontSizeMedium_Click(object sender, RoutedEventArgs e)
        {
            this.FontSize = 16;
        }

        /// <summary>
        /// Helps in handling the click event for setting the window's font to
        /// large based upon a fixed value. 
        /// </summary>
        /// <param name="sender"></param>
        /// The source of an event without the need for a toggle button.
        /// <param name="e"></param>
        /// The event data associated with clicks and other functionality.
        /// </summary>
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

    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application app = new();
            app.Run(new MainWindow());
        }
    }
}