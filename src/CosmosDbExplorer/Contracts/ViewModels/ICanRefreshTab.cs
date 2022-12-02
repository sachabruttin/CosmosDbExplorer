using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace CosmosDbExplorer.Contracts.ViewModels
{
    public interface ICanRefreshTab
    {
        ICommand RefreshCommand { get; }
    }
}
