using System;
using System.IO;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Extensions;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Services;
using DocumentDbExplorer.Services.DialogSettings;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.ViewModel
{
    public class ImportDocumentViewModel : PaneViewModel
    {
        private RelayCommand _executeCommand;
        private readonly IDialogService _dialogService;
        private readonly IDocumentDbService _dbService;
        private CollectionNodeViewModel _node;
        private RelayCommand _openFileCommand;

        public ImportDocumentViewModel(IMessenger messenger, IDialogService dialogService, IDocumentDbService dbService) : base(messenger)
        {
            Content = new TextDocument();
            _dialogService = dialogService;
            _dbService = dbService;
        }

        public CollectionNodeViewModel Node
        {
            get { return _node; }
            set
            {
                if (_node != value)
                {
                    _node = value;
                    Header = Node.Parent.Name;
                    
                    var split = Node.Collection.AltLink.Split(new char[] { '/' });
                    ToolTip = $"{split[1]}>{split[3]}>{Title}";
                }
            }
        }

        protected Connection Connection => Node.Parent.Parent.Connection;

        public TextDocument Content { get; set; }

        public bool IsDirty { get; set; }

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
                                var count = await _dbService.ImportDocument(Connection, Node.Collection, Content.Text);
                                await _dialogService.ShowMessageBox($"{count} document(s) imported!", "Import");
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

        public RelayCommand OpenFileCommand
        {
            get
            {
                return _openFileCommand
                    ?? (_openFileCommand = new RelayCommand(
                        async x =>
                        {
                            var settings = new OpenFileDialogSettings
                            {
                                DefaultExt = "json",
                                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                                AddExtension = true,
                                CheckFileExists = true,
                                Multiselect = false,
                                Title = "Open document"
                            };

                            await _dialogService.ShowOpenFileDialog(settings,
                                async (confirm, result) =>
                                {
                                    if (confirm)
                                    {
                                        using (var reader = File.OpenText(result.FileName))
                                        {
                                            await DispatcherHelper.RunAsync(async () =>
                                            {
                                                Content.FileName = result.FileName;
                                                Content.Text = await reader.ReadToEndAsync();
                                            });
                                        }
                                    }
                                });
                        }
                        ));
            }
        }
    }
}
