using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using PPC.DataContracts;
using PPC.Popup;

namespace PPC.Sale
{
    // TODO:
    public static class ArticleDb
    {
        private static IPopupService PopupService => EasyIoc.IocContainer.Default.Resolve<IPopupService>();

        public static List<Article> Articles { get; private set; }

        static ArticleDb()
        {
            Articles = new List<Article>();
        }

        public static void Load()
        {
            // Load articles
            try
            {
                string filename = ConfigurationManager.AppSettings["ArticlesPath"];
                if (File.Exists(filename))
                {
                    List<Article> newArticles;
                    using (XmlTextReader reader = new XmlTextReader(filename))
                    {
                        DataContractSerializer serializer = new DataContractSerializer(typeof(List<Article>));
                        newArticles = (List<Article>) serializer.ReadObject(reader);
                    }
                    Articles.AddRange(newArticles);
                }
                else
                {
                    ErrorPopupViewModel vm = new ErrorPopupViewModel("Articles DB file not found.");
                    PopupService.DisplayModal(vm, "Warning");
                }
            }
            catch (Exception ex)
            {
                ErrorPopupViewModel vm = new ErrorPopupViewModel(ex);
                PopupService.DisplayModal(vm, "Cannot load articles");
            }
        }

        public static void Save()
        {
            try
            {
                string filename = ConfigurationManager.AppSettings["ArticlesPath"];
                using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
                {
                    writer.Formatting = Formatting.Indented;
                    DataContractSerializer serializer = new DataContractSerializer(typeof(List<Article>));
                    serializer.WriteObject(writer, Articles);
                }
            }
            catch (Exception ex)
            {
                ErrorPopupViewModel vm = new ErrorPopupViewModel(ex);
                PopupService.DisplayModal(vm, "Cannot save articles");
            }
        }

        public static void Import()
        {
            try
            {
                string filename = @"C:\temp\ppc\produits.xml";
                ArticleTable table;
                using (StreamReader sr = new StreamReader(filename))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ArticleTable));
                    table = (ArticleTable)serializer.Deserialize(sr);
                }
                Articles.AddRange(table.ArticleRow.Select(x => new Article
                {
                    Guid = Guid.NewGuid(),
                    Ean = x.Id,
                    Description = x.Description,
                    Category = x.Category,
                    SupplierPrice = x.SupplierPrice ?? 0,
                    Price = x.Price ?? 0,
                    Producer = null,
                    VatRate = x.VAT.HasValue ? (x.VAT.Value == 6 ? VatRates.FoodDrink : VatRates.Other) : VatRates.Other
                }));
            }
            catch (Exception ex)
            {
                ErrorPopupViewModel vm = new ErrorPopupViewModel(ex);
                PopupService.DisplayModal(vm, "Cannot import articles");
            }
        }
    }
}
