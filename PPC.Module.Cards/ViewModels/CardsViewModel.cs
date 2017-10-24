using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Input;
using System.Xml;
using EasyIoc;
using EasyMVVM;
using PPC.Data.Contracts;
using PPC.Log;
using PPC.Module.Cards.ViewModels.Popups;
using PPC.Services.Popup;

namespace PPC.Module.Cards.ViewModels
{
    public enum CardSellersModes
    {
        List,
        Detail
    }

    public class CardsViewModel : ObservableObject
    {
        private IPopupService PopupService => IocContainer.Default.Resolve<IPopupService>();
        private ILog Logger => IocContainer.Default.Resolve<ILog>();

        private CardSellers _cardSellers;

        private Func<string,string> SearchEmailByName => name => _cardSellers?.Sellers.FirstOrDefault(x => CultureInfo.CurrentCulture.CompareInfo.IndexOf(x.Name, name, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) >= 0)?.Email;
        private IEnumerable<string> SellerNames => _cardSellers?.Sellers.Select(x => x.Name) ?? Enumerable.Empty<string>();

        #region Seller selection

        private ICommand _selectSellerCommand;
        public ICommand SelectSellerCommand => _selectSellerCommand = _selectSellerCommand ?? new RelayCommand<CardSellerViewModel>(SelectSeller);

        private void SelectSeller(CardSellerViewModel seller)
        {
            SelectedSeller = seller;
        }

        private CardSellerViewModel _selectedSeller;
        public CardSellerViewModel SelectedSeller
        {
            get { return _selectedSeller; }
            set
            {
                if (Set(() => SelectedSeller, ref _selectedSeller, value))
                {
                    if (SelectedSeller == null)
                        Mode = CardSellersModes.List;
                    else
                    {
                        Mode = CardSellersModes.Detail;
                        SelectedSeller.GotFocus();
                    }
                }
            }
        }

        #endregion

        #region Mode

        private CardSellersModes _mode;
        public CardSellersModes Mode
        {
            get { return _mode; }
            protected set { Set(() => Mode, ref _mode, value); }
        }

        #endregion

        private ObservableCollection<CardSellerViewModel> _sellers;
        public ObservableCollection<CardSellerViewModel> Sellers
        {
            get { return _sellers; }
            protected set { Set(() => Sellers, ref _sellers, value); }
        }

        #region Add seller

        private ICommand _addNewSellerCommand;
        public ICommand AddNewSellerCommand => _addNewSellerCommand = _addNewSellerCommand ?? new RelayCommand(AddNewSeller);

        private void AddNewSeller()
        {
            AskNameEmailPopupViewModel vm = new AskNameEmailPopupViewModel(AddNewSellerNameSelected, SellerNames, SearchEmailByName);
            PopupService.DisplayModal(vm, "Seller name?");
        }

        private void AddNewSellerNameSelected(string name, string email)
        {
            CardSellerViewModel alreadyExisingSeller = Sellers.FirstOrDefault(x => String.Equals(x.SellerName, name, StringComparison.InvariantCultureIgnoreCase));
            if (alreadyExisingSeller != null)
            {
                Logger.Warning($"A seller with than name '{name}' has already been opened!");
                PopupService.DisplayError(
                    "Warning",
                    $"A seller with than name '{name}' has already been opened! Switching to {name}",
                    () => SelectedSeller = alreadyExisingSeller);
            }
            else
            {
                // Save sellers
                _cardSellers = _cardSellers ?? new CardSellers();
                CardSeller cardSeller = _cardSellers.Sellers.FirstOrDefault(x => CultureInfo.CurrentCulture.CompareInfo.IndexOf(x.Name, name, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) >= 0);
                if (cardSeller == null)
                    _cardSellers.Sellers.Add(new CardSeller
                    {
                        Name = name,
                        Email = email
                    });
                else
                    cardSeller.Email = email;
                SaveSellers();

                // Add seller view model
                CardSellerViewModel newSeller = new CardSellerViewModel(name, email);
                Sellers.Add(newSeller);
                SelectedSeller = newSeller;
            }
        }

        #endregion

        public void Reload()
        {
            string path = CardSellerViewModel.Path;
            if (Directory.Exists(path))
            {
                foreach (string filename in Directory.EnumerateFiles(path, "*.xml", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        CardSellerViewModel seller = new CardSellerViewModel(filename);
                        Sellers.Add(seller);
                    }
                    catch (Exception ex)
                    {
                        Logger.Exception($"Error while loading {filename ?? "??"} seller", ex);
                        PopupService.DisplayError($"Error while loading {filename ?? "??"} seller", ex);
                    }
                }
            }
        }

        public List<SoldCards> PrepareClosure()
        {
            return Sellers.Select(x => new SoldCards
            {
                SellerName = x.SellerName,
                Email = x.Email,
                Cards = x.Items.Select(c => new SoldCard
                {
                    CardName = c.CardName,
                    Price = c.Price,
                    Quantity = c.Quantity,
                }).ToList()
            }).ToList();
        }

        public void DeleteBackupFiles(string savePath)
        {
            // Move backup files into save folder
            try
            {
                string backupPath = CardSellerViewModel.Path;
                foreach (string file in Directory.EnumerateFiles(backupPath))
                {
                    string saveFilename = savePath + "cards_"+Path.GetFileName(file);
                    File.Move(file, saveFilename);
                }
            }
            catch (Exception ex)
            {
                Logger.Exception("Error", ex);
                PopupService.DisplayError("Error", ex);
            }
        }

        public CardsViewModel()
        {
            Sellers = new ObservableCollection<CardSellerViewModel>();
            Mode = CardSellersModes.List;

            if (!DesignMode.IsInDesignModeStatic)
                LoadSellers();
        }

        private void LoadSellers()
        {
            try
            {
                string filename = ConfigurationManager.AppSettings["CardSellersPath"];
                if (File.Exists(filename))
                {
                    using (XmlTextReader reader = new XmlTextReader(filename))
                    {
                        DataContractSerializer serializer = new DataContractSerializer(typeof(CardSellers));
                        _cardSellers = (CardSellers) serializer.ReadObject(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Exception("Error loading card sellers file", ex);
                PopupService.DisplayError("Error loading card sellers file", ex);
            }
        }

        private void SaveSellers()
        {
            try
            {
                string filename = ConfigurationManager.AppSettings["CardSellersPath"];
                using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
                {
                    writer.Formatting = Formatting.Indented;
                    DataContractSerializer serializer = new DataContractSerializer(typeof(CardSellers));
                    serializer.WriteObject(writer, _cardSellers);
                }
            }
            catch (Exception ex)
            {
                Logger.Exception("Error saving card sellers file", ex);
                PopupService.DisplayError("Error saving card sellers file", ex);
            }
        }
    }

    public class CardsViewModelDesignData : CardsViewModel
    {
        public CardsViewModelDesignData()
        {
            Sellers = new ObservableCollection<CardSellerViewModel>
            {
                new CardSellerViewModelDesignData(),
                new CardSellerViewModelDesignData()
            };
        }
    }
}
