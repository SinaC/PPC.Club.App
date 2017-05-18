using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using EasyDBFParser;
using PPC.Data.Contracts;

namespace PPC.Data.Articles
{
    public class ArticlesDb : IArticleDb
    {
        private List<Article> _articles;

        #region IArticleDb

        public IEnumerable<Article> Articles => _articles ?? Enumerable.Empty<Article>();

        public void Add(Article article)
        {
            _articles.Add(article);
            Save();
        }

        public Article GetByEan(string ean)
        {
            return _articles.FirstOrDefault(x => x.Ean == ean);
        }

        public Article GetById(Guid guid)
        {
            return _articles.FirstOrDefault(x => x.Guid == guid);
        }

        public IEnumerable<string> Categories => Articles.Where(x => !string.IsNullOrWhiteSpace(x.Category)).Select(x => x.Category).Distinct();
        public IEnumerable<string> Producers => Articles.Where(x => !string.IsNullOrWhiteSpace(x.Producer)).Select(x => x.Producer).Distinct();
        public IEnumerable<string> SubCategories(string category)
        {
            return Articles.Where(x => x.Category == category && !string.IsNullOrWhiteSpace(x.SubCategory)).Select(x => x.SubCategory).Distinct();
        }
        public IEnumerable<Article> FilterArticles(string category)
        {
            IQueryable<Article> query = Articles.AsQueryable();
            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(x => x.Category == category);
            return query;
        }
        public IEnumerable<Article> FilterArticles(string category, string subCategory)
        {
            IQueryable<Article> query = Articles.AsQueryable();
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(x => x.Category == category);
                if (!string.IsNullOrWhiteSpace(subCategory))
                    query = query.Where(x => x.SubCategory == subCategory);
            }
            return query;
        }

        public void ImportFromDbf(string filename)
        {
            // Extract articles from DBF
            DBFParser parser = new DBFParser();
            parser.Parse(filename);

            _articles = parser.DataTable.AsEnumerable().Select(row => new Article
            {
                Guid = Guid.NewGuid(),
                Ean = row.Field<string>("CODE_ART"),
                Description = row.Field<string>("NOM_ART"),
                Category = row.Field<string>("CATEGORIE"),
                SubCategory = row.Field<string>("CATEGORIE2"),
                Stock = ConvertFromDoubleToInt(row.Field<double>("QTE_STOCK")),
                SupplierPrice = ConvertFromDoubleToDecimal(row.Field<double>("PRIX_ACHAT")),
                Price = ConvertFromDoubleToDecimal(row.Field<double>("PX_VTE_TC")),
                Producer = row.Field<string>("NOM_FOU"),
                VatRate = row.Field<int>("CODE_TVA") == 2 ? VatRates.FoodDrink : VatRates.Other
            }).ToList();
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

        #endregion

        private static decimal ConvertFromDoubleToDecimal(double d)
        {
            if (double.IsInfinity(d) || double.IsNaN(d) || double.IsNegativeInfinity(d) || double.IsPositiveInfinity(d))
                return 0;
            return (decimal)d;
        }

        private static int ConvertFromDoubleToInt(double d)
        {
            if (double.IsInfinity(d) || double.IsNaN(d) || double.IsNegativeInfinity(d) || double.IsPositiveInfinity(d))
                return 0;
            return (int)d;
        }

        private static readonly Regex CsvSplitRegEx = new Regex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)", RegexOptions.Compiled);

        [Obsolete("Use ImportFromDBF instead.")]
        private void ImportFromCsv(string filename)
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
                        decimal vat = Math.Round(100 * (price - priceNoVat) / priceNoVat, 0, MidpointRounding.AwayFromZero);

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

        private static IEnumerable<string> SplitCsv(string input)
        {
            foreach (Match match in CsvSplitRegEx.Matches(input))
                yield return match.Value.TrimStart(',').TrimStart('\"').TrimEnd('\"').Trim();
        }
    }
}
