using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dman.ObjectSets
{
    public abstract class UniqueObjectRegistry : ScriptableObject
    {
        public abstract List<IDableObject> AllObjects { get; set; }

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
            for (var i = 0; i < AllObjects.Count; i++)
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
        public List<T> allObjects;
        public override List<IDableObject> AllObjects
        {
            get => allObjects.Cast<IDableObject>().ToList();
            set => allObjects = value.Cast<T>().ToList();
        }

        public T GetUniqueObjectFromID(int id)
        {
            return allObjects[id];
        }
    }
}
