using Microsoft.Toolkit.Mvvm.Input;

namespace CosmosDbExplorer.Contracts.ViewModels
{
    public interface ICanRefreshNode
    {
        RelayCommand RefreshCommand { get; }
    }
}
