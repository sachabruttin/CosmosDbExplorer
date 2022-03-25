using System.Windows.Controls;

using CosmosDbExplorer.Contracts.Views;
using CosmosDbExplorer.ViewModels;

using MahApps.Metro.Controls;

namespace CosmosDbExplorer.Views
{
    public partial class ShellDialogWindow : MetroWindow, IShellDialogWindow
    {
        public ShellDialogWindow(ShellDialogViewModel viewModel)
        {
            InitializeComponent();
            viewModel.SetResult = OnSetResult;
            DataContext = viewModel;
        }

        public Frame GetDialogFrame()
            => dialogFrame;

        private void OnSetResult(bool? result)
        {
            DialogResult = result;
            Close();
        }
    }
}
