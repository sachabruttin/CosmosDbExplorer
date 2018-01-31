using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Services;
using FluentValidation;
using GalaSoft.MvvmLight.Messaging;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using Validar;

namespace DocumentDbExplorer.ViewModel
{
    [InjectValidation]
    public class ScaleAndSettingsTabViewModel : PaneWithZoomViewModel
    {
        private ScaleSettingsNodeViewModel _node;
        private readonly IDocumentDbService _dbService;
        private RelayCommand _discardCommand;
        private RelayCommand _saveCommand;
        private bool _onTimeToLive;

        public ScaleAndSettingsTabViewModel(IMessenger messenger, IDocumentDbService dbService) : base(messenger)
        {
            Content = new TextDocument();
            _dbService = dbService;
            PropertyChanged += OnPropetyChanged;
        }

        private void OnPropetyChanged(object sender, PropertyChangedEventArgs e)
        {
            var names = new[] { "Throughput", "TimeToLiveInSecond", "OffTimeToLive", "NoDefaultTimeToLive", "OnTimeToLive" };

            if (!IsLoading && names.Contains(e.PropertyName))
            {
                IsDirty = true;
            }
        }

        public bool IsLoading { get; set; }

        public ScaleSettingsNodeViewModel Node
        {
            get { return _node; }
            set
            {
                IsLoading = true;

                if (_node != value)
                {
                    _node = value;
                    Title = value.Name;
                    Header = value.Name;
                    Connection = value.Parent.Parent.Parent.Connection;
                    Collection = value.Parent.Collection;

                    var split = Collection.AltLink.Split(new char[] { '/' });
                    ToolTip = $"{split[1]}>{split[3]}>{Title}";

                    SetInformation();
                }

                IsLoading = false;
            }
        }

        private void SetInformation()
        {
            TimeToLiveInSecond = Collection.DefaultTimeToLive;
            OffTimeToLive = Collection.DefaultTimeToLive == null;
            NoDefaultTimeToLive = Collection.DefaultTimeToLive == -1;
            OnTimeToLive = Collection.DefaultTimeToLive >= 0;
            PartitionKey = Collection.PartitionKey?.Paths.FirstOrDefault();
            IsFixedStorage = PartitionKey == null;

            Content = new TextDocument(JsonConvert.SerializeObject(Collection.IndexingPolicy, Formatting.Indented));
        }

        public Connection Connection { get; protected set; }

        public DocumentCollection Collection { get; protected set; }

        public int Throughput { get; set; }

        public string PartitionKey { get; set; }

        public bool IsFixedStorage { get; set; }

        public int MaxThroughput => IsFixedStorage ? 10000 : 50000;

        public int MinThroughput => IsFixedStorage ? 400 : 1000;

        public int? TimeToLiveInSecond { get; set; }

        public TextDocument Content { get; set; }

        public bool IsDirty { get; set; }

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
            Throughput = await _dbService.GetThroughput(Connection, Collection);
            IsLoading = false;
        }

        public RelayCommand DiscardCommand
        {
            get
            {
                return _discardCommand
                    ?? (_discardCommand = new RelayCommand(
                        async x =>
                        {
                            SetInformation();
                            await LoadDataAsync();
                            IsDirty = false;
                        },
                        x => IsDirty));
            }
        }

        public RelayCommand SaveCommand
        {
            get
            {
                return _saveCommand
                    ?? (_saveCommand = new RelayCommand(
                        async x =>
                        {
                            Collection.DefaultTimeToLive = GetTimeToLive();

                            await _dbService.UpdateCollectionSettings(Connection, Collection, Throughput);
                            IsDirty = false;
                        },
                        x => !((INotifyDataErrorInfo)this).HasErrors));
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
        }
    }
}
