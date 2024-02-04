using System;
using UnityEditor;
using UnityEngine;

namespace Dman.Utilities
{
    public static class AssetUtilities
    {
        public static ScriptableObject[] GetAllInstances(Type instanceType)
        {
            if (!typeof(ScriptableObject).IsAssignableFrom(instanceType))
            {
                return null;
            }
            var guids = AssetDatabase.FindAssets("t:" + instanceType.Name);  //FindAssets uses tags check documentation for more info
            var a = new ScriptableObject[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            }

            return a;
        }
        public static T[] GetAllInstances<T>() where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);  //FindAssets uses tags check documentation for more info
            T[] a = new T[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return a;
        }
    }
}
