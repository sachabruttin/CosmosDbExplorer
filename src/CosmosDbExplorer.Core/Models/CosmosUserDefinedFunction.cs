using CosmosDbExplorer.Core.Contracts;
using Microsoft.Azure.Cosmos.Scripts;

namespace CosmosDbExplorer.Core.Models
{
    public class CosmosUserDefinedFunction : ICosmosScript
    {
        public CosmosUserDefinedFunction(string body)
        {
            Body = body;
        }

        public CosmosUserDefinedFunction(UserDefinedFunctionProperties properties)
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
    }
}
