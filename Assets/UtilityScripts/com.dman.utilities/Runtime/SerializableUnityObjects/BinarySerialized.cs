using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Dman.Utilities.SerializableUnityObjects
{
    [Serializable]
    public class BinarySerialized<T> : ISerializationCallbackReceiver where T : class, new()
    {
        public T data;
        [SerializeField]
        private byte[] allFileData;

        public BinarySerialized()
        {
            this.data = new T();
        }
        public BinarySerialized(T backing)
        {
            this.data = backing;
        }
        public void OnAfterDeserialize()
        {
            if (allFileData == null)
            {
                data = null;
                return;
            }
            using (MemoryStream stream = new MemoryStream(allFileData))
            {
                var resultObject = new BinaryFormatter().Deserialize(stream);
                data = resultObject as T;
            }

        }

        public void OnBeforeSerialize()
        {
            if (data == null)
            {
                allFileData = null;
                return;
            }
            using (MemoryStream stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, data);
                this.allFileData = stream.ToArray();
            }
        }
    }
}
