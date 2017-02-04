using System.Windows;
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

            InitializeComponent();

            EasyIoc.IocContainer.Default.RegisterInstance<IPopupService>(ModalPopupPresenter);

            DataContext = new MainWindowViewModel();
        }
    }
}
