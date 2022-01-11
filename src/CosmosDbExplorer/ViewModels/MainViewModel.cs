using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Timers;
using CosmosDbExplorer.ViewModel;
using CosmosDbExplorer.ViewModels.DatabaseNodes;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace CosmosDbExplorer.ViewModels
{
    public class MainViewModel : ObservableRecipient
    {
        private IEnumerable<ToolViewModel> _tools;
        private readonly DatabaseViewModel _databaseViewModel;

        public MainViewModel(IMessenger messenger, DatabaseViewModel databaseViewModel)
            : base(messenger)
        {
            _databaseViewModel = databaseViewModel;
            
            SpyUsedMemory();
            RegisterMessages();
        }
        public string Title { get; set; }

        public long UsedMemory => GC.GetTotalMemory(true) / 1014;

        public bool IsBusy { get; set; }

        public double Zoom { get; set; }

        public ObservableCollection<PaneViewModelBase> Tabs { get; } = new ObservableCollection<PaneViewModelBase>();

        public IEnumerable<ToolViewModel> Tools => _tools ??= new ToolViewModel[] { _databaseViewModel };

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
        //public DatabaseNodeViewModel Database { get; set; }
        //public CollectionNodeViewModel Collection { get; set; }
        //public UsersNodeViewModel Users { get; set; }
        //public UserNodeViewModel UserNode { get; set; }
        //public ICanRefreshNode CanRefreshNodeViewModel { get; set; }
        //public ICanEditDelete CanEditDelete { get; set; }

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

        public RelayCommand ExitCommand => new RelayCommand(Close);

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
            //    MessengerInstance.Register<ActivePaneChangedMessage>(this, OnActivePaneChanged);

            //    MessengerInstance.Register<OpenDocumentsViewMessage>(this, msg => OpenOrSelectTab<DocumentsTabViewModel, DocumentNodeViewModel>(msg));
            //    MessengerInstance.Register<OpenQueryViewMessage>(this, msg => OpenOrSelectTab<QueryEditorViewModel, CollectionNodeViewModel>(msg));
            //    MessengerInstance.Register<OpenImportDocumentViewMessage>(this, msg => OpenOrSelectTab<ImportDocumentViewModel, CollectionNodeViewModel>(msg));
            //    MessengerInstance.Register<OpenScaleAndSettingsViewMessage>(this, msg => OpenOrSelectTab<ScaleAndSettingsTabViewModel, ScaleSettingsNodeViewModel>(msg));
            //    MessengerInstance.Register<EditUserMessage>(this, msg => OpenOrSelectTab<UserEditViewModel, UserNodeViewModel>(msg));
            //    MessengerInstance.Register<EditPermissionMessage>(this, msg => OpenOrSelectTab<PermissionEditViewModel, PermissionNodeViewModel>(msg));
            //    MessengerInstance.Register<OpenCollectionMetricsViewMessage>(this, msg => OpenOrSelectTab<CollectionMetricsTabViewModel, CollectionMetricsNodeViewModel>(msg));

            //    MessengerInstance.Register<EditStoredProcedureMessage>(this, msg => OpenOrSelectTab<StoredProcedureTabViewModel, StoredProcedureNodeViewModel>(msg));
            //    MessengerInstance.Register<EditUserDefFuncMessage>(this, msg => OpenOrSelectTab<UserDefFuncTabViewModel, UserDefFuncNodeViewModel>(msg));
            //    MessengerInstance.Register<EditTriggerMessage>(this, msg => OpenOrSelectTab<TriggerTabViewModel, TriggerNodeViewModel>(msg));

            //    MessengerInstance.Register<TreeNodeSelectedMessage>(this, OnTreeNodeSelected);
            //    MessengerInstance.Register<CloseDocumentMessage>(this, CloseDocument);
            //    MessengerInstance.Register<IsBusyMessage>(this, msg => IsBusy = msg.IsBusy);
        }
    }
}
