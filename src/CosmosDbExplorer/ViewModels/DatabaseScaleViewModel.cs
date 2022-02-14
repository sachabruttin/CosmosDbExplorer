using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using CosmosDbExplorer.ViewModels.DatabaseNodes;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

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

        public bool ProvisionThroughput { get; set; } = true;

        public bool IsThroughputAutoscale { get; set; } = true;

        public int MaxThroughput => IsThroughputAutoscale ? 10000 : 100000;

        public int MinThroughput => IsThroughputAutoscale ? 400 : 1000;

        //[OnChangedMethod(nameof(UpdateSaveCommandStatus))]
        public int? Throughput { get; set; }


        public override async void Load(string contentId, DatabaseScaleNodeViewModel node, CosmosConnection connection, CosmosContainer container)
        {
            ContentId = contentId;
            Node = node;
            Title = node.Name;
            Header = node.Name;
            Connection = connection;
            Database = node.Parent.Database;

            AccentColor = connection.AccentColor;
            ToolTip = $"{Connection.Label}/{Database.Id}";

            var service = ActivatorUtilities.CreateInstance<CosmosDatabaseService>(_serviceProvider, connection);
            var throughput = await service.GetThroughputAsync(Database);

            Throughput = throughput.AutoscaleMaxThroughput ?? throughput.Throughput; 
        }
    }
}
