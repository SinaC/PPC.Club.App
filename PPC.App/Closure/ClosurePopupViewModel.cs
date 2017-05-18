using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using EasyMVVM;
using PPC.Data.Contracts;
using PPC.Services.Popup;

namespace PPC.App.Closure
{
    public enum ClosureDisplayModes
    {
        Articles,
        SoldCards,
        CashCount,
    }

    [PopupAssociatedView(typeof(ClosurePopup))]
    public class ClosurePopupViewModel : ObservableObject
    {
        private IPopupService PopupService => EasyIoc.IocContainer.Default.Resolve<IPopupService>();

        private readonly Action _closeAction;
        private readonly Func<CashRegisterClosure, List<SoldCards>, Task> _sendMailsAsyncFunc;

        private bool _isWaiting;
        public bool IsWaiting
        {
            get { return _isWaiting; }
            set { Set(() => IsWaiting, ref _isWaiting, value); }
        }

        private ArticlesViewModel _articlesViewModel;
        public ArticlesViewModel ArticlesViewModel
        {
            get { return _articlesViewModel;}
            protected set { Set(() => ArticlesViewModel, ref _articlesViewModel, value); }
        }

        private SoldCardsViewModel _soldCardsViewModel;
        public SoldCardsViewModel SoldCardsViewModel
        {
            get { return _soldCardsViewModel;}
            protected set { Set(() => SoldCardsViewModel, ref _soldCardsViewModel, value); }
        }

        private CashCountViewModel _cashcountViewModel;
        public CashCountViewModel CashCountViewModel
        {
            get { return _cashcountViewModel; }
            protected set { Set(() => CashCountViewModel, ref _cashcountViewModel, value); }
        }

        private ClosureDisplayModes _mode;
        public ClosureDisplayModes Mode
        {
            get { return _mode; }
            protected set { Set(() => Mode, ref _mode, value); }
        }

        private ICommand _okCommand;
        public ICommand OkCommand => _okCommand = _okCommand ?? new RelayCommand(Ok);
        private void Ok()
        {
            PopupService?.Close(this);
            _closeAction?.Invoke();
        }

        private ICommand _sendMailsCommand;
        public ICommand SendMailsCommand => _sendMailsCommand = _sendMailsCommand ?? new RelayCommand(async () => await SendMails());

        private async Task SendMails()
        {
            IsWaiting = true;
            await _sendMailsAsyncFunc(ArticlesViewModel.ClosureData, SoldCardsViewModel.SoldCards);
            IsWaiting = false;
        }

        private ICommand _switchToArticlesCommand;
        public ICommand SwitchToArticlesCommand => _switchToArticlesCommand = _switchToArticlesCommand ?? new RelayCommand(() => Mode = ClosureDisplayModes.Articles);

        private ICommand _switchToSoldCardsCommand;
        public ICommand SwitchToSoldCardsCommand => _switchToSoldCardsCommand = _switchToSoldCardsCommand ?? new RelayCommand(() => Mode = ClosureDisplayModes.SoldCards);

        private ICommand _switchToCashCountCommand;
        public ICommand SwitchToCashCountCommand => _switchToCashCountCommand = _switchToCashCountCommand ?? new RelayCommand(() => Mode = ClosureDisplayModes.CashCount);

        //http://stackoverflow.com/questions/12466049/passing-an-awaitable-anonymous-function-as-a-parameter
        public ClosurePopupViewModel(Action closeAction, CashRegisterClosure cashRegisterClosure, List<SoldCards> soldCards, Func<CashRegisterClosure, List<SoldCards>, Task> sendMailsAsyncFunc)
        {
            Mode = ClosureDisplayModes.Articles;

            _closeAction = closeAction;
            _sendMailsAsyncFunc = sendMailsAsyncFunc;

            ArticlesViewModel = new ArticlesViewModel(cashRegisterClosure);
            SoldCardsViewModel = new SoldCardsViewModel(soldCards);
            CashCountViewModel = new CashCountViewModel();
        }
    }

    public class ClosurePopupViewModelDesignData : ClosurePopupViewModel
    {
        public ClosurePopupViewModelDesignData() : base(() => { }, new CashRegisterClosure(), Enumerable.Empty<SoldCards>().ToList(), null)
        {
            ArticlesViewModel = new ArticlesViewModelDesignData();
            SoldCardsViewModel = new SoldCardsViewModelDesignData();
            CashCountViewModel = new CashCountViewModelDesignData();
        }
    }
}
