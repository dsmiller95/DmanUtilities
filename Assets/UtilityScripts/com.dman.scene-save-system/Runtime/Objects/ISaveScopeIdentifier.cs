using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dman.SceneSaveSystem
{
    public interface ISaveScopeIdentifier : IEquatable<ISaveScopeIdentifier>
    {
        public string UniqueSemiReadableName { get; }
    }
}
