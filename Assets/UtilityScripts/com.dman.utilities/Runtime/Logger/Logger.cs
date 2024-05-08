using UnityEngine;

namespace Dman.Utilities
{
    public static class Logger
    {
        public static void Log(this MonoBehaviour sourceScope, string log)
        {
            var sourceType = sourceScope.GetType();
            Debug.Log(sourceType.Name + ": " + log, sourceScope);
        }
    }
}
