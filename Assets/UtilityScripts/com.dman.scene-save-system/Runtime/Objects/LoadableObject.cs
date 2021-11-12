using Dman.SceneSaveSystem.Objects.Identifiers;
using System;
using System.Linq;
using UnityEngine;

namespace Dman.SceneSaveSystem.Objects
{
    internal interface ILoadableObject
    {
        public int LoadOrder { get; }
        public void LoadDataIn(SaveablePrefabRegistry prefabRegistry);
    }

    internal class BasicLoadableObject: ILoadableObject
    {
        ISaveableData saveDataTarget;
        SaveTreeContext currentContext;
        SaveScopeData currentScope;

        public BasicLoadableObject(ISaveableData saveableData, SaveTreeContext currentContext, SaveScopeData currentScope)
        {
            this.saveDataTarget = saveableData;
            this.currentContext = currentContext;
            this.currentScope = currentScope;
        }

        public int LoadOrder => saveDataTarget.LoadOrderPriority;

        public void LoadDataIn(SaveablePrefabRegistry prefabRegistry)
        {
            // current scope can be null if loading a scene which has not been saved before, and
            //  this object is not in the global scope
            if (currentScope == null)
            {
                return;
            }
            if (currentScope.DataInScopeDictionary.TryGetValue(saveDataTarget.UniqueSaveIdentifier, out var saveData))
            {
                saveDataTarget.SetupFromSaveObject(saveData.savedSerializableObject);
            }
        }
    }

    internal class PrefabParentLoadable : ILoadableObject
    {
        SaveablePrefabParent prefabParent;
        SaveTreeContext currentContext;
        SaveScopeData currentScope;
        SaveScopeData globalScope;

        public PrefabParentLoadable(SaveablePrefabParent prefabParent, SaveTreeContext currentContext, SaveScopeData currentScope, SaveScopeData globalScope)
        {
            this.prefabParent = prefabParent;
            this.currentContext = currentContext;
            this.currentScope = currentScope;
            this.globalScope = globalScope;
        }

        public int LoadOrder => 1000;

        public void LoadDataIn(SaveablePrefabRegistry prefabRegistry)
        {
            // current scope can be null if loading a scene which has not been saved before, and
            //  this object is not in the global scope
            if(currentScope == null)
            {
                return;
            }
            var childScopesForPrefab = currentScope.childScopes
                .Where(x =>
                    (x.scopeIdentifier is PrefabSaveScopeIdentifier prefabIdentifier) &&
                    prefabIdentifier.prefabParentId == prefabParent.prefabParentName
                ).ToList();
            if (childScopesForPrefab.Count <= 0)
            {
                // this prefab parent was not saved. do nothing, and allow any default children
                //  in the scene to stick around
                return;
            }
            foreach (Transform transform in prefabParent.gameObject.transform)
            {
                if (transform.GetComponent<SaveablePrefab>())
                {
                    GameObject.Destroy(transform.gameObject);
                }
            }
            foreach (var childScopeData in childScopesForPrefab)
            {
                var prefabIdentifier = childScopeData.scopeIdentifier as PrefabSaveScopeIdentifier;
                if (prefabIdentifier.IsMarkerPrefab)
                {
                    continue;
                }
                var prefab = prefabRegistry.GetUniqueObjectFromID(prefabIdentifier.prefabTypeId);
                if (prefab == null)
                {
                    Debug.LogError($"No prefab found for prefab ID {prefabIdentifier.prefabTypeId}, did the prefab configuration change since the last save?", prefabParent);
                    throw new System.Exception("Bad prefab fomat");
                }
                var newInstance = GameObject.Instantiate(prefab.prefab, prefabParent.transform);
                WorldSaveManager.LoadDataInsideScope(
                    newInstance.transform,
                    currentContext,
                    childScopeData,
                    globalScope,
                    prefabRegistry);
            }
        }
    }
}
