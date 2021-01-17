using System;
using UnityEngine;

namespace Dman.Utilities.SerializableUnityObjects
{
    [Serializable]
    public class SerializableVector4
    {
        private float[] vectorData;
        public SerializableVector4(Vector4 vector)
        {
            vectorData = new float[] {
                vector.x,
                vector.y,
                vector.z,
                vector.w
                };
        }

        public Vector2 GetDeserialized()
        {
            var newVector = new Vector4();
            newVector.x = vectorData[0];
            newVector.y = vectorData[1];
            newVector.z = vectorData[2];
            newVector.w = vectorData[3];
            return newVector;
        }

        public static implicit operator Vector4(SerializableVector4 serializable) => serializable.GetDeserialized();
        public static implicit operator SerializableVector4(Vector4 notSerializable) => new SerializableVector4(notSerializable);
    }
}
