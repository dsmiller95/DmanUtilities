using Dman.SceneSaveSystem.Objects.Identifiers;
using System;
using System.Linq;
using UnityEngine;

namespace Dman.SceneSaveSystem.Objects
{
    internal interface ILoadableObject
    {
        public int LoadOrder { get; }
        public void LoadDataIn();
    }

    internal class BasicLoadableObject: ILoadableObject
    {
        ISaveableData saveDataTarget;
        LoadIterationContext currentContext;

        public BasicLoadableObject(ISaveableData saveableData, LoadIterationContext currentContext)
        {
            this.saveDataTarget = saveableData;
            this.currentContext = currentContext;
        }

        public int LoadOrder => saveDataTarget.LoadOrderPriority;

        public void LoadDataIn()
        {
            // current scope can be null if loading a scene which has not been saved before, and
            //  this object is not in the global scope
            if (currentContext.currentScope == null)
            {
                return;
            }
            if (currentContext.currentScope.DataInScopeDictionary.TryGetValue(saveDataTarget.UniqueSaveIdentifier, out var saveData))
            {
                saveDataTarget.SetupFromSaveObject(saveData.savedSerializableObject);
            }
        }
    }

    internal class PrefabParentLoadable : ILoadableObject
    {
        SaveablePrefabParent prefabParent;
        LoadIterationContext currentContext;

        public PrefabParentLoadable(SaveablePrefabParent prefabParent, LoadIterationContext currentContext)
        {
            this.prefabParent = prefabParent;
            this.currentContext = currentContext;
        }

        public int LoadOrder => 1000;

        public void LoadDataIn()
        {
            // current scope can be null if loading a scene which has not been saved before, and
            //  this object is not in the global scope
            if(currentContext.currentScope == null)
            {
                return;
            }
            var childScopesForPrefab = currentContext.currentScope.childScopes
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
                var prefab = currentContext.prefabRegistry.GetUniqueObjectFromID(prefabIdentifier.prefabTypeId);
                if (prefab == null)
                {
                    Debug.LogError($"No prefab found for prefab ID {prefabIdentifier.prefabTypeId}, did the prefab configuration change since the last save?", prefabParent);
                    throw new System.Exception("Bad prefab fomat");
                }
                var newInstance = GameObject.Instantiate(prefab.prefab, prefabParent.transform);


                var allLoadables = WorldSaveManager.ExtractLoadableObjectsFromScope(
                        newInstance.transform,
                        currentContext.ForChildScope(childScopeData))
                    .OrderBy(x => x.LoadOrder)
                    .ToList();

                foreach (var loadable in allLoadables)
                {
                    loadable.LoadDataIn();
                }
            }
        }
    }
}
