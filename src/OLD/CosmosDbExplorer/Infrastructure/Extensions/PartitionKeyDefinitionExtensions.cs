using System.Linq;
using Microsoft.Azure.Documents;

namespace CosmosDbExplorer.Infrastructure.Extensions
{
    public static class PartitionKeyDefinitionExtensions
    {
        public static string GetSelectToken(this PartitionKeyDefinition input)
        {
            return GetQueryToken(input)?
                                .Replace("[\"", "['")
                                .Replace("\"]", "']");
        }

        public static string GetQueryToken(this PartitionKeyDefinition input)
        {
            var partitionKey = input?.Paths.FirstOrDefault();

            if (partitionKey == null)
            {
                return null;
            }

            var nodes = partitionKey.TrimStart('/')
                        .Split('/')
                        .Select(node => node[0] == '"' ? $"[{node}]" : $".{node}");

            return string.Concat(nodes);
        }
    }
}
