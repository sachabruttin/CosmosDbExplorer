using System.Threading.Tasks;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Messages;
using DocumentDbExplorer.Services;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.ViewModel
{
    public class TriggerRootNodeViewModel : TreeViewItemViewModel, ICanRefreshNode, IHaveCollectionNodeViewModel
    {
        private readonly IDocumentDbService _dbService;
        private RelayCommand _refreshCommand;

        public TriggerRootNodeViewModel(CollectionNodeViewModel parent)
            : base(parent, parent.MessengerInstance, true)
        {
            Name = "Triggers";
            _dbService = SimpleIoc.Default.GetInstance<IDocumentDbService>();
        }

        public string Name { get; }

        public new CollectionNodeViewModel Parent
        {
            get { return base.Parent as CollectionNodeViewModel; }
        }

        protected override async Task LoadChildren()
        {
            IsLoading = true;

            var _triggers = await _dbService.GetTriggers(Parent.Parent.Parent.Connection, Parent.Collection);

            foreach (var trigger in _triggers)
            {
                await DispatcherHelper.RunAsync(() => Children.Add(new TriggerNodeViewModel(this, trigger)));
            }

            IsLoading = false;
        }

        public RelayCommand RefreshCommand
        {
            get
            {
                return _refreshCommand
                    ?? (_refreshCommand = new RelayCommand(
                        async x =>
                        {
                            Children.Clear();
                            await LoadChildren();
                        }));
            }
        }

        public CollectionNodeViewModel CollectionNode => Parent;
    }

    public class TriggerNodeViewModel : TreeViewItemViewModel, ICanEditDelete
    {
        private RelayCommand _deleteCommand;
        private readonly IDialogService _dialogService;
        private readonly IDocumentDbService _dbService;
        private RelayCommand _editCommand;

        public TriggerNodeViewModel(TriggerRootNodeViewModel parent, Trigger trigger)
            : base(parent, parent.MessengerInstance, false)
        {
            Trigger = trigger;
            _dialogService = SimpleIoc.Default.GetInstance<IDialogService>();
            _dbService = SimpleIoc.Default.GetInstance<IDocumentDbService>();
        }

        public string Name => Trigger?.Id;

        public string ContentId => Trigger?.SelfLink;

        public Trigger Trigger { get; }

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
                        async x =>
                        {
                            await _dialogService.ShowMessage("Are sure you want to delete this Trigger?", "Delete", null, null,
                                async confirm =>
                                {
                                    if (confirm)
                                    {
                                        await _dbService.DeleteTrigger(Parent.Parent.Parent.Parent.Connection, Trigger.SelfLink);
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
                        x => MessengerInstance.Send(new EditTriggerMessage(Parent.Parent, this))));
            }
        }
    }
}
