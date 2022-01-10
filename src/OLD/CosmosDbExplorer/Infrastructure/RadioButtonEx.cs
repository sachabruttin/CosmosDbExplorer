using System;
using System.Windows;
using System.Windows.Controls;

namespace CosmosDbExplorer.Infrastructure
{
    public class RadioButtonEx : RadioButton
    {
        public object RadioValue
        {
            get { return (object)GetValue(RadioValueProperty); }
            set { SetValue(RadioValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RadioValue.
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RadioValueProperty = DependencyProperty.Register("RadioValue", typeof(object), typeof(RadioButtonEx), new UIPropertyMetadata(null));

        public object RadioBinding
        {
            get { return (object)GetValue(RadioBindingProperty); }
            set { SetValue(RadioBindingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RadioBinding.
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RadioBindingProperty = DependencyProperty.Register("RadioBinding", typeof(object), typeof(RadioButtonEx), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnRadioBindingChanged));

        private static void OnRadioBindingChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var rb = (RadioButtonEx)d;

            switch (e.NewValue)
            {
                case bool boolValue:
                    if (bool.Parse(rb.RadioValue.ToString()).Equals(boolValue))
                    {
                        rb.SetCurrentValue(RadioButton.IsCheckedProperty, true);
                    }
                    break;
                case Enum enumValue:
                    if (Enum.Parse(e.NewValue.GetType(), rb.RadioValue.ToString()).Equals(enumValue))
                    {
                        rb.SetCurrentValue(RadioButton.IsCheckedProperty, true);
                    }
                    break;
                default:
                    if (rb.RadioValue.Equals(e.NewValue))
                    {
                        rb.SetCurrentValue(RadioButton.IsCheckedProperty, true);
                    }
                    break;
            }
        }

        protected override void OnChecked(RoutedEventArgs e)
        {
            base.OnChecked(e);
            SetCurrentValue(RadioBindingProperty, RadioValue);
        }
    }
}
