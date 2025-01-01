using UnityEditor;
using UnityEngine;

namespace Dman.SaveSystem
{
    public class JsonSaveSystemSettings : ScriptableObject
    {
        public static JsonSaveSystemSettings Singleton => _singleton ??= GetSingleton();
        private static JsonSaveSystemSettings _singleton;
        
        public string SaveFolderName => saveFolderName;
        public string DefaultSaveFileName => defaultSaveFileName;
        
        [Header("All values are read on first use of save system. Changes during runtime are ignored.")]
        [SerializeField] private string saveFolderName = "SaveContexts";
        [SerializeField] private string defaultSaveFileName = "root";

        public static JsonSaveSystemSettings Create(string folder, string defaultFile)
        {
            var newSettings = CreateInstance<JsonSaveSystemSettings>();
            newSettings.saveFolderName = folder;
            newSettings.defaultSaveFileName = defaultFile;
            return newSettings;
        }
        
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
        
        public static void ForceOverrideSettingsObject(JsonSaveSystemSettings settings, bool suppressWarningDangerously = false)
        {
            if(!suppressWarningDangerously && _singleton != null)
            {
                Debug.LogWarning("Forcing override of JsonSaveSystemSettings object after it has already been used. " +
                                 "This is dangerous and should only be done during startup.");
            }
            _singleton = settings;
        }
        
        public static JsonSaveSystemSettings GetSingleton()
        {
            var settingsList = Resources.LoadAll<JsonSaveSystemSettings>("JsonSaveSystemSettings");
            if(settingsList.Length == 0)
            {
                var newSettings = ScriptableObject.CreateInstance<JsonSaveSystemSettings>();
                return newSettings;
            }
            if (settingsList.Length != 1)
            {
                Debug.LogWarning("The number of PlayFabSharedSettings objects should be 1: " + settingsList.Length);
            }
            return settingsList[0];
        }
    }
}