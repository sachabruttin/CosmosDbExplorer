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
    public class UserDefFuncRootNodeViewModel : TreeViewItemViewModel, ICanRefreshNode, IHaveCollectionNodeViewModel
    {
        private readonly IDocumentDbService _dbService;
        private RelayCommand _refreshCommand;

        public UserDefFuncRootNodeViewModel(CollectionNodeViewModel parent)
            : base(parent, parent.MessengerInstance, true)
        {
            Name = "User Defined Functions";
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

            var _function = await _dbService.GetUdfs(Parent.Parent.Parent.Connection, Parent.Collection);

            foreach (var func in _function)
            {
                await DispatcherHelper.RunAsync(() => Children.Add(new UserDefFuncNodeViewModel(this, func)));
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
                            await LoadChildren().ConfigureAwait(false);
                        }));
            }
        }

        public CollectionNodeViewModel CollectionNode => Parent;
    }

    public class UserDefFuncNodeViewModel : TreeViewItemViewModel, ICanEditDelete
    {
        private RelayCommand _deleteCommand;
        private readonly IDialogService _dialogService;
        private readonly IDocumentDbService _dbService;
        private RelayCommand _editCommand;

        public UserDefFuncNodeViewModel(UserDefFuncRootNodeViewModel parent, UserDefinedFunction function)
            : base(parent, parent.MessengerInstance, false)
        {
            Function = function;
            _dialogService = SimpleIoc.Default.GetInstance<IDialogService>();
            _dbService = SimpleIoc.Default.GetInstance<IDocumentDbService>();
        }

        public string Name => Function?.Id;

        public string ContentId => Function?.AltLink;

        public new UserDefFuncRootNodeViewModel Parent
        {
            get { return base.Parent as UserDefFuncRootNodeViewModel; }
        }

        public UserDefinedFunction Function { get; }

        public RelayCommand DeleteCommand
        {
            get
            {
                return _deleteCommand
                    ?? (_deleteCommand = new RelayCommand(
                        async x =>
                        {
                            await _dialogService.ShowMessage("Are sure you want to delete this User Definied Function?", "Delete", null, null,
                                async confirm =>
                                {
                                    if (confirm)
                                    {
                                        await _dbService.DeleteUdf(Parent.Parent.Parent.Parent.Connection, Function.AltLink);
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
                        x => MessengerInstance.Send(new EditUserDefFuncMessage(Parent.Parent, this))));
            }
        }
    }
}
