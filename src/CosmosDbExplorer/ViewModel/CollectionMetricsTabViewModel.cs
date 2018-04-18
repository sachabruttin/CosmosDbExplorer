﻿using System;
using System.Linq;
using System.Threading.Tasks;
using CosmosDbExplorer.Infrastructure;
using CosmosDbExplorer.Infrastructure.Extensions;
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
        private readonly IDialogService _dialogService;
        private readonly StatusBarItem _requestChargeStatusBarItem;

        public CollectionMetricsTabViewModel(IMessenger messenger, IUIServices uiServices, 
            IDialogService dialogService,
            IDocumentDbService dbService)
            : base(messenger, uiServices)
        {
            _dbService = dbService;
            _dialogService = dialogService;

            Title = "Collection Metrics";
            Header = Title;

            _requestChargeStatusBarItem = new StatusBarItem(new StatusBarItemContext { Value = RequestCharge, IsVisible = IsBusy }, StatusBarItemType.SimpleText, "Request Charge", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_requestChargeStatusBarItem);

            ChartConfiguration();
        }

        private void ChartConfiguration()
        {
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

        public CollectionMetric Metrics { get; set; }

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

            try
            {
                Metrics = await _dbService.GetPartitionMetricsAsync(_connection, _collection).ConfigureAwait(false);
                RequestCharge = $"Request Charge: {Metrics.RequestCharge:N2}";

                await DispatcherHelper.RunAsync(() =>
                {
                    var sorted = Metrics.PartitionMetrics.OrderBy(pm => int.Parse(pm.PartitionKeyRangeId)).ToArray();
                    Labels = sorted.Select(pm => pm.PartitionKeyRangeId).ToArray();
                    PartitionSizeSeries = new SeriesCollection
                    {
                        new ColumnSeries
                        {
                            Title = "Size",
                            Values = new ChartValues<PartitionKeyRangeStatistics>(sorted)
                        }
                    };
                });
            }
            catch (DocumentClientException clientEx)
            {
                await _dialogService.ShowError(clientEx.Parse(), "Error", "ok", null).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowError(ex, "Error", "ok", null).ConfigureAwait(false);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
