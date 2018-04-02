using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;

namespace PPC.Module.Common.Popups
{
    /// <summary>
    /// Interaction logic for CreateEditArticlePopup.xaml
    /// </summary>
    public partial class CreateEditArticlePopup : UserControl
    {
        public CreateEditArticlePopup()
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

        private void UpDownBase_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            DecimalUpDown @this = sender as DecimalUpDown;
            if (@this == null)
                return;
            @this.Text = @this.Text.Replace('.', ',');
        }
    }
}
