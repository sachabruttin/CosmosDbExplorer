namespace DocumentDbExplorer.ViewModel
{
    public interface IHaveQuerySettings
    {
        bool HideSystemProperties { get; set; }
        bool EnableScanInQuery { get; set; }
        bool EnableCrossPartitionQuery { get; set; }
    }
}
