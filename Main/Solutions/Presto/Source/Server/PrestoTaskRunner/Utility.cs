using System.Configuration;
using Xanico.Core;

namespace PrestoTaskRunner
{
    /// <summary>
    /// Helper class for the PTR assembly
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Sets the Logger.Source property to the processName value from the app.config.
        /// </summary>
        public static void SetLoggerSource()
        {
            Logger.Source = ConfigurationManager.AppSettings["processName"];
        }
    }
}
