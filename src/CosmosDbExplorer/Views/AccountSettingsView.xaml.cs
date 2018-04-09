using System.Windows;
using CosmosDbExplorer.ViewModel;

namespace CosmosDbExplorer.Views
{
    /// <summary>
    /// Interaction logic for AccountSettingsControl.xaml
    /// </summary>
    public partial class AccountSettingsView 
    {
        public AccountSettingsView()
        {
            InitializeComponent();
            Owner = App.Current.MainWindow;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as AccountSettingsViewModel;
            vm.RequestClose += () =>
            {
                DialogResult = true;
                Close();
            };
        }
    }
}
