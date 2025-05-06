using System;
using Newtonsoft.Json;
using UnityEngine;


namespace Dman.SaveSystem.Converters
{
    public class Vector3IntConverter : JsonConverter<Vector3Int>
    {
        public override void WriteJson(JsonWriter writer, Vector3Int value, JsonSerializer serializer)
        {
            var serializedModel = new Vector3IntDeserializeModel
            {
                x = value.x,
                y = value.y,
                z = value.z
            };
            serializer.Serialize(writer, serializedModel);
        }

        public override Vector3Int ReadJson(JsonReader reader, Type objectType, Vector3Int existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            Vector3IntDeserializeModel deserialized;
            try
            {
                deserialized = serializer.Deserialize<Vector3IntDeserializeModel>(reader);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to deserialize Vector3Int: {e.Message} at {reader.Path} {reader.Depth}");
                Debug.LogException(e);
                throw;
            }
            if (deserialized == null)
            {
                if (hasExistingValue) return existingValue;
                return default;
            }

            return new Vector3Int(deserialized.x, deserialized.y, deserialized.z);
        }

        private class Vector3IntDeserializeModel
        {
            public int x { get; set; }
            public int y { get; set; }
            public int z { get; set; }
        }
    }
}