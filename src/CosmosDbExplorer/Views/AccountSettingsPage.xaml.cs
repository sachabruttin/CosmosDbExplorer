using System.Windows.Controls;

using CosmosDbExplorer.Helpers;
using CosmosDbExplorer.ViewModels;

using MahApps.Metro.Controls;

namespace CosmosDbExplorer.Views
{
    /// <summary>
    /// Interaction logic for AccountSettingsPage.xaml
    /// </summary>
    public partial class AccountSettingsPage : Page
    {
        public AccountSettingsPage(AccountSettingsViewModel viewModel)
        {
            InitializeComponent();
            viewModel.SetResult = OnSetResult;
            DataContext = viewModel;
        }

        private void OnSetResult(bool? result)
        {
            var window = UIHelper.FindVisualParent<MetroWindow>(this);

            window?.Close();
        }
    }
}
