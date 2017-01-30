using System;
using System.Runtime.InteropServices;
using System.Windows.Input;
using PPC.MVVM;

namespace PPC.Popup
{
    public class AskNamePopupViewModel : ObservableObject
    {
        private readonly IPopupService _popupService;
        private readonly Action<string> _okAction;

        private string _name;
        public string Name
        {
            get { return _name; }
            set { Set(() => Name, ref _name, value); }
        }

        private ICommand _enterCommand;
        public ICommand EnterCommand => _enterCommand = _enterCommand ?? new RelayCommand<string>(Ok);

        private ICommand _okCommand;
        public ICommand OkCommand => _okCommand = _okCommand ?? new RelayCommand(() => Ok(Name));

        private void Ok(string name)
        {
            _popupService.Close(this);
            _okAction(name);
        }

        public AskNamePopupViewModel(IPopupService popupService, Action<string> okAction)
        {
            _popupService = popupService;
            _okAction = okAction;
        }
    }

    public class AskNamePopupViewModelDesignData : AskNamePopupViewModel
    {
        public AskNamePopupViewModelDesignData() : base(null, name => { })
        {
        }
    }
}
