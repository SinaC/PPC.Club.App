using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using Xceed.Wpf.AvalonDock.Layout;

namespace PPC.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            //ShutdownMode = ShutdownMode.OnExplicitShutdown;

            //AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            //Dispatcher.CurrentDispatcher.UnhandledException += CurrentDispatcherOnUnhandledException;
            //Dispatcher.UnhandledException += DispatcherOnUnhandledException;
            //Application.Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
            //TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
            //DispatcherUnhandledException += OnDispatcherUnhandledException;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-BE");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-BE");
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            //EventManager.RegisterClassHandler(typeof(WatermarkTextBox), UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(SelectivelyHandleMouseButton), true);
            //EventManager.RegisterClassHandler(typeof(WatermarkTextBox), UIElement.GotKeyboardFocusEvent, new RoutedEventHandler(SelectAllText), true);

            base.OnStartup(e);
        }

        //private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs unobservedTaskExceptionEventArgs)
        //{
        //    Debug.WriteLine("TaskSchedulerOnUnobservedTaskException");
        //    MessageBox.Show("TaskSchedulerOnUnobservedTaskException");
        //}

        //private void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs dispatcherUnhandledExceptionEventArgs)
        //{
        //    Debug.WriteLine("CurrentOnDispatcherUnhandledException");
        //    MessageBox.Show("CurrentOnDispatcherUnhandledException");
        //}

        //private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs dispatcherUnhandledExceptionEventArgs)
        //{
        //    Debug.WriteLine("DispatcherOnUnhandledException");
        //    MessageBox.Show("DispatcherOnUnhandledException");
        //}

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            //Debug.WriteLine("CurrentDomainOnUnhandledException");
            //MessageBox.Show("CurrentDomainOnUnhandledException");
        }

        private void CurrentDispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs dispatcherUnhandledExceptionEventArgs)
        {
            //Debug.WriteLine("CurrentDispatcherOnUnhandledException");
            //MessageBox.Show("CurrentDispatcherOnUnhandledException");

            DisplayErrorMessageBox(dispatcherUnhandledExceptionEventArgs.Exception);

            dispatcherUnhandledExceptionEventArgs.Handled = true;
            Application.Current.Shutdown(0);
        }

        //private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs dispatcherUnhandledExceptionEventArgs)
        //{
        //    Debug.WriteLine("OnDispatcherUnhandledException");
        //    MessageBox.Show("OnDispatcherUnhandledException");
        //}

        private void DisplayErrorMessageBox(Exception ex)
        {
            //http://stackoverflow.com/questions/576503/how-to-set-wpf-messagebox-owner-to-desktop-window-because-splashscreen-closes-mes/5328590#5328590
            //Window temp = new Window { Visibility = Visibility.Hidden };
            Window temp = new Window {AllowsTransparency = true, ShowInTaskbar = false, WindowStyle = WindowStyle.None, Background = Brushes.Transparent};
            temp.Show();
            MessageBox.Show(ex.ToString(), "Application has stopped working.", MessageBoxButton.OK);
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
