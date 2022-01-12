namespace CosmosDbExplorer.Contracts.ViewModels
{
    public interface IHaveQuerySettings
    {
        bool HideSystemProperties { get; set; }
        bool? EnableScanInQuery { get; set; }
        bool? EnableCrossPartitionQuery { get; set; }
        int? MaxItemCount { get; set; }
        int? MaxDOP { get; set; }
        int? MaxBufferItem { get; set; }
        string PartitionKeyValue { get; set; }
    }

    public interface IHaveSystemProperties
    {
        bool HideSystemProperties { get; set; }
    }
}
