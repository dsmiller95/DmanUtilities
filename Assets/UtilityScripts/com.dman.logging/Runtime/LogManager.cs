using System;
using System.IO;
using UnityEngine;
using UnityEngine.Analytics;

namespace Dman.Logging
{
    /// <summary>
    /// When present, will listen for all logs and write them to file.
    /// Exceptions will include a truncated stack trace
    /// </summary>
    public class LogManager : MonoBehaviour
    {
        public string LogFolder = "logs";

        public int ExceptionStackTraceTruncateCount = 1000;

        private StreamWriter logWriter;

        private void Awake()
        {
            logWriter = OpenNewLogFile(LogFolder);
            Application.logMessageReceived += LogMessageReceived;
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= LogMessageReceived;
            logWriter?.Flush();
            logWriter?.Close();
            logWriter?.Dispose();
        }

        private static StreamWriter OpenNewLogFile(string relativeLogFolder)
        {
            var logFileName = GetLogFileName();
            var logFolder = Path.Combine(Application.persistentDataPath, relativeLogFolder);
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }
            var activeLogFilePath = Path.Combine(logFolder, logFileName);
            var logWriter = File.AppendText(activeLogFilePath);
            return logWriter;
        }

        private static string GetLogFileName()
        {
            var currentTime = DateTime.UtcNow;
            var uniqueID = AnalyticsSessionInfo.sessionId;

            return $"log_{currentTime}_{uniqueID}.txt";
        }


        private void LogMessageReceived(string condition, string stackTrace, LogType type)
        {
            var timestamp = DateTime.UtcNow;
            logWriter.Write($"{timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff")} | {LogCode(type)} | {condition}");

            if (type == LogType.Exception)
            {
                var truncatedStack = stackTrace.Substring(0, Math.Min(stackTrace.Length, ExceptionStackTraceTruncateCount));
                logWriter.Write($" | {truncatedStack}");
            }
            logWriter.WriteLine();
        }

        private string LogCode(LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                    return "ERR ";

                case LogType.Assert:
                    return "ASRT";

                case LogType.Warning:
                    return "WARN";

                case LogType.Log:
                    return "LOG ";

                case LogType.Exception:
                    return "EXCP";
                default:
                    return "UNKN";
            }
        }
    }
}
