using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for AboutView.xaml
    /// </summary>
    public partial class AboutPage : Page
    {
        public AboutPage(AboutViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            OpenUrl(e.Uri);
            e.Handled = true;
        }

        private void Hyperlink_RequestNavigate_Github(object sender, RequestNavigateEventArgs e)
        {
            OpenUrl(new Uri($"https://github.com/{e.Uri.OriginalString}"));
            e.Handled = true;
        }

        private static void OpenUrl(Uri uri)
        {
            var psi = new ProcessStartInfo(uri.AbsoluteUri)
            {
                UseShellExecute = true
            };

            Process.Start(psi);
        }
    }
}
