using System.Windows;
using System.Windows.Input;
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
