using System.Windows;
using DocumentDbExplorer.ViewModel;

namespace DocumentDbExplorer.Views
{
    /// <summary>
    /// Interaction logic for AccountSettingsControl.xaml
    /// </summary>
    public partial class AccountSettingsView 
    {
        public AccountSettingsView()
        {
            InitializeComponent();
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
