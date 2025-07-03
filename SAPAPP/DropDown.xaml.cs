using SAPAPP.Configs;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

/// <summary>
/// ViewModel has the ability of managing product and part
/// selections.
/// Maintains a list of available products and parts, along with a
/// mapping of file paths and structures between them.
/// Helps in handling of selection updates, dialog notifications of 
/// changes and persists selections within a JSON file.
/// </summary>

namespace SAPAPP
{
    public class SelectionViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<string> ProductsList { get; set; }
        public ObservableCollection<string> PartsList { get; set; }
        public Dictionary<string, List<string>> ProductPartMap { get; set; }
        private static string selections_save_File = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SAPAPP", "selection.json");

        private string _selectedProduct;
        public string SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged(nameof(SelectedProduct));
                UpdatePartOptions();
                SaveSelection();
            }
        }

        private string _selectedPart;
        public string SelectedPart
        {
            get => _selectedPart;
            set
            {
                _selectedPart = value;
                OnPropertyChanged(nameof(SelectedPart));
                SaveSelection();
            }
        }

        /// <summary>
        /// Initializations of new instances of the SelectionViewModel
        /// class, and in setting up both product and part lists based on
        /// the instances of provided firmware configurations through
        /// ensuring a default selection. 
        /// </summary>
        /// <param name="config"></param>
        /// The firmware configuration is conveyed in containing both product
        /// and part details used in the initialization of selections. 
        /// </param>
        public SelectionViewModel(FirmwareConfigs config)
        {
            ProductsList = [];
            PartsList = [];
            ProductPartMap = [];


            Product defaultView = new();
            defaultView.Parts.Add(new Part());
            List<string> parts = [defaultView.Parts[0].PartName];
            ProductsList.Add(defaultView.ProductName);
            ProductPartMap.Add(defaultView.ProductName, parts);


            foreach (Product product in config.Products)
            {
                ProductsList.Add(product.ProductName);
                List<string> PartNames = [];
                foreach (Part part in product.Parts)
                {
                    PartNames.Add(part.PartName);
                }

                if (ProductPartMap.ContainsKey(product.ProductName))
                {
                    foreach (string PartName in PartNames)
                    {
                        ProductPartMap[product.ProductName].Add(PartName);
                    }
                }
                else
                {
                    ProductPartMap.Add(product.ProductName, PartNames);
                }
            }

            LoadSelection();
        }

        /// <summary>
        /// Serves as an updater for a list of available parts based on the
        /// currently selected product.
        /// Has the ability to clear the existing parts list in adding in a 
        /// default placeholder, and updates it with the parts that are 
        /// associated with the selection.
        /// Resets the selected part and product to the default value.
        /// </summary>
        private void UpdatePartOptions()
        {
            PartsList.Clear();
            PartsList.Add("---");
            if (!string.IsNullOrEmpty(SelectedProduct) && ProductPartMap.ContainsKey(SelectedProduct))
            {
                foreach (var part in ProductPartMap[SelectedProduct])
                {
                    PartsList.Add(part);
                }
            }
            SelectedPart = "---"; // Reset Part selection when product changes
        }

        /// <summary>
        /// Serves as a save function of what product and part are currently selected
        /// onto a JSON file.
        /// Serializes the selection data and writes it to the specified file path. 
        /// </summary>
        private void SaveSelection()
        {
            Dictionary<string, string> selection = new()
            {
                { "Product", SelectedProduct },
                { "Part", SelectedPart }
            };

            Settings.Save_Dictionary_Configs(selection, selections_save_File);
        }

        /// <summary>
        /// Loads the previously saved product and part selection from a JSON file. 
        /// If the file exists, it deserializes the selection from the data and updates
        /// the selected product and part accordingly from there. 
        /// </summary>
        private void LoadSelection()
        {
            if (File.Exists(selections_save_File))
            {
                Dictionary<string, string> selection = Settings.Load_Dictionary_Configs(selections_save_File);
                if (selection != null)
                {
                    SelectedProduct = selection.ContainsKey("Product") ? selection["Product"] : "---";
                    UpdatePartOptions();
                    SelectedPart = selection.ContainsKey("Part") ? selection["Part"] : "---";
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies users of a change in the property of the selection process
        /// and invokes the PropertyChanged event with a specified property name.
        /// </summary>
        /// <param name="propertyName"></param>
        /// Serves as the process of the property of the name change. 
        /// </param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}