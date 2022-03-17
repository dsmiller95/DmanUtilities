using Dman.SceneSaveSystem.Objects;
using Dman.SceneSaveSystem.Objects.Identifiers;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Dman.SceneSaveSystem
{
    public static class SerializationManager
    {

        public static string saveFileSuffix = ".dat";

        internal static bool Save(SaveScopeData saveScopeData, string saveName)
        {
            return Save(saveScopeData.scopeIdentifier, saveName, saveScopeData);
        }
        internal static bool Save<T>(ISaveScopeIdentifier saveScope, string saveName, T saveData)
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
            Debug.Log("Saving file: " + path);

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

        public static void DeleteAll(string saveName)
        {
            var saveFolderPath = Path.Combine(Application.persistentDataPath, "saves");
            if (!Directory.Exists(saveFolderPath))
            {
                Directory.CreateDirectory(saveFolderPath);
            }
            var specificSaveDirectoryPath = Path.Combine(saveFolderPath, saveName);
            if (Directory.Exists(specificSaveDirectoryPath))
            {
                Directory.Delete(specificSaveDirectoryPath, true);
            }
        }
        public static void DeleteChunks(string saveName, params string[] saveFiles)
        {
            foreach (var file in saveFiles)
            {
                var savePath = GetSavePath(file, saveName);
                if (File.Exists(savePath))
                {
                    Debug.Log("deleting save file: " + savePath);
                    File.Delete(savePath);
                }
            }
        }
        internal static void DeleteChunks(string saveName, params ISaveScopeIdentifier[] saveScope)
        {
            DeleteChunks(saveName, saveScope.Select(x => x.UniqueSemiReadableName + saveFileSuffix).ToArray());
        }

        internal static T Load<T>(ISaveScopeIdentifier saveScope, string saveName) where T : class
        {
            return Load<T>(saveScope.UniqueSemiReadableName + saveFileSuffix, saveName);
        }

        public static T Load<T>(string saveFile, string saveName) where T : class
        {
            var path = SerializationManager.GetSavePath(saveFile, saveName);
            if (!File.Exists(path))
            {
                return null;
            }

            Debug.Log("Loading file: " + path);

            var formatter = SerializationManager.GetBinaryFormatter();
            FileStream file = File.Open(path, FileMode.Open);

            try
            {
                var resultObj = formatter.Deserialize(file);
                if (resultObj is T castedResult)
                {
                    return castedResult;
                }
                throw new System.Exception($"Saved file does not match type {typeof(T)}");
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                throw new SaveFormatException($"exception when loading an object of type {typeof(T).FullName} from file");
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
