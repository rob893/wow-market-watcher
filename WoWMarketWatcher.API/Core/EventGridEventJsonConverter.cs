using System;
using Azure.Messaging.EventGrid;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WoWMarketWatcher.API.Extensions;

namespace WoWMarketWatcher.API.Core
{
    public class EventGridEventJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(EventGridEvent);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            var asString = jo.ToJson();

            // Newtonsoft is unable to deserialize the event grid event however system.text.json can.
            var result = System.Text.Json.JsonSerializer.Deserialize<EventGridEvent>(asString);

            return result;
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}