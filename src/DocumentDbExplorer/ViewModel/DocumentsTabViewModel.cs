﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Extensions;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Properties;
using DocumentDbExplorer.Services;
using DocumentDbExplorer.Services.DialogSettings;
using DocumentDbExplorer.ViewModel.Interfaces;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace DocumentDbExplorer.ViewModel
{
    public class DocumentsTabViewModel : PaneWithZoomViewModel, IHaveQuerySettings, IHaveRequestOptions
    {
        private readonly IDocumentDbService _dbService;
        private readonly IDialogService _dialogService;
        private readonly StatusBarItem _requestChargeStatusBarItem;
        private readonly StatusBarItem _progessBarStatusBarItem;
        private RelayCommand _loadMoreCommand;
        private RelayCommand _refreshLoadCommand;
        private RelayCommand _newDocumentCommand;
        private RelayCommand _discardCommand;
        private RelayCommand _saveDocumentCommand;
        private RelayCommand _deleteDocumentCommand;
        private RelayCommand _editFilterCommand;
        private RelayCommand _applyFilterCommand;
        private RelayCommand _closeFilterCommand;
        private RelayCommand _saveLocalCommand;
        private DocumentNodeViewModel _node;
        private ResourceResponse<Document> _currentDocument;
        private RelayCommand _resetRequestOptionsCommand;

        public DocumentsTabViewModel(IMessenger messenger, IDocumentDbService dbService, IDialogService dialogService) : base(messenger)
        {
            Documents = new ObservableCollection<DocumentDescription>();
            _dbService = dbService;
            _dialogService = dialogService;

            EditorViewModel = SimpleIoc.Default.GetInstanceWithoutCaching<DocumentEditorViewModel>();
            HeaderViewModel = SimpleIoc.Default.GetInstanceWithoutCaching<HeaderEditorViewModel>();
            HeaderViewModel.IsReadOnly = true;

            Title = "Documents";
            Header = Title;

            _requestChargeStatusBarItem = new StatusBarItem(new StatusBarItemContext { Value = RequestCharge }, StatusBarItemType.SimpleText, "Request Charge", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_requestChargeStatusBarItem);
            _progessBarStatusBarItem = new StatusBarItem(new StatusBarItemContext { Value = IsRunning, IsVisible = IsRunning }, StatusBarItemType.ProgessBar, "Progess", System.Windows.Controls.Dock.Left);
            StatusBarItems.Add(_progessBarStatusBarItem);
        }

        public DocumentNodeViewModel Node
        {
            get { return _node; }
            set
            {
                if (_node != value)
                {
                    _node = value;

                    PartitionKey = Node.Parent.Collection.PartitionKey?.Paths.FirstOrDefault();
                    var split = Node.Parent.Collection.AltLink.Split(new char[] { '/' });
                    ToolTip = $"{split[1]}>{split[3]}>{Title}";
                    AccentColor = Node.Parent.Parent.Parent.Connection.AccentColor;
                }
            }
        }

        public string PartitionKey { get; set; }

        public ObservableCollection<DocumentDescription> Documents { get; }

        public DocumentDescription SelectedDocument { get; set; }

        public async void OnSelectedDocumentChanged()
        {
            if (SelectedDocument != null)
            {
                IsRunning = true;

                if (_currentDocument?.Resource.Id != SelectedDocument.Id)
                {
                    try
                    {
                        _currentDocument = await _dbService.GetDocument(Node.Parent.Parent.Parent.Connection, SelectedDocument);
                    }
                    catch (DocumentClientException clientEx)
                    {
                        await _dialogService.ShowError(clientEx.Parse(), "Error", null, null);
                    }
                    catch (Exception ex)
                    {
                        await _dialogService.ShowError(ex, "Error", "ok", null);
                    }
                }

                SetStatusBar(_currentDocument);
                IsRunning = false;
            }
            else
            {
                SetStatusBar(null);
            }
        }

        public string Filter { get; set; }

        public bool IsEditingFilter { get; set; }

        public bool HasMore { get; set; }
        public string ContinuationToken { get; set; }

        public string RequestCharge { get; set; }

        public void OnRequestChargeChanged()
        {
            _requestChargeStatusBarItem.DataContext.Value = RequestCharge;
        }

        public bool IsRunning { get; set; }

        public void OnIsRunningChanged()
        {
            _progessBarStatusBarItem.DataContext.IsVisible = IsRunning;
            _requestChargeStatusBarItem.DataContext.IsVisible = !IsRunning;
        }

        private void SetStatusBar(ResourceResponse<Document> response)
        {
            RequestCharge = response != null
                ? $"Request Charge: {response.RequestCharge:N2}"
                : null;

            EditorViewModel.SetText(response?.Resource, HideSystemProperties);
            HeaderViewModel.SetText(response?.ResponseHeaders, HideSystemProperties);
        }

        public async Task LoadDocuments(bool cleanContent = false)
        {
            try
            {
                IsRunning = true;

                if (cleanContent)
                {
                    Documents.Clear();
                    ContinuationToken = null;
                }

                var list = await _dbService.GetDocuments(Node.Parent.Parent.Parent.Connection,
                                       Node.Parent.Collection,
                                       Filter,
                                       Settings.Default.MaxDocumentToRetrieve,
                                       ContinuationToken);

                HasMore = list.HasMore;
                ContinuationToken = list.ContinuationToken;
                RequestCharge = $"Request Charge: {list.RequestCharge:N2}";

                foreach (var document in list)
                {
                    Documents.Add(document);
                }

                TotalItemsCount = list.CollectionSize;
                RaisePropertyChanged(() => ItemsCount);
            }
            catch (DocumentClientException clientEx)
            {
                await _dialogService.ShowError(clientEx.Parse(), "Error", "ok", null);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowError(ex, "Error", "ok", null);
            }
            finally
            {
                IsRunning = false;
            }
        }

        public long TotalItemsCount { get; set; }
        public long ItemsCount => Documents.Count;

        public DocumentEditorViewModel EditorViewModel { get; set; }

        public HeaderEditorViewModel HeaderViewModel { get; set; }

        protected Connection Connection => Node.Parent.Parent.Parent.Connection;

        protected DocumentCollection Collection => Node.Parent.Collection;

        public RelayCommand LoadMoreCommand
        {
            get
            {
                return _loadMoreCommand
                    ?? (_loadMoreCommand = new RelayCommand(
                        async x => await LoadDocuments(false)));
            }
        }

        public RelayCommand RefreshLoadCommand
        {
            get
            {
                return _refreshLoadCommand
                    ?? (_refreshLoadCommand = new RelayCommand(
                        async x =>
                        {
                            await LoadDocuments(true);
                        },
                        x => !IsRunning));
            }
        }

        public RelayCommand NewDocumentCommand
        {
            get
            {
                return _newDocumentCommand
                    ?? (_newDocumentCommand = new RelayCommand(
                        x =>
                        {
                            SelectedDocument = null;
                            SetStatusBar(null);
                            EditorViewModel.SetText(new Document() { Id = "replace_with_the_new_document_id" }, HideSystemProperties);
                        },
                        x =>
                        {
                            // Can create new document if current document is not a new document
                            return !IsRunning && !EditorViewModel.IsNewDocument && !EditorViewModel.IsDirty;
                        }));
            }
        }

        public RelayCommand DiscardCommand
        {
            get
            {
                return _discardCommand
                    ?? (_discardCommand = new RelayCommand(
                        x => OnSelectedDocumentChanged(),
                        x => !IsRunning && EditorViewModel.IsDirty));
            }
        }

        public RelayCommand SaveDocumentCommand
        {
            get
            {
                return _saveDocumentCommand
                    ?? (_saveDocumentCommand = new RelayCommand(
                        async x =>
                        {
                            IsRunning = true;
                            try
                            {
                                var response = await _dbService.UpdateDocument(Connection, Collection.AltLink, EditorViewModel.Content.Text, this);
                                var document = response.Resource;

                                SetStatusBar(response);

                                var description = new DocumentDescription(document, Collection);

                                if (SelectedDocument == null)
                                {
                                    Documents.Add(description);
                                    SelectedDocument = description;
                                }

                                HeaderViewModel.SetText(response.ResponseHeaders, HideSystemProperties);
                            }
                            catch (DocumentClientException ex)
                            {
                                var message = ex.Parse();
                                await _dialogService.ShowError(message, "Error", null, null);
                            }
                            finally
                            {
                                IsRunning = false;
                            }
                        },
                        x => !IsRunning && EditorViewModel.IsDirty));
            }
        }

        public RelayCommand DeleteDocumentCommand
        {
            get
            {
                return _deleteDocumentCommand
                    ?? (_deleteDocumentCommand = new RelayCommand(
                        async x =>
                        {
                            var documentId = SelectedDocument.Id;

                            await _dialogService.ShowMessage($"Are you sure that you want to delete document '{documentId}'?", "Delete Document", null, null, async confirm =>
                            {
                                if (confirm)
                                {
                                    IsRunning = true;
                                    var response = await _dbService.DeleteDocument(Node.Parent.Parent.Parent.Connection, SelectedDocument);
                                    IsRunning = false;
                                    SetStatusBar(response);

                                    await DispatcherHelper.RunAsync(() =>
                                    {
                                        Documents.Remove(SelectedDocument);
                                        SelectedDocument = null;

                                        SetStatusBar(response);
                                        EditorViewModel.SetText(new { result = $"Document '{documentId}' deleted" }, HideSystemProperties);
                                    });
                                }
                            });
                        },
                        x => !IsRunning && SelectedDocument != null && !EditorViewModel.IsNewDocument));
            }
        }

        public RelayCommand EditFilterCommand
        {
            get
            {
                return _editFilterCommand
                    ?? (_editFilterCommand = new RelayCommand(
                        x =>
                        {
                            IsEditingFilter = true;
                        }));
            }
        }

        public RelayCommand ApplyFilterCommand
        {
            get
            {
                return _applyFilterCommand
                    ?? (_applyFilterCommand = new RelayCommand(
                        async x =>
                        {
                            IsEditingFilter = false;
                            await LoadDocuments(true);
                        }));
            }
        }

        public RelayCommand CloseFilterCommand
        {
            get
            {
                return _closeFilterCommand
                    ?? (_closeFilterCommand = new RelayCommand(
                        x =>
                        {
                            IsEditingFilter = false;
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
                            FileName = $"{SelectedDocument.Id}.json",
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
                                    try
                                    {
                                        IsRunning = true;
                                        File.WriteAllText(result.FileName, EditorViewModel.Content.Text);
                                    }
                                    catch
                                    {
                                        // TOOD: 
                                    }
                                    finally
                                    {
                                        IsRunning = false;
                                    }
                                });
                            }
                        });
                    },
                    x => !IsRunning && SelectedDocument != null));
            }
        }

        public bool HideSystemProperties { get; set; } = true;

        public void OnHideSystemPropertiesChanged()
        {
            OnSelectedDocumentChanged();
        }

        public bool? EnableScanInQuery { get; set; } = null;
        public bool? EnableCrossPartitionQuery { get; set; } = null;
        public int? MaxItemCount { get; set; } = null;
        public int? MaxDOP { get; set; } = null;
        public int? MaxBufferItem { get; set; } = null;

        private void ClearDocuments()
        {
            HasMore = false;
            ContinuationToken = null;
            Documents.Clear();
        }


        public RelayCommand ResetRequestOptionsCommand
        {
            get
            {
                return _resetRequestOptionsCommand
                    ?? (_resetRequestOptionsCommand = new RelayCommand(
                        x =>
                        {
                            var instance = ((IHaveRequestOptions)this);
                            instance.IndexingDirective = null;
                            instance.ConsistencyLevel = null;
                            instance.PartitionKey = null;
                            instance.AccessConditionType = null;
                            instance.AccessCondition = null;
                            instance.PreTrigger = null;
                            instance.PostTrigger = null;
                        }));
            }
        }

        IndexingDirective? IHaveRequestOptions.IndexingDirective { get; set; }
        ConsistencyLevel? IHaveRequestOptions.ConsistencyLevel { get; set; }
        string IHaveRequestOptions.PartitionKey { get; set; }
        AccessConditionType? IHaveRequestOptions.AccessConditionType { get; set; }
        string IHaveRequestOptions.AccessCondition { get; set; }
        string IHaveRequestOptions.PreTrigger { get; set; }
        string IHaveRequestOptions.PostTrigger { get; set; }
    }
}
