using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using EasyIoc;
using EasyMVVM;
using PPC.Domain;
using PPC.Module.Notes.ViewModels;
using PPC.Services.Popup;

namespace PPC.App.Closure
{
    public enum ClosureDisplayModes
    {
        Articles,
        SoldCards,
        CashCount,
        Notes,
    }

    [PopupAssociatedView(typeof(ClosurePopup))]
    public class ClosurePopupViewModel : ObservableObject
    {
        private IPopupService PopupService => IocContainer.Default.Resolve<IPopupService>();

        private readonly Action _closeAction;
        private readonly Func<Domain.Closure, List<SoldCards>, Task> _sendMailsAsyncFunc;

        private bool _isWaiting;
        public bool IsWaiting
        {
            get { return _isWaiting; }
            protected set { Set(() => IsWaiting, ref _isWaiting, value); }
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

        // Will reuse MainViewModel.NotesViewModel
        public NotesViewModel NotesViewModel { get; protected set; }

        private ClosureDisplayModes _mode;
        public ClosureDisplayModes Mode
        {
            get { return _mode; }
            protected set
            {
                if (Set(() => Mode, ref _mode, value))
                {
                    RaisePropertyChanged(() => IsArticlesSelected);
                    RaisePropertyChanged(() => IsSoldCardsSelected);
                    RaisePropertyChanged(() => IsCashCountSelected);
                    RaisePropertyChanged(() => IsNotesSelected);
                }
            }
        }

        public bool IsArticlesSelected => Mode == ClosureDisplayModes.Articles;
        public bool IsSoldCardsSelected => Mode == ClosureDisplayModes.SoldCards;
        public bool IsCashCountSelected => Mode == ClosureDisplayModes.CashCount;
        public bool IsNotesSelected => Mode == ClosureDisplayModes.Notes;

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
            CashRegisterClosure cashRegisterClosure = ArticlesViewModel.ClosureData;
            Domain.Closure closure = new Domain.Closure
            {
                Notes = NotesViewModel.Note,
                CashRegisterClosure = cashRegisterClosure
            };
            await _sendMailsAsyncFunc(closure, SoldCardsViewModel.SoldCards);
            IsWaiting = false;
        }

        private ICommand _switchToArticlesCommand;
        public ICommand SwitchToArticlesCommand => _switchToArticlesCommand = _switchToArticlesCommand ?? new RelayCommand(() => Mode = ClosureDisplayModes.Articles);

        private ICommand _switchToSoldCardsCommand;
        public ICommand SwitchToSoldCardsCommand => _switchToSoldCardsCommand = _switchToSoldCardsCommand ?? new RelayCommand(() => Mode = ClosureDisplayModes.SoldCards);

        private ICommand _switchToCashCountCommand;
        public ICommand SwitchToCashCountCommand => _switchToCashCountCommand = _switchToCashCountCommand ?? new RelayCommand(() => Mode = ClosureDisplayModes.CashCount);

        private ICommand _switchToNotesCommand;
        public ICommand SwitchToNotesCommand => _switchToNotesCommand = _switchToNotesCommand ?? new RelayCommand(SwitchToNotes);

        private void SwitchToNotes()
        {
            Mode = ClosureDisplayModes.Notes;
            NotesViewModel.GotFocus();
        }

        //http://stackoverflow.com/questions/12466049/passing-an-awaitable-anonymous-function-as-a-parameter
        public ClosurePopupViewModel(NotesViewModel notesViewModel, Action closeAction, CashRegisterClosure cashRegisterClosure, List<SoldCards> soldCards, Func<Domain.Closure, List<SoldCards>, Task> sendMailsAsyncFunc)
        {
            Mode = ClosureDisplayModes.Articles;

            _closeAction = closeAction;
            _sendMailsAsyncFunc = sendMailsAsyncFunc;

            ArticlesViewModel = new ArticlesViewModel(cashRegisterClosure);
            SoldCardsViewModel = new SoldCardsViewModel(soldCards);
            CashCountViewModel = new CashCountViewModel();
            NotesViewModel = notesViewModel;
        }
    }

    public class ClosurePopupViewModelDesignData : ClosurePopupViewModel
    {
        public ClosurePopupViewModelDesignData() : base(null, () => { }, new CashRegisterClosure(), Enumerable.Empty<SoldCards>().ToList(), null)
        {
            ArticlesViewModel = new ArticlesViewModelDesignData();
            SoldCardsViewModel = new SoldCardsViewModelDesignData();
            CashCountViewModel = new CashCountViewModelDesignData();
            NotesViewModel = new NotesViewModelDesignData();
        }
    }
}
