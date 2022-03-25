using System.Windows.Input;

namespace CosmosDbExplorer.Contracts.ViewModels
{
    public interface ICanRefreshNode
    {
        ICommand RefreshCommand { get; }
    }
}
