using System;

namespace Dman.SceneSaveSystem.Objects.Identifiers
{
    internal interface ISaveScopeIdentifier : IEquatable<ISaveScopeIdentifier>
    {
        public string UniqueSemiReadableName { get; }
    }
}
