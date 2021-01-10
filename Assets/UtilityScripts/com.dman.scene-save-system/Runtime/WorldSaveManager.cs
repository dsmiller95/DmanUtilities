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

        /// <summary>
        /// Save all ISaveableData in the scene, but not a child of a SaveablePrefab.
        /// then find all saveablePrefabs, and get their identifying information and all the information of the 
        ///     components inside each prefab. save all of these as a list
        /// </summary>
        public void Save()
        {
            var saveDataObject = GetMasterSaveObject(SceneManager.GetActiveScene());
            SaveSystemHooks.TriggerPreSave();
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

        private IEnumerator LoadCoroutine()
        {
            var loadedData = SerializationManager.Load(gameobjectSaveRootFileName, SaveContext.instance.saveName);
            if (loadedData == null || !(loadedData is MasterSaveObject worldSaveData))
            {
                yield break;
            }

            SaveSystemHooks.TriggerPreLoad();
            DontDestroyOnLoad(gameObject);
            var sceneIndexToLoad = SceneUtility.GetBuildIndexByScenePath(saveLoadScene.scenePath);
            var loadingScene = SceneManager.LoadScene(sceneIndexToLoad, new LoadSceneParameters(LoadSceneMode.Single));
            yield return new WaitUntil(() => loadingScene.isLoaded);
            LoadFromMasterSaveObjectIntoScene(worldSaveData, loadingScene, saveablePrefabRegistry);
            SaveSystemHooks.TriggerPostLoad();
            Destroy(gameObject);
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
