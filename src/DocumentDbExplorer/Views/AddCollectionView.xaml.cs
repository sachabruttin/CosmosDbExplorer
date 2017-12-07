using System.Windows;
using DocumentDbExplorer.ViewModel;

namespace DocumentDbExplorer.Views
{
    /// <summary>
    /// Interaction logic for AddCollectionView.xaml
    /// </summary>
    public partial class AddCollectionView : Window
    {
        public AddCollectionView()
        {
            InitializeComponent();
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
