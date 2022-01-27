using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;

namespace CosmosDbExplorer.Contracts.ViewModels
{
    public interface ICanRefreshTab
    {
        ICommand RefreshCommand { get; }
    }
}
