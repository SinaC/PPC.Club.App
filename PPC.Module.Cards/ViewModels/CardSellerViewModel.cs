using System;
using System.Collections.ObjectModel;
using System.Configuration;
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
using PPC.Module.Cards.Models;
using PPC.Services.Popup;

namespace PPC.Module.Cards.ViewModels
{
    public class CardSellerViewModel : ObservableObject
    {
        private IPopupService PopupService => IocContainer.Default.Resolve<IPopupService>();
        private ILog Logger => IocContainer.Default.Resolve<ILog>();

        public static string Path => $"{ConfigurationManager.AppSettings["BackupPath"]}cards\\";
        public string Filename => $"{Path}{SellerName}.xml";

        #region Seller info

        private string _sellerName;
        public string SellerName
        {
            get { return _sellerName; }
            protected set { Set(() => SellerName, ref _sellerName, value); }
        }

        private string _email;
        public string Email
        {
            get { return _email; }
            protected set { Set(() => Email, ref _email, value); }
        }

        #endregion

        #region Card info

        private string _cardName;
        public string CardName
        {
            get { return _cardName; }
            set { Set(() => CardName, ref _cardName, value); }
        }

        private bool _isCardNameFocused;
        public bool IsCardNameFocused
        {
            get { return _isCardNameFocused; }
            set { Set(() => IsCardNameFocused, ref _isCardNameFocused, value, true); }
        }

        private decimal _price;
        public decimal Price
        {
            get { return _price; }
            set { Set(() => Price, ref _price, value); }
        }

        private int _quantity;
        public int Quantity
        {
            get { return _quantity; }
            set { Set(() => Quantity, ref _quantity, value); }
        }

        #endregion

        #region Increment quantity

        private ICommand _incrementQuantityCommand;
        public ICommand IncrementQuantityCommand => _incrementQuantityCommand = _incrementQuantityCommand ?? new RelayCommand(IncrementQuantity);

        private void IncrementQuantity()
        {
            Quantity++;
        }

        #endregion

        #region Decrement quantity

        private ICommand _decrementQuantityCommand;
        public ICommand DecrementQuantityCommand => _decrementQuantityCommand = _decrementQuantityCommand ?? new RelayCommand(DecrementQuantity);

        private void DecrementQuantity()
        {
            if (Quantity > 0)
                Quantity--;
        }

        #endregion

        #region Add card(s) to items

        private ICommand _addCardCommand;
        public ICommand AddCardCommand => _addCardCommand = _addCardCommand ?? new RelayCommand(AddCard);

        private void AddCard()
        {
            if (Quantity <= 0)
                return;
            Items.Add(new SoldCardItem
            {
                CardName = CardName,
                Price = Price,
                Quantity = Quantity
            });
            Quantity = 1;
            IsCardNameFocused = true;
            RaisePropertyChanged(() => Total);

            Save();
        }

        #endregion

        #region Delete card

        private ICommand _deleteCardCommand;
        public ICommand DeleteCardCommand => _deleteCardCommand = _deleteCardCommand ?? new RelayCommand<SoldCardItem>(DeleteCard);

        private void DeleteCard(SoldCardItem item)
        {
            Items.Remove(item);
            RaisePropertyChanged(() => Total);

            Save();
        }

        #endregion

        #region Increment card quantity

        private ICommand _incrementCardCommand;
        public ICommand IncrementCardCommand => _incrementCardCommand = _incrementCardCommand ?? new RelayCommand<SoldCardItem>(IncrementCard);

        private void IncrementCard(SoldCardItem item)
        {
            item.Quantity++;
            RaisePropertyChanged(() => Total);

            Save();
        }

        #endregion

        #region Decrement card quantity

        private ICommand _decrementCardCommand;
        public ICommand DecrementCardCommand => _decrementCardCommand = _decrementCardCommand ?? new RelayCommand<SoldCardItem>(DecrementCard);

        private void DecrementCard(SoldCardItem item)
        {
            if (item.Quantity == 1)
                DeleteCard(item);
            else
            {
                item.Quantity--;
                RaisePropertyChanged(() => Total);

                Save();
            }
        }

        #endregion

        public decimal Total => Items?.Sum(x => x.Total) ?? 0;

        private SoldCardItem _selectedCard;
        public SoldCardItem SelectedCard
        {
            get { return _selectedCard; }
            set
            {
                if (Set(() => SelectedCard, ref _selectedCard, value))
                {
                    if (SelectedCard != null)
                    {
                        CardName = SelectedCard.CardName;
                        Price = SelectedCard.Price;
                        Quantity = SelectedCard.Quantity;
                    }
                }
            }
        }

        private ObservableCollection<SoldCardItem> _items;
        public ObservableCollection<SoldCardItem> Items
        {
            get { return _items; }
            protected set { Set(() => Items, ref _items, value); }
        }

        public void GotFocus()
        {
            IsCardNameFocused = true;
        }

        protected CardSellerViewModel()
        {
            Quantity = 1;
        }

        public CardSellerViewModel(string sellerName, string email)
            : this()
        {
            SellerName = sellerName;
            Email = email;

            Items = new ObservableCollection<SoldCardItem>();
        }

        public CardSellerViewModel(string filename)
             : this()
        {
            Load(filename);
        }

        #region Load/Save

        private void Load(string filename)
        {
            SoldCards soldCards;
            using (XmlTextReader reader = new XmlTextReader(filename))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(SoldCards));
                soldCards = (SoldCards)serializer.ReadObject(reader);
            }
            SellerName = soldCards.SellerName;
            Email = soldCards.Email;
            Items = new ObservableCollection<SoldCardItem>(soldCards.Cards.Select(x => new SoldCardItem
            {
                CardName = x.CardName,
                Price = x.Price,
                Quantity = x.Quantity
            }));
        }

        private void Save()
        {
            try
            {
                if (!Directory.Exists(Path))
                    Directory.CreateDirectory(Path);
                SoldCards soldCards = new SoldCards
                {
                    SellerName = SellerName,
                    Email = Email,
                    Cards = Items.Select(x => new SoldCard
                    {
                        CardName = x.CardName,
                        Price = x.Price,
                        Quantity = x.Quantity
                    }).ToList()
                };
                string filename = Filename;
                using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
                {
                    writer.Formatting = Formatting.Indented;
                    DataContractSerializer serializer = new DataContractSerializer(typeof(SoldCards));
                    serializer.WriteObject(writer, soldCards);
                }
            }
            catch (Exception ex)
            {
                Logger.Exception("Error while saving sold cards", ex);
                PopupService.DisplayError("Error while saving sold cards", ex);
            }
        }

        #endregion
    }

    public class CardSellerViewModelDesignData : CardSellerViewModel
    {
        public CardSellerViewModelDesignData() 
            : base("user", "user@mail.com")
        {
            Items = new ObservableCollection<SoldCardItem>
            {
                new SoldCardItem
                {
                    CardName = "Karakas",
                    Quantity = 1,
                    Price = 64m
                },
                new SoldCardItem
                {
                    CardName = "Abyss",
                    Quantity = 1,
                    Price = 120m
                }
            };
            RaisePropertyChanged(() => Total);
        }
    }
}
