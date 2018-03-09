using System;
using System.Threading.Tasks;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Extensions;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Messages;
using DocumentDbExplorer.Services;
using DocumentDbExplorer.ViewModel.Interfaces;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.ViewModel.Assets
{
    public abstract class AssetTabViewModelBase<TNode, TResource> : PaneWithZoomViewModel<TNode>, IAssetTabCommand
        where TNode : TreeViewItemViewModel, IAssetNode<TResource>
        where TResource : Resource
    {
        private readonly IDialogService _dialogService;
        private readonly IDocumentDbService _dbService;

        private DocumentCollection _collection;
        private RelayCommand _discardCommand;
        private RelayCommand _saveCommand;
        private RelayCommand _deleteCommand;

        protected AssetTabViewModelBase(IMessenger messenger, IDialogService dialogService, IDocumentDbService dbService) : base(messenger)
        {
            Content = new TextDocument(GetDefaultContent());
            _dialogService = dialogService;
            _dbService = dbService;
            Header = GetDefaultHeader();
            Title = GetDefaultTitle();
            ContentId = Guid.NewGuid().ToString();
        }

        protected abstract string GetDefaultHeader();
        protected abstract string GetDefaultTitle();
        protected abstract string GetDefaultContent();
        protected abstract void SetInformationImpl(TResource resource);
        protected abstract Task<TResource> SaveAsyncImpl(IDocumentDbService dbService);
        protected abstract Task DeleteAsyncImpl(IDocumentDbService dbService);

        protected string AltLink { get; set; }

        public TextDocument Content { get; set; }

        public override void Load(string contentId, TNode node, Connection connection, DocumentCollection collection)
        {
            ContentId = contentId;
            Node = node;
            Connection = connection;
            Collection = collection;
            AccentColor = connection.AccentColor;
            SetInformation(Node?.Resource);
        }

        public TNode Node { get; protected set; }

        protected void SetInformation(TResource resource)
        {
            if (resource != null)
            {
                Id = resource.Id;
                AltLink = resource.AltLink;
                ContentId = AltLink;
                Header = resource.Id;
                SetInformationImpl(resource);
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

        public string Id { get; set; }

        public bool IsDirty { get; set; }

        public bool IsNewDocument => AltLink == null;

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
                        () =>
                        {
                            if (IsNewDocument)
                            {
                                SetText(Constants.Default.UserDefiniedFunction);
                            }
                            else
                            {
                                SetInformation(Node.Resource);
                            }
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
                            try
                            {
                                var resource = await SaveAsyncImpl(_dbService).ConfigureAwait(false);
                                MessengerInstance.Send(new UpdateOrCreateNodeMessage<TResource>(resource, AltLink));
                                SetInformation(resource);
                            }
                            catch (DocumentClientException clientEx)
                            {
                                await _dialogService.ShowError(clientEx.Parse(), "Error", "ok", null).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                await _dialogService.ShowError(ex, "Error", "ok", null).ConfigureAwait(false);
                            }
                        },
                        () => IsDirty));
            }
        }

        public RelayCommand DeleteCommand
        {
            get
            {
                return _deleteCommand
                    ?? (_deleteCommand = new RelayCommand(
                        async () =>
                        {
                            await _dialogService.ShowMessage("Are you sure...", "Delete", null, null, async confirm =>
                            {
                                if (confirm)
                                {
                                    await DeleteAsyncImpl(_dbService).ConfigureAwait(false);
                                    MessengerInstance.Send(new RemoveNodeMessage(AltLink));
                                    MessengerInstance.Send(new CloseDocumentMessage(this));
                                }
                            }).ConfigureAwait(false);
                        },
                        () => !IsNewDocument));
            }
        }
    }
}
