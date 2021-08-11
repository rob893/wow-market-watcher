using System;
using Azure.Messaging.EventGrid;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            var jo = (string?)reader.Value; //JObject.Load(reader);
            //JObject.
            var result = (string?)jo;

            // var result = new EventGridEvent(
            //     (string?)jo["subject"],
            //     (string?)jo["eventType"],
            //     (string?)jo["dataVersion"],
            //     jo["data"]?.ToObject<object>())
            // {
            //     Id = (string?)jo["id"],
            //     Topic = (string?)jo["topic"],
            //     EventTime = (DateTimeOffset)jo["eventTime"]
            // };

            var thing = new Azure.Core.Serialization.JsonObjectSerializer();
            //thing.

            //System.Text.Json.JsonSerializer.Deserialize<EventGridEvent>(reader.ReadAsString());

            return result;
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}