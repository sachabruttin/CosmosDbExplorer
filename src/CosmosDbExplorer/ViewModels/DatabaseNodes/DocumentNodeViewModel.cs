using System;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Infrastructure;
using CosmosDbExplorer.Messages;
using Microsoft.Toolkit.Mvvm.Input;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class DocumentNodeViewModel : TreeViewItemViewModel<ContainerNodeViewModel>, IHaveContainerNodeViewModel, IContent
    {
        private RelayCommand _openDocumentCommand;

        public DocumentNodeViewModel(ContainerNodeViewModel parent)
            : base(parent, false)
        {
            Name = "Documents";
        }

        public string Name { get; set; }

        public RelayCommand OpenDocumentCommand
        {
            get
            {
                return _openDocumentCommand
                    ?? (_openDocumentCommand = new RelayCommand(
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
