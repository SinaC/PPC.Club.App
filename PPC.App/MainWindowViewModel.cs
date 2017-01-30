using System.Collections.ObjectModel;
using PPC.MVVM;
using PPC.Players;
using PPC.Sale;
using PPC.Tab;

namespace PPC.App
{
    public class MainWindowViewModel : ObservableObject
    {
        private ObservableCollection<TabBase> _tabs;
        public ObservableCollection<TabBase> Tabs
        {
            get { return _tabs; }
            set { Set(() => Tabs, ref _tabs, value); }
        }

        private TabBase _selectedTab;
        public TabBase SelectedTab
        {
            get {  return _selectedTab; }
            set { Set(() => SelectedTab, ref _selectedTab, value); }
        }

        private PlayersViewModel _playersViewModel;
        public PlayersViewModel PlayersViewModel
        {
            get { return _playersViewModel; }
            set { Set(() => PlayersViewModel, ref _playersViewModel, value); }
        }

        private SaleViewModel _saleViewModel;
        public SaleViewModel SaleViewModel
        {
            get { return _saleViewModel; }
            set { Set(() => SaleViewModel, ref _saleViewModel, value); }
        }

        public MainWindowViewModel()
        {
            PlayersViewModel = new PlayersViewModel();
            SaleViewModel = new SaleViewModel();

            Tabs = new ObservableCollection<TabBase>
            {
                SaleViewModel,
                PlayersViewModel
            };
            SelectedTab = Tabs[0];
        }
    }

    public class MainWindowViewModelDesignData : MainWindowViewModel
    {
        public MainWindowViewModelDesignData()
        {
            PlayersViewModel = new PlayersViewModelDesignData();
            SaleViewModel = new SaleViewModelDesignData();

            Tabs = new ObservableCollection<TabBase>
            {
                SaleViewModel,
                PlayersViewModel
            };
            SelectedTab = Tabs[0];
        }
    }
}
