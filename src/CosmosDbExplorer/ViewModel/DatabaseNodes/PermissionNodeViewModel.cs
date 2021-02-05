using CosmosDbExplorer.Infrastructure;
using CosmosDbExplorer.Infrastructure.Models;
using CosmosDbExplorer.Messages;
using Microsoft.Azure.Documents;

namespace CosmosDbExplorer.ViewModel
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

        public override string Name => Permission?.Id;

        public string ContentId => Permission?.AltLink ?? "NewPermission";

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
