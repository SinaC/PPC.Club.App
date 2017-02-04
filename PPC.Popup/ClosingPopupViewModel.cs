using System;
using System.Collections.Generic;
using System.Windows.Input;
using PPC.DataContracts;
using PPC.MVVM;

namespace PPC.Popup
{
    public class ClosingPopupViewModel : ObservableObject
    {
        private readonly IPopupService _popupService;
        private readonly Action _closeAction;

        public Closing Closing { get; private set; }

        private ICommand _okCommand;
        public ICommand OkCommand => _okCommand = _okCommand ?? new RelayCommand(Ok);
        private void Ok()
        {
            _popupService.Close(this);
            _closeAction();
        }

        public ClosingPopupViewModel(IPopupService service, Action closeAction, Closing closing)
        {
            _popupService = service;
            _closeAction = closeAction;

            Closing = closing;
        }
    }

    public class ClosingPopupViewModelDesignData : ClosingPopupViewModel
    {
        public ClosingPopupViewModelDesignData() : base(null, () => { }, new Closing
        {
            BankCard = 53,
            Cash = 19,
            Articles = new List<FullArticle>
            {
                new FullArticle
                {
                    Description = "Article 1",
                    Ean = "123456",
                    Price = 5,
                    Quantity = 2
                },
                new FullArticle
                {
                    Description = "Article 2",
                    Ean = "9876",
                    Price = 4,
                    Quantity = 5
                }
            }
        })
        {
        }
    }
}
