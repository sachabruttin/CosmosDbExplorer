using System.Windows.Controls;

using CosmosDbExplorer.ViewModels;

namespace CosmosDbExplorer.Views
{
    public partial class MainPage : Page
    {
        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
