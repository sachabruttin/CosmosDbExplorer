using CosmosDbExplorer.Core.Contracts;
using Microsoft.Azure.Cosmos;

namespace CosmosDbExplorer.Core.Models
{
    public class CosmosDatabase : ICosmosResource
    {
        public CosmosDatabase(string id)
        {
            Id = id;
        }

        public CosmosDatabase(DatabaseProperties properties, int? throughput, bool isServerless)
        {
            Id = properties.Id;
            ETag = properties.ETag;
            SelfLink = properties.SelfLink;
            Throughput = throughput;
            IsServerless = isServerless;
        }

        public string Id { get; }
        public string? ETag { get; }
        public string? SelfLink { get; }
        public int? Throughput { get; } // Null value indicates a container with no throughput provisioned.
        public bool IsServerless { get; }
    }
}
