using System;
using System.IO;

namespace Fouad.Services
{
    public static class LoggingService
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.log");

        public static void LogInfo(string message)
        {
            Log("INFO", message);
        }

        public static void LogWarning(string message)
        {
            Log("WARNING", message);
        }

        public static void LogError(string message, Exception? ex = null)
        {
            var fullMessage = ex != null ? $"{message} - Exception: {ex.Message}\nStack Trace: {ex.StackTrace}" : message;
            Log("ERROR", fullMessage);
        }

        private static void Log(string level, string message)
        {
            try
            {
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
            }
            catch
            {
                // Ignore logging errors to prevent infinite loops
            }
        }
    }
}