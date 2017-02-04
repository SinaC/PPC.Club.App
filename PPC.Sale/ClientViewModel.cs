using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Input;
using System.Xml;
using PPC.DataContracts;
using PPC.Helpers;
using PPC.MVVM;
using PPC.Popup;

namespace PPC.Sale
{
    public class ClientViewModel : ShoppingCartTabBase
    {
        private IPopupService PopupService => EasyIoc.IocContainer.Default.Resolve<IPopupService>();

        private readonly Action _cartPaidAction;
        private readonly Action _cartReopenedAction;

        #region ShoppingCartTabBase

        public override string Header => ClientName;

        #endregion

        private string _clientName;
        public string ClientName
        {
            get { return _clientName; }
            set { Set(() => ClientName, ref _clientName, value); }
        }

        private ICommand _reOpenCommand;
        public ICommand ReOpenCommand => _reOpenCommand = _reOpenCommand ?? new RelayCommand(ReOpen);
        private void ReOpen()
        {
            IsPaid = false;
            _cartReopenedAction();
            Save();
        }

        public void Load(string filename)
        {
            ClientCart cart;
            using (XmlTextReader reader = new XmlTextReader(filename))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(ClientCart));
                cart = (ClientCart)serializer.ReadObject(reader);
            }
            ClientName = cart.ClientName;
            IsPaid = cart.IsPaid;
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

        public ClientViewModel(Action cartPaidAction, Action cartReopenedAction)
        {
            _cartPaidAction = cartPaidAction;
            _cartReopenedAction = cartReopenedAction;

            IsPaid = false;

            ShoppingCart = new ShoppingCartViewModel(Payment, Save);
        }

        private void Payment(decimal cash, decimal bankCard)
        {
            IsPaid = true;
            Cash = cash;
            BankCard = bankCard;
            _cartPaidAction();
            Save();
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
                    IsPaid = IsPaid,
                    Cash = Cash,
                    BankCard = BankCard,
                    Articles = ShoppingCart.ShoppingCartArticles.Select(x => new Item
                    {
                        Guid = x.Article.Guid,
                        Quantity = x.Quantity,
                    }).ToList()
                };
                string filename = $"{ConfigurationManager.AppSettings["BackupPath"]}{Header.ToLowerInvariant()}.xml";
                using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
                {
                    writer.Formatting = Formatting.Indented;
                    DataContractSerializer serializer = new DataContractSerializer(typeof(ClientCart));
                    serializer.WriteObject(writer, cart);
                }
            }
            catch (Exception ex)
            {
                ErrorPopupViewModel vm = new ErrorPopupViewModel(ex);
                PopupService.DisplayModal(vm, "Error while saving client cart");
            }
        }
    }

    public class ClientViewModelDesignData : ClientViewModel
    {
        public ClientViewModelDesignData() : base(() => { }, () => { })
        {
            ClientName = "James";
            IsPaid = true;
            Cash = 15;
            BankCard = 20;
        }
    }
}
