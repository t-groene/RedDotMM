
using System;
using System.Diagnostics;
using System.IO;

namespace RedDotMM.Logging
{
    public enum LogType
    {
        Info,
        Warnung,
        Fehler
    }

    public class Logger
    {
        private static readonly Logger instance = new Logger();
        private readonly string logFilePath;

        // Privater Konstruktor für Singleton
        private Logger()
        {
            string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            logFilePath = Path.Combine(logDirectory, "RedDotMM-application.log");
        }

        public static Logger Instance => instance;

        public void Log(string message, LogType logType)
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logType}] {message}";
            Debug.WriteLine(logEntry); // Optional: Konsolenausgabe für Debugging
            try
            {
                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Optional: Fehler beim Schreiben ins Log behandeln
                Console.WriteLine($"Fehler beim Schreiben ins Log: {ex.Message}");
            }
        }
    }
}
