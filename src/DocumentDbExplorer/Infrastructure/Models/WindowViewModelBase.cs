using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

namespace DocumentDbExplorer.Infrastructure.Models
{
    public class WindowViewModelBase : ViewModelBase
    {
        public event Action RequestClose;

        public WindowViewModelBase(IMessenger messenger) : base(messenger)
        {
        }

        public virtual void Close()
        {
            RequestClose?.Invoke();
        }
    }
}
