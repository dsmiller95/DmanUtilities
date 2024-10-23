using System;
using Newtonsoft.Json;
using UnityEngine;


namespace Dman.SaveSystem.Converters
{
    public class Vector2IntConverter : JsonConverter<Vector2Int>
    {
        public override void WriteJson(JsonWriter writer, Vector2Int value, JsonSerializer serializer)
        {
            writer.WriteRawValue($"{{\"x\":{value.x},\"y\":{value.y}}}");
        }

        public override Vector2Int ReadJson(JsonReader reader, Type objectType, Vector2Int existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var deserialized = serializer.Deserialize<Vector2IntDeserializeModel>(reader);
            if (deserialized == null)
            {
                if (hasExistingValue) return existingValue;
                return default;
            }

            return new Vector2Int(deserialized.x, deserialized.y);
        }

        private class Vector2IntDeserializeModel
        {
            public int x { get; set; }
            public int y { get; set; }
        }
    }
}