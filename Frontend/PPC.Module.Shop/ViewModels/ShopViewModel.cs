using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using EasyIoc;
using EasyMVVM;
using PPC.Domain;
using PPC.Helpers;
using PPC.IDataAccess;
using PPC.Log;
using PPC.Module.Shop.Models;
using PPC.Module.Shop.ViewModels.Popups;
using PPC.Services.Popup;
using PPC.Module.Common;

namespace PPC.Module.Shop.ViewModels
{
    public enum ShopModes
    {
        CashRegister,
        ClientShoppingCarts,
        SoldArticles
    }

    public class ShopViewModel : ObservableObject, IReloadModule
    {
        private IPopupService PopupService => IocContainer.Default.Resolve<IPopupService>();
        private ILog Logger => IocContainer.Default.Resolve<ILog>();
        private ISessionDL SessionDL => IocContainer.Default.Resolve<ISessionDL>();

        #region Shop mode

        private ShopModes _mode;
        public ShopModes Mode
        {
            get => _mode;
            protected set { Set(() => Mode, ref _mode, value); }
        }

        #endregion

        #region Cash register

        private CashRegisterViewModel _cashRegisterViewModel;
        public CashRegisterViewModel CashRegisterViewModel
        {
            get => _cashRegisterViewModel;
            protected set { Set(() => CashRegisterViewModel, ref _cashRegisterViewModel, value); }
        }

        #endregion

        #region Clients shopping cart

        private ClientShoppingCartsViewModel _clientShoppingCartsViewModel;
        public ClientShoppingCartsViewModel ClientShoppingCartsViewModel
        {
            get => _clientShoppingCartsViewModel;
            protected set { Set(() => ClientShoppingCartsViewModel, ref _clientShoppingCartsViewModel, value); }
        }

        #endregion

        #region Sold articles

        private List<ShopArticleItem> _soldArticles;
        public List<ShopArticleItem> SoldArticles
        {
            get => _soldArticles;
            protected set { Set(() => SoldArticles, ref _soldArticles, value); }
        }

        #endregion

        #region Computed values

        private int _soldArticlesCount;

        public int SoldArticlesCount
        {
            get => _soldArticlesCount;
            protected set { Set(() => SoldArticlesCount, ref _soldArticlesCount, value); }
        }

        private decimal _soldArticlesTotal;

        public decimal SoldArticlesTotal
        {
            get => _soldArticlesTotal;
            protected set { Set(() => SoldArticlesTotal, ref _soldArticlesTotal, value); }
        }

        private decimal _soldArticlesTotalCash;

        public decimal SoldArticlesTotalCash
        {
            get => _soldArticlesTotalCash;
            protected set { Set(() => SoldArticlesTotalCash, ref _soldArticlesTotalCash, value); }
        }

        private decimal _soldArticlesTotalBankCard;

        public decimal SoldArticlesTotalBankCard
        {
            get => _soldArticlesTotalBankCard;
            protected set { Set(() => SoldArticlesTotalBankCard, ref _soldArticlesTotalBankCard, value); }
        }

        #endregion

        #region Transactions

        public decimal Cash => Transactions?.Sum(x => x.Cash) ?? 0;

        public decimal BankCard => Transactions?.Sum(x => x.BankCard) ?? 0;

        private ObservableCollection<ShopTransactionItem> _transactions;
        public ObservableCollection<ShopTransactionItem> Transactions
        {
            get => _transactions;
            protected set
            {
                if (Set(() => Transactions, ref _transactions, value))
                {
                    RaisePropertyChanged(() => Cash);
                    RaisePropertyChanged(() => BankCard);
                }
            }
        }

        private void AddTransaction(ShopTransactionItem transaction)
        {
            Transactions.Add(transaction);
            RaisePropertyChanged(() => Cash);
            RaisePropertyChanged(() => BankCard);

            SaveTransactions();
            RefreshSoldArticles();
        }

        private void SaveTransactions() // TODO: remove this and use SaveTransaction/DeleteTransaction from DL
        {
            try
            {
                List<ShopTransaction> transactions = Transactions.Select(t => new ShopTransaction
                {
                    Guid = t.Id,
                    Timestamp = t.Timestamp,
                    Articles = t.Articles.Select(a => new Item
                    {
                        Guid = a.Article.Guid,
                        Quantity = a.Quantity,
                    }).ToList(),
                    Cash = t.Cash,
                    BankCard = t.BankCard,
                    DiscountPercentage = t.DiscountPercentage
                }).ToList();
                SessionDL.SaveTransactions(transactions);
            }
            catch (Exception ex)
            {
                Logger.Exception("Error while saving transactions", ex);
                PopupService.DisplayError("Error while saving transactions", ex);
            }
        }

        private ICommand _editTransactionCommand;
        public ICommand EditTransactionCommand => _editTransactionCommand = _editTransactionCommand ?? new RelayCommand<ShopTransactionItem>(EditTransaction);

        private void EditTransaction(ShopTransactionItem transactionItem)
        {
            TransactionEditorPopupViewModel vm = new TransactionEditorPopupViewModel(transactionItem, SaveEditedTransaction, DeleteTransactionConfirmed);
            PopupService.DisplayModal(vm, "Edit transaction");
        }

        private void SaveEditedTransaction(ShopTransactionItem transactionItem)
        {
            // Search transaction based on Id, remote it, insert new transaction
            Transactions.RemoveAll(x => x.Id == transactionItem.Id);
            AddTransaction(transactionItem);
        }

        private ICommand _deleteTransactionCommand;
        public ICommand DeleteTransactionCommand => _deleteTransactionCommand = _deleteTransactionCommand ?? new RelayCommand<ShopTransactionItem>(DeleteTransaction);

        private void DeleteTransaction(ShopTransactionItem transactionItem)
        {
            TransactionDeleteConfirmationPopupViewModel vm = new TransactionDeleteConfirmationPopupViewModel(transactionItem, DeleteTransactionConfirmed);
            PopupService.DisplayModal(vm, "Delete transaction");
        }

        private void DeleteTransactionConfirmed(ShopTransactionItem transactionItem)
        {
            // Search transaction based on Id, remote it, insert new transaction
            Transactions.RemoveAll(x => x.Id == transactionItem.Id);
            RaisePropertyChanged(() => Cash);
            RaisePropertyChanged(() => BankCard);

            SaveTransactions();

            RefreshSoldArticles();
        }

        #endregion

        #region Paid carts

        private ObservableCollection<ShopTransactionItem> _paidCarts;
        public ObservableCollection<ShopTransactionItem> PaidCarts
        {
            get => _paidCarts;
            protected set { Set(() => PaidCarts, ref _paidCarts, value); }
        }

        public decimal PaidCartsCount => PaidCarts.Count;

        #endregion

        public void ViewCashRegister()
        {
            Mode = ShopModes.CashRegister;
            CashRegisterViewModel.ShoppingCart.GotFocus();
        }

        public void ViewShoppingCarts()
        {
            ClientShoppingCartsViewModel.SelectClientCommand.Execute(null); // unselect client (parameter null)
            Mode = ShopModes.ClientShoppingCarts;
        }

        public void ViewSoldArticles()
        {
            Mode = ShopModes.SoldArticles;
        }

        #region IReloadModule

        public void Reload(Session session)
        {
            ReloadTransactions(session);
            ReloadClients(session);

            // If transactions / clients contains unknown article->remove them and display a warning
            bool unknownArticleFound = false;
            foreach (ShopTransactionItem transaction in Transactions.Where(t => t.Articles.Any(a => a.Article == null)))
            {
                transaction.Articles.RemoveAll(x => x.Article == null);
                unknownArticleFound = true;
            }

            if (ClientShoppingCartsViewModel.FindAndRemoveInvalidArticles())
                unknownArticleFound = true;

            if (unknownArticleFound)
            {
                Logger.Warning("Unknown articles have been found and removed!");
                PopupService.DisplayError("Warning", "Unknown articles have been found and removed!");
                // TODO: 
                // recompute transaction cash/bank if possible
                // recompute client cash/bank if possible
            }

            RefreshSoldArticles();
        }

        #region IModule

        public void GotFocus()
        {

        }

        #endregion

        #endregion

        public CashRegisterClosure PrepareClosure()
        {
            // Compute shop closure
            //  transactions from cash register
            List<TransactionFullArticle> transactions = Transactions.Select(t => new TransactionFullArticle
            {
                Timestamp = t.Timestamp,
                Articles = t.Articles.Select(x => new FullArticle
                {
                    Guid = x.Article.Guid,
                    Ean = x.Article.Ean,
                    Description = x.Article.Description,
                    Category = x.Article.Category,
                    SubCategory = x.Article.SubCategory,
                    Price = x.Article.Price,
                    Quantity = x.Quantity
                }).ToList(),
                Cash = t.Cash,
                BankCard = t.BankCard,
                DiscountPercentage = t.DiscountPercentage
            }).ToList();
            //  transactions from clients shopping cart
            transactions.AddRange(ClientShoppingCartsViewModel.BuildTransactions());

            //  closure summary
            CashRegisterClosure closure = new CashRegisterClosure
            {
                Articles = SoldArticles?.Select(x => new FullArticle
                {
                    Guid = x.Article.Guid,
                    Ean = x.Article.Ean,
                    Description = x.Article.Description,
                    Category = x.Article.Category,
                    SubCategory = x.Article.SubCategory,
                    Price = x.Article.Price,
                    Quantity = x.Quantity
                }).ToList(),
                Cash = SoldArticlesTotalCash,
                BankCard = SoldArticlesTotalBankCard,
                Transactions = transactions
            };
            return closure;
        }

        public ShopViewModel()
        {
            Mode = ShopModes.CashRegister;

            Transactions = new ObservableCollection<ShopTransactionItem>();
            CashRegisterViewModel = new CashRegisterViewModel(AddTransaction);
            ClientShoppingCartsViewModel = new ClientShoppingCartsViewModel(AddTransaction, ClientCartPaid, RefreshSoldArticles);
            SoldArticles = new List<ShopArticleItem>();
            PaidCarts = new ObservableCollection<ShopTransactionItem>();

            CashRegisterViewModel.ShoppingCart.GotFocus();
        }

        private void ClientCartPaid(decimal cash, decimal bankCard, decimal discountPercentage)
        {
            ViewShoppingCarts();

            RefreshSoldArticles();
        }

        private void RefreshSoldArticles()
        {
            decimal totalCash = 0;
            decimal totalBankCard = 0;
            Dictionary<Guid, ShopArticleItem> soldItems = new Dictionary<Guid, ShopArticleItem>();
            // Gather sold items in closed transactions
            foreach (ShopArticleItem item in Transactions.SelectMany(x => x.Articles))
            {
                if (!soldItems.TryGetValue(item.Article.Guid, out var soldItem))
                {
                    soldItem = new ShopArticleItem
                    {
                        Article = item.Article,
                        Quantity = 0
                    };
                    soldItems.Add(item.Article.Guid, soldItem);
                }
                soldItem.Quantity += item.Quantity;
            }
            totalCash += Cash;
            totalBankCard += BankCard;
            // Gather sold items in closed client shopping carts + compute pseudo transaction for paid carts
            PaidCarts.Clear();
            foreach (ClientShoppingCartViewModel client in ClientShoppingCartsViewModel.Clients.Where(x => x.PaymentState == ClientShoppingCartPaymentStates.Paid))
            {
                ShopTransactionItem paidCart = new ShopTransactionItem
                {
                    Articles = new List<ShopArticleItem>()
                };
                foreach (ShopArticleItem item in client.ShoppingCart.ShoppingCartArticles)
                {
                    if (!soldItems.TryGetValue(item.Article.Guid, out var soldItem))
                    {
                        soldItem = new ShopArticleItem
                        {
                            Article = item.Article,
                            Quantity = 0,
                        };
                        soldItems.Add(item.Article.Guid, soldItem);
                    }
                    soldItem.Quantity += item.Quantity;
                    //
                    paidCart.Articles.Add(new ShopArticleItem
                    {
                        Article = item.Article,
                        Quantity = item.Quantity
                    });
                }
                totalCash += client.Cash;
                totalBankCard += client.BankCard;
                //
                paidCart.Cash = client.Cash;
                paidCart.BankCard = client.BankCard;
                paidCart.DiscountPercentage = client.DiscountPercentage;
                PaidCarts.Add(paidCart);
            }
            //
            SoldArticles = soldItems.Values.ToList();
            // Compute count/total/cash/bc
            SoldArticlesCount = SoldArticles.Sum(x => x.Quantity);
            SoldArticlesTotal = totalCash + totalBankCard;
            SoldArticlesTotalCash = totalCash;
            SoldArticlesTotalBankCard = totalBankCard;
            //
            RaisePropertyChanged(() => PaidCarts);
            RaisePropertyChanged(() => PaidCartsCount);
        }

        private void ReloadTransactions(Session session)
        {
            Transactions = new ObservableCollection<ShopTransactionItem>(session.Transactions.Select(t => new ShopTransactionItem
            {
                Id = t.Guid,
                Timestamp = t.Timestamp,
                Articles = t.Articles.Select(a => new ShopArticleItem
                {
                    Article = IocContainer.Default.Resolve<IArticleDL>().GetById(a.Guid),
                    Quantity = a.Quantity
                }).ToList(),
                Cash = t.Cash,
                BankCard = t.BankCard,
            }));
        }

        private void ReloadClients(Session session)
        {
            ClientShoppingCartsViewModel.ReloadClients(session);
        }
    }

    public class ShopViewModelDesignData : ShopViewModel
    {
        public ShopViewModelDesignData()
        {
            CashRegisterViewModel = new CashRegisterViewModelDesignData();
            ClientShoppingCartsViewModel = new ClientShoppingCartsViewModelDesignData();

            Mode = ShopModes.CashRegister;
        }
    }
}
