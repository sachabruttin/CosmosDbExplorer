using System;
using System.IO;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Extensions;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Services;
using DocumentDbExplorer.Services.DialogSettings;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.ViewModel
{
    public class QueryEditorViewModel : PaneViewModel
    {
        private RelayCommand _executeCommand;
        private readonly IDocumentDbService _dbService;
        private readonly IDialogService _dialogService;
        private CollectionNodeViewModel _node;
        private RelayCommand _saveLocalCommand;

        public QueryEditorViewModel(IMessenger messenger, IDocumentDbService dbService, IDialogService dialogService) : base(messenger)
        {
            Content = new TextDocument("SELECT * FROM c");
            EditorViewModel = SimpleIoc.Default.GetInstanceWithoutCaching<JsonViewerViewModel>();
            _dbService = dbService;
            _dialogService = dialogService;
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
                    Header = $"Query - {Node.Parent.Name}";

                    var split = Node.Collection.AltLink.Split(new char[] { '/' });
                    ToolTip = $"{split[1]}>{split[3]}>{Title}";
                }
            }
        }

        protected Connection Connection => Node.Parent.Parent.Connection;

        public TextDocument Content { get; set; }

        public bool IsDirty { get; set; }

        public JsonViewerViewModel EditorViewModel { get; set; }

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
                                var result = await _dbService.ExecuteQuery(Connection, Node.Collection, Content.Text);

                                EditorViewModel.SetText(result, true);
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
                        }));
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
    }
}
