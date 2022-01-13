using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using Microsoft.Xaml.Behaviors;

namespace CosmosDbExplorer.Behaviors
{
    public class TextAreaZoomBehavior : Behavior<TextEditor>
    {
        public static readonly DependencyProperty ZoomLevelProperty =
            DependencyProperty.Register("ZoomLevel", typeof(double), typeof(TextAreaZoomBehavior),
            new UIPropertyMetadata(1.0d, OnZoomLevelChanged));

        private static void OnZoomLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextAreaZoomBehavior behavior)
            {
                if (behavior.AssociatedObject is TextEditor editor)
                {
                    editor.TextArea.LayoutTransform = new ScaleTransform((double)e.NewValue, (double)e.NewValue, 0, 0);
                }
            }
        }

        public double ZoomLevel
        {
            get { return (double)GetValue(ZoomLevelProperty); }
            set { SetValue(ZoomLevelProperty, value); }
        }
    }
}
