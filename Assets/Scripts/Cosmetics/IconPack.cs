using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Cosmetics
{
    [System.Serializable]
    public class ManaIcon
    {
        [JsonIgnore] // dictionary key holds the ID now
        public string id;

        [JsonConverter(typeof(ManaIconSpriteConverter))]
        public Sprite bgSprite;

        [JsonConverter(typeof(ManaIconSpriteConverter))]
        public Sprite iconSprite;

        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 offset = Vector2.zero;

        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 scale = Vector2.one;
        
        public float rotation;

        [JsonConverter(typeof(ManaIconSpriteConverter))]
        public Sprite ghostSprite;
    }

    [System.Serializable]
    public class IconPack : CosmeticItem
    {
        public ManaIcon[] icons;

        [JsonIgnore]
        public override Color32 iconColor => Color.white;

        [JsonIgnore]
        public override Sprite icon => throw new System.NotImplementedException();
    }

    public class ManaIconSpriteConverter : JsonConverter<Sprite>
    {
        public override Sprite ReadJson(JsonReader reader, Type objectType, Sprite existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Sprite sprite = default;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    switch (reader.Value.ToString())
                    {
                        case "name":
                            string name = reader.ReadAsString();
                            // don't load the sprite if already loaded
                            if (hasExistingValue && existingValue.name == name) {
                                break;
                            }

                            sprite = Resources.Load<Sprite>("ManaIcons/" + name);
                            break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                    break;
            }

            return sprite;
        }

        public override void WriteJson(JsonWriter writer, Sprite value, JsonSerializer serializer)
        {
            writer.WriteStartObject();            
            if(serializer.TypeNameHandling != TypeNameHandling.None)
            {
                writer.WritePropertyName("$type");
                writer.WriteValue(string.Format("{0}, {1}", value.GetType().ToString(), value.GetType().Assembly.GetName().Name));
            }                
            if (value) {
                writer.WritePropertyName("name");
                writer.WriteValue(value.name);
            }
            writer.WriteEndObject();
        }
    }

    public class Vector2Converter : JsonConverter<Vector2>
    {
        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Vector2 result = default;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    switch (reader.Value.ToString())
                    {
                        case "x":
                            result.x = (float)reader.ReadAsDouble().Value;
                            break;
                        case "y":
                            result.y = (float)reader.ReadAsDouble().Value;
                            break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                    break;
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();            
            if(serializer.TypeNameHandling != TypeNameHandling.None)
            {
                writer.WritePropertyName("$type");
                writer.WriteValue(string.Format("{0}, {1}", value.GetType().ToString(), value.GetType().Assembly.GetName().Name));
            }                
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WriteEndObject();
        }
    }
}