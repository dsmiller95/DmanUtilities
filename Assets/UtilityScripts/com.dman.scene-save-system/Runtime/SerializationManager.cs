using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Dman.SceneSaveSystem
{
    public static class SerializationManager
    {
        public static bool Save(string saveFile, string saveName, object saveData)
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

        public static object Load(string saveFile, string saveName)
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
                return formatter.Deserialize(file);
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
