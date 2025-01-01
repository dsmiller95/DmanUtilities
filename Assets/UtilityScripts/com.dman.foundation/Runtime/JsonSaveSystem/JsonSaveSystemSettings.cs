using UnityEngine;

namespace Dman.SaveSystem
{
    [CreateAssetMenu(fileName = "JsonSaveSystemSettings", menuName = "Utility/JsonSaveSystemSettings")]
    public class JsonSaveSystemSettings : ScriptableObject
    {
        [Header("All values are read on first use of save system. Changes during runtime are ignored.")]
        public string saveFolderName = "Saves";
        public string defaultSaveFileName = "root";
    }
}