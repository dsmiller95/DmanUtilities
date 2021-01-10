﻿using UnityEngine;

namespace Dman.ObjectSets
{
    /// <summary>
    /// Use this to set up a reference to every type of unique object registry in the scene
    ///     This helps with automatic serialization and deserialization: to save an object
    ///     which is registered in a unique object registry, just use the RegistryRegistry
    ///     to get access to that registry. This can be more convenient than passing in a
    ///     registry to every behavior which needs access to those registered object. Of
    ///     course since this is a singleton all the typical drawbacks of singletons come
    ///     with this. Don't use for a registry type that you want to have more than one 
    ///     of at some point in your project.
    /// </summary>
    public class RegistryRegistry : MonoBehaviour
    {
        public UniqueObjectRegistry[] registries;

        public static RegistryRegistry Instance;

        private void Awake()
        {
            Instance = this;
        }
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public static UniqueObjectRegistryWithAccess<T> GetObjectRegistry<T>() where T : IDableObject
        {
            if (Instance == null)
            {
                throw new System.Exception("the registry registry is not initialized!!");
            }
            foreach (var registry in Instance.registries)
            {
                if (registry is UniqueObjectRegistryWithAccess<T> typedRegistry)
                {
                    return typedRegistry;
                }
            }
            return null;
        }
    }
}