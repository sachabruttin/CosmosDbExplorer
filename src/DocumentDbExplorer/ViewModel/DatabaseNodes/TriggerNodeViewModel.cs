using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Messages;
using DocumentDbExplorer.Services;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.ViewModel
{
    public class TriggerRootNodeViewModel : AssetRootNodeViewModelBase<Trigger>
    {
        public TriggerRootNodeViewModel(CollectionNodeViewModel parent)
            : base(parent)
        {
            Name = "Triggers";
        }

        protected override async Task LoadChildren()
        {
            IsLoading = true;

            var _triggers = await DbService.GetTriggersAsync(Parent.Parent.Parent.Connection, Parent.Collection).ConfigureAwait(false);

            foreach (var trigger in _triggers)
            {
                await DispatcherHelper.RunAsync(() => Children.Add(new TriggerNodeViewModel(this, trigger)));
            }

            IsLoading = false;
        }

        protected override void OnUpdateOrCreateNodeMessage(UpdateOrCreateNodeMessage<Trigger> message)
        {
            if (message.IsNewResource)
            {
                var item = new TriggerNodeViewModel(this, message.Resource);
                DispatcherHelper.RunAsync(() => Children.Add(item));
            }
            else
            {
                var item = Children.Cast<TriggerNodeViewModel>().FirstOrDefault(i => i.Resource.AltLink == message.OldAltLink);

                if (item != null)
                {
                    item.Resource = message.Resource;
                }
            }
        }
    }

    public class TriggerNodeViewModel : TreeViewItemViewModel, ICanEditDelete, IAssetNode<Trigger>
    {
        private RelayCommand _deleteCommand;
        private readonly IDialogService _dialogService;
        private readonly IDocumentDbService _dbService;
        private RelayCommand _editCommand;

        public TriggerNodeViewModel(TriggerRootNodeViewModel parent, Trigger trigger)
            : base(parent, parent.MessengerInstance, false)
        {
            Resource = trigger;
            _dialogService = SimpleIoc.Default.GetInstance<IDialogService>();
            _dbService = SimpleIoc.Default.GetInstance<IDocumentDbService>();
        }

        public string Name => Resource?.Id;

        public string ContentId => Resource.AltLink;

        public Color? AccentColor => Parent.Parent.Parent.Parent.Connection.AccentColor;

        public Trigger Resource { get; set; }

        public new TriggerRootNodeViewModel Parent
        {
            get { return base.Parent as TriggerRootNodeViewModel; }
        }

        public RelayCommand DeleteCommand
        {
            get
            {
                return _deleteCommand
                    ?? (_deleteCommand = new RelayCommand(
                        async () =>
                        {
                            await _dialogService.ShowMessage("Are sure you want to delete this Trigger?", "Delete", null, null,
                                async confirm =>
                                {
                                    if (confirm)
                                    {
                                        await _dbService.DeleteTriggerAsync(Parent.Parent.Parent.Parent.Connection, Resource.AltLink).ConfigureAwait(false);
                                        await DispatcherHelper.RunAsync(() => Parent.Children.Remove(this));
                                    }
                                });
                        }));
            }
        }

        public RelayCommand EditCommand
        {
            get
            {
                return _editCommand
                    ?? (_editCommand = new RelayCommand(
                        () => MessengerInstance.Send(new EditTriggerMessage(Parent.Parent, this))));
            }
        }
    }
}
