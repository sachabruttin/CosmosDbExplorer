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



        public bool UseFolding
        {
            get { return (bool)GetValue(UseFoldingProperty); }
            set { SetValue(UseFoldingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UseFolding.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UseFoldingProperty =
            DependencyProperty.Register("UseFolding", typeof(bool), typeof(JsonEditorView), new PropertyMetadata(false, OnUseFoldingChanged));

        private static void OnUseFoldingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (bool)e.NewValue;
            var target = (JsonEditorView)d;
            target.foldingBehavior.UseFolding = value;
        }



        public int FoldingInterval
        {
            get { return (int)GetValue(FoldingIntervalProperty); }
            set { SetValue(FoldingIntervalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FoldingInterval.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FoldingIntervalProperty =
            DependencyProperty.Register("FoldingInterval", typeof(int), typeof(JsonEditorView), new PropertyMetadata(1000, OnFoldingIntervalChanged));

        private static void OnFoldingIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (int)e.NewValue;
            var target = (JsonEditorView)d;
            target.foldingBehavior.Interval = value;
        }
    }
}
