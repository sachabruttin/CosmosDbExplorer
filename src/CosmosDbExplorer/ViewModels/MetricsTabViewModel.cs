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
        private readonly CosmosConnection _connection;
        private readonly CosmosContainer _container;
        private readonly CosmosContainerService _cosmosContainerService;
        private AsyncRelayCommand? _refreshCommand;

        public MetricsTabViewModel(IServiceProvider serviceProvider, IUIServices uiServices, string contentId, NodeContext<MetricsNodeViewModel> nodeContext)
            : base(uiServices, contentId, nodeContext)
        {
            Title = "Collection Metrics";
            Header = Title;

            _requestChargeStatusBarItem = new StatusBarItem(new StatusBarItemContext { Value = RequestCharge, IsVisible = IsBusy }, StatusBarItemType.SimpleText, "Request Charge", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_requestChargeStatusBarItem);

            if (nodeContext.Connection is null || nodeContext.Container is null || nodeContext.Database is null)
            {
                throw new NullReferenceException("Node context is not correctly initialized!");
            }

            _connection = nodeContext.Connection;
            _container = nodeContext.Container;

            _cosmosContainerService = ActivatorUtilities.CreateInstance<CosmosContainerService>(serviceProvider, nodeContext.Connection, nodeContext.Database);

            ToolTip = $"{nodeContext.Connection.Label}/{nodeContext.Database.Id}/{nodeContext.Container.Id}";
            AccentColor = _connection.AccentColor;
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

        public override Task InitializeAsync()
        {
            return LoadMetrics();
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

                Metrics = await _cosmosContainerService.GetContainerMetricsAsync(_container, tokenSource.Token);
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
