using System;
using UnityEngine;

namespace Dman.SaveSystem
{
    public class JsonSaveSystemSingleton
    {
        public static string SaveFolderName => Settings.saveFolderName;
        
        private static JsonSaveSystemSettings Settings => _settings ??= GetSettingsObject();
        private static JsonSaveSystemSettings _settings; 

        private static JsonSaveSystemObjectSet SaveSystemObjectSet => _saveSystemObjectSet ??= JsonSaveSystemObjectSet.Create(Settings);
        private static JsonSaveSystemObjectSet _saveSystemObjectSet;
        
        public static ISaveDataPersistence GetPersistor() => SaveSystemObjectSet.SavePersistence;
        public static ISaveDataContextProvider GetContextProvider() => SaveSystemObjectSet.ContextProvider;
        /// <summary>
        /// The save system persists data on application exit. In order to prevent race conditions in the case
        /// where a component writes save data OnDestroy, take a keep-alive handle during Awake/Start/OnEnable.
        /// Then dispose the keepAlive handle after your save code runs in OnDestroy.  
        /// </summary>
        /// <returns></returns>
        public static IDisposable KeepAliveUntilDisposed() => SaveSystemObjectSet.KeepAlive.KeepAliveUntil();
        
        private static JsonSaveSystemSettings GetSettingsObject()
        {
            var settingsList = Resources.LoadAll<JsonSaveSystemSettings>("JsonSaveSystemSettings");
            if (settingsList.Length != 1)
            {
                Debug.LogWarning("The number of PlayFabSharedSettings objects should be 1: " + settingsList.Length);
            }
            return settingsList[0];
        }

        [RuntimeInitializeOnLoadMethod]
        static void RunOnStart()
        {
            Application.quitting += OnApplicationQuit;
        }

        private static void OnApplicationQuit()
        {
            SaveSystemObjectSet.Dispose();
        }
    }
    
    public class JsonSaveSystemObjectSet : IDisposable
    {
        public ISaveDataPersistence SavePersistence => _saveContextProvider;

        public ISaveDataContextProvider ContextProvider => _saveContextProvider;

        public IAmKeptAlive KeepAlive { get {
            ThrowIfDisposed();
            return _keepAliveContainer;
        } }

        private readonly SaveDataContextProvider _saveContextProvider;
        private readonly KeepAliveContainer _keepAliveContainer;
        private readonly IDisposable _keepAliveInternalHandle;
        private IPersistText _persistence;
        private bool _isDisposed = false;
        
        private JsonSaveSystemObjectSet(SaveDataContextProvider saveContextProvider, IPersistText persistence)
        {
            _saveContextProvider = saveContextProvider;
            _persistence = persistence;
            _keepAliveContainer = new KeepAliveContainer(OnKeepAliveDisposed);
            _keepAliveInternalHandle = _keepAliveContainer.KeepAliveUntil();
        }

        public static JsonSaveSystemObjectSet Create(JsonSaveSystemSettings forSettings)
        {
            var persistence = new FilesystemPersistence(forSettings.saveFolderName);
            var saveContextProvider = SaveDataContextProvider.CreateAndPersistTo(persistence);
            return new JsonSaveSystemObjectSet(saveContextProvider, persistence);
        }
        
        private void ThrowIfDisposed()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(JsonSaveSystemObjectSet));
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            _keepAliveContainer.SetReadyToDispose();
            _keepAliveInternalHandle?.Dispose();
        }
        
        /// <summary>
        /// called once all keep-alive handles are disposed, and this object set is also disposed.
        /// </summary>
        private void OnKeepAliveDisposed()
        {
            SavePersistence.PersistAll(logInfo: true);
            _saveContextProvider.Dispose();
        }
    }
    
}