using CosmosDbExplorer.Core.Contracts;
using Microsoft.Azure.Cosmos;

namespace CosmosDbExplorer.Core.Models
{
    public class CosmosContainer : ICosmosResource
    {
        public CosmosContainer(ContainerProperties properties)
        {
            Id = properties.Id;
            ETag = properties.ETag;
            SelfLink = properties.SelfLink;
            DefaultTimeToLive = properties.DefaultTimeToLive;
            PartitionKeyPath = properties.PartitionKeyPath;
            IsLargePartitionKey = properties.PartitionKeyDefinitionVersion > PartitionKeyDefinitionVersion.V1;
        }

        public string Id { get; }
        public string ETag { get; }
        public string SelfLink { get; }
        public string PartitionKeyPath { get; }
        public string? PartitionKeyJsonPath => string.IsNullOrEmpty(PartitionKeyPath) ? null : PartitionKeyPath.Replace('/', '.');
        public bool? IsLargePartitionKey { get; }
        public int? DefaultTimeToLive { get; set; }
    }
}

