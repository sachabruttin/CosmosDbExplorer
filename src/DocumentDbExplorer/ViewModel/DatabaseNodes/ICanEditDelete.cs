using System.Windows.Input;
using DocumentDbExplorer.Infrastructure;

namespace DocumentDbExplorer.ViewModel
{
    public interface ICanEditDelete
    {
        RelayCommand EditCommand { get; }

        RelayCommand DeleteCommand { get; }
    }
}
