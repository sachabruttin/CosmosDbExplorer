using System;
using System.Linq;
using System.Threading.Tasks;
using CosmosDbExplorer.Infrastructure;
using CosmosDbExplorer.Infrastructure.Models;
using CosmosDbExplorer.Services;
using CosmosDbExplorer.ViewModel.Interfaces;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using Microsoft.Azure.Documents;

namespace CosmosDbExplorer.ViewModel
{
    public class CollectionMetricsTabViewModel : PaneViewModel<CollectionMetricsNodeViewModel>, ICanRefreshTab
    {
        private readonly IDocumentDbService _dbService;
        private DocumentCollection _collection;
        private string _partitionKey;
        private Connection _connection;
        private RelayCommand _refreshCommand;

        public CollectionMetricsTabViewModel(IMessenger messenger, IUIServices uiServices, IDocumentDbService dbService)
            : base(messenger, uiServices)
        {
            _dbService = dbService;
            Title = "Collection Metrics";
            Header = Title;

            // Chart configuratin
            var wrapper = Mappers.Xy<PartitionKeyRangeStatistics>()
                .X((value, index) => index)
                .Y(value => value.SizeInKB);

            Formatter = value => $"{Math.Round(value / (1024 * 1024.0), 3)} GiB";

            Charting.For<PartitionKeyRangeStatistics>(wrapper);
        }

        public override async void Load(string contentId, CollectionMetricsNodeViewModel node, Connection connection, DocumentCollection collection)
        {
            ContentId = contentId;
            _connection = connection;
            _collection = collection;
            _partitionKey = collection.PartitionKey?.Paths.FirstOrDefault();
            var split = _collection.AltLink.Split(new char[] { '/' });
            ToolTip = $"{split[1]}>{split[3]}";
            AccentColor = _connection.AccentColor;

            await LoadMetrics().ConfigureAwait(false);
        }

        public int PartitionCount { get; set; }

        public long DocumentSize { get; set; }

        public long DocumentCount { get; set; }

        public SeriesCollection PartitionSizeSeries { get; set; }

        public string[] Labels { get; set; }

        public Func<double, string> Formatter { get; set; }

        public RelayCommand RefreshCommand
        {
            get
            {
                return _refreshCommand
                    ?? (_refreshCommand = new RelayCommand(
                        async () => await LoadMetrics().ConfigureAwait(false),
                        () => !IsBusy));
            }
        }

        private async Task LoadMetrics()
        {
            IsBusy = true;

            var metrics = await _dbService.GetPartitionMetricsAsync(_connection, _collection).ConfigureAwait(false);

            PartitionCount = metrics.PartitionCount;
            DocumentSize = metrics.DocumentSize;
            DocumentCount = metrics.DocumentCount;

            await DispatcherHelper.RunAsync(() =>
            {
                Labels = metrics.PartitionMetrics.OrderBy(pm => pm.PartitionKeyRangeId).Select(pm => pm.PartitionKeyRangeId).ToArray();
                PartitionSizeSeries = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Size",
                        Values = new ChartValues<PartitionKeyRangeStatistics>(metrics.PartitionMetrics.OrderBy(pm => pm.PartitionKeyRangeId))
                    }
                };
            });

            IsBusy = false;
        }
    }
}
