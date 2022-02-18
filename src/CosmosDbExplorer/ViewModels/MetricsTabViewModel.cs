using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using CosmosDbExplorer.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;

namespace CosmosDbExplorer.ViewModels
{
    public class MetricsTabViewModel : PaneViewModel<MetricsNodeViewModel>, ICanRefreshTab
    {
        private readonly StatusBarItem _requestChargeStatusBarItem;
        private readonly IServiceProvider _serviceProvider;
        private CosmosConnection? _connection;
        private CosmosContainer? _container;
        private CosmosContainerService? _cosmosContainerService;
        private AsyncRelayCommand? _refreshCommand;

        public MetricsTabViewModel(IServiceProvider serviceProvider, IUIServices uiServices) 
            : base(uiServices)
        {
            Title = "Collection Metrics";
            Header = Title;

            _requestChargeStatusBarItem = new StatusBarItem(new StatusBarItemContext { Value = RequestCharge, IsVisible = IsBusy }, StatusBarItemType.SimpleText, "Request Charge", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_requestChargeStatusBarItem);
            _serviceProvider = serviceProvider;
        }

        public CosmosContainerMetric? Metrics { get; private set; }

        public string? RequestCharge { get; private set; }

        public void OnRequestChargeChanged()
        {
            _requestChargeStatusBarItem.DataContext.Value = RequestCharge;
        }

        protected override void OnIsBusyChanged()
        {
            _requestChargeStatusBarItem.DataContext.IsVisible = !IsBusy;

            base.OnIsBusyChanged();
        }

        public override async void Load(string contentId, MetricsNodeViewModel? node, CosmosConnection? connection, CosmosDatabase? database, CosmosContainer? container)
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (database is null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            ContentId = contentId;
            _connection = connection;
            _container = container;

            _cosmosContainerService = ActivatorUtilities.CreateInstance<CosmosContainerService>(_serviceProvider, connection, database);

            ToolTip = $"{connection.Label}/{database.Id}/{container.Id}";
            AccentColor = _connection.AccentColor;

            await LoadMetrics();
        }

        public ICommand RefreshCommand => _refreshCommand ??= new AsyncRelayCommand(LoadMetrics, () => !IsBusy);

        private async Task LoadMetrics()
        {
            if (_container is null)
            {
                throw new NullReferenceException(nameof(_container));
            }

            if (_cosmosContainerService is null)
            {
                throw new NullReferenceException(nameof(_cosmosContainerService));
            }

            IsBusy = true;

            try
            {
                var tokenSource = new CancellationTokenSource();

                Metrics = await _cosmosContainerService.GetContainerMetricsAsync(_container, tokenSource.Token).ConfigureAwait(false);
                RequestCharge = $"Request Charge: {Metrics.RequestCharge:N2}";

                OnPropertyChanged(nameof(Metrics));

                //await DispatcherHelper.RunAsync(() =>
                //{
                //    var sorted = Metrics.PartitionMetrics.OrderBy(pm => int.Parse(pm.PartitionKeyRangeId)).ToArray();
                //    Labels = sorted.Select(pm => pm.PartitionKeyRangeId).ToArray();
                //    PartitionSizeSeries = new SeriesCollection
                //    {
                //        new ColumnSeries
                //        {
                //            Title = "Size",
                //            Values = new ChartValues<PartitionKeyRangeStatistics>(sorted)
                //        }
                //    };
                //});
            }
            //catch (DocumentClientException clientEx)
            //{
            //    await _dialogService.ShowError(clientEx.Parse(), "Error", "ok", null).ConfigureAwait(false);
            //}
            //catch (Exception ex)
            //{
            //    await _dialogService.ShowError(ex, "Error", "ok", null).ConfigureAwait(false);
            //}
            finally
            {
                IsBusy = false;
            }
        }
    }
}
