using System;
using System.Collections.Generic;
using PPC.Domain;

namespace PPC.IDataAccess
{
    public interface IArticleDL
    {
        IEnumerable<Article> Articles { get; }

        void Insert(Article article);
        void Update(Article article);

        Article GetByEan(string ean);
        Article GetById(Guid guid);

        IEnumerable<string> Categories { get; }
        IEnumerable<string> Producers { get; }
        IEnumerable<string> SubCategories(string category);
        IEnumerable<Article> FilterArticles(string category);
        IEnumerable<Article> FilterArticles(string category, string subCategory);

        //void ImportFromDbf(string filename);

        //void Load();
        //Task LoadAsync();
        //void Save();
        //Task SaveAsync();
    }
}
