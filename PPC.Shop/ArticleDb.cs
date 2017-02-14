using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using PPC.DataContracts;
using PPC.Popup;

namespace PPC.Shop
{
    // TODO:
    public static class ArticleDb
    {
        private static IPopupService PopupService => EasyIoc.IocContainer.Default.Resolve<IPopupService>();

        public static List<Article> Articles { get; }

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

        public static void ImportFromXml()
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

        public static void ImportFromCsv()
        {
            string filename = @"C:\temp\ppc\liste des produits.csv";
            if (File.Exists(filename))
            {
                try
                {
                    int newArticlesCount = 0;
                    int categoryModifiedCount = 0;
                    int priceModifiedCount = 0;
                    int supplierPriceModifiedCount = 0;
                    int vatModifiedCount = 0;
                    string[] lines = File.ReadAllLines(filename, Encoding.GetEncoding("iso-8859-1"));
                    // column 2: id
                    // column 3: description
                    // column 6: category
                    // column 10: supplier price
                    // column 13: price
                    // column 16: price-vat -> can be used to compute vat
                    // if column 0 or 1 is non-empty or 3 is empty -> irrevelant line
                    foreach (string rawLine in lines)
                    {
                        string[] tokens = SplitCsv(rawLine).ToArray();
                        if (string.IsNullOrWhiteSpace(tokens[0]) && string.IsNullOrWhiteSpace(tokens[1]) && !string.IsNullOrWhiteSpace(tokens[3]))
                        {
                            Debug.Assert(tokens.Length == 17);
                            string id = tokens[2];
                            string description = tokens[3];

                            bool isNewArticle = false;

                            // search if article already exists
                            Article article = Articles.SingleOrDefault(x => x.Ean == id && x.Description.Trim().ToLowerInvariant() == description.ToLowerInvariant());
                            if (article == null)
                            {
                                article = new Article
                                {
                                    Guid = Guid.NewGuid(),
                                    Ean = id,
                                    Description = description,
                                };
                                Articles.Add(article);
                                isNewArticle = true;
                            }

                            string category = tokens[6].Trim();
                            int stock;
                            if (!int.TryParse(tokens[8], out stock))
                                stock = 0;
                            decimal supplierPrice;
                            if (!decimal.TryParse(tokens[10], out supplierPrice))
                                supplierPrice = 0;
                            decimal price;
                            if (!decimal.TryParse(tokens[13], out price))
                                price = 0;
                            decimal priceNoVat;
                            if (!decimal.TryParse(tokens[16], out priceNoVat))
                                priceNoVat = 0;
                            decimal vat = Math.Round(100*(price - priceNoVat)/priceNoVat, 0, MidpointRounding.AwayFromZero);
                            VatRates vatRate = vat == 6 ? VatRates.FoodDrink : VatRates.Other;

                            if (isNewArticle)
                            {
                                Debug.WriteLine($"NEW: Id:[{id}] Descr:[{description}] Cat:[{category}] P:[{price:C}] SP:[{supplierPrice:C}] VAT:[{vatRate}] Stock:[{stock}] PHT:[{priceNoVat}]");
                                newArticlesCount++;
                            }
                            else
                            {
                                if (!string.Equals(category.Trim(), article.Category.Trim(), StringComparison.InvariantCultureIgnoreCase))
                                {
                                    Debug.WriteLine($"{id} {description}: category: [{article.Category}] -> [{category}]");
                                    categoryModifiedCount++;
                                }
                                if (price != article.Price)
                                {
                                    Debug.WriteLine($"{id} {description}: price: [{article.Price:C}] -> [{price:C}]");
                                    priceModifiedCount++;
                                }
                                if (supplierPrice != article.SupplierPrice)
                                {
                                    Debug.WriteLine($"{id} {description}: supplierPrice: [{article.SupplierPrice:C}] -> [{supplierPrice:C}]");
                                    supplierPriceModifiedCount++;
                                }
                                if (vatRate != article.VatRate)
                                {
                                    Debug.WriteLine($"{id} {description}: VatRate: [{article.VatRate}] -> [{vatRate}]");
                                    vatModifiedCount++;
                                }
                            }

                            article.Category = category;
                            article.Price = price;
                            article.SupplierPrice = supplierPrice;
                            article.Stock = stock;
                            article.VatRate = vatRate;
                        }
                    }

                    Debug.WriteLine($"New: {newArticlesCount}");
                    Debug.WriteLine($"Category modified: {categoryModifiedCount}");
                    Debug.WriteLine($"Price modified: {priceModifiedCount}");
                    Debug.WriteLine($"SupplierPrice modified: {supplierPriceModifiedCount}");
                    Debug.WriteLine($"VAT modified: {vatModifiedCount}");

                    //if (newArticlesCount > 0 || categoryModifiedCount > 0 || priceModifiedCount > 0 || supplierPriceModifiedCount > 0 || vatModifiedCount > 0)
                    //    Save();
                }
                catch (Exception ex)
                {
                    ErrorPopupViewModel vm = new ErrorPopupViewModel(ex);
                    PopupService.DisplayModal(vm, "Cannot import articles");
                }
            }
        }

        private static readonly Regex CsvSplitRegEx = new Regex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)", RegexOptions.Compiled);

        private static IEnumerable<string> SplitCsv(string input)
        {
            foreach (Match match in CsvSplitRegEx.Matches(input))
                yield return match.Value.TrimStart(',').TrimStart('\"').TrimEnd('\"').Trim();
        }
    }
}
