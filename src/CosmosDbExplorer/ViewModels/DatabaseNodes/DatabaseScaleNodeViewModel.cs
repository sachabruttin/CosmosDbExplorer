using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Messages;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace CosmosDbExplorer.ViewModels.DatabaseNodes
{
    public class DatabaseScaleNodeViewModel : TreeViewItemViewModel<DatabaseNodeViewModel>, IContent, IHaveOpenCommand
    {
        private RelayCommand? _openCommand;

        public DatabaseScaleNodeViewModel(DatabaseNodeViewModel parent)
            : base(parent, false)
        {
            Name = "Scale";
        }

        public string Name { get; private set; }

        public string ContentId => Parent.Database.SelfLink + "/Scale";

        public RelayCommand OpenCommand => _openCommand ??= new(() => Messenger.Send(new OpenDatabaseScaleViewMessage(this, Parent.Parent.Connection, Parent.Database, null)));
    }
}
