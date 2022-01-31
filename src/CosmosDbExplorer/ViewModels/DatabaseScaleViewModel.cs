using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;
using Microsoft.Azure.Cosmos;

namespace CosmosDbExplorer.ViewModels
{
    public class DatabaseScaleViewModel : PaneWithZoomViewModel<DatabaseScaleNodeViewModel>
    {
        private IServiceProvider _serviceProvider;

        public DatabaseScaleViewModel(IServiceProvider serviceProvider, IUIServices uiServices)
            : base(uiServices)
        {
            _serviceProvider = serviceProvider;
        }

        public DatabaseScaleNodeViewModel Node { get; private set; }
        public CosmosConnection Connection { get; private set; }
        public CosmosDatabase Database { get; private set; }

        public override void Load(string contentId, DatabaseScaleNodeViewModel node, CosmosConnection connection, CosmosContainer container)
        {
            ContentId = contentId;
            Node = node;
            Title = node.Name;
            Header = node.Name;
            Connection = connection;
            Database = node.Parent.Database;

            AccentColor = connection.AccentColor;
        }
    }
}
