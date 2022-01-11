using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CosmosDbExplorer.Messages;
using CosmosDbExplorer.Services;
using GongSolutions.Wpf.DragDrop;
using CosmosDbExplorer.ViewModels;
using Microsoft.Toolkit.Mvvm.Messaging;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.ViewModels.DatabaseNodes;
using Microsoft.Extensions.Options;
using CosmosDbExplorer.Models;
using CosmosDbExplorer.Core.Contracts.Services;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;

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

        //private void RegisterMessages()
        //{
        //    MessengerInstance.Register<ConnectionSettingSavedMessage>(this, OnConnectionSettingsSaved);
        //    MessengerInstance.Register<RemoveConnectionMessage>(this, OnRemoveConnection);
        //}

        public ObservableCollection<ConnectionNodeViewModel> Nodes { get; private set; }

        public void LoadNodes()
        {
            var nodes = App.Connections.Select(c => new ConnectionNodeViewModel(_serviceProvider, c.Value));

            Nodes = new ObservableCollection<ConnectionNodeViewModel>(nodes);
        }

        //private void OnRemoveConnection(RemoveConnectionMessage msg)
        //{
        //    var node = Nodes.FirstOrDefault(n => n.Connection == msg.Connection);

        //    if (node != null)
        //    {
        //        DispatcherHelper.RunAsync(() => Nodes.Remove(node));
        //    }
        //}

        //private void OnConnectionSettingsSaved(ConnectionSettingSavedMessage msg)
        //{
        //    var node = Nodes.FirstOrDefault(n => n.Connection.Equals(msg.Connection));

        //    if (node != null)
        //    {
        //        node.Connection = msg.Connection;
        //    }
        //    else
        //    {
        //        var connection = SimpleIoc.Default.GetInstanceWithoutCaching<ConnectionNodeViewModel>();
        //        connection.Connection = msg.Connection;
        //        Nodes.Add(connection);
        //    }
        //}

        public void DragOver(IDropInfo dropInfo)
        {
            //if (dropInfo.Data != dropInfo.TargetItem && dropInfo.Data is ConnectionNodeViewModel && dropInfo.TargetItem is ConnectionNodeViewModel)
            //{
            //    dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            //    dropInfo.Effects = System.Windows.DragDropEffects.Move;
            //}
        }

        public async void Drop(IDropInfo dropInfo)
        {
            //var sourceItem = dropInfo.Data as ConnectionNodeViewModel;
            //var targetItem = dropInfo.TargetItem as ConnectionNodeViewModel;

            //if (sourceItem == targetItem)
            //{
            //    return;
            //}

            //var sourceIndex = Nodes.IndexOf(sourceItem);
            //var targetIndex = Nodes.IndexOf(targetItem);

            //switch (dropInfo.InsertPosition)
            //{
            //    case RelativeInsertPosition.None:
            //        return;
            //    case RelativeInsertPosition.BeforeTargetItem:
            //        if (sourceIndex + 1 == targetIndex)
            //        {
            //            return;
            //        }
            //        else if (targetIndex != 0)
            //        {
            //            targetIndex--;
            //        }
            //        break;
            //    case RelativeInsertPosition.AfterTargetItem:
            //        if (sourceIndex - 1 == targetIndex)
            //        {
            //            return;
            //        }
            //        else if (targetIndex == Nodes.Count)
            //        {
            //            targetIndex--;
            //        }
            //        break;
            //    case RelativeInsertPosition.TargetItemCenter:
            //        return;
            //}

            //Nodes.Move(sourceIndex, targetIndex);
            //await _settingsService.ReorderConnections(sourceIndex, targetIndex).ConfigureAwait(false);
        }
    }
}
