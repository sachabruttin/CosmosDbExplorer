using System.Collections.ObjectModel;

namespace CosmosDbExplorer.Contracts.ViewModels
{
    public interface ITreeViewItemViewModel
    {
        ObservableCollection<ITreeViewItemViewModel> Children { get; }
        bool HasDummyChild { get; }
        bool IsExpanded { get; set; }
        bool IsSelected { get; set; }
        bool IsLoading { get; set; }
        ITreeViewItemViewModel Parent { get; }
    }
}
