﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using System.Windows.Input;

using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Contracts.ViewModels;
using CosmosDbExplorer.Messages;
using CosmosDbExplorer.ViewModels.DatabaseNodes;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using CosmosDbExplorer.ViewModels.Assets;

namespace CosmosDbExplorer.ViewModels
{
    public class ShellViewModel : ObservableRecipient
    {
        private readonly IRightPaneService _rightPaneService;
        private readonly IApplicationInfoService _applicationInfoService;
        private readonly DatabaseViewModel _databaseViewModel;
        private readonly IServiceProvider _serviceProvider;
        private ICommand _loadedCommand;
        private ICommand _unloadedCommand;

        public ICommand LoadedCommand => _loadedCommand ?? (_loadedCommand = new RelayCommand(OnLoaded));

        public ICommand UnloadedCommand => _unloadedCommand ?? (_unloadedCommand = new RelayCommand(OnUnloaded));

        public ShellViewModel(IRightPaneService rightPaneService, IApplicationInfoService applicationInfoService, DatabaseViewModel databaseViewModel, IServiceProvider serviceProvider)
        {
            _rightPaneService = rightPaneService;
            _applicationInfoService = applicationInfoService;
            _databaseViewModel = databaseViewModel;
            _serviceProvider = serviceProvider;
            SpyUsedMemory();
            RegisterMessages();
        }

        private void OnLoaded()
        {
        }

        private void OnUnloaded()
        {
            _rightPaneService.CleanUp();
        }

        public string Title { get; set; }

        public long UsedMemory => GC.GetTotalMemory(true) / 1014;

        public bool IsBusy { get; set; }

        public double Zoom { get; set; }

        public ObservableCollection<PaneViewModelBase> Tabs { get; } = new ObservableCollection<PaneViewModelBase>();

        public IEnumerable<ToolViewModel> Tools => new ToolViewModel[] { _databaseViewModel };

        public PaneViewModelBase? SelectedTab { get; set; }

        public void OnSelectedTabChanged()
        {
            IsTabDocumentsVisible = SelectedTab is DocumentsTabViewModel;
            //IsSettingsTabVisible = SelectedTab is ScaleAndSettingsTabViewModel;
            IsAssetTabVisible = SelectedTab is IAssetTabCommand;
            IsQueryTabVisible = SelectedTab is QueryEditorViewModel || SelectedTab is StoredProcedureTabViewModel;
            //IsImportTabVisible = SelectedTab is ImportDocumentViewModel;
            IsQuerySettingsVisible = SelectedTab is QueryEditorViewModel;
            IsSystemPropertiesVisible = SelectedTab is IHaveSystemProperties;
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
        public bool IsSystemPropertiesVisible { get; set; }

        public ConnectionNodeViewModel? Connection { get; set; }
        public DatabaseNodeViewModel? Database { get; set; }
        public ContainerNodeViewModel? Collection { get; set; }
        public UsersNodeViewModel? Users { get; set; }
        public UserNodeViewModel? UserNode { get; set; }
        public ICanRefreshNode? CanRefreshNodeViewModel { get; set; }
        public ICanEditDelete? CanEditDelete { get; set; }

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

        public RelayCommand RefreshCommand => new(() => CanRefreshNodeViewModel?.RefreshCommand.Execute(null), () =>
        {
            if (CanRefreshNodeViewModel?.RefreshCommand == null)
            {
                return false;
            }

            return CanRefreshNodeViewModel.RefreshCommand.CanExecute(null);
        }); 

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
            Messenger.Register<ShellViewModel, ActivePaneChangedMessage>(this, static (r, msg) => r.OnActivePaneChanged(msg));

            Messenger.Register<ShellViewModel, OpenDocumentsViewMessage>(this, static (r, msg) => r.OpenOrSelectTab<DocumentsTabViewModel, DocumentNodeViewModel>(msg));
            Messenger.Register<ShellViewModel, OpenQueryViewMessage>(this, static (r, msg) => r.OpenOrSelectTab<QueryEditorViewModel, ContainerNodeViewModel>(msg));
            //Messenger.Register<ShellViewModel, OpenImportDocumentViewMessage>(this, static (r, msg) => r.OpenOrSelectTab<ImportDocumentViewModel, CollectionNodeViewModel>(msg));
            //Messenger.Register<ShellViewModel, OpenScaleAndSettingsViewMessage>(this, static (r, msg) => r.OpenOrSelectTab<ScaleAndSettingsTabViewModel, ScaleSettingsNodeViewModel>(msg));
            //Messenger.Register<ShellViewModel, EditUserMessage>(this, static (r, msg) => r.OpenOrSelectTab<UserEditViewModel, UserNodeViewModel>(msg));
            //Messenger.Register<ShellViewModel, EditPermissionMessage>(this, static (r, msg) => r.OpenOrSelectTab<PermissionEditViewModel, PermissionNodeViewModel>(msg));
            //Messenger.Register<ShellViewModel, OpenMetricsViewMessage>(this, static (r, msg) => r.OpenOrSelectTab<CollectionMetricsTabViewModel, CollectionMetricsNodeViewModel>(msg));

            Messenger.Register<ShellViewModel, EditStoredProcedureMessage>(this, static (r, msg) => r.OpenOrSelectTab<StoredProcedureTabViewModel, StoredProcedureNodeViewModel>(msg));
            Messenger.Register<ShellViewModel, EditUserDefFuncMessage>(this, static (r, msg) => r.OpenOrSelectTab<UserDefFuncTabViewModel, UserDefFuncNodeViewModel>(msg));
            Messenger.Register<ShellViewModel, EditTriggerMessage>(this, static (r, msg) => r.OpenOrSelectTab<TriggerTabViewModel, TriggerNodeViewModel>(msg));

            Messenger.Register<ShellViewModel, TreeNodeSelectedMessage>(this, static (r, msg) => r.OnTreeNodeSelected(msg));
            Messenger.Register<ShellViewModel, CloseDocumentMessage>(this, static (r, msg) => r.CloseDocument(msg));
            Messenger.Register<ShellViewModel, IsBusyMessage>(this, static (r, msg) => r.IsBusy = msg.IsBusy);
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
            if (message is null || message.Item is null)
            {
                return;
            }

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
            if (message is null || message.Node is null)
            {
                throw new Exception("Node is null!");
            }

            var contentId = message.Node?.ContentId ?? Guid.NewGuid().ToString();

            var tab = Tabs.FirstOrDefault(t => t.ContentId == contentId);

            if (tab != null)
            {
                SelectedTab = tab;
            }
            else
            {
                var content = _serviceProvider.GetService<TTabViewModel>();

                if (content != null)
                {
                    content.Load(contentId, message.Node, message.Connection, message.Container);

                    Tabs.Add(content);
                    SelectedTab = content;
                }
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
