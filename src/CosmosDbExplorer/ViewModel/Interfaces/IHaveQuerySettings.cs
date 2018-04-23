namespace CosmosDbExplorer.ViewModel.Interfaces
{
    public interface IHaveQuerySettings
    {
        bool HideSystemProperties { get; set; }
        bool? EnableScanInQuery { get; set; }
        bool? EnableCrossPartitionQuery { get; set; }
        int? MaxItemCount { get; set; }
        int? MaxDOP { get; set; }
        int? MaxBufferItem { get; set; }
    }
}
