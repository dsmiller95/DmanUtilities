using UnityEngine;

namespace Dman.Utilities.Logger
{
    public static class Log
    {
#if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
#endif
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
        
#if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
#endif
        public static void Error(
            string message,
            Object context = null,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
        {
            var log = GetLogMessage(message, memberName, sourceFilePath);
            Debug.LogError(log, context);
        }

#if UNITY_2021_3_OR_NEWER
        [HideInCallstack]
#endif
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