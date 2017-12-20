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
    public class StoredProcedureRootNodeViewModel : TreeViewItemViewModel, ICanRefreshNode, IHaveCollectionNodeViewModel
    {
        private readonly IDocumentDbService _dbService;
        private RelayCommand _refreshCommand;

        public StoredProcedureRootNodeViewModel(CollectionNodeViewModel parent)
            : base(parent, parent.MessengerInstance, true)
        {
            Name = "Stored Procedures";
            _dbService = SimpleIoc.Default.GetInstance<IDocumentDbService>();
        }

        public string Name { get; set; }

        public new CollectionNodeViewModel Parent
        {
            get { return base.Parent as CollectionNodeViewModel; }
        }

        protected override async Task LoadChildren()
        {
            IsLoading = true;

            var _storedProcedure = await _dbService.GetStoredProcedures(Parent.Parent.Parent.Connection, Parent.Collection);

            foreach (var sp in _storedProcedure)
            {
                await DispatcherHelper.RunAsync(() => Children.Add(new StoredProcedureNodeViewModel(this, sp)));
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

    public class StoredProcedureNodeViewModel : TreeViewItemViewModel, ICanEditDelete
    {
        private RelayCommand _deleteCommand;
        private readonly IDialogService _dialogService;
        private readonly IDocumentDbService _dbService;
        private RelayCommand _editCommand;

        public StoredProcedureNodeViewModel(StoredProcedureRootNodeViewModel parent, StoredProcedure storedProcedure)
            : base(parent, parent.MessengerInstance, false)
        {
            StoredProcedure = storedProcedure;
            _dialogService = SimpleIoc.Default.GetInstance<IDialogService>();
            _dbService = SimpleIoc.Default.GetInstance<IDocumentDbService>();
        }

        public string Name => StoredProcedure.Id;
        
        public string ContentId => StoredProcedure.SelfLink;

        public new StoredProcedureRootNodeViewModel Parent
        {
            get { return base.Parent as StoredProcedureRootNodeViewModel; }
        }
        
        public StoredProcedure StoredProcedure { get; }

        public RelayCommand DeleteCommand
        {
            get
            {
                return _deleteCommand
                    ?? (_deleteCommand = new RelayCommand(
                        async x =>
                        {
                            await _dialogService.ShowMessage("Are sure you want to delete this Stored Procedure?", "Delete", null, null,
                                async confirm =>
                                {
                                    if (confirm)
                                    {
                                        await _dbService.DeleteStoredProcedure(Parent.Parent.Parent.Parent.Connection, StoredProcedure.SelfLink);
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
                        x => MessengerInstance.Send(new EditStoredProcedureMessage(Parent.Parent, this))));
            }
        }
    }
}
