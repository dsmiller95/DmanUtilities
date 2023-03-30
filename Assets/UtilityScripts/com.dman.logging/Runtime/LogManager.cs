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

        [Tooltip("Truncate stack traces to this length in log files. Only exceptions retain stack traces.")]
        public int ExceptionStackTraceTruncateCount = 1000;

        [Tooltip("Used in log file name and text")]
        public string TimeFormat = "yyyy-MM-dd HH.mm.ss.fff";

        private StreamWriter logWriter;

        private void Awake()
        {
            logWriter = OpenNewLogFile();
            Application.logMessageReceived += LogMessageReceived;

            Debug.Log($"LogManager: Log file created. " +
                $"Version [{Application.version}], " +
                $"isEditor [{Application.isEditor}], " +
                $"platform [{Application.platform}]");
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= LogMessageReceived;
            logWriter?.Flush();
            logWriter?.Close();
            logWriter?.Dispose();
        }

        private StreamWriter OpenNewLogFile()
        {
            var logFileName = GetLogFileName();
            var logFolder = Path.Combine(Application.persistentDataPath, LogFolder);
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }
            var activeLogFilePath = Path.Combine(logFolder, logFileName);
            var logWriter = File.AppendText(activeLogFilePath);
            return logWriter;
        }

        private string GetLogFileName()
        {
            var currentTime = DateTime.UtcNow.ToString(TimeFormat);
            var uniqueID = AnalyticsSessionInfo.sessionId;

            var editorCode = Application.isEditor ? "EDIT" : "PLAY";

            return $"log_{editorCode}_{currentTime}_{uniqueID:X16}.log";
        }


        private void LogMessageReceived(string condition, string stackTrace, LogType type)
        {
            var timestamp = DateTime.UtcNow;
            logWriter.Write($"{LogCode(type)}|{timestamp.ToString(TimeFormat)}|{condition}");

            if (type == LogType.Exception)
            {
                var truncatedStack = stackTrace.Substring(0, Math.Min(stackTrace.Length, ExceptionStackTraceTruncateCount));
                truncatedStack = ("\n" + truncatedStack)
                    .TrimEnd('\n')
                    .Replace("\n", "\nSTAK|");
                logWriter.Write(truncatedStack);
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
