﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CosmosDbExplorer.Infrastructure;
using CosmosDbExplorer.Infrastructure.Extensions;
using CosmosDbExplorer.Infrastructure.Models;
using CosmosDbExplorer.Services;
using CosmosDbExplorer.Services.DialogSettings;
using FluentValidation;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Azure.Documents;
using Validar;

namespace CosmosDbExplorer.ViewModel.Assets
{
    [InjectValidation]
    public class StoredProcedureTabViewModel : AssetTabViewModelBase<StoredProcedureNodeViewModel, StoredProcedure>
    {
        private RelayCommand _executeCommand;
        private RelayCommand<StoredProcParameterViewModel> _removeParameterCommand;
        private RelayCommand _addParameterCommand;
        private RelayCommand<object> _browseParameterCommand;
        private RelayCommand _saveLocalCommand;
        private RelayCommand _goToNextPageCommand;
        private readonly IDialogService _dialogService;
        private readonly IDocumentDbService _dbService;
        private readonly StatusBarItem _requestChargeStatusBarItem;

        public StoredProcedureTabViewModel(IMessenger messenger, IDialogService dialogService, IDocumentDbService dbService, IUIServices uiServices)
            : base(messenger, dialogService, dbService, uiServices)
        {
            _dialogService = dialogService;
            _dbService = dbService;

            ResultViewModel = SimpleIoc.Default.GetInstanceWithoutCaching<JsonViewerViewModel>();
            ResultViewModel.IsReadOnly = true;

            HeaderViewModel = SimpleIoc.Default.GetInstanceWithoutCaching<HeaderEditorViewModel>();
            HeaderViewModel.IsReadOnly = true;

            _requestChargeStatusBarItem = new StatusBarItem(new StatusBarItemContext { Value = RequestCharge, IsVisible = IsBusy }, StatusBarItemType.SimpleText, "Request Charge", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_requestChargeStatusBarItem);
        }

        protected override string GetDefaultHeader() { return "New Stored Procedure"; }
        protected override string GetDefaultTitle() { return "Stored Procedure"; }
        protected override string GetDefaultContent() { return Constants.Default.StoredProcedure; }

        public override void Load(string contentId, StoredProcedureNodeViewModel node, Connection connection, DocumentCollection collection)
        {
            IsCollectionPartitioned = collection.PartitionKey.Paths.Count > 0;

            base.Load(contentId, node, connection, collection);
        }

        protected override void SetInformationImpl(StoredProcedure resource)
        {
            SetText(resource.Body);
        }

        protected override Task<StoredProcedure> SaveAsyncImpl(IDocumentDbService dbService)
        {
            return dbService.SaveStoredProcedureAsync(Connection, Collection, Id, Content.Text, AltLink);
        }

        protected override Task DeleteAsyncImpl(IDocumentDbService dbService)
        {
            return dbService.DeleteStoredProcedureAsync(Connection, AltLink);
        }

        public string Log { get; protected set; }

        public JsonViewerViewModel ResultViewModel { get; set; }

        public HeaderEditorViewModel HeaderViewModel { get; set; }

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

        public string PartitionKey { get; set; }

        public bool IsCollectionPartitioned { get; protected set; }

        public ObservableCollection<StoredProcParameterViewModel> Parameters { get; } = new ObservableCollection<StoredProcParameterViewModel>();

        public RelayCommand AddParameterCommand
        {
            get
            {
                return _addParameterCommand ?? (_addParameterCommand = new RelayCommand(
                    () => Parameters.Add(new StoredProcParameterViewModel()),
                    () => !IsBusy && !IsDirty));
            }
        }

        public RelayCommand<StoredProcParameterViewModel> RemoveParameterCommand
        {
            get
            {
                return _removeParameterCommand ?? (_removeParameterCommand = new RelayCommand<StoredProcParameterViewModel>(
                    item =>
                    {
                        Parameters.Remove(item);
                        item.Dispose();
                    },
#pragma warning disable RCS1163 // Unused parameter.
                    item => !IsBusy & !IsDirty));
#pragma warning restore RCS1163 // Unused parameter.
            }
        }

        public RelayCommand<object> BrowseParameterCommand
        {
            get
            {
                return _browseParameterCommand ?? (_browseParameterCommand = new RelayCommand<object>(
                    async item =>
                    {
                        var options = new OpenFileDialogSettings
                        {
                            Title = "Select file...",
                            DefaultExt = "json",
                            Multiselect = false, Filter = "JSON|*.json"
                        };

                        await _dialogService.ShowOpenFileDialog(options, (confirm, result) =>
                        {
                            if (confirm && item is StoredProcParameterViewModel vm)
                            {
                                vm.FileName = result.FileName;
                            }
                        }).ConfigureAwait(false);
                    },
#pragma warning disable RCS1163 // Unused parameter.
                    item => !IsBusy & !IsDirty));
#pragma warning restore RCS1163 // Unused parameter.
            }
        }

        public RelayCommand ExecuteCommand
        {
            get
            {
                return _executeCommand ?? (_executeCommand = new RelayCommand(
                    async () =>
                    {
                        try
                        {
                            IsBusy = true;
                            var result = await _dbService.ExecuteStoreProcedureAsync(Connection, AltLink, Parameters.Select(p => p.GetValue()).ToArray(), PartitionKey).ConfigureAwait(false);
                            RequestCharge = $"Request Charge: {result.RequestCharge:N2}";

                            Log += $"{DateTime.Now:T} : {WebUtility.UrlDecode(result.ScriptLog)}{Environment.NewLine}";

                            ResultViewModel.SetText(result.Response, false);
                            HeaderViewModel.SetText(result.ResponseHeaders, false);
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
                    },
                    () => !IsBusy && !IsDirty && IsValid));
            }
        }

        public RelayCommand SaveLocalCommand
        {
            get
            {
                return _saveLocalCommand ?? (_saveLocalCommand = new RelayCommand(
                    async () =>
                    {
                        var settings = new SaveFileDialogSettings
                        {
                            DefaultExt = "json",
                            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                            AddExtension = true,
                            OverwritePrompt = true,
                            CheckFileExists = false,
                            Title = "Save document locally"
                        };

                        await _dialogService.ShowSaveFileDialog(settings, async (confirm, result) =>
                        {
                            if (confirm)
                            {
                                try
                                {
                                    IsBusy = true;

                                    await DispatcherHelper.RunAsync(() => File.WriteAllText(result.FileName, ResultViewModel.Content.Text));
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
                        }).ConfigureAwait(false);
                    },
                    () => !IsBusy && !string.IsNullOrEmpty(ResultViewModel.Content?.Text)));
            }
        }

        public RelayCommand GoToNextPageCommand
        {
            get
            {
                return _goToNextPageCommand ?? (_goToNextPageCommand = new RelayCommand(
                    () => throw new NotImplementedException(),
                    () => false));
            }
        }

        public bool IsValid => !((INotifyDataErrorInfo)this).HasErrors;
    }

    public class StoredProcedureTabViewModelValidator : AbstractValidator<StoredProcedureTabViewModel>
    {
        public StoredProcedureTabViewModelValidator()
        {
            RuleFor(x => x.PartitionKey)
                .NotEmpty()
                .When(x => x.IsCollectionPartitioned);
        }
    }
}
