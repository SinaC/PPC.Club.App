using System;
using System.Collections.ObjectModel;
using System.Configuration;
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

        private ObservableCollection<PlayerModel> _players;
        public ObservableCollection<PlayerModel> Players
        {
            get { return _players; }
            protected set { Set(() => Players, ref _players, value); }
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

        #region DoubleClick

        private ICommand _selectPlayerCommand;
        public ICommand SelectPlayerCommand => _selectPlayerCommand = _selectPlayerCommand ?? new RelayCommand<PlayerModel>(pm => SelectPlayer(pm, true));

        private ICommand _selectPlayerWithModifierCommand;
        public ICommand SelectPlayerWithModifierCommand => _selectPlayerWithModifierCommand = _selectPlayerWithModifierCommand ?? new RelayCommand<PlayerModel>(pm => SelectPlayer(pm, false));

        private void SelectPlayer(PlayerModel player, bool switchToShop)
        {
            if (player != null)
                Mediator.Default.Send(new PlayerSelectedMessage
                {
                    DciNumber = player.DCINumber,
                    FirstName = player.FirstName,
                    LastName = player.LastName,
                    SwitchToShop = switchToShop
                });
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
