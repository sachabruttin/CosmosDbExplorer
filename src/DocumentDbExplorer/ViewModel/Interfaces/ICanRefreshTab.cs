using CosmosDbExplorer.Infrastructure;

namespace CosmosDbExplorer.ViewModel.Interfaces
{
    public interface ICanRefreshTab
    {
        RelayCommand RefreshCommand { get; }
    }
}
