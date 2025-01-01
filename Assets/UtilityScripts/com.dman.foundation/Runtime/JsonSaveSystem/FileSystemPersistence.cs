using System.IO;
using Dman.Utilities.Logger;
using UnityEngine;

namespace Dman.SaveSystem
{
    public class FileSystemPersistence : IPersistText
    {
        private readonly string _rootFolderPath;
        private readonly string _directoryPath;

        public FileSystemPersistence(string rootFolderPath)
        {
            _rootFolderPath = rootFolderPath;
            _directoryPath = Path.Combine(Application.persistentDataPath, _rootFolderPath);
            
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

        // TODO: this is dangerous. what if the directory has other stuff.. like our game executables?
        public void DeleteAll()
        {
            if (Directory.Exists(_directoryPath))
            {
                Directory.Delete(_directoryPath, recursive: true);
            }
        }

        private string EnsureSaveFilePath(string contextKey)
        {
            var fileName = $"{contextKey}.json";
            if (!Directory.Exists(_directoryPath))
            {
                Directory.CreateDirectory(_directoryPath);
            }
            
            var saveFile = Path.Combine(_directoryPath, fileName);
            return saveFile;
        }
    }
}