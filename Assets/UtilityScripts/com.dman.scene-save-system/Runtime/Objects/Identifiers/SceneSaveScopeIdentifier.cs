using Dman.Utilities;
using System;
using UnityEngine.SceneManagement;

namespace Dman.SceneSaveSystem.Objects.Identifiers
{
    [Serializable]
    internal class SceneSaveScopeIdentifier : ISaveScopeIdentifier
    {
        public SceneReference scene;

        public SceneSaveScopeIdentifier(SceneReference scene)
        {
            this.scene = scene;
        }

        public string UniqueSemiReadableName
        {
            get
            {
                var pathBytes = System.Text.Encoding.UTF8.GetBytes(scene.scenePath);
                var base64Path = System.Convert.ToBase64String(pathBytes);
                return $"Scene_{scene.Name}_{base64Path}";
            }
        }

        public bool Equals(ISaveScopeIdentifier other)
        {
            if (!(other is SceneSaveScopeIdentifier casted))
            {
                return false;
            }
            return casted.scene == scene;
        }
    }
}
