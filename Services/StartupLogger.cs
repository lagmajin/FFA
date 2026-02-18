using System;
using System.IO;

namespace FFA.Services
{
    // Simple startup and error logger that appends to App_Data/startup.log
    public class StartupLogger
    {
        private readonly string _logPath;
        public StartupLogger()
        {
            var appData = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            Directory.CreateDirectory(appData);
            _logPath = Path.Combine(appData, "startup.log");
        }

        public void LogInfo(string message)
        {
            try
            {
                File.AppendAllText(_logPath, $"[{DateTime.UtcNow:O}] INFO: {message}{Environment.NewLine}");
            }
            catch { }
        }

        public void LogException(Exception ex, string context = null)
        {
            try
            {
                var ctx = string.IsNullOrEmpty(context) ? "" : $"[{context}] ";
                File.AppendAllText(_logPath, $"[{DateTime.UtcNow:O}] ERROR: {ctx}{ex}\n\n");
            }
            catch { }
        }
    }
}
