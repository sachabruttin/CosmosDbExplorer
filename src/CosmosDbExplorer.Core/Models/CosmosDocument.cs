using System;
using System.Collections.Generic;
using System.Linq;

using CosmosDbExplorer.Core.Contracts;
using CosmosDbExplorer.Core.Services;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CosmosDbExplorer.Core.Models
{
    public class CosmosDocument : ICosmosDocument
    {

        public static CosmosDocument CreateFrom(JObject resource, IList<string> partitionPath)
        {
            var instance = resource.ToObject<CosmosDocument>() ?? throw new Exception("Cannot create CosmosDocument");

            if (partitionPath is not null)
            {
                var doc = new Document(resource, partitionPath);

                switch (partitionPath.Count)
                {
                    case 1:
                        instance.PartitionKey0 = doc.PartitionKey0;
                        instance.HasPartitionKey = true;
                        break;
                    case 2:
                        instance.PartitionKey0 = doc.PartitionKey0;
                        instance.PartitionKey1 = doc.PartitionKey1;
                        instance.HasPartitionKey = true;
                        break;
                    case 3:
                        instance.PartitionKey0 = doc.PartitionKey0;
                        instance.PartitionKey1 = doc.PartitionKey1;
                        instance.PartitionKey2 = doc.PartitionKey2;
                        instance.HasPartitionKey = true;
                        break;
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

        [JsonProperty("_pk0")]
        public object? PartitionKey0 { get; set; }

        [JsonProperty("_pk1")]
        public object? PartitionKey1 { get; set; }

        [JsonProperty("_pk2")]
        public object? PartitionKey2 { get; set; }

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
                    && PartitionKey0 == other.PartitionKey0
                    && PartitionKey1 == other.PartitionKey1
                    && PartitionKey2 == other.PartitionKey2;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, SelfLink, PartitionKey0, PartitionKey1, PartitionKey2);
        }
    }
}
