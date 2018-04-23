using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace CosmosDbExplorer.Infrastructure.JsonHelpers
{
    public class OrderedDictionaryConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            return typeof(IDictionary<string, string>).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dict = (IDictionary<string, string>)value;
            var obj = new JObject();

            foreach (var key in dict.Keys.OrderBy(name => name))
            {
                obj.Add(key, dict[key]);
            }

            obj.WriteTo(writer);
        }
    }
}
