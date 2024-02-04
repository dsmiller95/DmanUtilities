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
            referenceID = target == null ? -1 : target.myId;
        }

        public T GetObject<T>() where T : IDableObject
        {
            if (referenceID == -1)
            {
                return null;
            }
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
