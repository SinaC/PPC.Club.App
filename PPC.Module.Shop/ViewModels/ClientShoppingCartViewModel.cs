using System;
using System.Linq;
using System.Windows.Input;
using EasyIoc;
using EasyMVVM;
using PPC.Common;
using PPC.Domain;
using PPC.Helpers;
using PPC.IDataAccess;
using PPC.Log;
using PPC.Module.Shop.Models;
using PPC.Services.Popup;

namespace PPC.Module.Shop.ViewModels
{
    public enum ClientShoppingCartPaymentStates
    {
        Paid,
        Unpaid
    }

    public class ClientShoppingCartViewModel : ObservableObject
    {
        private IPopupService PopupService => IocContainer.Default.Resolve<IPopupService>();
        private ILog Logger => IocContainer.Default.Resolve<ILog>();
        private IArticleDL ArticlesDL => IocContainer.Default.Resolve<IArticleDL>();
        private ISessionDL SessionDL => IocContainer.Default.Resolve<ISessionDL>();

        private Action<decimal, decimal, decimal> _cartPaidAction;
        private Action _cartReopenedAction;

        public string Filename => $"{PPCConfigurationManager.BackupPath}{(HasFullPlayerInfos ? DciNumber : ClientName.ToLowerInvariant())}.xml";

        public DateTime PaymentTimestamp { get; private set; }

        #region Client Info

        private Guid _clientGuid;
        public Guid ClientGuid
        {
            get { return _clientGuid; }
            set { Set(() => ClientGuid, ref _clientGuid, value); }
        }

        private string _clientName;
        public string ClientName
        {
            get { return _clientName; }
            set { Set(() => ClientName, ref _clientName, value); }
        }

        private string _clientFirstName;
        public string ClientFirstName
        {
            get { return _clientFirstName; }
            set { Set(() => ClientFirstName, ref _clientFirstName, value); }
        }

        private string _clientLastName;
        public string ClientLastName
        {
            get { return _clientLastName; }
            set { Set(() => ClientLastName, ref _clientLastName, value); }
        }

        private string _dciNumber;
        public string DciNumber
        {
            get { return _dciNumber; }
            set { Set(() => DciNumber, ref _dciNumber, value); }
        }

        private bool _hasFullPlayerInfos;
        public bool HasFullPlayerInfos
        {
            get { return _hasFullPlayerInfos; }
            set { Set(() => HasFullPlayerInfos, ref _hasFullPlayerInfos, value); }
        }

        public string DisplayName => HasFullPlayerInfos ? $"{ClientFirstName} {ClientLastName}" : ClientName;

        #endregion

        #region Reopen cart

        private ICommand _reOpenCommand;
        public ICommand ReOpenCommand => _reOpenCommand = _reOpenCommand ?? new RelayCommand(ReOpen);
        private void ReOpen()
        {
            PaymentState = ClientShoppingCartPaymentStates.Unpaid;
            _cartReopenedAction();
            Save();
        }

        #endregion

        private ClientShoppingCartPaymentStates _paymentState;
        public ClientShoppingCartPaymentStates PaymentState
        {
            get { return _paymentState; }
            protected set { Set(() => PaymentState, ref _paymentState, value); }
        }

        private ShoppingCartViewModel _shoppingCart;
        public ShoppingCartViewModel ShoppingCart
        {
            get { return _shoppingCart; }
            protected set { Set(() => ShoppingCart, ref _shoppingCart, value); }
        }

        private decimal _cash;
        public decimal Cash
        {
            get { return _cash; }
            protected set { Set(() => Cash, ref _cash, value); }
        }

        private decimal _bankCard;
        public decimal BankCard
        {
            get { return _bankCard; }
            protected set { Set(() => BankCard, ref _bankCard, value); }
        }

        private decimal _discountPercentage;
        public decimal DiscountPercentage
        {
            get { return _discountPercentage; }
            protected set { Set(() => DiscountPercentage, ref _discountPercentage, value); }
        }

        public void RemoveHandlers()
        {
            _cartPaidAction = null;
            _cartReopenedAction = null;
            ShoppingCart.RemoveHandlers();
        }

        public ClientShoppingCartViewModel(Action<decimal, decimal, decimal> cartPaidAction, Action cartReopenedAction)
        {
            ClientGuid = Guid.NewGuid();

            _cartPaidAction = cartPaidAction;
            _cartReopenedAction = cartReopenedAction;

            PaymentState = ClientShoppingCartPaymentStates.Unpaid;

            ShoppingCart = new ShoppingCartViewModel(Payment, Save);
        }

        //public ClientShoppingCartViewModel(Action<decimal, decimal, decimal> cartPaidAction, Action cartReopenedAction, string filename) 
        //    : this(cartPaidAction, cartReopenedAction)
        //{
        //    Load(filename);
        //}

        public ClientShoppingCartViewModel(Action<decimal, decimal, decimal> cartPaidAction, Action cartReopenedAction, ClientCart cart)
            : this(cartPaidAction, cartReopenedAction)
        {
            InitializeFromCart(cart);
        }

        private void Payment(decimal cash, decimal bankCard, decimal discountPercentage)
        {
            PaymentState = ClientShoppingCartPaymentStates.Paid;
            Cash = cash;
            BankCard = bankCard;
            DiscountPercentage = discountPercentage;
            PaymentTimestamp = DateTime.Now;

            Save();

            _cartPaidAction(cash, bankCard, discountPercentage);
        }

        #region Load/Save

        //private void Load(string filename)
        //{
        //    ClientCart cart;
        //    using (XmlTextReader reader = new XmlTextReader(filename))
        //    {
        //        DataContractSerializer serializer = new DataContractSerializer(typeof(ClientCart));
        //        cart = (ClientCart)serializer.ReadObject(reader);
        //    }
            
        //    LoadFromCart(cart);
        //}

        private void InitializeFromCart(ClientCart cart)
        {
            ClientGuid = cart.Guid;
            ClientName = cart.ClientName;
            ClientFirstName = cart.ClientFirstName;
            ClientLastName = cart.ClientLastName;
            DciNumber = cart.DciNumber;
            HasFullPlayerInfos = cart.HasFullPlayerInfos;
            PaymentState = cart.IsPaid
                ? ClientShoppingCartPaymentStates.Paid
                : ClientShoppingCartPaymentStates.Unpaid;
            PaymentTimestamp = cart.PaymentTimeStamp;
            Cash = cart.Cash;
            BankCard = cart.BankCard;
            ShoppingCart.ShoppingCartArticles.Clear();
            ShoppingCart.ShoppingCartArticles.AddRange(cart.Articles.Select(x => new ShopArticleItem
            {
                Article = ArticlesDL.GetById(x.Guid),
                Quantity = x.Quantity
            }));

            RaisePropertyChanged(() => ClientName);
        }

        private void Save()
        {
            try
            {
                ClientCart cart = new ClientCart
                {
                    Guid = ClientGuid,
                    ClientName = ClientName,
                    ClientFirstName = ClientFirstName,
                    ClientLastName = ClientLastName,
                    DciNumber = DciNumber,
                    HasFullPlayerInfos = HasFullPlayerInfos,
                    IsPaid = PaymentState == ClientShoppingCartPaymentStates.Paid,
                    PaymentTimeStamp = PaymentTimestamp,
                    Cash = Cash,
                    BankCard = BankCard,
                    Articles = ShoppingCart.ShoppingCartArticles.Select(x => new Item
                    {
                        Guid = x.Article.Guid,
                        Quantity = x.Quantity,
                    }).ToList()
                };

                SessionDL.SaveClientCart(cart);
            }
            catch (Exception ex)
            {
                Logger.Exception("Error while saving client cart", ex);
                PopupService.DisplayError("Error while saving client cart", ex);
            }
            //try
            //{
            //    if (!Directory.Exists(PPCConfigurationManager.BackupPath))
            //        Directory.CreateDirectory(PPCConfigurationManager.BackupPath);
            //    ClientCart cart = new ClientCart
            //    {
            //        ClientName = ClientName,
            //        ClientFirstName = ClientFirstName,
            //        ClientLastName = ClientLastName,
            //        DciNumber = DciNumber,
            //        HasFullPlayerInfos = HasFullPlayerInfos,
            //        IsPaid = PaymentState == ClientShoppingCartPaymentStates.Paid,
            //        PaymentTimeStamp = PaymentTimestamp,
            //        Cash = Cash,
            //        BankCard = BankCard,
            //        Articles = ShoppingCart.ShoppingCartArticles.Select(x => new Item
            //        {
            //            Guid = x.Article.Guid,
            //            Quantity = x.Quantity,
            //        }).ToList()
            //    };
            //    string filename = Filename;
            //    using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
            //    {
            //        writer.Formatting = Formatting.Indented;
            //        DataContractSerializer serializer = new DataContractSerializer(typeof(ClientCart));
            //        serializer.WriteObject(writer, cart);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Logger.Exception("Error while saving client cart", ex);
            //    PopupService.DisplayError("Error while saving client cart", ex);
            //}
        }

        #endregion
    }

    public class ClientShoppingCartViewModelDesignData : ClientShoppingCartViewModel
    {
        public ClientShoppingCartViewModelDesignData() : base((a, b, c) => { }, () => { })
        {
            ShoppingCart = new ShoppingCartViewModelDesignData();

            HasFullPlayerInfos = true;
            ClientName = "Toto";
            ClientFirstName = "Toto";
            ClientLastName = "Tsekwa";
            DciNumber = "123456789";
            PaymentState = ClientShoppingCartPaymentStates.Unpaid;
            Cash = 15;
            BankCard = 20;
        }
    }
}
