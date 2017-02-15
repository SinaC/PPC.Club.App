using System;
using System.Linq;
using PPC.MVVM;

namespace PPC.Players.Models
{
    public class PlayerModel : ObservableObject
    {
        private string _dciNumber;
        public string DCINumber
        {
            get { return _dciNumber; }
            set
            {
                if (Set(() => DCINumber, ref _dciNumber, value))
                    RaisePropertyChanged(() => AutoCompleteSearchValue);
            }
        }

        private string _firstName;
        public string FirstName
        {
            get { return _firstName; }
            set
            {
                if (Set(() => FirstName, ref _firstName, value))
                    RaisePropertyChanged(() => AutoCompleteSearchValue);
            }
        }

        private string _middleName;
        public string MiddleName
        {
            get { return _middleName; }
            set { Set(() => MiddleName, ref _middleName, value); }
        }

        private string _lastName;
        public string LastName
        {
            get { return _lastName; }
            set
            {
                if (Set(() => LastName, ref _lastName, value))
                    RaisePropertyChanged(() => AutoCompleteSearchValue);
            }
        }

        private string _countryCode;
        public string CountryCode
        {
            get { return _countryCode; }
            set { Set(() => CountryCode, ref _countryCode, value); }
        }

        private bool _isJudge;
        public bool IsJudge
        {
            get { return _isJudge; }
            set { Set(() => IsJudge, ref _isJudge, value); }
        }

        public string AutoCompleteSearchValue => (DCINumber ?? string.Empty) + " " + (FirstName ?? string.Empty) + " " + (LastName ?? string.Empty);
    }
}
