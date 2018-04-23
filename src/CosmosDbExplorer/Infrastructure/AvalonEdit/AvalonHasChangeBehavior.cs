using System;
using System.Windows;
using System.Windows.Interactivity;
using ICSharpCode.AvalonEdit;

namespace DocumentDbExplorer.Infrastructure.AvalonEdit
{
    public sealed class AvalonHasChangeBehavior : Behavior<TextEditor>
    {
        private static readonly DependencyProperty HasChangeProperty =
                DependencyProperty.Register("HasChange", typeof(bool), typeof(AvalonHasChangeBehavior));
    
        public bool HasChange
        {
            get { return (bool)GetValue(HasChangeProperty); }
            set { SetValue(HasChangeProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                AssociatedObject.TextChanged += AssociatedObjectOnTextChanged;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
            {
                AssociatedObject.TextChanged -= AssociatedObjectOnTextChanged;
            }
        }

        private void AssociatedObjectOnTextChanged(object sender, EventArgs eventArgs)
        {
            if (sender is TextEditor textEditor)
            {
                if (textEditor.Document != null)
                {
                    HasChange = textEditor.Document.UndoStack.CanUndo;
                }
            }
        }
    }
}
