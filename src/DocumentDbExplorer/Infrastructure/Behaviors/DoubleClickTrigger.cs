using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace DocumentDbExplorer.Infrastructure.Behaviors
{
    public class DoubleClickTrigger : TriggerBase<UIElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseDown += OnPreviewMouseDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewMouseDown -= OnPreviewMouseDown;
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                InvokeActions(e);
            }
        }
    }
}
