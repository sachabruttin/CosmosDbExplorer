using CosmosDbExplorer.Infrastructure;
using CosmosDbExplorer.Infrastructure.Models;
using CosmosDbExplorer.Messages;

namespace CosmosDbExplorer.ViewModel
{
    public class ScaleSettingsNodeViewModel : TreeViewItemViewModel<CollectionNodeViewModel>, IHaveCollectionNodeViewModel, IContent
    {
        private RelayCommand _openCommand;

        public ScaleSettingsNodeViewModel(CollectionNodeViewModel parent)
            : base(parent, parent.MessengerInstance, false)
        {
        }

        public override string Name => "Scale & Settings";

        public string ContentId => Parent.Collection.SelfLink + "/ScaleSettings";

        public RelayCommand OpenCommand
        {
            get
            {
                return _openCommand
                    ?? (_openCommand = new RelayCommand(
                        () => MessengerInstance.Send(new OpenScaleAndSettingsViewMessage(this, Parent.Parent.Parent.Connection, Parent.Collection))));
            }
        }

        public CollectionNodeViewModel CollectionNode => Parent;
    }
}
