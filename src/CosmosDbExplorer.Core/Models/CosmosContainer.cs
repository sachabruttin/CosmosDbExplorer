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
            PartitionKeyDefVersion = properties.PartitionKeyDefinitionVersion;
        }

        public CosmosContainer(string id, bool isLargePartition) 
        {
            Id = id;
            PartitionKeyDefVersion = isLargePartition
                ? PartitionKeyDefinitionVersion.V2
                : PartitionKeyDefinitionVersion.V1;
        }

        public string Id { get; }
        public string ETag { get; }
        public string SelfLink { get; }
        public string PartitionKeyPath { get; set; }
        public string? PartitionKeyJsonPath => string.IsNullOrEmpty(PartitionKeyPath) ? null : PartitionKeyPath.Replace('/', '.');
        public bool? IsLargePartitionKey => PartitionKeyDefVersion > PartitionKeyDefinitionVersion.V1;
        public int? DefaultTimeToLive { get; set; }
        public PartitionKeyDefinitionVersion? PartitionKeyDefVersion { get; }
    }
}

