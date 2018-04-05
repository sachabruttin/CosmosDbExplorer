using CosmosDbExplorer.Infrastructure;

namespace CosmosDbExplorer.ViewModel.Interfaces
{
    public interface IAssetTabCommand
    {
        RelayCommand SaveCommand { get;  } 
        RelayCommand DiscardCommand { get; }
        RelayCommand DeleteCommand { get;  }
    }
}
