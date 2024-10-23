using System;
using Newtonsoft.Json;
using UnityEngine;


namespace Dman.SaveSystem.Converters
{
    public class Vector3IntConverter : JsonConverter<Vector3Int>
    {
        public override void WriteJson(JsonWriter writer, Vector3Int value, JsonSerializer serializer)
        {
            writer.WriteRawValue($"{{\"x\":{value.x},\"y\":{value.y},\"z\":{value.z}}}");
        }

        public override Vector3Int ReadJson(JsonReader reader, Type objectType, Vector3Int existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var deserialized = serializer.Deserialize<Vector3IntDeserializeModel>(reader);
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