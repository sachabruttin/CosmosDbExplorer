using CosmosDbExplorer.Infrastructure;
using CosmosDbExplorer.Infrastructure.Models;
using CosmosDbExplorer.Messages;
using Microsoft.Azure.Documents;

namespace CosmosDbExplorer.ViewModel
{
    public class DatabaseScaleNodeViewModel : TreeViewItemViewModel<DatabaseNodeViewModel>, IContent
    {
        private RelayCommand _openCommand;

        public DatabaseScaleNodeViewModel(DatabaseNodeViewModel parent)
            : base(parent, parent.MessengerInstance, false)
        {
        }

        public string Name => "Scale";
        
        public string ContentId => Parent.Database.SelfLink + "/Scale";

        public Database Database => Parent.Database;

        public RelayCommand OpenCommand
        {
            get
            {
                return _openCommand
                    ?? (_openCommand = new RelayCommand(
                        () => MessengerInstance.Send(new OpenDatabaseScaleViewMessage(this, Parent.Parent.Connection, null))));
            }
        }

    }
}
