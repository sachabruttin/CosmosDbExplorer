using System;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Infrastructure;
using CosmosDbExplorer.Messages;
using Microsoft.Toolkit.Mvvm.Input;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class DocumentNodeViewModel : TreeViewItemViewModel<ContainerNodeViewModel>, IHaveContainerNodeViewModel, IContent, IHaveOpenCommand
    {
        private RelayCommand _openCommand;

        public DocumentNodeViewModel(ContainerNodeViewModel parent)
            : base(parent, false)
        {
            Name = "Documents";
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
                            //Messenger.Send(new OpenDocumentsViewMessage(this, Parent.Parent.Parent.Connection, Parent.Collection));
                        }));
            }
        }

        public ContainerNodeViewModel ContainerNode => Parent;

        public string ContentId => Parent.Container.SelfLink + "/Documents";
    }
}
