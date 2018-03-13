using System;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Messages;

namespace DocumentDbExplorer.ViewModel
{
    public class DocumentNodeViewModel : TreeViewItemViewModel<CollectionNodeViewModel>, IHaveCollectionNodeViewModel, IContent
    {
        private RelayCommand _openDocumentCommand;

        public DocumentNodeViewModel(CollectionNodeViewModel parent)
            : base(parent, parent.MessengerInstance, false)
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
                        () => {
                            IsSelected = false;
                            MessengerInstance.Send(new OpenDocumentsViewMessage(this, Parent.Parent.Parent.Connection, Parent.Collection));
                        }));
            }
        }

        public CollectionNodeViewModel CollectionNode => Parent;

        public string ContentId => Parent.Collection.SelfLink + "/Documents";
    }
}
