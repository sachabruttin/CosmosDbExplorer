using System.Linq;
using Microsoft.Azure.Documents;
using Newtonsoft.Json.Linq;

namespace CosmosDbExplorer.Infrastructure.Extensions
{

    public static class DocumentExtensions
    {
        public static object GetPartitionKeyValue(this Document document, string selectToken)
        {
            return JObject.FromObject(document).SelectToken(selectToken).ToObject<object>();
        }
    }
}
