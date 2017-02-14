using System.Windows.Input;
using PPC.Messages;
using PPC.MVVM;
using PPC.Players.ViewModels;
using PPC.Shop.ViewModels;

namespace PPC.App
{
    public enum ApplicationModes
    {
        Shop,
        Players
    }

    public class MainWindowViewModel : ObservableObject
    {
        private bool _isWaiting;
        public bool IsWaiting
        {
            get { return _isWaiting; }
            set { Set(() => IsWaiting, ref _isWaiting, value); }
        }

        private PlayersViewModel _playersViewModel;
        public PlayersViewModel PlayersViewModel
        {
            get { return _playersViewModel; }
            protected set { Set(() => PlayersViewModel, ref _playersViewModel, value); }
        }

        private ShopViewModel _shopViewModel;
        public ShopViewModel ShopViewModel
        {
            get { return _shopViewModel; }
            set { Set(() => ShopViewModel, ref _shopViewModel, value); }
        }

        #region Buttons + application mode

        private ApplicationModes _applicationMode;
        public ApplicationModes ApplicationMode
        {
            get { return _applicationMode; }
            set { Set(() => ApplicationMode, ref _applicationMode, value); }
        }

        private ICommand _switchToShopSummaryCommand;
        public ICommand SwitchToShopSummaryCommand => _switchToShopSummaryCommand = _switchToShopSummaryCommand ?? new RelayCommand(SwitchToShopSummary);

        private void SwitchToShopSummary()
        {
            ApplicationMode = ApplicationModes.Shop;
            ShopViewModel.ViewSummaryCommand.Execute(null);
        }

        private ICommand _switchToSoldArticlesCommand;
        public ICommand SwitchToSoldArticlesCommand => _switchToSoldArticlesCommand = _switchToSoldArticlesCommand ?? new RelayCommand(SwitchToSoldArticles);

        private void SwitchToSoldArticles()
        {
            ApplicationMode = ApplicationModes.Shop;
            ShopViewModel.ViewSoldArticlesCommand.Execute(null);
        }

        private ICommand _addNewClientCommand;
        public ICommand AddNewClientCommand => _addNewClientCommand = _addNewClientCommand ?? new RelayCommand(AddNewClient);

        private void AddNewClient()
        {
            ApplicationMode = ApplicationModes.Shop;
            ShopViewModel.AddNewClientCommand.Execute(null);
        }

        private ICommand _switchToPlayersCommand;
        public ICommand SwitchToPlayersCommand => _switchToPlayersCommand = _switchToPlayersCommand ?? new RelayCommand(SwitchToPlayers);

        private void SwitchToPlayers()
        {
            ApplicationMode = ApplicationModes.Players;
        }

        #endregion

        #region Reload

        private ICommand _reloadCommand;
        public ICommand ReloadCommand => _reloadCommand= _reloadCommand ?? new RelayCommand(Reload);

        private void Reload()
        {
            // TODO
            ShopViewModel.ReloadCommand.Execute(null);
        }

        #endregion

        #region Close

        private ICommand _closeCommand;
        public ICommand CloseCommand => _closeCommand = _closeCommand ?? new RelayCommand(Close);

        private void Close()
        {
            // TODO
            ShopViewModel.CashRegisterClosureCommand.Execute(null);
        }

        #endregion

        public MainWindowViewModel()
        {
            PlayersViewModel = new PlayersViewModel();
            ShopViewModel = new ShopViewModel();

            ApplicationMode = ApplicationModes.Shop;

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
                ApplicationMode = ApplicationModes.Shop;
        }
    }

    public class MainWindowViewModelDesignData : MainWindowViewModel
    {
        public MainWindowViewModelDesignData()
        {
            PlayersViewModel = new PlayersViewModelDesignData();
            ShopViewModel = new ShopViewModelDesignData();

            IsWaiting = false;
        }
    }
}
