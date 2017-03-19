using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using EasyIoc;
using EasyMVVM;
using PPC.Data.Articles;
using PPC.Data.Contracts;
using PPC.Helpers;
using PPC.Messages;
using PPC.Popups;
using PPC.Shop.Models;

namespace PPC.Shop.ViewModels
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

        public decimal Cash => Transactions?.Sum(x => x.Cash) ?? 0;

        public decimal BankCard => Transactions?.Sum(x => x.BankCard) ?? 0;

        #region Shop mode

        private ShopModes _mode;
        public ShopModes Mode
        {
            get { return _mode; }
            protected set { Set(() => Mode, ref _mode, value); }
        }

        private ICommand _viewCashRegisterCommand;
        public ICommand ViewCashRegisterCommand => _viewCashRegisterCommand = _viewCashRegisterCommand ?? new RelayCommand(ViewCashRegister);

        private void ViewCashRegister()
        {
            Mode = ShopModes.CashRegister;
            CashRegisterViewModel.ShoppingCart.GotFocus();
        }

        private ICommand _viewShoppingCartsCommand;
        public ICommand ViewShoppingCartsCommand => _viewShoppingCartsCommand = _viewShoppingCartsCommand ?? new RelayCommand(ViewShoppingCarts);

        private void ViewShoppingCarts()
        {
            ClientShoppingCartsViewModel.SelectClientCommand.Execute(null); // unselect client
            Mode = ShopModes.ClientShoppingCarts;
        }

        private ICommand _viewSoldArticlesCommand;
        public ICommand ViewSoldArticlesCommand => _viewSoldArticlesCommand = _viewSoldArticlesCommand ?? new RelayCommand(() => Mode = ShopModes.SoldArticles);

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

        #region Reload

        private ICommand _reloadCommand;
        public ICommand ReloadCommand => _reloadCommand = _reloadCommand ?? new RelayCommand(Reload);

        private void Reload()
        {
            PopupService.DisplayQuestion("Reload", "Are you sure you want to reload from backup ?",
                   new ActionButton
                   {
                       Order = 1,
                       Caption = "Yes",
                       ClickCallback = ReloadConfirmed
                   },
                   new ActionButton
                   {
                       Order = 2,
                       Caption = "No"
                   });
        }

        private void ReloadConfirmed()
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
                        PopupService.DisplayError("Warning", "Unknown articles have been found and removed!");
                        // TODO: 
                        // recompute transaction cash/bank if possible
                        // recompute client cash/bank if possible
                    }

                    RefreshSoldArticles();

                    PopupService.DisplayQuestion("Reload", "Reload done", new ActionButton
                    {
                        Caption = "Ok"
                    });
                }
                catch (Exception ex)
                {
                    PopupService.DisplayError("Error while loading shop", ex);
                }
            }
            else
            {
                PopupService.DisplayError("Error while loading shop", "Backup path not found");
            }
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
                        shop = (Data.Contracts.Shop) serializer.ReadObject(reader);
                    }
                    Transactions = new ObservableCollection<ShopTransactionItem>(shop.Transactions.Select(t => new ShopTransactionItem
                    {
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
                    PopupService.DisplayError("Error while loading shop", ex);
                }
            }
            else
            {
                PopupService.DisplayError("Error while loading shop", "Shop file not found");
            }
        }

        #endregion

        #region Cash register closure

        private ICommand _cashRegisterClosureCommand;
        public ICommand CashRegisterClosureCommand => _cashRegisterClosureCommand = _cashRegisterClosureCommand ?? new RelayCommand(CashRegisterClosure);

        private void CashRegisterClosure()
        {
            if (ClientShoppingCartsViewModel.UnpaidClientShoppingCartsCount > 0)
                PopupService.DisplayQuestion("Warning", "There is 1 or more client shopping cart opened.",
                    new ActionButton
                    {
                        Order = 1,
                        Caption = "Ok"
                    });
            else
            {
                PopupService.DisplayQuestion("Cash register closure", "Are you sure you want to perform closure ?",
                    new ActionButton
                    {
                        Order = 1,
                        Caption = "Yes",
                        ClickCallback = async () => await CashRegisterClosureConfirmedAsync()
                    },
                    new ActionButton
                    {
                        Order = 2,
                        Caption = "No"
                    });
            }
        }

        private async Task CashRegisterClosureConfirmedAsync()
        {
            CashRegisterClosure closure = BuildClosure();

            // Display popup
            CashRegisterClosurePopupViewModel vm = new CashRegisterClosurePopupViewModel(PopupService, closure, CloseApplicationAfterClosure, async c => await SendCashRegisterClosureMailAsync(closure));
            PopupService.DisplayModal(vm, "Cash register closure"); // !! Shutdown application on close

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
                    await serializer.WriteObjectAsync(writer, closure);
                }
            }
            catch (Exception ex)
            {
                PopupService.DisplayError("Error", ex);
            }
        }

        private CashRegisterClosure BuildClosure()
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
                BankCard = t.BankCard
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
            return closure;
        }

        private void CloseApplicationAfterClosure()
        {
            // Delete backup files
            try
            {
                string backupPath = ConfigurationManager.AppSettings["BackupPath"];
                foreach (string file in Directory.EnumerateFiles(backupPath))
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                PopupService.DisplayError("Error", ex);
            }

            Application.Current.Shutdown();
        }

        private async Task SendCashRegisterClosureMailAsync(CashRegisterClosure closure)
        {
            // Send shop closure mail
            Mediator.Default.Send(new ChangeWaitingMessage { IsWaiting = true });
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

                    // Send mail
                    MailAddress fromAddress = new MailAddress(closureConfig.SenderMail, "From PPC Club");
                    MailAddress toAddress = new MailAddress(closureConfig.RecipientMail, "To PPC");
                    using (SmtpClient client = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(fromAddress.Address, closureConfig.SenderPassword)
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
                else
                    PopupService.DisplayError("Warning", "Cash register closure config file not found -> Cannot send automatically cash register closure mail.");
            }
            catch (Exception ex)
            {
                PopupService.DisplayError("Error", ex);
            }
            finally
            {
                Mediator.Default.Send(new ChangeWaitingMessage { IsWaiting = false });
            }
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

        private ObservableCollection<ShopTransactionItem> _transactions;
        public ObservableCollection<ShopTransactionItem> Transactions
        {
            get { return _transactions; }
            set
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
                        BankCard = t.BankCard
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
                PopupService.DisplayError("Error while saving shop", ex);
            }
        }

        #endregion

        public ShopViewModel()
        {
            Mode = ShopModes.CashRegister;

            Transactions = new ObservableCollection<ShopTransactionItem>();
            CashRegisterViewModel = new CashRegisterViewModel(AddTransaction);
            ClientShoppingCartsViewModel = new ClientShoppingCartsViewModel(AddTransaction, ClientPaid, RefreshSoldArticles);
            SoldArticles = new List<ShopArticleItem>();

            CashRegisterViewModel.ShoppingCart.GotFocus();
        }

        private void ClientPaid(decimal cash, decimal bankCard)
        {
            Mode = ShopModes.CashRegister;

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
            // Gather sold items in closed client shopping carts
            foreach (ClientShoppingCartViewModel client in ClientShoppingCartsViewModel.Clients.Where(x => x.PaymentState == ClientShoppingCartPaymentStates.Paid))
            {
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
                }
                totalCash += client.Cash;
                totalBankCard += client.BankCard;
            }
            //
            SoldArticles = soldItems.Values.ToList();
            // Compute count/total/cash/bc
            SoldArticlesCount = SoldArticles.Sum(x => x.Quantity);
            SoldArticlesTotal = SoldArticles.Sum(x => x.Total);
            SoldArticlesTotalCash = totalCash;
            SoldArticlesTotalBankCard = totalBankCard;
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
