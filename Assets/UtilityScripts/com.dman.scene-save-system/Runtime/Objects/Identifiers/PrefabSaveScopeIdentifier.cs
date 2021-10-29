using System;

namespace Dman.SceneSaveSystem.Objects.Identifiers
{
    [Serializable]
    internal class PrefabSaveScopeIdentifier : ISaveScopeIdentifier
    {
        public int prefabTypeId;
        public string prefabParentId;

        public bool IsMarkerPrefab => prefabTypeId < 0;

        public string UniqueSemiReadableName => $"Prefab_Id_{prefabTypeId}_Parent_{prefabParentId}";

        public PrefabSaveScopeIdentifier(string parentId)
        {
            prefabTypeId = -1;
            prefabParentId = parentId;
        }
        public PrefabSaveScopeIdentifier(SaveablePrefabType type, string parentId)
        {
            prefabTypeId = type.myId;
            prefabParentId = parentId;
        }

        public bool Equals(ISaveScopeIdentifier other)
        {
            if (!(other is PrefabSaveScopeIdentifier casted))
            {
                return false;
            }
            return casted.prefabTypeId == prefabTypeId && casted.prefabParentId == prefabParentId;
        }
    }
}
