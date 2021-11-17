using UnityEngine;

namespace Dman.ObjectSets
{
    public abstract class UniqueObjectRegistry : ScriptableObject
    {
        public abstract IDableObject[] AllObjects { get; }

        private void Awake()
        {
            OnObjectSetChanged();
        }
        /// <summary>
        /// Override this method to respond to changes to the set of objects stored in the registry
        /// </summary>
        public virtual void OnObjectSetChanged()
        {

        }
        public void AssignAllIDs()
        {
            for (var i = 0; i < AllObjects.Length; i++)
            {
                var uniqueObject = AllObjects[i];
                uniqueObject.myId = i;
            }
            OnObjectSetChanged();
        }
    }

    /// <summary>
    /// Extend this class to create a generic unique object registry. These are used to maintain a set of scriptable objects
    ///     which need to be differentiated. Basically a very simple form of a static lookup table.
    ///     See com.dman.scenesavesystem for example of how this can be used with a save system to save references
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class UniqueObjectRegistryWithAccess<T> : UniqueObjectRegistry where T : IDableObject
    {
        public T[] allObjects;
        public override IDableObject[] AllObjects => allObjects;

        public T GetUniqueObjectFromID(int id)
        {
            return allObjects[id];
        }
    }
}
