using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Messages;

namespace DocumentDbExplorer.ViewModel
{
    public class ScaleSettingsNodeViewModel : TreeViewItemViewModel, IHaveCollectionNodeViewModel, IContent
    {
        private RelayCommand _openCommand;

        public ScaleSettingsNodeViewModel(TreeViewItemViewModel parent)
            : base(parent, parent.MessengerInstance, false)
        {
            Name = "Scale & Settings";
        }

        public string Name { get; set; }

        public new CollectionNodeViewModel Parent
        {
            get { return base.Parent as CollectionNodeViewModel; }
        }

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
