using SAPAPP.Configs;
using SAPAPP.Util;
using System.IO;
using static SAPAPP.Util.Logger;

namespace SAPAPP.Controllers
{
    public class ConfigController
    {

        /// <summary>
        /// The programs current configuration settings
        /// </summary>
        public FirmwareConfigs Configs { get; set; } = new();

        // Feedback Devices
        private readonly Logger Logger = new();

        private static string PRODUCT_CONFIG_FILE;
        private static string CONFIG_PATHWAYS_FILE;

        /// <summary>
        /// Config File most recently loaded from the cloud
        /// </summary>
        public string LastLoadedCloudFile { 
            get; 
            private set
            {
                field = value;
                SaveConfigLocations();
            } 
        }

        /// <summary>
        /// Controls all firmware configurations for the program
        /// </summary>
        /// <param name="logger">the program/s logger</param>
        /// <param name="productConfigFile">the file location for the local config file</param>
        /// <param name="configPathwaysFile">the file location for storing local config pathways</param>
        public ConfigController(Logger logger, string productConfigFile, string configPathwaysFile)
        {
            Logger = logger;
            PRODUCT_CONFIG_FILE = productConfigFile;
            CONFIG_PATHWAYS_FILE = configPathwaysFile;
            if (!File.Exists(CONFIG_PATHWAYS_FILE))
            {
                SaveConfigLocations();
            }
            LoadConfigLocations();
        }


        /// <summary>
        /// Saves the current config settings to the local config file
        /// </summary>
        public void SaveToConfigFile()
        {
            Settings.Save_Firmware_Configs(Configs, PRODUCT_CONFIG_FILE);
        }

        /// <summary>
        /// Loads a config file to be used
        /// </summary>
        /// <param name="filename">The name of the file being loaded</param>
        public void LoadConfigFile(string filename)
        {
            if (filename == PRODUCT_CONFIG_FILE)
            {
                var cloudfile = new FileInfo(LastLoadedCloudFile);
                var localfile = new FileInfo(PRODUCT_CONFIG_FILE);

                if ((cloudfile.Exists && localfile.Exists) && 
                    cloudfile.LastWriteTime > localfile.LastWriteTime)
                {
                    LoadConfigFile(LastLoadedCloudFile);
                    return;
                }
            }
            
            Configs = Settings.Open_Firmware_Configs(filename);
            Logger.Log("Loaded Firmware Configurations from file: " + filename, LogType.Info);


            if (filename != PRODUCT_CONFIG_FILE)
            {
                LastLoadedCloudFile = filename;
                SaveToConfigFile();
            }
        }

        private void SaveConfigLocations()
        {
            Dictionary<string, string> selection = new()
            {
                { "currentFileName", LastLoadedCloudFile }
            };

            Settings.Save_Dictionary_Configs(selection, CONFIG_PATHWAYS_FILE);
        }

        private void LoadConfigLocations()
        {
            if (File.Exists(CONFIG_PATHWAYS_FILE))
            {
                Dictionary<string, string> selection = Settings.Load_Dictionary_Configs(CONFIG_PATHWAYS_FILE);
                if (selection != null)
                {
                    var lastFile = selection.ContainsKey("currentFileName") ? selection["currentFileName"] : "---";
                    LastLoadedCloudFile = (lastFile == null || lastFile.Length < 1) ? "No File" : lastFile;                                  
                }
            }
        }
    }
}
