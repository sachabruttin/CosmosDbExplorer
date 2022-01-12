using Microsoft.Toolkit.Mvvm.Input;

namespace CosmosDbExplorer.Contracts.ViewModels
{
    public interface IAssetTabCommand
    {
        RelayCommand SaveCommand { get; }
        RelayCommand DiscardCommand { get; }
        RelayCommand DeleteCommand { get; }
    }
}
