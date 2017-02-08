using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;

namespace PPC.Popup
{
    /// <summary>
    /// Interaction logic for CreateEditArticlePopup.xaml
    /// </summary>
    public partial class CreateEditArticlePopup : UserControl
    {
        public CreateEditArticlePopup()
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
