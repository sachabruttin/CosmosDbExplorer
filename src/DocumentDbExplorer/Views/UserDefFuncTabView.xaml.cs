using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DocumentDbExplorer.Infrastructure.Models;

namespace DocumentDbExplorer.Views
{
    /// <summary>
    /// Interaction logic for UserDefFuncTabView.xaml
    /// </summary>
    public partial class UserDefFuncTabView : UserControl
    {
        public UserDefFuncTabView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PaneViewModel datacontext)
            {
                datacontext.IconSource = FindResource("UdfIcon") as ImageSource;
            }
        }
    }
}
