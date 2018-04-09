using System.Windows;
using CosmosDbExplorer.ViewModel;

namespace CosmosDbExplorer.Views
{
    /// <summary>
    /// Interaction logic for AddCollectionView.xaml
    /// </summary>
    public partial class AddCollectionView : Window
    {
        public AddCollectionView()
        {
            InitializeComponent();
            Owner = App.Current.MainWindow;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as AddCollectionViewModel;
            vm.RequestClose += () =>
            {
                DialogResult = true;
                Close();
            };
        }
    }
}
