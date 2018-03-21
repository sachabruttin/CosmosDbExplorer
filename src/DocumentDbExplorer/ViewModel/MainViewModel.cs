using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Timers;
using DocumentDbExplorer.Infrastructure;
using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.Messages;
using DocumentDbExplorer.Services;
using DocumentDbExplorer.ViewModel.Assets;
using DocumentDbExplorer.ViewModel.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;

namespace DocumentDbExplorer.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly DatabaseViewModel _databaseViewModel;
        private readonly ISimpleIoc _ioc;
        private RelayCommand _showAboutCommand;
        private RelayCommand _showAccountSettingsCommand;
        private RelayCommand _exitCommand;
        private IEnumerable<ToolViewModel> _tools;
        private RelayCommand _refreshCommand;

        public event Action RequestClose;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        /// <param name="dialogService"></param>
        /// <param name="messenger"></param>
        /// <param name="ioc"></param>
        public MainViewModel(IDialogService dialogService, IMessenger messenger, ISimpleIoc ioc)
            : base(messenger)
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
                Title = "Design Mode";
            }
            else
            {
                // Code runs "for real"
                Title = "DocumentDB Explorer";
            }

            _dialogService = dialogService;
            _ioc = ioc;
            _databaseViewModel = _ioc.GetInstance<DatabaseViewModel>();
            Tabs = new ObservableCollection<PaneViewModelBase>();

            SpyUsedMemory();

            RegisterMessages();
        }

        private void SpyUsedMemory()
        {
            var timer = new Timer(TimeSpan.FromSeconds(3).TotalMilliseconds);
            timer.Elapsed += (s, e) => base.RaisePropertyChanged(() => UsedMemory);
            timer.Start();
        }

        private void RegisterMessages()
        {
            MessengerInstance.Register<ActivePaneChangedMessage>(this, OnActivePaneChanged);

            MessengerInstance.Register<OpenDocumentsViewMessage>(this, msg => OpenOrSelectTab<DocumentsTabViewModel, DocumentNodeViewModel>(msg));
            MessengerInstance.Register<OpenQueryViewMessage>(this, msg => OpenOrSelectTab<QueryEditorViewModel, CollectionNodeViewModel>(msg));
            MessengerInstance.Register<OpenImportDocumentViewMessage>(this, msg => OpenOrSelectTab<ImportDocumentViewModel, CollectionNodeViewModel>(msg));
            MessengerInstance.Register<OpenScaleAndSettingsViewMessage>(this, msg => OpenOrSelectTab<ScaleAndSettingsTabViewModel, ScaleSettingsNodeViewModel>(msg));
            MessengerInstance.Register<EditUserMessage>(this, msg => OpenOrSelectTab<UserEditViewModel, UserNodeViewModel>(msg));
            MessengerInstance.Register<EditPermissionMessage>(this, msg => OpenOrSelectTab<PermissionEditViewModel, PermissionNodeViewModel>(msg));
            MessengerInstance.Register<OpenCollectionMetricsViewMessage>(this, msg => OpenOrSelectTab<CollectionMetricsTabViewModel, CollectionMetricsNodeViewModel>(msg));

            MessengerInstance.Register<EditStoredProcedureMessage>(this, msg => OpenOrSelectTab<StoredProcedureTabViewModel, StoredProcedureNodeViewModel>(msg));
            MessengerInstance.Register<EditUserDefFuncMessage>(this, msg => OpenOrSelectTab<UserDefFuncTabViewModel, UserDefFuncNodeViewModel>(msg));
            MessengerInstance.Register<EditTriggerMessage>(this, msg => OpenOrSelectTab<TriggerTabViewModel, TriggerNodeViewModel>(msg));

            MessengerInstance.Register<TreeNodeSelectedMessage>(this, OnTreeNodeSelected);
            MessengerInstance.Register<CloseDocumentMessage>(this, CloseDocument);
            MessengerInstance.Register<IsBusyMessage>(this, msg => IsBusy = msg.IsBusy);
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
            Collection = (message.Item as IHaveCollectionNodeViewModel)?.CollectionNode;
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
                var content = _ioc.GetInstance<TTabViewModel>(contentId);
                content.Load(contentId, message.Node, message.Connection, message.Collection);

                Tabs.Add(content);
                SelectedTab = content;
            }
        }

        private void CloseDocument(CloseDocumentMessage msg)
        {
            DispatcherHelper.RunAsync(() =>
            {
                Tabs.Remove(msg.PaneViewModel);
                SelectedTab = Tabs.LastOrDefault();
            });
        }

        public string Title { get; set; }

        public long UsedMemory => GC.GetTotalMemory(true) / 1014;

        public bool IsBusy { get; set; }

        public double Zoom { get; set; }

        public ObservableCollection<PaneViewModelBase> Tabs { get; }

        public IEnumerable<ToolViewModel> Tools
        {
            get
            {
                return _tools
                     ?? (_tools = new ToolViewModel[] { _databaseViewModel });
            }
        }

        public PaneViewModelBase SelectedTab { get; set; }

        public void OnSelectedTabChanged()
        {
            IsTabDocumentsVisible = SelectedTab is DocumentsTabViewModel;
            IsSettingsTabVisible = SelectedTab is ScaleAndSettingsTabViewModel;
            IsAssetTabVisible = SelectedTab is IAssetTabCommand;
            IsQueryTabVisible = SelectedTab is QueryEditorViewModel;
            IsImportTabVisible = SelectedTab is ImportDocumentViewModel;
            IsQuerySettingsVisible = SelectedTab is IHaveQuerySettings;
            IsRequestOptionsVisible = SelectedTab is IHaveRequestOptions;
            IsConnectionOptionsVisible = false; // Only visible when selecting a tab
            IsRefreshTabVisible = SelectedTab is ICanRefreshTab;
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

        public ConnectionNodeViewModel Connection { get; set; }
        public DatabaseNodeViewModel Database { get; set; }
        public CollectionNodeViewModel Collection { get; set; }
        public UsersNodeViewModel Users { get; set; }
        public UserNodeViewModel UserNode { get; set; }
        public ICanRefreshNode CanRefreshNodeViewModel { get; set; }
        public ICanEditDelete CanEditDelete { get; set; }

        public RelayCommand ShowAboutCommand
        {
            get
            {
                return _showAboutCommand
                    ?? (_showAboutCommand = new RelayCommand(
                    async () =>
                    {
                        var fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
                        var name = ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyTitleAttribute), false))?.Title ?? "Unknown Title";
                        await _dialogService.ShowMessageBox($"{name}\nVersion {fvi.FileVersion}", "About").ConfigureAwait(false);
                    }));
            }
        }

        public RelayCommand ShowAccountSettingsCommand
        {
            get
            {
                return _showAccountSettingsCommand
                    ?? (_showAccountSettingsCommand = new RelayCommand(
                    () =>
                    {
                        var form = new Views.AccountSettingsView();
                        var vm = (AccountSettingsViewModel)form.DataContext;
                        vm.SetConnection(new Connection(Guid.NewGuid()));

                        var result = form.ShowDialog();
                    }));
            }
        }

        public RelayCommand ExitCommand
        {
            get
            {
                return _exitCommand
                    ?? (_exitCommand = new RelayCommand(Close));
            }
        }

        public RelayCommand RefreshCommand
        {
            get
            {
                return _refreshCommand
                    ?? (_refreshCommand = new RelayCommand(
                        () => CanRefreshNodeViewModel.RefreshCommand.Execute(null),
                        () => CanRefreshNodeViewModel?.RefreshCommand.CanExecute(null) == true
                        ));
            }
        }

        public virtual void Close()
        {
            RequestClose?.Invoke();
        }
    }
}
