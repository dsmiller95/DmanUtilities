using UnityEditor;
using UnityEngine;

namespace Dman.SaveSystem
{
    public class JsonSaveSystemSettings : ScriptableObject
    {
        [Header("All values are read on first use of save system. Changes during runtime are ignored.")]
        public string saveFolderName = "SaveContexts";
        public string defaultSaveFileName = "root";
        
        #if UNITY_EDITOR
        [MenuItem("SaveSystem/Create Json Save System Settings")]
        private static void CreateSettingsObject()
        {
            var newSettings = CreateInstance<JsonSaveSystemSettings>();
            if(!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            AssetDatabase.CreateAsset(newSettings, "Assets/Resources/JsonSaveSystemSettings.asset");
            AssetDatabase.SaveAssets();
        }
        #endif
    }
}