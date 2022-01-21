using System;
using System.Threading.Tasks;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Extensions;
using CosmosDbExplorer.Models;
using CosmosDbExplorer.Messages;
using CosmosDbExplorer.Services;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Azure.Documents;
using Microsoft.Toolkit.Mvvm.Input;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Core.Contracts;
using CosmosDbExplorer.Core.Models;

namespace CosmosDbExplorer.ViewModels.Assets
{
    public abstract class AssetTabViewModelBase<TNode, TResource> : PaneWithZoomViewModel<TNode>, IAssetTabCommand
        where TNode : TreeViewItemViewModel, IAssetNode<TResource>
        where TResource : ICosmosScript
    {
        //private readonly IDialogService _dialogService;
        private readonly IServiceProvider _serviceProvider;

        private CosmosContainer _collection;

        protected AssetTabViewModelBase(IServiceProvider serviceProvider, IUIServices uiServices)
            : base(uiServices)
        {
            Content = GetDefaultContent();
            //_dialogService = dialogService;
            _serviceProvider = serviceProvider;
            Header = GetDefaultHeader();
            Title = GetDefaultTitle();
            ContentId = Guid.NewGuid().ToString();
        }

        protected abstract string GetDefaultHeader();
        protected abstract string GetDefaultTitle();
        protected abstract string GetDefaultContent();
        protected virtual void SetInformationImpl(TResource resource)
        {
            SetText(resource.Body);
        }
        //protected abstract Task<TResource> SaveAsyncImpl(IDocumentDbService dbService);
        //protected abstract Task DeleteAsyncImpl(IDocumentDbService dbService);

        protected string AltLink { get; set; }

        public string Content { get; set; }

        public override void Load(string contentId, TNode node, CosmosConnection connection, CosmosContainer collection)
        {
            ContentId = contentId;
            Node = node;
            Connection = connection;
            Collection = collection;
            AccentColor = connection.AccentColor;

            if (node != null)
            {
                SetInformation(node.Resource);
            }
        }

        public TNode Node { get; protected set; }

        protected void SetInformation(TResource? resource)
        {
            if (resource != null)
            {
                Id = resource.Id;
                AltLink = resource.SelfLink;
                ContentId = AltLink;
                Header = resource.Id;
                SetInformationImpl(resource);
            }
        }

        public CosmosConnection Connection { get; set; }

        public CosmosContainer Collection
        {
            get { return _collection; }
            set
            {
                _collection = value;
                var split = value.SelfLink.Split(new char[] { '/' });
                ToolTip = $"{split[1]}>{split[3]}";
            }
        }

        public string? Id { get; set; }

        public bool IsDirty { get; set; }

        public bool IsNewDocument => AltLink == null;

        protected void SetText(string content)
        {
            //DispatcherHelper.RunAsync(() =>
            //{
                Content = content;
                IsDirty = false;
            //});
        }

        public RelayCommand DiscardCommand => new(DiscardCommandExecute, DiscardCommandCanExecute);

        protected abstract void DiscardCommandExecute();

        protected virtual bool DiscardCommandCanExecute()
        {
            return IsDirty;
        }

        public RelayCommand SaveCommand => new(SaveCommandExecute, SaveCommandCanExecute);

        protected virtual void SaveCommandExecute()
        {
            throw new NotImplementedException();
            //try
            //{
            //    var resource = await SaveAsyncImpl(_dbService).ConfigureAwait(false);
            //    MessengerInstance.Send(new UpdateOrCreateNodeMessage<TResource>(resource, Collection, AltLink));
            //    SetInformation(resource);
            //}
            //catch (DocumentClientException clientEx)
            //{
            //    await _dialogService.ShowError(clientEx.Parse(), "Error", "ok", null).ConfigureAwait(false);
            //}
            //catch (Exception ex)
            //{
            //    await _dialogService.ShowError(ex, "Error", "ok", null).ConfigureAwait(false);
            //}
        }

        protected virtual bool SaveCommandCanExecute()
        {
            return IsDirty;
        }

        public RelayCommand DeleteCommand => new(DeleteCommandExecute, DeleteCommandCanExecute);

        protected virtual void DeleteCommandExecute()
        {
            throw new NotImplementedException();
            //await _dialogService.ShowMessage("Are you sure...", "Delete", null, null, async confirm =>
            //{
            //    if (confirm)
            //    {
            //        await DeleteAsyncImpl(_dbService).ConfigureAwait(false);
            //        MessengerInstance.Send(new RemoveNodeMessage(AltLink));
            //        MessengerInstance.Send(new CloseDocumentMessage(this));
            //    }
            //}).ConfigureAwait(false);
        }

        protected virtual bool DeleteCommandCanExecute()
        {
            return !IsNewDocument;
        }
    }
}
