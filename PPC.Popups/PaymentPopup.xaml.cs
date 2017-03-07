using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.Toolkit;

namespace PPC.Popups
{
    /// <summary>
    /// Interaction logic for PaymentPopup.xaml
    /// </summary>
    public partial class PaymentPopup : UserControl
    {
        public PaymentPopup()
        {
            InitializeComponent();
        }

        private void DecimalUpDown_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // DecimalUpDown uses current culture and it seems fr-BE doesn't use . but , as decimal separator
            DecimalUpDown @this = sender as DecimalUpDown;
            if (@this == null)
                return;
            @this.Text = @this.Text.Replace('.', ',');
        }

        private void DecimalUpDown_OnGotFocus(object sender, RoutedEventArgs e)
        {
            // Crappy workaround because FocusManager.FocusedElement doesn't set Keyboard focus
            DecimalUpDown @this = sender as DecimalUpDown;
            TextBox partTextBox = @this?.FindVisualChildren<TextBox>().FirstOrDefault(x => x.Name == "PART_TextBox");
            if (partTextBox == null)
                return;
            Dispatcher.BeginInvoke((Action)delegate
            {
                Keyboard.Focus(partTextBox);
                partTextBox.SelectAll();
            }, DispatcherPriority.Render);

        }

        private void ClientCashDecimalUpDown_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return) // set focus on PaidButton when Enter is pressed
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    Keyboard.Focus(PaidButton);
                }, DispatcherPriority.Render);
            }
        }
    }
}
