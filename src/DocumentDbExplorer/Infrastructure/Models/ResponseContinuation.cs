using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DocumentDbExplorer.Infrastructure.Models
{
    public class ResponseContinuation
    { 
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("range")]
        public Range Range { get; set; }
    }

    public class Range
    {

        [JsonProperty("min")]
        public string Minimum { get; set; }

        [JsonProperty("max")]
        public string Maximum { get; set; }
    }

    public class ResponseContinuationListConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new System.NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(List<ResponseContinuation>).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                var item = JObject.Load(reader);
                var rc = item.ToObject<ResponseContinuation>();

                return new List<ResponseContinuation> { rc };
            }
            else
            {
                var array = JArray.Load(reader);
                return array.ToObject<List<ResponseContinuation>>();
            }
        }
    }

}
