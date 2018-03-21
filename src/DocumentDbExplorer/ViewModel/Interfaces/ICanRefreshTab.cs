using DocumentDbExplorer.Infrastructure;

namespace DocumentDbExplorer.ViewModel.Interfaces
{
    public interface ICanRefreshTab
    {
        RelayCommand RefreshCommand { get; }
    }
}
