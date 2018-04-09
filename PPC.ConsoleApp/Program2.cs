using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using PPC.Domain;

namespace PPC.ConsoleApp
{
    class Program2
    {
        private static Dictionary<string, string> UnknownArticles = new Dictionary<string, string>()
        {
            {"6288396d-433a-462b-9756-8d683affc268", "CARTES"},
            {"d4394353-eb3a-4617-ad43-c479af483681", "CARTES"},
            {"bf663855-6d66-488d-aa6c-dab713238525", "CARTES"},
            {"301fb83e-3f84-4863-be0c-ca55ff536f47", "CARTES"},
            {"dbb1d9db-86a1-4861-96ed-f57912ca9fba", "CARTES"},
            {"8522e1ce-fabe-45cf-875b-8f0be8837900", "CARTES"},
            {"070e5eba-93d8-4b8c-8f30-6c26caf3c28e", "CLUB"},
            {"2f46f740-7f9e-48f0-b9a5-d45ad61878e7", "CLUB"},
            {"0d393448-1ae7-4659-bd0a-f52cc5219fd5", "CONFISERIES"},
            {"83798f1c-2cbf-44ef-87d1-dec6601eadbb", "CARTES"},
            {"cbf5bd01-c2fc-479d-b551-7c4d37a5c05c", "CARTES"},
            {"7ef0ced9-5b43-4a37-96f8-d2f58f056670", "CARTES"},
            {"4cfa4666-abee-42c9-8432-226f7e08ab79", "BOISSONS"},
            {"4494c67f-95c8-458a-b93f-a6616b413c88", "CARTES"},
            {"6f9b161b-7a2d-48aa-ad7d-bc16c1e73e07", "CARTES"},
            {"232883a2-a45c-425b-ada3-0deab2f404a9", "CARTES"},
            {"9afc4b58-b88f-44bd-a618-b8a3874a85bb", "CARTES"},
            {"726c504a-c62d-4d79-bacf-b7e897073ba6", "CARTES"},
            {"db710863-6601-4c14-9985-7ee4b441c4b0", "CARTES"},
            {"583d670d-f439-4534-bca3-d8e7378a9f08", "CONFISERIES"},
            {"cd065119-350a-4403-bdec-803c1e72a585", "CARTES"},
            {"61da9e5d-6275-41e3-80b6-dca37f8d5d58", "CARTES"},
            {"7a91270b-9c03-48b9-bd35-7dcac91018b9", "CARTES"},
            {"52c34575-0d25-41e1-bd22-ae15002cbbdb", "BOISSONS"},
            {"d8d2a4ef-2b51-41a4-830b-dd8d1da80052", "CARTES"},
            {"ffa58214-2ae2-448b-ac8e-d2dbdcc97ec5", "PAPETERIE"},
            {"ff1a0ca9-c55c-4543-b99d-902ed92180f5", "SLEEVES"},
            {"a35d5706-aac7-4d75-b2f7-3f306cc51b7c", "ACCESSOIRES DIVERS"},
            {"6816b822-db8a-4e4b-8819-01dbd39512f2", "CARTES"},
            {"f44eabda-c6f2-40ad-bf6d-55f38b18d1c0", "CARTES"},
            {"216423b5-11bd-42d2-a85d-65c793569df7", "CARTES"},
            {"1921ea2e-5879-4b5e-9cbf-e6cb06182edb", "CARTES"},
            {"5ea0f0fd-efd1-4e81-91fb-346805acd424", "CARTES"},
            {"0d769c4b-efea-4b29-956b-c44d14dc9993", "CARTES"},
            {"8a1c320f-400f-4c26-a7df-8eed5b12db54", "CONFISERIES"},
            {"2e4237de-1d7a-4abc-8f2b-aa96ae1396ac", "BOISSONS"},
            {"c1ebe6e9-311e-4d4b-8940-53f5a7770991", "CARTES"},
            {"7b48af74-f7f6-41ca-ad76-b3bd7806d2bd", "CARTES"},
            {"30525998-7738-43fd-808a-02ef0b6942e5", "CARTES"},
            {"0b235e8f-1487-4bb4-9bed-f4a08993257b", "CARTES"},
            {"72e91163-b969-4909-8f30-2ccd5c8e4bca", "CARTES"},
            {"171d8adb-659a-4e6b-8e5f-4f710c0848ca", "CARTES"},
            {"9f8899fd-860d-48c5-abb8-01046a157819", "CARTES"},
            {"d0cb8451-eba8-4ab3-ac89-37410342e7de", "CARTES"},
            {"d256315e-57c6-4d87-8a17-bce2e4d8f207", "SLEEVES"},
            {"69c3479f-e48f-4338-82a4-7ef122c68c69", "CARTES"},
            {"108deacd-f50c-4659-bfa9-2b4855a398b8", "CARTES"},
            {"3561da03-3d04-4038-91eb-7b6c3d377aeb", "CARTES"},
            {"0d708827-3c61-4844-9d32-808adddbce29", "CARTES"},
            {"caf4c96f-049b-4c0f-a18e-0b235fa6e180", "CARTES"},
            {"1c34d575-757b-4724-80eb-15d82649c0f3", "CLUB"},
            {"00747e00-8844-4d92-ad15-74df4aba4fb8", "CARTES"},
            {"37819f34-9540-466c-8a88-ddfd2789a560", "SLEEVES"},
            {"9b6eef83-a2a7-4974-bf17-295fed3586ef", "BOISSONS"},
            {"66edecb8-6764-492d-9e3a-17fef581d7a3", "CARTES"},
            {"5c7b6f55-fdd9-46bc-bc0e-8675710f870e", "BOISSONS"},
            {"f29479ec-918a-4840-95c5-daadec84f113", "CONFISERIES"},
            {"8118970f-6bcd-4606-829f-ed671ba191e7", "CONFISERIES"},
            {"c2bef2d3-d3a8-480f-8fb1-e66cdc6198f6", "CONFISERIES"},
            {"e19680c9-05c4-45bc-83f8-8b32d80974ed", "CARTES"},
            {"6b2764e9-7dd2-42ed-8c72-88372b0aad49", "CONFISERIES"},
            {"b6e458e8-feed-4105-b820-248da05d04ee", "CARTES"},
        };

        private static Dictionary<string, string> SuperCategories = new Dictionary<string, string>
        {
            {"CARTES", "CLUB"},
            {"CARTES MAGIC", "CLUB"},
            {"BOISSONS", "ALIMENTAIRE"},
            {"FORCE OF WILL", "CLUB"},
            {"CONFISERIES", "ALIMENTAIRE"},
            {"BOXES", "CLUB"},
            {"CLUB", "CLUB"},
            {"DRAGON SHIELD", "CLUB"},
            {"SLEEVES", "CLUB"},
            {"DIVERS", "PAPETERIE"},
            {"CARTES POKEMON", "CLUB"},
            {"MATéRIEL ARTISTIQUE", "PAPETERIE"},
            {"PAPETERIE", "PAPETERIE"},
            {"ACCESSOIRES DIVERS", "ALIMENTAIRE"},
            {"GLACES", "ALIMENTAIRE"},
            {"ALBUM CARTES", "CLUB"},
            {"PLAYMAT", "CLUB"},
        };

        static void Main2(string[] args)
        {
            PerformStatisticsOnClosure();
        }

        private static void PerformStatisticsOnClosure()
        {
            try
            {
                // Read closure
                Dictionary<DateTime, CashRegisterClosure> closures = new Dictionary<DateTime, CashRegisterClosure>();
                string path = ConfigurationManager.AppSettings["CashRegisterClosurePath"];
                foreach (string filename in Directory.EnumerateFiles(path, "*.xml", SearchOption.TopDirectoryOnly))
                {
                    //Console.WriteLine($"Reading {Path.GetFileName(filename)}");
                    string[] parts1 = Path.GetFileNameWithoutExtension(filename).Split('_');
                    string[] date = parts1[1].Split('-');
                    int year = Convert.ToInt32(date[0]);
                    int month = Convert.ToInt32(date[1]);
                    int day = Convert.ToInt32(date[2]);
                    string[] time = parts1[2].Split('-');
                    int hour = Convert.ToInt32(time[0]);
                    int minute = Convert.ToInt32(time[1]);
                    int second = Convert.ToInt32(time[2]);
                    DateTime creationDate;
                    if (hour < 3)
                        creationDate = new DateTime(year, month, day, 23, 59, 59).AddDays(-1); // previous day
                    else
                        creationDate = new DateTime(year, month, day, hour, minute, second);
                    string[] lines = File.ReadAllLines(filename);
                    lines[0] = "<CashRegisterClosure xmlns:i=\"" + @"http://www.w3.org/2001/XMLSchema-instance" + "\">";
                    string cleanedXml = string.Join(Environment.NewLine, lines);
                    //string xml = File.ReadAllText(filename, Encoding.UTF8);
                    //string namespaceToRemove = "xmlns=\"" + "http://schemas.datacontract.org/2004/07/PPC.DataContracts" + "\"";
                    //string cleanedXml = xml.Replace(namespaceToRemove, string.Empty);
                    CashRegisterClosure closure = Deserialize<CashRegisterClosure>(cleanedXml);
                    closures.Add(creationDate, closure);
                }
                Console.WriteLine($"{closures.Count} closure read and parsed");

                // Read articles
                //closures.SelectMany(x => x.Value.Articles).Select(x => x.)
                List<Article> oldArticles = ReadArticles(@"c:\temp\ppc\articles.xml");
                Console.WriteLine($"{oldArticles.Count} old articles read");
                List<Article> newArticles = ReadArticles(@"c:\temp\PPC data from club\articles.xml");
                Console.WriteLine($"{newArticles.Count} new articles read");

                // Search closure article in old and new article list
                List<FullArticleWithCategoryAndDate> fullArticleWithCategoryList = new List<FullArticleWithCategoryAndDate>();
                int totalArticleCount = 0;
                List<FullArticle> notFoundArticles = new List<FullArticle>();
                foreach (KeyValuePair<DateTime, CashRegisterClosure> kv in closures)
                {
                    foreach (FullArticle fullArticle in kv.Value.Articles)
                    {
                        Article newArticle = newArticles.FirstOrDefault(x => x.Guid == fullArticle.Guid) ?? newArticles.FirstOrDefault(x => string.Compare(x.Description, fullArticle.Description, StringComparison.OrdinalIgnoreCase) == 0);
                        Article oldArticle = oldArticles.FirstOrDefault(x => x.Guid == fullArticle.Guid) ?? oldArticles.FirstOrDefault(x => string.Compare(x.Description, fullArticle.Description, StringComparison.OrdinalIgnoreCase) == 0);

                        string category = newArticle?.Category ?? oldArticle?.Category;
                        if (category == null)
                        {
                            string guid = fullArticle.Guid.ToString().ToLowerInvariant();
                            if (!UnknownArticles.TryGetValue(guid, out category))
                                //Console.WriteLine($"Article not found: {fullArticle.Guid} {fullArticle.Description} {fullArticle.Price}");
                                notFoundArticles.Add(fullArticle);
                        }
                        fullArticleWithCategoryList.Add(new FullArticleWithCategoryAndDate
                        {
                            FullArticle = fullArticle,
                            SuperCategory = GetSuperCategory(category),
                            Category = category,
                            DateTime = kv.Key
                        });

                        totalArticleCount++;
                    }
                }
                Console.WriteLine($"Total: {totalArticleCount}. Not found: {notFoundArticles.Count}.");
                foreach (FullArticle article in notFoundArticles)
                    Console.WriteLine($"{article.Guid} {article.Ean} {article.Description} {article.Price}");

                List<string> categories = fullArticleWithCategoryList.Select(x => x.Category).Distinct().ToList();
                Console.WriteLine("Categories: ");
                foreach(string category in categories)
                    Console.WriteLine(category);

                // Amount by category
                var amountsByCategory = fullArticleWithCategoryList.GroupBy(x => x.Category).Select(x => new
                {
                    category = x.Key,
                    total = x.Sum(y => y.FullArticle.Quantity*y.FullArticle.Price)
                });
                foreach (var amountByCategory in amountsByCategory)
                    Console.WriteLine($"{amountByCategory.category}: {amountByCategory.total}");

                // Amount by month and category
                var amountsByMonthAndCategory = fullArticleWithCategoryList.GroupBy(x => new
                {
                    x.DateTime.Year,
                    x.DateTime.Month,
                    x.Category
                }).Select(x => new
                {
                    year = x.Key.Year,
                    month = x.Key.Month, 
                    category = x.Key.Category,
                    total = x.Sum(y => y.FullArticle.Quantity * y.FullArticle.Price)
                });
                Console.WriteLine("BY MONTH/YEAR/CATEGORY");
                foreach (var amountByMonthAndCategory in amountsByMonthAndCategory.OrderBy(x => x.year).ThenBy(x => x.month).ThenBy(x => x.category))
                    Console.WriteLine($"{amountByMonthAndCategory.month:D2}/{amountByMonthAndCategory.year} | {amountByMonthAndCategory.category}: {amountByMonthAndCategory.total}");

                // Amount by month and super category
                var amountsByMonthAndSuperCategory = fullArticleWithCategoryList.GroupBy(x => new
                {
                    x.DateTime.Year,
                    x.DateTime.Month,
                    x.SuperCategory
                }).Select(x => new
                {
                    year = x.Key.Year,
                    month = x.Key.Month,
                    superCategory = x.Key.SuperCategory,
                    total = x.Sum(y => y.FullArticle.Quantity * y.FullArticle.Price)
                });
                Console.WriteLine("BY MONTH/YEAR/SUPER CATEGORY");
                foreach (var amountByMonthAndSuperCategory in amountsByMonthAndSuperCategory.OrderBy(x => x.year).ThenBy(x => x.month).ThenBy(x => x.superCategory))
                    Console.WriteLine($"{amountByMonthAndSuperCategory.month:D2}/{amountByMonthAndSuperCategory.year} | {amountByMonthAndSuperCategory.superCategory}: {amountByMonthAndSuperCategory.total}EUR");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static string GetSuperCategory(string category)
        {
            string superCategory;
            if (!SuperCategories.TryGetValue(category, out superCategory))
                superCategory = category;
            return superCategory;
        }

        public class FullArticleWithCategoryAndDate
        {
            public FullArticle FullArticle { get; set; }
            public string SuperCategory { get; set; }
            public string Category { get; set; }
            public DateTime DateTime { get; set; }
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
                //DataAccess.MongoDB.ArticleDL mongoArticlesDB = new DataAccess.MongoDB.ArticleDL();
                //mongoArticlesDB.Fetch(articles);
                //Console.WriteLine($"{articles.Count} articles inserted");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static List<Article> ReadArticles(string filename)
        {
            using (XmlTextReader reader = new XmlTextReader(filename))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(List<Article>));
                return (List<Article>)serializer.ReadObject(reader);
            }
        }

        public static T Deserialize<T>(string rawXml)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(rawXml)))
            {
                DataContractSerializer formatter0 =
                    new DataContractSerializer(typeof(T));
                return (T)formatter0.ReadObject(reader);
            }
        }
    }
}
