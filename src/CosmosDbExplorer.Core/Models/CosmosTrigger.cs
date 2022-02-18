using System;
using CosmosDbExplorer.Core.Contracts;
using Microsoft.Azure.Cosmos.Scripts;

namespace CosmosDbExplorer.Core.Models
{
    public class CosmosTrigger : ICosmosScript
    {
        public CosmosTrigger(string id, string body, string? selfLink)
        {
            Id = id;
            Body = body;
            SelfLink = selfLink;
        }

        public CosmosTrigger(TriggerProperties properties)
        {
            Id = properties.Id;
            ETag = properties.ETag;
            SelfLink = properties.SelfLink;
            Body = properties.Body;
            Operation = (CosmosTriggerOperation)(int)properties.TriggerOperation;
            Type = (CosmosTriggerType)(int)properties.TriggerType;
        }

        public string? Id { get; set; }
        public string? ETag { get; private set; }
        public string? SelfLink { get; private set; }
        public string Body { get; }

        public CosmosTriggerOperation Operation { get; set; }
        public CosmosTriggerType Type { get; set; }
    }

    public enum CosmosTriggerOperation
    {
        //
        // Summary:
        //     Specifies all operations.
        All = 0,
        //
        // Summary:
        //     Specifies create operations only.
        Create = 1,
        //
        // Summary:
        //     Specifies update operations only.
        Update = 2,
        //
        // Summary:
        //     Specifies delete operations only.
        Delete = 3,
        //
        // Summary:
        //     Specifies replace operations only.
        Replace = 4
    }

    public enum CosmosTriggerType
    {
        //
        // Summary:
        //     Trigger should be executed before the associated operation(s).
        Pre = 0,
        //
        // Summary:
        //     Trigger should be executed after the associated operation(s).
        Post = 1
    }
}
