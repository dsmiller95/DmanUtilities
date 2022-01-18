using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dman.Utilities
{
    [Serializable]
    public class SceneReference
    {
        public static SceneReference Active => new SceneReference(SceneManager.GetActiveScene().path);
        public static List<SceneReference> Loaded => LoadedGenerator.ToList();
        private static IEnumerable<SceneReference> LoadedGenerator
        {
            get
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    yield return new SceneReference(SceneManager.GetSceneAt(i));
                }
            }
        }

        public string scenePath;
        public string Name => System.IO.Path.GetFileNameWithoutExtension(scenePath);
        public int BuildIndex => SceneUtility.GetBuildIndexByScenePath(scenePath);

        public Scene ScenePointerIfLoaded => SceneManager.GetSceneByBuildIndex(BuildIndex);

        public bool IsLoaded => ScenePointerIfLoaded.isLoaded;

        public SceneReference()
        {

        }
        public SceneReference(string path)
        {
            this.scenePath = path;
        }
        public SceneReference(Scene sceneObject)
        {
            this.scenePath = sceneObject.path;
        }

        public bool ContainsGameObject(GameObject go)
        {
            return go.scene.buildIndex == this.BuildIndex;
        }

        public override bool Equals(object obj)
        {
            if (obj is SceneReference sceneRef)
            {
                return sceneRef == this;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return scenePath.GetHashCode();
        }

        public static bool operator ==(SceneReference a, SceneReference b) => a?.scenePath == b?.scenePath;
        public static bool operator !=(SceneReference a, SceneReference b) => !(a.scenePath == b.scenePath);
    }
}
