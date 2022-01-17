using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Messages;
using CosmosDbExplorer.ViewModels.DatabaseNodes;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace CosmosDbExplorer.ViewModels
{
    public class MainViewModel : ObservableRecipient
    {
        private IEnumerable<ToolViewModel> _tools;
        private readonly DatabaseViewModel _databaseViewModel;
        private readonly IServiceProvider _serviceProvider;

        public MainViewModel(DatabaseViewModel databaseViewModel, IServiceProvider serviceProvider)
        {
            _databaseViewModel = databaseViewModel;
            _serviceProvider = serviceProvider;

            SpyUsedMemory();
            RegisterMessages();
        }
        public string Title { get; set; }

        public long UsedMemory => GC.GetTotalMemory(true) / 1014;

        public bool IsBusy { get; set; }

        public double Zoom { get; set; }

        public ObservableCollection<PaneViewModelBase> Tabs { get; } = new ObservableCollection<PaneViewModelBase>();

        //public IEnumerable<ToolViewModel> Tools => _tools ??= new ToolViewModel[] { _databaseViewModel };
        public IEnumerable<ToolViewModel> Tools => new ToolViewModel[] { _databaseViewModel };

        public PaneViewModelBase SelectedTab { get; set; }

        public void OnSelectedTabChanged()
        {
            //IsTabDocumentsVisible = SelectedTab is DocumentsTabViewModel;
            //IsSettingsTabVisible = SelectedTab is ScaleAndSettingsTabViewModel;
            //IsAssetTabVisible = SelectedTab is IAssetTabCommand;
            //IsQueryTabVisible = SelectedTab is QueryEditorViewModel || SelectedTab is StoredProcedureTabViewModel;
            //IsImportTabVisible = SelectedTab is ImportDocumentViewModel;
            //IsQuerySettingsVisible = SelectedTab is IHaveQuerySettings;
            //IsSystemPropertiesVisible = SelectedTab is IHaveSystemProperties;
            //IsRequestOptionsVisible = SelectedTab is IHaveRequestOptions;
            //IsConnectionOptionsVisible = false; // Only visible when selecting a tab
            //IsRefreshTabVisible = SelectedTab is ICanRefreshTab;
        }

        public int SelectedRibbonTab { get; set; }
        public bool IsConnectionOptionsVisible { get; set; }
        public bool IsTabDocumentsVisible { get; set; }
        public bool IsSettingsTabVisible { get; set; }
        public bool IsAssetTabVisible { get; set; }
        public bool IsQueryTabVisible { get; set; }
        public bool IsImportTabVisible { get; set; }
        public bool IsQuerySettingsVisible { get; set; }
        public bool IsRequestOptionsVisible { get; set; }
        public bool IsRefreshTabVisible { get; set; }
        public bool IsSystemPropertiesVisible { get; set; }

        public ConnectionNodeViewModel Connection { get; set; }
        public DatabaseNodeViewModel Database { get; set; }
        public ContainerNodeViewModel Collection { get; set; }
        public UsersNodeViewModel Users { get; set; }
        public UserNodeViewModel UserNode { get; set; }
        public ICanRefreshNode CanRefreshNodeViewModel { get; set; }
        public ICanEditDelete CanEditDelete { get; set; }

        public RelayCommand ShowAboutCommand => throw new NotImplementedException();
        //{
        //    get
        //    {
        //        return _showAboutCommand
        //            ?? (_showAboutCommand = new RelayCommand(
        //            async () =>
        //            {
        //                var fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
        //                var name = ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyTitleAttribute), false))?.Title ?? "Unknown Title";
        //                await _dialogService.ShowMessageBox($"{name}\nVersion {fvi.FileVersion}", "About").ConfigureAwait(false);
        //            }));
        //    }
        //}

        public RelayCommand ShowAccountSettingsCommand => throw new NotImplementedException();
        //{
        //    get
        //    {
        //        return _showAccountSettingsCommand
        //            ?? (_showAccountSettingsCommand = new RelayCommand(
        //            () =>
        //            {
        //                var form = new Views.AccountSettingsView();
        //                var vm = (AccountSettingsViewModel)form.DataContext;
        //                vm.SetConnection(new Connection(Guid.NewGuid()));

        //                var result = form.ShowDialog();
        //            }));
        //    }
        //}

        public RelayCommand RefreshCommand => throw new NotImplementedException();
        //{
        //    get
        //    {
        //        return _refreshCommand
        //            ?? (_refreshCommand = new RelayCommand(
        //                () => CanRefreshNodeViewModel.RefreshCommand.Execute(null),
        //                () => CanRefreshNodeViewModel?.RefreshCommand.CanExecute(null) == true
        //                ));
        //    }
        //}

        public RelayCommand ExitCommand => new(Close);

        public virtual void Close()
        {
            throw new NotImplementedException();
            //RequestClose?.Invoke();
        }

        private void SpyUsedMemory()
        {
            var timer = new Timer(TimeSpan.FromSeconds(3).TotalMilliseconds);
            timer.Elapsed += (s, e) => OnPropertyChanged(nameof(UsedMemory));
            timer.Start();
        }

        private void RegisterMessages()
        {
            Messenger.Register<MainViewModel, ActivePaneChangedMessage>(this, static (r, msg) => r.OnActivePaneChanged(msg));

            Messenger.Register<MainViewModel, OpenDocumentsViewMessage>(this, static (r, msg) => r.OpenOrSelectTab<DocumentsTabViewModel, DocumentNodeViewModel>(msg));
            Messenger.Register<MainViewModel, OpenQueryViewMessage>(this, static (r, msg) => r.OpenOrSelectTab<QueryEditorViewModel, ContainerNodeViewModel>(msg));
            //Messenger.Register<MainViewModel, OpenImportDocumentViewMessage>(this, static (r, msg) => r.OpenOrSelectTab<ImportDocumentViewModel, CollectionNodeViewModel>(msg));
            //Messenger.Register<MainViewModel, OpenScaleAndSettingsViewMessage>(this, static (r, msg) => r.OpenOrSelectTab<ScaleAndSettingsTabViewModel, ScaleSettingsNodeViewModel>(msg));
            //Messenger.Register<MainViewModel, EditUserMessage>(this, static (r, msg) => r.OpenOrSelectTab<UserEditViewModel, UserNodeViewModel>(msg));
            //Messenger.Register<MainViewModel, EditPermissionMessage>(this, static (r, msg) => r.OpenOrSelectTab<PermissionEditViewModel, PermissionNodeViewModel>(msg));
            //Messenger.Register<MainViewModel, OpenMetricsViewMessage>(this, static (r, msg) => r.OpenOrSelectTab<CollectionMetricsTabViewModel, CollectionMetricsNodeViewModel>(msg));

            //Messenger.Register<MainViewModel, EditStoredProcedureMessage>(this, static (r, msg) => r.OpenOrSelectTab<StoredProcedureTabViewModel, StoredProcedureNodeViewModel>(msg));
            //Messenger.Register<MainViewModel, EditUserDefFuncMessage>(this, static(r, msg) => r.OpenOrSelectTab<UserDefFuncTabViewModel, UserDefFuncNodeViewModel>(msg));
            //Messenger.Register<MainViewModel, EditTriggerMessage>(this, static (r, msg) => r.OpenOrSelectTab<TriggerTabViewModel, TriggerNodeViewModel>(msg));

            Messenger.Register<MainViewModel, TreeNodeSelectedMessage>(this, static(r, msg) => r.OnTreeNodeSelected(msg));
            Messenger.Register<MainViewModel, CloseDocumentMessage>(this, static (r, msg) => r.CloseDocument(msg));
            Messenger.Register<MainViewModel, IsBusyMessage>(this, static (r, msg) => r.IsBusy = msg.IsBusy);
        }

        private void OnActivePaneChanged(ActivePaneChangedMessage message)
        {
            if (message.PaneViewModel is DatabaseViewModel)
            {
                IsRequestOptionsVisible = false;
                IsConnectionOptionsVisible = true;
                SelectedRibbonTab = 1;
            }
            else
            {
                IsConnectionOptionsVisible = ShouldConnectionOptionBeVisible();
                OnSelectedTabChanged();
                SelectedRibbonTab = 0;
            }
        }

        private void OnTreeNodeSelected(TreeNodeSelectedMessage message)
        {
            CanRefreshNodeViewModel = message.Item as ICanRefreshNode;
            Connection = message.Item as ConnectionNodeViewModel;
            Database = message.Item as DatabaseNodeViewModel;
            Collection = (message.Item as IHaveContainerNodeViewModel)?.ContainerNode;
            Users = message.Item as UsersNodeViewModel;
            UserNode = message.Item as UserNodeViewModel;
            CanEditDelete = message.Item as ICanEditDelete;

            IsConnectionOptionsVisible = ShouldConnectionOptionBeVisible();
        }

        private bool ShouldConnectionOptionBeVisible()
        {
            return CanRefreshNodeViewModel != null
                                    || Connection != null
                                    || Database != null
                                    || Collection != null
                                    || CanEditDelete != null
                                    || Users != null
                                    || UserNode != null;
        }

        private void OpenOrSelectTab<TTabViewModel, TNodeViewModel>(OpenTabMessageBase<TNodeViewModel> message)
            where TTabViewModel : PaneViewModel<TNodeViewModel>
            where TNodeViewModel : TreeViewItemViewModel, IContent
        {
            var contentId = message.Node?.ContentId ?? Guid.NewGuid().ToString();
            var tab = Tabs.FirstOrDefault(t => t.ContentId == contentId);

            if (tab != null)
            {
                SelectedTab = tab;
            }
            else
            {
                var content = _serviceProvider.GetService<TTabViewModel>();
                //var content = SimpleIoc.Default.GetInstanceWithoutCaching<TTabViewModel>(contentId); //_ioc.GetInstance<TTabViewModel>(contentId);
                content.Load(contentId, message.Node, message.Connection, message.Container);

                Tabs.Add(content);
                SelectedTab = content;
            }
        }

        private void CloseDocument(CloseDocumentMessage msg)
        {
            //DispatcherHelper.RunAsync(() =>
            //{
                var vm = Tabs.FirstOrDefault(t => t.ContentId == msg.ContentId);

                if (vm != null)
                {
                    Tabs.Remove(vm);
                    //_ioc.Unregister(vm);
                    vm = null;
                    SelectedTab = Tabs.LastOrDefault();
                }
            //});
        }
    } 
}
