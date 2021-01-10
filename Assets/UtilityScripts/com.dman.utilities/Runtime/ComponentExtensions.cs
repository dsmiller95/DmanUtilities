using UnityEngine;

namespace Dman.Utilities
{
    public static class ComponentExtensions
    {
        public static void DestroyAllChildren(this GameObject gameObject)
        {
            for (int i = gameObject.transform.childCount; i > 0; --i)
            {
                Object.DestroyImmediate(gameObject.transform.GetChild(0).gameObject);
            }
        }
        public static void DestroyAllChildren(this GameObject gameObject, System.Func<GameObject, bool> predicate)
        {
            for (int i = gameObject.transform.childCount; i > 0; --i)
            {
                var child = gameObject.transform.GetChild(0).gameObject;
                if (predicate(child))
                {
                    Object.DestroyImmediate(child);
                }
            }
        }
    }
}
