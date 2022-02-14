using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Core;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using CosmosDbExplorer.ViewModels.DatabaseNodes;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

using Validar;

namespace CosmosDbExplorer.ViewModels
{
    public class ContainerScaleSettingsViewModel : PaneWithZoomViewModel<ScaleSettingsNodeViewModel>
    {
        private readonly IServiceProvider _serviceProvider;

        public ContainerScaleSettingsViewModel(IServiceProvider serviceProvider, IUIServices uiServices) 
            : base(uiServices)
        {
            _serviceProvider = serviceProvider;
        }

        public ScaleSettingsNodeViewModel Node { get; private set; }
        public CosmosConnection Connection { get; private set; }
        public CosmosContainer Container { get; private set; }

        public bool? IsTimeLiveInSecondVisible => TimeToLive == TimeToLiveType.On;

        public int? TimeToLiveInSecond { get; set; }

        public TimeToLiveType? TimeToLive { get; set; }

        public CosmosGeospatialType GeoType { get; set; }

        public string? IndexingPolicy { get; set; }

        public bool IsIndexingPolicyChanged { get; set; }


        public int? Throughput { get; set; }


        public override async void Load(string contentId, ScaleSettingsNodeViewModel node, CosmosConnection connection, CosmosContainer container)
        {
            //IsLoading = true;

            ContentId = contentId;
            Node = node;
            Title = node.Name;
            Header = node.Name;
            Connection = connection;
            Container = container;

            var split = Container.SelfLink.Split(new char[] { '/' });
            ToolTip = $"{split[1]}>{split[3]}";

            AccentColor = Connection.AccentColor;


            TimeToLiveInSecond = Container.DefaultTimeToLive;
            TimeToLive = Container.DefaultTimeToLive switch
            {
                null => TimeToLiveType.Off,
                -1 => TimeToLiveType.Default,
                _ => TimeToLiveType.On,
            };

            GeoType = Container.GeospatialType;
            IndexingPolicy = Container.IndexingPolicy;

            var service = ActivatorUtilities.CreateInstance<CosmosContainerService>(_serviceProvider, connection, node.Parent.Parent.Database);
            var response = await service.GetThroughputAsync(container);

            if (response is not null)
            {
                Throughput = response.AutoscaleMaxThroughput ?? response.Throughput;
            }

            //IsLoading = false;
        }

    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum TimeToLiveType
    {
        Off = 0,
        [Description("On (Default)")]
        Default = 1,
        On = 2
    }
}
