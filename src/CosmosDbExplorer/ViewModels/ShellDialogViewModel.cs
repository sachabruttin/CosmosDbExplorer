using System;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CosmosDbExplorer.ViewModels
{
    public class ShellDialogViewModel : ObservableObject
    {
        private ICommand? _closeCommand;

        public ICommand CloseCommand => _closeCommand ??= new RelayCommand(OnClose);

        public Action<bool?>? SetResult { get; set; }

        public ShellDialogViewModel()
        {
        }

        private void OnClose()
        {
            if (SetResult is not null)
            {
                var result = true;
                SetResult(result);
            }
        }
    }
}
