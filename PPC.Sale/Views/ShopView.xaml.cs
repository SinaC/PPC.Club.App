using System.Windows.Controls;
using System.Windows.Media;

namespace PPC.Sale.Views
{
    /// <summary>
    /// Interaction logic for ShopView.xaml
    /// </summary>
    public partial class ShopView : UserControl
    {
        public ShopView()
        {
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display); // avoid blurry rotated text

            InitializeComponent();
        }
    }
}
