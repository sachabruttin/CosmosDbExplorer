using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Messages;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class DocumentNodeViewModel : TreeViewItemViewModel<ContainerNodeViewModel>, IHaveContainerNodeViewModel, IContent, IHaveOpenCommand
    {
        public DocumentNodeViewModel(ContainerNodeViewModel parent)
            : base(parent, false)
        {
            Name = "Documents";
        }

        public string Name { get; set; }

        public RelayCommand OpenCommand => new(OpenCommandExecute);

        public ContainerNodeViewModel ContainerNode => Parent;

        public string ContentId => Parent.Container.SelfLink + "/Documents";

        private void OpenCommandExecute()
        {
            IsSelected = false;
            Messenger.Send(new OpenDocumentsViewMessage(this, Parent.Parent.Parent.Connection, Parent.Container));
        }
    }
}
