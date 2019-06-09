using System.Windows.Controls;

namespace PPC.Module.Shop.Views.Popups
{
    /// <summary>
    /// Interaction logic for AskNamePopup.xaml
    /// </summary>
    public partial class AskNamePopup : UserControl
    {
        public AskNamePopup()
        {
            InitializeComponent();
            //Loaded += OnLoaded;
        }

        //private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        //{
        //    Loaded -= OnLoaded;
        //    NameTextBox.Dispatcher.BeginInvoke((Action)delegate
        //    {
        //        NameTextBox.Focus();
        //        Keyboard.Focus(NameTextBox);
        //    }, DispatcherPriority.Input);
        //}
    }
}
