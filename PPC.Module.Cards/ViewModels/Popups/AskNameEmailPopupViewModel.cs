using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using EasyIoc;
using EasyMVVM;
using PPC.Module.Cards.Views.Popups;
using PPC.Services.Popup;

namespace PPC.Module.Cards.ViewModels.Popups
{
    [PopupAssociatedView(typeof(AskNameEmailPopup))]
    public class AskNameEmailPopupViewModel : ObservableObject
    {
        private IPopupService PopupService => IocContainer.Default.Resolve<IPopupService>();

        private readonly Action<string, string> _okAction;
        private readonly Func<string, string> _searchEmailByNameFunc;

        public ObservableCollection<string> Names { get; private set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (Set(() => Name, ref _name, value))
                    if (!string.IsNullOrWhiteSpace(Name) && Name.Length > 2)
                    {
                        string email = _searchEmailByNameFunc(Name);
                        if (!string.IsNullOrWhiteSpace(email))
                            Email = email;
                    }
            }
        }

        private string _email;
        public string Email
        {
            get { return _email; }
            set { Set(() => Email, ref _email, value); }
        }

        private ICommand _okCommand;
        public ICommand OkCommand => _okCommand = _okCommand ?? new RelayCommand(Ok);

        private void Ok()
        {
            PopupService?.Close(this);
            _okAction(Name, Email);
        }

        public AskNameEmailPopupViewModel(Action<string, string> okAction, IEnumerable<string> names, Func<string,string> searchEmailByNameFunc)
        {
            _okAction = okAction;
            Names = new ObservableCollection<string>(names);
            _searchEmailByNameFunc = searchEmailByNameFunc;
        }
    }

    public class AskNameEmailPopupViewModelDesignData : AskNameEmailPopupViewModel
    {
        public AskNameEmailPopupViewModelDesignData():base((n, e) => { }, Enumerable.Empty<string>(),  n => string.Empty)
        {
        }
    }
}
