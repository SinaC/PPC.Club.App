using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Xml;
using PPC.DataContracts;
using PPC.Helpers;
using PPC.MVVM;
using PPC.Popup;

namespace PPC.Sale.ViewModels
{
    public class ClientViewModel : ShoppingCartTabViewModelBase
    {
        private IPopupService PopupService => EasyIoc.IocContainer.Default.Resolve<IPopupService>();

        private readonly Action _cartPaidAction;
        private readonly Action _cartReopenedAction;

        public string Filename => $"{ConfigurationManager.AppSettings["BackupPath"]}{Header.ToLowerInvariant()}.xml";

        public DateTime PaymentTimestamp { get; private set; }

        #region ShoppingCartTabBase

        public override string Header => ClientName;

        #region TabBase

        public override bool IsClosable => true;

        #endregion

        #endregion

        #region Client Info

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

        #endregion

        #region Reopen cart

        private ICommand _reOpenCommand;
        public ICommand ReOpenCommand => _reOpenCommand = _reOpenCommand ?? new RelayCommand(ReOpen);
        private void ReOpen()
        {
            PaymentState = PaymentStates.Unpaid;
            _cartReopenedAction();
            Save();
        }

        #endregion

        public ClientViewModel(Action cartPaidAction, Action cartReopenedAction)
        {
            _cartPaidAction = cartPaidAction;
            _cartReopenedAction = cartReopenedAction;

            PaymentState = PaymentStates.Unpaid;

            ShoppingCart = new ShoppingCartViewModel(Payment, Save);
        }

        public ClientViewModel(Action cartPaidAction, Action cartReopenedAction, string filename) : this(cartPaidAction, cartReopenedAction)
        {
            Load(filename);
        }

        private void Payment(decimal cash, decimal bankCard)
        {
            PaymentState = PaymentStates.Paid;
            Cash = cash;
            BankCard = bankCard;
            _cartPaidAction();
            PaymentTimestamp = DateTime.Now;
            Save();
        }

        #region Load/Save

        private void Load(string filename)
        {
            ClientCart cart;
            using (XmlTextReader reader = new XmlTextReader(filename))
            {
                System.Runtime.Serialization.DataContractSerializer serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(ClientCart));
                cart = (ClientCart)serializer.ReadObject(reader);
            }
            ClientName = cart.ClientName;
            PaymentState = cart.IsPaid 
                ? PaymentStates.Paid 
                : PaymentStates.Unpaid;
            Cash = cart.Cash;
            BankCard = cart.BankCard;
            ShoppingCart.ShoppingCartArticles.Clear();
            ShoppingCart.ShoppingCartArticles.AddRange(cart.Articles.Select(x => new ShopArticleItem
            {
                Article = ArticleDb.Articles.FirstOrDefault(a => a.Guid == x.Guid),
                Quantity = x.Quantity
            }));

            RaisePropertyChanged(() => Header);
        }

        private void Save()
        {
            try
            {
                if (!Directory.Exists(ConfigurationManager.AppSettings["BackupPath"]))
                    Directory.CreateDirectory(ConfigurationManager.AppSettings["BackupPath"]);
                ClientCart cart = new ClientCart
                {
                    ClientName = ClientName,
                    IsPaid = PaymentState == PaymentStates.Paid,
                    Cash = Cash,
                    BankCard = BankCard,
                    Articles = ShoppingCart.ShoppingCartArticles.Select(x => new Item
                    {
                        Guid = x.Article.Guid,
                        Quantity = x.Quantity,
                    }).ToList()
                };
                string filename = Filename;
                using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
                {
                    writer.Formatting = Formatting.Indented;
                    System.Runtime.Serialization.DataContractSerializer serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(ClientCart));
                    serializer.WriteObject(writer, cart);
                }
            }
            catch (Exception ex)
            {
                ErrorPopupViewModel vm = new ErrorPopupViewModel(ex);
                PopupService.DisplayModal(vm, "Error while saving client cart");
            }
        }

        #endregion
    }

    public class ClientViewModelDesignData : ClientViewModel
    {
        public ClientViewModelDesignData() : base(() => { }, () => { })
        {
            ClientName = "Toto";
            ClientFirstName = "Toto";
            ClientLastName = "Tsekwa";
            DciNumber = "123456789";
            PaymentState = PaymentStates.Paid;
            Cash = 15;
            BankCard = 20;
        }
    }
}
