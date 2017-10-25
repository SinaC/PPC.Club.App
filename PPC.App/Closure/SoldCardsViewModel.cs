using System.Collections.Generic;
using System.Linq;
using EasyMVVM;
using PPC.Domain;

namespace PPC.App.Closure
{
    public class SoldCardsViewModel : ObservableObject
    {
        //TODO: total ?

        public List<SoldCards> SoldCards { get; }

        public SoldCardsViewModel(List<SoldCards> soldCards)
        {
            SoldCards = soldCards;
        }
    }

    public class SoldCardsViewModelDesignData : SoldCardsViewModel
    {
        public SoldCardsViewModelDesignData() : base(Enumerable.Empty<SoldCards>().ToList())
        {
        }
    }
}
