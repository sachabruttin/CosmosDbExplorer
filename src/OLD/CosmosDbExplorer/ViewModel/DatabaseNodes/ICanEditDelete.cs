using System.Windows.Input;
using CosmosDbExplorer.Infrastructure;

namespace CosmosDbExplorer.ViewModel
{
    public interface ICanEditDelete
    {
        RelayCommand EditCommand { get; }

        RelayCommand DeleteCommand { get; }
    }
}
