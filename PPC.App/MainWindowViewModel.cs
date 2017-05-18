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

        private ICommand _switchToCardSellerCommand;
        public ICommand SwitchToCardSellerCommand => _switchToCardSellerCommand = _switchToCardSellerCommand ?? new RelayCommand(SwitchToCardSeller);

        private void SwitchToCardSeller()
        {
            ApplicationMode = ApplicationModes.Cards;
            CardsViewModel.SelectedSeller = null; // unselect currently selected if any and switch to list mode
        }

        #endregion

        #region Reload

        private ICommand _reloadCommand;
        public ICommand ReloadCommand => _reloadCommand= _reloadCommand ?? new RelayCommand(Reload);

        private void Reload()
        {
            PopupService.DisplayQuestion("Reload", "Are you sure you want to reload from backup ?",
                   new QuestionActionButton
                   {
                       Order = 1,
                       Caption = "Yes",
                       ClickCallback = ReloadConfirmed
                   },
                   new QuestionActionButton
                   {
                       Order = 2,
                       Caption = "No"
                   });
        }

        private void ReloadConfirmed()
        {
            ShopViewModel.Reload();
            CardsViewModel.Reload();
        }

        #endregion

        #region Close

        private ICommand _closeCommand;
        public ICommand CloseCommand => _closeCommand = _closeCommand ?? new RelayCommand(Close);

        private void Close()
        {
            PopupService.DisplayQuestion("Close application", "Do you want to perform cash registry closure",
                new QuestionActionButton
                {
                    Caption = "Yes",
                    Order = 1,
                    ClickCallback = DisplayClosurePopup
                },
                new QuestionActionButton
                {
                    Caption = "No",
                    Order = 2,
                    ClickCallback = () => Application.Current.Shutdown()
                },
                new QuestionActionButton
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

        private void DisplayClosurePopup()
        {
            //Old way: ShopViewModel.CashRegisterClosureCommand.Execute(null);

            CashRegisterClosure closureData = ShopViewModel.PrepareClosure();
            List<SoldCards> soldCards = CardsViewModel.PrepareClosure();
            ClosurePopupViewModel vm = new ClosurePopupViewModel(CloseApplicationAfterClosurePopup, closureData, soldCards, async (closure,cards) => await SendMailsAsync(closure, cards));
            PopupService.DisplayModal(vm, "Cash register closure", 640, 480);
        }

        private void CloseApplicationAfterClosurePopup()
        {
            ShopViewModel.DeleteBackupFiles();
            CardsViewModel.DeleteBackupFiles();

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
                        closureConfig = (CashRegisterClosureConfig)await serializer.ReadObjectAsync(reader);
                    }

                    try
                    {
                        // Send closure mail
                        await SendClosureMailAsync(closure, closureConfig.SenderMail, closureConfig.SenderPassword, closureConfig.RecipientMail);
                    }
                    catch (Exception ex)
                    {
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
                            PopupService.DisplayError($"Error while sending sold cards mail to {cards.SellerName}", ex);
                        }
                    }
                }
                else
                    PopupService.DisplayError("Warning", "Cash register closure config file not found -> Cannot send automatically cash register closure mail.");
            }
            catch (Exception ex)
            {
                PopupService.DisplayError("Error", ex);
            }
            finally
            {
                IsWaiting = false;
            }
        }

        private async Task SendClosureMailAsync(CashRegisterClosure closure, string senderMail, string senderPassword, string recipientMail)
        {
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
        }

        private async Task SendSoldCardsMailAsync(SoldCards soldCards, string senderMail, string senderPassword)
        {
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
                    PopupService.DisplayError("Error while loading articles DB", ex);
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

    // TODO: Use following code in ClosurePopupViewModel

    //http://stackoverflow.com/questions/12466049/passing-an-awaitable-anonymous-function-as-a-parameter
    public class Test
    {
        // In ClosurePopupViewModel SendMailCommand
        private async Task DoStuff(string a, string b, Func<string,string,Task> sendMailFunc)
        {
            // IsWaiting = true;
            await sendMailFunc(a, b);
            // IsWaiting = false
        }

        // In MainViewModel
        private async Task SendMailAsync(string a, string b)
        {
        }

        //
        private void PerformTest()
        {
            string a = "", b = "";
            DoStuff(a, b, SendMailAsync);
        }
    }
}
