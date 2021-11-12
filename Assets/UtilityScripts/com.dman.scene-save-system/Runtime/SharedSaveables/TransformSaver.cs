using Dman.Utilities.SerializableUnityObjects;
using System;
using UnityEngine;

namespace Dman.SceneSaveSystem.SharedSaveables
{
    public class TransformSaver : MonoBehaviour, ISaveableData
    {
        [Serializable]
        class TransformSaveObject
        {
            private SerializableVector3 position;
            private SerializableVector3 eulerAngles;
            private SerializableVector3 scale;

            public TransformSaveObject(Transform source)
            {
                position = source.position;
                eulerAngles = source.eulerAngles;
                scale = source.localScale;
            }

            public void Apply(Transform target)
            {
                target.position = position;
                target.eulerAngles = eulerAngles;
                target.localScale = scale;
            }
        }

        public string uniqueNameInsideContext = "Something Unique";

        public string UniqueSaveIdentifier => "transform data: " + uniqueNameInsideContext;

        public int LoadOrderPriority => 0;

        public object GetSaveObject()
        {
            return new TransformSaveObject(transform);
        }

        public void SetupFromSaveObject(object save)
        {
            if (save is TransformSaveObject savedData)
            {
                savedData.Apply(transform);
            }
        }
    }
}
