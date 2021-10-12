using System;
using UnityEngine;

namespace Dman.Utilities.SerializableUnityObjects
{
    [Serializable]
    public class SerializableVector2Int
    {
        private int[] vectorData;
        public SerializableVector2Int(Vector2Int vector)
        {
            vectorData = new int[] {
                vector.x,
                vector.y
                };
        }

        public Vector2Int GetDeserialized()
        {
            var newVector = new Vector2Int();
            newVector.x = vectorData[0];
            newVector.y = vectorData[1];
            return newVector;
        }

        public static implicit operator Vector2Int(SerializableVector2Int serializable) => serializable.GetDeserialized();
        public static implicit operator SerializableVector2Int(Vector2Int notSerializable) => new SerializableVector2Int(notSerializable);
    }
}
