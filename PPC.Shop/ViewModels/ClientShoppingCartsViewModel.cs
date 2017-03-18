using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows.Input;
using EasyMVVM;
using PPC.Data.Contracts;
using PPC.Helpers;
using PPC.Messages;
using PPC.Popups;
using PPC.Shop.Models;

namespace PPC.Shop.ViewModels
{
    public enum ClientShoppingCartsMode
    {
        List,
        Detail
    }

    public class ClientShoppingCartsViewModel : ObservableObject
    {
        private IPopupService PopupService => EasyIoc.IocContainer.Default.Resolve<IPopupService>();

        private readonly Action<ShopTransactionItem> _addTransactionAction;
        private readonly Action<decimal, decimal> _clientCartPaidAction;
        private readonly Action _clientCartReopenedAction;

        public bool HasClientShoppingCartsOpened => ClientShoppingCartsCount > 0;

        public int ClientShoppingCartsCount => Clients.Count;

        public int PaidClientShoppingCartsCount => Clients.Count(x => x.PaymentState == ClientShoppingCartPaymentStates.Paid);

        public int UnpaidClientShoppingCartsCount => Clients.Count(x => x.PaymentState == ClientShoppingCartPaymentStates.Unpaid);

        private ClientShoppingCartsMode _mode;

        public ClientShoppingCartsMode Mode
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
                        Mode = ClientShoppingCartsMode.List;
                    else
                        Mode = ClientShoppingCartsMode.Detail;
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
            AskNamePopupViewModel vm = new AskNamePopupViewModel(PopupService, AddNewClientNameSelected);
            PopupService.DisplayModal(vm, "Client name?");
        }

        private void AddNewClientNameSelected(string name)
        {
            if (Clients.Any(x => x.ClientName == name))
            {
                PopupService.DisplayError("Error", "A shopping cart with that client name has already been opened!");
            }
            else
            {
                ClientShoppingCartViewModel newClient = new ClientShoppingCartViewModel(_clientCartPaidAction, _clientCartReopenedAction)
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
                PopupService.DisplayQuestion($"Close {client.ClientName} shopping cart", $"Client {client.ClientName} has not paid yet. shopping cart cannot be closed.",
                    new ActionButton
                    {
                        Caption = "Ok",
                        Order = 2,
                        ClickCallback = () => { }
                    });
            else
                PopupService.DisplayQuestion($"Close {client.ClientName} shopping cart", "Are you sure ?",
                    new ActionButton
                    {
                        Caption = "Yes",
                        Order = 1,
                        ClickCallback = () => CloseClientConfirmed(client)
                    },
                    new ActionButton
                    {
                        Caption = "No",
                        Order = 2,
                        ClickCallback = () => { }
                    });
        }

        private void CloseClientConfirmed(ClientShoppingCartViewModel client)
        {
            SelectedClient = null;
            Mode = ClientShoppingCartsMode.List;
            Clients.Remove(client);
            if (client.ShoppingCart.ShoppingCartArticles.Any())
            {
                // Create a transaction and add it to transactions
                ShopTransactionItem transaction = new ShopTransactionItem
                {
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
                PopupService.DisplayError("Error while deleting backup file", ex);
            }
            //
            client.RemoveHandlers();
        }

        #endregion

        public void LoadClients(string shopFilename)
        {
            SelectedClient = null;
            Clients.Clear();
            //  add backup clients
            foreach (string filename in Directory.EnumerateFiles(ConfigurationManager.AppSettings["BackupPath"], "*.xml", SearchOption.TopDirectoryOnly).Where(x => !x.Contains(shopFilename)))
            {
                try
                {
                    ClientShoppingCartViewModel client = new ClientShoppingCartViewModel(_clientCartPaidAction, _clientCartReopenedAction, filename);
                    Clients.Add(client);
                }
                catch (Exception ex)
                {
                    PopupService.DisplayError($"Error while loading {filename} cart", ex);
                }
            }
        }

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
                    Price = x.Article.Price,
                    Quantity = x.Quantity
                }).ToList(),
                Cash = client.Cash,
                BankCard = client.BankCard
            }).ToList();
            return transactions;
        }

        public ClientShoppingCartsViewModel(Action<ShopTransactionItem> addTransactionAction, Action<decimal, decimal> clientCartPaidAction, Action clientCartReopenedAction)
        {
            _addTransactionAction = addTransactionAction;
            _clientCartPaidAction = clientCartPaidAction;
            _clientCartReopenedAction = clientCartReopenedAction;

            Clients = new ObservableCollection<ClientShoppingCartViewModel>();
            Clients.CollectionChanged += (sender, args) =>
            {
                RaisePropertyChanged(() => ClientShoppingCartsCount);
                RaisePropertyChanged(() => PaidClientShoppingCartsCount);
                RaisePropertyChanged(() => UnpaidClientShoppingCartsCount);
                RaisePropertyChanged(() => HasClientShoppingCartsOpened);
            };

            Mediator.Default.Register<PlayerSelectedMessage>(this, PlayerSelected);
        }

        private void PlayerSelected(PlayerSelectedMessage msg)
        {
            // Select shopping cart or create it
            ClientShoppingCartViewModel client = Clients.FirstOrDefault(x => x.DciNumber == msg.DciNumber && x.ClientFirstName == msg.FirstName && x.ClientLastName == msg.LastName);
            if (client == null)
            {
                ClientShoppingCartViewModel newClient = new ClientShoppingCartViewModel(_clientCartPaidAction, _clientCartReopenedAction)
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
    }

    public class ClientShoppingCartsViewModelDesignData : ClientShoppingCartsViewModel
    {
        public ClientShoppingCartsViewModelDesignData() : base(_ => { }, (c, b) => { }, () => { })
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
