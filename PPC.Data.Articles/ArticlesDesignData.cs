using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PPC.Data.Contracts;

namespace PPC.Data.Articles
{
    public class ArticlesDesignData : IArticleDb
    {
        private static readonly string[] EmptyList = { string.Empty };

        private readonly List<Article> _articles;
        public IEnumerable<Article> Articles => _articles;

        public IEnumerable<string> Categories => EmptyList.Concat(Articles.Where(x => !string.IsNullOrWhiteSpace(x.Category)).Select(x => x.Category).Distinct());
        public IEnumerable<string> Producers => EmptyList.Concat(Articles.Where(x => !string.IsNullOrWhiteSpace(x.Producer)).Select(x => x.Producer).Distinct());
        public IEnumerable<string> SubCategories(string category)
        {
            return EmptyList.Concat(Articles.Where(x => x.Category == category && !string.IsNullOrWhiteSpace(x.SubCategory)).Select(x => x.SubCategory).Distinct());
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

        public ArticlesDesignData(IEnumerable<Article> articles)
        {
            _articles = articles.ToList();
        }

        public void Add(Article article)
        {
            throw new NotImplementedException();
        }

        public Article GetByEan(string ean)
        {
            throw new NotImplementedException();
        }

        public Article GetById(Guid guid)
        {
            throw new NotImplementedException();
        }

        public void ImportFromDbf(string filename)
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            throw new NotImplementedException();
        }

        public async Task LoadAsync()
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public async Task SaveAsync()
        {
            throw new NotImplementedException();
        }

        public void Inject()
        {
            throw new NotImplementedException();
        }
    }
}
