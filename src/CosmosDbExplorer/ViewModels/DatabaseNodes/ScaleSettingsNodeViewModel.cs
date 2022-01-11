using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Infrastructure;
using CosmosDbExplorer.Messages;
using Microsoft.Toolkit.Mvvm.Input;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class ScaleSettingsNodeViewModel : TreeViewItemViewModel<ContainerNodeViewModel>, IHaveContainerNodeViewModel, IContent
    {
        private RelayCommand _openCommand;

        public ScaleSettingsNodeViewModel(ContainerNodeViewModel parent)
            : base(parent, false)
        {
            Name = "Scale & Settings";
        }

        public string Name { get; private set; }

        public string ContentId => Parent.Container.SelfLink + "/ScaleSettings";

        public RelayCommand OpenCommand
        {
            get
            {
                return _openCommand
                    ?? (_openCommand = new RelayCommand(
                        () => throw new System.NotImplementedException()
                        /*Messenger.Send(new OpenScaleAndSettingsViewMessage(this, Parent.Parent.Parent.Connection, Parent.Collection))*/
                        ));
            }
        }

        public ContainerNodeViewModel ContainerNode => Parent;
    }
}
