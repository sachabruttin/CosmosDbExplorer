using System;
using System.Threading.Tasks;
using System.Windows.Input;

using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Core.Contracts;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Messages;
using CosmosDbExplorer.Models;

using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

using PropertyChanged;

namespace CosmosDbExplorer.ViewModels.Assets
{
    public abstract class AssetTabViewModelBase<TNode, TResource> : PaneWithZoomViewModel<TNode>, IAssetTabCommand
        where TNode : TreeViewItemViewModel, IAssetNode<TResource>
        where TResource : ICosmosScript
    {
        //private readonly IDialogService _dialogService;
        //private readonly IServiceProvider _serviceProvider;
        private readonly IDialogService _dialogService;
        private RelayCommand? _discardCommand;
        private AsyncRelayCommand? _saveCommand;
        private AsyncRelayCommand? _deleteCommand;

        protected AssetTabViewModelBase(IUIServices uiServices, IDialogService dialogService, string contentId, NodeContext<TNode> nodeContext)
            : base(uiServices, contentId, nodeContext)
        {
            Content = GetDefaultContent();
            _dialogService = dialogService;
            Header = GetDefaultHeader();
            Title = GetDefaultTitle();

            if (nodeContext.Node is null || nodeContext.Connection is null || nodeContext.Container is null || nodeContext.Database is null)
            {
                throw new NullReferenceException("Node context is not correctly initialized!");
            }
            
            Node = nodeContext.Node;
            Connection = nodeContext.Connection;
            Container = nodeContext.Container;
            AccentColor = Connection.AccentColor;

            //var databaseNode = ((DatabaseNodes.DatabaseNodeViewModel)Node.Parent.Parent.Parent);
            ToolTip = $"{Connection.Label}/{nodeContext.Database.Id}/{Container.Id}";
            SetInformation(Node.Resource);
        }

        protected abstract string GetDefaultHeader();
        protected abstract string GetDefaultTitle();
        protected abstract string GetDefaultContent();
        protected virtual void SetInformationImpl(TResource resource)
        {
            SetText(resource.Body);
        }

        [OnChangedMethod(nameof(UpdateCommandStatus))]
        protected string? AltLink { get; set; }

        [OnChangedMethod(nameof(UpdateCommandStatus))]
        public string Content { get; set; }

        public TNode Node { get; protected set; }
        
        protected void SetInformation(TResource? resource)
        {
            if (resource != null)
            {
                Id = resource.Id;
                AltLink = resource.SelfLink;
                ContentId = AltLink ?? string.Empty;
                Header = resource.Id ?? string.Empty;
                SetInformationImpl(resource);
            }
        }

        public CosmosConnection Connection { get; set; }

        public CosmosContainer Container { get; set; }

        public string? Id { get; set; }

        [OnChangedMethod(nameof(UpdateCommandStatus))]
        public bool IsDirty { get; set; }

        public bool IsNewDocument => AltLink == null;

        protected void SetText(string content)
        {
            Content = content;
            IsDirty = false;
        }

        public ICommand DiscardCommand => _discardCommand ??= new(DiscardCommandExecute, DiscardCommandCanExecute);

        protected virtual void DiscardCommandExecute()
        {
            if (IsNewDocument)
            {
                SetText(GetDefaultContent());
            }
            else
            {
                SetInformation(Node.Resource);
            }
        }

        protected virtual bool DiscardCommandCanExecute()
        {
            return IsDirty;
        }

        public ICommand SaveCommand => _saveCommand ??= new(SaveCommandExecute, SaveCommandCanExecute);

        protected virtual async Task SaveCommandExecute()
        {
            try
            {
                var resource = await SaveAsyncImpl();
                Messenger.Send(new UpdateOrCreateNodeMessage<TResource, CosmosContainer>(resource, Container, AltLink));
                SetInformation(resource);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowError(ex, "Error");
            }
        }

        protected virtual bool SaveCommandCanExecute() => IsDirty;

        protected abstract Task<TResource> SaveAsyncImpl();

        public ICommand DeleteCommand => _deleteCommand ??= new(DeleteCommandExecute, DeleteCommandCanExecute);

        protected virtual async Task DeleteCommandExecute()
        {
            await _dialogService.ShowQuestion("Are you sure...", "Delete", async confirm =>
            {
                if (confirm)
                {
                    await DeleteAsyncImpl();
                    Messenger.Send(new RemoveNodeMessage(AltLink));
                    Messenger.Send(new CloseDocumentMessage(this));
                }
            });
        }

        protected abstract Task<CosmosResult> DeleteAsyncImpl();
        protected virtual bool DeleteCommandCanExecute() => !IsNewDocument;

        protected virtual void UpdateCommandStatus()
        {
            ((AsyncRelayCommand)SaveCommand).NotifyCanExecuteChanged();
            ((AsyncRelayCommand)DeleteCommand).NotifyCanExecuteChanged();
            ((RelayCommand)DiscardCommand).NotifyCanExecuteChanged();
        }
    }
}
