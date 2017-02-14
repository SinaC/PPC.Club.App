using System.Windows.Controls;
using System.Windows.Media;

namespace PPC.Shop.Views
{
    /// <summary>
    /// Interaction logic for CashRegisterView.xaml
    /// </summary>
    public partial class CashRegisterView : UserControl
    {
        public CashRegisterView()
        {
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display); // avoid blurry rotated text

            InitializeComponent();
        }
    }
}
