using System;
using CosmosDbExplorer.Core.Models;

namespace CosmosDbExplorer.Core.Contracts
{
    public interface ICosmosQuery
    {
        string QueryText { get; set; }
        int MaxItemCount { get; set; }
        string? ContinuationToken { get; set; }
        bool EnableScanInQuery { get; set; }
        bool EnableCrossPartitionQuery { get; set; }
        int MaxDOP { get; set; }
        int MaxBufferItem { get; set; }
        Optional<object?> PartitionKeyValue { get; set; }
    }
}
