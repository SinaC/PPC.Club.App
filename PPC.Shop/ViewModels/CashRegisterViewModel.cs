using System;

namespace PPC.Shop.ViewModels
{
    public class CashRegisterViewModel : Sale.ViewModels.ShopViewModel
    {
        // TODO: transfer code instead of inheriting once PPC.Shop is finished

        public CashRegisterViewModel(Action cartPaidAction)
            : base(cartPaidAction)
        {
        }
    }

    public class CashRegisterViewModelDesignData : CashRegisterViewModel
    {
        public CashRegisterViewModelDesignData() :
            base(() => { })
        {
        }
    }
}
