using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CosmosDbExplorer.ViewModels;
using CosmosDbExplorer.Helpers;
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

            if (window != null)
            {
                window.Close();
            }
        }
    }
}
