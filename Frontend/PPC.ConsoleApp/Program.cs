using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using EasyDBFParser;
using PPC.Domain;

namespace PPC.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            FetchMongoArticles();
        }

        private static void Marges()
        {
            //SessionDL sessionDL = new SessionDL();
            //sessionDL.Test(1);
            //sessionDL.Test(2);
            ////sessionDL.Test(3);
            //sessionDL.Test(4);
            //sessionDL.Test(-1);
            DBFParser parser = new DBFParser();
            parser.Parse(@"D:\TEMP\article.dbf");
            var results = parser.DataTable.AsEnumerable()
                //.GroupBy(row => new { category = row.Field<string>("CATEGORIE"), subCategory = row.Field<string>("CATEGORIE2") });
                .GroupBy(row => row.Field<string>("CATEGORIE"))
                .Select(g => new ProfitMarginByCategoryAndSubCategory
                {
                    Category = g.Key,
                    Count = g.Count(),
                    TotalPrixAchat = g.Sum(x => ConvertFromDoubleToDecimal(x.Field<double>("PRIX_ACHAT"))),
                    TotalPxVteTc = g.Sum(x => ConvertFromDoubleToDecimal(x.Field<double>("PX_VTE_TC"))),
                    TotalPxVteHt = g.Sum(x => ConvertFromDoubleToDecimal(x.Field<double>("PX_VTE_HT"))),
                })
                .ToList();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Categorie;Nbre Articles;Total Prix Achat;Total Prix Vente TTC; Total Prix HTVA; Marge;");
            foreach (var result in results)
                sb.AppendLine($"{result.Category};{result.Count};{result.TotalPrixAchat};{result.TotalPxVteTc};{result.TotalPxVteHt};{result.ProfitMargin:P2}");
            File.WriteAllText(@"d:\temp\marges magasin.csv", sb.ToString());
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

        private static void FetchMongoArticles()
        {
            try
            {
                DataAccess.FileBased.ArticleDL fileArticlesDB = new DataAccess.FileBased.ArticleDL();
                List<Article> articles = fileArticlesDB.Articles.ToList();
                Console.WriteLine($"{articles.Count} articles read");
                var counts = articles.GroupBy(x => x.Guid).Select(g => new
                {
                    guid = g.Key,
                    count = g.Count()
                });
                foreach (var entry in counts.Where(x => x.count > 1))
                {
                    Console.WriteLine($"Duplicate Guid: {entry.guid}");
                    List<Article> duplicates = articles.Where(x => x.Guid == entry.guid).ToList();
                    foreach (var duplicate in duplicates)
                        Console.WriteLine($"{duplicate.Description}");
                }
                DataAccess.MongoDB.ArticleDL mongoArticlesDB = new DataAccess.MongoDB.ArticleDL();
                mongoArticlesDB.Fetch(articles);
                Console.WriteLine($"{articles.Count} articles inserted");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }

    class ProfitMarginByCategoryAndSubCategory
    {
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public int Count { get; set; }
        public decimal TotalPrixAchat { get; set; }
        public decimal TotalPxVteTc { get; set; }
        public decimal TotalPxVteHt { get; set; }

        public decimal ProfitMargin => TotalPrixAchat == 0 
            ? -1 
            : (TotalPxVteHt - TotalPrixAchat)/ TotalPrixAchat;
    }
}
