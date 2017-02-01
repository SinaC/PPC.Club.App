using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;

namespace PPC.Popup
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

        private void UpDownBase_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            DecimalUpDown @this = sender as DecimalUpDown;
            if (@this == null)
                return;
            @this.Text = @this.Text.Replace('.', ',');
        }
    }
}
