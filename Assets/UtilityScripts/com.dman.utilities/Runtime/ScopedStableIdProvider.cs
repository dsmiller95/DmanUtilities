using Dman.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dman.NarrativeSystem
{
    public class ScopedStableIdProvider<T>
    {
        private static ScopedStableIdProvider<T> _instance;
        private static ScopedStableIdProvider<T> Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ScopedStableIdProvider<T>();
                }
                return _instance;
            }
        }


        private Dictionary<string, T> registeredObjects = new Dictionary<string, T>();
        private Stack<string> scopeStack = new Stack<string>();

        private ScopedStableIdProvider() { }

        /// <summary>
        /// Register <paramref name="registered"/> object with a given local id, with the stable scoped id
        /// </summary>
        /// <param name="localId">the local ID to register against</param>
        /// <param name="registered">the registered object</param>
        /// <returns>the new stable ID which was registered against</returns>
        public static string RegeisterOnStableId(string localId, T registered)
        {
            return Instance.RegeisterOnStableIdInternal(localId, registered);
        }

        private string RegeisterOnStableIdInternal(string localIdentifier, T registered)
        {
            var baseScope = string.Join(".", scopeStack);
            var newId = baseScope + "." + localIdentifier;
            Debug.Log($"adding a {typeof(T).Name} with id: {newId}");
            if (registeredObjects.ContainsKey(newId))
            {
                Debug.LogError($"adding a {typeof(T).Name} with an id which has already been used: {newId}");
            }
            registeredObjects[newId] = registered;
            return newId;
        }

        public static IEnumerable<T> GetAll()
        {
            return Instance.GetAllInternal();
        }
        private IEnumerable<T> GetAllInternal()
        {
            return registeredObjects.Values;
        }
        public static T Get(string stableId)
        {
            return Instance.GetInternal(stableId);
        }
        private T GetInternal(string stableId)
        {
            if(!registeredObjects.TryGetValue(stableId, out var result))
            {
                throw new Exception($"ScopedStableIdProvider.Get: Tried to get object from missing id: '{stableId}'");
            }
            return result;
        }

        public static void ClearAll()
        {
            Instance.ClearAllInternal();
        }
        private void ClearAllInternal()
        {
            registeredObjects.Clear();
            if (scopeStack.Count > 0)
            {
                Debug.LogError($"clearing stable id stack of {typeof(T).Name} when scopes are currently stacked. This will lead to errors");
                scopeStack.Clear();
            }
        }


        public static IDisposable PushScope(string localId)
        {
            return Instance.PushScopeInternal(localId);
        }
        private IDisposable PushScopeInternal(string scope)
        {
            scopeStack.Push(scope);
            return new DisposableAbuse.LambdaDispose(() => PopScope(scope));
        }
        private void PopScope(string scope)
        {
            if (scopeStack.Peek() != scope)
            {
                Debug.LogError("error when popping scope from narrative scope. popped scope does not match top of stack.");
                return;
            }
            scopeStack.Pop();
        }
    }
}
