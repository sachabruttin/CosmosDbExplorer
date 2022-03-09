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

using CosmosDbExplorer.Properties;

namespace CosmosDbExplorer.Views
{
    /// <summary>
    /// Interaction logic for QueryEditor.xaml
    /// </summary>
    public partial class QueryEditorView : UserControl
    {
        public QueryEditorView()
        {
            InitializeComponent();

            Properties.Settings.Default.PropertyChanged += Default_PropertyChanged;
            DefineGestures();
        }

        private void Default_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            DefineGestures();
        }

        private static KeyGesture? GetGesture(string gesture)
        {
            var converter = new KeyGestureConverter();
            return converter.ConvertFromString(gesture) as KeyGesture;
        }

        private void DefineGestures()
        {
            ExecuteKeyBinding.Gesture = GetGesture(Properties.Settings.Default.ExecuteGesture);
        }
    }
}
