using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dman.SceneSaveSystem
{
    [Serializable]
    public class SceneSaveScopeIdentifier : ISaveScopeIdentifier
    {
        public int sceneId;
        public string scenePath;
        public string sceneName;

        public SceneSaveScopeIdentifier(Scene scene)
        {
            this.sceneId = scene.handle;
            this.scenePath = scene.path;
            this.sceneName = scene.name;
        }

        public string UniqueSemiReadableName => $"Scene_{sceneId}_{sceneName}";

        public bool Equals(ISaveScopeIdentifier other)
        {
            if(!(other is SceneSaveScopeIdentifier casted))
            {
                return false;
            }
            return casted.sceneId == sceneId && casted.scenePath == scenePath && casted.sceneName == sceneName;
        }
    }
}
