using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PPC.Domain.v2;

namespace PPC.IDataAccess.v2
{
    public interface IArticleDL
    {
        Task<IEnumerable<Article>> GetAllsAsync();
        Task SaveAsync(Article article);

        Task<Article> GetByEanAsync(string ean);
        Task<Article> GetByIdAsync(Guid guid);

        Task<IEnumerable<string>> CategoriesAsync();
        Task<IEnumerable<string>> GetProducersAsync();
        Task<IEnumerable<string>> SubCategoriesAsync(string category);
        Task<IEnumerable<Article>> FilterByCategoryAsync(string category);
        Task<IEnumerable<Article>> FilterByCategoryAndSubCategoryAsync(string category, string subCategory);
    }
}
