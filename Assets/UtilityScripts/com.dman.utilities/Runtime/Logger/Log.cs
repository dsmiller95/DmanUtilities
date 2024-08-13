using UnityEngine;

namespace Dman.Utilities.Logger
{
    public static class Log
    {
        [HideInCallstack]
        public static void Warning(
            string message,
            Object context = null,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = ""
        )
        {
            var log = GetLogMessage(message, memberName, sourceFilePath);
            Debug.LogWarning(log, context);
        }
        
        [HideInCallstack]
        public static void Error(
            string message,
            Object context = null,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
        {
            var log = GetLogMessage(message, memberName, sourceFilePath);
            Debug.LogError(log, context);
        }

        [HideInCallstack]
        public static void Info(
            string message,
            Object context = null,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
        {
            var log = GetLogMessage(message, memberName, sourceFilePath);
            Debug.Log(log, context);
        }
        
        private static string GetLogMessage(string message, string memberName, string sourceFilePath)
        {
            var fileName = System.IO.Path.GetFileNameWithoutExtension(sourceFilePath);
            return $"{fileName}.{memberName}: {message}";
        }
    }
}