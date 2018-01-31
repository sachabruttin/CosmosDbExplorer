using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DocumentDbExplorer.Infrastructure.Models;

namespace DocumentDbExplorer.Views
{
    /// <summary>
    /// Interaction logic for PermissionEditView.xaml
    /// </summary>
    public partial class PermissionEditView : UserControl
    {
        public PermissionEditView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as PaneViewModel;
            vm.IconSource = FindResource("PermissionIcon") as ImageSource;
        }
    }
}
