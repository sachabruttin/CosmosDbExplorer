using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Messages;

namespace DocumentDbExplorer.ViewModel
{
    public class DocumentNodeViewModel : TreeViewItemViewModel, IHaveCollectionNodeViewModel
    {
        private RelayCommand _openDocumentCommand;

        public DocumentNodeViewModel(CollectionNodeViewModel parent)
            : base(parent, parent.MessengerInstance, false)
        {
            Name = "Documents";
        }

        public string Name { get; set; }

        public new CollectionNodeViewModel Parent 
        {
            get { return base.Parent as CollectionNodeViewModel; }
        }

        public RelayCommand OpenDocumentCommand
        {
            get
            {
                return _openDocumentCommand
                    ?? (_openDocumentCommand = new RelayCommand(
                        x => {
                            IsSelected = false;
                            MessengerInstance.Send(new OpenDocumentsViewMessage(this));
                        }));
            }
        }

        public CollectionNodeViewModel CollectionNode => Parent;
    }
}
