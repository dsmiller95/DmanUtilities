using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dman.SceneSaveSystem
{
    [Serializable]
    public class GlobalSaveScopeIdentifier : ISaveScopeIdentifier
    {
        public string UniqueSemiReadableName => $"Globals";

        public bool Equals(ISaveScopeIdentifier other)
        {
            return other is GlobalSaveScopeIdentifier;
        }
    }
}
