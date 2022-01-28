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
using CosmosDbExplorer.ViewModels;

namespace CosmosDbExplorer.Views
{
    /// <summary>
    /// Interaction logic for DatabaseView.xaml
    /// </summary>
    public partial class DatabaseView : UserControl
    {
        public DatabaseView()
        {
            InitializeComponent();
            DataContextChanged += DatabaseView_DataContextChanged;
        }

        private void DatabaseView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = (DatabaseViewModel)DataContext;
            vm.LoadNodes();
        }
    }
}
