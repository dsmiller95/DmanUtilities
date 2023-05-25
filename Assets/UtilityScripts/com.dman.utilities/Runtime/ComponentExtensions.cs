using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dman.Utilities
{
    public static class ComponentExtensions
    {
        public static IEnumerable<Transform> AllChildren(this Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                yield return transform.GetChild(i);
            }
        }
        /// <summary>
        /// Should be refactored away. this is handy but not performant, should be easy to cache a list instead.
        /// </summary>
        /// <param name="transform"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> AllChildren<T>(this Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var childComponent = transform.GetChild(i).GetComponent<T>();
                if(childComponent != null) yield return childComponent;
            }
        }
        
        public static void DestroyAllChildren(this GameObject gameObject, bool destroyImmediate)
        {
            for (int i = gameObject.transform.childCount - 1; i >= 0; --i)
            {
                var child = gameObject.transform.GetChild(i).gameObject;
                if (destroyImmediate)
                {
                    Object.DestroyImmediate(child);
                }else
                {
                    Object.Destroy(child);
                }
            }
        }
        public static void DestroyAllChildren(this GameObject gameObject, System.Func<GameObject, bool> predicate, bool destroyImmediate)
        {
            for (int i = gameObject.transform.childCount - 1; i >= 0; --i)
            {
                var child = gameObject.transform.GetChild(i).gameObject;
                if (!predicate(child))
                {
                    continue;
                }
                if (destroyImmediate)
                {
                    Object.DestroyImmediate(child);
                }
                else
                {
                    Object.Destroy(child);
                }
            }
        }
    }
}
