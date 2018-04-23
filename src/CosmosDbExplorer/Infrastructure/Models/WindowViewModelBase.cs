using System;
using CosmosDbExplorer.Services;
using GalaSoft.MvvmLight.Messaging;

namespace CosmosDbExplorer.Infrastructure.Models
{
    public abstract class WindowViewModelBase : UIViewModelBase
    {
        public event Action RequestClose;

        protected WindowViewModelBase(IMessenger messenger, IUIServices uiServices) 
            : base(messenger, uiServices)
        {
        }

        public virtual void Close()
        {
            RequestClose?.Invoke();
        }
    }
}
