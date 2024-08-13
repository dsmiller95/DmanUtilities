using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dman.Utilities
{
    /// <summary>
    /// must only be applied to objects which derive from UnityEngine.Object
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class UnitySingletonAttribute : Attribute
    {
    
    }

    /// <summary>
    /// Queries for a singleton of or implementing a given type.
    /// Will search for any type which derives from <see cref="UnityEngine.Object"/>, has the <see cref="UnitySingletonAttribute"/> attribute,
    /// and implements the generic <typeparamref name="TSingletonType"/>. If 0 types or multiple types are found, will throw an exception.
    /// </summary>
    /// <remarks>
    /// Will cache a given singleton instance until it is destroyed.
    /// </remarks>
    /// <example>
    /// Example singleton declaration:
    /// <code>
    /// public interface ISwitchLevels { public void RestartLevel(); }
    /// [UnitySingleton]
    /// public class LevelSwitcher : MonoBehaviour, ISwitchLevels { }
    /// </code>
    /// Example singleton query and usage in user code:
    /// <code>
    /// <![CDATA[
    /// ISwitchLevels levelSwitcher =  SingletonLocator<ISwitchLevels>.Instance;
    /// levelSwitcher.RestartLevel();
    /// ]]>
    /// </code>
    /// </example>
    /// <typeparam name="TSingletonType">The type of the singleton to locate</typeparam>
    public static class SingletonLocator<TSingletonType> where TSingletonType: class
    {
        private static TSingletonType _instance;
        // ReSharper disable once StaticMemberInGenericType - This is a type directly related to the generic type
        private static readonly Type TargetSingletonType;
    
        public static TSingletonType Instance
        {
            get
            {
                // explicit check for unityEngine object destroy state
                if (_instance != null && _instance as Object == null) _instance = null;
                if (_instance == null)
                {
                    _instance = UnityEngine.Object.FindObjectOfType(TargetSingletonType) as TSingletonType;
                }

                return _instance;
            }
        }
    
        static SingletonLocator()
        {
            var validTypes = SingletonCollector.AllSingletonTypes
                .Where(x => typeof(TSingletonType).IsAssignableFrom(x))
                .ToList();
            var count = validTypes.Count;
            if (count > 1)
            {
                throw new Exception($"Multiple Singleton types found for {typeof(TSingletonType).Name}: {string.Join(", ", validTypes.Select(x => x.Name))}");
            }
            if (count <= 0)
            {
                throw new Exception($"No Singleton types found for {typeof(TSingletonType).Name}");
            }
            TargetSingletonType = validTypes.Single();
        }
    }

    internal static class SingletonCollector
    {
        private static List<Type> _allSingletonTypes;

        public static IReadOnlyList<Type> AllSingletonTypes => _allSingletonTypes ??= CollectAllSingletonTypes();

        private static List<Type> CollectAllSingletonTypes()
        {
            var allTypesWithAttribute = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(type => type.GetCustomAttributes(typeof(UnitySingletonAttribute), false).Length > 0)
                .ToList();

            var didError = false;
            foreach (Type singleton in allTypesWithAttribute)
            {
                if (typeof(UnityEngine.Object).IsAssignableFrom(singleton)) continue;
            
                didError = true;
                Debug.LogError($"Singleton type {singleton.Name} does not inherit from UnityEngine.Object");
            }

            if (didError)
            {
                throw new Exception("One or more Singleton types did not inherit from UnityEngine.Object. critical error.");
            }
        
            return allTypesWithAttribute;
        }
    
        static SingletonCollector()
        {
            _allSingletonTypes = CollectAllSingletonTypes();
        }
    }
}