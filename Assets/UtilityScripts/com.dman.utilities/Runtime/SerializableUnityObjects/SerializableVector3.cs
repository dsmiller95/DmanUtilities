using System;
using UnityEngine;

namespace Dman.Utilities.SerializableUnityObjects
{
    [Serializable]
    public class SerializableVector3
    {
        private float[] vectorData;
        public SerializableVector3(Vector3 vector)
        {
            vectorData = new float[] {
                vector.x,
                vector.y,
                vector.z
                };
        }

        public Vector3 GetDeserialized()
        {
            var newVector = new Vector3();
            newVector.x = vectorData[0];
            newVector.y = vectorData[1];
            newVector.z = vectorData[2];
            return newVector;
        }

        public static implicit operator Vector3(SerializableVector3 serializable) => serializable.GetDeserialized();
        public static implicit operator SerializableVector3(Vector3 notSerializable) => new SerializableVector3(notSerializable);
    }
}
