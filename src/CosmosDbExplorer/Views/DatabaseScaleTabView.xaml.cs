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
using CosmosDbExplorer.ViewModel;

namespace CosmosDbExplorer.Views
{
    /// <summary>
    /// Interaction logic for DatabaseScaleTabView.xaml
    /// </summary>
    public partial class DatabaseScaleTabView : UserControl
    {
        public DatabaseScaleTabView()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as DatabaseScaleTabViewModel;
            vm.IconSource = FindResource("ScaleSettingsIcon") as ImageSource;
            await vm.LoadDataAsync().ConfigureAwait(false);
        }
    }
}
