using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        private CosmosConnection _connection;
        private CosmosContainer _container;
        private CosmosContainerService _cosmosContainerService;

        public MetricsTabViewModel(IServiceProvider serviceProvider, IUIServices uiServices) 
            : base(uiServices)
        {
            Title = "Collection Metrics";
            Header = Title;

            _requestChargeStatusBarItem = new StatusBarItem(new StatusBarItemContext { Value = RequestCharge, IsVisible = IsBusy }, StatusBarItemType.SimpleText, "Request Charge", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_requestChargeStatusBarItem);
            _serviceProvider = serviceProvider;
        }

        public CosmosContainerMetric Metrics { get; set; }

        public string RequestCharge { get; set; }

        public void OnRequestChargeChanged()
        {
            _requestChargeStatusBarItem.DataContext.Value = RequestCharge;
        }

        protected override void OnIsBusyChanged()
        {
            _requestChargeStatusBarItem.DataContext.IsVisible = !IsBusy;

            base.OnIsBusyChanged();
        }

        public override async void Load(string contentId, MetricsNodeViewModel node, CosmosConnection connection, CosmosContainer container)
        {
            ContentId = contentId;
            _connection = connection;
            _container = container;

            _cosmosContainerService = ActivatorUtilities.CreateInstance<CosmosContainerService>(_serviceProvider, connection, node.Parent.Parent.Database);

            var split = _container.SelfLink.Split(new char[] { '/' });
            ToolTip = $"{split[1]}>{split[3]}";
            AccentColor = _connection.AccentColor;

            await LoadMetrics().ConfigureAwait(false);
        }

        public RelayCommand RefreshCommand => new(async () => await LoadMetrics().ConfigureAwait(false), () => !IsBusy);

        private async Task LoadMetrics()
        {
            IsBusy = true;

            try
            {
                var tokenSource = new CancellationTokenSource();

                Metrics = await _cosmosContainerService.GetContainerMetricsAsync(_container, tokenSource.Token).ConfigureAwait(false);
                RequestCharge = $"Request Charge: {Metrics.RequestCharge:N2}";

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
