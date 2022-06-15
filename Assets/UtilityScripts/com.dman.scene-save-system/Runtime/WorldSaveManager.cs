using Dman.SceneSaveSystem.Objects;
using Dman.SceneSaveSystem.Objects.Identifiers;
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
        public string gameobjectSaveRootFileName = "objects.dat";

        private static readonly string LastSavedSceneSaveDataId = "Internal_SaveData_LastSavedScene";

        public static void DeleteSaveData()
        {
            SerializationManager.DeleteAll(SaveContext.instance.saveName);
        }
        public static void DeleteGlobalSaveData()
        {
            SerializationManager.DeleteChunks(SaveContext.instance.saveName, new GlobalSaveScopeIdentifier());
        }

        public void DeleteSaveData(SceneReference scene)
        {
            SerializationManager.DeleteChunks(SaveContext.instance.saveName, new SceneSaveScopeIdentifier(scene));
        }
        public void SaveActiveScene()
        {
            this.Save(SceneReference.Active);
        }


        public void SwapScenes(SceneReference oldScene, SceneReference nextScene)
        {
            this.StartCoroutine(this.SwapScenesCoroutine(oldScene, nextScene));
        }
        private IEnumerator SwapScenesCoroutine(SceneReference oldScene, SceneReference nextScene)
        {
            var saveManager = GameObject.FindObjectOfType<WorldSaveManager>();
            saveManager.Save(oldScene);

            Debug.Log($"unloading scene {oldScene.Name} [{oldScene.BuildIndex}]");
            yield return SceneManager.UnloadSceneAsync(oldScene.ScenePointerIfLoaded);

            Debug.Log($"loading scene {nextScene.Name} [{nextScene.BuildIndex}]");
            yield return saveManager.LoadCoroutine(nextScene, LoadSceneMode.Additive);
        }

        /// <summary>
        /// Save all ISaveableData in the scene, but not a child of a SaveablePrefab.
        /// then find all saveablePrefabs, and get their identifying information and all the information of the 
        ///     components inside each prefab. save all of these as a list
        /// </summary>
        public void Save(SceneReference sceneToSave)
        {
            if (!sceneToSave.IsLoaded)
            {
                throw new System.InvalidOperationException("Cannot save a scene which is not loaded");
            }
            UnityEngine.Profiling.Profiler.BeginSample("Saving " + sceneToSave.Name);
            UnityEngine.Profiling.Profiler.BeginSample("generate serializables");
            var saveScopes = GetTopLevelSaveScopeData(sceneToSave);
            UnityEngine.Profiling.Profiler.EndSample();

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


            UnityEngine.Profiling.Profiler.BeginSample("presave hook");
            SaveSystemHooks.TriggerPreSave(sceneToSave);
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("saving to file");
            SerializationManager.Save(sceneScope, SaveContext.instance.saveName);
            SerializationManager.Save(oldGlobalScope, SaveContext.instance.saveName);
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("postsave hook");
            SaveSystemHooks.TriggerPostSave(sceneToSave);
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.EndSample();
        }

        /// <summary>
        /// unload the active scene, and wait for the scene at <paramref name="scenePath"/> to be loaded.
        /// </summary>
        public void Load(SceneReference sceneToLoad, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            StartCoroutine(LoadCoroutine(sceneToLoad, loadMode));
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
        public IEnumerator LoadCoroutine(SceneReference sceneToLoad = null, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            // only protect continuity of this object if its scene will be unloaded
            if (loadMode == LoadSceneMode.Single)
            {
                Debug.Log($"load mode is single");
                DontDestroyOnLoad(gameObject);
            }

            var globalScope = new GlobalSaveScopeIdentifier();
            UnityEngine.Profiling.Profiler.BeginSample("loading: deserializing");
            var globalData = SerializationManager.Load<SaveScopeData>(globalScope, SaveContext.instance.saveName);
            UnityEngine.Profiling.Profiler.EndSample();

            if (sceneToLoad == null)
            {
                if (globalData.DataInScopeDictionary.TryGetValue(LastSavedSceneSaveDataId, out var saveData) && saveData.savedSerializableObject is SceneSaveScopeIdentifier savedScene)
                {
                    sceneToLoad = savedScene.scene;
                }
                else
                {
                    Debug.LogError("Cannot load, invalid save format or no save data.");
                    throw new SaveFormatException("Bad save format. No last saved scene flag");
                }
            }

            Scene loadingScene;
            UnityEngine.Profiling.Profiler.BeginSample("loading: preload hook");
            SaveSystemHooks.TriggerPreLoad(sceneToLoad);
            UnityEngine.Profiling.Profiler.EndSample();
            if (sceneToLoad.BuildIndex >= 0)
            {
                loadingScene = SceneManager.LoadScene(sceneToLoad.BuildIndex, new LoadSceneParameters(loadMode));
            }
            else
            {
                // TODO: outdated?
                loadingScene = SceneManager.LoadScene(sceneToLoad.Name, new LoadSceneParameters(loadMode));
            }
            yield return new WaitUntil(() => loadingScene.isLoaded);

            UnityEngine.Profiling.Profiler.BeginSample("loading: midload hook");
            SaveSystemHooks.TriggerMidLoad(sceneToLoad);
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("loading: applying new data");
            LoadIntoSingleScene(sceneToLoad, saveablePrefabRegistry, globalData);
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("loading: postload hook");
            SaveSystemHooks.TriggerPostLoad(sceneToLoad);
            UnityEngine.Profiling.Profiler.EndSample();

            yield return null;
            if (loadMode == LoadSceneMode.Single)
            {
                Destroy(gameObject);
            }
        }

        private static void LoadIntoSingleScene(SceneReference scene, SaveablePrefabRegistry saveablePrefabRegistry, SaveScopeData globalData)
        {
            var sceneScopeIdentity = new SceneSaveScopeIdentifier(scene);

            var sceneData = SerializationManager.Load<SaveScopeData>(sceneScopeIdentity, SaveContext.instance.saveName);

            var iterationContext = new LoadIterationContext
            {
                isGlobal = false,
                currentScope = sceneData,
                globalScope = globalData,
                prefabRegistry = saveablePrefabRegistry
            };
            var allLoadablesAtScene = scene.ScenePointerIfLoaded.GetRootGameObjects()
                .SelectMany(x => ExtractLoadableObjectsFromScope(x.transform, iterationContext))
                .OrderBy(x => x.LoadOrder)
                .ToList();

            foreach (var loadable in allLoadablesAtScene)
            {
                loadable.LoadDataIn();
            }
        }

        struct LoadTraversalState
        {
            public Transform transform;
            public LoadIterationContext context;
        }

        internal static IEnumerable<ILoadableObject> ExtractLoadableObjectsFromScope(
            Transform initialTransform,
            LoadIterationContext currentContext)
        {
            var transformIterationStack = new Stack<LoadTraversalState>();
            transformIterationStack.Push(new LoadTraversalState
            {
                transform = initialTransform,
                context = currentContext,
            });

            while (transformIterationStack.Count > 0)
            {
                var iteration = transformIterationStack.Pop();

                if (iteration.transform.GetComponent<GlobalSaveFlag>() != null && !iteration.context.isGlobal)
                {
                    if (iteration.context.currentScope != null && !(iteration.context.currentScope.scopeIdentifier is SceneSaveScopeIdentifier))
                    {
                        Debug.LogError("Global save flag detected on a prefab game object instantiated outside of global scope. Either remove the global flag on the prefab, or make sure that it is instantiated under a parent in the global scope", iteration.transform.gameObject);
                        throw new SaveFormatException("Bad save scene format");
                    }

                    iteration.context.isGlobal = true;
                    iteration.context.currentScope = iteration.context.globalScope;
                }

                var saveables = iteration.transform.GetComponents<ISaveableData>();
                if (saveables != null && saveables.Length > 0)
                {
                    foreach (var saveable in saveables)
                    {
                        yield return new BasicLoadableObject(saveable, iteration.context);
                    }
                }

                var prefabParent = iteration.transform.GetComponent<SaveablePrefabParent>();
                if (prefabParent != null)
                {
                    yield return new PrefabParentLoadable(prefabParent, iteration.context);
                    continue;
                }

                foreach (Transform child in iteration.transform)
                {
                    transformIterationStack.Push(new LoadTraversalState
                    {
                        transform = child,
                        context = iteration.context
                    });
                }
            }
        }

        private static List<SaveScopeData> GetTopLevelSaveScopeData(SceneReference sceneToSave)
        {
            var sceneScopeData = new SaveScopeData(new SceneSaveScopeIdentifier(sceneToSave));
            var globalScopeData = new SaveScopeData(new GlobalSaveScopeIdentifier());

            var sceneScopeStack = new Stack<GameObject>(sceneToSave.ScenePointerIfLoaded.GetRootGameObjects());
            var globalScopeStack = new Stack<GameObject>();

            var saveTreeContext = new SaveTreeContext
            {
                isGlobal = false
            };

            DigestSaveObjectStack(sceneScopeStack, sceneScopeData, saveTreeContext, globalScopeStack);

            DigestSaveObjectStack(globalScopeStack, globalScopeData, saveTreeContext);
            return new List<SaveScopeData> { sceneScopeData, globalScopeData };
        }

        private static void DigestSaveObjectStack(Stack<GameObject> stackToDigest, SaveScopeData targetSaveScope, SaveTreeContext saveTreeContext, Stack<GameObject> globalFalloffStack = null)
        {
            while (stackToDigest.Count > 0)
            {
                var nextObj = stackToDigest.Pop();
                if (globalFalloffStack != null && nextObj.GetComponent<GlobalSaveFlag>() != null)
                {
                    // global save flags push this game object and all under it into the global scope
                    globalFalloffStack.Push(nextObj);
                    continue;
                }

                if (nextObj.GetComponent<DontSaveFlag>() != null)
                {
                    continue;
                }

                var saveables = nextObj.GetComponents<ISaveableData>();
                targetSaveScope.dataInScope.AddRange(saveables
                    .Select(x => new SaveData
                    {
                        savedSerializableObject = x.GetSaveObject(),
                        uniqueSaveDataId = x.UniqueSaveIdentifier
                    }));

                var prefabParent = nextObj.GetComponent<SaveablePrefabParent>();
                if (prefabParent != null)
                {
                    // prefab parent flags push only the game objects under this scope into a sub-scope
                    targetSaveScope.childScopes.AddRange(GetPrefabSaveScopeData(prefabParent, saveTreeContext));
                    continue;
                }

                foreach (Transform childTransform in nextObj.transform)
                {
                    stackToDigest.Push(childTransform.gameObject);
                }
            }
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
