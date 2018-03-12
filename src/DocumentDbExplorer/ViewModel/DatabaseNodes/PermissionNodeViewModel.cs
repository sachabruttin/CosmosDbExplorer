using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Messages;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.ViewModel
{
    public class PermissionNodeViewModel : TreeViewItemViewModel<UserNodeViewModel>, ICanRefreshNode, IContent
    {
        private RelayCommand _refreshCommand;
        private RelayCommand _openCommand;

        public PermissionNodeViewModel(Permission permission, UserNodeViewModel parent)
            : base(parent, parent.MessengerInstance, false)
        {
            Permission = permission;
        }

        public Permission Permission { get; set; }

        public string Name => Permission.Id;

        public string ContentId => Permission.AltLink ?? "NewPermission";

        public RelayCommand RefreshCommand
        {
            get
            {
                return _refreshCommand
                    ?? (_refreshCommand = new RelayCommand(
                        async () =>
                        {
                            Children.Clear();
                            await LoadChildren();
                        }));
            }
        }

        public RelayCommand OpenCommand
        {
            get
            {
                return _openCommand ?? (_openCommand = new RelayCommand(
                    () => MessengerInstance.Send(new EditPermissionMessage(this, Parent.Parent.Parent.Parent.Connection, null))));
            }
        }
    }
}
