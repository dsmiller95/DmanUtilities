using Dman.Utilities;
using UnityEngine;

namespace Dman.SceneSaveSystem
{
    /// <summary>
    /// used to switch the current scene to another scene
    ///     preserving current state and loading new state
    /// </summary>
    public class SwitchScene : MonoBehaviour
    {
        public SceneReference targetScene;
        public WorldSaveManager saveManager;

        public void SwitchToTargetScene()
        {
            saveManager.SaveActiveScene();

            saveManager.Load(targetScene.scenePath);
        }
    }
}