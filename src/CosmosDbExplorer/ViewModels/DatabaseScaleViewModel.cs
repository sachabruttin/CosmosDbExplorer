using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CosmosDbExplorer.Contracts.Services;
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
    public class DatabaseScaleViewModel : PaneWithZoomViewModel<DatabaseScaleNodeViewModel>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDialogService _dialogService;
        private readonly ISystemService _systemService;
        private readonly ICosmosDatabaseService _cosmosDatabaseService;
        private ICommand? _openUrlCommand;
        private AsyncRelayCommand? _saveCommand;
        private RelayCommand? _discardCommand;
        private CosmosThroughput? _originalThroughput;

        public DatabaseScaleViewModel(IServiceProvider serviceProvider, IUIServices uiServices, IDialogService dialogService, ISystemService systemService, string contentId, NodeContext<DatabaseScaleNodeViewModel> nodeContext)
            : base(uiServices, contentId, nodeContext)
        {
            _serviceProvider = serviceProvider;
            _dialogService = dialogService;
            _systemService = systemService;
            IconSource = App.Current.FindResource("ScaleSettingsIcon");

            if (NodeContext.Node is null)
            {
                throw new NullReferenceException(nameof(NodeContext.Node));
            }

            if (NodeContext.Connection is null)
            {
                throw new NullReferenceException(nameof(NodeContext.Connection));
            }

            if (NodeContext.Database is null)
            {
                throw new NullReferenceException(nameof(NodeContext.Database));
            }

            Node = NodeContext.Node;
            Title = Node.Name;
            Header = Node.Name;
            Connection = NodeContext.Connection;
            Database = NodeContext.Database;

            AccentColor = Connection.AccentColor;
            ToolTip = $"{Connection.Label}/{Database.Id}";

            _cosmosDatabaseService = ActivatorUtilities.CreateInstance<CosmosDatabaseService>(_serviceProvider, Connection);
        }

        public DatabaseScaleNodeViewModel Node { get; private set; }
        public CosmosConnection Connection { get; private set; }
        public CosmosDatabase Database { get; private set; }

        public bool IsThroughputAutoscale { get; set; } = true;

        public int MaxThroughput { get; set; }

        public int? MinThroughput { get; set; }

        [OnChangedMethod(nameof(UpdateCommandStatus))]
        public int? Throughput { get; set; }

        public int Increment => IsThroughputAutoscale ? 1000 : 100;

        [DependsOn(nameof(IsThroughputAutoscale), nameof(Throughput))]
        public string Information => $"{Throughput * 0.1} RU/s (10 % of max RU/s) - {Throughput} RU/s";

        [DependsOn(nameof(IsThroughputAutoscale), nameof(Throughput))]
        public string DataStoredInGb => $"{Throughput * 0.01}";

        public ICommand OpenUrlCommand => _openUrlCommand ??= new RelayCommand<string>(OpenUrl);

        public AsyncRelayCommand SaveCommand => _saveCommand ??= new(SaveCommandExecute, () => HasThroughputChanged);

        public RelayCommand DiscardCommand => _discardCommand ??= new(DiscardCommandExecute, () => HasThroughputChanged);
        private bool HasThroughputChanged => (_originalThroughput?.AutoscaleMaxThroughput ?? _originalThroughput?.Throughput) != Throughput;

        public override async Task InitializeAsync()
        {
            var throughput = await _cosmosDatabaseService.GetThroughputAsync(Database);
            SetThroughputInfo(throughput);
        }

        private void SetThroughputInfo(CosmosThroughput? throughput)
        {
            if (throughput is null)
            {
                throw new ArgumentNullException(nameof(throughput));
            }

            _originalThroughput = throughput;

            MinThroughput = _originalThroughput.MinThroughtput;
            MaxThroughput = int.MaxValue - (int.MaxValue % 1000);
            IsThroughputAutoscale = _originalThroughput.AutoscaleMaxThroughput.HasValue;
            Throughput = _originalThroughput.AutoscaleMaxThroughput ?? _originalThroughput.Throughput;
        }

        private void OpenUrl(string? url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                _systemService.OpenInWebBrowser(url);
            }
        }

        private async Task SaveCommandExecute()
        {
            if (_cosmosDatabaseService is null)
            {
                throw new NullReferenceException("Database service should not be null!");
            }

            if (Database is null)
            {
                throw new NullReferenceException("Database propery should not be null!");
            }

            try
            {
                IsBusy = true;

                if (HasThroughputChanged && Throughput is not null)
                {
                    var throughput = await _cosmosDatabaseService.UpdateThroughputAsync(Database, Throughput.Value, IsThroughputAutoscale);
                    SetThroughputInfo(throughput);
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
        }

        private void UpdateCommandStatus()
        {
            SaveCommand.NotifyCanExecuteChanged();
            DiscardCommand.NotifyCanExecuteChanged();
        }
    }

    public class DatabaseScaleViewModelValidator : AbstractValidator<DatabaseScaleViewModel>
    {
        public DatabaseScaleViewModelValidator()
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
        }
    }
}
