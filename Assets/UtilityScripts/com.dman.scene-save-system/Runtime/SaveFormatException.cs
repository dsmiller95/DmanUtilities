using System.Collections;
using UnityEngine;

namespace Dman.SceneSaveSystem
{
    /// <summary>
    /// Thrown when the save file is formatted improperly. Could happen if trying to load
    ///     a save file which was saved in an old version of the game, or with an old
    ///     version of the save framework
    /// </summary>
    public class SaveFormatException : System.Exception
    {
        public SaveFormatException(string message) : base(message)
        {

        }
    }
}