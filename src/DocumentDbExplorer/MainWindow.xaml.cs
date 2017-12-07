using DocumentDbExplorer.ViewModel;

namespace DocumentDbExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = (MainViewModel)DataContext;
            vm.RequestClose += () => Close();
        }
    }
}
