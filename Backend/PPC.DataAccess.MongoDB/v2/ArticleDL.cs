using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using PPC.Domain.v2;
using PPC.IDataAccess.v2;

namespace PPC.DataAccess.MongoDB.v2
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

        public async Task<IEnumerable<Article>> GetAllsAsync()
        {
            return await ArticleCollection.AsQueryable().ToListAsync();
        }

        public async Task SaveAsync(Article article)
        {
            //UpdateResult updateResult = await ArticleCollection.UpdateOneAsync(x => x.Id == article.Id, )
        }

        public Task<Article> GetByEanAsync(string ean)
        {
            throw new NotImplementedException();
        }

        public Task<Article> GetByIdAsync(Guid guid)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> CategoriesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetProducersAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> SubCategoriesAsync(string category)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Article>> FilterByCategoryAsync(string category)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Article>> FilterByCategoryAndSubCategoryAsync(string category, string subCategory)
        {
            throw new NotImplementedException();
        }
    }
}
