using UnityEngine;

namespace Dman.Utilities
{
    public static class ComponentExtensions
    {
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
