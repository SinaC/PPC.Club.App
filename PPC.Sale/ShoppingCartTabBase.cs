using PPC.Tab;

namespace PPC.Sale
{
    public abstract class ShoppingCartTabBase : TabBase
    {
        private bool _isPaid;
        public bool IsPaid {
            get { return _isPaid; }
            set { Set(() => IsPaid, ref _isPaid, value); }
        }
    }
}
