﻿using System;
using CosmosDbExplorer.Infrastructure;
using CosmosDbExplorer.Infrastructure.Models;
using CosmosDbExplorer.Messages;

namespace CosmosDbExplorer.ViewModel
{

    public class CollectionMetricsNodeViewModel : TreeViewItemViewModel<CollectionNodeViewModel>, IHaveCollectionNodeViewModel, IContent
    {
        private RelayCommand _openCommand;

        public CollectionMetricsNodeViewModel(CollectionNodeViewModel parent)
            : base(parent, parent.MessengerInstance, false)
        {
            Name = "Collection Metrics";
        }

        public string Name { get; set; }

        public RelayCommand OpenCommand
        {
            get
            {
                return _openCommand
                    ?? (_openCommand = new RelayCommand(
                        () => {
                            IsSelected = false;
                            MessengerInstance.Send(new OpenCollectionMetricsViewMessage(this, Parent.Parent.Parent.Connection, Parent.Collection));
                        }));
            }
        }

        public CollectionNodeViewModel CollectionNode => Parent;

        public string ContentId => Parent.Collection.SelfLink + "/Metrics";
    }
}
