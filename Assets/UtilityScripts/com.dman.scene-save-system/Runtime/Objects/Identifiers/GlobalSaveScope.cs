using System;

namespace Dman.SceneSaveSystem.Objects.Identifiers
{
    [Serializable]
    internal class GlobalSaveScopeIdentifier : ISaveScopeIdentifier
    {
        public string UniqueSemiReadableName => $"Globals";

        public bool Equals(ISaveScopeIdentifier other)
        {
            return other is GlobalSaveScopeIdentifier;
        }
    }
}
