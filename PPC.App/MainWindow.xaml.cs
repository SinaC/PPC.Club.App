using System.Windows;
using System.Windows.Input;
using EasyIoc;
using PPC.Data.Articles;
using PPC.Data.Players;
using PPC.Services.Popup;

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

            IocContainer.Default.RegisterInstance<IArticleDb>(new ArticlesDb());
            IocContainer.Default.RegisterInstance<IPlayersDb>(new PlayersDb());
            IocContainer.Default.RegisterInstance<IPopupService>(new PopupService(this));

            DataContext = new MainWindowViewModel();

            FocusManager.AddGotFocusHandler(this, GotFocusHandler );
        }

        private void GotFocusHandler(object sender, RoutedEventArgs routedEventArgs)
        {
            //StackTrace t = new StackTrace();
            //Debug.WriteLine("GotFocus: source:"+routedEventArgs.Source + " original:" +routedEventArgs.OriginalSource+ Environment.NewLine+"stack:"+ t);
        }

        private void MainWindow_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
