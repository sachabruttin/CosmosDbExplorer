using System;
using System.Windows;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using Microsoft.Xaml.Behaviors;

namespace CosmosDbExplorer.Behaviors
{
    public sealed class SelectedTextBehavior : Behavior<TextEditor>
    {
        public static readonly DependencyProperty SelectedTextProperty =
            DependencyProperty.Register("SelectedText", typeof(string), typeof(SelectedTextBehavior),
                new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string SelectedText
        {
            get { return (string)GetValue(SelectedTextProperty); }
            set { SetValue(SelectedTextProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                AssociatedObject.TextArea.SelectionChanged += SelectionChanged;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
            {
                AssociatedObject.TextArea.SelectionChanged -= SelectionChanged;
            }
        }

        private void SelectionChanged(object? sender, EventArgs e)
        {
            if (sender is TextArea textarea)
            {
                SelectedText = textarea.Selection.GetText();
            }
        }
    }
}
