
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Search;

using Microsoft.Xaml.Behaviors;

using System.Windows;

namespace CosmosDbExplorer.Behaviors
{

    public class AvalonTextEditorBehavior : Behavior<TextEditor>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            OnUseSearchChanged();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }

        public bool UseSearch
        {
            get { return (bool)GetValue(UseSearchProperty); }
            set { SetValue(UseSearchProperty, value); }
        }

        public static readonly DependencyProperty UseSearchProperty =
            DependencyProperty.Register(
                "UseSearch",
                typeof(bool),
                typeof(AvalonTextEditorBehavior),
                new PropertyMetadata(false, OnUseSearchPropertyChanged));

        private static void OnUseSearchPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AvalonTextEditorBehavior behavior)
            {
                behavior.OnUseSearchChanged();
            }
        }

        private SearchPanel? _searchPanel;
        private void OnUseSearchChanged()
        {
            if (AssociatedObject is null)
            {
                return;
            }

            if (UseSearch)
            {
                _searchPanel ??= SearchPanel.Install(AssociatedObject.TextArea);
            }
            else if (_searchPanel != null)
            {
                _searchPanel.Uninstall();
                _searchPanel = null;
            }
        }
    }
}
