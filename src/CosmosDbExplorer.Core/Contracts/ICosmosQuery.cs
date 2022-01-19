namespace CosmosDbExplorer.Core.Contracts
{
    public interface ICosmosQuery
    {
        string QueryText { get; set; }
        int MaxItemCount { get; set;  }
        string? ContinuationToken { get; set; }  
        bool HideSystemProperties { get; set; }
        bool EnableScanInQuery { get; set; }
        bool EnableCrossPartitionQuery { get; set; }
        int MaxDOP { get; set; }
        int MaxBufferItem { get; set; }
        string? PartitionKeyValue { get; set; }
    }
}
