using System;
using System.Windows.Input;
using EasyMVVM;

namespace PPC.Popups
{
    public class PaymentPopupViewModel : ObservableObject
    {
        private readonly IPopupService _popupService;
        private readonly Action<decimal, decimal> _paidAction;

        public decimal TotalWithoutDiscount { get; }

        public decimal MinBankCard { get; private set; } = 5; // TODO: Config ?

        public decimal Total => TotalWithoutDiscount - Discount;

        private bool _hasBeenInitializedFromCash;
        public bool HasBeenInitializedFromCash
        {
            get { return _hasBeenInitializedFromCash; }
            protected set { Set(() => HasBeenInitializedFromCash, ref _hasBeenInitializedFromCash, value); }
        }

        private decimal _cash;
        public decimal Cash
        {
            get { return _cash; }
            set
            {
                if (Set(() => Cash, ref _cash, value))
                {
                    _bankCard = Total - Cash; // TODO: check MinBankCard
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

        private decimal _discount;
        public decimal Discount
        {
            get {  return _discount;}
            set
            {
                if (Set(() => Discount, ref _discount, value))
                {
                    // Set cash to total if no bank card payment
                    if (BankCard == 0)
                        _cash = Total;
                    // Set bank to total if no cash payment
                    else if (Cash == 0)
                    {
                        if (Total < MinBankCard) // *** same code (TODO: refactoring)
                        {
                            _bankCard = MinBankCard;
                            _cash = Math.Max(0, Total - MinBankCard);
                        }
                        else
                            _bankCard = Total;
                    }
                    // In case of mixed payment, try to remove discount from biggest value without going below bank card minimum (5EUR)
                    else
                    {
                        if (Cash >= Discount)
                            _cash -= Discount;
                        else if (BankCard >= Discount) // *** same code (TODO: refactoring)
                        {
                            if (BankCard - Discount < MinBankCard)
                            {
                                _bankCard = MinBankCard;
                                _cash = Math.Max(0, Total - MinBankCard);
                            }
                            else
                                _bankCard -= Discount;
                        }
                        else
                        {
                            //
                            _cash = 0;
                            _bankCard = Total;
                        }
                    }
                    RaisePropertyChanged(() => HasDiscount);
                    RaisePropertyChanged(() => Total);
                    RaisePropertyChanged(() => Cash);
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

        public bool HasDiscount => Discount > 0;

        public bool IsClientCashNotEnough => Cash > 0 && ClientCash < Cash;

        public decimal ClientChange => ClientCash > 0 && Cash > 0 ? ClientCash - Cash : 0;

        public bool IsClientChangePositive => ClientChange > 0;

        public decimal CurrentTotal => ClientCash + BankCard;

        public bool IsPaidButtonActive => !IsClientCashNotEnough && CurrentTotal >= Total; // >= because client can give more cash than needed

        private ICommand _paidCommand;
        public ICommand PaidCommand => _paidCommand = _paidCommand ?? new RelayCommand(Paid, () => IsPaidButtonActive);

        private void Paid()
        {
            _popupService.Close(this);
            _paidAction(Cash, BankCard);
        }

        public PaymentPopupViewModel(IPopupService popupService, decimal total, bool isCashFirst, Action<decimal,decimal> paidAction)
        {
            _popupService = popupService;
            TotalWithoutDiscount = total;
            _paidAction = paidAction;

            Cash = 0;
            BankCard = 0;
            if (isCashFirst || total < MinBankCard)
            {
                Cash = total;
                ClientCash = total;
            }
            else
                BankCard = total;
            HasBeenInitializedFromCash = isCashFirst;
        }
    }

    public class PaymentPopupViewModelDesignData : PaymentPopupViewModel
    {
        public PaymentPopupViewModelDesignData() : base(null, 20, true, (a, b) => { })
        {
            ClientCash = 5;
            BankCard = 7;
            Discount = 2;
        }
    }
}
