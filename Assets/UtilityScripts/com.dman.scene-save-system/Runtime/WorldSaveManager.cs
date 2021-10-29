﻿using Dman.SceneSaveSystem.Objects;
using Dman.SceneSaveSystem.Objects.Identifiers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dman.SceneSaveSystem
{
    public class WorldSaveManager : MonoBehaviour
    {
        public SaveablePrefabRegistry saveablePrefabRegistry;
        public string gameobjectSaveRootFileName = "objects.dat";

        private static readonly string LastSavedSceneSaveDataId = "Internal_SaveData_LastSavedScene";

        public static void DeleteSaveData()
        {
            SerializationManager.DeleteAll(SaveContext.instance.saveName);
        }

        public void SaveActiveScene()
        {
            this.Save(SceneManager.GetActiveScene());
        }

        /// <summary>
        /// Save all ISaveableData in the scene, but not a child of a SaveablePrefab.
        /// then find all saveablePrefabs, and get their identifying information and all the information of the 
        ///     components inside each prefab. save all of these as a list
        /// </summary>
        public void Save(Scene sceneToSave)
        {
            var saveScopes = GetTopLevelSaveScopeData(sceneToSave);
            var globalScope = saveScopes.FirstOrDefault(x => x.scopeIdentifier is GlobalSaveScopeIdentifier);
            var sceneScope = saveScopes.FirstOrDefault(x => x.scopeIdentifier is SceneSaveScopeIdentifier);

            SaveScopeData oldGlobalScope;
            try
            {
                oldGlobalScope = SerializationManager.Load<SaveScopeData>(globalScope.scopeIdentifier, SaveContext.instance.saveName);
            }
            catch
            {
                oldGlobalScope = null;
            }

            if (oldGlobalScope == null)
            {
                oldGlobalScope = globalScope;
            }
            else
            {
                oldGlobalScope.OverwriteWith(globalScope);
            }

            var lastSceneSaved = (sceneScope.scopeIdentifier as SceneSaveScopeIdentifier);
            var lastSavedSceneSaveData = new SaveData
            {
                uniqueSaveDataId = LastSavedSceneSaveDataId,
                savedSerializableObject = lastSceneSaved
            };
            oldGlobalScope.InsertSaveData(lastSavedSceneSaveData);


            SaveSystemHooks.TriggerPreSave();
            SerializationManager.Save(sceneScope, SaveContext.instance.saveName);
            SerializationManager.Save(oldGlobalScope, SaveContext.instance.saveName);
            SaveSystemHooks.TriggerPostSave();
        }

        /// <summary>
        /// unload the active scene, and wait for the scene at <paramref name="scenePath"/> to be loaded.
        /// </summary>
        public void Load(string scenePath)
        {
            StartCoroutine(LoadCoroutine(scenePath));
        }
        /// <summary>
        /// loads the scene last saved in the current save name. Use this when, for example, you are loading a whole game
        ///     from a menu screen.
        /// Same as Load(), but with null scene Path. useful for binding to unity events.
        /// </summary>
        public void LoadLastSavedScene()
        {
            StartCoroutine(LoadCoroutine(null));
        }

        /// <summary>
        /// Load the scene at the specified path. if null, then load the scene last saved
        /// </summary>
        /// <param name="scenePath"></param>
        /// <returns></returns>
        public IEnumerator LoadCoroutine(string scenePath = null)
        {
            SaveSystemHooks.TriggerPreLoad();
            DontDestroyOnLoad(gameObject);

            var globalScope = new GlobalSaveScopeIdentifier();
            var globalData = SerializationManager.Load<SaveScopeData>(globalScope, SaveContext.instance.saveName);

            if (scenePath == null)
            {
                if (globalData.DataInScopeDictionary.TryGetValue(LastSavedSceneSaveDataId, out var saveData) && saveData.savedSerializableObject is SceneSaveScopeIdentifier savedScene)
                {
                    scenePath = savedScene.scenePath;
                }
                else
                {
                    Debug.LogError("Cannot load, invalid save format or no save data.");
                    throw new SaveFormatException("Bad save format. No last saved scene flag");
                }
            }

            var sceneIndexToLoad = SceneUtility.GetBuildIndexByScenePath(scenePath);
            Scene loadingScene;
            if (sceneIndexToLoad >= 0)
            {
                loadingScene = SceneManager.LoadScene(sceneIndexToLoad, new LoadSceneParameters(LoadSceneMode.Single));
            }
            else
            {
                var sceneNameToLoad = SceneNameFromPath(scenePath);
                loadingScene = SceneManager.LoadScene(sceneNameToLoad, new LoadSceneParameters(LoadSceneMode.Single));
            }
            yield return new WaitUntil(() => loadingScene.isLoaded);


            LoadIntoSingleScene(loadingScene, saveablePrefabRegistry, globalData);
            SaveSystemHooks.TriggerPostLoad();
            yield return null;
            Destroy(gameObject);
        }
        private static string SceneNameFromPath(string path)
        {
            return System.IO.Path.GetFileNameWithoutExtension(path);
        }


        private static void LoadIntoSingleScene(Scene scene, SaveablePrefabRegistry saveablePrefabRegistry, SaveScopeData globalData)
        {
            var sceneScopeIdentity = new SceneSaveScopeIdentifier(scene);

            var sceneData = SerializationManager.Load<SaveScopeData>(sceneScopeIdentity, SaveContext.instance.saveName);

            foreach (var go in scene.GetRootGameObjects())
            {
                LoadRecurse(go, sceneData, globalData, saveablePrefabRegistry, new SaveTreeContext());
            }

        }

        private static void LoadRecurse(GameObject iterationPoint, SaveScopeData currentScope, SaveScopeData globalScope, SaveablePrefabRegistry prefabRegistry, SaveTreeContext treeContext)
        {
            if (iterationPoint.GetComponent<GlobalSaveFlag>() != null && !treeContext.isGlobal)
            {
                if (currentScope != null && !(currentScope.scopeIdentifier is SceneSaveScopeIdentifier))
                {
                    Debug.LogError("Global save flag detected on a prefab game object instantiated outside of global scope. Either remove the global flag on the prefab, or make sure that it is instantiated under a parent in the global scope", iterationPoint);
                    throw new SaveFormatException("Bad save scene format");
                }
                treeContext.isGlobal = true;
                LoadRecurse(iterationPoint, globalScope, globalScope, prefabRegistry, treeContext);
                return;
            }

            if (currentScope != null) // don't try to load any data if there is no current scope. wait until global scope is encountered
            {
                var saveables = iterationPoint.GetComponents<ISaveableData>();
                foreach (var saveable in saveables)
                {
                    if (currentScope.DataInScopeDictionary.TryGetValue(saveable.UniqueSaveIdentifier, out var saveData))
                    {
                        saveable.SetupFromSaveObject(saveData.savedSerializableObject);
                    }
                }

                var prefabParent = iterationPoint.GetComponent<SaveablePrefabParent>();
                if (prefabParent != null)
                {
                    foreach (Transform transform in prefabParent.gameObject.transform)
                    {
                        if (transform.GetComponent<SaveablePrefab>())
                        {
                            GameObject.Destroy(transform.gameObject);
                        }
                    }
                    foreach (var childScopeData in currentScope.childScopes)
                    {
                        if (!(childScopeData.scopeIdentifier is PrefabSaveScopeIdentifier prefabIdentifier) ||
                            prefabIdentifier.prefabParentId != prefabParent.prefabParentName ||
                            prefabIdentifier.IsMarkerPrefab)
                        {
                            continue;
                        }
                        var prefab = prefabRegistry.GetUniqueObjectFromID(prefabIdentifier.prefabTypeId);
                        if (prefab == null)
                        {
                            Debug.LogError($"No prefab found for prefab ID {prefabIdentifier.prefabTypeId}, did the prefab configuration change since the last save?", iterationPoint);
                            throw new System.Exception("Bad prefab fomat");
                        }
                        var newInstance = Instantiate(prefab.prefab, prefabParent.transform);
                        LoadRecurse(newInstance.gameObject, childScopeData, globalScope, prefabRegistry, treeContext);
                    }
                    return;
                }
            }


            foreach (Transform childTransform in iterationPoint.transform)
            {
                LoadRecurse(childTransform.gameObject, currentScope, globalScope, prefabRegistry, treeContext);
            }

        }

        private static List<SaveScopeData> GetTopLevelSaveScopeData(Scene sceneToSave)
        {
            var sceneScopeData = new SaveScopeData(new SceneSaveScopeIdentifier(sceneToSave));
            var globalScopeData = new SaveScopeData(new GlobalSaveScopeIdentifier());

            var sceneScopeStack = new Stack<GameObject>(sceneToSave.GetRootGameObjects());
            var globalScopeStack = new Stack<GameObject>();

            var saveTreeContext = new SaveTreeContext
            {
                isGlobal = false
            };

            while (sceneScopeStack.Count > 0)
            {
                var nextObj = sceneScopeStack.Pop();
                if (nextObj.GetComponent<GlobalSaveFlag>() != null)
                {
                    // global save flags push this game object and all under it into the global scope
                    globalScopeStack.Push(nextObj);
                    continue;
                }

                var saveables = nextObj.GetComponents<ISaveableData>();
                sceneScopeData.dataInScope.AddRange(saveables
                    .Select(x => new SaveData
                    {
                        savedSerializableObject = x.GetSaveObject(),
                        uniqueSaveDataId = x.UniqueSaveIdentifier
                    }));

                var prefabParent = nextObj.GetComponent<SaveablePrefabParent>();
                if (prefabParent != null)
                {
                    // prefab parent flags push only the game objects under this scope into a sub-scope
                    sceneScopeData.childScopes.AddRange(GetPrefabSaveScopeData(prefabParent, saveTreeContext));
                    continue;
                }

                foreach (Transform childTransform in nextObj.transform)
                {
                    sceneScopeStack.Push(childTransform.gameObject);
                }
            }


            while (globalScopeStack.Count > 0)
            {
                var nextObj = globalScopeStack.Pop();

                var saveables = nextObj.GetComponents<ISaveableData>();
                globalScopeData.dataInScope.AddRange(saveables
                    .Select(x => new SaveData
                    {
                        savedSerializableObject = x.GetSaveObject(),
                        uniqueSaveDataId = x.UniqueSaveIdentifier
                    }));

                var prefabParent = nextObj.GetComponent<SaveablePrefabParent>();
                if (prefabParent != null)
                {
                    // prefab parent flags push only the game objects under this scope into a sub-scope
                    globalScopeData.childScopes.AddRange(GetPrefabSaveScopeData(prefabParent, saveTreeContext));
                    continue;
                }

                foreach (Transform childTransform in nextObj.transform)
                {
                    globalScopeStack.Push(childTransform.gameObject);
                }
            }

            return new List<SaveScopeData> { sceneScopeData, globalScopeData };
        }



        private static List<SaveScopeData> GetPrefabSaveScopeData(SaveablePrefabParent prefabParent, SaveTreeContext treeContext)
        {
            var resultData = new List<SaveScopeData>();

            var markerScope = new PrefabSaveScopeIdentifier(prefabParent.prefabParentName);
            resultData.Add(new SaveScopeData(markerScope));

            foreach (Transform childTransform in prefabParent.transform)
            {
                var saveablePrefab = childTransform.GetComponent<SaveablePrefab>();
                if (saveablePrefab == null) continue;

                var newScope = new PrefabSaveScopeIdentifier(saveablePrefab.myPrefabType, prefabParent.prefabParentName);
                var dataInScope = GetPrefabSaveScopeDataRecursive(childTransform.gameObject, treeContext).ToList();
                var newData = new SaveScopeData(newScope)
                {
                    dataInScope = dataInScope.OfType<SaveData>().ToList(),
                    childScopes = dataInScope.OfType<SaveScopeData>().ToList()
                };
                resultData.Add(newData);
            }

            return resultData;
        }

        private static IEnumerable<ISaveDataPiece> GetPrefabSaveScopeDataRecursive(GameObject iterationPoint, SaveTreeContext treeContext)
        {
            if (!treeContext.isGlobal && iterationPoint.GetComponent<GlobalSaveFlag>() != null)
            {
                Debug.LogError("Global save flag detected on a prefab game object instantiated outside of global scope. Either remove the global flag on the prefab, or make sure that it is instantiated under a parent in the global scope", iterationPoint);
                throw new System.Exception("Bad save scene format");
            }

            var saveables = iterationPoint.GetComponents<ISaveableData>();
            foreach (var saveable in saveables)
            {
                yield return new SaveData
                {
                    savedSerializableObject = saveable.GetSaveObject(),
                    uniqueSaveDataId = saveable.UniqueSaveIdentifier
                };
            }

            var prefabParent = iterationPoint.GetComponent<SaveablePrefabParent>();
            if (prefabParent != null)
            {
                var prefabSaveData = GetPrefabSaveScopeData(prefabParent, treeContext);
                foreach (var datum in prefabSaveData)
                {
                    yield return datum;
                }
                yield break;
            }

            foreach (Transform childTransform in iterationPoint.transform)
            {
                foreach (var datum in GetPrefabSaveScopeDataRecursive(childTransform.gameObject, treeContext))
                {
                    yield return datum;
                }
            }
        }
    }
}
