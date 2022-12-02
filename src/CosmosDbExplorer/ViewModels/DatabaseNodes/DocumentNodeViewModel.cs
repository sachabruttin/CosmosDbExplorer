using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Messages;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class DocumentNodeViewModel : TreeViewItemViewModel<ContainerNodeViewModel>, IHaveContainerNodeViewModel, IContent, IHaveOpenCommand
    {
        private RelayCommand? _openCommand;

        public DocumentNodeViewModel(ContainerNodeViewModel parent)
            : base(parent, false)
        {
            Name = "Items";
        }

        public string Name { get; set; }

        public RelayCommand OpenCommand => _openCommand ??= new(OpenCommandExecute);

        public ContainerNodeViewModel ContainerNode => Parent;

        public string ContentId => Parent.Container.SelfLink + "/Documents";

        private void OpenCommandExecute()
        {
            IsSelected = false;
            Messenger.Send(new OpenDocumentsViewMessage(this, Parent.Parent.Parent.Connection, Parent.Parent.Database, Parent.Container));
        }
    }
}
