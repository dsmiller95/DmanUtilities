using Dman.SceneSaveSystem.PlaymodeTests;
using Dman.Utilities;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace Dman.SceneSaveSystem.EditmodeTests
{
    public class SceneSaveCycleTests
    {
        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator CyclesSimpleSaveablesAtTopLevelScope()
        {
            var testScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var testSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(testScene.path);

            var savedObject1 = new GameObject("save object 1");
            var saveable1 = savedObject1.AddComponent<SimpleSaveable>();
            saveable1.MySavedData = "I am save data 1";

            var savedObject2 = new GameObject("save object two");
            var saveable2 = savedObject2.AddComponent<SimpleSaveable>();
            saveable2.MySavedData = "I am save data two";
            saveable2.uniqueNameInScope = "unique";

            var saveManagerObject = new GameObject("save manager");
            var saveManager = saveManagerObject.AddComponent<WorldSaveManager>();

            var prefabRegistry = ScriptableObject.CreateInstance<SaveablePrefabRegistry>();
            saveManager.saveablePrefabRegistry = prefabRegistry;


            yield return new EnterPlayMode();

            SaveContext.instance.saveName = "test_save";
            WorldSaveManager.DeleteSaveData();


            var saveables = GameObject.FindObjectsOfType<SimpleSaveable>();

            saveManager = GameObject.FindObjectOfType<WorldSaveManager>();
            Assert.NotNull(saveManager);
            saveable1 = saveables.Where(x => x.gameObject.name == "save object 1").FirstOrDefault();
            Assert.NotNull(saveable1);
            saveable2 = saveables.Where(x => x.gameObject.name == "save object two").FirstOrDefault();
            Assert.NotNull(saveable2);

            Assert.AreEqual("I am save data 1", saveable1.MySavedData);
            saveable1.MySavedData = "my save data has changed!";
            Assert.AreEqual("I am save data two", saveable2.MySavedData);

            saveManager.SaveActiveScene();

            saveable2.MySavedData = "my save data has changed, two!!";
            Assert.AreEqual("my save data has changed!", saveable1.MySavedData);
            Assert.AreEqual("my save data has changed, two!!", saveable2.MySavedData);

            yield return saveManager.LoadCoroutine(new SceneReference(""));
            yield return null;

            saveables = GameObject.FindObjectsOfType<SimpleSaveable>();

            saveManager = GameObject.FindObjectOfType<WorldSaveManager>();
            Assert.NotNull(saveManager);
            saveable1 = saveables.Where(x => x.gameObject.name == "save object 1").FirstOrDefault();
            Assert.NotNull(saveable1);
            saveable2 = saveables.Where(x => x.gameObject.name == "save object two").FirstOrDefault();
            Assert.NotNull(saveable2);

            Assert.AreEqual("my save data has changed!", saveable1.MySavedData);
            Assert.AreEqual("I am save data two", saveable2.MySavedData);

            yield return new ExitPlayMode();
            yield return null;

            Object.DestroyImmediate(testSceneAsset);
            Object.DestroyImmediate(prefabRegistry);
            WorldSaveManager.DeleteSaveData();

            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }

        private GameObject CreateSavablePrefab(string topLevelName, System.Action<GameObject> extraSetup = null)
        {
            var prefabType = ScriptableObject.CreateInstance<SaveablePrefabType>();
            AssetDatabase.CreateAsset(prefabType, $"Assets/{topLevelName}_type.asset");
            prefabType = AssetDatabase.LoadAssetAtPath<SaveablePrefabType>($"Assets/{topLevelName}_type.asset");

            var prefabObject = new GameObject(topLevelName);
            var saveablePrefab = prefabObject.AddComponent<SaveablePrefab>();
            saveablePrefab.myPrefabType = prefabType;

            var nestedObject = new GameObject("data inside prefab");
            nestedObject.transform.parent = prefabObject.transform;
            var savedPrefab = nestedObject.AddComponent<SimpleSaveable>();
            savedPrefab.MySavedData = "inside a prefab!";

            extraSetup?.Invoke(prefabObject);

            var prefab = PrefabUtility.SaveAsPrefabAsset(prefabObject, $"Assets/{topLevelName}_prefab.prefab");

            Object.DestroyImmediate(prefabObject);

            prefabType.prefab = prefab.GetComponent<SaveablePrefab>();
            EditorUtility.SetDirty(prefabType);
            AssetDatabase.SaveAssets();

            prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/{topLevelName}_prefab.prefab");

            return prefab;
        }

        [UnityTest]
        public IEnumerator CyclesWithSavedPrefabsAndSceneInitializedState()
        {
            var testScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var testSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(testScene.path);

            {
                var topLevelSavedObject = new GameObject("save object 1");
                var saveable = topLevelSavedObject.AddComponent<SimpleSaveable>();
                saveable.MySavedData = "I am save data 1";

                var saveablePrefab = CreateSavablePrefab("topLevel");
                var prefabRegistry = ScriptableObject.CreateInstance<SaveablePrefabRegistry>();
                prefabRegistry.allObjects = new List<SaveablePrefabType> { saveablePrefab.GetComponent<SaveablePrefab>().myPrefabType };
                prefabRegistry.AssignAllIDs();

                var prefabParentObject = new GameObject("prefab parent");
                prefabParentObject.AddComponent<SaveablePrefabParent>().prefabParentName = "a parent to prefabs";

                var nextPrefab = GameObject.Instantiate(saveablePrefab, prefabParentObject.transform);
                nextPrefab.transform.GetChild(0).GetComponent<SimpleSaveable>().MySavedData = "default!";

                var saveManagerObject = new GameObject("save manager");
                var saveManager = saveManagerObject.AddComponent<WorldSaveManager>();

                saveManager.saveablePrefabRegistry = prefabRegistry;
            }


            yield return new EnterPlayMode();

            SaveContext.instance.saveName = "test_save";
            WorldSaveManager.DeleteSaveData();
            {
                var saveablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/topLevel_prefab.prefab");
                var prefabParent = GameObject.FindObjectOfType<SaveablePrefabParent>();

                var nextPrefab = GameObject.Instantiate(saveablePrefab, prefabParent.transform);
                nextPrefab.transform.GetChild(0).GetComponent<SimpleSaveable>().MySavedData = "first!";
                nextPrefab = GameObject.Instantiate(saveablePrefab, prefabParent.transform);
                nextPrefab.transform.GetChild(0).GetComponent<SimpleSaveable>().MySavedData = "second prefab instance";
                nextPrefab = GameObject.Instantiate(saveablePrefab, prefabParent.transform);
                nextPrefab.transform.GetChild(0).GetComponent<SimpleSaveable>().MySavedData = "third prefab instance";

                var saveables = GameObject.FindObjectsOfType<SimpleSaveable>();

                var saveManager = GameObject.FindObjectOfType<WorldSaveManager>();
                Assert.NotNull(saveManager);
                var saveable = saveables.Where(x => x.gameObject.name == "save object 1").FirstOrDefault();
                Assert.NotNull(saveable);

                Assert.AreEqual("I am save data 1", saveable.MySavedData);
                saveable.MySavedData = "my save data has changed!";

                saveManager.SaveActiveScene();

                Assert.AreEqual("my save data has changed!", saveable.MySavedData);

                yield return saveManager.LoadCoroutine(new SceneReference(""));
                yield return null;
            }
            {
                var saveables = GameObject.FindObjectsOfType<SimpleSaveable>();

                var saveManager = GameObject.FindObjectOfType<WorldSaveManager>();
                Assert.NotNull(saveManager);
                var saveable = saveables.Where(x => x.gameObject.name == "save object 1").FirstOrDefault();
                Assert.NotNull(saveable);

                Assert.AreEqual("my save data has changed!", saveable.MySavedData);

                var prefabParent = GameObject.FindObjectOfType<SaveablePrefabParent>();
                Assert.NotNull(prefabParent);
                Assert.AreEqual(4, prefabParent.transform.childCount);
                Assert.AreEqual("default!", prefabParent.transform.GetChild(0).GetChild(0).GetComponent<SimpleSaveable>().MySavedData);
                Assert.AreEqual("first!", prefabParent.transform.GetChild(1).GetChild(0).GetComponent<SimpleSaveable>().MySavedData);
                Assert.AreEqual("second prefab instance", prefabParent.transform.GetChild(2).GetChild(0).GetComponent<SimpleSaveable>().MySavedData);
                Assert.AreEqual("third prefab instance", prefabParent.transform.GetChild(3).GetChild(0).GetComponent<SimpleSaveable>().MySavedData);
            }
            yield return new ExitPlayMode();
            yield return null;

            Object.DestroyImmediate(testSceneAsset);

            AssetDatabase.DeleteAsset("Assets/topLevel_type.asset");
            AssetDatabase.DeleteAsset("Assets/topLevel_prefab.prefab");
            WorldSaveManager.DeleteSaveData();

            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }

        [UnityTest]
        public IEnumerator CyclesWithNestedSavedPrefabs()
        {
            var testScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var testSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(testScene.path);
            var prefabRegistry = ScriptableObject.CreateInstance<SaveablePrefabRegistry>();

            try
            {
                {
                    var topLevelPrefab = CreateSavablePrefab("topLevel", go =>
                    {
                        var nestedObject = new GameObject("prefab parent in prefab");
                        nestedObject.transform.parent = go.transform;
                        var prefabParent = nestedObject.AddComponent<SaveablePrefabParent>();
                        prefabParent.prefabParentName = "NestedPrefabParent";
                    });
                    var nestedPrefab = CreateSavablePrefab("nested");

                    prefabRegistry.allObjects = new List<SaveablePrefabType> {
                        topLevelPrefab.GetComponent<SaveablePrefab>().myPrefabType,
                        nestedPrefab.GetComponent<SaveablePrefab>().myPrefabType
                    };
                    prefabRegistry.AssignAllIDs();

                    var prefabParentObject = new GameObject("prefab parent in scene");
                    prefabParentObject.AddComponent<SaveablePrefabParent>().prefabParentName = "ScenePrefabParent";

                    var saveManagerObject = new GameObject("save manager");
                    var saveManager = saveManagerObject.AddComponent<WorldSaveManager>();

                    saveManager.saveablePrefabRegistry = prefabRegistry;
                }


                yield return new EnterPlayMode();
                SaveContext.instance.saveName = "test_save";
                WorldSaveManager.DeleteSaveData();
                {
                    var topLevelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/topLevel_prefab.prefab");
                    var nestedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/nested_prefab.prefab");
                    var prefabParentInScene = GameObject.FindObjectOfType<SaveablePrefabParent>();

                    {
                        var nextPrefab = GameObject.Instantiate(topLevelPrefab, prefabParentInScene.transform);
                        nextPrefab.transform.GetChild(0).GetComponent<SimpleSaveable>().MySavedData = "first!";

                        AddNestedPrefab(nestedPrefab, nextPrefab, "first nested!");
                        AddNestedPrefab(nestedPrefab, nextPrefab, "second nested!");
                        AddNestedPrefab(nestedPrefab, nextPrefab, "third nested!");
                    }

                    {
                        var nextPrefab = GameObject.Instantiate(topLevelPrefab, prefabParentInScene.transform);
                        nextPrefab.transform.GetChild(0).GetComponent<SimpleSaveable>().MySavedData = "second prefab instance";
                    }

                    {
                        var nextPrefab = GameObject.Instantiate(topLevelPrefab, prefabParentInScene.transform);
                        nextPrefab.transform.GetChild(0).GetComponent<SimpleSaveable>().MySavedData = "third prefab instance";

                        AddNestedPrefab(nestedPrefab, nextPrefab, "first third nested!");
                        AddNestedPrefab(nestedPrefab, nextPrefab, "second third nested!");
                    }

                    var saveables = GameObject.FindObjectsOfType<SimpleSaveable>();

                    var saveManager = GameObject.FindObjectOfType<WorldSaveManager>();
                    Assert.NotNull(saveManager);

                    saveManager.SaveActiveScene();

                    yield return saveManager.LoadCoroutine(new SceneReference(""));
                    yield return null;
                }

                {
                    var saveables = GameObject.FindObjectsOfType<SimpleSaveable>();

                    var saveManager = GameObject.FindObjectOfType<WorldSaveManager>();
                    Assert.NotNull(saveManager);

                    var prefabParentInScene = GameObject.FindObjectsOfType<SaveablePrefabParent>().Where(x => x.prefabParentName == "ScenePrefabParent").First();
                    Assert.NotNull(prefabParentInScene);
                    Assert.AreEqual(3, prefabParentInScene.transform.childCount);

                    {
                        var prefabInstance = prefabParentInScene.transform.GetChild(0);
                        Assert.AreEqual("first!", prefabInstance.GetChild(0).GetComponent<SimpleSaveable>().MySavedData);
                        var prefabParentInPrefab = prefabInstance.GetComponentInChildren<SaveablePrefabParent>();
                        Assert.AreEqual(3, prefabParentInPrefab.transform.childCount);
                        Assert.AreEqual("first nested!", prefabParentInPrefab.transform.GetChild(0).GetChild(0).GetComponent<SimpleSaveable>().MySavedData);
                        Assert.AreEqual("second nested!", prefabParentInPrefab.transform.GetChild(1).GetChild(0).GetComponent<SimpleSaveable>().MySavedData);
                        Assert.AreEqual("third nested!", prefabParentInPrefab.transform.GetChild(2).GetChild(0).GetComponent<SimpleSaveable>().MySavedData);
                    }
                    {
                        var prefabInstance = prefabParentInScene.transform.GetChild(1);
                        Assert.AreEqual("second prefab instance", prefabInstance.GetChild(0).GetComponent<SimpleSaveable>().MySavedData);
                        var prefabParentInPrefab = prefabInstance.GetComponentInChildren<SaveablePrefabParent>();
                        Assert.AreEqual(0, prefabParentInPrefab.transform.childCount);
                    }
                    {
                        var prefabInstance = prefabParentInScene.transform.GetChild(2);
                        Assert.AreEqual("third prefab instance", prefabInstance.GetChild(0).GetComponent<SimpleSaveable>().MySavedData);
                        var prefabParentInPrefab = prefabInstance.GetComponentInChildren<SaveablePrefabParent>();
                        Assert.AreEqual(2, prefabParentInPrefab.transform.childCount);
                        Assert.AreEqual("first third nested!", prefabParentInPrefab.transform.GetChild(0).GetChild(0).GetComponent<SimpleSaveable>().MySavedData);
                        Assert.AreEqual("second third nested!", prefabParentInPrefab.transform.GetChild(1).GetChild(0).GetComponent<SimpleSaveable>().MySavedData);
                    }
                }

                yield return new ExitPlayMode();
                yield return null;
            }
            finally
            {
                Object.DestroyImmediate(testSceneAsset);
                Object.DestroyImmediate(prefabRegistry);

                AssetDatabase.DeleteAsset("Assets/topLevel_type.asset");
                AssetDatabase.DeleteAsset("Assets/topLevel_prefab.prefab");
                AssetDatabase.DeleteAsset("Assets/nested_type.asset");
                AssetDatabase.DeleteAsset("Assets/nested_prefab.prefab");
                WorldSaveManager.DeleteSaveData();
            }


            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }

        private void AddNestedPrefab(GameObject nestedPrefab, GameObject parentPrefab, string savedData)
        {
            var nextNestedPrefab = GameObject.Instantiate(nestedPrefab, parentPrefab.GetComponentInChildren<SaveablePrefabParent>().transform);
            nextNestedPrefab.transform.GetChild(0).GetComponent<SimpleSaveable>().MySavedData = savedData;
        }

        [UnityTest]
        public IEnumerator RetainsWholeGlobalScopeDataBetweenScenes()
        {
            var prefabRegistry = ScriptableObject.CreateInstance<SaveablePrefabRegistry>();

            var oldSceneBuilds = EditorBuildSettings.scenes.ToList().ToArray();
            try
            {
                {
                    // setup scene A
                    var testSceneA = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                    var savedObject1 = new GameObject("save object 1");
                    var saveable1 = savedObject1.AddComponent<SimpleSaveable>();
                    saveable1.MySavedData = "I am save data 1 in scene A";

                    var savedObject2 = new GameObject("save object two");
                    savedObject2.AddComponent<GlobalSaveFlag>();
                    var saveable2 = savedObject2.AddComponent<SimpleSaveable>();
                    saveable2.MySavedData = "I am save data two";
                    saveable2.uniqueNameInScope = "unique";

                    var savedObject3 = new GameObject("save object three");
                    savedObject3.AddComponent<GlobalSaveFlag>();
                    var saveable3 = savedObject3.AddComponent<SimpleSaveable>();
                    saveable3.MySavedData = "I am save data three!";
                    saveable3.uniqueNameInScope = "uniqueTwo";

                    var saveManagerObject = new GameObject("save manager");
                    var saveManager = saveManagerObject.AddComponent<WorldSaveManager>();

                    saveManager.saveablePrefabRegistry = prefabRegistry;

                    testSceneA.name = "SceneA";
                    EditorSceneManager.SaveScene(testSceneA, $"Assets/SceneA.unity");
                }

                {
                    // setup scene B
                    var testSceneB = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);


                    var savedObject1 = new GameObject("save object 1");
                    var saveable1 = savedObject1.AddComponent<SimpleSaveable>();
                    saveable1.MySavedData = "I am save data 1 in scene B";

                    var savedObject2 = new GameObject("save object two");
                    savedObject2.AddComponent<GlobalSaveFlag>();
                    var saveable2 = savedObject2.AddComponent<SimpleSaveable>();
                    saveable2.MySavedData = "I am save data two. but Scene B!";
                    saveable2.uniqueNameInScope = "unique";

                    var savedObject3 = new GameObject("save object four");
                    savedObject3.AddComponent<GlobalSaveFlag>();
                    var saveable3 = savedObject3.AddComponent<SimpleSaveable>();
                    saveable3.MySavedData = "I am save data four!";
                    saveable3.uniqueNameInScope = "uniqueThree";

                    var saveManagerObject = new GameObject("save manager");
                    var saveManager = saveManagerObject.AddComponent<WorldSaveManager>();

                    saveManager.saveablePrefabRegistry = prefabRegistry;

                    testSceneB.name = "SceneB";
                    EditorSceneManager.SaveScene(testSceneB, $"Assets/SceneB.unity");
                }

                EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
                {
                    new EditorBuildSettingsScene("Assets/SceneA.unity", true),
                    new EditorBuildSettingsScene("Assets/SceneB.unity", true)
                };

                var activeScene = EditorSceneManager.OpenScene("Assets/SceneA.unity", OpenSceneMode.Single);

                yield return new EnterPlayMode();

                {
                    SaveContext.instance.saveName = "test_save";
                    WorldSaveManager.DeleteSaveData();

                    // modify and save scene A
                    var saveables = GameObject.FindObjectsOfType<SimpleSaveable>();

                    var saveManager = GameObject.FindObjectOfType<WorldSaveManager>();
                    Assert.NotNull(saveManager);
                    var saveable1 = saveables.Where(x => x.gameObject.name == "save object 1").FirstOrDefault();
                    Assert.NotNull(saveable1);
                    var saveable2 = saveables.Where(x => x.gameObject.name == "save object two").FirstOrDefault();
                    Assert.NotNull(saveable2);
                    var saveable3 = saveables.Where(x => x.gameObject.name == "save object three").FirstOrDefault();
                    Assert.NotNull(saveable3);

                    Assert.AreEqual("I am save data 1 in scene A", saveable1.MySavedData);
                    saveable1.MySavedData = "my save data has changed!";
                    Assert.AreEqual("I am save data two", saveable2.MySavedData);
                    saveable2.MySavedData = "my global save data is different!";
                    Assert.AreEqual("I am save data three!", saveable3.MySavedData);
                    saveable3.MySavedData = "my special global data is different!";

                    saveManager.SaveActiveScene();

                    Assert.AreEqual("my save data has changed!", saveable1.MySavedData);
                    Assert.AreEqual("my global save data is different!", saveable2.MySavedData);
                    Assert.AreEqual("my special global data is different!", saveable3.MySavedData);

                    yield return saveManager.LoadCoroutine(new SceneReference("Assets/SceneB.unity"));
                    yield return null;
                }

                {
                    // Assert global save data, modify and save scene B
                    var saveables = GameObject.FindObjectsOfType<SimpleSaveable>();

                    var saveManager = GameObject.FindObjectOfType<WorldSaveManager>();
                    Assert.NotNull(saveManager);
                    var saveable1 = saveables.Where(x => x.gameObject.name == "save object 1").FirstOrDefault();
                    Assert.NotNull(saveable1);
                    var saveable2 = saveables.Where(x => x.gameObject.name == "save object two").FirstOrDefault();
                    Assert.NotNull(saveable2);
                    var saveable3 = saveables.Where(x => x.gameObject.name == "save object four").FirstOrDefault();
                    Assert.NotNull(saveable3);

                    Assert.AreEqual("I am save data 1 in scene B", saveable1.MySavedData);
                    Assert.AreEqual("my global save data is different!", saveable2.MySavedData);
                    saveable2.MySavedData = "my global save data is different... again";
                    Assert.AreEqual("I am save data four!", saveable3.MySavedData);
                    saveable3.MySavedData = "my global save data four is different, but only in scene B";

                    saveManager.SaveActiveScene();

                    yield return saveManager.LoadCoroutine(new SceneReference("Assets/SceneA.unity"));
                    yield return null;
                }

                {
                    // Assert save data changes from global and scene A
                    var saveables = GameObject.FindObjectsOfType<SimpleSaveable>();

                    var saveManager = GameObject.FindObjectOfType<WorldSaveManager>();
                    Assert.NotNull(saveManager);
                    var saveable1 = saveables.Where(x => x.gameObject.name == "save object 1").FirstOrDefault();
                    Assert.NotNull(saveable1);
                    var saveable2 = saveables.Where(x => x.gameObject.name == "save object two").FirstOrDefault();
                    Assert.NotNull(saveable2);
                    var saveable3 = saveables.Where(x => x.gameObject.name == "save object three").FirstOrDefault();
                    Assert.NotNull(saveable3);

                    Assert.AreEqual("my save data has changed!", saveable1.MySavedData);
                    Assert.AreEqual("my global save data is different... again", saveable2.MySavedData);
                    Assert.AreEqual("my special global data is different!", saveable3.MySavedData);

                    yield return saveManager.LoadCoroutine(new SceneReference("Assets/SceneB.unity"));
                    yield return null;
                }

                {
                    // Assert save data changes from global and scene B
                    var saveables = GameObject.FindObjectsOfType<SimpleSaveable>();

                    var saveManager = GameObject.FindObjectOfType<WorldSaveManager>();
                    Assert.NotNull(saveManager);
                    var saveable1 = saveables.Where(x => x.gameObject.name == "save object 1").FirstOrDefault();
                    Assert.NotNull(saveable1);
                    var saveable2 = saveables.Where(x => x.gameObject.name == "save object two").FirstOrDefault();
                    Assert.NotNull(saveable2);
                    var saveable3 = saveables.Where(x => x.gameObject.name == "save object four").FirstOrDefault();
                    Assert.NotNull(saveable2);

                    Assert.AreEqual("I am save data 1 in scene B", saveable1.MySavedData);
                    Assert.AreEqual("my global save data is different... again", saveable2.MySavedData);
                    Assert.AreEqual("my global save data four is different, but only in scene B", saveable3.MySavedData);

                    yield return null;
                }

                yield return new ExitPlayMode();
                yield return null;

            }
            finally
            {
                EditorBuildSettings.scenes = oldSceneBuilds;

                Object.DestroyImmediate(prefabRegistry);

                AssetDatabase.DeleteAsset("Assets/SceneA.unity");
                AssetDatabase.DeleteAsset("Assets/SceneB.unity");
                WorldSaveManager.DeleteSaveData();
            }

            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }


        [UnityTest]
        public IEnumerator RetainsWholeGlobalScopeDataBetweenScenesReconcilesEmptyPrefabParentCorrectly()
        {
            {
                var prefabRegistry = ScriptableObject.CreateInstance<SaveablePrefabRegistry>();
                var saveablePrefab = CreateSavablePrefab("topLevel");
                var prefabType = AssetDatabase.LoadAssetAtPath<SaveablePrefabType>("Assets/topLevel_type.asset");
                prefabRegistry.allObjects = new List<SaveablePrefabType> { prefabType };
                prefabRegistry.AssignAllIDs();
                AssetDatabase.CreateAsset(prefabRegistry, $"Assets/prefab_type_registry.asset");
            }

            var oldSceneBuilds = EditorBuildSettings.scenes.ToList().ToArray();
            try
            {
                {
                    // setup scene A
                    var testSceneA = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                    var saveablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/topLevel_prefab.prefab");

                    var prefabParentObject = new GameObject("prefab parent");
                    prefabParentObject.AddComponent<GlobalSaveFlag>();
                    prefabParentObject.AddComponent<SaveablePrefabParent>().prefabParentName = "a parent to prefabs";

                    var prefabParentObjectTwo = new GameObject("prefab parent two");
                    prefabParentObjectTwo.AddComponent<GlobalSaveFlag>();
                    prefabParentObjectTwo.AddComponent<SaveablePrefabParent>().prefabParentName = "another parent to prefabs";

                    var nextPrefab = GameObject.Instantiate(saveablePrefab, prefabParentObjectTwo.transform);
                    nextPrefab.transform.GetChild(0).GetComponent<SimpleSaveable>().MySavedData = "third prefab! in another!";

                    var saveManagerObject = new GameObject("save manager");
                    var saveManager = saveManagerObject.AddComponent<WorldSaveManager>();
                    var prefabRegistry = AssetDatabase.LoadAssetAtPath<SaveablePrefabRegistry>("Assets/prefab_type_registry.asset");
                    saveManager.saveablePrefabRegistry = prefabRegistry;

                    testSceneA.name = "SceneA";
                    EditorSceneManager.SaveScene(testSceneA, $"Assets/SceneA.unity");
                }

                {
                    // setup scene B
                    var testSceneB = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                    var prefabParentObject = new GameObject("prefab parent");
                    prefabParentObject.AddComponent<GlobalSaveFlag>();
                    prefabParentObject.AddComponent<SaveablePrefabParent>().prefabParentName = "a parent to prefabs";

                    var saveManagerObject = new GameObject("save manager");
                    var saveManager = saveManagerObject.AddComponent<WorldSaveManager>();
                    var prefabRegistry = AssetDatabase.LoadAssetAtPath<SaveablePrefabRegistry>("Assets/prefab_type_registry.asset");
                    saveManager.saveablePrefabRegistry = prefabRegistry;

                    testSceneB.name = "SceneB";
                    EditorSceneManager.SaveScene(testSceneB, $"Assets/SceneB.unity");
                }

                EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
                {
                    new EditorBuildSettingsScene("Assets/SceneA.unity", true),
                    new EditorBuildSettingsScene("Assets/SceneB.unity", true)
                };

                var activeScene = EditorSceneManager.OpenScene("Assets/SceneB.unity", OpenSceneMode.Single);

                yield return new EnterPlayMode();

                SaveContext.instance.saveName = "test_save";
                WorldSaveManager.DeleteSaveData();


                {
                    // save scene B, load scene A
                    //  done to ensure that global save data flags, when not present, will behave as if loading fresh
                    var saveManager = GameObject.FindObjectOfType<WorldSaveManager>();

                    var prefabParent = GameObject.FindObjectOfType<SaveablePrefabParent>();

                    Assert.AreEqual(0, prefabParent.transform.childCount);

                    saveManager.SaveActiveScene();

                    yield return saveManager.LoadCoroutine(new SceneReference("Assets/SceneA.unity"));
                    yield return null;
                }

                {
                    // Assert scene A state, add some prefabs, and save

                    var saveablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/topLevel_prefab.prefab");
                    var prefabParent = GameObject.FindObjectsOfType<SaveablePrefabParent>().Where(x => x.prefabParentName == "a parent to prefabs").FirstOrDefault();

                    var nextPrefab = GameObject.Instantiate(saveablePrefab, prefabParent.transform);
                    nextPrefab.transform.GetChild(0).GetComponent<SimpleSaveable>().MySavedData = "first!";
                    nextPrefab = GameObject.Instantiate(saveablePrefab, prefabParent.transform);
                    nextPrefab.transform.GetChild(0).GetComponent<SimpleSaveable>().MySavedData = "second prefab instance";

                    var prefabParentTwo = GameObject.FindObjectsOfType<SaveablePrefabParent>().Where(x => x.prefabParentName == "another parent to prefabs").FirstOrDefault();
                    Assert.AreEqual(1, prefabParentTwo.transform.childCount);
                    Assert.AreEqual("third prefab! in another!", prefabParentTwo.transform.GetChild(0).GetChild(0).GetComponent<SimpleSaveable>().MySavedData);

                    nextPrefab = GameObject.Instantiate(saveablePrefab, prefabParentTwo.transform);
                    nextPrefab.transform.GetChild(0).GetComponent<SimpleSaveable>().MySavedData = "fourth prefab! in another!";

                    var saveManager = GameObject.FindObjectOfType<WorldSaveManager>();

                    saveManager.SaveActiveScene();

                    yield return saveManager.LoadCoroutine(new SceneReference("Assets/SceneB.unity"));
                    yield return null;
                }

                {
                    // Assert global save data, modify and save scene B
                    var saveManager = GameObject.FindObjectOfType<WorldSaveManager>();

                    var prefabParent = GameObject.FindObjectOfType<SaveablePrefabParent>();

                    Assert.AreEqual(2, prefabParent.transform.childCount);
                    Assert.AreEqual("first!", prefabParent.transform.GetChild(0).GetChild(0).GetComponent<SimpleSaveable>().MySavedData);
                    Assert.AreEqual("second prefab instance", prefabParent.transform.GetChild(1).GetChild(0).GetComponent<SimpleSaveable>().MySavedData);

                    GameObject.Destroy(prefabParent.transform.GetChild(0).gameObject);
                    GameObject.Destroy(prefabParent.transform.GetChild(1).gameObject);

                    yield return null;

                    Assert.AreEqual(0, prefabParent.transform.childCount);

                    saveManager.SaveActiveScene();

                    yield return saveManager.LoadCoroutine(new SceneReference("Assets/SceneA.unity"));
                    yield return null;
                }

                {
                    // Assert save data changes from global and scene A
                    var prefabParentTwo = GameObject.FindObjectsOfType<SaveablePrefabParent>().Where(x => x.prefabParentName == "another parent to prefabs").FirstOrDefault();
                    Assert.AreEqual(2, prefabParentTwo.transform.childCount);
                    Assert.AreEqual("third prefab! in another!", prefabParentTwo.transform.GetChild(0).GetChild(0).GetComponent<SimpleSaveable>().MySavedData);
                    Assert.AreEqual("fourth prefab! in another!", prefabParentTwo.transform.GetChild(1).GetChild(0).GetComponent<SimpleSaveable>().MySavedData);

                    var prefabParent = GameObject.FindObjectsOfType<SaveablePrefabParent>().Where(x => x.prefabParentName == "a parent to prefabs").FirstOrDefault();
                    Assert.AreEqual(0, prefabParent.transform.childCount);


                    yield return null;
                }

                yield return new ExitPlayMode();
                yield return null;

            }
            finally
            {
                EditorBuildSettings.scenes = oldSceneBuilds;

                AssetDatabase.DeleteAsset("Assets/SceneA.unity");
                AssetDatabase.DeleteAsset("Assets/SceneB.unity");
                AssetDatabase.DeleteAsset("Assets/topLevel_type.asset");
                AssetDatabase.DeleteAsset("Assets/topLevel_prefab.prefab");
                AssetDatabase.DeleteAsset("Assets/prefab_type_registry.asset");
                WorldSaveManager.DeleteSaveData();
            }

            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }


        [UnityTest]
        public IEnumerator RemembersLastSavedSceneWhenLoading()
        {
            var prefabRegistry = ScriptableObject.CreateInstance<SaveablePrefabRegistry>();

            var oldSceneBuilds = EditorBuildSettings.scenes.ToList().ToArray();
            try
            {
                {
                    // setup scene A
                    var testSceneA = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                    var savedObject1 = new GameObject("save object 1");
                    var saveable1 = savedObject1.AddComponent<SimpleSaveable>();
                    saveable1.MySavedData = "I am save data 1 in scene A";

                    var saveManagerObject = new GameObject("save manager");
                    var saveManager = saveManagerObject.AddComponent<WorldSaveManager>();

                    saveManager.saveablePrefabRegistry = prefabRegistry;

                    testSceneA.name = "SceneA";
                    EditorSceneManager.SaveScene(testSceneA, $"Assets/SceneA.unity");
                }

                {
                    // setup scene B
                    var testSceneB = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                    var savedObject1 = new GameObject("save object 1");
                    var saveable1 = savedObject1.AddComponent<SimpleSaveable>();
                    saveable1.MySavedData = "I am save data 1 in scene B";

                    var saveManagerObject = new GameObject("save manager");
                    var saveManager = saveManagerObject.AddComponent<WorldSaveManager>();

                    saveManager.saveablePrefabRegistry = prefabRegistry;

                    testSceneB.name = "SceneB";
                    EditorSceneManager.SaveScene(testSceneB, $"Assets/SceneB.unity");
                }

                EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
                {
                    new EditorBuildSettingsScene("Assets/SceneA.unity", true),
                    new EditorBuildSettingsScene("Assets/SceneB.unity", true)
                };

                var activeScene = EditorSceneManager.OpenScene("Assets/SceneA.unity", OpenSceneMode.Single);

                yield return new EnterPlayMode();
                SaveContext.instance.saveName = "test_save";
                WorldSaveManager.DeleteSaveData();

                {
                    // modify and save scene A
                    var saveManager = GameObject.FindObjectOfType<WorldSaveManager>();
                    Assert.NotNull(saveManager);
                    var saveable1 = GameObject.FindObjectOfType<SimpleSaveable>(); ;
                    Assert.NotNull(saveable1);

                    Assert.AreEqual("I am save data 1 in scene A", saveable1.MySavedData);
                    saveable1.MySavedData = "my save data has changed!";

                    saveManager.SaveActiveScene();

                    Assert.AreEqual("my save data has changed!", saveable1.MySavedData);

                    yield return saveManager.LoadCoroutine(new SceneReference("Assets/SceneB.unity"));
                    yield return null;
                }

                {
                    // Modify and save scene B
                    var saveManager = GameObject.FindObjectOfType<WorldSaveManager>();
                    Assert.NotNull(saveManager);
                    var saveable1 = GameObject.FindObjectOfType<SimpleSaveable>(); ;
                    Assert.NotNull(saveable1);

                    Assert.AreEqual("I am save data 1 in scene B", saveable1.MySavedData);
                    saveable1.MySavedData = "my save data has changed in scene B!";

                    saveManager.SaveActiveScene();

                    yield return saveManager.LoadCoroutine(new SceneReference("Assets/SceneA.unity"));
                    yield return null;
                }

                {
                    // Assert save data changes from scene A. but don't save. then load the last saved scene (scene B)
                    var saveManager = GameObject.FindObjectOfType<WorldSaveManager>();
                    Assert.NotNull(saveManager);
                    var saveable1 = GameObject.FindObjectOfType<SimpleSaveable>(); ;
                    Assert.NotNull(saveable1);

                    Assert.AreEqual("my save data has changed!", saveable1.MySavedData);

                    yield return saveManager.LoadCoroutine(null);
                    yield return null;
                }

                {
                    // Assert that Scene B has been loaded
                    var saveManager = GameObject.FindObjectOfType<WorldSaveManager>();
                    Assert.NotNull(saveManager);
                    var saveable1 = GameObject.FindObjectOfType<SimpleSaveable>(); ;
                    Assert.NotNull(saveable1);

                    Assert.AreEqual("my save data has changed in scene B!", saveable1.MySavedData);

                    saveManager.SaveActiveScene();

                    yield return saveManager.LoadCoroutine(new SceneReference("Assets/SceneA.unity"));
                    yield return null;
                }

                yield return new ExitPlayMode();
                yield return null;

            }
            finally
            {
                EditorBuildSettings.scenes = oldSceneBuilds;

                Object.DestroyImmediate(prefabRegistry);

                AssetDatabase.DeleteAsset("Assets/SceneA.unity");
                AssetDatabase.DeleteAsset("Assets/SceneB.unity");
                WorldSaveManager.DeleteSaveData();
            }

            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }


        [UnityTest]
        public IEnumerator CyclesWithSavedPrefabsInSpecifiedLoadOrder()
        {
            var testScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var testSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(testScene.path);
            try
            {

                {
                    var topLevelSavedObject = new GameObject("save object 1");
                    var saveable = topLevelSavedObject.AddComponent<SimpleSaveable>();
                    saveable.MySavedData = "I am save data 1 with default load order";
                    saveable.LoadOrderPriority = 0;
                    saveable.uniqueNameInScope = "one";

                    topLevelSavedObject = new GameObject("save object 2");
                    saveable = topLevelSavedObject.AddComponent<SimpleSaveable>();
                    saveable.MySavedData = "I am save data 2 with post-prefab load order";
                    saveable.LoadOrderPriority = 1001;
                    saveable.uniqueNameInScope = "two";

                    topLevelSavedObject = new GameObject("save object 3");
                    saveable = topLevelSavedObject.AddComponent<SimpleSaveable>();
                    saveable.MySavedData = "I am save data 3 with pre-everything load order";
                    saveable.LoadOrderPriority = -1000;
                    saveable.uniqueNameInScope = "three";

                    topLevelSavedObject = new GameObject("save object 4");
                    saveable = topLevelSavedObject.AddComponent<SimpleSaveable>();
                    saveable.MySavedData = "I am save data 4 with pre-most things load order";
                    saveable.LoadOrderPriority = -10;
                    saveable.uniqueNameInScope = "four";

                    var saveablePrefab = CreateSavablePrefab("topLevel");
                    var prefabRegistry = ScriptableObject.CreateInstance<SaveablePrefabRegistry>();
                    prefabRegistry.allObjects = new List<SaveablePrefabType> { saveablePrefab.GetComponent<SaveablePrefab>().myPrefabType };
                    prefabRegistry.AssignAllIDs();

                    var prefabParentObject = new GameObject("prefab parent");
                    prefabParentObject.AddComponent<SaveablePrefabParent>().prefabParentName = "a parent to prefabs";

                    var saveManagerObject = new GameObject("save manager");
                    var saveManager = saveManagerObject.AddComponent<WorldSaveManager>();

                    saveManager.saveablePrefabRegistry = prefabRegistry;
                }


                yield return new EnterPlayMode();

                SaveContext.instance.saveName = "test_save";
                WorldSaveManager.DeleteSaveData();
                {
                    var saveablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/topLevel_prefab.prefab");
                    var prefabParent = GameObject.FindObjectOfType<SaveablePrefabParent>();

                    var nextPrefab = GameObject.Instantiate(saveablePrefab, prefabParent.transform);
                    var nextSaveable = nextPrefab.transform.GetChild(0).GetComponent<SimpleSaveable>();
                    nextSaveable.MySavedData = "first!";
                    nextPrefab = GameObject.Instantiate(saveablePrefab, prefabParent.transform);
                    nextSaveable = nextPrefab.transform.GetChild(0).GetComponent<SimpleSaveable>();
                    nextSaveable.MySavedData = "second prefab instance";

                    var saveables = GameObject.FindObjectsOfType<SimpleSaveable>();

                    var saveManager = GameObject.FindObjectOfType<WorldSaveManager>();
                    Assert.NotNull(saveManager);

                    saveManager.SaveActiveScene();
                }
                var sharedSaveManager = GameObject.FindObjectOfType<WorldSaveManager>();
                var loadOrderCapture = new CapturedLoadOrderAction();
                SaveContext.instance.saveName = "test_save";
                yield return loadOrderCapture.CaptureLoadOrderAndLoad(sharedSaveManager);
                var loadedObjectsInOrder = loadOrderCapture.capturedLoadOrder;

                var expectedSaveableOrder = new List<string>
                {
                    "I am save data 3 with pre-everything load order",
                    "I am save data 4 with pre-most things load order",
                    "I am save data 1 with default load order",
                    "first!",
                    "second prefab instance",
                    "I am save data 2 with post-prefab load order"
                };

                Assert.AreEqual(expectedSaveableOrder.Count, loadedObjectsInOrder.Count);

                for (int i = 0; i < expectedSaveableOrder.Count; i++)
                {
                    Assert.AreEqual(
                        expectedSaveableOrder[i],
                        loadedObjectsInOrder[i].MySavedData,
                        $"Expected '{expectedSaveableOrder[i]}' to be saved at index {i}, but was '{loadedObjectsInOrder[i].MySavedData}'");
                }

                yield return new ExitPlayMode();
                yield return null;

                Object.DestroyImmediate(testSceneAsset);

            }
            finally
            {
                SaveContext.instance.saveName = "test_save";
                AssetDatabase.DeleteAsset("Assets/topLevel_type.asset");
                AssetDatabase.DeleteAsset("Assets/topLevel_prefab.prefab");
                WorldSaveManager.DeleteSaveData();
            }

            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }

        private class CapturedLoadOrderAction
        {
            public List<SimpleSaveable> capturedLoadOrder;

            public IEnumerator CaptureLoadOrderAndLoad(WorldSaveManager saveManager)
            {
                SaveSystemHooks.Instance.MidLoad += SetupCallbackHook;
                SaveSystemHooks.Instance.PostLoad += TeardownCallbackHook;

                capturedLoadOrder = new List<SimpleSaveable>();

                yield return saveManager.LoadCoroutine(new SceneReference(""));
                yield return null;
            }

            private void TeardownCallbackHook(SceneReference scene)
            {
                SimpleSaveable.OnLoad -= SimpleSaveableLoaded;
                SaveSystemHooks.Instance.MidLoad -= SetupCallbackHook;
                SaveSystemHooks.Instance.PostLoad -= TeardownCallbackHook;
            }

            private void SetupCallbackHook(SceneReference scene)
            {
                SimpleSaveable.OnLoad += SimpleSaveableLoaded;
            }

            private void SimpleSaveableLoaded(SimpleSaveable obj)
            {
                capturedLoadOrder.Add(obj);
            }
        }


        // eventually write test to assert load ordering is respected, inside and out of prefabs
    }
}
