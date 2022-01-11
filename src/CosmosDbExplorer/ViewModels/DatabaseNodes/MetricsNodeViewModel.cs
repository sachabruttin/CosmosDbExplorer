using CosmosDbExplorer.Contracts.ViewModels;
using Microsoft.Toolkit.Mvvm.Input;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class MetricsNodeViewModel : TreeViewItemViewModel<ContainerNodeViewModel>, IHaveContainerNodeViewModel, IContent
    {
        private RelayCommand _openCommand;

        public MetricsNodeViewModel(ContainerNodeViewModel parent)
            : base(parent, false)
        {
            Name = "Collection Metrics";
        }

        public string Name { get; set; }

        public RelayCommand OpenCommand
        {
            get
            {
                return _openCommand
                    ?? (_openCommand = new RelayCommand(
                        () =>
                        {
                            IsSelected = false;
                            //Messenger.Send(new OpenCollectionMetricsViewMessage(this, Parent.Parent.Parent.Connection, Parent.Collection));
                        }));
            }
        }

        public ContainerNodeViewModel ContainerNode => Parent;

        public string ContentId => Parent.Container.SelfLink + "/Metrics";
    }
}
