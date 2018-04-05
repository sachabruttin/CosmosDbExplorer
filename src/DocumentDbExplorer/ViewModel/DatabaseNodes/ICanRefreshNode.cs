using CosmosDbExplorer.Infrastructure;

namespace CosmosDbExplorer.ViewModel
{
    public interface ICanRefreshNode
    {
        RelayCommand RefreshCommand { get; }
    }
}
