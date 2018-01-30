﻿using System;
using System.IO;
using System.Linq;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Extensions;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Services;
using DocumentDbExplorer.Services.DialogSettings;
using DocumentDbExplorer.ViewModel.Interfaces;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace DocumentDbExplorer.ViewModel
{
    public class QueryEditorViewModel : PaneWithZoomViewModel, IHaveQuerySettings
    {
        private RelayCommand _executeCommand;
        private readonly IDocumentDbService _dbService;
        private readonly IDialogService _dialogService;
        private CollectionNodeViewModel _node;
        private RelayCommand _saveLocalCommand;
        private FeedResponse<dynamic> _queryResult;
        private readonly StatusBarItem _requestChargeStatusBarItem;
        private readonly StatusBarItem _queryInformationStatusBarItem;

        public QueryEditorViewModel(IMessenger messenger, IDocumentDbService dbService, IDialogService dialogService) : base(messenger)
        {
            Content = new TextDocument("SELECT * FROM c");
            EditorViewModel = SimpleIoc.Default.GetInstanceWithoutCaching<JsonViewerViewModel>();
            EditorViewModel.IsReadOnly = true;

            HeaderViewModel = SimpleIoc.Default.GetInstanceWithoutCaching<HeaderEditorViewModel>();
            HeaderViewModel.IsReadOnly = true;

            _dbService = dbService;
            _dialogService = dialogService;

            _requestChargeStatusBarItem = new StatusBarItem(RequestCharge, StatusBarItemType.SimpleText, "Request Charge", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_requestChargeStatusBarItem);
            _queryInformationStatusBarItem = new StatusBarItem(QueryInformation, StatusBarItemType.SimpleText, "Information", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_queryInformationStatusBarItem);
        }

        public CollectionNodeViewModel Node
        {
            get { return _node; }
            set
            {
                if (_node != value)
                {
                    _node = value;
                    ContentId = Node.Parent.Name;
                    Header = $"SQL Query";

                    var split = Node.Collection.AltLink.Split(new char[] { '/' });
                    ToolTip = $"{split[1]}>{split[3]}";
                }
            }
        }

        protected Connection Connection => Node.Parent.Parent.Connection;

        public TextDocument Content { get; set; }

        public string SelectedText { get; set; }

        public bool IsDirty { get; set; }

        public JsonViewerViewModel EditorViewModel { get; set; }

        public HeaderEditorViewModel HeaderViewModel { get; set; }

        public string RequestCharge { get; set; }

        public void OnRequestChargeChanged()
        {
            _requestChargeStatusBarItem.DataContext = RequestCharge;
        }

        public string QueryInformation { get; set; }

        public void OnQueryInformationChanged()
        {
            _queryInformationStatusBarItem.DataContext = QueryInformation;
        }

        public ResponseContinuation ContinuationToken { get; set; }

        public RelayCommand ExecuteCommand
        {
            get
            {
                return _executeCommand
                    ?? (_executeCommand = new RelayCommand(
                        async x =>
                        {
                            try
                            {
                                var query = string.IsNullOrEmpty(SelectedText) ? Content.Text : SelectedText;
                                _queryResult = await _dbService.ExecuteQuery(Connection, Node.Collection, query, EnableCrossPartitionQuery, EnableScanInQuery);

                                RequestCharge = $"Request Charge: {_queryResult.RequestCharge}";
                                ContinuationToken = JsonConvert.DeserializeObject<ResponseContinuation>(_queryResult.ResponseContinuation ?? string.Empty);

                                QueryInformation = $"Returned {_queryResult.Count} documents." + 
                                                        (ContinuationToken?.Token != null 
                                                                ?" (more results available)" 
                                                                : string.Empty);

                                EditorViewModel.SetText(_queryResult, HideSystemProperties);
                                HeaderViewModel.SetText(_queryResult.ResponseHeaders, HideSystemProperties);
                            }
                            catch (DocumentClientException clientEx)
                            {
                                await _dialogService.ShowError(clientEx.Parse(), "Error", "ok", null);
                            }
                            catch (Exception ex)
                            {
                                await _dialogService.ShowError(ex, "Error", "ok", null);
                            }
                        },
                        x => !string.IsNullOrEmpty(Content.Text)));
            }
        }

        public RelayCommand SaveLocalCommand
        {
            get
            {
                return _saveLocalCommand ??
                (_saveLocalCommand = new RelayCommand(
                    async x =>
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
                                await DispatcherHelper.RunAsync(() =>
                                {
                                    File.WriteAllText(result.FileName, EditorViewModel.Content.Text);
                                });
                            }
                        });
                    },
                    x => !string.IsNullOrEmpty(EditorViewModel.Content?.Text)));
            }
        }

        public bool HideSystemProperties { get; set; } = true;

        public void OnHideSystemPropertiesChanged()
        {
            if (_queryResult != null)
            {
                EditorViewModel.SetText(_queryResult, HideSystemProperties);
            }
        }

        public bool? EnableScanInQuery { get; set; } = false;
        public bool? EnableCrossPartitionQuery { get; set; } = false;
    }
}
