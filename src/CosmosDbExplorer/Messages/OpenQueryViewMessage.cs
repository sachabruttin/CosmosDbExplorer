﻿using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

namespace CosmosDbExplorer.Messages
{
    public class OpenQueryViewMessage : OpenTabMessageBase<ContainerNodeViewModel>
    {
        public OpenQueryViewMessage(ContainerNodeViewModel node, CosmosConnection connection, CosmosContainer container)
        : base(node, connection, container)
        {
        }
    }
}
