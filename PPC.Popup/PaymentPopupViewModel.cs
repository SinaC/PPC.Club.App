using System;
using System.Windows.Input;
using PPC.MVVM;

namespace PPC.Popup
{
    public class PaymentPopupViewModel : ObservableObject
    {
        private readonly IPopupService _popupService;
        private readonly Action<double, double> _paidAction;
        public double Total { get; }

        private double _cash;
        public double Cash
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

        public double MaxCash => Total - BankCard;

        private double _bankCard;
        public double BankCard
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

        public double MaxBankCard => Total - Cash;

        private double _clientCash;
        public double ClientCash
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

        public double ClientChange => ClientCash > 0 && Cash > 0 ? ClientCash - Cash : 0;

        public bool IsClientChangePositive => ClientChange > 0;

        public double CurrentTotal => ClientCash + BankCard;

        public bool IsPaidButtonActive => !IsClientCashNotEnough && CurrentTotal >= Total; // >= because client can give more cash than needed

        private ICommand _paidCommand;
        public ICommand PaidCommand => _paidCommand = _paidCommand ?? new RelayCommand(Paid);

        private void Paid()
        {
            _popupService.Close(this);
            _paidAction(Cash, BankCard);
        }

        public PaymentPopupViewModel(IPopupService popupService, double total, bool isCashFirst, Action<double,double> paidAction)
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
