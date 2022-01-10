using System;
using CosmosDbExplorer.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using PropertyChanged;

namespace CosmosDbExplorer.Infrastructure.Models
{

    public abstract class UIViewModelBase : ViewModelBase
    {
        private readonly IUIServices _uiServices;

        protected UIViewModelBase(IMessenger messenger, IUIServices uiServices)
            : base(messenger)
        {
            _uiServices = uiServices;
        }

        [DoNotSetChanged]
        public bool IsBusy { get; set; }

        protected virtual void OnIsBusyChanged()
        {
            _uiServices.SetBusyState(IsBusy);
        }
    }
}
