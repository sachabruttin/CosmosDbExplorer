using System.Windows.Input;

namespace CosmosDbExplorer.Contracts.ViewModels
{
    public interface IAssetTabCommand
    {
        ICommand SaveCommand { get; }
        ICommand DiscardCommand { get; }
        ICommand DeleteCommand { get; }
    }
}
