using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using PPC.Domain;
using PPC.IDataAccess;

namespace PPC.DataAccess.MongoDB
{
    public class ArticleDL : IArticleDL
    {
        private const string DatabaseName = "PPCClub";
        private const string CollectionName = "Articles";

        private IMongoCollection<Article> ArticleCollection => _db.GetCollection<Article>(CollectionName);

        private readonly IMongoDatabase _db;

        public ArticleDL()
        {
            var client = new MongoClient();
            _db = client.GetDatabase(DatabaseName);
        }

        public void Fetch(IEnumerable<Article> articles)
        {
            _db.DropCollection(CollectionName);
            ArticleCollection.InsertMany(articles);
        }

        #region IArticleDL

        public IEnumerable<Article> Articles => ArticleCollection.AsQueryable();

        public Article GetByEan(string ean)
        {
            return ArticleCollection.AsQueryable().FirstOrDefault(x => x.Ean == ean);
        }

        public Article GetById(Guid guid)
        {
            return ArticleCollection.AsQueryable().FirstOrDefault(x => x.Guid == guid);
        }

        public IEnumerable<string> Categories => ArticleCollection.AsQueryable().Where(x => !string.IsNullOrEmpty(x.Category)).Select(x => x.Category).Distinct();
        public IEnumerable<string> Producers => ArticleCollection.AsQueryable().Where(x => !string.IsNullOrEmpty(x.Producer)).Select(x => x.Producer).Distinct();
        public IEnumerable<string> SubCategories(string category)
        {
            return ArticleCollection.AsQueryable().Where(x => x.Category == category && !string.IsNullOrEmpty(x.SubCategory)).Select(x => x.SubCategory).Distinct();
        }

        public IEnumerable<Article> FilterArticles(string category)
        {
            IQueryable<Article> query = ArticleCollection.AsQueryable();
            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(x => x.Category == category);
            return query;
        }

        public IEnumerable<Article> FilterArticles(string category, string subCategory)
        {
            IQueryable<Article> query = ArticleCollection.AsQueryable();
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(x => x.Category == category);
                if (!string.IsNullOrWhiteSpace(subCategory))
                    query = query.Where(x => x.SubCategory == subCategory);
            }
            return query;
        }

        public void SaveArticle(Article article)
        {
            ArticleCollection.FindOneAndReplace(x => x.Guid == article.Guid, article);
        }

        public void AddArticle(Article article)
        {
            ArticleCollection.InsertOne(article);
        }

        #endregion
    }
}
