using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Input;
using System.Xml;
using EasyIoc;
using EasyMVVM;
using PPC.Data.Articles;
using PPC.Data.Contracts;
using PPC.Helpers;
using PPC.Log;
using PPC.Module.Shop.Models;
using PPC.Module.Shop.ViewModels.Popups;
using PPC.Services.Popup;

namespace PPC.Module.Shop.ViewModels
{
    public enum ShopModes
    {
        CashRegister,
        ClientShoppingCarts,
        SoldArticles
    }

    public class ShopViewModel : ObservableObject
    {
        private const string ShopFilename = "_shop.xml";

        private IPopupService PopupService => IocContainer.Default.Resolve<IPopupService>();
        private ILog Logger => IocContainer.Default.Resolve<ILog>();

        #region Shop mode

        private ShopModes _mode;
        public ShopModes Mode
        {
            get { return _mode; }
            protected set { Set(() => Mode, ref _mode, value); }
        }

        #endregion

        #region Cash register

        private CashRegisterViewModel _cashRegisterViewModel;
        public CashRegisterViewModel CashRegisterViewModel
        {
            get { return _cashRegisterViewModel; }
            protected set { Set(() => CashRegisterViewModel, ref _cashRegisterViewModel, value); }
        }

        #endregion

        #region Clients shopping cart

        private ClientShoppingCartsViewModel _clientShoppingCartsViewModel;
        public ClientShoppingCartsViewModel ClientShoppingCartsViewModel
        {
            get { return _clientShoppingCartsViewModel; }
            protected set { Set(() => ClientShoppingCartsViewModel, ref _clientShoppingCartsViewModel, value); }
        }

        #endregion

        #region Sold articles

        private List<ShopArticleItem> _soldArticles;
        public List<ShopArticleItem> SoldArticles
        {
            get { return _soldArticles; }
            protected set { Set(() => SoldArticles, ref _soldArticles, value); }
        }

        #endregion

        #region Computed values

        private int _soldArticlesCount;

        public int SoldArticlesCount
        {
            get { return _soldArticlesCount; }
            protected set { Set(() => SoldArticlesCount, ref _soldArticlesCount, value); }
        }

        private decimal _soldArticlesTotal;

        public decimal SoldArticlesTotal
        {
            get { return _soldArticlesTotal; }
            protected set { Set(() => SoldArticlesTotal, ref _soldArticlesTotal, value); }
        }

        private decimal _soldArticlesTotalCash;

        public decimal SoldArticlesTotalCash
        {
            get { return _soldArticlesTotalCash; }
            protected set { Set(() => SoldArticlesTotalCash, ref _soldArticlesTotalCash, value); }
        }

        private decimal _soldArticlesTotalBankCard;

        public decimal SoldArticlesTotalBankCard
        {
            get { return _soldArticlesTotalBankCard; }
            protected set { Set(() => SoldArticlesTotalBankCard, ref _soldArticlesTotalBankCard, value); }
        }

        #endregion

        #region Transactions

        public decimal Cash => Transactions?.Sum(x => x.Cash) ?? 0;

        public decimal BankCard => Transactions?.Sum(x => x.BankCard) ?? 0;

        private ObservableCollection<ShopTransactionItem> _transactions;
        public ObservableCollection<ShopTransactionItem> Transactions
        {
            get { return _transactions; }
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

        private void SaveTransactions()
        {
            try
            {
                if (!Directory.Exists(ConfigurationManager.AppSettings["BackupPath"]))
                    Directory.CreateDirectory(ConfigurationManager.AppSettings["BackupPath"]);
                Data.Contracts.Shop shop = new Data.Contracts.Shop
                {
                    Transactions = Transactions.Select(t => new ShopTransaction
                    {
                        Timestamp = t.Timestamp,
                        Articles = t.Articles.Select(a => new Item
                        {
                            Guid = a.Article.Guid,
                            Quantity = a.Quantity,
                        }).ToList(),
                        Cash = t.Cash,
                        BankCard = t.BankCard,
                        DiscountPercentage = t.DiscountPercentage
                    }).ToList(),
                };
                string filename = $"{ConfigurationManager.AppSettings["BackupPath"]}{ShopFilename}";
                using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
                {
                    writer.Formatting = Formatting.Indented;
                    DataContractSerializer serializer = new DataContractSerializer(typeof(Data.Contracts.Shop));
                    serializer.WriteObject(writer, shop);
                }
            }
            catch (Exception ex)
            {
                Logger.Exception("Error while saving shop", ex);
                PopupService.DisplayError("Error while saving shop", ex);
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

        private List<ShopTransactionItem> _paidCarts;
        public List<ShopTransactionItem> PaidCarts
        {
            get { return _paidCarts; }
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

        public void Reload()
        {
            if (Directory.Exists(ConfigurationManager.AppSettings["BackupPath"]))
            {
                try
                {
                    // Reload transactions
                    LoadTransactions();
                    // Reload clients
                    ClientShoppingCartsViewModel.LoadClients(ShopFilename);

                    // If transactions/clients contains unknown article -> remove them and display a warning
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
                catch (Exception ex)
                {
                    Logger.Exception("Error while loading shop", ex);
                    PopupService.DisplayError("Error while loading shop", ex);
                }
            }
            else
            {
                Logger.Error("Error while loading shop: Backup path not found");
                PopupService.DisplayError("Error while loading shop", "Backup path not found");
            }
        }

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
                    Price = x.Article.Price,
                    Quantity = x.Quantity
                }).ToList(),
                Cash = SoldArticlesTotalCash,
                BankCard = SoldArticlesTotalBankCard,
                Transactions = transactions
            };

            // Dump cash register closure file
            DateTime now = DateTime.Now;
            //  txt
            try
            {
                if (!Directory.Exists(ConfigurationManager.AppSettings["CashRegisterClosurePath"]))
                    Directory.CreateDirectory(ConfigurationManager.AppSettings["CashRegisterClosurePath"]);
                string filename = $"{ConfigurationManager.AppSettings["CashRegisterClosurePath"]}CashRegister_{now:yyyy-MM-dd_HH-mm-ss}.txt";
                File.WriteAllText(filename, closure.ToString());
            }
            catch (Exception ex)
            {
                Logger.Exception("Error", ex);
                PopupService.DisplayError("Error", ex);
            }
            //  xml
            try
            {
                if (!Directory.Exists(ConfigurationManager.AppSettings["CashRegisterClosurePath"]))
                    Directory.CreateDirectory(ConfigurationManager.AppSettings["CashRegisterClosurePath"]);
                string filename = $"{ConfigurationManager.AppSettings["CashRegisterClosurePath"]}CashRegister_{now:yyyy-MM-dd_HH-mm-ss}.xml";
                using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
                {
                    writer.Formatting = Formatting.Indented;
                    DataContractSerializer serializer = new DataContractSerializer(typeof(CashRegisterClosure));
                    serializer.WriteObject(writer, closure);
                }
            }
            catch (Exception ex)
            {
                Logger.Exception("Error", ex);
                PopupService.DisplayError("Error", ex);
            }
            return closure;
        }

        public void DeleteBackupFiles(string savePath)
        {
            // Move backup files into save folder
            try
            {
                string backupPath = ConfigurationManager.AppSettings["BackupPath"];
                foreach (string file in Directory.EnumerateFiles(backupPath))
                {
                    string saveFilename = savePath + Path.GetFileName(file);
                    File.Move(file, saveFilename);
                }
            }
            catch (Exception ex)
            {
                Logger.Exception("Error", ex);
                PopupService.DisplayError("Error", ex);
            }
        }

        public ShopViewModel()
        {
            Mode = ShopModes.CashRegister;

            Transactions = new ObservableCollection<ShopTransactionItem>();
            CashRegisterViewModel = new CashRegisterViewModel(AddTransaction);
            ClientShoppingCartsViewModel = new ClientShoppingCartsViewModel(AddTransaction, ClientPaid, RefreshSoldArticles);
            SoldArticles = new List<ShopArticleItem>();
            PaidCarts = new List<ShopTransactionItem>();

            CashRegisterViewModel.ShoppingCart.GotFocus();
        }

        private void ClientPaid(decimal cash, decimal bankCard, decimal discountPercentage)
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
                ShopArticleItem soldItem;
                if (!soldItems.TryGetValue(item.Article.Guid, out soldItem))
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
                    ShopArticleItem soldItem;
                    if (!soldItems.TryGetValue(item.Article.Guid, out soldItem))
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

        private void LoadTransactions()
        {
            string filename = $"{ConfigurationManager.AppSettings["BackupPath"]}{ShopFilename}";
            if (File.Exists(filename))
            {
                try
                {
                    Data.Contracts.Shop shop;
                    using (XmlTextReader reader = new XmlTextReader(filename))
                    {
                        DataContractSerializer serializer = new DataContractSerializer(typeof(Data.Contracts.Shop));
                        shop = (Data.Contracts.Shop)serializer.ReadObject(reader);
                    }
                    Transactions = new ObservableCollection<ShopTransactionItem>(shop.Transactions.Select(t => new ShopTransactionItem
                    {
                        Id = Guid.NewGuid(),
                        Timestamp = t.Timestamp,
                        Articles = t.Articles.Select(a => new ShopArticleItem
                        {
                            Article = IocContainer.Default.Resolve<IArticleDb>().GetById(a.Guid),
                            Quantity = a.Quantity
                        }).ToList(),
                        Cash = t.Cash,
                        BankCard = t.BankCard,
                    }));
                }
                catch (Exception ex)
                {
                    Logger.Exception("Error while loading shop", ex);
                    PopupService.DisplayError("Error while loading shop", ex);
                }
            }
            else
            {
                Logger.Error("Error while loading shop: Shop file not found");
                PopupService.DisplayError("Error while loading shop", "Shop file not found");
            }
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
