using System.Collections.ObjectModel;
using PPC.Messages;
using PPC.MVVM;
using PPC.Players.ViewModels;
using PPC.Sale.ViewModels;
using PPC.Shop.ViewModels;

namespace PPC.App
{
    public class MainWindowViewModel : ObservableObject
    {
        private bool _isWaiting;
        public bool IsWaiting
        {
            get { return _isWaiting; }
            set { Set(() => IsWaiting, ref _isWaiting, value); }
        }

        private ObservableCollection<TabBase> _tabs;
        public ObservableCollection<TabBase> Tabs
        {
            get { return _tabs; }
            protected set { Set(() => Tabs, ref _tabs, value); }
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
            protected set { Set(() => PlayersViewModel, ref _playersViewModel, value); }
        }

        private SaleViewModel _saleViewModel;
        public SaleViewModel SaleViewModel
        {
            get { return _saleViewModel; }
            protected set { Set(() => SaleViewModel, ref _saleViewModel, value); }
        }

        //private Shop.ViewModels.ShopViewModel _shopViewModel;
        //public Shop.ViewModels.ShopViewModel ShopViewModel
        //{
        //    get { return _shopViewModel; }
        //    set { Set(() => ShopViewModel, ref _shopViewModel, value); }
        //}

        public MainWindowViewModel()
        {
            PlayersViewModel = new PlayersViewModel();
            SaleViewModel = new SaleViewModel();
            //ShopViewModel = new Shop.ViewModels.ShopViewModel();

            Tabs = new ObservableCollection<TabBase>
            {
                SaleViewModel,
                PlayersViewModel,
                //ShopViewModel
            };
            SelectedTab = Tabs[0];

            Mediator.Default.Register<ChangeWaitingMessage>(this, ChangeWaiting);
            Mediator.Default.Register<PlayerSelectedMessage>(this, PlayerSelected);
        }

        private void ChangeWaiting(ChangeWaitingMessage msg)
        {
            IsWaiting = msg.IsWaiting;
        }

        private void PlayerSelected(PlayerSelectedMessage msg)
        {
            if (msg.SwitchToSaleTab)
                SelectedTab = SaleViewModel;
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

            IsWaiting = true;
        }
    }
}
