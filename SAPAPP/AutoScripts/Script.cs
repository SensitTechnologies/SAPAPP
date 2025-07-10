using SAPAPP.Configs;
using SAPAPP.Util;
using System.Diagnostics;

namespace SAPAPP.AutoScripts
{
    /// <summary>
    /// Basic abstract class defining the core functionality of a script for the purpose of this software
    /// </summary>
    public abstract class Script
    {
        /// <summary>
        /// Details whether the given script is able to be run in automatic mode
        /// </summary>
        /// <remarks>
        /// This variable is assumed to be true unless otherwise stated by a script's constructor
        /// </remarks>
        protected bool CapableAutomatic = true; // assume script can do automatic mode. if not modify script constructors until it's compatible
        
        /// <summary>
        /// Tells which specific Architecture each script is compatible with for comparison purposes
        /// </summary>
        public Architecture CompatibleArchitecture;

        /// <summary>
        /// Logger object each script uses to document feedback while it's being processed
        /// </summary>
        protected Logger Logger;

        private Action<string> UpdateMessageAction { get; set; } = (message) =>
        {
            Debug.WriteLine(message);
        };

        private Action<int> UpdateProgbarAction { get; set; } = (progress) =>
        {
            Debug.WriteLine(progress);
        };

        #region Constructors

        /// <summary>
        /// Creates a new Script object with the given parameters
        /// </summary>
        public Script()
        {
            Logger = new Logger();
        }

        /// <summary>
        /// Creates a new Script object with the given parameters
        /// </summary>
        /// <param name="logger">a Logger object created from the main window</param>
        public Script(Logger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Creates a new Script object with the given parameters
        /// </summary>
        /// <param name="logger">a Logger object created from the main window</param>
        /// <param name="updateFeedbackAction">a method from the main window for updating the feedback region of the UI</param>
        /// <param name="updateProgBarAction">a method from the main window for updating the progress bar region of the UI</param>
        public Script(Logger logger, Action<string> updateFeedbackAction, Action<int> updateProgBarAction) : this(logger)
        {
            UpdateMessageAction = updateFeedbackAction;
            UpdateProgbarAction = updateProgBarAction;
        }

        #endregion

        /// <summary>
        /// Tells whether a script is capable of being run in automatic mode
        /// </summary>
        /// <returns>A bool representing if the script is capable of being run in automatic mode</returns>
        public bool GetCapableAutomatic()
        {
            return CapableAutomatic;
        }

        #region Search, and Download algorithms

        /// <summary>
        /// Detects if a microcontroller compatible with the current script is connected to the computer
        /// </summary>
        /// <returns>Whether a board has been detected for the active script</returns>
        public abstract bool Detect();

        /// <summary>
        /// Downloads the current firmware configuration
        /// </summary>
        public abstract void Download(Part currentDownload);

        #endregion

        #region Feedback callbacks and Filtering

        /// <summary>
        /// Writes a new message to the Message Display Box inside the Status Bar
        /// </summary>
        /// <param name="message">The message to be written</param>
        public void UpdateMessageFeedback(string message)
        {
            UpdateMessageAction(message);
        }

        /// <summary>
        /// Changes the progress display on the Status Bar
        /// </summary>
        /// <param name="progress">The current progress of an Download Process</param>
        public void UpdateProgbarFeedback(int progress)
        {
            UpdateProgbarAction(progress);
        }

        /// <summary>
        /// Handles error cases by showing them to the user in ways they c
        /// </summary>
        /// <param name="line"></param>
        protected abstract void HandleError(string line);

        /// <summary>
        /// Handles output data by showing them to the user in ways they can understand
        /// </summary>
        /// <param name="data">A string of the data being processed</param>
        protected abstract void ProcessOutputData(string data);

        #endregion
    }
}
