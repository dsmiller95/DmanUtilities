using UnityEngine;

namespace Dman.SceneSaveSystem
{
    /// <summary>
    /// Attach to a game object anywhere in the scene scope, and that game object and all below it will be saved
    ///     in a global scope, and will be loaded again in any other scene that is instantiated
    /// useful for things like player currency balances, or score
    /// </summary>
    public class GlobalSaveFlag : MonoBehaviour
    {
    }
}