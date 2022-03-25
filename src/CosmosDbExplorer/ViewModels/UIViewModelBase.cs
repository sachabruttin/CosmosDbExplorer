using CosmosDbExplorer.Contracts.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using PropertyChanged;

namespace CosmosDbExplorer.ViewModels
{
    public abstract class UIViewModelBase : ObservableRecipient
    {
        private readonly IUIServices _uiServices;

        protected UIViewModelBase(IUIServices uiServices)
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
