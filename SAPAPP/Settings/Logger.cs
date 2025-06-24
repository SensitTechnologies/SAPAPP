using System.Diagnostics;

namespace SAPAPP
{
    public class Logger
    {

        /// <summary>
        /// Specifies different types of loggable messages.
        /// </summary>
        public enum LogType
        {
            Info, Warn, Error, Pass, Fail
        }

        /// <summary>
        /// Defines how the logger should save the log. By default this will report to the debug terminal.
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
        /// <param name="logAction">Defines how the logger should save the log. By default this will report to the debug terminal.</param>
        public Logger(Action<string, LogType> logAction) : base()
        {
            LogAction = logAction;
        }

        /// <summary>
        /// Function used to write to the logger
        /// </summary>
        /// <param name="message">Message to be written</param>
        /// <param name="level">The logging level/ level of severity of the message</param>
        public void Log(string message, LogType level)
        {
            LogAction(message, level);
        }
    }
}
