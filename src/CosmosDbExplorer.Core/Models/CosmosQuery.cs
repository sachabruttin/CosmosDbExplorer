using CosmosDbExplorer.Core.Contracts;

namespace CosmosDbExplorer.Core.Models
{
    public class CosmosQuery : ICosmosQuery
    {
        public string QueryText { get; set; } = string.Empty;
        public int MaxItemCount { get; set; } = 100;
        public string? ContinuationToken { get; set; }
        public bool EnableScanInQuery { get; set; } = false;
        public bool EnableCrossPartitionQuery { get; set; } = false;
        public int MaxDOP { get; set; } = -1;
        public int MaxBufferItem { get; set; } = -1;
        public Optional<object?> PartitionKeyValue { get; set; } = new Optional<object?>(null);
    }
}
