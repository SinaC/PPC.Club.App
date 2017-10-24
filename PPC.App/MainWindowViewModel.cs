using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using EasyIoc;
using EasyMVVM;
using PPC.App.Closure;
using PPC.Data.Articles;
using PPC.Data.Contracts;
using PPC.Helpers;
using PPC.Log;
using PPC.Messages;
using PPC.Module.Cards.ViewModels;
using PPC.Module.Inventory.ViewModels;
using PPC.Module.Players.ViewModels;
using PPC.Module.Shop.ViewModels;
using PPC.Services.Popup;

namespace PPC.App
{
    public enum ApplicationModes
    {
        Shop,
        Players,
        Inventory,
        Cards,
    }

    public class MainWindowViewModel : ObservableObject
    {
        private IPopupService PopupService => IocContainer.Default.Resolve<IPopupService>();
        private ILog Logger => IocContainer.Default.Resolve<ILog>();

        private bool _isWaiting;
        public bool IsWaiting
        {
            get { return _isWaiting; }
            protected set { Set(() => IsWaiting, ref _isWaiting, value); }
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

        private CardsViewModel _cardsViewModel;
        public CardsViewModel CardsViewModel
        {
            get { return _cardsViewModel; }
            protected set { Set(() => CardsViewModel, ref _cardsViewModel, value); }
        }

        #region Buttons + application mode

        private ApplicationModes _applicationMode;
        public ApplicationModes ApplicationMode
        {
            get { return _applicationMode; }
            protected  set { Set(() => ApplicationMode, ref _applicationMode, value); }
        }

        public bool IsCashRegisterSelected => ApplicationMode == ApplicationModes.Shop && ShopViewModel.Mode == ShopModes.CashRegister;
        public bool IsClientShoppingCartSelected => ApplicationMode == ApplicationModes.Shop && ShopViewModel.Mode == ShopModes.ClientShoppingCarts;
        public bool IsSoldArticlesSelected => ApplicationMode == ApplicationModes.Shop && ShopViewModel.Mode == ShopModes.SoldArticles;
        public bool IsPlayersSelected => ApplicationMode == ApplicationModes.Players;
        public bool IsInventorySelected => ApplicationMode == ApplicationModes.Inventory;
        public bool IsCardsSelected => ApplicationMode == ApplicationModes.Cards;

        private void RaiseSelectedMode()
        {
            RaisePropertyChanged(() => IsCashRegisterSelected);
            RaisePropertyChanged(() => IsClientShoppingCartSelected);
            RaisePropertyChanged(() => IsSoldArticlesSelected);
            RaisePropertyChanged(() => IsPlayersSelected);
            RaisePropertyChanged(() => IsInventorySelected);
            RaisePropertyChanged(() => IsCardsSelected);
        }

        private ICommand _switchToCashRegisterCommand;
        public ICommand SwitchToCashRegisterCommand => _switchToCashRegisterCommand = _switchToCashRegisterCommand ?? new RelayCommand(SwitchToCashRegister);

        private void SwitchToCashRegister()
        {
            ApplicationMode = ApplicationModes.Shop;
            ShopViewModel.ViewCashRegister();
            RaiseSelectedMode();
        }

        private ICommand _switchToShoppingCartsCommand;
        public ICommand SwitchToShoppingCartsCommand => _switchToShoppingCartsCommand = _switchToShoppingCartsCommand ?? new RelayCommand(SwitchToShoppingCarts);

        private void SwitchToShoppingCarts()
        {
            ApplicationMode = ApplicationModes.Shop;
            ShopViewModel.ViewShoppingCarts();
            RaiseSelectedMode();
        }

        private ICommand _switchToSoldArticlesCommand;
        public ICommand SwitchToSoldArticlesCommand => _switchToSoldArticlesCommand = _switchToSoldArticlesCommand ?? new RelayCommand(SwitchToSoldArticles);

        private void SwitchToSoldArticles()
        {
            ApplicationMode = ApplicationModes.Shop;
            ShopViewModel.ViewSoldArticles();
            RaiseSelectedMode();
        }

        private ICommand _addNewClientCommand;
        public ICommand AddNewClientCommand => _addNewClientCommand = _addNewClientCommand ?? new RelayCommand(AddNewClient);

        private void AddNewClient()
        {
            ApplicationMode = ApplicationModes.Shop;
            ShopViewModel.ViewShoppingCarts();
            ShopViewModel.ClientShoppingCartsViewModel.AddNewClientCommand.Execute(null);
            RaiseSelectedMode();
        }

        private ICommand _switchToPlayersCommand;
        public ICommand SwitchToPlayersCommand => _switchToPlayersCommand = _switchToPlayersCommand ?? new RelayCommand(SwitchToPlayers);

        private void SwitchToPlayers()
        {
            ApplicationMode = ApplicationModes.Players;
            RaiseSelectedMode();
        }

        private ICommand _switchToInventoryCommand;
        public ICommand SwitchToInventoryCommand => _switchToInventoryCommand = _switchToInventoryCommand ?? new RelayCommand(SwitchToInventory);

        private void SwitchToInventory()
        {
            ApplicationMode = ApplicationModes.Inventory;
            RaiseSelectedMode();
        }

        private ICommand _switchToCardSellerCommand;
        public ICommand SwitchToCardSellerCommand => _switchToCardSellerCommand = _switchToCardSellerCommand ?? new RelayCommand(SwitchToCardSeller);

        private void SwitchToCardSeller()
        {
            ApplicationMode = ApplicationModes.Cards;
            CardsViewModel.SelectedSeller = null; // unselect currently selected if any and switch to list mode
            RaiseSelectedMode();
        }

        #endregion

        #region Reload

        private ICommand _reloadCommand;
        public ICommand ReloadCommand => _reloadCommand= _reloadCommand ?? new RelayCommand(Reload);

        private void Reload()
        {
            PopupService.DisplayQuestion("Reload", "Are you sure you want to reload from backup ?", QuestionActionButton.Yes(ReloadConfirmed), QuestionActionButton.No());
        }

        private void ReloadConfirmed()
        {
            Logger.Info("Reload started");

            ShopViewModel.Reload();
            CardsViewModel.Reload();

            int cartsCount = ShopViewModel.ClientShoppingCartsViewModel.Clients.Count;
            int cardSellersCount = CardsViewModel.Sellers.Count;
            int transactionsCount = ShopViewModel.Transactions.Count;
            PopupService.DisplayQuestion("Reload", $"Reload done. Carts:{cartsCount} Card sellers:{cardSellersCount} Transactions:{transactionsCount}.", QuestionActionButton.Ok());

            Logger.Info($"Reload done. Carts:{cartsCount} Card sellers:{cardSellersCount} Transactions:{transactionsCount}.");
        }

        #endregion

        #region Close

        private ICommand _closeCommand;
        public ICommand CloseCommand => _closeCommand = _closeCommand ?? new RelayCommand(Close);

        private void Close()
        {
            // TODO: check if players have been saved, check if one or more shopping carts articles still opened: new method string PrepareClose (return null if ready or error message otherwise)
            PopupService.DisplayQuestion("Close application", "Do you want to perform cash registry closure", QuestionActionButton.Yes(CheckUnpaidShoppingCarts), QuestionActionButton.No(() => Application.Current.Shutdown()), QuestionActionButton.Cancel());
        }

        private void CheckUnpaidShoppingCarts()
        {
            if (ShopViewModel.ClientShoppingCartsViewModel.HasUnpaidClientShoppingCards)
                PopupService.DisplayQuestion("Close application", "Closure cannot be performed because one or more shopping cards are not yet paid.", QuestionActionButton.Ok(SwitchToShoppingCarts));
            else
                DisplayClosurePopup();
        }

        private void DisplayClosurePopup()
        {
            CashRegisterClosure closureData = ShopViewModel.PrepareClosure();
            List<SoldCards> soldCards = CardsViewModel.PrepareClosure();
            ClosurePopupViewModel vm = new ClosurePopupViewModel(CloseApplicationAfterClosurePopup, closureData, soldCards, SendMailsAsync);
            PopupService.DisplayModal(vm, "Cash register closure", 640, 480);
        }

        private void CloseApplicationAfterClosurePopup()
        {
            string savePath = ConfigurationManager.AppSettings["BackupPath"] + $"{DateTime.Now:yyyy-MM-dd hh-mm-ss}\\";
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);

            Logger.Info("Deleting backup files");
            ShopViewModel.DeleteBackupFiles(savePath);
            CardsViewModel.DeleteBackupFiles(savePath);

            Application.Current.Shutdown();
        }

        private async Task SendMailsAsync(CashRegisterClosure closure, List<SoldCards> soldCards)
        {
            IsWaiting = true;
            try
            {
                string closureConfigFilename = ConfigurationManager.AppSettings["CashRegisterClosureConfigPath"];
                if (File.Exists(closureConfigFilename))
                {
                    // Read closure config
                    CashRegisterClosureConfig closureConfig;
                    using (XmlTextReader reader = new XmlTextReader(closureConfigFilename))
                    {
                        DataContractSerializer serializer = new DataContractSerializer(typeof(CashRegisterClosureConfig));
                        closureConfig = (CashRegisterClosureConfig) await serializer.ReadObjectAsync(reader);
                    }

                    try
                    {
                        // Send closure mail
                        await SendClosureMailAsync(closure, closureConfig.SenderMail, closureConfig.SenderPassword, closureConfig.RecipientMail);
                    }
                    catch (Exception ex)
                    {
                        Logger.Exception("Error while sending closure mail", ex);
                        PopupService.DisplayError("Error while sending closure mail", ex);
                    }

                    foreach (SoldCards cards in soldCards.Where(x => !string.IsNullOrWhiteSpace(x.Email) && x.Email.Contains('@')))
                    {
                        try
                        {
                            await SendSoldCardsMailAsync(cards, closureConfig.SenderMail, closureConfig.SenderPassword);
                        }
                        catch (Exception ex)
                        {
                            Logger.Exception($"Error while sending sold cards mail to {cards?.SellerName ?? "??"}", ex);
                            PopupService.DisplayError($"Error while sending sold cards mail to {cards?.SellerName ?? "??"}", ex);
                        }
                    }
                }
                else
                {
                    Logger.Warning("Cash register closure config file not found -> Cannot send automatically cash register closure mail.");
                    PopupService.DisplayError("Warning", "Cash register closure config file not found -> Cannot send automatically cash register closure mail.");
                }
            }
            catch (Exception ex)
            {
                Logger.Exception("Error", ex);
                PopupService.DisplayError("Error", ex);
            }
            finally
            {
                IsWaiting = false;
            }
        }

        private async Task SendClosureMailAsync(CashRegisterClosure closure, string senderMail, string senderPassword, string recipientMail)
        {
            Logger.Info("Sending closure mail.");

            MailAddress fromAddress = new MailAddress(senderMail, "From PPC Club");
            MailAddress toAddress = new MailAddress(recipientMail, "To PPC");
            using (SmtpClient client = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, senderPassword)
            })
            {

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = $"Cloture caisse du club (date {DateTime.Now:F})",
                    Body = closure.ToString()
                })
                {
                    await client.SendMailAsync(message);
                }
            }

            Logger.Info("Closure mail sent.");
        }

        private async Task SendSoldCardsMailAsync(SoldCards soldCards, string senderMail, string senderPassword)
        {
            Logger.Info($"Sending sold cards mail to {soldCards.Email}.");

            MailAddress fromAddress = new MailAddress(senderMail, "From PPC Club");
            MailAddress toAddress = new MailAddress(soldCards.Email, $"To {soldCards.SellerName}");
            using (SmtpClient client = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, senderPassword)
            })
            {

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = $"Vente de carte à la pièce au club (date {DateTime.Now:F})",
                    Body = soldCards.ToString()
                })
                {
                    await client.SendMailAsync(message);
                }
            }

            Logger.Info($"Sold cards mail to {soldCards.Email} sent.");
        }

        #endregion

        public MainWindowViewModel()
        {
            if (!DesignMode.IsInDesignModeStatic)
            {
                try
                {
                    IocContainer.Default.Resolve<IArticleDb>().Load();
                }
                catch (Exception ex)
                {
                    Logger.Exception("Error while loading articles DB", ex);
                    PopupService.DisplayError("Error while loading articles DB", ex); // --> Popup will never be displayed because MainWindow is still in creation
                    throw; // ensure application crashes
                }
            }

            PlayersViewModel = new PlayersViewModel();
            ShopViewModel = new ShopViewModel();
            InventoryViewModel = new InventoryViewModel();
            CardsViewModel = new CardsViewModel();

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
            CardsViewModel = new CardsViewModelDesignData();

            ApplicationMode = ApplicationModes.Shop;

            IsWaiting = false;
        }
    }
}
