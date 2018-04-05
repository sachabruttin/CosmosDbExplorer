using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace CosmosDbExplorer.Views
{
    /// <summary>
    /// Interaction logic for AboutView.xaml
    /// </summary>
    public partial class AboutView : UserControl
    {
        public AboutView()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Hyperlink_RequestNavigate_Github(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo($"https://github.com/{e.Uri.OriginalString}"));
            e.Handled = true;
        }
    }
}
