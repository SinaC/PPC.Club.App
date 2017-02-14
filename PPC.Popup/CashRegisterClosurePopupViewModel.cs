using System;
using System.Linq;
using System.Windows.Input;
using PPC.DataContracts;
using PPC.MVVM;

namespace PPC.Popup
{
    public class CashRegisterClosurePopupViewModel : ObservableObject
    {
        private readonly IPopupService _popupService;
        private readonly Action _closeAction;
        private readonly Action<CashRegisterClosure> _sendMailAction;

        public CashRegisterClosure ClosureData { get; }

        private ICommand _okCommand;
        public ICommand OkCommand => _okCommand = _okCommand ?? new RelayCommand(Ok);
        private void Ok()
        {
            _popupService.Close(this);
            _closeAction();
        }

        private ICommand _sendMailCommand;
        public ICommand SendMailCommand => _sendMailCommand = _sendMailCommand ?? new RelayCommand(() => _sendMailAction(ClosureData));

        public CashRegisterClosurePopupViewModel(IPopupService service, CashRegisterClosure closure, Action closeAction, Action<CashRegisterClosure> sendMailAction)
        {
            _popupService = service;
            _closeAction = closeAction;
            _sendMailAction = sendMailAction;

            ClosureData = closure;
        }
    }

    public class CashRegisterClosurePopupViewModelDesignData : CashRegisterClosurePopupViewModel
    {
        public CashRegisterClosurePopupViewModelDesignData() : base(null, new CashRegisterClosure
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
            Articles = Enumerable.Range(0,50).Select(x => new FullArticle
            {
                Description = $"Article {x}",
                Ean = "123456",
                Price = 2+x,
                Quantity = 5+x
            }).ToList()
        }, () => { }, _ => { })
        {
        }
    }
}
