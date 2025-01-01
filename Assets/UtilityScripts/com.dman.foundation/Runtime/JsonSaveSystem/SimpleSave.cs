using System;
using Dman.Utilities;

namespace Dman.SaveSystem
{
    public static class SimpleSave
    {
        /// <summary>
        /// The save file name used by SimpleSave. Can be different from the default.
        /// </summary>
        public static string SaveFileName
        {
            get => _cachedSaveFileName ??= JsonSaveSystemSingleton.DefaultSaveFileName;
            private set => _cachedSaveFileName = value;
        }
        private static string _cachedSaveFileName;

        private static ISaveDataContextProvider SaveFileProvider => JsonSaveSystemSingleton.GetContextProvider();
        private static ISaveDataPersistence SavePersistence => JsonSaveSystemSingleton.GetPersistor();
        private static ISaveDataContext SaveFile => SaveFileProvider.GetContext(SaveFileName);

        /// <summary>
        /// Save the current file to disk.
        /// </summary>
        public static void Save()
        {
            SavePersistence.PersistContext(SaveFileName);
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
            SavePersistence.LoadContext(SaveFileName);
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
        }
        
        /// <summary>
        /// Same as ChangeSaveFile, but sets to the default save file name.
        /// </summary>
        public static void ChangeSaveFileToDefault() => ChangeSaveFile(JsonSaveSystemSingleton.DefaultSaveFileName);
        
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
            if(!SaveFile.TryLoad(key, out T value)) return defaultValue;

            return value;
        }
        /// <summary>
        /// Set generic data. Supports JsonUtility style serializable types.
        /// </summary>
        public static void Set<T>(string key, T value)
        {
            SaveFile.Save(key, value);
        }

        public static bool HasKey(string key)
        {
            return SaveFile.HasKey(key);
        }

        public static void DeleteKey(string key)
        {
            SaveFile.DeleteKey(key);
        }

        public static void DeleteAll()
        {
            SavePersistence.DeleteContext(SaveFileName);
        }
    }
}
