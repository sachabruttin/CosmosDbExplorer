using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace DocumentDbExplorer.Infrastructure.Behaviors
{
    public class WatermarkBehavior : Behavior<ComboBox>
    {
        private WaterMarkAdorner _adorner;

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(WatermarkBehavior), new PropertyMetadata("Watermark"));


        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(double), typeof(WatermarkBehavior), new PropertyMetadata(12.0));


        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Foreground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(Brush), typeof(WatermarkBehavior), new PropertyMetadata(Brushes.Black));


        public string FontFamily
        {
            get { return (string)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FontFamily.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FontFamilyProperty =
            DependencyProperty.Register("FontFamily", typeof(string), typeof(WatermarkBehavior), new PropertyMetadata("Segoe UI"));


        protected override void OnAttached()
        {
            _adorner = new WaterMarkAdorner(AssociatedObject, Text, FontSize, FontFamily, Foreground);

            AssociatedObject.Loaded += OnLoaded;
            AssociatedObject.GotFocus += OnFocus;
            AssociatedObject.LostFocus += OnLostFocus;
            AssociatedObject.KeyUp += OnKeyUp;
            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AssociatedObject.SelectedItem != null)
            {
                var layer = AdornerLayer.GetAdornerLayer(AssociatedObject);
                layer.Remove(_adorner);
            }
            else
            {
                try
                {
                    var layer = AdornerLayer.GetAdornerLayer(AssociatedObject);
                    layer.Add(_adorner);
                }
                catch { }
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (!string.IsNullOrEmpty(AssociatedObject.Text))
            {
                var layer = AdornerLayer.GetAdornerLayer(AssociatedObject);
                layer.Remove(_adorner);
            }
            else
            {
                try
                {
                    var layer = AdornerLayer.GetAdornerLayer(AssociatedObject);
                    layer.Add(_adorner);
                }
                catch { }
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!AssociatedObject.IsFocused)
            {
                if (string.IsNullOrEmpty(AssociatedObject.Text))
                {
                    var layer = AdornerLayer.GetAdornerLayer(AssociatedObject);
                    layer.Add(_adorner);
                }
            }
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(AssociatedObject.Text))
            {
                try
                {
                    var layer = AdornerLayer.GetAdornerLayer(AssociatedObject);
                    layer.Add(_adorner);
                }
                catch { }
            }
        }

        private void OnFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AssociatedObject.Text))
            {
                var layer = AdornerLayer.GetAdornerLayer(AssociatedObject);
                layer.Remove(_adorner);
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }

        public class WaterMarkAdorner : Adorner
        {
            private string _text;
            private double _fontSize;
            private string _fontFamily;
            private Brush _foreground;

            public WaterMarkAdorner(UIElement element, string text, double fontsize, string font, Brush foreground)
                : base(element)
            {
                IsHitTestVisible = false;
                Opacity = 0.6;
                _text = text;
                _fontSize = fontsize;
                _fontFamily = font;
                _foreground = foreground;
            }

            protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);
                var text = new FormattedText(
                        _text,
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface(_fontFamily),
                        _fontSize,
                        _foreground);

                drawingContext.DrawText(text, new Point(3, 3));
            }
        }
    }
}
