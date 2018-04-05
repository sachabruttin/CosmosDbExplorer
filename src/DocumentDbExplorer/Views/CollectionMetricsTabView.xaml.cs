using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CosmosDbExplorer.Infrastructure.Models;

namespace CosmosDbExplorer.Views
{
    /// <summary>
    /// Interaction logic for CollectionMetricsTabView.xaml
    /// </summary>
    public partial class CollectionMetricsTabView : UserControl
    {
        public CollectionMetricsTabView()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is PaneViewModelBase datacontext)
            {
                datacontext.IconSource = FindResource("ChartIcon") as ImageSource;
            }
        }
    }
}
