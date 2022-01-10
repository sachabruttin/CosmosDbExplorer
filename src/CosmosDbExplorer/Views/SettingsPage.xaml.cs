using System.Windows.Controls;

using CosmosDbExplorer.ViewModels;

namespace CosmosDbExplorer.Views
{
    public partial class SettingsPage : Page
    {
        public SettingsPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
