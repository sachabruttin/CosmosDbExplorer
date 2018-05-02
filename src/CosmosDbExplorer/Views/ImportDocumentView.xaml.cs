using System.Windows.Controls;
using CosmosDbExplorer.Infrastructure.Models;

namespace CosmosDbExplorer.Views
{
    /// <summary>
    /// Interaction logic for ImportDocumentView.xaml
    /// </summary>
    public partial class ImportDocumentView : UserControl
    {
        public ImportDocumentView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is PaneViewModelBase datacontext)
            {
                datacontext.IconSource = FindResource("ImportIcon") as TextBlock;
            }
        }
    }
}
