using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows.Input;
using PPC.MVVM;
using PPC.Tab;

namespace PPC.Players
{
    public class PlayersViewModel : TabBase
    {
        #region TabBase

        public override string Header => "Players";

        #endregion

        private ObservableCollection<PlayerModel> _players;
        public ObservableCollection<PlayerModel> Players
        {
            get { return _players; }
            set { Set(() => Players, ref _players, value); }
        }

        #region Load

        private ICommand _loadCommand;
        public ICommand LoadCommand
        {
            get
            {
                _loadCommand = _loadCommand ?? new RelayCommand(Load);
                return _loadCommand;
            }
        }

        private void Load()
        {
            Players = new ObservableCollection<PlayerModel>(PlayerManager.Load(ConfigurationManager.AppSettings["PlayersPath"]));
        }

        #endregion

        #region Save

        private ICommand _saveCommand;

        public ICommand SaveCommand {
            get
            {
                _saveCommand = _saveCommand ?? new RelayCommand(Save);
                return _saveCommand;
            }
        }

        private void Save()
        {
            PlayerManager.Save(ConfigurationManager.AppSettings["PlayersPath"], Players);
            Load(); // crappy workaround to reset row.IsNewItem
        }

        #endregion
    }

    public class PlayersViewModelDesignData : PlayersViewModel
    {
        public PlayersViewModelDesignData()
        {
            Players = new ObservableCollection<PlayerModel>
            {
                new PlayerModel
                {
                    DCINumber = "123456789",
                    FirstName = "pouet",
                    LastName = "taratata",
                    CountryCode = "BE"
                },
                 new PlayerModel
                {
                    DCINumber = "9876543",
                    FirstName = "tsekwa",
                    LastName = "gamin",
                    CountryCode = "FR"
                }
            };
        }
    }
}
