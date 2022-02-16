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
        }

        public string? Id { get; set; }
        public string? ETag { get; private set; }
        public string? SelfLink { get; private set; }
        public string Body { get; }

        public TriggerOperation Operation { get; set; }
        public TriggerType Type { get; set; }
    }
}
