using Microsoft.Azure.Documents;
using System.Collections.Generic;
using Newtonsoft.Json;
using CosmosDbExplorer.Infrastructure.Extensions;

namespace CosmosDbExplorer.Services
{
    public class DocumentDescription
    {
        [JsonConstructor]
        public DocumentDescription(string id, string selfLink, object partitionKey, bool hasPartitionKey)
        {
            Id = id;
            SelfLink = selfLink;
            PartitionKey = partitionKey;
            HasPartitionKey = hasPartitionKey;
        }

        public DocumentDescription(Document document, DocumentCollection collection)
        {
            Id = document.Id;
            SelfLink = document.SelfLink;

            var token = collection.PartitionKey.GetSelectToken();

            if (token != null)
            {
                PartitionKey = document.GetPartitionKeyValue(token);
                HasPartitionKey = true;
            }
        }

        [JsonProperty(PropertyName ="id")]
        public string Id  { get; set; }

        [JsonProperty(PropertyName = "_self")]
        public string SelfLink { get; set; }

        [JsonProperty(PropertyName = "_partitionKey")]
        public object PartitionKey { get; set; }

        [JsonProperty(PropertyName = "_hasPartitionKey")]
        public bool HasPartitionKey { get; set; }

        [JsonIgnore]
        public bool IsSelected { get; set; }
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

        public double RequestCharge { get; internal set; }
    }
}
