using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows.Input;
using PPC.Data.Articles;
using PPC.DataContracts;
using PPC.MVVM;
using PPC.Popup;
using PPC.Services;

namespace PPC.Inventory.ViewModels
{
    public class InventoryViewModel : ObservableObject
    {
        // TODO: edit/new article

        private IPopupService PopupService => EasyIoc.IocContainer.Default.Resolve<IPopupService>();

        public IEnumerable<Article> Articles => ArticlesDb.Instance.Articles;

        #region Load

        private ICommand _loadCommand;
        public ICommand LoadCommand => _loadCommand = _loadCommand ?? new RelayCommand(Load);

        private void Load()
        {
            try
            {
                ArticlesDb.Instance.Load();
                RaisePropertyChanged(() => Articles);
                PopupService.DisplayQuestion("Load", "Articles successfully loaded.", new ActionButton
                {
                    Caption = "Ok",
                    Order = 1
                });
            }
            catch (Exception ex)
            {
                ErrorPopupViewModel vm = new ErrorPopupViewModel(PopupService, ex);
                PopupService.DisplayModal(vm, "Load error");
            }
        }

        #endregion

        #region Save

        private ICommand _saveCommand;
        public ICommand SaveCommand => _saveCommand = _saveCommand ?? new RelayCommand(Save);

        private void Save()
        {
            try
            {
                ArticlesDb.Instance.Save();
                PopupService.DisplayQuestion("Save", "Articles successfully saved.", new ActionButton
                {
                    Caption = "Ok",
                    Order = 1
                });
            }
            catch (Exception ex)
            {
                ErrorPopupViewModel vm = new ErrorPopupViewModel(PopupService, ex);
                PopupService.DisplayModal(vm, "Save error");
            }
        }

        #endregion

        #region Import

        private ICommand _importCommand;
        public ICommand ImportCommand => _importCommand = _importCommand ?? new RelayCommand(Import);

        private void Import()
        {
            string path = System.IO.Path.GetDirectoryName(ConfigurationManager.AppSettings["ArticlesPath"]);

            IIOService ioService = new IOService();
            string filename = ioService.OpenFileDialog(path);
            if (!string.IsNullOrWhiteSpace(filename))
            {
                try
                {
                    ArticlesDb.Instance.ImportFromCsv(filename);
                    RaisePropertyChanged(() => Articles);
                    PopupService.DisplayQuestion("Import", $"{Articles.Count()} articles successfully imported. Don't forget to click on 'Save' button to save imported articles.",
                        new ActionButton
                        {
                            Caption = "Ok",
                            Order = 1
                        });
                    //!! after import -> every article guid are modified -> shopping carts and backup files are invalid
                    // TODO: 
                    //  import button must be disabled once a shopping cart has been created
                    //  backup files must be deleted once import is performed
                }
                catch (Exception ex)
                {
                    ErrorPopupViewModel vm = new ErrorPopupViewModel(PopupService, ex);
                    PopupService.DisplayModal(vm, "Import error");
                }
            }
        }

        #endregion
    }

    public class InventoryViewModelDesignData : InventoryViewModel
    {
    }
}
