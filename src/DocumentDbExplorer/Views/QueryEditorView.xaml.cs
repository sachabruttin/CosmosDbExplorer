using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using DocumentDbExplorer.Infrastructure.Models;
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PaneViewModel datacontext)
            {
                datacontext.IconSource = FindResource("SqlQueryIcon") as ImageSource;
            }
        }
    }
}
