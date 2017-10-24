using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using PPC.Common;

namespace PPC.Services.Popup
{
    /// <summary>
    /// Interaction logic for PopupWindow.xaml
    /// </summary>
    public partial class PopupWindow : Window
    {
        public PopupWindow()
        {
            InitializeComponent();

            double? fontSize = PPCConfigurationManager.FontSize;
            if (fontSize.HasValue)
                TextElement.SetFontSize(this, fontSize.Value);
        }

        private void MainWindow_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                double actualFontSize = TextElement.GetFontSize(this);
                double newFontSize = actualFontSize * Math.Pow(1.3, e.Delta / 120F);
                TextElement.SetFontSize(this, newFontSize);

                e.Handled = true;
            }
        }
    }
}
