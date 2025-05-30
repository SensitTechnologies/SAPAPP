﻿using SAPAPP.Configs;
using SAPAPP.Scripts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace SAPAPP
{
    public partial class MainWindow : Window
    {
        private TestScript TestScript;
        private FetScript FetScript;
        private MegaScript MegaScript;

        private FirmwareConfigs configs;

        public MainWindow()
        {
            InitializeComponent();
            //DataContext = new SelectionViewModel();
            InitializeScripts();

            configs = Settings.Settings.openConfigs(Settings.Settings.configFile);
            DataContext = new SelectionViewModel(configs);

        }

        private void InitializeScripts()
        {
            TestScript = new TestScript(StatusMessageDisplay, progressPercentage, progbar);
            FetScript = new FetScript(StatusMessageDisplay, progressPercentage, progbar);
            MegaScript = new MegaScript(StatusMessageDisplay, progressPercentage, progbar);
        }

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

        private void Preferences_Click(object sender, RoutedEventArgs e)
        {
            StatusMessageDisplay.Text = "Preferences option selected";

            PreferencesDialog preferencesDialog = new PreferencesDialog(this);
            preferencesDialog.ShowDialog();
        }

        private void Wiki_Click(object sender, RoutedEventArgs e)
        {
            WikiDialog wikiDialog = new WikiDialog();
            wikiDialog.ShowDialog();
        }

        private PCB Get_Current_PCB()
        {
            PCB currentPCB = new();
            foreach (PCB pcb in configs.PCBs)
            {
                if (pcb.PCBName == PCBPicker.Text)
                {
                    currentPCB = pcb;
                    break;
                }
            }
            return currentPCB;
        }

        private ProductConfig Get_Current_Product(PCB pcb)
        {
            ProductConfig currentProduct = new();
            foreach (ProductConfig product in pcb.Products)
            {
                if (product.ProductName == ProductPicker.Text)
                {
                    currentProduct = product;
                }
            }
            return currentProduct;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            SetButtonAppearance(StartButton, Brushes.White, Brushes.Black);
            SetButtonAppearance(StopButton, Brushes.Red, Brushes.White);


            PCB currentPCB = Get_Current_PCB();
            switch (currentPCB.PCBName)
            {
                case "---": TestScript.Cancel(); break;
                case "MSP430": FetScript.Cancel(); break;
                //case 2: MegaScript.Download(); break;
                default: break;
            }

            StatusMessageDisplay.Text = "Download Canceled";
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            SetButtonAppearance(StartButton, Brushes.Green, Brushes.White);
            SetButtonAppearance(StopButton, Brushes.White, Brushes.Black);
            ResetProgressBar();

            StatusMessageDisplay.Text = "Starting Download";

            PCB currentPCB = Get_Current_PCB();
            switch (currentPCB.PCBName)
            {
                case "---":     TestScript.Download(Get_Current_Product(currentPCB)); break;
                case "MSP430":  FetScript.Download(Get_Current_Product(currentPCB)); break;
                case "ATmega":  MegaScript.Download(Get_Current_Product(currentPCB)); break;
                default: break;

            }

            StartButton.IsEnabled = true;
        }


        private void SetButtonAppearance(Button button, Brush background, Brush foreground)
        {
            button.Background = background;
            button.Foreground = foreground;
        }

        private void ResetProgressBar()
        {
            progbar.IsIndeterminate = false;
            progbar.Value = 0;
        }

        private void ToggleDarkMode_Click(object sender, RoutedEventArgs e)
        {
            if (this.Background == Brushes.Black)
            {
                this.Background = Brushes.White;
                SetLightModeColors();
            }
            else
            {
                this.Background = Brushes.Black;
                SetDarkModeColors();
            }
        }

        private void SetLightModeColors()
        {
            StatusMessageDisplay.Foreground = Brushes.Black;
            progressPercentage.Foreground = Brushes.Black;
            progbar.Foreground = Brushes.Black;
            StartButton.Background = Brushes.LightGray;
            StopButton.Background = Brushes.LightGray;
        }

        private void SetDarkModeColors()
        {
            StatusMessageDisplay.Foreground = Brushes.White;
            progressPercentage.Foreground = Brushes.White;
            progbar.Foreground = Brushes.Green;
            StartButton.Background = Brushes.Gray;
            StopButton.Background = Brushes.Gray;
        }

        private void ToggleStayOnTop_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;
        }

        private void FontSizeSmall_Click(object sender, RoutedEventArgs e)
        {
            this.FontSize = 12;
        }

        private void FontSizeMedium_Click(object sender, RoutedEventArgs e)
        {
            this.FontSize = 16;
        }

        private void FontSizeLarge_Click(object sender, RoutedEventArgs e)
        {
            this.FontSize = 20;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void PCBPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }

    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application app = new Application();
            app.Run(new MainWindow());
        }
    }
}