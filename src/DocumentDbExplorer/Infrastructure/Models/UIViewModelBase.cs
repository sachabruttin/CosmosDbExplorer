using System;
using DocumentDbExplorer.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

namespace DocumentDbExplorer.Infrastructure.Models
{

    public abstract class UIViewModelBase : ViewModelBase
    {
        private readonly IUIServices _uiServices;

        protected UIViewModelBase(IMessenger messenger, IUIServices uiServices) 
            : base(messenger)
        {
            _uiServices = uiServices;
        }

        public bool IsBusy { get; set; }

        protected void OnIsBusyChanged()
        {
            _uiServices.SetBusyState(IsBusy);
        }
    }
}
