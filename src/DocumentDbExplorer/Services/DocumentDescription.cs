using Microsoft.Azure.Documents;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CosmosDbExplorer.Services
{
    public class DocumentDescription
    {
        [JsonConstructor]
        public DocumentDescription(string id, string selfLink, string partitionKey)
        {
            Id = id;
            SelfLink = selfLink;
            PartitionKey = partitionKey;
        }

        public DocumentDescription(Document document, DocumentCollection collection)
        {
            Id = document.Id;
            SelfLink = document.SelfLink;

            var partitionKey = collection.PartitionKey?.Paths.FirstOrDefault();

            if (partitionKey != null)
            {
                PartitionKey = document.GetPropertyValue<string>(partitionKey.TrimStart('/'));
            }
        }

        [JsonProperty(PropertyName ="id")]
        public string Id  { get; set; }

        [JsonProperty(PropertyName = "_self")]
        public string SelfLink { get; set; }

        [JsonProperty(PropertyName = "_partitionKey")]
        public string PartitionKey { get; set; }
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
