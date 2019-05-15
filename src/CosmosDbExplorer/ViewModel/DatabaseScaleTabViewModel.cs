using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CosmosDbExplorer.Infrastructure;
using CosmosDbExplorer.Infrastructure.Extensions;
using CosmosDbExplorer.Infrastructure.Models;
using CosmosDbExplorer.Services;
using FluentValidation;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Azure.Documents;
using PropertyChanged;
using Validar;

namespace CosmosDbExplorer.ViewModel
{
    [InjectValidation]
    public class DatabaseScaleTabViewModel : PaneWithZoomViewModel<DatabaseScaleNodeViewModel>
    {
        private readonly IMessenger _messenger;
        private readonly IDialogService _dialogService;
        private readonly IDocumentDbService _dbService;
        private readonly IUIServices _uiServices;

        private RelayCommand _discardCommand;
        private RelayCommand _saveCommand;

        private Database _database;
        private Connection _connection;

        public DatabaseScaleTabViewModel(IMessenger messenger, IDialogService dialogService, IDocumentDbService dbService, IUIServices uiServices, ThroughputViewModel throughput)
            : base(messenger, uiServices)
        {
            _messenger = messenger;
            _dialogService = dialogService;
            _dbService = dbService;
            _uiServices = uiServices;
            Throughput = throughput;
        }

        internal async Task LoadDataAsync()
        {
            IsLoading = true;

            await Throughput.LoadData(() => _dbService.GetThroughputAsync(_connection, _database));

            IsChanged = false;
            IsLoading = false;
        }

        [DoNotSetChanged]
        public bool IsLoading { get; set; }

        public bool IsChanged { get; set; }
        
        public bool IsDirty
        {
            get
            {
                return IsChanged || Throughput?.IsChanged == true;
            }
        }

        public bool IsValid
        {
            get
            {
                return !((INotifyDataErrorInfo)this).HasErrors && (Throughput?.IsValid == true);
            }
        }

        public ThroughputViewModel Throughput { get; }

        public override void Load(string contentId, DatabaseScaleNodeViewModel node, Connection connection, DocumentCollection collection)
        {
            IsLoading = true;

            Title = node.Name;
            Header = node.Name;

            AccentColor = connection.AccentColor;

            _database = node.Database;
            _connection = connection;

            IsLoading = false;
        }

        public RelayCommand DiscardCommand
        {
            get
            {
                return _discardCommand
                    ?? (_discardCommand = new RelayCommand(
                        async () =>
                        {
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
                            IsLoading = true;
                            try
                            {
                                await _dbService.UpdateThroughputAsync(_connection, _database, Throughput.Value).ConfigureAwait(false);
                                await LoadDataAsync().ConfigureAwait(false);
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
                            finally
                            {
                                IsLoading = false;
                            }
                        },
                        () => IsDirty && IsValid));
            }
        }
    }

    public class DatabaseScaleTabViewModelValidator : AbstractValidator<DatabaseScaleTabViewModel>
    { }
}
