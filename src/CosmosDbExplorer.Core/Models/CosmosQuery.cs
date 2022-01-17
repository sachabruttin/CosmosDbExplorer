using CosmosDbExplorer.Core.Contracts;

namespace CosmosDbExplorer.Core.Models
{
    public class CosmosQuery : ICosmosQuery
    {
        public string QueryText { get; set; } = string.Empty;

        public int MaxItemCount { get; set; } = 100;

        public string? ContinuationToken { get; set; }
    }
}
