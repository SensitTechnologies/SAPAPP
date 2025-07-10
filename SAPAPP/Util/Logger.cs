using System.Diagnostics;

namespace SAPAPP.Util
{
    /// <summary>
    /// Class functionality for logging messages and from the program into log files based on user actions
    /// </summary>
    public class Logger
    {

        /// <summary>
        /// Specifies different types of loggable messages.
        /// </summary>
        public enum LogType
        {
            /// <summary>
            /// Details information about a process
            /// </summary>
            Info, 

            /// <summary>
            /// Warnings to the user about what they are doing
            /// </summary>
            Warn, 

            /// <summary>
            /// When errors happen inside the program
            /// </summary>
            Error, 

            /// <summary>
            /// When a procedure passes
            /// </summary>
            Pass, 

            /// <summary>
            /// When a procedure fails
            /// </summary>
            Fail
        }

        /// <summary>
        /// Defines how the Logger should save the log. By default this will report to the debug terminal.
        /// </summary>
        private Action<string, LogType> LogAction { get; set; } = (message, type) =>
        {
            Debug.WriteLine(message);
        };

        /// <summary>
        /// The Logger is used to record messages and error levels for display of information
        /// </summary>
        public Logger()
        {

        }

        /// <summary>
        /// The Logger is used to record messages and error levels for display of information
        /// </summary>
        /// <param name="logAction">Defines how the Logger should save the log. By default this will report to the debug terminal.</param>
        public Logger(Action<string, LogType> logAction) : base()
        {
            LogAction = logAction;
        }

        /// <summary>
        /// Function used to write to the Logger
        /// </summary>
        /// <param name="message">Message to be written</param>
        /// <param name="level">The logging level/ level of severity of the message</param>
        public void Log(string message, LogType level)
        {
            LogAction(message, level);
        }
    }
}
