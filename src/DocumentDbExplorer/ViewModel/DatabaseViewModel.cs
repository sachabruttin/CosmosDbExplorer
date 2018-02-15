using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Messages;
using DocumentDbExplorer.Services;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;

namespace DocumentDbExplorer.ViewModel
{
    public class DatabaseViewModel : ToolViewModel
    {
        private readonly IDocumentDbService _dbService;
        private readonly ISettingsService _settingsService;

        public DatabaseViewModel(IMessenger messenger, IDocumentDbService dbService, ISettingsService settingsService) : base(messenger)
        {
            Header = "Connections";
            Title = Header;
            IsVisible = true;

            _dbService = dbService;
            _settingsService = settingsService;

            RegisterMessages();
        }

        private void RegisterMessages()
        {
            MessengerInstance.Register<ConnectionSettingSavedMessage>(this, OnConnectionSettingsSaved);
            MessengerInstance.Register<RemoveConnectionMessage>(this, OnRemoveConnection);
        }

        public ObservableCollection<ConnectionNodeViewModel> Nodes { get; private set; }

        public async Task LoadNodesAsync()
        {
            var connections = await _settingsService.GetConnectionsAsync();
            var nodes = connections.Select(c =>
            {
                var connection = SimpleIoc.Default.GetInstanceWithoutCaching<ConnectionNodeViewModel>();
                connection.Connection = c.Value;

                return connection;
            });

            Nodes = new ObservableCollection<ConnectionNodeViewModel>(nodes);
        }

        private void OnRemoveConnection(RemoveConnectionMessage msg)
        {
            var node = Nodes.FirstOrDefault(n => n.Connection == msg.Connection);
            
            if (node != null)
            {
                DispatcherHelper.RunAsync(() => Nodes.Remove(node));
            }
        }

        private void OnConnectionSettingsSaved(ConnectionSettingSavedMessage msg)
        {
            var node = Nodes.FirstOrDefault(n => n.Connection.Equals(msg.Connection));

            if (node != null)
            {
                node.Connection = msg.Connection;
            }
            else
            {
                var connection = SimpleIoc.Default.GetInstanceWithoutCaching<ConnectionNodeViewModel>();
                connection.Connection = msg.Connection;
                Nodes.Add(connection);
            }
        }
    }
}
