using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
