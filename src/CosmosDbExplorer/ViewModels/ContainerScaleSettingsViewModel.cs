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
using CosmosDbExplorer.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

using FluentValidation;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;

using PropertyChanged;

using Validar;

namespace CosmosDbExplorer.ViewModels
{
    [InjectValidation]
    public class ContainerScaleSettingsViewModel : PaneWithZoomViewModel<ScaleSettingsNodeViewModel>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDialogService _dialogService;
        private readonly ISystemService _systemService;
        private ICosmosContainerService? _containerService;
        private AsyncRelayCommand? _saveCommand;
        private RelayCommand? _discardCommand;
        private RelayCommand<string>? _openUrlCommand;
        private CosmosThroughput? _originalThroughput;

        public ContainerScaleSettingsViewModel(IServiceProvider serviceProvider, IUIServices uiServices, IDialogService dialogService, ISystemService systemService)
            : base(uiServices)
        {
            _serviceProvider = serviceProvider;
            _dialogService = dialogService;
            _systemService = systemService;
            IconSource = App.Current.FindResource("ScaleSettingsIcon");
        }

        public ScaleSettingsNodeViewModel? Node { get; private set; }
        public CosmosConnection? Connection { get; private set; }
        public CosmosContainer? Container { get; private set; }

        public bool? IsTimeLiveInSecondVisible => TimeToLive == TimeToLiveType.On;

        [OnChangedMethod(nameof(UpdateCommandStatus))]
        public int? TimeToLiveInSecond { get; set; }

        public TimeToLiveType? TimeToLive { get; set; }

        protected void OnTimeToLiveChanged()
        {
            TimeToLiveInSecond = TimeToLive switch
            {
                TimeToLiveType.On => TimeToLiveInSecond,
                TimeToLiveType.Default => -1,
                _ => null,
            };

            UpdateCommandStatus();
        }

        [OnChangedMethod(nameof(UpdateCommandStatus))]
        public CosmosGeospatialType GeoType { get; set; }

        [OnChangedMethod(nameof(UpdateCommandStatus))]
        public string? IndexingPolicy { get; set; }

        public bool IsIndexingPolicyChanged { get; set; }

        public bool IsThroughputAutoscale { get; set; } = true;

        public int MaxThroughput { get; set; }

        public int? MinThroughput { get; set; }

        public bool IsFixedStorage { get; set; }

        public void OnIsFixedStorageChanged()
        {
            if (Throughput > 10000)
            {
                Throughput = 10000;
            }
        }

        public bool IsUnlimitedStorage { get; set; }

        public void OnIsUnlimitedStorageChanged()
        {
            if (Throughput < 1000)
            {
                Throughput = 1000;
            }
        }

        [OnChangedMethod(nameof(UpdateCommandStatus))]
        public int? Throughput { get; set; }

        public int Increment => IsThroughputAutoscale ? 1000 : 100;

        [DependsOn(nameof(IsThroughputAutoscale), nameof(Throughput))]
        public string Information => $"{Throughput * 0.1} RU/s (10 % of max RU/s) - {Throughput} RU/s";

        [DependsOn(nameof(IsThroughputAutoscale), nameof(Throughput))]
        public string DataStoredInGb => $"{Throughput * 0.01}";

        public AsyncRelayCommand SaveCommand => _saveCommand ??= new(SaveCommandExecute, () => HasThroughputChanged || HasIndexingPolicyChanged.GetValueOrDefault(false) || HasSettingsChanged);

        public RelayCommand DiscardCommand => _discardCommand ??= new(DiscardCommandExecute, () => HasThroughputChanged || HasIndexingPolicyChanged.GetValueOrDefault(false) || HasSettingsChanged);

        public RelayCommand<string> OpenUrlCommand => _openUrlCommand ??= new RelayCommand<string>(OpenUrl);

        private bool HasThroughputChanged => (_originalThroughput?.AutoscaleMaxThroughput ?? _originalThroughput?.Throughput) != Throughput;
        private bool HasSettingsChanged => (Container?.DefaultTimeToLive != TimeToLiveInSecond) || (Container?.GeospatialType != GeoType);
        private bool? HasIndexingPolicyChanged => !Container?.IndexingPolicy?.Equals(IndexingPolicy);

        public override async void Load(string contentId, NodeContext<ScaleSettingsNodeViewModel> nodeContext)
        {
            if (nodeContext.Node is null)
            {
                throw new NullReferenceException(nameof(nodeContext.Node));
            }

            if (nodeContext.Connection is null)
            {
                throw new NullReferenceException(nameof(nodeContext.Connection));
            }

            if (nodeContext.Database is null)
            {
                throw new NullReferenceException(nameof(nodeContext.Database));
            }

            if (nodeContext.Container is null)
            {
                throw new NullReferenceException(nameof(nodeContext.Container));
            }

            //IsLoading = true;

            ContentId = contentId;
            Node = nodeContext.Node;
            Title = Node.Name;
            Header = Node.Name;
            Connection = nodeContext.Connection;
            Container = nodeContext.Container;

            //var split = Container.SelfLink.Split(new char[] { '/' });
            //ToolTip = $"{split[1]}>{split[3]}";
            ToolTip = $"{Connection.Label}/{nodeContext.Database.Id}/{Container.Id}";

            AccentColor = Connection.AccentColor;


            SetSettings();

            _containerService = ActivatorUtilities.CreateInstance<CosmosContainerService>(_serviceProvider, nodeContext.Connection, nodeContext.Database);

            if (!nodeContext.Database.IsServerless)
            {
                var response = await _containerService.GetThroughputAsync(Container);
                SetThroughputInfo(response);
            }

            //IsLoading = false;
        }

        private void SetSettings()
        {
            TimeToLiveInSecond = Container?.DefaultTimeToLive;
            TimeToLive = TimeToLiveTypeExtensions.Get(Container?.DefaultTimeToLive);
            GeoType = Container?.GeospatialType ?? CosmosGeospatialType.Geography;
            IndexingPolicy = Container?.IndexingPolicy;
        }

        private void SetThroughputInfo(CosmosThroughput? throughput)
        {
            if (throughput is not null)
            {
                _originalThroughput = throughput;

                MinThroughput = _originalThroughput.MinThroughtput;
                MaxThroughput = int.MaxValue - (int.MaxValue % 1000);
                IsThroughputAutoscale = _originalThroughput.AutoscaleMaxThroughput.HasValue;
                Throughput = _originalThroughput.AutoscaleMaxThroughput ?? _originalThroughput.Throughput;
            }
        }

        private async Task SaveCommandExecute()
        {
            try
            {
                if (Container == null)
                {
                    throw new Exception("Container not defined!");
                }

                IsBusy = true;

                if (HasThroughputChanged && Throughput is not null)
                {
                    var throughput = await _containerService.UpdateThroughputAsync(Container, Throughput.Value, IsThroughputAutoscale);
                    SetThroughputInfo(throughput);
                }

                if (HasSettingsChanged || HasIndexingPolicyChanged.GetValueOrDefault(false))
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

        private void OpenUrl(string? url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                _systemService.OpenInWebBrowser(url);
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

    public class ContainerScaleSettingsViewModelValidator : AbstractValidator<ContainerScaleSettingsViewModel>
    {
        public ContainerScaleSettingsViewModelValidator()
        {
            RuleFor(x => x.Throughput)
                .NotEmpty()
                .GreaterThanOrEqualTo(x => x.MinThroughput)
                .LessThanOrEqualTo(x => x.MaxThroughput)
                .Custom((throughput, context) =>
                {
                    if (throughput % context.InstanceToValidate.Increment != 0)
                    {
                        context.AddFailure($"Value must be a multiple of {context.InstanceToValidate.Increment}.");
                    }
                });

            RuleFor(x => x.TimeToLiveInSecond)
                .GreaterThan(0).NotEmpty().When(x => x.TimeToLive == TimeToLiveType.On)
                .Equal(0).When(x => x.TimeToLive == TimeToLiveType.Default)
                .Equal(-1).When(x => x.TimeToLive == TimeToLiveType.Off);
        }
    }
}
