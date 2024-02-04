using System;
using UnityEngine;

namespace Dman.SceneSaveSystem.SharedSaveables
{
    public class EnabledSaver : MonoBehaviour, ISaveableData
    {
        [Serializable]
        class SaveObject
        {
            private bool isActive;

            public SaveObject(GameObject source)
            {
                isActive = source.activeInHierarchy;
            }

            public void Apply(GameObject target)
            {
                target.SetActive(isActive);
            }
        }

        public string uniqueNameInsideContext = "Something Unique";

        public string UniqueSaveIdentifier => "gameobject is active: " + uniqueNameInsideContext;

        public int LoadOrderPriority => 0;

        public object GetSaveObject()
        {
            return new SaveObject(gameObject);
        }

        public void SetupFromSaveObject(object save)
        {
            if (save is SaveObject savedData)
            {
                savedData.Apply(gameObject);
            }
        }
    }
}
