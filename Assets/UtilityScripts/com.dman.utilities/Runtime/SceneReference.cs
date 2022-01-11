using System;
using UnityEngine.SceneManagement;

namespace Dman.Utilities
{
    [Serializable]
    public class SceneReference
    {
        public static SceneReference Active => new SceneReference(SceneManager.GetActiveScene().path);

        public string scenePath;
        public int BuildIndex => SceneUtility.GetBuildIndexByScenePath(scenePath);

        public Scene ScenePointer => SceneManager.GetSceneByBuildIndex(BuildIndex);


        public SceneReference()
        {

        }
        public SceneReference(string path)
        {
            this.scenePath = path;
        }

    }
}
