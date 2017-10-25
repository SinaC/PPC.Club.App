using System.Linq;
using EasyMVVM;
using PPC.Domain;

namespace PPC.App.Closure
{
    public class ArticlesViewModel : ObservableObject
    {
        public CashRegisterClosure ClosureData { get; }

        public ArticlesViewModel(CashRegisterClosure cashRegisterClosure)
        {
            ClosureData = cashRegisterClosure;
        }
    }

    public class ArticlesViewModelDesignData : ArticlesViewModel
    {
        public ArticlesViewModelDesignData():base(new CashRegisterClosure
        {
            BankCard = 53,
            Cash = 19,
            //Articles = new List<FullArticle>
            //{
            //    new FullArticle
            //    {
            //        Description = "Article 1",
            //        Ean = "123456",
            //        Price = 5,
            //        Quantity = 2
            //    },
            //    new FullArticle
            //    {
            //        Description = "Article 2",
            //        Ean = "9876",
            //        Price = 4,
            //        Quantity = 5
            //    }
            //}
            Articles = Enumerable.Range(0, 50).Select(x => new FullArticle
            {
                Description = $"Article {x}",
                Ean = "123456",
                Price = 2 + x,
                Quantity = 5 + x
            }).ToList()
        })
        {
        }
    }
}
