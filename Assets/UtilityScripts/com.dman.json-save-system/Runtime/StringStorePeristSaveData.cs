using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dman.SaveSystem
{
    public class StringStorePersistSaveData : IPersistSaveData, IDisposable
    {
        private Dictionary<string, MemoryStream> _store = new Dictionary<string, MemoryStream>();

        public static StringStorePersistSaveData WithFiles(params (string name, string contents)[] files)
        {
            var res = new StringStorePersistSaveData();
            foreach (var (name, contents) in files)
            {
                using var writer = res.WriteTo(name);
                writer.Write(contents);
            }

            return res;
        }
        
        public TextWriter WriteTo(string contextKey)
        {
            if(!_store.TryGetValue(contextKey, out var memoryStream))
            {
                memoryStream = new MemoryStream();
                _store.Add(contextKey, memoryStream);
            }
            memoryStream.SetLength(0);
            return new StreamWriter(memoryStream, Encoding.UTF8, bufferSize: 1024, leaveOpen: true);
        }

        public TextReader ReadFrom(string contextKey)
        {
            if(!_store.TryGetValue(contextKey, out var memoryStream))
            {
                return null;
            }
            memoryStream.Position = 0;
            return new StreamReader(memoryStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);
        }

        public void Delete(string contextKey)
        {
            if (_store.TryGetValue(contextKey, out var removedStream))
            {
                _store.Remove(contextKey);
                removedStream.Dispose();
            }
        }

        public void Dispose()
        {
            foreach (var memoryStream in _store.Values)
            {
                memoryStream.Dispose();
            }
            _store.Clear();
        }
    }
}