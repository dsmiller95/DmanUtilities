using System;
using UnityEngine.SceneManagement;

namespace Dman.SceneSaveSystem
{
    [Serializable]
    public class SceneSaveScopeIdentifier : ISaveScopeIdentifier
    {
        public string scenePath;
        public string sceneName;

        public SceneSaveScopeIdentifier(Scene scene)
        {
            this.scenePath = scene.path;
            this.sceneName = scene.name;
        }

        public string UniqueSemiReadableName
        {
            get
            {
                var pathBytes = System.Text.Encoding.UTF8.GetBytes(scenePath);
                var base64Path = System.Convert.ToBase64String(pathBytes);
                return $"Scene_{sceneName}_{base64Path}";
            }
        }

        public bool Equals(ISaveScopeIdentifier other)
        {
            if (!(other is SceneSaveScopeIdentifier casted))
            {
                return false;
            }
            return casted.scenePath == scenePath && casted.sceneName == sceneName;
        }
    }
}
