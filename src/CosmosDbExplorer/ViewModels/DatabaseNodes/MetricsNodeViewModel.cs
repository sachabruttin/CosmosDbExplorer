using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Messages;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class MetricsNodeViewModel : TreeViewItemViewModel<ContainerNodeViewModel>, IHaveContainerNodeViewModel, IContent, IHaveOpenCommand
    {
        private RelayCommand _openCommand;

        public MetricsNodeViewModel(ContainerNodeViewModel parent)
            : base(parent, false)
        {
            Name = "Collection Metrics";
        }

        public string Name { get; set; }

        public RelayCommand OpenCommand => _openCommand ??= new(ExecuteOpenCommand);

        public ContainerNodeViewModel ContainerNode => Parent;

        public string ContentId => Parent.Container.SelfLink + "/Metrics";

        private void ExecuteOpenCommand()
        {
            IsSelected = false;
            Messenger.Send(new OpenMetricsViewMessage(this, Parent.Parent.Parent.Connection, Parent.Container));
        }
    }
}
