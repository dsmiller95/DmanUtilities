using System;
using UnityEngine;

namespace Dman.Utilities.SerializableUnityObjects
{
    [Serializable]
    public class SerializableVector3Int
    {
        private int[] vectorData;
        public SerializableVector3Int(Vector3Int vector)
        {
            vectorData = new int[] {
                vector.x,
                vector.y,
                vector.z
                };
        }

        public Vector3Int GetDeserialized()
        {
            var newVector = new Vector3Int();
            newVector.x = vectorData[0];
            newVector.y = vectorData[1];
            newVector.z = vectorData[2];
            return newVector;
        }

        public static implicit operator Vector3Int(SerializableVector3Int serializable) => serializable.GetDeserialized();
        public static implicit operator SerializableVector3Int(Vector3Int notSerializable) => new SerializableVector3Int(notSerializable);
    }
}
