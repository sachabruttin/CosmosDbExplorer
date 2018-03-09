﻿using DocumentDbExplorer.Infrastructure.Models;
using DocumentDbExplorer.ViewModel;
using Microsoft.Azure.Documents;

namespace DocumentDbExplorer.Messages
{
    public class EditUserDefFuncMessage : OpenTabMessageBase<UserDefFuncNodeViewModel>
    {
        public EditUserDefFuncMessage(UserDefFuncNodeViewModel node, Connection connection, DocumentCollection collection) 
            : base(node, connection, collection)
        {
        }
    }
}
