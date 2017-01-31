using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using PPC.DataContracts;
using PPC.Helpers;
using PPC.Popup;

namespace PPC.Sale
{
    public class ShopViewModel : ShoppingCartTabBase
    {
        public const string ShopFile = "_shop.xml";

        private IPopupService PopupService => EasyIoc.IocContainer.Default.Resolve<IPopupService>();
        private readonly Action _cartPaidAction;

        #region ShoppingCartTabBase

        public override string Header => "Shop";

        #endregion

        public ObservableCollection<SoldArticleItem> SoldArticles { get; }

        public void Load()
        {
            try
            {
                string filename = $"{ConfigurationManager.AppSettings["BackupPath"]}{ShopFile}";
                if (File.Exists(filename))
                {
                    Shop shop;
                    using (XmlTextReader reader = new XmlTextReader(filename))
                    {
                        DataContractSerializer serializer = new DataContractSerializer(typeof(Shop));
                        shop = (Shop)serializer.ReadObject(reader);
                    }
                    SoldArticles.Clear();
                    SoldArticles.AddRange(shop.Articles.Select(x => new SoldArticleItem
                    {
                        Article = ArticleDb.Articles.FirstOrDefault(a => a.Guid == x.Guid),
                        Quantity = x.Quantity,
                    }));
                    Cash = shop.Cash;
                    BankCard = shop.BankCard;
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

        public ShopViewModel(Action cartPaidAction)
        {
            IsPaid = false; // always false

            _cartPaidAction = cartPaidAction;

            SoldArticles = new ObservableCollection<SoldArticleItem>();
            ShoppingCart = new ShoppingCartViewModel(Payment);
        }

        private void Payment(double cash, double bankCard)
        {
            // Add shopping cart articles to sold articles
            foreach (ShoppingCartArticleItem shoppingCartArticle in ShoppingCart.ShoppingCartArticles)
            {
                SoldArticleItem soldArticle = SoldArticles.FirstOrDefault(x => x.Article.Guid == shoppingCartArticle.Article.Guid);
                if (soldArticle == null)
                {
                    soldArticle = new SoldArticleItem
                    {
                        Article = shoppingCartArticle.Article
                    };
                    SoldArticles.Add(soldArticle);
                }
                soldArticle.Quantity += shoppingCartArticle.Quantity;
            }

            // Add cash/bank card
            Cash += cash;
            BankCard += BankCard;

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
                Shop shop = new Shop
                {
                    Articles = SoldArticles.Select(x => new Item
                    {
                        Guid = x.Article.Guid,
                        Quantity = x.Quantity,
                    }).ToList(),
                    Cash = Cash,
                    BankCard = BankCard
                };
                string filename = $"{ConfigurationManager.AppSettings["BackupPath"]}{ShopFile}";
                using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
                {
                    writer.Formatting = Formatting.Indented;
                    DataContractSerializer serializer = new DataContractSerializer(typeof(Shop));
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

    public class ShopViewModelDesignData : ShopViewModel
    {
        public ShopViewModelDesignData() : base(() => { })
        {
            ShoppingCart = new ShoppingCartViewModelDesignData();
            SoldArticles.AddRange( new []
            {
                new SoldArticleItem
                {
                    Article = new Article
                    {
                        Ean = "1111111",
                        Description = "Article1",
                        Price = 10
                    },
                    Quantity = 7,
                },
                new SoldArticleItem
                {
                    Article = new Article
                    {
                        Ean = "222222222",
                        Description = "Article2",
                        Price = 20
                    },
                    Quantity = 5,
                },
                new SoldArticleItem
                {
                    Article = new Article
                    {
                        Ean = "33333333",
                        Description = "Article3",
                        Price = 30
                    },
                    Quantity = 3,
                }
            });
            Cash = 47;
            BankCard = 28;
        }
    }
}
