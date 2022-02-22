using UnityEngine;

namespace Dman.SceneSaveSystem
{
    /// <summary>
    /// Attach to a game object anywhere in the scene scope, and that game object and all below it will not be saved
    /// useful when using behaviors which are saveable in a transient way, which should not be serialized
    /// </summary>
    public class DontSaveFlag : MonoBehaviour
    {
    }
}