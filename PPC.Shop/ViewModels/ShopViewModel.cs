using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using PPC.Helpers;
using PPC.Messages;
using PPC.MVVM;
using PPC.Popup;

namespace PPC.Shop.ViewModels
{
    // TODO: move referenced classes from Sale to Shop
    public enum ShopStates
    {
        Summary,
        Detail,
        SoldArticles
    }

    public class ShopViewModel : TabBase
    {
        private IPopupService PopupService => EasyIoc.IocContainer.Default.Resolve<IPopupService>();

        #region TabBase

        public override string Header => "Shop";

        #endregion

        #region View state

        private ShopStates _shopState;
        public ShopStates ShopState
        {
            get { return _shopState; }
            set
            {
                if (Set(() => ShopState, ref _shopState, value))
                {
                    RaisePropertyChanged(() => IsStateSummary);
                    RaisePropertyChanged(() => IsStateDetail);
                    RaisePropertyChanged(() => IsStateSoldArticles);
                }
            }
        }

        public bool IsStateSummary
        {
            get { return ShopState == ShopStates.Summary; }
            set { ShopState = ShopStates.Summary; }
        }

        public bool IsStateDetail
        {
            get { return ShopState == ShopStates.Detail; }
            set { ShopState = ShopStates.Detail; }
        }

        public bool IsStateSoldArticles
        {
            get { return ShopState == ShopStates.SoldArticles; }
            set { ShopState = ShopStates.SoldArticles; }
        }

        private ICommand _returnToSummaryCommand;
        public ICommand ReturnToSummaryCommand => _returnToSummaryCommand = _returnToSummaryCommand ?? new RelayCommand(ReturnToSummary);
        private void ReturnToSummary()
        {
            SelectedButton = null;
            ShopState = ShopStates.Summary;
        }

        private ICommand _viewSoldArticlesCommand;
        public ICommand ViewSoldArticlesCommand => _viewSoldArticlesCommand = _viewSoldArticlesCommand ?? new RelayCommand(ViewSoldArticle);
        private void ViewSoldArticle()
        {
            SelectedButton = null;
            ShopState = ShopStates.SoldArticles;
        }

        #endregion

        #region Cash register

        private CashRegisterViewModel _cashRegisterViewModel;
        public CashRegisterViewModel CashRegisterViewModel
        {
            get { return _cashRegisterViewModel; }
            set { Set(() => CashRegisterViewModel, ref _cashRegisterViewModel, value); }
        }

        #endregion

        #region Cash register/client buttons

        private Sale.ViewModels.ShoppingCartTabViewModelBase _selectedButton;
        public Sale.ViewModels.ShoppingCartTabViewModelBase SelectedButton
        {
            get { return _selectedButton;}
            set
            {
                if (Set(() => SelectedButton, ref _selectedButton, value))
                {
                    if (value == null)
                        ShopState = ShopStates.Summary; // switch back to summary when no button are selected
                    else
                        ShopState = ShopStates.Detail;
                }
            }
        }

        private ObservableCollection<Sale.ViewModels.ShoppingCartTabViewModelBase> _buttons;
        public ObservableCollection<Sale.ViewModels.ShoppingCartTabViewModelBase> Buttons
        {
            get { return _buttons; }
            protected set { Set(() => Buttons, ref _buttons, value); }
        }

        private ICommand _selectButtonCommand;
        public ICommand SelectButtonCommand => _selectButtonCommand = _selectButtonCommand ?? new RelayCommand<Sale.ViewModels.ShoppingCartTabViewModelBase>(button => SelectedButton = button);

        private ICommand _addNewClientCommand;
        public ICommand AddNewClientCommand => _addNewClientCommand = _addNewClientCommand ?? new RelayCommand(AddNewClient);

        private void AddNewClient()
        {
            AskNamePopupViewModel vm = new AskNamePopupViewModel(PopupService, AddNewTabNameSelected);
            PopupService.DisplayModal(vm, "Client name?");
        }

        private void AddNewTabNameSelected(string name)
        {
            if (Buttons.OfType<Sale.ViewModels.ClientViewModel>().Any(x => x.ClientName == name))
            {
                ErrorPopupViewModel vm = new ErrorPopupViewModel("A tab with that client name has already been opened!");
                PopupService.DisplayModal(vm, "Error");
            }
            else
            {
                Sale.ViewModels.ClientViewModel newClient = new Sale.ViewModels.ClientViewModel(RefreshSoldArticles, RefreshSoldArticles)
                {
                    ClientName = name
                };
                Buttons.Add(newClient);
                SelectedButton = newClient;
            }
        }

        #endregion

        #region Sold articles

        private List<Sale.ViewModels.ShopArticleItem> _soldArticles;

        public List<Sale.ViewModels.ShopArticleItem> SoldArticles
        {
            get { return _soldArticles; }
            private set { Set(() => SoldArticles, ref _soldArticles, value); }
        }

        #endregion

        #region Reload

        private ICommand _reloadCommand;
        public ICommand ReloadCommand => _reloadCommand = _reloadCommand ?? new RelayCommand(Reload);

        private void Reload()
        {
            PopupService.DisplayQuestion("Reload", "Are you sure you want to reload from backup ?",
                   new ActionButton
                   {
                       Order = 1,
                       Caption = "Yes",
                       ClickCallback = ReloadConfirmed
                   },
                   new ActionButton
                   {
                       Order = 2,
                       Caption = "No"
                   });
        }

        private void ReloadConfirmed()
        {
            if (Directory.Exists(ConfigurationManager.AppSettings["BackupPath"]))
            {
                try
                {
                    SelectedButton = CashRegisterViewModel; // Select shop tab
                    // Reload shop (sold articles)
                    CashRegisterViewModel.Load();
                    // Reload clients
                    //  remove existing clients
                    Buttons.RemoveOfType<Sale.ViewModels.ShoppingCartTabViewModelBase, Sale.ViewModels.ClientViewModel>();
                    //  add backup clients
                    foreach (string filename in Directory.EnumerateFiles(ConfigurationManager.AppSettings["BackupPath"], "*.xml", SearchOption.TopDirectoryOnly).Where(x => !x.Contains(Sale.ViewModels.ShopViewModel.ShopFile)))
                    {
                        try
                        {
                            Sale.ViewModels.ClientViewModel client = new Sale.ViewModels.ClientViewModel(RefreshSoldArticles, RefreshSoldArticles, filename);
                            Buttons.Add(client);
                        }
                        catch (Exception ex)
                        {
                            ErrorPopupViewModel vm = new ErrorPopupViewModel(ex);
                            PopupService.DisplayModal(vm, $"Error while loading {filename} cart");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorPopupViewModel vm = new ErrorPopupViewModel(ex);
                    PopupService.DisplayModal(vm, "Error while loading shop");
                }
                RefreshSoldArticles();
            }
            else
            {
                ErrorPopupViewModel vm = new ErrorPopupViewModel("Backup path not found");
                PopupService.DisplayModal(vm, "Error while loading shop");
            }
        }

        #endregion

        #region Computed values

        private int _soldArticlesCount;

        public int SoldArticlesCount
        {
            get { return _soldArticlesCount; }
            set { Set(() => SoldArticlesCount, ref _soldArticlesCount, value); }
        }

        private decimal _soldArticlesTotal;

        public decimal SoldArticlesTotal
        {
            get { return _soldArticlesTotal; }
            set { Set(() => SoldArticlesTotal, ref _soldArticlesTotal, value); }
        }

        private decimal _soldArticlesTotalCash;

        public decimal SoldArticlesTotalCash
        {
            get { return _soldArticlesTotalCash; }
            set { Set(() => SoldArticlesTotalCash, ref _soldArticlesTotalCash, value); }
        }

        private decimal _soldArticlesTotalBankCard;

        public decimal SoldArticlesTotalBankCard
        {
            get { return _soldArticlesTotalBankCard; }
            set { Set(() => SoldArticlesTotalBankCard, ref _soldArticlesTotalBankCard, value); }
        }

        #endregion

        public ShopViewModel()
        {
            ShopState = ShopStates.Summary;

            CashRegisterViewModel = new CashRegisterViewModel(RefreshSoldArticles);

            Buttons = new ObservableCollection<Sale.ViewModels.ShoppingCartTabViewModelBase>
            {
                CashRegisterViewModel
            };

            Mediator.Default.Register<PlayerSelectedMessage>(this, PlayerSelected);
        }

        private void RefreshSoldArticles()
        {
            decimal totalCash = 0;
            decimal totalBankCard = 0;
            Dictionary<Guid, Sale.ViewModels.ShopArticleItem> soldItems = new Dictionary<Guid, Sale.ViewModels.ShopArticleItem>();
            // Gather sold items in current shopping cart
            foreach (Sale.ViewModels.ShopArticleItem item in CashRegisterViewModel.Transactions.SelectMany(x => x.Articles))
            {
                Sale.ViewModels.ShopArticleItem soldItem;
                if (!soldItems.TryGetValue(item.Article.Guid, out soldItem))
                {
                    soldItem = new Sale.ViewModels.ShopArticleItem
                    {
                        Article = item.Article,
                        Quantity = 0
                    };
                    soldItems.Add(item.Article.Guid, soldItem);
                }
                soldItem.Quantity += item.Quantity;
            }
            totalCash += CashRegisterViewModel.Cash;
            totalBankCard += CashRegisterViewModel.BankCard;
            // Gather sold items in closed client shopping carts
            foreach (Sale.ViewModels.ClientViewModel client in Buttons.OfType<Sale.ViewModels.ClientViewModel>().Where(x => x.PaymentState == Sale.ViewModels.PaymentStates.Paid))
            {
                foreach (Sale.ViewModels.ShopArticleItem item in client.ShoppingCart.ShoppingCartArticles)
                {
                    Sale.ViewModels.ShopArticleItem soldItem;
                    if (!soldItems.TryGetValue(item.Article.Guid, out soldItem))
                    {
                        soldItem = new Sale.ViewModels.ShopArticleItem
                        {
                            Article = item.Article,
                            Quantity = 0,
                        };
                        soldItems.Add(item.Article.Guid, soldItem);
                    }
                    soldItem.Quantity += item.Quantity;
                }
                totalCash += client.Cash;
                totalBankCard += client.BankCard;
            }
            //
            SoldArticles = soldItems.Values.ToList();
            // Compute count/total/cash/bc
            SoldArticlesCount = SoldArticles.Sum(x => x.Quantity);
            SoldArticlesTotal = SoldArticles.Sum(x => x.Total);
            SoldArticlesTotalCash = totalCash;
            SoldArticlesTotalBankCard = totalBankCard;
        }

        private void PlayerSelected(PlayerSelectedMessage msg)
        {
            // Select tab or create it
            Sale.ViewModels.ClientViewModel client = Buttons.OfType<Sale.ViewModels.ClientViewModel>().FirstOrDefault(x => (x.DciNumber == msg.DciNumber && x.ClientFirstName == msg.FirstName && x.ClientLastName == msg.LastName));
            if (client == null)
            {
                Sale.ViewModels.ClientViewModel newClient = new Sale.ViewModels.ClientViewModel(RefreshSoldArticles, RefreshSoldArticles)
                {
                    ClientName = msg.FirstName,
                    ClientFirstName = msg.FirstName,
                    ClientLastName = msg.LastName,
                    DciNumber = msg.DciNumber
                };
                Buttons.Add(newClient);
                SelectedButton = newClient;
            }
            else
                SelectedButton = client;
        }
    }

    public class ShopViewModelDesignData : ShopViewModel
    {
        public ShopViewModelDesignData()
        {
            CashRegisterViewModel = new CashRegisterViewModelDesignData();

            Buttons = new ObservableCollection<Sale.ViewModels.ShoppingCartTabViewModelBase>
            {
                CashRegisterViewModel,
                new Sale.ViewModels.ClientViewModelDesignData
                {
                    ClientName = "un super long nom0",
                    PaymentState = Sale.ViewModels.PaymentStates.Unpaid
                },

                new Sale.ViewModels.ClientViewModelDesignData
                {
                    ClientName = "Joel",
                    PaymentState = Sale.ViewModels.PaymentStates.Paid
                },
                new Sale.ViewModels.ClientViewModelDesignData
                {
                    ClientName = "Pouet",
                    PaymentState = Sale.ViewModels.PaymentStates.Unpaid
                }
            };
            Buttons.AddRange(Enumerable.Range(1, 5).Select(x => new Sale.ViewModels.ClientViewModelDesignData
            {
                ClientName = $"un super long nom[{x}]",
                PaymentState = x%2 == 0 ? Sale.ViewModels.PaymentStates.Unpaid : Sale.ViewModels.PaymentStates.Paid
            }));
            //SelectedButton = Buttons[0];

            ShopState = ShopStates.Summary;
        }
    }
}
