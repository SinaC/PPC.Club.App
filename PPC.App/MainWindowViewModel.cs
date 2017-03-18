using System;
using System.Windows;
using System.Windows.Input;
using EasyIoc;
using EasyMVVM;
using PPC.Data.Articles;
using PPC.Inventory.ViewModels;
using PPC.Messages;
using PPC.Players.ViewModels;
using PPC.Popups;
using PPC.Shop.ViewModels;
using PPC.Shop.ViewModels.ArticleSelector;

namespace PPC.App
{
    public enum ApplicationModes
    {
        Shop,
        Players,
        Inventory,
        //
        Test
    }

    public class MainWindowViewModel : ObservableObject
    {
        private IPopupService PopupService => EasyIoc.IocContainer.Default.Resolve<IPopupService>();

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
            protected set { Set(() => ShopViewModel, ref _shopViewModel, value); }
        }

        private InventoryViewModel _inventoryViewModel;
        public InventoryViewModel InventoryViewModel
        {
            get { return _inventoryViewModel; }
            protected set { Set(() => InventoryViewModel, ref _inventoryViewModel, value); }
        }

        private MobileArticleSelectorViewModel _testViewModel;
        public MobileArticleSelectorViewModel TestViewModel
        {
            get {  return _testViewModel;}
            protected set { Set(() => TestViewModel, ref _testViewModel, value); }
        }

        #region Buttons + application mode

        private ApplicationModes _applicationMode;
        public ApplicationModes ApplicationMode
        {
            get { return _applicationMode; }
            set { Set(() => ApplicationMode, ref _applicationMode, value); }
        }

        private ICommand _switchToCashRegisterCommand;
        public ICommand SwitchToCashRegisterCommand => _switchToCashRegisterCommand = _switchToCashRegisterCommand ?? new RelayCommand(SwitchToCashRegister);

        private void SwitchToCashRegister()
        {
            ApplicationMode = ApplicationModes.Shop;
            ShopViewModel.ViewCashRegisterCommand.Execute(null);
        }

        private ICommand _switchToShoppingCartsCommand;
        public ICommand SwitchToShoppingCartsCommand => _switchToShoppingCartsCommand = _switchToShoppingCartsCommand ?? new RelayCommand(SwitchToShoppingCarts);

        private void SwitchToShoppingCarts()
        {
            ApplicationMode = ApplicationModes.Shop;
            ShopViewModel.ViewShoppingCartsCommand.Execute(null);
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
            ShopViewModel.ViewShoppingCartsCommand.Execute(null);
            ShopViewModel.ClientShoppingCartsViewModel.AddNewClientCommand.Execute(null);
        }

        private ICommand _switchToPlayersCommand;
        public ICommand SwitchToPlayersCommand => _switchToPlayersCommand = _switchToPlayersCommand ?? new RelayCommand(SwitchToPlayers);

        private void SwitchToPlayers()
        {
            ApplicationMode = ApplicationModes.Players;
        }

        private ICommand _switchToInventoryCommand;
        public ICommand SwitchToInventoryCommand => _switchToInventoryCommand = _switchToInventoryCommand ?? new RelayCommand(SwitchToInventory);

        private void SwitchToInventory()
        {
            ApplicationMode = ApplicationModes.Inventory;
        }

        private ICommand _switchToTestCommand;
        public ICommand SwitchToTestCommand => _switchToTestCommand = _switchToTestCommand ?? new RelayCommand(SwitchToTest);

        private void SwitchToTest()
        {
            ApplicationMode = ApplicationModes.Test;
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
            //ShopViewModel.CashRegisterClosureCommand.Execute(null);
            PopupService.DisplayQuestion("Close application", "Do you want to perform cash registry closure",
                new ActionButton
                {
                    Caption = "Yes",
                    Order = 1,
                    ClickCallback = () => ShopViewModel.CashRegisterClosureCommand.Execute(null)
                },
                new ActionButton
                {
                    Caption = "No",
                    Order = 2,
                    ClickCallback = () => Application.Current.Shutdown(0)
                },
                new ActionButton
                {
                    Caption = "Cancel",
                    Order = 3,
                });
            // TODO: check if players have been saved, check if one or more shopping carts articles still opened: new method string PrepareClose (return null if ready or error message otherwise)
            // TODO
            //PopupService.DisplayQuestion("Close application", String.Empty,
            //    new ActionButton
            //    {
            //        Caption = "Perform cash registry closure",
            //        Order = 1,
            //        ClickCallback = () => ShopViewModel.PerformClosure(() => Application.Current.Shutdown(0)) // ShopViewModel is not responsible for closing application
            //    },
            //    new ActionButton
            //    {
            //        Caption = "Exit with closure",
            //        Order = 2,
            //        ClickCallback = () => Application.Current.Shutdown(0)
            //    },
            //    new ActionButton
            //    {
            //        Caption = "Cancel",
            //        Order = 3,
            //    });
        }

        #endregion

        public MainWindowViewModel()
        {
            try
            {
                IocContainer.Default.Resolve<IArticleDb>().Load();
            }
            catch (Exception ex)
            {
                PopupService.DisplayError("Error while loading articles DB", ex);
            }

            PlayersViewModel = new PlayersViewModel();
            ShopViewModel = new ShopViewModel();
            InventoryViewModel = new InventoryViewModel();

            TestViewModel = new MobileArticleSelectorViewModel();

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
            if (msg.SwitchToShop)
                ApplicationMode = ApplicationModes.Shop;
        }
    }

    public class MainWindowViewModelDesignData : MainWindowViewModel
    {
        public MainWindowViewModelDesignData()
        {
            PlayersViewModel = new PlayersViewModelDesignData();
            ShopViewModel = new ShopViewModelDesignData();
            InventoryViewModel = new InventoryViewModelDesignData();

            TestViewModel = new MobileArticleSelectorViewModelDesignData();

            ApplicationMode = ApplicationModes.Shop;

            IsWaiting = false;
        }
    }
}
