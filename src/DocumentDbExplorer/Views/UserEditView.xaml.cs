using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CosmosDbExplorer.Infrastructure.Models;

namespace CosmosDbExplorer.Views
{
    /// <summary>
    /// Interaction logic for UserEditView.xaml
    /// </summary>
    public partial class UserEditView : UserControl
    {
        public UserEditView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as PaneViewModelBase;
            vm.IconSource = FindResource("UserIcon") as ImageSource;
        }
    }
}
