using System;
using Dman.Utilities.Logger;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dman.SaveSystem
{
    public class SimpleSaveFile
    {
        private readonly JObject _data;
        public JToken SavedToken => _data;
        private readonly JsonSerializer _serializer;
        private bool _isDisposed = false;
            
        private SimpleSaveFile(JsonSerializer serializer, JObject data = null)
        {
            _serializer = serializer;
            _data = data ?? new JObject();
        }

        public static SimpleSaveFile Empty(JsonSerializer serializer) => new SimpleSaveFile(serializer);
        public static SimpleSaveFile Loaded(JObject data, JsonSerializer serializer) => new SimpleSaveFile(serializer, data);
            
        public void Save<T>(string key, T value)
        {
            if(_isDisposed) throw new ObjectDisposedException(nameof(SimpleSaveFile));
            try
            {
                _data[key] = JToken.FromObject(value, _serializer);
            }
            catch (JsonSerializationException e)
            {
                throw new SaveDataException($"Failed to save data for key {key} of type {typeof(T)}", e);
            }
            catch (InvalidOperationException e)
            {
                throw new SaveDataException($"Failed to save data for key {key} of type {typeof(T)}", e);
            }
        }

        public bool TryLoad(string key, out object value, Type objectType)
        {
            if(_isDisposed) throw new ObjectDisposedException(nameof(SimpleSaveFile));
            if (!_data.TryGetValue(key, out JToken existing))
            {
                value = default;
                return false;
            }

            try
            {
                value = existing.ToObject(objectType, _serializer);
            }
            catch (JsonException)
            {
                Log.Error($"Failed to load data of type {objectType} for key {key}. Raw json: {existing}");
                value = default;
                return false;
            }
            return true;
        }
        
        public bool TryLoad<T>(string key, out T value)
        {
            if (TryLoad(key, out var obj, typeof(T)))
            {
                value = (T)obj;
                return true;
            }
            value = default;
            return false;
        }

        public bool HasKey(string key)
        {
            if(_isDisposed) throw new ObjectDisposedException(nameof(SimpleSaveFile));
            return _data.ContainsKey(key);
        }
            
        public bool DeleteKey(string key)
        {
            return _data.Remove(key);
        }
            
        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
        }
    }
}