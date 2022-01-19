using System;
using CosmosDbExplorer.Core.Contracts;
using Microsoft.Azure.Cosmos.Scripts;

namespace CosmosDbExplorer.Core.Models
{
    public class CosmosStoredProcedure : ICosmosScript
    {
        public CosmosStoredProcedure(string body) 
        {
            Body = body;
        }

        public CosmosStoredProcedure(StoredProcedureProperties properties)
        {
            Id = properties.Id;
            ETag = properties.ETag;
            SelfLink = properties.SelfLink;

            Body = properties.Body;
            LastModified = properties.LastModified;
        }

        public string? Id { get; set; }
        public string? ETag { get; private set; }
        public string? SelfLink { get; private set; }
        public string Body { get; }
        public DateTime? LastModified { get; private set; }
    }
}
