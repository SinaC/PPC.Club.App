using System;
using System.Windows.Input;
using EasyIoc;
using EasyMVVM;
using PPC.Module.Shop.Views.Popups;
using PPC.Services.Popup;

namespace PPC.Module.Shop.ViewModels.Popups
{
    [PopupAssociatedView(typeof(PaymentPopup))]
    public class PaymentPopupViewModel : ObservableObject
    {
        private IPopupService PopupService => IocContainer.Default.Resolve<IPopupService>();

        private readonly Action<decimal, decimal, decimal> _paidAction;

        public decimal TotalWithoutDiscount { get; }

        public decimal MinBankCard { get; private set; } = 5; // TODO: Config ?

        public decimal Total => HasDiscount ? TotalWithoutDiscount * (1-DiscountPercentage) : TotalWithoutDiscount;

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

        // TODO: refactor logic
        // TODO: set client cash to cash if cash > 0
        private decimal _discountPercentage;
        public decimal DiscountPercentage
        {
            get {  return _discountPercentage; }
            set
            {
                if (Set(() => DiscountPercentage, ref _discountPercentage, value))
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
                    // In case of mixed payment, try to remove discount from bank card first
                    else
                    {
                        // TODO: fix this !!! multiple percentage leads to a wrong value
                        decimal discount = TotalWithoutDiscount * DiscountPercentage;
                        if (Cash >= discount)
                            _cash -= discount;
                        else if (BankCard >= discount)
                        {
                            if (BankCard - discount < MinBankCard)
                            {
                                _bankCard = MinBankCard;
                                _cash = Math.Max(0, Total - MinBankCard);
                            }
                            else
                                _bankCard -= discount;
                        }
                        else
                        {
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

        private ICommand _setDiscountCommand;
        public ICommand SetDiscountCommand => _setDiscountCommand = _setDiscountCommand ?? new RelayCommand<decimal>(discount => DiscountPercentage = discount/100m);

        public bool HasDiscount => DiscountPercentage > 0;

        public bool IsClientCashNotEnough => Cash > 0 && ClientCash < Cash;

        public decimal ClientChange => ClientCash > 0 && Cash > 0 ? ClientCash - Cash : 0;

        public bool IsClientChangePositive => ClientChange > 0;

        public decimal CurrentTotal => ClientCash + BankCard;

        public bool IsPaidButtonActive => !IsClientCashNotEnough && CurrentTotal >= Total; // >= because client can give more cash than needed

        private ICommand _paidCommand;
        public ICommand PaidCommand => _paidCommand = _paidCommand ?? new RelayCommand(Paid, () => IsPaidButtonActive);

        private void Paid()
        {
            PopupService?.Close(this);
            _paidAction(Cash, BankCard, DiscountPercentage);
        }

        private ICommand _cancelCommand;
        public ICommand CancelCommand => _cancelCommand = _cancelCommand ?? new RelayCommand(Cancel);

        private void Cancel()
        {
            PopupService.Close(this);
        }

        public PaymentPopupViewModel(decimal total, bool isCashFirst, Action<decimal,decimal, decimal> paidAction)
        {
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
        public PaymentPopupViewModelDesignData() : base(20, true, (a, b, c) => { })
        {
            ClientCash = 5;
            BankCard = 7;
            DiscountPercentage = 0.2m;
        }
    }
}
