using System;
using Dman.Utilities;

namespace Dman.SaveSystem
{
    public static class SimpleSave
    {
        public static string SaveFileName { get; private set; }= "root";

        private static ISaveDataContextProvider SaveFileProvider => SingletonLocator<ISaveDataContextProvider>.Instance;
        private static ISaveDataPersistence SavePersistence => SingletonLocator<ISaveDataPersistence>.Instance;
        private static ISaveDataContext SaveFile => SaveFileProvider.GetContext(SaveFileName);

        public static void ChangeDefaultSaveFile(string newSaveFileName)
        {
            if (newSaveFileName == SaveFileName) return;
            
            // save the old file before switching
            Save();
            SaveFileName = newSaveFileName;
        }
        
        public static string GetString(string key, string defaultValue = null) => Get(key, defaultValue);
        public static void SetString(string key, string value) => Set(key, value);

        public static int GetInt(string key, int defaultValue = 0) => Get(key, defaultValue);
        public static void SetInt(string key, int value) => Set(key, value);
        
        public static float GetFloat(string key, float defaultValue = 0) => Get(key, defaultValue);
        public static void SetFloat(string key, float value) => Set(key, value);
        
        public static T Get<T>(string key, T defaultValue = default)
        {
            if(!SaveFile.TryLoad(key, out T value)) return defaultValue;

            return value;
        }
        
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

        public static void Save()
        {
            SavePersistence.PersistContext(SaveFileName);
        }
    }
}