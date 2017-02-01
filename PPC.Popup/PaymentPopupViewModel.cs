using System;
using System.Windows.Input;
using PPC.MVVM;

namespace PPC.Popup
{
    public class PaymentPopupViewModel : ObservableObject
    {
        private readonly IPopupService _popupService;
        private readonly Action<decimal, decimal> _paidAction;
        public decimal Total { get; }

        private decimal _cash;
        public decimal Cash
        {
            get { return _cash; }
            set
            {
                if (Set(() => Cash, ref _cash, value))
                {
                    _bankCard = Total - Cash;
                    RaisePropertyChanged(() => BankCard);
                    RaisePropertyChanged(() => CurrentTotal);
                    RaisePropertyChanged(() => IsPaidButtonActive);
                    RaisePropertyChanged(() => IsClientCashNotEnough);
                    RaisePropertyChanged(() => ClientChange);
                    RaisePropertyChanged(() => IsClientChangePositive);
                    RaisePropertyChanged(() => MaxBankCard);
                    RaisePropertyChanged(() => IsPaidButtonActive);
                }
            }
        }

        public decimal MaxCash => Total - BankCard;

        private decimal _bankCard;
        public decimal BankCard
        {
            get { return _bankCard; }
            set
            {
                if (Set(() => BankCard, ref _bankCard, value))
                {
                    _cash = Total - BankCard;
                    RaisePropertyChanged(() => Cash);
                    RaisePropertyChanged(() => CurrentTotal);
                    RaisePropertyChanged(() => IsPaidButtonActive);
                    RaisePropertyChanged(() => IsClientCashNotEnough);
                    RaisePropertyChanged(() => ClientChange);
                    RaisePropertyChanged(() => IsClientChangePositive);
                    RaisePropertyChanged(() => MaxCash);
                    RaisePropertyChanged(() => IsPaidButtonActive);
                }
            }
        }

        public decimal MaxBankCard => Total - Cash;

        private decimal _clientCash;
        public decimal ClientCash
        {
            get { return _clientCash; }
            set
            {
                if (Set(() => ClientCash, ref _clientCash, value))
                {
                    RaisePropertyChanged(() => CurrentTotal);
                    RaisePropertyChanged(() => IsClientCashNotEnough);
                    RaisePropertyChanged(() => ClientChange);
                    RaisePropertyChanged(() => IsClientChangePositive);
                    RaisePropertyChanged(() => IsPaidButtonActive);
                }
            }
        }

        public bool IsClientCashNotEnough => Cash > 0 && ClientCash < Cash;

        public decimal ClientChange => ClientCash > 0 && Cash > 0 ? ClientCash - Cash : 0;

        public bool IsClientChangePositive => ClientChange > 0;

        public decimal CurrentTotal => ClientCash + BankCard;

        public bool IsPaidButtonActive => !IsClientCashNotEnough && CurrentTotal >= Total; // >= because client can give more cash than needed

        private ICommand _paidCommand;
        public ICommand PaidCommand => _paidCommand = _paidCommand ?? new RelayCommand(Paid);

        private void Paid()
        {
            _popupService.Close(this);
            _paidAction(Cash, BankCard);
        }

        public PaymentPopupViewModel(IPopupService popupService, decimal total, bool isCashFirst, Action<decimal,decimal> paidAction)
        {
            _popupService = popupService;
            Total = total;
            _paidAction = paidAction;

            Cash = 0;
            BankCard = 0;
            if (isCashFirst)
                Cash = total;
            else
                BankCard = total;
        }
    }

    public class PaymentPopupViewModelDesignData : PaymentPopupViewModel
    {
        public PaymentPopupViewModelDesignData() : base(null, 20, true, (a, b) => { })
        {
            ClientCash = 5;
            BankCard = 7;
        }
    }
}
