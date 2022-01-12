using Microsoft.Toolkit.Mvvm.Input;

namespace CosmosDbExplorer.Contracts.ViewModels
{
    public interface ICanRefreshTab
    {
        RelayCommand RefreshCommand { get; }
    }
}
