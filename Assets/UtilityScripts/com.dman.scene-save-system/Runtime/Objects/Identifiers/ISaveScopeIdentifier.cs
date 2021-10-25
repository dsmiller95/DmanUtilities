using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dman.SceneSaveSystem.Objects.Identifiers
{
    internal interface ISaveScopeIdentifier : IEquatable<ISaveScopeIdentifier>
    {
        public string UniqueSemiReadableName { get; }
    }
}
