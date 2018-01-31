using System.Threading.Tasks;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Messages;
using DocumentDbExplorer.Services;
using DocumentDbExplorer.ViewModel.Interfaces;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.ViewModel
{
    public class UsersNodeViewModel : TreeViewItemViewModel, ICanRefreshNode
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

        public new DatabaseNodeViewModel Parent
        {
            get { return base.Parent as DatabaseNodeViewModel; }
        }

        public RelayCommand RefreshCommand
        {
            get
            {
                return _refreshCommand
                    ?? (_refreshCommand = new RelayCommand(
                        async x =>
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
                    x => MessengerInstance.Send(new EditUserMessage(new UserNodeViewModel(new User(), this))
                    )));
            }
        }


        protected override async Task LoadChildren()
        {
            IsLoading = true;

            var users = await _dbService.GetUsers(Parent.Parent.Connection, _database).ConfigureAwait(false);

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
