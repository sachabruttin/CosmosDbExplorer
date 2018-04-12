using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CosmosDbExplorer.Infrastructure;
using CosmosDbExplorer.Infrastructure.Extensions;
using CosmosDbExplorer.Infrastructure.Models;
using CosmosDbExplorer.Services;
using CosmosDbExplorer.ViewModel.Indexes;
using FluentValidation;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using PropertyChanged;
using Validar;

namespace CosmosDbExplorer.ViewModel
{
    [InjectValidation]
    public class ScaleAndSettingsTabViewModel : PaneWithZoomViewModel<ScaleSettingsNodeViewModel>
    {
        private readonly IDocumentDbService _dbService;
        private RelayCommand _discardCommand;
        private RelayCommand _saveCommand;
        private bool _onTimeToLive;
        private const decimal HourlyPrice = 0.00008m;
        private readonly IDialogService _dialogService;
        private IDisposable _textChangedObservable;

        public ScaleAndSettingsTabViewModel(IMessenger messenger, IDialogService dialogService, IDocumentDbService dbService, IUIServices uiServices)
            : base(messenger, uiServices)
        {
            Content = new TextDocument();

            _textChangedObservable = Observable.FromEventPattern<EventArgs>(Content, "TextChanged")
                                                  .ObserveOnDispatcher()
                                                  .Select(evt => ((TextDocument)evt.Sender).Text)
                                                  .Throttle(TimeSpan.FromMilliseconds(600))
                                                  .Where(text => !string.IsNullOrEmpty(text))
                                                  .DistinctUntilChanged()
                                                  .SubscribeOnDispatcher()
                                                  .Subscribe(OnContentTextChanged);

            _dbService = dbService;
            _dialogService = dialogService;
        }

        private void OnContentTextChanged(string text)
        {
            DispatcherHelper.RunAsync(() =>
            {
                try
                {
                    IsLoading = true;
                    PolicyViewModel.Policy = JsonConvert.DeserializeObject<IndexingPolicy>(text);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"TextChanged => {ex.Message}");
                }
                finally
                {
                    IsLoading = false;
                }
            });
        }

        public override void Cleanup()
        {
            _textChangedObservable.Dispose();
            base.Cleanup();
        }

        [DoNotSetChanged]
        public bool IsLoading { get; set; }

        public override void Load(string contentId, ScaleSettingsNodeViewModel node, Connection connection, DocumentCollection collection)
        {
            IsLoading = true;

            ContentId = contentId;
            Node = node;
            Title = node.Name;
            Header = node.Name;
            Connection = connection;
            Collection = collection;

            var split = Collection.AltLink.Split(new char[] { '/' });
            ToolTip = $"{split[1]}>{split[3]}";

            AccentColor = Connection.AccentColor;

            SetInformation();

            IsLoading = false;
        }

        public ScaleSettingsNodeViewModel Node { get; protected set; }

        private void SetInformation()
        {
            TimeToLiveInSecond = Collection.DefaultTimeToLive;
            OffTimeToLive = Collection.DefaultTimeToLive == null;
            NoDefaultTimeToLive = Collection.DefaultTimeToLive == -1;
            OnTimeToLive = Collection.DefaultTimeToLive >= 0;
            PartitionKey = Collection.PartitionKey?.Paths.FirstOrDefault();
            IsFixedStorage = PartitionKey == null;

            PolicyViewModel = new IndexingPolicyViewModel(Collection.IndexingPolicy);
            PolicyViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PolicyViewModel.IsValid))
                {
                    return;
                }

                if (!IsLoading && PolicyViewModel.IsValid)
                {
                    Content.Text = JsonConvert.SerializeObject(PolicyViewModel.Policy, Formatting.Indented);
                }
            };

            Content.Text = JsonConvert.SerializeObject(PolicyViewModel.Policy, Formatting.Indented);

            IsChanged = false;
        }

        public IndexingPolicyViewModel PolicyViewModel { get; protected set; }

        public Connection Connection { get; protected set; }

        public DocumentCollection Collection { get; protected set; }

        public int Throughput { get; set; }

        public string PartitionKey { get; set; }

        public bool IsFixedStorage { get; set; }

        public int PartitionCount { get; set; }

        public int MaxThroughput => PartitionCount * 10000;

        public int MinThroughput => IsFixedStorage ? 400 : Math.Max(1000, PartitionCount * 100);

        public string EstimatedPrice => $"${HourlyPrice * Throughput:N3} hourly / {HourlyPrice * Throughput * 24:N2} daily.";

        public int? TimeToLiveInSecond { get; set; }

        public TextDocument Content { get; set; }

        public bool IsChanged { get; set; }

        public bool OffTimeToLive { get; set; }

        public bool NoDefaultTimeToLive { get; set; }

        public bool OnTimeToLive
        {
            get { return _onTimeToLive; }
            set
            {
                _onTimeToLive = value;
                if (_onTimeToLive && TimeToLiveInSecond.GetValueOrDefault(-1) == -1)
                {
                    TimeToLiveInSecond = 1;
                    RaisePropertyChanged(() => OnTimeToLive);
                }
            }
        }

        public async Task LoadDataAsync()
        {
            IsLoading = true;

            var throughputTask = _dbService.GetThroughputAsync(Connection, Collection);
            var partitionTask = _dbService.GetPartitionKeyRangeCountAsync(Connection, Collection);

            var result = await Task.WhenAll(throughputTask, partitionTask).ConfigureAwait(false);

            PartitionCount = result[1];
            Throughput = result[0];

            IsLoading = false;
            IsChanged = false;
        }

        public RelayCommand DiscardCommand
        {
            get
            {
                return _discardCommand
                    ?? (_discardCommand = new RelayCommand(
                        async () =>
                        {
                            SetInformation();
                            await LoadDataAsync().ConfigureAwait(false);
                            IsChanged = false;
                        },
                        () => IsDirty));
            }
        }

        public RelayCommand SaveCommand
        {
            get
            {
                return _saveCommand
                    ?? (_saveCommand = new RelayCommand(
                        async () =>
                        {
                            try
                            {
                                Collection.DefaultTimeToLive = GetTimeToLive();
                                Collection.IndexingPolicy = JsonConvert.DeserializeObject<IndexingPolicy>(Content.Text);

                                await _dbService.UpdateCollectionSettingsAsync(Connection, Collection, Throughput).ConfigureAwait(false);
                                IsChanged = false;
                            }
                            catch (OperationCanceledException)
                            {
                                await _dialogService.ShowMessage("Operation cancelled by user...", "Cancel").ConfigureAwait(false);
                            }
                            catch (DocumentClientException clientEx)
                            {
                                await _dialogService.ShowError(clientEx.Parse(), "Error", "ok", null).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                await _dialogService.ShowError(ex, "Error", "ok", null).ConfigureAwait(false);
                            }
                        },
                        () => IsDirty && IsValid));
            }
        }

        public bool IsDirty
        {
            get
            {
                return IsChanged || PolicyViewModel?.IsChanged == true;
            }
        }

        public bool IsValid
        {
            get
            {
                return !((INotifyDataErrorInfo)this).HasErrors && (PolicyViewModel?.IsValid == true);
            }
        }

        private int? GetTimeToLive()
        {
            if (OffTimeToLive)
            {
                return null;
            }
            else if (OnTimeToLive)
            {
                return TimeToLiveInSecond;
            }
            else
            {
                return -1;
            }
        }
    }

    public class ScaleAndSettingsTabViewModelValidator : AbstractValidator<ScaleAndSettingsTabViewModel>
    {
        public ScaleAndSettingsTabViewModelValidator()
        {
            RuleFor(x => x.Throughput).NotEmpty()
                                      .Must(throughput => throughput % 100 == 0)
                                      .WithMessage("Throughput must be a multiple of 100");

            RuleFor(x => x.Content)
                .Custom((content, ctx) =>
                {
                    DispatcherHelper.RunAsync(() =>
                    {
                        try
                        {
                            JsonConvert.DeserializeObject<IndexingPolicy>(content.Text);
                        }
                        catch (Exception ex)
                        {
                            ctx.AddFailure(ex.Message);
                        }
                    });
                });
        }
    }
}
