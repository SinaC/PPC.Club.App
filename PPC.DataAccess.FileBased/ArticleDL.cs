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

        public void AddArticle(Article article)
        {
            Load(); // Load if needed

            _articles.Add(article);
            Save();
        }

        public void SaveArticle(Article article)
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
            using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                DataContractSerializer serializer = new DataContractSerializer(typeof(List<Article>));
                serializer.WriteObject(writer, Articles);
            }
        }

        private async Task SaveAsync()
        {
            string filename = PPCConfigurationManager.ArticlesPath;
            using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                DataContractSerializer serializer = new DataContractSerializer(typeof(List<Article>));
                await serializer.WriteObjectAsync(writer, Articles);
            }
        }

        private void Load()
        {
            if (_articles != null)
                return;

            // Load articles
            string filename = PPCConfigurationManager.ArticlesPath;
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

        private async Task LoadAsync()
        {
            // Load articles
            string filename = PPCConfigurationManager.ArticlesPath;
            if (File.Exists(filename))
            {
                List<Article> newArticles;
                using (XmlTextReader reader = new XmlTextReader(filename))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(List<Article>));
                    newArticles = (List<Article>)await serializer.ReadObjectAsync(reader);
                }
                _articles = newArticles;
            }
            else
                throw new InvalidOperationException("Article DB not found.");
        }
    }
}
