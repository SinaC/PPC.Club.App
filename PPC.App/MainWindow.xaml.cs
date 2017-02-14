using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PPC.Popup;

namespace PPC.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display); // avoid blurry rotated text
            //TextOptions.SetTextRenderingMode(this, TextRenderingMode.Auto);
            //TextOptions.SetTextFormattingMode(this, TextFormattingMode.Ideal);
            //RenderOptions.SetClearTypeHint(this, ClearTypeHint.Auto);

            InitializeComponent();

            EasyIoc.IocContainer.Default.RegisterInstance<IPopupService>(ModalPopupPresenter);

            DataContext = new MainWindowViewModel();
        }

        private void MainWindow_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
