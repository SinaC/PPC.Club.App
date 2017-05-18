using System;
using System.Collections.Generic;
using PPC.Data.Contracts;

namespace PPC.Data.Articles
{
    public interface IArticleDb
    {
        IEnumerable<Article> Articles { get; }

        void Add(Article article);

        Article GetByEan(string ean);
        Article GetById(Guid guid);

        IEnumerable<string> Categories { get; }
        IEnumerable<string> Producers { get; }
        IEnumerable<string> SubCategories(string category);
        IEnumerable<Article> FilterArticles(string category);
        IEnumerable<Article> FilterArticles(string category, string subCategory);

        void ImportFromDbf(string filename);

        void Load();
        void Save();
    }
}
