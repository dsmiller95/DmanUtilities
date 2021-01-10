using System;
using UnityEngine;

namespace Dman.ObjectSets
{
    [Serializable]
    public class IDableSavedReference
    {
        int referenceID;
        public IDableSavedReference(IDableObject target)
        {
            referenceID = target.myId;
        }

        public T GetObject<T>() where T : IDableObject
        {
            var registry = RegistryRegistry.GetObjectRegistry<T>();
            if (registry == null)
            {
                return null;
            }
            return registry.GetUniqueObjectFromID(referenceID);
        }
    }
    [Serializable]
    public abstract class IDableObject : ScriptableObject
    {
        public int myId;
    }
}
