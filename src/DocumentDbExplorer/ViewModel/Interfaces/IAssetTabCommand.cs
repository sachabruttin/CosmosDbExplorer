using DocumentDbExplorer.Infrastructure;

namespace DocumentDbExplorer.ViewModel.Interfaces
{
    public interface IAssetTabCommand
    {
        RelayCommand SaveCommand { get;  } 
        RelayCommand DiscardCommand { get; }
        RelayCommand DeleteCommand { get;  }
    }
}
