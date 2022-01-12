using System;
using System.Collections.ObjectModel;
using System.Linq;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Core.Contracts.Services;
using CosmosDbExplorer.Messages;
using CosmosDbExplorer.ViewModels;
using CosmosDbExplorer.ViewModels.DatabaseNodes;
using GongSolutions.Wpf.DragDrop;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace CosmosDbExplorer.ViewModel
{
    public class DatabaseViewModel : ToolViewModel, IDropTarget
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICosmosClientService _cosmosClientService;

        public DatabaseViewModel(IServiceProvider serviceProvider, IUIServices uiServices, ICosmosClientService cosmosClientService) 
            : base(uiServices)
        {
            Header = "Connections";
            Title = Header;
            IsVisible = true;
            _serviceProvider = serviceProvider;
            _cosmosClientService = cosmosClientService;

            RegisterMessages();
        }

        //private readonly ISettingsService _settingsService;

        //public DatabaseViewModel(IMessenger messenger, IDocumentDbService dbService, ISettingsService settingsService, IUIServices uiServices)
        //    : base(messenger, uiServices)
        //{
        //    Header = "Connections";
        //    Title = Header;
        //    IsVisible = true;

        //    _settingsService = settingsService;

        //    RegisterMessages();
        //}

        private void RegisterMessages()
        {
            Messenger.Register<DatabaseViewModel, ConnectionSettingSavedMessage>(this, static (r, msg) => r.OnConnectionSettingsSaved(msg));
            Messenger.Register<DatabaseViewModel, RemoveConnectionMessage>(this, static (r, msg) => r.OnRemoveConnection(msg));
        }

        public ObservableCollection<ConnectionNodeViewModel> Nodes { get; private set; }

        public void LoadNodes()
        {
            var nodes = App.Connections.Select(c => new ConnectionNodeViewModel(_serviceProvider, c));

            Nodes = new ObservableCollection<ConnectionNodeViewModel>(nodes);
        }

        private void OnRemoveConnection(RemoveConnectionMessage msg)
        {
            var node = Nodes.FirstOrDefault(n => n.Connection == msg.Connection);

            if (node != null)
            {
                //DispatcherHelper.RunAsync(() => Nodes.Remove(node));
                Nodes.Remove(node);
            }
        }

        private void OnConnectionSettingsSaved(ConnectionSettingSavedMessage msg)
        {
            var node = Nodes.FirstOrDefault(n => n.Connection.Equals(msg.Connection));

            if (node != null)
            {
                node.Connection = msg.Connection;
            }
            else
            {
                //var connection = SimpleIoc.Default.GetInstanceWithoutCaching<ConnectionNodeViewModel>();
                var connection = _serviceProvider.GetService<ConnectionNodeViewModel>();
                connection.Connection = msg.Connection;
                Nodes.Add(connection);
            }
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data != dropInfo.TargetItem && dropInfo.Data is ConnectionNodeViewModel && dropInfo.TargetItem is ConnectionNodeViewModel)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = System.Windows.DragDropEffects.Move;
            }
        }

        public async void Drop(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data as ConnectionNodeViewModel;
            var targetItem = dropInfo.TargetItem as ConnectionNodeViewModel;

            if (sourceItem == targetItem)
            {
                return;
            }

            var sourceIndex = Nodes.IndexOf(sourceItem);
            var targetIndex = Nodes.IndexOf(targetItem);

            switch (dropInfo.InsertPosition)
            {
                case RelativeInsertPosition.None:
                    return;
                case RelativeInsertPosition.BeforeTargetItem:
                    if (sourceIndex + 1 == targetIndex)
                    {
                        return;
                    }
                    else if (targetIndex != 0)
                    {
                        targetIndex--;
                    }
                    break;
                case RelativeInsertPosition.AfterTargetItem:
                    if (sourceIndex - 1 == targetIndex)
                    {
                        return;
                    }
                    else if (targetIndex == Nodes.Count)
                    {
                        targetIndex--;
                    }
                    break;
                case RelativeInsertPosition.TargetItemCenter:
                    return;
            }

            Nodes.Move(sourceIndex, targetIndex);
            //await _settingsService.ReorderConnections(sourceIndex, targetIndex).ConfigureAwait(false);
        }
    }
}
