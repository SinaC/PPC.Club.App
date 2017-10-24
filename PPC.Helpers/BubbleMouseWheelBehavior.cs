using System.Windows;
using System.Windows.Input;

namespace PPC.Helpers
{
    public static class BubbleMouseWheelBehavior
    {
        public static bool GetIsBubblingEnabled(DependencyObject dependencyObject)
        {
            return (bool) dependencyObject.GetValue(IsBubblingEnabledProperty);
        }

        public static void SetIsBubblingEnabled(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(IsBubblingEnabledProperty, value);
        }

        public static readonly DependencyProperty IsBubblingEnabledProperty = DependencyProperty.RegisterAttached(
            "IsBubblingEnabled", 
            typeof(bool),
            typeof(BubbleMouseWheelBehavior),
            new UIPropertyMetadata(false, OnIsBubblingEnabledChanged));

        private static void OnIsBubblingEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            UIElement element = dependencyObject as UIElement;
            if (element == null)
                return;
            bool oldValue = (bool)dependencyPropertyChangedEventArgs.OldValue;
            bool newValue = (bool)dependencyPropertyChangedEventArgs.NewValue;
            if (oldValue == newValue)
                return;
            if (oldValue)
                element.PreviewMouseWheel -= Element_PreviewMouseWheel;
            if (newValue)
                element.PreviewMouseWheel += Element_PreviewMouseWheel;
        }

        private static void Element_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                // Raise to top window
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = UIElement.MouseWheelEvent,
                    Source = sender
                };
                var parent = VisualTreeHelpers.FindAncestor<Window>(sender as DependencyObject);
                parent?.RaiseEvent(eventArg);
            }
        }
    }
}
