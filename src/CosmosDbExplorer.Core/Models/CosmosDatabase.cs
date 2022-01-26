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

        public CosmosDatabase(DatabaseProperties properties)
        {
            Id = properties.Id;
            ETag = properties.ETag;
            SelfLink = properties.SelfLink;
        }

        public string Id { get; }
        public string ETag { get; }
        public string SelfLink { get; }
        
    }
}
