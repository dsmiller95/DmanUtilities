using Dman.SceneSaveSystem.PlaymodeTests;
using NUnit.Framework;
using System.Collections;
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

            saveManager.saveLoadScene = new Utilities.SceneReference()
            {
                scenePath = testScene.path
            };

            SaveContext.instance.saveName = "test_save";

            yield return new EnterPlayMode();


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

            saveManager.Save();

            saveable2.MySavedData = "my save data has changed, two!!";
            Assert.AreEqual("my save data has changed!", saveable1.MySavedData);
            Assert.AreEqual("my save data has changed, two!!", saveable2.MySavedData);

            yield return saveManager.LoadCoroutine();
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

            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }

        private GameObject CreateSavablePrefab(string topLevelName, System.Action<GameObject> extraSetup = null)
        {
            var prefabType = ScriptableObject.CreateInstance<SaveablePrefabType>();
            AssetDatabase.CreateAsset(prefabType, $"Assets/{topLevelName}_type.asset");

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
            return prefab;
        }

        [UnityTest]
        public IEnumerator CyclesWithSavedPrefabs()
        {
            var testScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var testSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(testScene.path);

            var topLevelSavedObject = new GameObject("save object 1");
            var saveable = topLevelSavedObject.AddComponent<SimpleSaveable>();
            saveable.MySavedData = "I am save data 1";

            var saveablePrefab = CreateSavablePrefab("topLevel");
            var prefabRegistry = ScriptableObject.CreateInstance<SaveablePrefabRegistry>();
            prefabRegistry.allObjects = new SaveablePrefabType[] { saveablePrefab.GetComponent<SaveablePrefab>().myPrefabType };
            prefabRegistry.AssignAllIDs();

            var prefabParentObject = new GameObject("prefab parent");
            prefabParentObject.AddComponent<SaveablePrefabParent>().prefabParentName = "a parent to prefabs";

            var saveManagerObject = new GameObject("save manager");
            var saveManager = saveManagerObject.AddComponent<WorldSaveManager>();
            saveManager.saveLoadScene = new Utilities.SceneReference()
            {
                scenePath = testScene.path
            };

            saveManager.saveablePrefabRegistry = prefabRegistry;


            SaveContext.instance.saveName = "test_save";

            yield return new EnterPlayMode();

            saveablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/topLevel_prefab.prefab");
            var prefabParent = GameObject.FindObjectOfType<SaveablePrefabParent>();

            var nextPrefab = GameObject.Instantiate(saveablePrefab, prefabParent.transform);
            nextPrefab.transform.GetChild(0).GetComponent<SimpleSaveable>().MySavedData = "first!";
            nextPrefab = GameObject.Instantiate(saveablePrefab, prefabParent.transform);
            nextPrefab.transform.GetChild(0).GetComponent<SimpleSaveable>().MySavedData = "second prefab instance";
            nextPrefab = GameObject.Instantiate(saveablePrefab, prefabParent.transform);
            nextPrefab.transform.GetChild(0).GetComponent<SimpleSaveable>().MySavedData = "third prefab instance";

            var saveables = GameObject.FindObjectsOfType<SimpleSaveable>();

            saveManager = GameObject.FindObjectOfType<WorldSaveManager>();
            Assert.NotNull(saveManager);
            saveable = saveables.Where(x => x.gameObject.name == "save object 1").FirstOrDefault();
            Assert.NotNull(saveable);

            Assert.AreEqual("I am save data 1", saveable.MySavedData);
            saveable.MySavedData = "my save data has changed!";

            saveManager.Save();

            Assert.AreEqual("my save data has changed!", saveable.MySavedData);

            yield return saveManager.LoadCoroutine();
            yield return null;

            saveables = GameObject.FindObjectsOfType<SimpleSaveable>();

            saveManager = GameObject.FindObjectOfType<WorldSaveManager>();
            Assert.NotNull(saveManager);
            saveable = saveables.Where(x => x.gameObject.name == "save object 1").FirstOrDefault();
            Assert.NotNull(saveable);

            Assert.AreEqual("my save data has changed!", saveable.MySavedData);

            prefabParent = GameObject.FindObjectOfType<SaveablePrefabParent>();
            Assert.NotNull(prefabParent);
            Assert.AreEqual(3, prefabParent.transform.childCount);
            Assert.AreEqual("first!", prefabParent.transform.GetChild(0).GetChild(0).GetComponent<SimpleSaveable>().MySavedData);
            Assert.AreEqual("second prefab instance", prefabParent.transform.GetChild(1).GetChild(0).GetComponent<SimpleSaveable>().MySavedData);
            Assert.AreEqual("third prefab instance", prefabParent.transform.GetChild(2).GetChild(0).GetComponent<SimpleSaveable>().MySavedData);

            yield return new ExitPlayMode();
            yield return null;

            Object.DestroyImmediate(testSceneAsset);
            Object.DestroyImmediate(prefabRegistry);

            AssetDatabase.DeleteAsset("Assets/topLevel_type.asset");
            AssetDatabase.DeleteAsset("Assets/topLevel_prefab.prefab");

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
                var topLevelPrefab = CreateSavablePrefab("topLevel", go =>
                {
                    var nestedObject = new GameObject("prefab parent in prefab");
                    nestedObject.transform.parent = go.transform;
                    var prefabParent = nestedObject.AddComponent<SaveablePrefabParent>();
                    prefabParent.prefabParentName = "NestedPrefabParent";
                });
                var nestedPrefab = CreateSavablePrefab("nested");

                prefabRegistry.allObjects = new SaveablePrefabType[] {
                    topLevelPrefab.GetComponent<SaveablePrefab>().myPrefabType,
                    nestedPrefab.GetComponent<SaveablePrefab>().myPrefabType
                };
                prefabRegistry.AssignAllIDs();

                var prefabParentObject = new GameObject("prefab parent in scene");
                prefabParentObject.AddComponent<SaveablePrefabParent>().prefabParentName = "ScenePrefabParent";

                var saveManagerObject = new GameObject("save manager");
                var saveManager = saveManagerObject.AddComponent<WorldSaveManager>();
                saveManager.saveLoadScene = new Utilities.SceneReference()
                {
                    scenePath = testScene.path
                };

                saveManager.saveablePrefabRegistry = prefabRegistry;


                SaveContext.instance.saveName = "test_save";

                yield return new EnterPlayMode();

                topLevelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/topLevel_prefab.prefab");
                nestedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/nested_prefab.prefab");
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

                saveManager = GameObject.FindObjectOfType<WorldSaveManager>();
                Assert.NotNull(saveManager);

                saveManager.Save();

                yield return saveManager.LoadCoroutine();
                yield return null;

                saveables = GameObject.FindObjectsOfType<SimpleSaveable>();

                saveManager = GameObject.FindObjectOfType<WorldSaveManager>();
                Assert.NotNull(saveManager);

                prefabParentInScene = GameObject.FindObjectsOfType<SaveablePrefabParent>().Where(x => x.prefabParentName == "ScenePrefabParent").First();
                Assert.NotNull(prefabParentInScene);
                Assert.AreEqual(3, prefabParentInScene.transform.childCount);

                {
                    Assert.AreEqual("first!", prefabParentInScene.transform.GetChild(0).GetChild(0).GetComponent<SimpleSaveable>().MySavedData);
                    var prefabParentInPrefab = prefabParentInScene.transform.GetChild(0).GetComponentInChildren<SaveablePrefabParent>();
                    Assert.AreEqual(3, prefabParentInPrefab.transform.childCount);
                    Assert.AreEqual("first nested!", prefabParentInPrefab.transform.GetChild(0).GetChild(0).GetComponent<SimpleSaveable>().MySavedData);
                    Assert.AreEqual("second nested!", prefabParentInPrefab.transform.GetChild(1).GetChild(0).GetComponent<SimpleSaveable>().MySavedData);
                    Assert.AreEqual("third nested!", prefabParentInPrefab.transform.GetChild(2).GetChild(0).GetComponent<SimpleSaveable>().MySavedData);
                }
                {
                    Assert.AreEqual("second prefab instance", prefabParentInScene.transform.GetChild(1).GetChild(0).GetComponent<SimpleSaveable>().MySavedData);
                    var prefabParentInPrefab = prefabParentInScene.transform.GetChild(0).GetComponentInChildren<SaveablePrefabParent>();
                    Assert.AreEqual(0, prefabParentInPrefab.transform.childCount);
                }
                {
                    Assert.AreEqual("third prefab instance", prefabParentInScene.transform.GetChild(2).GetChild(0).GetComponent<SimpleSaveable>().MySavedData);
                    var prefabParentInPrefab = prefabParentInScene.transform.GetChild(0).GetComponentInChildren<SaveablePrefabParent>();
                    Assert.AreEqual(2, prefabParentInPrefab.transform.childCount);
                    Assert.AreEqual("first third nested!", prefabParentInPrefab.transform.GetChild(0).GetChild(0).GetComponent<SimpleSaveable>().MySavedData);
                    Assert.AreEqual("second third nested!", prefabParentInPrefab.transform.GetChild(1).GetChild(0).GetComponent<SimpleSaveable>().MySavedData);
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

        // TODO: test that global scope save data is transferred between scenes
    }
}
