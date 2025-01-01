using System;
using System.Collections.Generic;
using System.IO;
using Dman.SaveSystem.Converters;
using Dman.Utilities.Logger;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Dman.SaveSystem
{
    public interface IPersistText
    {
        public TextWriter WriteTo(string contextKey);
        public void OnWriteComplete(string contextKey);
        [CanBeNull] public TextReader ReadFrom(string contextKey);
        public void Delete(string contextKey);
    }
    
    public class SaveDataContextProvider : ISaveDataContextProvider, ISaveDataPersistence, IDisposable
    {
        private readonly IPersistText _persistence;
        private readonly string _rootFolderPath;
        private Dictionary<string, SaveDataContextHandle> saveContexts = new Dictionary<string, SaveDataContextHandle>();
        private readonly JsonSerializer _serializer;
        public bool IsDisposed { get; private set; } = false;

        public static SaveDataContextProvider CreateAndPersistTo(IPersistText persistence)
        {
            return new SaveDataContextProvider(persistence);
        } 
        
        private SaveDataContextProvider(IPersistText persistence)
        {
            _persistence = persistence;
            var serializerSettings = new JsonSerializerSettings
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ContractResolver = new UnitySerializationCompatibleContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy
                    {
                        OverrideSpecifiedNames = false
                    },
                    IgnoreSerializableAttribute = false,
                },
                ReferenceLoopHandling = ReferenceLoopHandling.Error,
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter(),
                    new Vector3IntConverter(),
                    new Vector2IntConverter(),
                    new UnityJsonUtilityJsonConverter(),
                },
                MissingMemberHandling = MissingMemberHandling.Error,
            };
            
            _serializer = JsonSerializer.CreateDefault(serializerSettings);
        }
        
        public ISaveDataContext GetContext(string contextKey)
        {
            if(IsDisposed) throw new ObjectDisposedException(nameof(SaveDataContextProvider));
            if(GetContextInternal(contextKey) is { } existingContext)
            {
                return existingContext;
            }
            
            // if context is not in memory, then try to load it from disk, or return default
            return TryLoadContextInternal(contextKey);
        }
        
        public void PersistContext(string contextKey)
        {
            if(IsDisposed) throw new ObjectDisposedException(nameof(SaveDataContextProvider));
            var context = GetContextInternal(contextKey);
            if (context == null) return;
            var contextData = context.WrappedHandle.SavedToken;
            using var writer = _persistence.WriteTo(contextKey);
            using var jsonWriter = new JsonTextWriter(writer);
            _serializer.Serialize(jsonWriter, contextData);
            _persistence.OnWriteComplete(contextKey);
        }
        
        public void LoadContext(string contextKey)
        {
            TryLoadContextInternal(contextKey);
        }
        
        private SaveDataContextHandle TryLoadContextInternal(string contextKey)
        {
            if(IsDisposed) throw new ObjectDisposedException(nameof(SaveDataContextProvider));
            var contextHandle = GetOrCreateContextInternal(contextKey);
            using var reader = _persistence.ReadFrom(contextKey);
            if (reader == null) return contextHandle;
            using var jsonReader = new JsonTextReader(reader);
            var data = JObject.Load(jsonReader);
            contextHandle.SwapInternalHandle(SaveDataContext.Loaded(data, _serializer));
            return contextHandle;
        }
        
        public void DeleteContext(string contextKey)
        {
            if(IsDisposed) throw new ObjectDisposedException(nameof(SaveDataContextProvider));
            _persistence.Delete(contextKey);
            var contextHandle = GetOrCreateContextInternal(contextKey);
            contextHandle.SwapInternalHandle(SaveDataContext.Empty(_serializer));
        }

        public IEnumerable<string> AllContexts()
        {
            if(IsDisposed) throw new ObjectDisposedException(nameof(SaveDataContextProvider));
            return saveContexts.Keys;
        }
        
        [CanBeNull]
        private SaveDataContextHandle GetContextInternal(string contextKey)
        {
            if (saveContexts.TryGetValue(contextKey, out SaveDataContextHandle context))
            {
                return context;
            }

            return null;
        }
        
        private SaveDataContextHandle CreateContextInternal(string contextKey)
        {
            var context = new SaveDataContextHandle(SaveDataContext.Empty(_serializer));
            saveContexts[contextKey] = context;
            return context;
        }

        private SaveDataContextHandle GetOrCreateContextInternal(string contextKey)
        {
            if (GetContextInternal(contextKey) is { } existingContext)
            {
                return existingContext;
            }
            return CreateContextInternal(contextKey);
        }
        
        public void Dispose()
        {
            if(IsDisposed) return;
            IsDisposed = true;
            foreach (SaveDataContextHandle context in saveContexts.Values)
            {
                context.Dispose();
            }
            saveContexts.Clear();
        }

        /// <summary>
        /// utility class used to keep the ISaveDataContext reference alive, while allowing new instances to be swapped in
        /// </summary>
        /// <remarks>
        /// Effectively a <![CDATA[ Rc<> ]]>, a ref-cell which delegates out all underlying behavior
        /// </remarks>
        private class SaveDataContextHandle : ISaveDataContext, IDisposable
        {
            public SaveDataContext WrappedHandle { get; private set; }
            public SaveDataContextHandle(SaveDataContext wrappedHandle) => WrappedHandle = wrappedHandle;
            public void SwapInternalHandle(SaveDataContext newHandle) => WrappedHandle = newHandle;

            public void Save<T>(string key, T value) => WrappedHandle.Save(key, value);
            public bool TryLoad(string key, out object value, Type type) => WrappedHandle.TryLoad(key, out value, type);
            public bool HasKey(string key) => WrappedHandle.HasKey(key);
            public bool DeleteKey(string key) => WrappedHandle.DeleteKey(key);

            public bool IsAlive => WrappedHandle.IsAlive;
            public void Dispose() => WrappedHandle.Dispose();
        }
        
        private class SaveDataContext : ISaveDataContext, IDisposable
        {
            private readonly JObject _data;
            public JToken SavedToken => _data;
            private readonly JsonSerializer _serializer;

            public bool IsAlive => !isDisposed;
            private bool isDisposed = false;
            
            private SaveDataContext(JsonSerializer serializer, JObject data = null)
            {
                _serializer = serializer;
                _data = data ?? new JObject();
            }

            public static SaveDataContext Empty(JsonSerializer serializer) => new SaveDataContext(serializer);
            public static SaveDataContext Loaded(JObject data, JsonSerializer serializer) => new SaveDataContext(serializer, data);
            
            public void Save<T>(string key, T value)
            {
                if(isDisposed) throw new ObjectDisposedException(nameof(SaveDataContext));
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
                if(isDisposed) throw new ObjectDisposedException(nameof(SaveDataContext));
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

            public bool HasKey(string key)
            {
                if(isDisposed) throw new ObjectDisposedException(nameof(SaveDataContext));
                return _data.ContainsKey(key);
            }
            
            public bool DeleteKey(string key)
            {
                return _data.Remove(key);
            }
            
            public void Dispose()
            {
                if (isDisposed) return;
                isDisposed = true;
            }
        }
    }
}