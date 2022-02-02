using System;
using CosmosDbExplorer.Core.Contracts;
using Newtonsoft.Json;

namespace CosmosDbExplorer.Core.Models
{
    public class CosmosDocument : ICosmosDocument
    {
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
    }
}
