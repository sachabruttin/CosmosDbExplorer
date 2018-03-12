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
    public class UsersNodeViewModel : TreeViewItemViewModel<DatabaseNodeViewModel>, ICanRefreshNode
    {
        private readonly Database _database;
        private readonly DatabaseNodeViewModel _parent;
        private readonly IDocumentDbService _dbService;
        private RelayCommand _refreshCommand;
        private RelayCommand _addUserCommand;

        public UsersNodeViewModel(Database database, DatabaseNodeViewModel parent)
                : base(parent, parent.MessengerInstance, true)
        {
            Name = "Users";
            _database = database;
            _parent = parent;
            _dbService = SimpleIoc.Default.GetInstance<IDocumentDbService>();
        }

        public string Name { get; set; }

        public Database Database => _database;

        public RelayCommand RefreshCommand
        {
            get
            {
                return _refreshCommand
                    ?? (_refreshCommand = new RelayCommand(
                        async () =>
                        {
                            await DispatcherHelper.RunAsync(async () =>
                            {
                                Children.Clear();
                                await LoadChildren();
                            });
                        }));
            }
        }

        public RelayCommand AddUserCommand
        {
            get
            {
                return _addUserCommand ?? (_addUserCommand = new RelayCommand(
                    () => MessengerInstance.Send(new EditUserMessage(new UserNodeViewModel(new User(), this), Parent.Parent.Connection, null)
                    )));
            }
        }


        protected override async Task LoadChildren()
        {
            IsLoading = true;

            var users = await _dbService.GetUsersAsync(Parent.Parent.Connection, _database).ConfigureAwait(false);

            await DispatcherHelper.RunAsync(() =>
            {
                foreach (var user in users)
                {
                    Children.Add(new UserNodeViewModel(user, this));
                }
            });

            IsLoading = false;
        }
    }
}
