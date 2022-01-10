using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Xml;
using CosmosDbExplorer.Infrastructure.Models;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace CosmosDbExplorer.Views
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

            // https://stackoverflow.com/a/1066009/20761
            NameScope.SetNameScope(editorContextMenu, NameScope.GetNameScope(this));
        }

        private void RegisterCustomHighlighting(string name)
        {
            // Load our custom highlighting definition
            IHighlightingDefinition customHightlighting;
            using (var stream = typeof(MainWindow).Assembly.GetManifestResourceStream($"CosmosDbExplorer.Infrastructure.AvalonEdit.{name}.xshd"))
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
            if (DataContext is PaneViewModelBase datacontext)
            {
                datacontext.IconSource = FindResource("SqlQueryIcon") as ImageSource;
            }
        }
    }
}
