using System.Text;
using Path = System.IO.Path;


namespace SAPAPP.Configs
{

    /// <summary>
    /// The type of Architecture that some firmware flash uses
    /// </summary>
    public enum Architecture
    {
        /// <summary>
        /// Configure for STM32 based microcontrollers
        /// </summary>
        STM32,

        /// <summary>
        /// Configure for AVR based microcontrollers
        /// </summary>
        ATMEGA,

        /// <summary>
        /// Configure for MSP430 based microcontrollers
        /// </summary>
        MSP430
    }

    /// <summary>
    /// A Serializable class representing a configuration for some part that has firmware
    /// </summary>
    [Serializable]
    public class Part
    {
        /// <summary>
        /// The name of some Part that requires firmware
        /// </summary>
        public string PartName { get; set; } = "---";

        /// <summary>
        /// The type of Architecture that the Part uses
        /// </summary>
        public Architecture Architecture { get; set; }

        /// <summary>
        /// general location where all firmware configs are stored
        /// </summary>
        public string SoftwareFolderLocation { get; set; } = "";

        /// <summary>
        /// The name of some folder in the SoftwareFolderLocation path that contains the Part's specific ProductFolder
        /// </summary>
        public string ProductFolder { get; set; } = "";

        /// <summary>
        /// The name of some folder in the ProductFolder path that contains the Part's specific firmware information
        /// </summary>
        public string FirmwareFolder { get; set; } = "";

        /// <summary>
        /// File name of a specific ccxml file to be used for an MSP430
        /// </summary>
        public string CCXML_Config_File { get; set; } = "";

        /// <summary>
        /// The name of some file containing firmware information for the product.
        /// </summary>
        public string FirmwareFile { get; set; } = "";

        /// <summary>
        /// Gets the full folder path of a specific part configuration
        /// </summary>
        /// <returns>a file path pointing to a folder that contains the part firmware</returns>
        public string FullPath()
        {
            string path;
            if (string.IsNullOrEmpty(FirmwareFolder))
            {
                path = Path.Combine(SoftwareFolderLocation, ProductFolder);
            }
            else
            {
                path = Path.Combine(SoftwareFolderLocation, ProductFolder, FirmwareFolder);
            }
            return path;
        }

        /// <summary>
        /// Gets the full firmware path of a specific part configuration
        /// </summary>
        /// <returns>a file path pointing to configured firmware file</returns>
        public string FullFirmwarePath()
        {
            return Path.Combine(FullPath(), FirmwareFile);
        }

        /// <summary>
        /// Gets the full firmware path of a specific part configuration
        /// </summary>
        /// <returns>a file path pointing to configured firmware file</returns>
        public string FullCCXMLPath()
        {
            return Path.Combine(FullPath(), CCXML_Config_File);
        }

        /// <inheritdoc/>
        public new string ToString()
        {
            StringBuilder sb = new();
            sb.Append(PartName);
            return sb.ToString();
        }
    }

    /// <summary>
    /// A Serializable class representing a configuration for some Product that has Parts that have firmware
    /// </summary>
    [Serializable]
    public class Product
    {
        /// <summary>
        /// Name of a given product derived from the deserialized xml file
        /// </summary>
        public string ProductName { get; set; } = "---";

        /// <summary>
        /// general location where all firmware configs are stored
        /// </summary>
        public string SoftwareFolderLocation { get; set; } = "";

        /// <summary>
        /// The name of some folder in the SoftwareFolderLocation path that contains the Part's specific ProductFolder
        /// </summary>
        public string ProductFolder { get; set; } = "";

        /// <summary>
        /// The list of different parts of a product that need software
        /// </summary>
        public List<Part> Parts { get; set; } = [];

        /// <summary>
        /// configures generalized pathing information to the more specific Parts Configuration
        /// </summary>
        public void ConfigureFullPaths()
        {
            foreach (Part part in Parts)
            {
                part.ProductFolder = ProductFolder;
                part.SoftwareFolderLocation = SoftwareFolderLocation;
            }
        }

        /// <summary>
        /// Sorts the object's list of Parts in Alphabetical order by Part Name
        /// </summary>
        public void Sort()
        {
            Parts.Sort(delegate (Part x, Part y)
            {
                if (x.PartName == null && y.PartName == null) return 0;
                else if (x.PartName == null) return -1;
                else if (y.PartName == null) return 1;
                else return x.PartName.CompareTo(y.PartName);
            });
        }

        /// <inheritdoc/>
        public new string ToString()
        {
            StringBuilder sb = new();

            sb.Append(ProductName); sb.Append(": ");
            foreach (Part part in Parts)
            {
                sb.Append(part.ToString());
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// A Serializable class containing a list configurations for Specific Products and their firmware
    /// </summary>
    [Serializable]
    public class FirmwareConfigs
    {
        /// <summary>
        /// general location where all firmware configs are stored
        /// </summary>
        public string SoftwareFolderLocation
        {
            get => field;
            set
            {
                field = value;
                ConfigureFullPaths();
            }
        } = "";

        /// <summary>
        /// Stored list of products based on the deserialized xml file
        /// </summary>
        public List<Product> Products { get; set; } = [];

        /// <summary>
        /// configures generalized pathing information to the more specific Products Configuration
        /// </summary>
        public void ConfigureFullPaths()
        {
            foreach (Product product in Products)
            {
                product.SoftwareFolderLocation = SoftwareFolderLocation;
                product.ConfigureFullPaths();
            }
        }

        /// <summary>
        /// Sorts the object's list of Products in Alphabetical order by Product Name
        /// </summary>
        public void Sort()
        {

            foreach (Product product in Products)
            {
                product.Sort();
            }

            Products.Sort(delegate (Product x, Product y)
            {
                if (x.ProductName == null && y.ProductName == null) return 0;
                else if (x.ProductName == null) return -1;
                else if (y.ProductName == null) return 1;
                else return x.ProductName.CompareTo(y.ProductName);
            });
        }
       
        /// <inheritdoc/>
        public new string ToString()
        {
            StringBuilder sb = new();
            foreach (Product product in Products)
            {
                sb.Append(product.ToString());
                sb.Append('\n');
            }
            return sb.ToString();
        }
    }
}
