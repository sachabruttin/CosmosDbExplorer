using System;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Services;
using DocumentDbExplorer.ViewModel.Interfaces;
using GalaSoft.MvvmLight.Messaging;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.ViewModel
{
    public class TriggerTabViewModel : PaneWithZoomViewModel, IAssetTabCommand
    {
        private TriggerNodeViewModel _node;
        private RelayCommand _saveCommand;
        private RelayCommand _deleteCommand;
        private RelayCommand _discardCommand;
        private readonly IDialogService _dialogService;
        private readonly IDocumentDbService _dbService;
        private TriggerType _triggerType;
        private TriggerOperation _triggerOperation;
        private DocumentCollection _collection;

        public TriggerTabViewModel(IMessenger messenger, IDialogService dialogService, IDocumentDbService dbService) : base(messenger)
        {
            Content = new TextDocument(Constants.Default.Trigger);
            _dialogService = dialogService;
            _dbService = dbService;
            Header = "New Trigger";
            Title = "Trigger";
            ContentId = Guid.NewGuid().ToString();
        }

        public TriggerNodeViewModel Node
        {
            get { return _node; }
            set
            {
                if (_node != value)
                {
                    _node = value;
                    Header = value.Name;
                    AccentColor = _node.Parent.Parent.Parent.Parent.Connection.AccentColor;

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
                ToolTip = $"{split[1]}>{split[3]}>{Title}";
            }
        }

        private void SetInformation()
        {
            Id = _node.Trigger.Id;
            TriggerOperation = _node.Trigger.TriggerOperation;
            TriggerType = _node.Trigger.TriggerType;
            SetText(_node.Trigger.Body);
        }

        public string Id { get; set; }

        public TextDocument Content { get; set; }

        public bool IsDirty { get; set; }

        public bool IsNewDocument
        {
            get
            {
                return Node?.Trigger?.SelfLink == null;
            }
        }

        protected void SetText(string content)
        {
            Content = new TextDocument(content);
            IsDirty = false;
        }

        public TriggerType TriggerType
        {
            get { return _triggerType; }
            set
            {
                if (value != _triggerType)
                {
                    _triggerType = value;
                    IsDirty = true;
                    RaisePropertyChanged(() => TriggerType);
                }
            }
        }

        public TriggerOperation TriggerOperation
        {
            get { return _triggerOperation; }
            set
            {
                if (value != _triggerOperation)
                {
                    _triggerOperation = value;
                    IsDirty = true;
                    RaisePropertyChanged(() => TriggerOperation);
                }
            }
        }

        public RelayCommand DiscardCommand
        {
            get
            {
                return _discardCommand
                    ?? (_discardCommand = new RelayCommand(
                        x =>
                        {
                            if (Node?.Trigger == null)
                            {
                                SetText(Constants.Default.StoredProcedure);
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
                            var proc = await _dbService.CreateTrigger(Connection, Collection, Id, Content.Text, TriggerType, TriggerOperation);
                            Id = proc.Id;
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
                                    await _dbService.DeleteTrigger(Connection, Node.Trigger.SelfLink);
                                }
                            });
                        },
                        x => !IsNewDocument));
            }
        }
    }
}
