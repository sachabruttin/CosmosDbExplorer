using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CosmosDbExplorer.Helpers
{
    public class DocumentDbWithoutSystemPropertyResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var systemResourceNames = new HashSet<string> { "_rid", "_etag", "_ts", "_self", "_id", "_attachments", "_docs", "_sprocs", "_triggers", "_udfs", "_conflicts", "_colls", "_users" };
            var prop = base.CreateProperty(member, memberSerialization);

            if (prop.PropertyName == "id")
            {
                prop.NullValueHandling = NullValueHandling.Ignore;
            }
            else if (prop.PropertyName is not null && systemResourceNames.Contains(prop.PropertyName))
            {
                prop.Readable = false;
            }

            return prop;
        }
    }
}
