using System;
using System.Collections.ObjectModel;
using System.Linq;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Core.Contracts.Services;
using CosmosDbExplorer.Messages;
using CosmosDbExplorer.ViewModels;
using CosmosDbExplorer.ViewModels.DatabaseNodes;
using GongSolutions.Wpf.DragDrop;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace CosmosDbExplorer.ViewModels
{
    public class DatabaseViewModel : ToolViewModel, IDropTarget
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICosmosClientService _cosmosClientService;
        private readonly IPersistAndRestoreService _persistAndRestoreService;

        public DatabaseViewModel(IServiceProvider serviceProvider, IUIServices uiServices, ICosmosClientService cosmosClientService, IPersistAndRestoreService persistAndRestoreService)
            : base(uiServices)
        {
            Header = "Connections";
            Title = Header;
            IconSource = App.Current.FindResource("ConnectionIcon");
            IsVisible = true;
            _serviceProvider = serviceProvider;
            _cosmosClientService = cosmosClientService;
            _persistAndRestoreService = persistAndRestoreService;
            LoadNodes();
            RegisterMessages();
        }


        private void RegisterMessages()
        {
            Messenger.Register<DatabaseViewModel, ConnectionSettingSavedMessage>(this, static (r, msg) => r.OnConnectionSettingsSaved(msg));
            Messenger.Register<DatabaseViewModel, RemoveConnectionMessage>(this, static (r, msg) => r.OnRemoveConnection(msg));
        }

        public ObservableCollection<ConnectionNodeViewModel> Nodes { get; private set; }

        private void LoadNodes()
        {
            var connections = _persistAndRestoreService.GetConnections();
            var nodes = connections.Select(c => new ConnectionNodeViewModel(_serviceProvider, c));

            Nodes = new ObservableCollection<ConnectionNodeViewModel>(nodes);
        }

        private void OnRemoveConnection(RemoveConnectionMessage msg)
        {
            var node = Nodes.FirstOrDefault(n => n.Connection == msg.Connection);

            if (node != null)
            {
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
                var connection = new ConnectionNodeViewModel(_serviceProvider, msg.Connection);
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

        public void Drop(IDropInfo dropInfo)
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
            _persistAndRestoreService.ReorderConnections(sourceIndex, targetIndex);
        }
    }
}
