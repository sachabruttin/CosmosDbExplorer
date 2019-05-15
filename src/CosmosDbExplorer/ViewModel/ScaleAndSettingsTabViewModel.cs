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
        private readonly IDialogService _dialogService;
        private readonly IDisposable _textChangedObservable;

        public ScaleAndSettingsTabViewModel(IMessenger messenger, IDialogService dialogService, IDocumentDbService dbService, IUIServices uiServices, ThroughputViewModel throughput)
            : base(messenger, uiServices)
        {
            Content = new TextDocument();

            _textChangedObservable = Observable.FromEventPattern<EventArgs>(Content, nameof(Content.TextChanged))
                                                  .Select(evt => ((TextDocument)evt.Sender).Text)
                                                  .Throttle(TimeSpan.FromMilliseconds(600))
                                                  .Where(text => !string.IsNullOrEmpty(text))
                                                  .DistinctUntilChanged()
                                                  .Subscribe(OnContentTextChanged);

            _dbService = dbService;
            _dialogService = dialogService;
            Throughput = throughput;
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
            IsDatabaseLevelThroughput = node.Parent.IsDatabaseLevelThroughput;

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
            TimeToLive = Collection.DefaultTimeToLive == null
                                ? TimeToLive.Off
                                : Collection.DefaultTimeToLive == -1 ? TimeToLive.Default : TimeToLive.On;

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

        public ThroughputViewModel Throughput { get; private set; }

        public string PartitionKey { get; set; }

        public bool IsFixedStorage { get; set; }

        public int PartitionCount { get; set; }

        public int? TimeToLiveInSecond { get; set; }

        public TextDocument Content { get; set; }

        public bool IsChanged { get; set; }

        public TimeToLive TimeToLive { get; set; }

        public void OnTimeToLiveChanged()
        {
            switch (TimeToLive)
            {
                case TimeToLive.Off:
                    TimeToLiveInSecond = null;
                    IsTimeLiveInSecondVisible = false;
                    break;
                case TimeToLive.On:
                    TimeToLiveInSecond = TimeToLiveInSecond.GetValueOrDefault(-1) == -1 ? 1 : TimeToLiveInSecond;
                    IsTimeLiveInSecondVisible = true;
                    break;
                case TimeToLive.Default:
                    TimeToLiveInSecond = -1;
                    IsTimeLiveInSecondVisible = false;
                    break;
            }
        }

        public bool IsTimeLiveInSecondVisible { get; set; }

        public bool IsDatabaseLevelThroughput { get; private set; }

        public async Task LoadDataAsync()
        {
            IsLoading = true;

            try
            {
                if (!IsDatabaseLevelThroughput)
                {
                    var throughputTask = await _dbService.GetThroughputAsync(Connection, Collection);
                    Throughput.Value = throughputTask.Value; // result[0];
                }

                var partitionTask = await _dbService.GetPartitionKeyRangeCountAsync(Connection, Collection);
                PartitionCount = partitionTask;// result[1];
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
                IsLoading = false;
                IsChanged = false;
            }
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
                                Collection.DefaultTimeToLive = TimeToLiveInSecond;
                                Collection.IndexingPolicy = JsonConvert.DeserializeObject<IndexingPolicy>(Content.Text);

                                await _dbService.UpdateCollectionSettingsAsync(Connection, Collection, Throughput.Value).ConfigureAwait(false);
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
    }

    public enum TimeToLive
    {
        Off,
        On,
        Default
    }

    public class ScaleAndSettingsTabViewModelValidator : AbstractValidator<ScaleAndSettingsTabViewModel>
    {
        public ScaleAndSettingsTabViewModelValidator()
        {
            RuleFor(x => x.Throughput).SetValidator(new ThroughputViewModelValidator());

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
