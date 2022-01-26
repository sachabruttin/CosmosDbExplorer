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
using CosmosDbExplorer.Helpers;
using CosmosDbExplorer.ViewModels;
using MahApps.Metro.Controls;

namespace CosmosDbExplorer.Views
{
    /// <summary>
    /// Interaction logic for ContainerPropertyPage.xaml
    /// </summary>
    public partial class ContainerPropertyPage : Page
    {
        public ContainerPropertyPage(ContainerPropertyViewModel viewModel)
        {
            InitializeComponent();
            viewModel.SetResult = OnSetResult;
            DataContext = viewModel;
        }

        private void OnSetResult(bool? result)
        {
            var view = UIHelper.FindVisualParent<SplitView>(this);

            if (view != null)
            {
                view.IsPaneOpen = false;
            }
        }
    }
}
