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


        private HashSet<string> usedIds = new HashSet<string>();
        private Stack<string> scopeStack = new Stack<string>();

        private ScopedStableIdProvider()
        {

        }

        public static string GetStableId(string localId)
        {
            return Instance.GetStableIdInternal(localId);
        }
        public static IDisposable PushScope(string localId)
        {
            return Instance.PushScopeInternal(localId);
        }
        public static void ClearAll()
        {
            Instance.ClearAllInternal();
        }

        private string GetStableIdInternal(string localIdentifier)
        {
            var baseScope = string.Join(".", scopeStack);
            var newId = baseScope + "." + localIdentifier;
            Debug.Log($"adding a {typeof(T).Name} with id: {newId}");
            if (!usedIds.Add(newId))
            {
                Debug.LogError($"adding a {typeof(T).Name} with an id which has already been used: {newId}");
            }
            return newId;
        }

        private IDisposable PushScopeInternal(string scope)
        {
            scopeStack.Push(scope);
            return new DisposableAbuse.LambdaDispose(() => PopScope(scope));
        }

        private void ClearAllInternal()
        {
            usedIds.Clear();
            if(scopeStack.Count > 0)
            {
                Debug.LogError($"clearing stable id stack of {typeof(T).Name} when scopes are currently stacked. This will lead to errors");
                scopeStack.Clear();
            }
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
