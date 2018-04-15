using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using PPC.Common;
using PPC.Domain;
using PPC.Helpers;
using PPC.IDataAccess;

namespace PPC.DataAccess.FileBased
{
    public class ArticleDL : IArticleDL
    {
        private List<Article> _articles;

        #region IArticleDb

        public IEnumerable<Article> Articles
        {
            get
            {
                Load(); // Load if needed

                return _articles ?? Enumerable.Empty<Article>();
            }
        }

        public void Insert(Article article)
        {
            Load(); // Load if needed

            _articles.Add(article);
            Save();
        }

        public void Update(Article article)
        {
            Save();
        }

        public Article GetByEan(string ean)
        {
            Load(); // Load if needed

            return _articles.FirstOrDefault(x => x.Ean == ean);
        }

        public Article GetById(Guid guid)
        {
            Load(); // Load if needed

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

        #endregion

        private void Save()
        {
            string filename = PPCConfigurationManager.ArticlesPath;
            DataContractHelpers.Write(filename, Articles);
        }

        private async Task SaveAsync()
        {
            string filename = PPCConfigurationManager.ArticlesPath;
            await DataContractHelpers.WriteAsync(filename, Articles);
        }

        private void Load()
        {
            if (_articles != null)
                return;

            // Load articles
            string filename = PPCConfigurationManager.ArticlesPath;
            if (File.Exists(filename))
            {
                List<Article> newArticles = DataContractHelpers.Read<List<Article>>(filename);
                _articles = newArticles;
            }
            else
                throw new InvalidOperationException("Article DB not found.");
        }

        private async Task LoadAsync()
        {
            // Load articles
            string filename = PPCConfigurationManager.ArticlesPath;
            if (File.Exists(filename))
            {
                List<Article> newArticles = await DataContractHelpers.ReadAsync<List<Article>>(filename);
                _articles = newArticles;
            }
            else
                throw new InvalidOperationException("Article DB not found.");
        }

        /* Import from DBF
        private void ImportFromDbf(string filename)
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
        */
    }
}
