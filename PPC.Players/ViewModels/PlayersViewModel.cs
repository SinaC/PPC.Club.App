using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows.Input;
using PPC.Messages;
using PPC.MVVM;
using PPC.Players.Models;
using PPC.Popup;

namespace PPC.Players.ViewModels
{
    public class PlayersViewModel : TabBase
    {
        private IPopupService PopupService => EasyIoc.IocContainer.Default.Resolve<IPopupService>();

        #region TabBase

        public override string Header => "Players";

        #endregion

        private ObservableCollection<PlayerModel> _players;
        public ObservableCollection<PlayerModel> Players
        {
            get { return _players; }
            protected set { Set(() => Players, ref _players, value); }
        }

        #region Load

        private ICommand _loadCommand;
        public ICommand LoadCommand => _loadCommand = _loadCommand ?? new RelayCommand(Load);

        private void Load()
        {
            try
            {
                Players = new ObservableCollection<PlayerModel>(PlayerManager.Load(ConfigurationManager.AppSettings["PlayersPath"]));
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

        private void SelectPlayer(PlayerModel player, bool switchToSaleTab)
        {
            Mediator.Default.Send(new PlayerSelectedMessage
            {
                DciNumber = player.DCINumber,
                FirstName = player.FirstName,
                LastName = player.LastName,
                SwitchToSaleTab = switchToSaleTab
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
