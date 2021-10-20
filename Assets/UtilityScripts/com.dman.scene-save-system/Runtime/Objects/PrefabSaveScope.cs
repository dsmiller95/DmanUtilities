﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dman.SceneSaveSystem
{
    [Serializable]
    public class PrefabSaveScopeIdentifier : ISaveScopeIdentifier
    {
        public int prefabTypeId;
        public string prefabParentId;

        public string UniqueSemiReadableName => $"Prefab_Id_{prefabTypeId}_Parent_{prefabParentId}";

        public PrefabSaveScopeIdentifier(SaveablePrefabType type, string parentId)
        {
            prefabTypeId = type.myId;
            prefabParentId = parentId;
        }

        public bool Equals(ISaveScopeIdentifier other)
        {
            if(!(other is PrefabSaveScopeIdentifier casted))
            {
                return false;
            }
            return casted.prefabTypeId == prefabTypeId && casted.prefabParentId == prefabParentId;
        }
    }
}
