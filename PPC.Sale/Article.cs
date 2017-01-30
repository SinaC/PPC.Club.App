using System.Collections.Generic;

namespace PPC.Sale
{
    public class Article
    {
        // STUB
        public string Name { get; set; }
        public string Ean { get; set; }
        public double Price { get; set; }
    }

    // Dummy DB
    public static class FakeArticleDb
    {
        public static List<Article> Articles = new List<Article>
        {
            new Article
            {
                Ean = "1111111111111",
                Name = "Article1",
                Price = 1
            },
            new Article
            {
                Ean = "2222222222222",
                Name = "Article2",
                Price = 2
            },
            new Article
            {
                Ean = "3333333333333",
                Name = "Article3",
                Price = 3
            }
        };
    }
}
