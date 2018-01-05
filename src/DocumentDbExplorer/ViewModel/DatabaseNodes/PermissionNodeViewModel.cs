using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Models;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.ViewModel
{
    public class PermissionNodeViewModel : TreeViewItemViewModel, ICanRefreshNode
    {
        private readonly Permission _permission;
        private RelayCommand _refreshCommand;

        public PermissionNodeViewModel(Permission permission, UserNodeViewModel parent)
            : base(parent, parent.MessengerInstance, false)
        {
            _permission = permission;
        }

        public string Name => _permission.Id;

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
    }
}
