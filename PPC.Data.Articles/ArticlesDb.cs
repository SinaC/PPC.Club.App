using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using EasyDBFParser;
using PPC.DataContracts;

namespace PPC.Data.Articles
{
    public class ArticlesDb
    {
        #region Singleton

        private static readonly Lazy<ArticlesDb> Lazy = new Lazy<ArticlesDb>(() => new ArticlesDb(), LazyThreadSafetyMode.ExecutionAndPublication);
        public static ArticlesDb Instance => Lazy.Value;

        private ArticlesDb()
        {
            // TODO: ideally Load should be called here but if an exception occurs in Load, it will not bubble
        }

        #endregion

        private List<Article> _articles;
        public IEnumerable<Article> Articles => _articles;

        public void Add(Article article)
        {
            _articles.Add(article);
            Save();
        }

        public void ImportFromCsv(string filename)
        {
            //string filename = @"C:\temp\ppc\liste des produits.csv";
            if (File.Exists(filename))
            {
                List<Article> articles = new List<Article>();

                // Parse file and create new article list
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
                    if (string.IsNullOrWhiteSpace(tokens[0]) && string.IsNullOrWhiteSpace(tokens[1]) && !string.IsNullOrWhiteSpace(tokens[3])) // don't consider invalid/header rows
                    {
                        Debug.Assert(tokens.Length == 17);
                        string id = tokens[2];
                        string description = tokens[3];

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
                        Article article = new Article
                        {
                            Guid = Guid.NewGuid(),
                            Ean = id,
                            Description = description,
                            Category = category,
                            Price = price,
                            SupplierPrice = supplierPrice,
                            Stock = stock,
                            VatRate = vatRate,
                        };
                        articles.Add(article);
                    }
                }
                // Assign new article list
                _articles = articles;
            }
        }

        public void ImportFromDbf(string filename)
        {
            DBFParser parser = new DBFParser();
            parser.Parse(filename);
            
            // Get column index
            int eanIndex = parser.Fields.FindIndex(x => x.Name == "CODE_ART");
            int descriptionIndex = parser.Fields.FindIndex(x => x.Name == "NOM_ART");
            int categoryIndex = parser.Fields.FindIndex(x => x.Name == "CATEGORIE");
            int subCategoryIndex = parser.Fields.FindIndex(x => x.Name == "CATEGORIE2");
            int stockIndex = parser.Fields.FindIndex(x => x.Name == "QTE_STOCK");
            int supplierPriceIndex = parser.Fields.FindIndex(x => x.Name == "PRIX_ACHAT");
            int priceIndex  = parser.Fields.FindIndex(x => x.Name == "PX_VTE_TC");
            int supplierIndex = parser.Fields.FindIndex(x => x.Name == "NOM_FOU");
            int vatIndex = parser.Fields.FindIndex(x => x.Name == "CODE_TVA");

            // Build articles DB
            List<Article> articles = new List<Article>();
            foreach (object[] data in parser.Datas)
            {
                try
                {
                    Article article = new Article
                    {
                        Guid = Guid.NewGuid(),
                        Ean = (string)data[eanIndex],
                        Description = (string)data[descriptionIndex],
                        Category = (string)data[categoryIndex],
                        SubCategory = (string)data[subCategoryIndex],
                        Stock = ConvertFromDoubleToInt(data[stockIndex]),
                        SupplierPrice = ConvertFromDoubleToDecimal(data[supplierPriceIndex]),
                        Price = ConvertFromDoubleToDecimal(data[priceIndex]),
                        Producer = (string)data[supplierIndex],
                        VatRate = (int)data[vatIndex] == 2 ? VatRates.FoodDrink : VatRates.Other
                    };
                    articles.Add(article);
                }
                catch (Exception ex)
                {
                    
                }
            }
            _articles = articles;
        }

        private static decimal ConvertFromDoubleToDecimal(object value)
        {
            double d = (double) value;
            if (double.IsInfinity(d) || double.IsNaN(d) || double.IsNegativeInfinity(d) || double.IsPositiveInfinity(d))
                return 0;
            return (decimal) d;
        }

        private static int ConvertFromDoubleToInt(object value)
        {
            double d = (double)value;
            if (double.IsInfinity(d) || double.IsNaN(d) || double.IsNegativeInfinity(d) || double.IsPositiveInfinity(d))
                return 0;
            return (int)d;
        }

        public void Save()
        {
            string filename = ConfigurationManager.AppSettings["ArticlesPath"];
            using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                DataContractSerializer serializer = new DataContractSerializer(typeof(List<Article>));
                serializer.WriteObject(writer, Articles);
            }
        }

        public void Load()
        {
            // Load articles
            string filename = ConfigurationManager.AppSettings["ArticlesPath"];
            if (File.Exists(filename))
            {
                List<Article> newArticles;
                using (XmlTextReader reader = new XmlTextReader(filename))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(List<Article>));
                    newArticles = (List<Article>)serializer.ReadObject(reader);
                }
                _articles = newArticles;
            }
            else
                throw new InvalidOperationException("Article DB not found.");
        }

        public void Inject(List<Article> articles) // useful while designing views
        {
            _articles = articles;
        }

        private static readonly Regex CsvSplitRegEx = new Regex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)", RegexOptions.Compiled);

        private static IEnumerable<string> SplitCsv(string input)
        {
            foreach (Match match in CsvSplitRegEx.Matches(input))
                yield return match.Value.TrimStart(',').TrimStart('\"').TrimEnd('\"').Trim();
        }
    }
}
