using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Dman.SceneSaveSystem
{
    public static class SerializationManager
    {

        public static string saveFileSuffix = ".dat";
        public static bool Save<T>(ISaveScopeIdentifier saveScope, string saveName, T saveData)
        {
            return Save(saveScope.UniqueSemiReadableName + saveFileSuffix, saveName, saveData);
        }
        public static bool Save<T>(string saveFile, string saveName, T saveData)
        {
            var formatter = SerializationManager.GetBinaryFormatter();

            var saveFolderPath = Path.Combine(Application.persistentDataPath, "saves");
            if (!Directory.Exists(saveFolderPath))
            {
                Directory.CreateDirectory(saveFolderPath);
            }
            var specificSaveDirectoryPath = Path.Combine(saveFolderPath, saveName);
            if (!Directory.Exists(specificSaveDirectoryPath))
            {
                Directory.CreateDirectory(specificSaveDirectoryPath);
            }

            string path = SerializationManager.GetSavePath(saveFile, saveName);

            FileStream file = File.Create(path);
            try
            {
                formatter.Serialize(file, saveData);
            }
            catch
            {
                Debug.LogError($"Failed to save file to {path}");
                throw;
            }
            finally
            {
                file.Close();
            }
            return true;
        }

        public static string GetSavePath(string saveFile, string saveName)
        {
            return Path.Combine(Application.persistentDataPath, "saves", saveName, saveFile);
        }

        public static void DeleteAll(string saveName, params string[] saveFiles)
        {
            foreach (var file in saveFiles)
            {
                var savePath = GetSavePath(file, saveName);
                if (File.Exists(savePath))
                    File.Delete(savePath);
            }
        }

        public static T Load<T>(ISaveScopeIdentifier saveScope, string saveName) where T : class
        {
            return Load<T>(saveScope.UniqueSemiReadableName + saveFileSuffix, saveName);
        }

        public static T Load<T>(string saveFile, string saveName) where T: class
        {
            var path = SerializationManager.GetSavePath(saveFile, saveName);
            if (!File.Exists(path))
            {
                return null;
            }

            var formatter = SerializationManager.GetBinaryFormatter();
            FileStream file = File.Open(path, FileMode.Open);

            try
            {
                var resultObj = formatter.Deserialize(file);
                if(resultObj is T castedResult)
                {
                    return castedResult;
                }
                throw new System.Exception($"Saved file does not match type {typeof(T)}");
            }
            catch
            {
                Debug.LogError($"Failed to load file at {path}");
                throw;
            }
            finally
            {
                file.Close();
            }
        }

        private static BinaryFormatter GetBinaryFormatter()
        {
            return new BinaryFormatter();
        }
    }
}
