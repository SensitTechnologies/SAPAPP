using System.Text;
using Path = System.IO.Path;


namespace SAPAPP.Configs
{

    public enum Architecture
    {
        STM32, ATMEGA, MSP430
    }


    [Serializable]
    public class Part
    {
        public string PartName { get; set; } = "---";
        public Architecture Architecture { get; set; }
        public string SoftwareFolderLocation { get; set; }
        public string ProductFolder { get; set; }
        public string FirmwareFolder { get; set; }
        public string CCXML_Config_File { get; set; }
        public string FirmwareFile { get; set; }

        public string FullPath()
        {
            string path = string.Empty;
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

        public string FullFirmwarePath()
        {
            return Path.Combine(FullPath(), FirmwareFile);
        }

        public string FullCCXMLPath()
        {
            return Path.Combine(FullPath(), CCXML_Config_File);
        }

        public new string ToString()
        {
            StringBuilder sb = new();
            sb.Append(PartName);
            return sb.ToString();
        }
    }

    [Serializable]
    public class Product
    {
        public string ProductName { get; set; } = "---";
        public string SoftwareFolderLocation { get; set; }
        public string ProductFolder { get; set; }
        public List<Part> Parts { get; set; } = [];
        public void configureFullPaths()
        {
            foreach (Part part in Parts)
            {
                part.ProductFolder = ProductFolder;
                part.SoftwareFolderLocation = SoftwareFolderLocation;
            }
        }

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

    [Serializable]
    public class FirmwareConfigs
    {
        public string SoftwareFolderLocation { get; set; }
        public List<Product> Products { get; set; } = [];
        public void configureFullPaths()
        {
            foreach (Product product in Products)
            {
                product.SoftwareFolderLocation = SoftwareFolderLocation;
                product.configureFullPaths();
            }
        }

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
