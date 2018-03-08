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
            Tabs = new ObservableCollection<PaneViewModel>();

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
            MessengerInstance.Register<OpenDocumentsViewMessage>(this, OpenDocumentsView);
            MessengerInstance.Register<OpenQueryViewMessage>(this, OpenQueryView);
            MessengerInstance.Register<OpenImportDocumentViewMessage>(this, OpenImportDocumentView);
            MessengerInstance.Register<CloseDocumentMessage>(this, CloseDocument);
            MessengerInstance.Register<EditStoredProcedureMessage>(this, OpenStoredProcedure);
            MessengerInstance.Register<EditUserDefFuncMessage>(this, OpenUserDefFunc);
            MessengerInstance.Register<EditTriggerMessage>(this, OpenTrigger);
            MessengerInstance.Register<OpenScaleAndSettingsViewMessage>(this, OpenScaleAndSettings);
            MessengerInstance.Register<EditUserMessage>(this, OpenEditUser);
            MessengerInstance.Register<EditPermissionMessage>(this, OpenPermission);

            MessengerInstance.Register<TreeNodeSelectedMessage>(this, OnTreeNodeSelected);
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

        private void OpenScaleAndSettings(OpenScaleAndSettingsViewMessage message)
        {
            //var contentId = message?.Node?.ContentId ?? Guid.NewGuid().ToString();
            var tab = Tabs.FirstOrDefault(t => t.ContentId == contentId);

            if (tab != null)
            {
                SelectedTab = tab;
            }
            else
            {
                var content = _ioc.GetInstance<ScaleAndSettingsTabViewModel>(contentId);
                content.Node = message.Node;
                content.ContentId = contentId;

                Tabs.Add(content);
                SelectedTab = content;
            }
        }

        private void OpenTrigger(EditTriggerMessage message)
        {
            var contentId = message?.Node?.ContentId ?? Guid.NewGuid().ToString();
            var tab = Tabs.FirstOrDefault(t => t.ContentId == contentId);

            if (tab != null)
            {
                SelectedTab = tab;
            }
            else
            {
                var content = _ioc.GetInstance<TriggerTabViewModel>(contentId);
                content.Node = message.Node;
                content.ContentId = contentId;
                content.Connection = message.Connection;
                content.Collection = message.Collection;

                Tabs.Add(content);
                SelectedTab = content;
            }
        }

        private void OpenEditUser(EditUserMessage message)
        {
            var contentId = message?.Node?.ContentId ?? Guid.NewGuid().ToString();
            var tab = Tabs.FirstOrDefault(t => t.ContentId == contentId);

            if (tab != null)
            {
                SelectedTab = tab;
            }
            else
            {
                var content = _ioc.GetInstance<UserEditViewModel>(contentId);
                content.Node = message.Node;

                Tabs.Add(content);
                SelectedTab = content;
            }
        }

        private void OpenPermission(EditPermissionMessage message)
        {
            var contentId = message?.Node?.ContentId ?? Guid.NewGuid().ToString();
            var tab = Tabs.FirstOrDefault(t => t.ContentId == contentId);

            if (tab != null)
            {
                SelectedTab = tab;
            }
            else
            {
                var content = _ioc.GetInstance<PermissionEditViewModel>(contentId);
                content.Node = message.Node;

                Tabs.Add(content);
                SelectedTab = content;
            }
        }

        private void OpenUserDefFunc(EditUserDefFuncMessage message)
        {
            var contentId = message?.Node?.ContentId ?? Guid.NewGuid().ToString();
            var tab = Tabs.FirstOrDefault(t => t.ContentId == contentId);

            if (tab != null)
            {
                SelectedTab = tab;
            }
            else
            {
                var content = _ioc.GetInstance<UserDefFuncTabViewModel>(contentId);
                content.Node = message.Node;
                content.ContentId = contentId;
                content.Connection = message.Connection;
                content.Collection = message.Collection;

                Tabs.Add(content);
                SelectedTab = content;
            }
        }

        private void OpenStoredProcedure(EditStoredProcedureMessage message)
        {
            var contentId = message?.Node?.ContentId ?? Guid.NewGuid().ToString();
            var tab = Tabs.FirstOrDefault(t => t.ContentId == contentId);

            if (tab != null)
            {
                SelectedTab = tab;
            }
            else
            {
                var content = _ioc.GetInstance<StoredProcedureTabViewModel>(contentId);
                content.Node = message.Node;
                content.ContentId = contentId;
                content.Connection = message.Connection;
                content.Collection = message.Collection;

                Tabs.Add(content);
                SelectedTab = content;
            }
        }

        private void CloseDocument(CloseDocumentMessage msg)
        {
            DispatcherHelper.RunAsync(() =>
            {
                Tabs.Remove(msg.Paneviewmodel);
                SelectedTab = Tabs.LastOrDefault();
            });
        }

        private async void OpenDocumentsView(OpenDocumentsViewMessage message)
        {
            var contentId = $"Documents:{message.Node.Parent.Collection.AltLink}";
            var tab = Tabs.FirstOrDefault(t => t.ContentId == contentId && t is DocumentsTabViewModel);

            if (tab != null)
            {
                SelectedTab = tab;
            }
            else
            {
                var content = _ioc.GetInstance<DocumentsTabViewModel>(contentId);
                content.Node = message.Node;
                content.ContentId = contentId;
                
                Tabs.Add(content);
                SelectedTab = content;

                await content.LoadDocuments(true);
            }
        }

        private void OpenQueryView(OpenQueryViewMessage message)
        {
            var content = _ioc.GetInstance<QueryEditorViewModel>(Guid.NewGuid().ToString());
            content.Node = message.Node;
            Tabs.Add(content);
            SelectedTab = content;
        }

        private void OpenImportDocumentView(OpenImportDocumentViewMessage message)
        {
            var contentId = $"Import:{message.Node.Collection.AltLink}";
            var tab = Tabs.FirstOrDefault(t => t.ContentId == contentId && t is ImportDocumentViewModel);

            if (tab != null)
            {
                SelectedTab = tab;
            }
            else
            {
                var content = _ioc.GetInstance<ImportDocumentViewModel>(contentId);
                content.Node = message.Node;
                content.ContentId = contentId;

                Tabs.Add(content);
                SelectedTab = content;
            }
        }

        public string Title { get; set; }

        public long UsedMemory => GC.GetTotalMemory(true) / 1014;

        public double Zoom { get; set; }

        public ObservableCollection<PaneViewModel> Tabs { get; }

        public IEnumerable<ToolViewModel> Tools
        {
            get
            {
                return _tools
                     ?? (_tools = new ToolViewModel[] { _databaseViewModel });

            }
        }
        public PaneViewModel SelectedTab { get; set; }

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
                    async x =>
                    {
                        var fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
                        var name = ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyTitleAttribute), false))?.Title ?? "Unknown Title";
                        await _dialogService.ShowMessageBox($"{name}\nVersion {fvi.FileVersion}", "About");
                    }));
            }
        }
        public RelayCommand ShowAccountSettingsCommand
        {
            get
            {
                return _showAccountSettingsCommand
                    ?? (_showAccountSettingsCommand = new RelayCommand(
                    x =>
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
                    ?? (_exitCommand = new RelayCommand(
                        x =>
                        {
                            Close();
                        }));
            }
        }

        public RelayCommand RefreshCommand
        {
            get
            {
                return _refreshCommand
                    ?? (_refreshCommand = new RelayCommand(
                        x =>
                        {
                            CanRefreshNodeViewModel.RefreshCommand.Execute(x);
                        },
                        x => CanRefreshNodeViewModel != null && CanRefreshNodeViewModel.RefreshCommand.CanExecute(x)                            
                        ));
            }
        }

        public virtual void Close()
        {
            RequestClose?.Invoke();
        }

    }
}
