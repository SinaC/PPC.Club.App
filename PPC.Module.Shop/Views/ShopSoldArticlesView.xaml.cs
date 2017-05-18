using System.Windows.Controls;
using System.Windows.Input;

namespace PPC.Module.Shop.Views
{
    /// <summary>
    /// Interaction logic for ShopSoldArticlesView.xaml
    /// </summary>
    public partial class ShopSoldArticlesView : UserControl
    {
        public ShopSoldArticlesView()
        {
            InitializeComponent();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
