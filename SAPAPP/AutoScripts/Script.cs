using SAPAPP.Configs;
using System.Diagnostics;

namespace SAPAPP.AutoScripts
{
    internal abstract class Script
    {

        protected bool CapableAutomatic = true; // asume script can do automatic mode. if not modify sript constructors until it's compatible
        public Architecture CompatibleArchitecture;

        protected Logger logger;

        private Action<string> UpdateMessageAction { get; set; } = (message) =>
        {
            Debug.WriteLine(message);
        };

        private Action<int> UpdateProgbarAction { get; set; } = (progress) =>
        {
            Debug.WriteLine(progress);
        };

        #region Constructors

        public Script()
        {

        }

        public Script(Logger logger) : this()
        {
            this.logger = logger;
        }

        public Script(Logger logger, Action<string> updateFeedbackAction, Action<int> updateProgBarAction) : this(logger)
        {
            UpdateMessageAction = updateFeedbackAction;
            UpdateProgbarAction = updateProgBarAction;
        }

        #endregion
        
        public bool GetCapableAutomatic()
        {
            return CapableAutomatic;
        }


        #region Search, and Download algorithms

        public abstract bool Detect();
        public abstract void Download(Part currentDownload);

        #endregion

        #region Feedback callbacks and Filtering

        public void UpdateMessageFeedback(string message) => UpdateMessageAction(message);
        public void UpdateProgbarFeedback(int progress) => UpdateProgbarAction(progress);

        protected abstract void HandleError(string line);

        protected abstract void ProcessOutputData(string data);

        #endregion
    }
}
