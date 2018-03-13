using System;
using DocumentDbExplorer.Services;
using GalaSoft.MvvmLight.Messaging;

namespace DocumentDbExplorer.Infrastructure.Models
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
