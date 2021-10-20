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

        private GameObject CreateSavablePrefab()
        {
            var prefabType = ScriptableObject.CreateInstance<SaveablePrefabType>();
            AssetDatabase.CreateAsset(prefabType, "Assets/Test_Prefab_type.asset");

            var prefabObject = new GameObject("saveable Prefab");
            var saveablePrefab = prefabObject.AddComponent<SaveablePrefab>();
            saveablePrefab.myPrefabType = prefabType;

            var nestedObject = new GameObject("data inside prefab");
            nestedObject.transform.parent = prefabObject.transform;
            var savedPrefab = nestedObject.AddComponent<SimpleSaveable>();
            savedPrefab.MySavedData = "inside a prefab!";

            var prefab = PrefabUtility.SaveAsPrefabAsset(prefabObject, "Assets/Test_Prefab.prefab");

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

            var saveablePrefab = CreateSavablePrefab();
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

            saveablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Test_Prefab.prefab");
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

            AssetDatabase.DeleteAsset("Assets/Test_Prefab_type.asset");
            AssetDatabase.DeleteAsset("Assets/Test_Prefab.asset");

            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }

        // TODO: test that global scope save data is transferred between scenes
    }
}
