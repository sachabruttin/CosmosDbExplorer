using Microsoft.Azure.Documents;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CosmosDbExplorer.Infrastructure.Extensions;

namespace CosmosDbExplorer.Services
{
    public class DocumentDescription
    {
        [JsonConstructor]
        public DocumentDescription(string id, string selfLink, object partitionKey)
        {
            Id = id;
            SelfLink = selfLink;
            PartitionKey = partitionKey;
        }

        public DocumentDescription(Document document, DocumentCollection collection)
        {
            Id = document.Id;
            SelfLink = document.SelfLink;

            var token = collection.PartitionKey.GetSelectToken();

            if (token != null)
            {
                PartitionKey = document.GetPartitionKeyValue(token);
            }
        }

        [JsonProperty(PropertyName ="id")]
        public string Id  { get; set; }

        [JsonProperty(PropertyName = "_self")]
        public string SelfLink { get; set; }

        [JsonProperty(PropertyName = "_partitionKey")]
        public object PartitionKey { get; set; }
    }

    public class DocumentDescriptionList : List<DocumentDescription>
    {
        public DocumentDescriptionList(IEnumerable<DocumentDescription> collection)
            : base(collection)
        {

        }

        public bool HasMore
        {
            get { return ContinuationToken != null; }
        }

        public string ContinuationToken { get; set; }

        public long CollectionSize { get; set; }
        public double RequestCharge { get; internal set; }
    }
}
