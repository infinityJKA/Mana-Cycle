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

        // BG sprite
        [JsonIgnore] [System.NonSerialized]
        public Sprite _bgSprite;
        [JsonIgnore]
        public Sprite bgSprite {
            get {
                if (!_bgSprite) _bgSprite = Resources.Load<Sprite>("ManaIcons/"+bgSpritePath);
                return _bgSprite;
            }
        }
        public string bgSpritePath;

        // Icon sprite
        [JsonIgnore] [System.NonSerialized]
        public Sprite _iconSprite;
        [JsonIgnore]
        public Sprite iconSprite {
            get {
                if (!_iconSprite) _iconSprite = Resources.Load<Sprite>("ManaIcons/"+iconSpritePath);
                return _iconSprite;
            }
        }
        public string iconSpritePath;

        // Ghost sprite
        [JsonIgnore] [System.NonSerialized]
        public Sprite _ghostSprite;
        [JsonIgnore]
        public Sprite ghostSprite {
            get {
                if (!_ghostSprite) _ghostSprite = Resources.Load<Sprite>("ManaIcons/"+ghostSpritePath);
                return _ghostSprite;
            }
        }
        public string ghostSpritePath;


        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 offset = Vector2.zero;

        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 scale = Vector2.one;
        
        public float rotation;
    }

    [System.Serializable]
    public class IconPack : CosmeticItem
    {
        public ManaIcon[] icons;

        public override GameObject MakeIcon(Transform parent)
        {
            throw new NotImplementedException();
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