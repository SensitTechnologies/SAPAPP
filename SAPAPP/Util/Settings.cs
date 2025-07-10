using SAPAPP.Configs;
using System.IO;
using System.Windows;

namespace SAPAPP.Util
{

    /// <summary>
    /// Class to allow editing of settings objects by user
    /// </summary>
	public static class Settings
    {
        /// <summary>
        /// Deserializes an XML file and returns a configuration object to change the data context of the main window's drop down selectors
        /// </summary>
        /// <param name="filename">the xml file containing some configuration</param>
        /// <returns>an object representing Configuration loaded</returns>
        public static FirmwareConfigs Open_Firmware_Configs(string filename)
        {
            FirmwareConfigs configs = new();
            if (!string.IsNullOrEmpty(filename))
            {
                try
                {
                    configs = Load_XML<FirmwareConfigs>(filename);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            configs.Sort();
            return configs;
        }

        /// <summary>
        /// Saves a firmware configuration to a specified firmware file
        /// </summary>
        /// <param name="configs">the configuration object to be serialized</param>
        /// <param name="filename">the file to save the configuration to</param>
        public static void Save_Firmware_Configs(FirmwareConfigs configs, string filename)
        {
            Save_XML(configs, filename);
        }

        /// <summary>
        /// Loads Dictionary configurations from a specified JSON file
        /// </summary>
        /// <param name="filename">the name of the file to be deserialized</param>
        /// <returns>a dictionary object containing the information from the JSON file</returns>
        public static Dictionary<string, string> Load_Dictionary_Configs(string filename)
        {
            return Load_JSON<Dictionary<string, string>>(filename);
        }

        /// <summary>
        /// Saves a dictionary configuration to a specified JSON file
        /// </summary>
        /// <param name="configs">the configurations to be serialized</param>
        /// <param name="filename">the file to save the configuration to</param>
        public static void Save_Dictionary_Configs(Dictionary<string, string> configs, string filename)
        {
            Save_JSON(configs, filename);
        }

        private static T Load_JSON<T>(string filepath)
        {
            // Start with a null settings class.
            T settings = default;

            // If the file exists...
            if (File.Exists(filepath))
            {
                // Retrieve settings from file into the object.
                settings = Serializer.DeserializeJson<T>(filepath);
            }

            // If the file didn't exist or otherwise couldn't be deserialized,
            // put default values into the settings object.
            settings ??= Activator.CreateInstance<T>();

            return settings;
        }

        /// <summary>
        /// Retrieve an object of the specified type from the specified settings file
        /// </summary>
        /// <typeparam name="T">the type the settings object</typeparam>
        /// <param name="filepath">path to XML file where settings are stored</param>
        /// <returns>an object containing settings from the file</returns>
        private static T Load_XML<T>(string filepath)
        {
            // Start with a null settings class.
            T settings = default;

            // If the file exists...
            if (File.Exists(filepath))
            {
                // Retrieve settings from file into the object.
                settings = Serializer.DeserializeXML<T>(filepath);
            }

            // If the file didn't exist or otherwise couldn't be deserialized,
            // put default values into the settings object.
            settings ??= Activator.CreateInstance<T>();

            return settings;
        }

        private static void Save_JSON<T>(T settings, string filepath)
        {
            // Save teh settings as a JSON file.
            Serializer.SerializeJson(settings, filepath);
        }

        /// <summary>
        /// Save the object of the specified type to the specified local settings file
        /// </summary>
        /// <typeparam name="T">the type the settings object</typeparam>
        /// <param name="settings">Object to be serialized to XML</param>
        /// <param name="filepath">path to XML file where settings are stored</param>
        private static void Save_XML<T>(T settings, string filepath)
        {
            // Save the settings as an XML file.
            Serializer.SerializeXML(settings, filepath);
        }

        #region HelperMethods

        /// <summary>
        /// Get the full path to an XML file where settings are stored.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="fileName">name of the file (not including the extension)</param>
        /// <returns>full file path</returns>
        public static string GetFilePath(string directory, string fileName)
        {
            // Remove the file extension if one exists.
            // But preserve path name if it exists.
            fileName = Path.ChangeExtension(fileName, null);

            return Path.Combine(directory, fileName.Replace(' ', '_') + ".xml");
        }

        #endregion
    }
}