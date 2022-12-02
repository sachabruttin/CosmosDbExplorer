using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Messages;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class ScaleSettingsNodeViewModel : TreeViewItemViewModel<ContainerNodeViewModel>, IHaveContainerNodeViewModel, IContent, IHaveOpenCommand
    {
        private RelayCommand? _openCommand;

        public ScaleSettingsNodeViewModel(ContainerNodeViewModel parent)
            : base(parent, false)
        {
            Name = "Settings";
        }

        public string Name { get; private set; }

        public string ContentId => Parent.Container.SelfLink + "/Settings";

        public RelayCommand OpenCommand => _openCommand ??= new(() => Messenger.Send(new OpenScaleAndSettingsViewMessage(this, Parent.Parent.Parent.Connection, Parent.Parent.Database, Parent.Container)));

        public ContainerNodeViewModel ContainerNode => Parent;
    }
}
