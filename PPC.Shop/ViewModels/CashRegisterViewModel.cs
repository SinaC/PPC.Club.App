using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using PPC.DataContracts;
using PPC.Helpers;
using PPC.Popup;

namespace PPC.Shop.ViewModels
{
    public class CashRegisterViewModel : ShoppingCartBasedViewModelBase
    {
        public const string ShopFile = "_shop.xml";

        private IPopupService PopupService => EasyIoc.IocContainer.Default.Resolve<IPopupService>();
        private readonly Action _cartPaidAction;

        public ObservableCollection<ShopTransactionItem> Transactions { get; }

        public void Load()
        {
            try
            {
                string filename = $"{ConfigurationManager.AppSettings["BackupPath"]}{ShopFile}";
                if (File.Exists(filename))
                {
                    DataContracts.Shop shop;
                    using (XmlTextReader reader = new XmlTextReader(filename))
                    {
                        System.Runtime.Serialization.DataContractSerializer serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(DataContracts.Shop));
                        shop = (DataContracts.Shop)serializer.ReadObject(reader);
                    }
                    Transactions.Clear();
                    Transactions.AddRange(shop.Transactions.Select(t => new ShopTransactionItem
                    {
                        Timestamp = t.Timestamp,
                        Articles = t.Articles.Select(a => new ShopArticleItem
                        {
                            Article = ArticleDb.Articles.FirstOrDefault(x => x.Guid == a.Guid),
                            Quantity = a.Quantity
                        }).ToList(),
                        Cash = t.Cash,
                        BankCard = t.BankCard
                    }));
                    Cash = Transactions.Sum(x => x.Cash);
                    BankCard = Transactions.Sum(x => x.BankCard);
                }
                else
                {
                    ErrorPopupViewModel vm = new ErrorPopupViewModel("Shop file not found");
                    PopupService.DisplayModal(vm, "Error while loading shop");
                }
            }
            catch (Exception ex)
            {
                ErrorPopupViewModel vm = new ErrorPopupViewModel(ex);
                PopupService.DisplayModal(vm, "Error while loading shop");
            }
        }

        public void AddTransactionFromClosedTab(ShopTransactionItem transaction)
        {
            Transactions.Add(transaction);
            Save();
        }

        public CashRegisterViewModel(Action cartPaidAction)
        {
            PaymentState = PaymentStates.Irrelevant;

            _cartPaidAction = cartPaidAction;

            Transactions = new ObservableCollection<ShopTransactionItem>();
            ShoppingCart = new ShoppingCartViewModel(Payment);
        }

        private void Payment(decimal cash, decimal bankCard)
        {
            // Create shoptransaction from shopping cart
            ShopTransactionItem transaction = new ShopTransactionItem
            {
                Timestamp = DateTime.Now,
                Articles = ShoppingCart.ShoppingCartArticles.Select(x => new ShopArticleItem
                {
                    Article = x.Article,
                    Quantity = x.Quantity
                }).ToList(),
                Cash = cash,
                BankCard = bankCard
            };
            Transactions.Add(transaction);

            // Add cash/bank card
            Cash += cash;
            BankCard += bankCard;

            // Clear shopping cart
            ShoppingCart.Clear();

            // Save
            Save();

            // Call cart paid action
            _cartPaidAction();
        }

        private void Save()
        {
            try
            {
                if (!Directory.Exists(ConfigurationManager.AppSettings["BackupPath"]))
                    Directory.CreateDirectory(ConfigurationManager.AppSettings["BackupPath"]);
                DataContracts.Shop shop = new DataContracts.Shop
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
                string filename = $"{ConfigurationManager.AppSettings["BackupPath"]}{ShopFile}";
                using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
                {
                    writer.Formatting = Formatting.Indented;
                    System.Runtime.Serialization.DataContractSerializer serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(DataContracts.Shop));
                    serializer.WriteObject(writer, shop);
                }
            }
            catch (Exception ex)
            {
                ErrorPopupViewModel vm = new ErrorPopupViewModel(ex);
                PopupService.DisplayModal(vm, "Error while saving shop");
            }
        }
    }

    public class CashRegisterViewModelDesignData : CashRegisterViewModel
    {
        public CashRegisterViewModelDesignData() :
            base(() => { })
        {
        }
    }
}
