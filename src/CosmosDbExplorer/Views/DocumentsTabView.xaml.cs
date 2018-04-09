using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CosmosDbExplorer.Infrastructure.Models;

namespace CosmosDbExplorer.Views
{
    /// <summary>
    /// Interaction logic for DocumentsTabView.xaml
    /// </summary>
    public partial class DocumentsTabView : UserControl
    {
        public DocumentsTabView()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is PaneViewModelBase datacontext)
            {
                datacontext.IconSource = FindResource("DocumentIcon") as ImageSource;
            }
        }
    }
}
