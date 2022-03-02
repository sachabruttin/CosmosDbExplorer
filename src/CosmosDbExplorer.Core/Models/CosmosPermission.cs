
using System;

using CosmosDbExplorer.Core.Contracts;

using Microsoft.Azure.Cosmos;

namespace CosmosDbExplorer.Core.Models
{
    public class CosmosPermission : ICosmosResource
    {
        public CosmosPermission(PermissionProperties properties)
        {
            Id = properties.Id;
            ETag = properties.ETag;
            SelfLink = properties.SelfLink;
            PermissionMode = (CosmosPermissionMode)properties.PermissionMode;
            PartitionKey = properties.ResourcePartitionKey?.ToString();
            ResourceUri = properties.ResourceUri;
            Token = properties.Token;
            LastModifed = properties.LastModified;
        }

        public CosmosPermission()
        {
        }

        public string? Id { get; }
        public string? ETag { get; }
        public string? SelfLink { get; }
        public CosmosPermissionMode PermissionMode { get; set; }
        public string? PartitionKey;
        public string ResourceUri { get; set; }
        public string Token { get; }
        public DateTime? LastModifed { get; }
    }

    public enum CosmosPermissionMode : byte
    {
        //
        // Summary:
        //     Read permission mode will provide the user with Read only access to a resource.
        Read = 0x1,
        //
        // Summary:
        //     All permission mode will provide the user with full access(read, insert, replace
        //     and delete) to a resource.
        All = 0x2
    }
}
