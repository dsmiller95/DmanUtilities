using Dman.Utilities;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dman.SceneSaveSystem
{
    /// <summary>
    /// API class to expose static methods for use in scripting, and instance methods to use when
    ///     binding unity events
    /// </summary>
    public class WorldSaveCalls : MonoBehaviour
    {
        private static WorldSaveManager saveManager => GameObject.FindObjectOfType<WorldSaveManager>();

        public static class StaticAPI
        {
            public static void SaveActiveScene()
            {
                saveManager.Save(SceneReference.Active);
            }
            /// <summary>
            /// Save the given scene synchronously. When method returns, save is complete
            /// </summary>
            public static void Save(SceneReference sceneToSave)
            {
                saveManager.Save(sceneToSave);
            }
            /// <summary>
            /// clear save data for the given scene
            /// </summary>
            public static void ClearSave(SceneReference sceneToSave)
            {
                saveManager.DeleteSaveData(sceneToSave);
            }
            /// <summary>
            /// clear save data in the global scope
            /// </summary>
            public static void ClearGlobalSave()
            {
                WorldSaveManager.DeleteGlobalSaveData();
            }

            /// <summary>
            /// delete all save data
            /// </summary>
            public static void ClearAllSaveData()
            {
                WorldSaveManager.DeleteSaveData();
            }

            /// <summary>
            /// loop through all active scenes and save them all in order
            /// </summary>
            public static void SaveAll()
            {
                foreach (var scene in SceneReference.Loaded)
                {
                    if (scene.IsLoaded)
                    {
                        saveManager.Save(scene);
                    }
                }
            }

            /// <summary>
            /// unload the active scene, and wait for the scene at <paramref name="scenePath"/> to be loaded.
            /// </summary>
            public static void Load(SceneReference sceneToLoad, LoadSceneMode loadMode = LoadSceneMode.Single)
            {
                saveManager.Load(sceneToLoad, loadMode);
            }

            /// <summary>
            /// unload the active scene, and wait for the scene at <paramref name="scenePath"/> to be loaded.
            /// </summary>
            public static IEnumerator LoadCoroutine(SceneReference sceneToLoad, LoadSceneMode loadMode = LoadSceneMode.Single)
            {
                return saveManager.LoadCoroutine(sceneToLoad, loadMode);
            }

            /// <summary>
            /// loads the scene last saved in the current save name. Use this when, for example, you are loading a whole game
            ///     from a menu screen.
            /// Same as Load(), but with null scene Path. useful for binding to unity events.
            /// </summary>
            public static void LoadLastSavedScene()
            {
                saveManager.LoadLastSavedScene();
            }
        }

        public void SaveActiveScene()
        {
            WorldSaveCalls.StaticAPI.SaveActiveScene();
        }

        public void SaveMyScene()
        {
            this.Save(new SceneReference(gameObject.scene));
        }

        public void Save(SceneReference sceneToSave)
        {
            WorldSaveCalls.StaticAPI.Save(sceneToSave);
        }

        public void SaveAll()
        {
            StaticAPI.SaveAll();
        }
        public void Load(SceneReference sceneToLoad, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            StaticAPI.Load(sceneToLoad, loadMode);
        }

        /// <summary>
        /// unload the active scene, and wait for the scene at <paramref name="scenePath"/> to be loaded.
        /// </summary>
        public IEnumerator LoadCoroutine(SceneReference sceneToLoad, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            return StaticAPI.LoadCoroutine(sceneToLoad, loadMode);
        }

        /// <summary>
        /// loads the scene last saved in the current save name. Use this when, for example, you are loading a whole game
        ///     from a menu screen.
        /// Same as Load(), but with null scene Path. useful for binding to unity events.
        /// </summary>
        public void LoadLastSavedScene()
        {
            StaticAPI.LoadLastSavedScene();
        }
    }
}
