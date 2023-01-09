using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
