using System.Threading.Tasks;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Services;
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

        public UsersNodeViewModel(Database database, DatabaseNodeViewModel parent)
                : base(parent, parent.MessengerInstance, true)
        {
            Name = "Users";
            _database = database;
            _parent = parent;
            _dbService = SimpleIoc.Default.GetInstance<IDocumentDbService>();
        }

        public string Name { get; set; }

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
                            Children.Clear();
                            await LoadChildren();
                        }));
            }
        }

        protected override async Task LoadChildren()
        {
            IsLoading = true;

            var users = await _dbService.GetUsers(Parent.Parent.Connection, _database);

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
