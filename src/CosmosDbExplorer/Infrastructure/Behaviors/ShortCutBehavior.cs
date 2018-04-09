using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace UI.Common.CommandBehaviors
{
    /// <summary>
    /// this is an Attached behavior for KeyDown that is specific to handling Shortcuts
    /// </summary>
    public class ShortCutBehavior
    {
        public static DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached( "Command",
            typeof( ICommand ),
            typeof( ShortCutBehavior ),
            new UIPropertyMetadata( CommandChanged ) );

        public static DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached( "CommandParameter",
                                                typeof( object ),
                                                typeof( ShortCutBehavior ) );


        public static ICommand GetCommand( DependencyObject obj )
        {
            return (ICommand)obj.GetValue( CommandProperty );
        }

        public static void SetCommand( DependencyObject target, ICommand value )
        {
            target.SetValue( CommandProperty, value );
        }

        public static void SetCommandParameter( DependencyObject target, object value )
        {
            target.SetValue( CommandParameterProperty, value );
        }

        public static object GetCommandParameter( DependencyObject target )
        {
            return target.GetValue( CommandParameterProperty );
        }

        private static void CommandChanged( DependencyObject target, DependencyPropertyChangedEventArgs e )
        {
            var control = target as Control;
            if (control != null)
            {
                if ((e.NewValue != null) && (e.OldValue == null))
                {
                    control.KeyDown += OnKeyDown;
                }
                else if ((e.NewValue == null) && (e.OldValue != null))
                {
                    control.KeyDown -= OnKeyDown;
                }
            }
        }

        private static void OnKeyDown( object sender, KeyEventArgs e )
        {
            var k = e.Key == Key.System ? e.SystemKey : e.Key;

            if (k == Key.System ||
                k == Key.LeftCtrl ||
                k == Key.RightCtrl ||
                k == Key.LeftAlt ||
                k == Key.RightAlt ||
                k == Key.LeftShift ||
                k == Key.RightShift)
            {
                return;
            }

            var control = sender as Control;
            var command = (ICommand)control.GetValue( CommandProperty );
            object commandParameter = e;
            command.Execute( commandParameter );
        }
    }
}
