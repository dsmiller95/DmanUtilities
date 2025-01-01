using System.IO;
using Dman.Utilities.Logger;
using UnityEngine;

namespace Dman.SaveSystem
{
    public class FilesystemPersistence : IPersistText
    {
        private readonly string _rootFolderPath;

        public FilesystemPersistence(string rootFolderPath)
        {
            _rootFolderPath = rootFolderPath;
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
        
        private string EnsureSaveFilePath(string contextKey)
        {
            var fileName = $"{contextKey}.json";
            var directoryPath = Path.Combine(Application.persistentDataPath, _rootFolderPath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            
            var saveFile = Path.Combine(directoryPath, fileName);
            return saveFile;
        }
    }
}