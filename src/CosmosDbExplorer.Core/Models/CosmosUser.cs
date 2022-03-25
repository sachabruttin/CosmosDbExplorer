using System;
using System.Collections.Generic;
using System.Text;

using CosmosDbExplorer.Core.Contracts;

using Microsoft.Azure.Cosmos;

namespace CosmosDbExplorer.Core.Models
{
    public class CosmosUser : ICosmosResource
    {
        public CosmosUser(UserProperties properties)
        {
            Id = properties.Id;
            ETag = properties.ETag;
            SelfLink = properties.SelfLink;
        }

        public CosmosUser() { }

        public string? Id { get; set; }
        public string? ETag { get; }
        public string? SelfLink { get; }
    }
}
