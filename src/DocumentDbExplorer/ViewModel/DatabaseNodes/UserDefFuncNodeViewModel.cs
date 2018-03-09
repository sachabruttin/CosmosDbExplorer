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
    public class UserDefFuncRootNodeViewModel : AssetRootNodeViewModelBase<UserDefinedFunction>
    {
        public UserDefFuncRootNodeViewModel(CollectionNodeViewModel parent)
            : base(parent)
        {
            Name = "User Defined Functions";
        }

        protected override async Task LoadChildren()
        {
            IsLoading = true;

            var _function = await DbService.GetUdfsAsync(Parent.Parent.Parent.Connection, Parent.Collection).ConfigureAwait(false);

            foreach (var func in _function)
            {
                await DispatcherHelper.RunAsync(() => Children.Add(new UserDefFuncNodeViewModel(this, func)));
            }

            IsLoading = false;
        }

        protected override void OnUpdateOrCreateNodeMessage(UpdateOrCreateNodeMessage<UserDefinedFunction> message)
        {
            if (message.IsNewResource)
            {
                var item = new UserDefFuncNodeViewModel(this, message.Resource);
                DispatcherHelper.RunAsync(() => Children.Add(item));
            }
            else
            {
                var item = Children.Cast<UserDefFuncNodeViewModel>().FirstOrDefault(i => i.Resource.AltLink == message.OldAltLink);

                if (item != null)
                {
                    item.Resource = message.Resource;
                }
            }
        }
    }

    public class UserDefFuncNodeViewModel : TreeViewItemViewModel, ICanEditDelete, IAssetNode<UserDefinedFunction>
    {
        private RelayCommand _deleteCommand;
        private readonly IDialogService _dialogService;
        private readonly IDocumentDbService _dbService;
        private RelayCommand _editCommand;

        public UserDefFuncNodeViewModel(UserDefFuncRootNodeViewModel parent, UserDefinedFunction function)
            : base(parent, parent.MessengerInstance, false)
        {
            Resource = function;
            _dialogService = SimpleIoc.Default.GetInstance<IDialogService>();
            _dbService = SimpleIoc.Default.GetInstance<IDocumentDbService>();
        }

        public string Name => Resource?.Id;

        public string ContentId => Resource?.AltLink;

        public Color? AccentColor => Parent.Parent.Parent.Parent.Connection.AccentColor;

        public new UserDefFuncRootNodeViewModel Parent
        {
            get { return base.Parent as UserDefFuncRootNodeViewModel; }
        }

        public UserDefinedFunction Resource { get; set; }

        public RelayCommand DeleteCommand
        {
            get
            {
                return _deleteCommand
                    ?? (_deleteCommand = new RelayCommand(
                        async () =>
                        {
                            await _dialogService.ShowMessage("Are sure you want to delete this User Definied Function?", "Delete", null, null,
                                async confirm =>
                                {
                                    if (confirm)
                                    {
                                        await _dbService.DeleteUdfAsync(Parent.Parent.Parent.Parent.Connection, Resource.AltLink);
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
                        () => MessengerInstance.Send(new EditUserDefFuncMessage(Parent.Parent, this))));
            }
        }
    }
}
