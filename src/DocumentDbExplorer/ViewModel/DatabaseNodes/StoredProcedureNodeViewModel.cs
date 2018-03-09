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

            var _storedProcedure = await _dbService.GetStoredProceduresAsync(Parent.Parent.Parent.Connection, Parent.Collection).ConfigureAwait(false);

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
                        async () =>
                        {
                            Children.Clear();
                            await LoadChildren().ConfigureAwait(false);
                        }));
            }
        }

        public CollectionNodeViewModel CollectionNode => Parent;
    }

    public class StoredProcedureNodeViewModel : TreeViewItemViewModel, ICanEditDelete, IAssetNode<StoredProcedure>
    {
        private RelayCommand _deleteCommand;
        private readonly IDialogService _dialogService;
        private readonly IDocumentDbService _dbService;
        private RelayCommand _editCommand;

        public StoredProcedureNodeViewModel(StoredProcedureRootNodeViewModel parent, StoredProcedure storedProcedure)
            : base(parent, parent.MessengerInstance, false)
        {
            Resource = storedProcedure;
            _dialogService = SimpleIoc.Default.GetInstance<IDialogService>();
            _dbService = SimpleIoc.Default.GetInstance<IDocumentDbService>();
        }

        public string Name => Resource.Id;

        public string ContentId => Resource.AltLink;

        public Color? AccentColor => Parent.Parent.Parent.Parent.Connection.AccentColor;

        public new StoredProcedureRootNodeViewModel Parent
        {
            get { return base.Parent as StoredProcedureRootNodeViewModel; }
        }

        public StoredProcedure Resource { get; }

        public RelayCommand DeleteCommand
        {
            get
            {
                return _deleteCommand
                    ?? (_deleteCommand = new RelayCommand(
                        async () =>
                        {
                            await _dialogService.ShowMessage("Are sure you want to delete this Stored Procedure?", "Delete", null, null,
                                async confirm =>
                                {
                                    if (confirm)
                                    {
                                        await _dbService.DeleteStoredProcedureAsync(Parent.Parent.Parent.Parent.Connection, Resource.AltLink).ConfigureAwait(false);
                                        await DispatcherHelper.RunAsync(() => Parent.Children.Remove(this));
                                    }
                                }).ConfigureAwait(false);
                        }));
            }
        }

        public RelayCommand EditCommand
        {
            get
            {
                return _editCommand
                    ?? (_editCommand = new RelayCommand(
                        () => MessengerInstance.Send(new EditStoredProcedureMessage(Parent.Parent, this))));
            }
        }
    }
}
