using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using EasyIoc;
using EasyMVVM;
using PPC.Common;
using PPC.Domain;
using PPC.Helpers;
using PPC.IDataAccess;
using PPC.Log;
using PPC.Messages;
using PPC.Module.Shop.Models;
using PPC.Module.Shop.ViewModels.Popups;
using PPC.Services.Popup;

namespace PPC.Module.Shop.ViewModels
{
    public enum ClientShoppingCartsModes
    {
        List,
        Detail
    }

    public class ClientShoppingCartsViewModel : ObservableObject
    {
        private IPopupService PopupService => IocContainer.Default.Resolve<IPopupService>();
        private ILog Logger => IocContainer.Default.Resolve<ILog>();
        private ISessionDL SessionDL => IocContainer.Default.Resolve<ISessionDL>();

        private readonly Action<ShopTransactionItem> _addTransactionAction;
        private readonly Action<decimal, decimal, decimal> _clientCartPaidAction;
        private readonly Action _clientCartReopenedAction;

        public bool HasClientShoppingCartsOpened => ClientShoppingCartsCount > 0;

        public int ClientShoppingCartsCount => Clients.Count;

        public int PaidClientShoppingCartsCount => Clients.Count(x => x.PaymentState == ClientShoppingCartPaymentStates.Paid);

        public int UnpaidClientShoppingCartsCount => Clients.Count(x => x.PaymentState == ClientShoppingCartPaymentStates.Unpaid);
        public bool HasUnpaidClientShoppingCards => Clients.Any(x => x.PaymentState == ClientShoppingCartPaymentStates.Unpaid && x.ShoppingCart.ShoppingCartArticles.Any());

        public decimal ClientShoppingCartsTotal => PaidClientShoppingCartsTotal + UnpaidClientShoppingCartsTotal;

        public decimal PaidClientShoppingCartsTotal => Clients.Where(x => x.PaymentState == ClientShoppingCartPaymentStates.Paid).Sum(x => x.Cash + x.BankCard);

        public decimal UnpaidClientShoppingCartsTotal => Clients.Where(x => x.PaymentState == ClientShoppingCartPaymentStates.Unpaid).Sum(x => x.ShoppingCart.Total);

        private ClientShoppingCartsModes _mode;

        public ClientShoppingCartsModes Mode
        {
            get { return _mode; }
            protected set { Set(() => Mode, ref _mode, value); }
        }

        #region Clients and client selection

        private ICommand _selectClientCommand;
        public ICommand SelectClientCommand => _selectClientCommand = _selectClientCommand ?? new RelayCommand<ClientShoppingCartViewModel>(client => SelectedClient = client);

        private ClientShoppingCartViewModel _selectedClient;
        public ClientShoppingCartViewModel SelectedClient
        {
            get { return _selectedClient; }
            protected set
            {
                if (Set(() => SelectedClient, ref _selectedClient, value))
                {
                    if (value == null)
                        Mode = ClientShoppingCartsModes.List;
                    else
                    {
                        Mode = ClientShoppingCartsModes.Detail;
                        value.ShoppingCart.GotFocus();
                    }
                }
            }
        }

        public ObservableCollection<ClientShoppingCartViewModel> Clients { get; }

        #endregion

        #region Add client

        private ICommand _addNewClientCommand;
        public ICommand AddNewClientCommand => _addNewClientCommand = _addNewClientCommand ?? new RelayCommand(AddNewClient);

        private void AddNewClient()
        {
            AskNamePopupViewModel vm = new AskNamePopupViewModel(AddNewClientNameSelected);
            PopupService.DisplayModal(vm, "Client name?");
        }

        private void AddNewClientNameSelected(string name)
        {
            ClientShoppingCartViewModel alreadyExistingClient = Clients.FirstOrDefault(x => String.Equals(x.ClientName, name, StringComparison.InvariantCultureIgnoreCase));
            if (alreadyExistingClient != null)
            {
                Logger.Warning($"A shopping cart with that client name '{name}' has already been opened!");
                PopupService.DisplayError(
                    "Warning",
                    $"A shopping cart with that client name '{name}' has already been opened! Switching to {name} shopping cart.",
                    () => SelectedClient = alreadyExistingClient); // switch to already existing client
            }
            else
            {
                ClientShoppingCartViewModel newClient = new ClientShoppingCartViewModel(ClientCartPaid, ClientCartReopened)
                {
                    HasFullPlayerInfos = false,
                    ClientName = name,
                };
                Clients.Add(newClient);
                SelectedClient = newClient;
            }
        }

        #endregion

        #region Close client

        private ICommand _closeClientCommand;
        public ICommand CloseClientCommand => _closeClientCommand = _closeClientCommand ?? new RelayCommand<ClientShoppingCartViewModel>(CloseClient);

        private void CloseClient(ClientShoppingCartViewModel client)
        {
            if (client.PaymentState == ClientShoppingCartPaymentStates.Unpaid && client.ShoppingCart.ShoppingCartArticles.Any())
                PopupService.DisplayQuestion($"Close {client.ClientName} shopping cart", $"Client {client.ClientName} has yet not paid, therefore shopping cart cannot be closed.", QuestionActionButton.Ok());
            else
                PopupService.DisplayQuestion($"Close {client.ClientName} shopping cart", "Are you sure ?", QuestionActionButton.Yes(() => CloseClientConfirmed(client)), QuestionActionButton.No());
        }

        private void CloseClientConfirmed(ClientShoppingCartViewModel client)
        {
            SelectedClient = null;
            Mode = ClientShoppingCartsModes.List;
            Clients.Remove(client);
            if (client.ShoppingCart.ShoppingCartArticles.Any())
            {
                // Create a transaction and add it to transactions
                ShopTransactionItem transaction = new ShopTransactionItem
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.Now,
                    Articles = client.ShoppingCart.ShoppingCartArticles.Select(x => new ShopArticleItem
                    {
                        Article = x.Article,
                        Quantity = x.Quantity
                    }).ToList(),
                    Cash = client.Cash,
                    BankCard = client.BankCard
                };
                _addTransactionAction?.Invoke(transaction);
            }
            // Delete backup file
            try
            {
                string filename = client.Filename;
                File.Delete(filename);
            }
            catch (Exception ex)
            {
                Logger.Exception("Error while deleting backup file", ex);
                PopupService.DisplayError("Error while deleting backup file", ex);
            }
            //
            client.RemoveHandlers();
        }

        #endregion

        public void ReloadClients(Session session)
        {
            SelectedClient = null;
            Clients.Clear();

            List<ClientCart> carts = session.ClientCarts;

            if (carts != null)
            {
                foreach (ClientCart cart in carts)
                {
                    try
                    {
                        ClientShoppingCartViewModel client = new ClientShoppingCartViewModel(ClientCartPaid, ClientCartReopened, cart);
                        Clients.Add(client);
                    }
                    catch (Exception ex)
                    {
                        Logger.Exception($"Error while creating client {cart.ClientName} from cart", ex);
                        PopupService.DisplayError($"Error while creating client {cart.ClientName} from cart", ex);
                    }
                }
            }
        }

        //public void LoadClients()
        //{
        //    SelectedClient = null;
        //    Clients.Clear();

        //    List<ClientCart> carts = null;
        //    try
        //    {
        //        carts = SessionDL.GetClientCarts();
        //    }
        //    catch (GetClientCartsException ex)
        //    {
        //        carts = ex.ClientCarts;

        //        Logger.Exception("Error while loading clients carts", ex);
        //        PopupService.DisplayError("Error while loading clients carts", ex);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Exception("Error while loading clients carts", ex);
        //        PopupService.DisplayError("Error while loading clients carts", ex);
        //    }

        //    if (carts != null)
        //    {
        //        foreach (ClientCart cart in carts)
        //        {
        //            try
        //            {
        //                ClientShoppingCartViewModel client = new ClientShoppingCartViewModel(ClientCartPaid, ClientCartReopened, cart);
        //                Clients.Add(client);
        //            }
        //            catch (Exception ex)
        //            {
        //                Logger.Exception($"Error while creating client {cart.ClientName} from cart", ex);
        //                PopupService.DisplayError($"Error while creating client {cart.ClientName} from cart", ex);
        //            }
        //        }
        //    }
        //}

        //public void LoadClients(string shopFilename)
        //{
        //    SelectedClient = null;
        //    Clients.Clear();
        //    //  add backup clients
        //    string path = PPCConfigurationManager.BackupPath;
        //    if (Directory.Exists(path))
        //    {
        //        foreach (string filename in Directory.EnumerateFiles(path, "*.xml", SearchOption.TopDirectoryOnly).Where(x => !x.Contains(shopFilename)))
        //        {
        //            try
        //            {
        //                ClientShoppingCartViewModel client = new ClientShoppingCartViewModel(ClientCartPaid, ClientCartReopened, filename);
        //                Clients.Add(client);
        //            }
        //            catch (Exception ex)
        //            {
        //                Logger.Exception($"Error while loading {filename ?? "??"} cart", ex);
        //                PopupService.DisplayError($"Error while loading {filename ?? "??"} cart", ex);
        //            }
        //        }
        //    }
        //}

        public bool FindAndRemoveInvalidArticles()
        {
            bool unknownArticleFound = false;
            foreach (ClientShoppingCartViewModel client in Clients.Where(c => c.ShoppingCart.ShoppingCartArticles.Any(a => a.Article == null)))
            {
                client.ShoppingCart.ShoppingCartArticles.RemoveAll(x => x.Article == null);
                unknownArticleFound = true;
            }
            return unknownArticleFound;
        }

        public List<TransactionFullArticle> BuildTransactions()
        {
            List<TransactionFullArticle> transactions = Clients.Select(client => new TransactionFullArticle
            {
                Timestamp = client.PaymentTimestamp,
                Articles = client.ShoppingCart.ShoppingCartArticles.Select(x => new FullArticle
                {
                    Guid = x.Article.Guid,
                    Ean = x.Article.Ean,
                    Description = x.Article.Description,
                    Category = x.Article.Category,
                    SubCategory = x.Article.SubCategory,
                    Price = x.Article.Price,
                    Quantity = x.Quantity
                }).ToList(),
                Cash = client.Cash,
                BankCard = client.BankCard,
                DiscountPercentage = client.DiscountPercentage
            }).ToList();
            return transactions;
        }

        public ClientShoppingCartsViewModel(Action<ShopTransactionItem> addTransactionAction, Action<decimal, decimal, decimal> clientCartPaidAction, Action clientCartReopenedAction)
        {
            _addTransactionAction = addTransactionAction;
            _clientCartPaidAction = clientCartPaidAction;
            _clientCartReopenedAction = clientCartReopenedAction;

            Clients = new ObservableCollection<ClientShoppingCartViewModel>();
            Clients.CollectionChanged += (sender, args) =>
            {
                RefreshCounters();
            };

            Mediator.Default.Register<PlayerSelectedMessage>(this, PlayerSelected);
        }

        private void PlayerSelected(PlayerSelectedMessage msg)
        {
            // Select shopping cart or create it
            ClientShoppingCartViewModel client = Clients.FirstOrDefault(x => x.DciNumber == msg.DciNumber && x.ClientFirstName == msg.FirstName && x.ClientLastName == msg.LastName);
            if (client == null)
            {
                ClientShoppingCartViewModel newClient = new ClientShoppingCartViewModel(ClientCartPaid, ClientCartReopened)
                {
                    HasFullPlayerInfos = true,
                    ClientName = msg.FirstName,
                    ClientFirstName = msg.FirstName,
                    ClientLastName = msg.LastName,
                    DciNumber = msg.DciNumber
                };
                Clients.Add(newClient);
                SelectedClient = newClient;
            }
            else
                SelectedClient = client;
        }

        private void RefreshCounters()
        {
            RaisePropertyChanged(() => ClientShoppingCartsCount);
            RaisePropertyChanged(() => PaidClientShoppingCartsCount);
            RaisePropertyChanged(() => UnpaidClientShoppingCartsCount);
            RaisePropertyChanged(() => HasUnpaidClientShoppingCards);
            RaisePropertyChanged(() => ClientShoppingCartsTotal);
            RaisePropertyChanged(() => PaidClientShoppingCartsTotal);
            RaisePropertyChanged(() => UnpaidClientShoppingCartsTotal);
            RaisePropertyChanged(() => HasClientShoppingCartsOpened);
        }

        // TODO: RefreshCounters should be called when an item is added in shopping cart

        private void ClientCartReopened()
        {
            RefreshCounters();

             _clientCartReopenedAction?.Invoke();
        }

        private void ClientCartPaid(decimal cash, decimal bankCard, decimal discountPercentage)
        {
            RefreshCounters();

            _clientCartPaidAction?.Invoke(cash, bankCard, discountPercentage);
        }
    }

    public class ClientShoppingCartsViewModelDesignData : ClientShoppingCartsViewModel
    {
        public ClientShoppingCartsViewModelDesignData() : base(_ => { }, (a, b, c) => { }, () => { })
        {
            Clients.Clear();
            Clients.AddRange(new List<ClientShoppingCartViewModel>
            {
                new ClientShoppingCartViewModelDesignData
                {
                    ClientName = "un super long nom0",
                    //PaymentState = ClientShoppingCartPaymentStates.Unpaid
                },

                new ClientShoppingCartViewModelDesignData
                {
                    ClientName = "Joel",
                   // PaymentState = ClientShoppingCartPaymentStates.Paid
                },
                new ClientShoppingCartViewModelDesignData
                {
                    ClientName = "Pouet",
                    //PaymentState = ClientShoppingCartPaymentStates.Unpaid
                }
            });
            Clients.AddRange(Enumerable.Range(1, 5).Select(x => new ClientShoppingCartViewModelDesignData
            {
                ClientName = $"un super long nom[{x}]",
                //PaymentState = x % 2 == 0 ? ClientShoppingCartPaymentStates.Unpaid : ClientShoppingCartPaymentStates.Paid
            }));
        }
    }
}
