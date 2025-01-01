using System;
using UnityEditor;
using UnityEngine;

namespace Dman.SaveSystem
{
    public class JsonSaveSystemSingleton
    {
        public static string SaveFolderName => Settings.saveFolderName;
        public static string DefaultSaveFileName => Settings.defaultSaveFileName;
        
        private static JsonSaveSystemSettings Settings => _settings ??= GetSettingsObject();
        private static JsonSaveSystemSettings _settings; 

        private static JsonSaveSystemObjectSet SaveSystemObjectSet => _saveSystemObjectSet ??= JsonSaveSystemObjectSet.Create(Settings);
        private static JsonSaveSystemObjectSet _saveSystemObjectSet;

        private static KeepAliveContainer KeepAlive { get; } = new KeepAliveContainer(OnAllKeepAliveDisposed);

        public static ISaveDataPersistence GetPersistor() => SaveSystemObjectSet.SavePersistence;
        public static ISaveDataContextProvider GetContextProvider() => SaveSystemObjectSet.ContextProvider;
        
        /// <summary>
        /// The save system persists data on application exit. In order to prevent race conditions in the case
        /// where a component writes save data OnDestroy, take a keep-alive handle during Awake/Start/OnEnable.
        /// Then dispose the keepAlive handle after your save code runs in OnDestroy.  
        /// </summary>
        /// <returns></returns>
        public static IDisposable KeepAliveUntilDisposed() => KeepAlive.KeepAliveUntil();

        /// <summary>
        /// Will save the current state of the save system to disk, then recreate the save system object graph.
        /// </summary>
        /// <param name="settings"></param>
        public static void ForceOverrideSettingsObject(JsonSaveSystemSettings settings)
        {
            _settings = settings;
            if (_saveSystemObjectSet != null)
            {
                _saveSystemObjectSet.PersistAllAndDispose();
                _saveSystemObjectSet = JsonSaveSystemObjectSet.Create(settings);
            }
        }

        /// <summary>
        /// Make the save system behave as if it experienced a forced application quit, without any calls
        /// to OnApplicationQuit. This means all in-memory save data will be lost.
        /// </summary>
        internal static void EmulateForcedQuit()
        {
            _saveSystemObjectSet.DisposeWithoutPersisting();
            _saveSystemObjectSet = null;
        }
        
        /// <summary>
        /// Make the save system behave as if it experienced a managed application quit. This will save all data to disk.
        /// and will clear out the in memory provider.
        /// </summary>
        internal static void EmulateManagedApplicationQuit()
        {
            _saveSystemObjectSet.PersistAllAndDispose();
            _saveSystemObjectSet = null;
        }
        
        private static JsonSaveSystemSettings GetSettingsObject()
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

        [MenuItem("SaveSystem/Create Json Save System Settings")]
        private static void CreateSettingsObject()
        {
            var newSettings = ScriptableObject.CreateInstance<JsonSaveSystemSettings>();
            AssetDatabase.CreateFolder("Assets", "Resources");
            AssetDatabase.CreateAsset(newSettings, "Assets/Resources/JsonSaveSystemSettings.asset");
            AssetDatabase.SaveAssets();
        }
        
        [RuntimeInitializeOnLoadMethod]
        private static void RunOnStart()
        {
            Application.quitting += OnApplicationQuit;
        }

        private static void OnApplicationQuit()
        {
            KeepAlive.SetReadyToDispose();
        }
        
        private static void OnAllKeepAliveDisposed()
        {
            _saveSystemObjectSet?.PersistAllAndDispose();
        }
    }
    
    public class JsonSaveSystemObjectSet
    {
        public ISaveDataPersistence SavePersistence => _saveContextProvider;
        public ISaveDataContextProvider ContextProvider => _saveContextProvider;

        private readonly SaveDataContextProvider _saveContextProvider;
        private IPersistText _persistence;
        
        private JsonSaveSystemObjectSet(SaveDataContextProvider saveContextProvider, IPersistText persistence)
        {
            _saveContextProvider = saveContextProvider;
            _persistence = persistence;
        }

        public static JsonSaveSystemObjectSet Create(JsonSaveSystemSettings forSettings)
        {
            var persistence = new FileSystemPersistence(forSettings.saveFolderName);
            var saveContextProvider = SaveDataContextProvider.CreateAndPersistTo(persistence);
            return new JsonSaveSystemObjectSet(saveContextProvider, persistence);
        }
        
        public void PersistAllAndDispose()
        {
            SavePersistence.PersistAll(logInfo: true);
            _saveContextProvider.Dispose();
        }
        
        internal void DisposeWithoutPersisting()
        {
            _saveContextProvider.Dispose();
        }
    }
}