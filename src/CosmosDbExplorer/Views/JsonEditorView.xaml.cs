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

namespace CosmosDbExplorer.Views
{
    /// <summary>
    /// Interaction logic for JsonEditorView.xaml
    /// </summary>
    public partial class JsonEditorView : UserControl
    {
        public JsonEditorView()
        {
            InitializeComponent();
         }

        public static readonly DependencyProperty ZoomLevelProperty =
            DependencyProperty.Register("ZoomLevel", typeof(double), typeof(JsonEditorView), new PropertyMetadata(0.5d, OnZoomLevelChanged));

        public double ZoomLevel
        {
            get { return (double)GetValue(ZoomLevelProperty); }
            set { SetValue(ZoomLevelProperty, value); }
        }

        private static void OnZoomLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (double)e.NewValue;
            var target = (JsonEditorView)d;
            target.zoomBehavior.ZoomLevel = value;
        }
    }
}
