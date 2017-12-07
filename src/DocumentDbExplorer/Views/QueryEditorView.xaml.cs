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
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace DocumentDbExplorer.Views
{
    /// <summary>
    /// Interaction logic for QueryEditorView.xaml
    /// </summary>
    public partial class QueryEditorView : UserControl
    {
        public QueryEditorView()
        {
            RegisterCustomHighlighting("DocumentDbSql");
            InitializeComponent();
        }

        private void RegisterCustomHighlighting(string name)
        {
            // Load our custom highlighting definition
            IHighlightingDefinition customHightlighting;
            using (var stream = typeof(MainWindow).Assembly.GetManifestResourceStream($"DocumentDbExplorer.Infrastructure.AvalonEdit.{name}.xshd"))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException("Could not find embedded resource");
                }

                using (var reader = new XmlTextReader(stream))
                {
                    customHightlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }

            // and register it in the HighlightingManager
            HighlightingManager.Instance.RegisterHighlighting(name, new string[] { $".{name.ToLower()}" }, customHightlighting);
        }
    }
}
