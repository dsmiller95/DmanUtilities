using System;
using System.Collections.Generic;
using System.IO;
using Dman.Utilities;
using Dman.Utilities.Logger;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dman.SaveSystem
{
    [UnitySingleton]
    [RequireComponent(typeof(EarlyAwakeHook))]
    public class SaveDataContextProviderBehavior : MonoBehaviour, 
        ISaveDataBehavior,
        ISaveDataPersistence,
        IPersistText,
        IAwakeEarly
    {
        [Tooltip("The root folder path is appended to the persistent data path to create the save file path")]
        [SerializeField] private string rootFolderPath = "SaveContexts";

        /// <summary>
        /// used to skip all lifecycle events if we spawned in while another instance was already alive
        /// </summary>
        private bool _isDestroyingDueToDuplicateSingleton = false;
        private SaveDataContextProvider _provider;
        private KeepAliveContainer _keepAliveContainer;
        
        public void AwakeEarly()
        {
            var existing = SingletonLocator<SaveDataContextProviderBehavior>.Instance;
            if (existing != null && existing != this)
            {
                Log.Warning("Duplicate SaveDataContextProviderBehavior detected, destroying the newly instantiated", this);
                _isDestroyingDueToDuplicateSingleton = true;
                gameObject.SetActive(false);
                Destroy(gameObject);
                return;
            }
            
            if (Application.isPlaying)
            {
                this.transform.SetParent(null);
                Object.DontDestroyOnLoad(gameObject);
            }
            
            _provider = SaveDataContextProvider.CreateAndPersistTo(this);
            _keepAliveContainer = new KeepAliveContainer(OnAllHandlesDestroyed);
        }
        private void Awake()
        {
            if (_isDestroyingDueToDuplicateSingleton) return;
            Debug.Assert(_provider != null, "SaveDataContextProviderBehavior.Awake: _provider != null");
        }
        private void OnDestroy()
        {
            if (_isDestroyingDueToDuplicateSingleton) return;
            Debug.Assert(_provider != null, "SaveDataContextProviderBehavior.OnDestroy: _provider != null");
            Debug.Assert(_provider.IsDisposed == false, "SaveDataContextProviderBehavior.OnDestroy: _provider.IsDisposed == false");
            
            _keepAliveContainer.SetReadyToDispose();
        }
        private void OnAllHandlesDestroyed()
        {
            if (_isDestroyingDueToDuplicateSingleton) return;
            _provider.PersistAll(logInfo: true);
            _provider.Dispose();
            _provider = null;
        }

        private string EnsureSaveFilePath(string contextKey)
        {
            var fileName = $"{contextKey}.json";
            var directoryPath = Path.Combine(Application.persistentDataPath, rootFolderPath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            
            var saveFile = Path.Combine(directoryPath, fileName);
            return saveFile;
        }
        
        public TextWriter WriteTo(string contextKey)
        {
            string filePath = EnsureSaveFilePath(contextKey);
            Log.Info($"Saving to {filePath}");
            return new StreamWriter(filePath, append: false);
        }

        public void OnWriteComplete(string contextKey)
        {
            FileSystemJslibAdapter.EnsureSynced();
        }

        public TextReader ReadFrom(string contextKey)
        {
            var filePath = EnsureSaveFilePath(contextKey);
            if (!File.Exists(filePath))
            {
                Log.Warning($"No file found at {filePath}");
                return null;
            }
            Log.Info($"Reading from {filePath}");
            return new StreamReader(filePath);
        }

        public void Delete(string contextKey)
        {
            var filePath = EnsureSaveFilePath(contextKey);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        
        public ISaveDataContext GetContext(string contextKey)
        {
            return _provider.GetContext(contextKey);
        }

        public void PersistContext(string contextKey) => _provider.PersistContext(contextKey);
        public void LoadContext(string contextKey) => _provider.LoadContext(contextKey);
        public void DeleteContext(string contextKey) => _provider.DeleteContext(contextKey);
        public IEnumerable<string> AllContexts() => _provider.AllContexts();
        public IKeepAliveHandle KeepAliveUntil() => _keepAliveContainer.KeepAliveUntil();
    }
}