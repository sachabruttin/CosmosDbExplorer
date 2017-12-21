using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DocumentDbExplorer.Infrastructure.Models;

namespace DocumentDbExplorer.Views
{
    /// <summary>
    /// Interaction logic for StoredProcedureTabView.xaml
    /// </summary>
    public partial class StoredProcedureTabView : UserControl
    {
        public StoredProcedureTabView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PaneViewModel datacontext)
            {
                datacontext.IconSource = FindResource("StoredProcedureIcon") as ImageSource;
            }
        }
    }
}
