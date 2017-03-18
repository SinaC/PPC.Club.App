using System.Windows;
using System.Windows.Input;
using EasyIoc;
using PPC.Data.Articles;
using PPC.Data.Players;
using PPC.Popups;

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
            IocContainer.Default.RegisterInstance<IPopupService>(ModalPopupPresenter);

            DataContext = new MainWindowViewModel();
        }

        private void MainWindow_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
