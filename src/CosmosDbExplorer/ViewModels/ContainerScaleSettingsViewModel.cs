using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Core;
using CosmosDbExplorer.Core.Contracts.Services;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using CosmosDbExplorer.ViewModels.DatabaseNodes;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;

using PropertyChanged;

using Validar;

namespace CosmosDbExplorer.ViewModels
{
    public class ContainerScaleSettingsViewModel : PaneWithZoomViewModel<ScaleSettingsNodeViewModel>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDialogService _dialogService;
        private ICosmosContainerService _containerService;
        private AsyncRelayCommand _saveCommand;
        private RelayCommand _discardCommand;
        private CosmosThroughput _originalThroughput;

        public ContainerScaleSettingsViewModel(IServiceProvider serviceProvider, IUIServices uiServices, IDialogService dialogService)
            : base(uiServices)
        {
            _serviceProvider = serviceProvider;
            _dialogService = dialogService;
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
        public bool IsThroughputAutoscale { get; set; } = true;

        public int MaxThroughput { get; set; }

        public int? MinThroughput { get; set; }

        [OnChangedMethod(nameof(UpdateCommandStatus))]
        public int? Throughput { get; set; }

        public int Increment => IsThroughputAutoscale ? 1000 : 100;

        public AsyncRelayCommand SaveCommand => _saveCommand ??= new(SaveCommandExecute, () => HasThroughputChanged || HasIndexingPolicyChanged || HasSettingsChanged);

        public RelayCommand DiscardCommand => _discardCommand ??= new(DiscardCommandExecute, () => HasThroughputChanged || HasIndexingPolicyChanged || HasSettingsChanged);

        private bool HasThroughputChanged => (_originalThroughput?.AutoscaleMaxThroughput ?? _originalThroughput?.Throughput) != Throughput;
        private bool HasSettingsChanged => true; // TODO: Check
        private bool HasIndexingPolicyChanged => true; // TODO check;

        public override async void Load(string contentId, ScaleSettingsNodeViewModel node, CosmosConnection connection, CosmosContainer container)
        {
            //IsLoading = true;

            ContentId = contentId;
            Node = node;
            Title = node.Name;
            Header = node.Name;
            Connection = connection;
            Container = container;

            //var split = Container.SelfLink.Split(new char[] { '/' });
            //ToolTip = $"{split[1]}>{split[3]}";
            ToolTip = $"{Connection.Label}/{node.Parent.Parent.Database.Id}/{Container.Id}";

            AccentColor = Connection.AccentColor;


            SetSettings();

            _containerService = ActivatorUtilities.CreateInstance<CosmosContainerService>(_serviceProvider, connection, node.Parent.Parent.Database);
            var response = await _containerService.GetThroughputAsync(container);

            if (response is not null)
            {
                SetThroughputInfo(response);
            }

            //IsLoading = false;
        }

        private void SetSettings()
        {
            TimeToLiveInSecond = Container.DefaultTimeToLive;
            TimeToLive = TimeToLiveTypeExtensions.Get(Container.DefaultTimeToLive);
            GeoType = Container.GeospatialType;
            IndexingPolicy = Container.IndexingPolicy;
        }

        private void SetThroughputInfo(CosmosThroughput throughput)
        {
            _originalThroughput = throughput;

            MinThroughput = _originalThroughput.MinThroughtput;
            MaxThroughput = int.MaxValue - (int.MaxValue % 1000);
            IsThroughputAutoscale = _originalThroughput.AutoscaleMaxThroughput.HasValue;
            Throughput = _originalThroughput.AutoscaleMaxThroughput ?? _originalThroughput.Throughput;
        }

        private async Task SaveCommandExecute()
        {
            try
            {
                IsBusy = true;

                if (HasThroughputChanged && Throughput is not null)
                {
                    var throughput = await _containerService.UpdateThroughputAsync(Container, Throughput.Value, IsThroughputAutoscale);
                    SetThroughputInfo(throughput);
                }

                if (HasSettingsChanged || HasIndexingPolicyChanged)
                {
                    var container = new CosmosContainer(Container.Id, Container.IsLargePartitionKey.GetValueOrDefault())
                    {
                        DefaultTimeToLive = TimeToLiveTypeExtensions.ToCosmosValue(TimeToLive, TimeToLiveInSecond),
                        GeospatialType = GeoType,
                        IndexingPolicy = IndexingPolicy ?? string.Empty,
                        PartitionKeyPath = Container.PartitionKeyPath // Cannot be changed!
                    };

                    var response = await _containerService.UpdateContainerAsync(container);
                    // TODO:Send message
                    Container = response;
                    SetSettings();
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowError(ex, "An unexpected error occured!");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void DiscardCommandExecute()
        {
            SetThroughputInfo(_originalThroughput);
            SetSettings();
        }

        private void UpdateCommandStatus()
        {
            SaveCommand.NotifyCanExecuteChanged();
            DiscardCommand.NotifyCanExecuteChanged();
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

    public static class TimeToLiveTypeExtensions
    {
        public static TimeToLiveType Get(int? ttl)
        {
            return ttl switch
            {
                null => TimeToLiveType.Off,
                -1 => TimeToLiveType.Default,
                _ => TimeToLiveType.On,
            };
        }

        public static int? ToCosmosValue(this TimeToLiveType? value, int? timeToLiveInSecs)
        {
            return value switch
            {
                TimeToLiveType.Off => null,
                TimeToLiveType.Default => -1,
                _ => timeToLiveInSecs
            };
        }
    }
        

}
