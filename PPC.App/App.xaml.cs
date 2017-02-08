using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace PPC.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-BE");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-BE");
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            //EventManager.RegisterClassHandler(typeof(WatermarkTextBox), UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(SelectivelyHandleMouseButton), true);
            //EventManager.RegisterClassHandler(typeof(WatermarkTextBox), UIElement.GotKeyboardFocusEvent, new RoutedEventHandler(SelectAllText), true);

            base.OnStartup(e);
        }

        //private static void SelectivelyHandleMouseButton(object sender, MouseButtonEventArgs e)
        //{
        //    var textbox = (sender as WatermarkTextBox);
        //    if (textbox != null && !textbox.IsKeyboardFocusWithin)
        //    {
        //        if (e.OriginalSource.GetType().Name == "TextBoxView")
        //        {
        //            e.Handled = true;
        //            textbox.Focus();
        //        }
        //    }
        //}

        //private static void SelectAllText(object sender, RoutedEventArgs e)
        //{
        //    var textBox = e.OriginalSource as WatermarkTextBox;
        //    if (textBox != null)
        //        textBox.SelectAll();
        //}
    }
}
