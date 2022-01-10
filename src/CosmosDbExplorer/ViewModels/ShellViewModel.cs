using System.Windows.Input;

using CosmosDbExplorer.Contracts.Services;

using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace CosmosDbExplorer.ViewModels
{
    public class ShellViewModel : ObservableObject
    {
        private readonly IRightPaneService _rightPaneService;
        private readonly IApplicationInfoService _applicationInfoService;
        private ICommand _loadedCommand;
        private ICommand _unloadedCommand;

        public ICommand LoadedCommand => _loadedCommand ?? (_loadedCommand = new RelayCommand(OnLoaded));

        public ICommand UnloadedCommand => _unloadedCommand ?? (_unloadedCommand = new RelayCommand(OnUnloaded));

        public ShellViewModel(IRightPaneService rightPaneService, IApplicationInfoService applicationInfoService)
        {
            _rightPaneService = rightPaneService;
            _applicationInfoService = applicationInfoService;
        }

        private void OnLoaded()
        {
        }

        private void OnUnloaded()
        {
            _rightPaneService.CleanUp();
        }
    }
}
