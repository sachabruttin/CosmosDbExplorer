using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Infrastructure;
using CosmosDbExplorer.Messages;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class ScaleSettingsNodeViewModel : TreeViewItemViewModel<ContainerNodeViewModel>, IHaveContainerNodeViewModel, IContent, IHaveOpenCommand
    {
        private RelayCommand _openCommand;

        public ScaleSettingsNodeViewModel(ContainerNodeViewModel parent)
            : base(parent, false)
        {
            Name = "Scale & Settings";
        }

        public string Name { get; private set; }

        public string ContentId => Parent.Container.SelfLink + "/ScaleSettings";

        public RelayCommand OpenCommand => new(() => Messenger.Send(new OpenScaleAndSettingsViewMessage(this, Parent.Parent.Parent.Connection, Parent.Container)));

        public ContainerNodeViewModel ContainerNode => Parent;
    }
}
