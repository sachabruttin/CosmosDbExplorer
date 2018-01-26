namespace DocumentDbExplorer.ViewModel.Interfaces
{
    public interface IHaveQuerySettings
    {
        bool HideSystemProperties { get; set; }
        bool? EnableScanInQuery { get; set; }
        bool? EnableCrossPartitionQuery { get; set; }
    }
}
