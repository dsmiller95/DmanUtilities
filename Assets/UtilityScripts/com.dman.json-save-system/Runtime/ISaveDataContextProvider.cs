using System;
using System.Collections.Generic;

namespace SaveSystem
{
    public interface ISaveDataBehavior : 
        ISaveDataContextProvider,
        IAmKeptAlive
    {
    
    }

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

    public interface ISaveDataPersistence
    {
        public void PersistContext(string contextKey);
        public void LoadContext(string contextKey);
        public void DeleteContext(string contextKey);
        public IEnumerable<string> AllContexts();
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