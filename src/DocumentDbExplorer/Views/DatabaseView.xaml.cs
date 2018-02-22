using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DocumentDbExplorer.ViewModel;

namespace DocumentDbExplorer.Views
{
    /// <summary>
    /// Interaction logic for DatabaseView.xaml
    /// </summary>
    public partial class DatabaseView : UserControl
    {
        public DatabaseView()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = (DatabaseViewModel)DataContext;
            vm.IconSource = FindResource("ConnectionIcon") as ImageSource;
            await vm.LoadNodesAsync();
        }
    }
}
