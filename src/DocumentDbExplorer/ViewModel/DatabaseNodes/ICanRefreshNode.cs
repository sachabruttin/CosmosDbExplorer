using DocumentDbExplorer.Infrastructure;

namespace DocumentDbExplorer.ViewModel
{
    public interface ICanRefreshNode
    {
        RelayCommand RefreshCommand { get; }
    }
}
