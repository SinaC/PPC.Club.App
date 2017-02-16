using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using PPC.Helpers;
using PPC.Messages;
using PPC.MVVM;
using PPC.Players.Models;
using PPC.Popup;

namespace PPC.Players.ViewModels
{
    public class PlayersViewModel : ObservableObject
    {
        private IPopupService PopupService => EasyIoc.IocContainer.Default.Resolve<IPopupService>();

        #region Filtered players

        private IEnumerable<PlayerModel> _filteredPlayers;
        public IEnumerable<PlayerModel> FilteredPlayers
        {
            get { return _filteredPlayers;}
            set { Set(() => FilteredPlayers, ref _filteredPlayers, value); }
        }

        private string _filter;
        public string Filter
        {
            get { return _filter; }
            set
            {
                if (Set(() => Filter, ref _filter, value))
                    FilterPlayers();
            }
        }

        private bool FilterPlayer(PlayerModel p)
        {
            if (string.IsNullOrWhiteSpace(Filter))
                return true;
            //http://stackoverflow.com/questions/359827/ignoring-accented-letters-in-string-comparison/7720903#7720903
            return CultureInfo.CurrentCulture.CompareInfo.IndexOf(p.FirstName, Filter, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) >= 0
                || CultureInfo.CurrentCulture.CompareInfo.IndexOf(p.LastName, Filter, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) >= 0
                || CultureInfo.CurrentCulture.CompareInfo.IndexOf(p.DCINumber, Filter, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) >= 0;
        }

        private void FilterPlayers()
        {
            if (Players == null)
                return;
             FilteredPlayers = Players?.Where(FilterPlayer).ToList();
            //// If only one player, select it
            //if (FilteredPlayers.Count() == 1)
            //    SelectedPlayer = FilteredPlayers.First();
            SelectedPlayer = FilteredPlayers.FirstOrDefault();
        }

        #endregion

        private ObservableCollection<PlayerModel> _players;
        public ObservableCollection<PlayerModel> Players
        {
            get { return _players; }
            protected set
            {
                if (Set(() => Players, ref _players, value))
                    FilterPlayers();
            }
        }

        private PlayerModel _selectedPlayer;
        public PlayerModel SelectedPlayer
        {
            get { return _selectedPlayer; }
            set { Set(() => SelectedPlayer, ref _selectedPlayer, value); }
        }

        #region Load

        private ICommand _loadCommand;
        public ICommand LoadCommand => _loadCommand = _loadCommand ?? new RelayCommand(Load);

        private void Load()
        {
            try
            {
                Players = new ObservableCollection<PlayerModel>(PlayerManager.Load(ConfigurationManager.AppSettings["PlayersPath"]).OrderBy(x => x.FirstName, new EmptyStringsAreLast()));
            }
            catch (Exception ex)
            {
                ErrorPopupViewModel vm = new ErrorPopupViewModel(ex);
                PopupService.DisplayModal(vm, "Error while loading player file");
            }
        }

        #endregion

        #region Save

        private ICommand _saveCommand;

        public ICommand SaveCommand  => _saveCommand = _saveCommand ?? new RelayCommand(Save);

        private void Save()
        {
            try
            {
                PlayerManager.Save(ConfigurationManager.AppSettings["PlayersPath"], Players);
                Load(); // crappy workaround to reset row.IsNewItem
            }
            catch (Exception ex)
            {
                ErrorPopupViewModel vm = new ErrorPopupViewModel(ex);
                PopupService.DisplayModal(vm, "Error while saving player file");
            }
        }

        #endregion

        #region Select player

        private ICommand _selectPlayerCommand;
        public ICommand SelectPlayerCommand => _selectPlayerCommand = _selectPlayerCommand ?? new RelayCommand<PlayerModel>(pm => SelectPlayer(pm, true));

        private ICommand _selectPlayerWithModifierCommand;
        public ICommand SelectPlayerWithModifierCommand => _selectPlayerWithModifierCommand = _selectPlayerWithModifierCommand ?? new RelayCommand<PlayerModel>(pm => SelectPlayer(pm, false));

        private void SelectPlayer(PlayerModel player, bool switchToShop)
        {
            if (player != null)
            {
                Mediator.Default.Send(new PlayerSelectedMessage
                {
                    DciNumber = player.DCINumber,
                    FirstName = player.FirstName,
                    LastName = player.LastName,
                    SwitchToShop = switchToShop
                });
                Filter = string.Empty;
            }
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
