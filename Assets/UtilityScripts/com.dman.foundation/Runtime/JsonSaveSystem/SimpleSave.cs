using System.Collections.Generic;
using Dman.SaveSystem.Converters;
using Dman.Utilities.Logger;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Dman.SaveSystem
{
    /// <summary>
    /// simple static API into the save system, for quick and easy usage in simple use cases.
    /// </summary>
    public static class SimpleSave
    {
        /// <summary>
        /// The save file name used by SimpleSave. Can be changed from the default.
        /// </summary>
        public static string SaveFileName
        {
            get => _cachedSaveFileName ??= Settings.DefaultSaveFileName;
            private set => _cachedSaveFileName = value;
        }
        private static string _cachedSaveFileName;

        public static string SaveFolderName => Settings.SaveFolderName;
        
        [NotNull]
        private static SimpleSaveFile CurrentSaveData
        {
            get
            {
                if (_currentSaveData == null)
                {
                    _currentSaveData = LoadFrom(SaveFileName);
                    // if Load did not load any data, then create empty data
                    _currentSaveData ??= SimpleSaveFile.Empty(DefaultSerializer);
                }
                return _currentSaveData;
            }
            set => _currentSaveData = value;
        }
        private static SimpleSaveFile _currentSaveData;

        private static JsonSerializer DefaultSerializer => _defaultSerializer ??= JsonSerializer.CreateDefault(GetSerializerSettings());
        private static JsonSerializer _defaultSerializer;

        public static IPersistText TextPersistence => _textPersistence ??= new FileSystemPersistence(SaveFolderName);
        private static IPersistText _textPersistence;
        
        private static JsonSaveSystemSettings Settings => _settings ??= JsonSaveSystemSettings.GetSingleton();
        private static JsonSaveSystemSettings _settings; 

        /// <summary>
        /// Save the current file to disk.
        /// </summary>
        public static void Save()
        {
            PersistFile(CurrentSaveData, SaveFileName);
        }
        
        /// <summary>
        /// Load the current file from disk, overwriting any unsaved changes in memory.
        /// </summary>
        /// <remarks>
        /// Loading happens automatically on first access. ForceLoad is required when loading changes made
        /// to the file outside the SaveSystem apis during runtime. For example, edits in a text editor
        /// or modifications made by other applications.
        /// </remarks>
        public static void Refresh()
        {
            var loadedData = LoadFrom(SaveFileName);
            if(loadedData != null)
            {
                CurrentSaveData = loadedData;
            }
        }
        
        /// <summary>
        /// Change the save file currently written to. This will save the current file before switching, if different. 
        /// </summary>
        public static void ChangeSaveFile(string newSaveFileName)
        {
            if (newSaveFileName == SaveFileName) return;
            
            // save the old file before switching
            Save();
            SaveFileName = newSaveFileName;
            // load the new save file after switching
            Refresh();
        }
        
        /// <summary>
        /// Same as ChangeSaveFile, but sets to the default save file name.
        /// </summary>
        public static void ChangeSaveFileToDefault() => ChangeSaveFile(Settings.DefaultSaveFileName);
        
        public static string GetString(string key, string defaultValue = "") => Get(key, defaultValue);
        public static void SetString(string key, string value) => Set(key, value);

        public static int GetInt(string key, int defaultValue = 0) => Get(key, defaultValue);
        public static void SetInt(string key, int value) => Set(key, value);
        
        public static float GetFloat(string key, float defaultValue = 0) => Get(key, defaultValue);
        public static void SetFloat(string key, float value) => Set(key, value);
        
        /// <summary>
        /// Get generic data. Supports JsonUtility style serializable types.
        /// </summary>
        /// <returns>
        /// Data from the shared store, or <paramref name="defaultValue"/> if the data at <paramref name="key"/>
        /// is not present or not deserializable into <typeparamref name="T"/>
        /// </returns>
        public static T Get<T>(string key, T defaultValue = default)
        {
            if(!CurrentSaveData.TryLoad(key, out T value)) return defaultValue;

            return value;
        }
        /// <summary>
        /// Set generic data. Supports JsonUtility style serializable types.
        /// </summary>
        public static void Set<T>(string key, T value)
        {
            CurrentSaveData.Save(key, value);
        }

        public static bool HasKey(string key)
        {
            return CurrentSaveData.HasKey(key);
        }

        public static void DeleteKey(string key)
        {
            CurrentSaveData.DeleteKey(key);
        }

        public static void DeleteAll()
        {
            CurrentSaveData = SimpleSaveFile.Empty(DefaultSerializer);
            string file = SaveFileName;
            TextPersistence.Delete(file);
        }

        internal static void EmulateForcedQuit()
        {
            _currentSaveData = null;
        }

        internal static void EmulateManagedApplicationQuit()
        {
            Save();
            _currentSaveData = null;
        }
        
        
        [RuntimeInitializeOnLoadMethod]
        private static void RunOnStart()
        {
            Application.quitting += OnApplicationQuit;
        }

        private static void OnApplicationQuit()
        {
            Save();
        }

        [CanBeNull]
        private static SimpleSaveFile LoadFrom(string file)
        {
            using var reader = TextPersistence.ReadFrom(file);
            if (reader == null) return null;
            using var jsonReader = new JsonTextReader(reader);
            
            try
            {
                var data = JObject.Load(jsonReader);
                return SimpleSaveFile.Loaded(data, DefaultSerializer);
            }
            catch (JsonException e)
            {
                using var reader2 = TextPersistence.ReadFrom(file);
                Log.Error($"Failed to load data for {SaveFolderName}/{file}.json, malformed Json. Raw json: {reader2?.ReadToEnd()}");
                Debug.LogException(e);
                return null;
            }
        }
        
        private static void PersistFile(SimpleSaveFile data, string file)
        {
            using var writer = TextPersistence.WriteTo(file);
            using var jsonWriter = new JsonTextWriter(writer);
            DefaultSerializer.Serialize(jsonWriter, data.SavedToken);
            TextPersistence.OnWriteComplete(file);
        }
        
        private static JsonSerializerSettings GetSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ContractResolver = new UnitySerializationCompatibleContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy
                    {
                        OverrideSpecifiedNames = false
                    },
                    IgnoreSerializableAttribute = false,
                },
                ReferenceLoopHandling = ReferenceLoopHandling.Error,
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter(),
                    new Vector3IntConverter(),
                    new Vector2IntConverter(),
                    new UnityJsonUtilityJsonConverter(),
                },
                MissingMemberHandling = MissingMemberHandling.Error,
            };
        }
    }
}
