using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Xml;
using PPC.DataContracts;

namespace PPC.Sale
{
    public class ShopViewModel : ShoppingCartTabBase
    {
        public const string ShopFile = "_shop.xml";

        #region ShoppingCartTabBase

        public override string Header => "Shop";

        #endregion

        private readonly Action _cardPaidAction;

        private ShoppingCartViewModel _shoppingCart;
        public ShoppingCartViewModel ShoppingCart
        {
            get { return _shoppingCart; }
            set { Set(() => ShoppingCart, ref _shoppingCart, value); }
        }

        public List<ShopArticleItem> SoldArticles { get; protected set; }

        public ShopViewModel(Action cartPaidAction)
        {
            _cardPaidAction = cartPaidAction;
            IsPaid = false;

            SoldArticles = new List<ShopArticleItem>();
            ShoppingCart = new ShoppingCartViewModel(CartPaid);
        }

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
                        shop = (Shop) serializer.ReadObject(reader);
                    }
                    SoldArticles = shop.Articles.Select(x => new ShopArticleItem
                    {
                        Article = FakeArticleDb.Articles.FirstOrDefault(a => a.Ean == x.Ean), // TODO: search DB
                        Quantity = x.Quantity,
                        IsCash = x.IsCash
                    }).ToList();
                }
                else
                    MessageBox.Show("Shop file not found.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot load shop. Exception: {ex}");
            }
        }

        private void Save()
        {
            try
            {
                if (!Directory.Exists(ConfigurationManager.AppSettings["BackupPath"]))
                    Directory.CreateDirectory(ConfigurationManager.AppSettings["BackupPath"]);
                Shop shop = new Shop
                {
                    Articles = SoldArticles.Select(x => new SoldArticle
                    {
                        Ean = x.Article.Ean,
                        Quantity = x.Quantity,
                        IsCash = x.IsCash
                    }).ToList()
                };
                string filename = $"{ConfigurationManager.AppSettings["BackupPath"]}{ShopFile}";
                using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(Shop));
                    serializer.WriteObject(writer, shop);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot save shop. Exception: {ex}");
            }
        }

        private void CartPaid(bool isCash)
        {

            SoldArticles.AddRange(ShoppingCart.ShoppingCartArticles.Select(x => new ShopArticleItem
            {
                Article = x.Article,
                Quantity = x.Quantity,
                IsCash = isCash
            }));
            Save();
            RaisePropertyChanged(() => SoldArticles);
            ShoppingCart.Clear();
            _cardPaidAction();
        }
    }

    public class ShopViewModelDesignData : ShopViewModel
    {
        public ShopViewModelDesignData() : base(() => { })
        {
            ShoppingCart = new ShoppingCartViewModelDesignData();
            SoldArticles = new List<ShopArticleItem>
            {
                new ShopArticleItem
                {
                    Article = FakeArticleDb.Articles[0],
                    Quantity = 7,
                    IsCash = true
                },
                new ShopArticleItem
                {
                    Article = FakeArticleDb.Articles[0],
                    Quantity = 3,
                    IsCash = false
                },
                new ShopArticleItem
                {
                    Article = FakeArticleDb.Articles[1],
                    Quantity = 5,
                    IsCash = false
                },
                new ShopArticleItem
                {
                    Article = FakeArticleDb.Articles[2],
                    Quantity = 3,
                    IsCash = true
                }
            };
        }
    }
}
