using System;
using UnityEngine;

namespace Dman.Utilities.SerializableUnityObjects
{
    [Serializable]
    public class SerializableVector2
    {
        private float[] vectorData;
        public SerializableVector2(Vector2 vector)
        {
            vectorData = new float[] {
                vector.x,
                vector.y
                };
        }

        public Vector2 GetDeserialized()
        {
            var newVector = new Vector2();
            newVector.x = vectorData[0];
            newVector.y = vectorData[1];
            return newVector;
        }

        public static implicit operator Vector2(SerializableVector2 serializable) => serializable.GetDeserialized();
        public static implicit operator SerializableVector2(Vector2 notSerializable) => new SerializableVector2(notSerializable);
    }
}
