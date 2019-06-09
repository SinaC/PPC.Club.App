using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace PPC.Helpers
{
    //http://stackoverflow.com/questions/1356045/set-focus-on-textbox-in-wpf-from-view-model-c-wpf
    //http://zamjad.wordpress.com/2011/01/24/difference-between-coercevaluecallback-and-propertychangedcallback/
    //use CoerceValueCallback to ensure event is raised even if value is not modified
    public static class AttachedFocus
    {
        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool) obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        public static readonly DependencyProperty IsFocusedProperty = DependencyProperty.RegisterAttached(
            "IsFocused",
            typeof(bool),
            typeof(AttachedFocus),
            new PropertyMetadata(false, null, OnIsFocusedPropertyChanged));

        private static object OnIsFocusedPropertyChanged(DependencyObject d, object value)
        {
            UIElement uie = d as UIElement;
            if (uie != null && (bool) value) // Don't care about false values
            {
                uie.Dispatcher.BeginInvoke((Action)delegate
                {
                    uie.Focus();
                    Keyboard.Focus(uie);
                }, DispatcherPriority.Input);
            }
            return value;
        }
    }
}
