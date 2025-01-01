﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dman.SaveSystem
{
    /// <summary>
    /// A persistence backend which just stores data in memory.
    /// </summary>
    public class StringStorePersistText : IPersistText, IDisposable
    {
        private readonly Dictionary<string, MemoryStream> _store = new Dictionary<string, MemoryStream>();

        public static StringStorePersistText WithFiles(params (string name, string contents)[] files)
        {
            var res = new StringStorePersistText();
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

        public void OnWriteComplete(string contextKey) { }

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

        public void DeleteAll()
        {
            foreach (var memoryStream in _store.Values)
            {
                memoryStream.Dispose();
            }
            _store.Clear();
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