using Dman.Utilities;
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
        [Tooltip("The scene which will be loaded in on top of the current scene when loading the save file.")]
        public SceneReference saveLoadScene;
        public string gameobjectSaveRootFileName = "objects.dat";

        public string saveFileSuffix = ".dat";


        /// <summary>
        /// Save all ISaveableData in the scene, but not a child of a SaveablePrefab.
        /// then find all saveablePrefabs, and get their identifying information and all the information of the 
        ///     components inside each prefab. save all of these as a list
        /// </summary>
        public void Save(Scene sceneToSave = default)
        {
            if (sceneToSave == default)
            {
                sceneToSave = SceneManager.GetActiveScene();
            }
            var saveDataObject = GetMasterSaveObject(sceneToSave);

            var saveScopes = GetTopLevelSaveScopeData(sceneToSave);
            var globalScope = saveScopes.FirstOrDefault(x => x.scopeIdentifier is GlobalSaveScopeIdentifier);
            var sceneScope = saveScopes.FirstOrDefault(x => x.scopeIdentifier is SceneSaveScopeIdentifier);

            var oldGlobalScope = SerializationManager.Load<SaveScopeData>(globalScope.scopeIdentifier.UniqueSemiReadableName + ".dat", SaveContext.instance.saveName);
            if(oldGlobalScope == null)
            {
                oldGlobalScope = globalScope;
            }else
            {
                oldGlobalScope.OverwriteWith(globalScope);
            }

            SaveSystemHooks.TriggerPreSave();

            SerializationManager.Save(sceneScope.scopeIdentifier.UniqueSemiReadableName + saveFileSuffix, SaveContext.instance.saveName, sceneScope);
            SerializationManager.Save(globalScope.scopeIdentifier.UniqueSemiReadableName + saveFileSuffix, SaveContext.instance.saveName, globalScope);

            SerializationManager.Save(gameobjectSaveRootFileName, SaveContext.instance.saveName, saveDataObject);
            SaveSystemHooks.TriggerPostSave();
        }

        /// <summary>
        /// Reload the active scene, and wait for the next scene to be loaded. then destroy all objects flagged with SaveablePrefab
        ///     in the newly loaded scene.
        /// Then load all ISaveableData into the scene. Then loop through all SaveablePrefabs, and spawn each according to the
        ///     parent identifyer in the save object. load all saveable data for each prefab into the prefab instance as each
        ///     is instantiated.
        /// </summary>
        public void Load()
        {
            StartCoroutine(LoadCoroutine());
        }

        public IEnumerator LoadCoroutine()
        {
            var worldSaveData = SerializationManager.Load<MasterSaveObject>(gameobjectSaveRootFileName, SaveContext.instance.saveName);

            SaveSystemHooks.TriggerPreLoad();
            DontDestroyOnLoad(gameObject);
            var sceneIndexToLoad = SceneUtility.GetBuildIndexByScenePath(saveLoadScene.scenePath);
            var loadingScene = SceneManager.LoadScene(sceneIndexToLoad, new LoadSceneParameters(LoadSceneMode.Single));
            yield return new WaitUntil(() => loadingScene.isLoaded);
            LoadFromMasterSaveObjectIntoScene(worldSaveData, loadingScene, saveablePrefabRegistry);
            SaveSystemHooks.TriggerPostLoad();
            Destroy(gameObject);
        }

        public static List<SaveScopeData> GetTopLevelSaveScopeData(Scene sceneToSave)
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
                        uniqueSaveDataId = x.UniqueSaveIdentifier,
                        saveDataIDDependencies = x.GetDependencies().Select(x => x.UniqueSaveIdentifier).ToArray()
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
                        uniqueSaveDataId = x.UniqueSaveIdentifier,
                        saveDataIDDependencies = x.GetDependencies().Select(x => x.UniqueSaveIdentifier).ToArray()
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
            var resultData = GetPrefabSaveScopeDataRecursive(prefabParent.gameObject, treeContext);

            var castedData = resultData.OfType<SaveScopeData>().ToList();
            if(castedData.Count != resultData.Count())
            {
                Debug.LogError($"No Save data can exist between the prefab parent and the prefab instances. found some under {prefabParent.prefabParentName}", prefabParent);
                throw new System.Exception("Bad save scene format");
            }
            return castedData;
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
                    uniqueSaveDataId = saveable.UniqueSaveIdentifier,
                    saveDataIDDependencies = saveable.GetDependencies().Select(x => x.UniqueSaveIdentifier).ToArray()
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

        public static MasterSaveObject GetMasterSaveObject(Scene sceneToSave)
        {
            var prefabsToSave = new List<SaveablePrefab>();

            var rootObjects = sceneToSave.GetRootGameObjects();

            var sceneSaveData = rootObjects
                .SelectMany(x => GetSaveablesForParent(x.transform, prefabsToSave))
                .Select(x => new SaveData
                {
                    savedSerializableObject = x.GetSaveObject(),
                    uniqueSaveDataId = x.UniqueSaveIdentifier,
                    saveDataIDDependencies = x.GetDependencies().Select(x => x.UniqueSaveIdentifier).ToArray()
                }).ToList();
            SortSavedDatasBasedOnInterdependencies(sceneSaveData);

            var savedPrefabData = prefabsToSave
                .Select(x =>
                {
                    try
                    {
                        return x.GetPrefabSaveData();
                    }
                    catch (System.Exception)
                    {
                        Debug.LogError($"Error saving data inside {x}");
                        throw;
                    }
                });

            return new MasterSaveObject
            {
                sceneSaveData = sceneSaveData.ToArray(),
                sceneSavedPrefabInstances = savedPrefabData.ToArray()
            };
        }
        public static void LoadFromMasterSaveObjectIntoScene(MasterSaveObject saveObject, Scene sceneToLoadTo, SaveablePrefabRegistry prefabRegistry)
        {
            Debug.Log($"loading from save post-scene-reload");
            var rootObjects = sceneToLoadTo.GetRootGameObjects();

            foreach (var prefabRootExistingInScene in rootObjects.SelectMany(x => x.GetComponentsInChildren<SaveablePrefab>(true)))
            {
                DestroyImmediate(prefabRootExistingInScene.gameObject);
            }
            AssignSaveDataToChildren(rootObjects.Select(x => x.transform), saveObject.sceneSaveData);

            var prefabParentDictionary = rootObjects
                .SelectMany(x => x.GetComponentsInChildren<SaveablePrefabParent>(true))
                .ToDictionary(x => x.prefabParentName);
            foreach (var savedPrefab in saveObject.sceneSavedPrefabInstances)
            {
                if (!prefabParentDictionary.TryGetValue(savedPrefab.prefabParentId, out var prefabParent))
                {
                    Debug.LogError($"No prefab parent found of ID {savedPrefab.prefabParentId}");
                }
                var prefab = prefabRegistry.GetUniqueObjectFromID(savedPrefab.prefabTypeId);
                var newInstance = Instantiate(prefab.prefab, prefabParent.transform);

                AssignSaveDataToChildren(new[] { newInstance.transform }, savedPrefab.saveData);
            }
        }

        private static IEnumerable<ISaveableData> GetSaveablesForParent(Transform initialTransform, List<SaveablePrefab> prefabList = null)
        {
            return DepthFirstRecurseTraverseAvoidingPrefabs(initialTransform, prefabList).SelectMany(x => x);
        }

        private static IEnumerable<ISaveableData[]> DepthFirstRecurseTraverseAvoidingPrefabs(Transform initialTransform, List<SaveablePrefab> prefabList = null)
        {
            var transformIterationStack = new Stack<Transform>();
            transformIterationStack.Push(initialTransform);

            while (transformIterationStack.Count > 0)
            {
                var currentTransform = transformIterationStack.Pop();

                var saveable = currentTransform.GetComponents<ISaveableData>();
                if (saveable != null && saveable.Length > 0)
                {
                    yield return saveable;
                }
                foreach (Transform child in currentTransform)
                {
                    var childPrefab = child.GetComponent<SaveablePrefab>();
                    if (childPrefab != null)
                    {
                        prefabList?.Add(childPrefab);
                        continue;
                    }
                    transformIterationStack.Push(child);
                }
            }
        }

        internal static void SortSavedDatasBasedOnInterdependencies(List<SaveData> datas)
        {
            datas.Sort((a, b) =>
            {
                var aDependsOnB = a.saveDataIDDependencies.Contains(b.uniqueSaveDataId);
                var bDependsOnA = b.saveDataIDDependencies.Contains(a.uniqueSaveDataId);
                if (aDependsOnB && bDependsOnA)
                {
                    throw new System.Exception("Circular dependency detected!");
                }
                if (aDependsOnB)
                {
                    return -1;
                }
                if (bDependsOnA)
                {
                    return 1;
                }
                return 0;
            });
        }


        private static void AssignSaveDataToChildren(IEnumerable<Transform> roots, SaveData[] orderedSaveData)
        {
            var saveableChildren = roots.SelectMany(x => GetSaveablesForParent(x)).ToDictionary(x => x.UniqueSaveIdentifier);
            foreach (var saveData in orderedSaveData)
            {
                if (!saveableChildren.TryGetValue(saveData.uniqueSaveDataId, out var saveable))
                {
                    Debug.LogError($"No matching saveable for {saveData.uniqueSaveDataId}");
                }
                saveable.SetupFromSaveObject(saveData.savedSerializableObject);
            }
        }
    }
}
