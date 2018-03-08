using System;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Messages;
using DocumentDbExplorer.Services;
using DocumentDbExplorer.ViewModel.Interfaces;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.ViewModel
{
    public class UserDefFuncTabViewModel : PaneWithZoomViewModel, IAssetTabCommand
    {
        private UserDefFuncNodeViewModel _node;
        private RelayCommand _saveCommand;
        private RelayCommand _deleteCommand;
        private RelayCommand _discardCommand;
        private readonly IDialogService _dialogService;
        private readonly IDocumentDbService _dbService;
        private DocumentCollection _collection;
        private string _altLink;

        public UserDefFuncTabViewModel(IMessenger messenger, IDialogService dialogService, IDocumentDbService dbService) : base(messenger)
        {
            Content = new TextDocument(Constants.Default.UserDefiniedFunction);
            _dialogService = dialogService;
            _dbService = dbService;
            Header = "New User Definied Function";
            Title = "User Definied Function";
            ContentId = Guid.NewGuid().ToString();
        }

        public UserDefFuncNodeViewModel Node
        {
            get { return _node; }
            set
            {
                if (_node != value)
                {
                    _node = value;
                    Header = value.Name;

                    AccentColor = _node.Parent.Parent.Parent.Parent.Connection.AccentColor;
                    _altLink = _node.Function.AltLink;

                    SetInformation();
                }
            }
        }
        public Connection Connection { get; set; }

        public DocumentCollection Collection
        {
            get { return _collection; }
            set
            {
                _collection = value;
                var split = value.AltLink.Split(new char[] { '/' });
                ToolTip = $"{split[1]}>{split[3]}";
            }
        }

        private void SetInformation()
        {
            Id = _node.Function.Id;
            SetText(_node.Function.Body);
        }

        public string Id { get; set; }

        public TextDocument Content { get; set; }

        public bool IsDirty { get; set; }

        public bool IsNewDocument
        {
            get
            {
                return _altLink == null;
            }
        }

        protected void SetText(string content)
        {
            DispatcherHelper.RunAsync(() =>
            {
                Content = new TextDocument(content);
                IsDirty = false;
            });
        }

        public RelayCommand DiscardCommand
        {
            get
            {
                return _discardCommand
                    ?? (_discardCommand = new RelayCommand(
                        x =>
                        {
                            if (Node?.Function== null)
                            {
                                SetText(Constants.Default.UserDefiniedFunction);
                            }
                            else
                            {
                                SetInformation();
                            }
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
                            var proc = await _dbService.SaveUdf(Connection, Collection, Id, Content.Text, _altLink).ConfigureAwait(false);
                            Id = proc.Id;
                            _altLink = proc.AltLink;
                            SetText(proc.Body);
                        },
                        x => IsDirty));
            }
        }

        public RelayCommand DeleteCommand
        {
            get
            {
                return _deleteCommand
                    ?? (_deleteCommand = new RelayCommand(
                        async x =>
                        {
                            await _dialogService.ShowMessage("Are you sure...", "Delete", null, null, async confirm =>
                            {
                                if (confirm)
                                {
                                    await _dbService.DeleteUdf(Connection, _altLink).ConfigureAwait(false);
                                    MessengerInstance.Send(new RemoveNodeMessage(_node));
                                    MessengerInstance.Send(new CloseDocumentMessage(this));
                                }
                            }).ConfigureAwait(false);
                        },
                        x => !IsNewDocument));
            }
        }
    }
}
