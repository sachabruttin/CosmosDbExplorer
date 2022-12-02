using CosmosDbExplorer.Core.Contracts;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

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
            IndexingPolicy = JsonConvert.SerializeObject(properties.IndexingPolicy, Formatting.Indented);
            GeospatialType = properties.GeospatialConfig.GeospatialType.ToLocalType();
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public CosmosContainer(string id, bool isLargePartition)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
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
        public int? DefaultTimeToLive { get; set; } // null = off, -1 = Default
        public PartitionKeyDefinitionVersion? PartitionKeyDefVersion { get; }
        public string IndexingPolicy { get; set; }
        public CosmosGeospatialType GeospatialType { get; set; }
    }

    public enum CosmosGeospatialType
    {
        Geography = 0,
        Geometry = 1
    }

    public static class GeospatialTypeExtensions
    {
        public static CosmosGeospatialType ToLocalType(this GeospatialType geospatialType)
        {
            return geospatialType switch
            {
                GeospatialType.Geometry => CosmosGeospatialType.Geometry,
                _ => CosmosGeospatialType.Geography,
            };
        }

        public static GeospatialType FromLocalType(this CosmosGeospatialType geospatialType)
        {
            return geospatialType switch
            {
                CosmosGeospatialType.Geography => GeospatialType.Geography,
                _ => GeospatialType.Geometry,
            };
        }
    }
}

