using System;
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

namespace DocumentDbExplorer.ViewModel
{
    public class QueryEditorViewModel : PaneWithZoomViewModel, IHaveQuerySettings
    {
        private RelayCommand _executeCommand;
        private readonly IDocumentDbService _dbService;
        private readonly IDialogService _dialogService;
        private CollectionNodeViewModel _node;
        private RelayCommand _saveLocalCommand;
        private FeedResponse<Document> _queryResult;
        private readonly StatusBarItem _requestChargeStatusBarItem;

        public QueryEditorViewModel(IMessenger messenger, IDocumentDbService dbService, IDialogService dialogService) : base(messenger)
        {
            Content = new TextDocument("SELECT * FROM c");
            EditorViewModel = SimpleIoc.Default.GetInstanceWithoutCaching<JsonViewerViewModel>();
            EditorViewModel.IsReadOnly = true;

            HeaderViewModel = SimpleIoc.Default.GetInstanceWithoutCaching<FeedResponseEditorViewModel>();
            HeaderViewModel.IsReadOnly = true;

            _dbService = dbService;
            _dialogService = dialogService;

            _requestChargeStatusBarItem = new StatusBarItem(RequestCharge, StatusBarItemType.SimpleText, "Request Charge", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_requestChargeStatusBarItem);
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

        public FeedResponseEditorViewModel HeaderViewModel { get; set; }

        public string RequestCharge { get; set; }

        public void OnRequestChargeChanged()
        {
            _requestChargeStatusBarItem.DataContext = RequestCharge;
        }

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
                                _queryResult = await _dbService.ExecuteQuery(Connection, Node.Collection, query);

                                RequestCharge = $"Request Charge: {_queryResult.RequestCharge}";
                                EditorViewModel.SetText(_queryResult, HideSystemProperties);
                                HeaderViewModel.SetText(_queryResult, HideSystemProperties);
                            }
                            catch (DocumentClientException clientEx)
                            {
                                var errors = clientEx.Parse();
                                await _dialogService.ShowError(errors.ToString(), "Error", "ok", null);
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

        public bool EnableScanInQuery { get; set; }
        public bool EnableCrossPartitionQuery { get; set; }
    }
}
