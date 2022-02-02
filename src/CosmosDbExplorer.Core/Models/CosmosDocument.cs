using System;
using CosmosDbExplorer.Core.Contracts;
using CosmosDbExplorer.Core.Services;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CosmosDbExplorer.Core.Models
{
    public class CosmosDocument : ICosmosDocument
    {

        public static CosmosDocument CreateFrom(JObject resource, string? partitionPath)
        {
            var instance = resource.ToObject<CosmosDocument>();

            if (instance is null)
            {
                throw new Exception("Cannot create CosmosDocument");
            }

            if (partitionPath is not null)
            {
                var doc = new Document(resource, partitionPath);

                if (doc.PK is not null)
                {
                    instance.HasPartitionKey = true;
                    instance.PartitionKey = doc.PK;
                }
            }

            return instance;
        }

        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("_etag")]
        public string? ETag { get; set; }

        [JsonProperty("_self")]
        public string? SelfLink { get; set; }

        [JsonProperty("_attachments")]
        public string? Attachments { get; set; }

        [JsonProperty("_ts")]
        public long TimeStamp { get; set; }

        [JsonProperty("_partitionKey")]
        public object? PartitionKey { get; set; } 

        [JsonProperty("_hasPartitionKey")]
        public bool HasPartitionKey { get; set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ICosmosDocument);
        }

        public bool Equals(ICosmosDocument? other)
        {
            return other != null
                    && Id == other.Id
                    && PartitionKey == other.PartitionKey;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, SelfLink, PartitionKey);
        }
    }
}
