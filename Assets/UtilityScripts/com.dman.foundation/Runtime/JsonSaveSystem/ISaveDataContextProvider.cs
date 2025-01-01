using System;
using System.Collections.Generic;
using Dman.Utilities.Logger;

namespace Dman.SaveSystem
{
    /// <summary>
    /// referenced as singleton from inside scripts which care about save data.
    /// </summary>
    public interface ISaveDataContextProvider
    {
        /// <summary>
        /// Get a handle into save data
        /// </summary>
        /// <remarks>
        /// This handle is long-lived. it is safe to grab one of these and use it
        /// for as long as the ISaveDataContextProvider is valid and alive
        /// </remarks>
        /// <param name="contextKey"></param>
        /// <returns></returns>
        public ISaveDataContext GetContext(string contextKey);
    }

    /// <summary>
    /// Manages the relationship between in-memory context data and the underlying persistence model.
    /// </summary>
    public interface ISaveDataPersistence
    {
        /// <summary>
        /// Save in memory data to overwrite the underlying persistence model.
        /// </summary>
        public void PersistContext(string contextKey);
        /// <summary>
        /// Load from underlying persistence model to overwrite in memory data.
        /// </summary>
        public void LoadContext(string contextKey);
        /// <summary>
        /// Deletes from both the underlying persistence model and in memory data.
        /// </summary>
        /// <param name="contextKey"></param>
        public void DeleteContext(string contextKey);
        /// <summary>
        /// All contexts that are currently present in memory
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> AllContexts();
    }

    public static class SaveDataPersistenceExtensions
    {
        public static void PersistAll(this ISaveDataPersistence persistence, bool logInfo = false)
        {
            if(logInfo) Log.Info($"PersistAll");
            foreach (var context in persistence.AllContexts())
            {
                if(logInfo) Log.Info($"persisting context {context}");
                persistence.PersistContext(context);
            }
        }
        
        public static void DeleteAll(this ISaveDataPersistence persistence, bool logInfo = false)
        {
            if(logInfo) Log.Info($"DeleteAll");
            foreach (var context in persistence.AllContexts())
            {
                if(logInfo) Log.Info($"deleting context {context}");
                persistence.DeleteContext(context);
            }
        }
    }

    /// <summary>
    /// should be safe to access in Awake. If you want to load a "save file", first set up the save data context,
    /// then instantiate a prefab which will load from it.
    /// </summary>
    /// <remarks>
    /// generally these operations are not optimized, so should be kept out of hot-paths, where possible. prefer local domain-specific caches
    /// <br/><br/>
    /// The top-level objects will not be polymorphic, meaning they must always be loaded with the same type that they were saved with.
    /// The objects inside the top-level types will be polymorphic, so their contents can be complex object type graphs.
    /// </remarks>
    public interface ISaveDataContext
    {
        public void Save<T>(string key, T value);
        public bool TryLoad(string key, out object value, Type type);
        public bool HasKey(string key);
        
        /// <summary>
        /// Delete the given key and its associated data
        /// </summary>
        /// <param name="key"></param>
        /// <returns>true if key existed, false if key was not present</returns>
        public bool DeleteKey(string key);
        /// <summary>
        /// returns true if this save data context is valid and can be used. Otherwise, false.
        /// </summary>
        /// <remarks>
        /// For example, if the ISaveDataContextProvider has been destroyed, this will return false.
        /// </remarks>
        public bool IsAlive { get; }
    }

    public static class SaveDataContextExtensions
    {
        public static bool TryLoad<T>(this ISaveDataContext saveDataContext, string key, out T value)
        {
            if (saveDataContext.TryLoad(key, out var obj, typeof(T)))
            {
                value = (T)obj;
                return true;
            }
            value = default;
            return false;
        }
    }

    public class SaveDataException : Exception
    {
        public SaveDataException(string message) : base(message) { }
        public SaveDataException(string message, Exception innerException) : base(message, innerException) { }
    }
}