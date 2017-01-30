using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Input;
using System.Xml;
using PPC.DataContracts;
using PPC.MVVM;
using PPC.Popup;

namespace PPC.Sale
{
    public class ClientViewModel : ShoppingCartTabBase
    {
        private IPopupService PopupService => EasyIoc.IocContainer.Default.Resolve<IPopupService>();

        #region ShoppingCartTabBase

        public override string Header => ClientName;

        #endregion

        private string _clientName;
        public string ClientName
        {
            get { return _clientName; }
            set { Set(() => ClientName, ref _clientName, value); }
        }

        private bool _isCash;
        public bool IsCash
        {
            get { return _isCash; }
            set { Set(() => IsCash, ref _isCash, value); }
        }

        private ICommand _reOpenCommand;
        public ICommand ReOpenCommand => _reOpenCommand = _reOpenCommand ?? new RelayCommand(ReOpen);
        private void ReOpen()
        {
            IsPaid = false;
            _cartReopenedAction();
            Save();
        }

        private ShoppingCartViewModel _shoppingCart;
        public ShoppingCartViewModel ShoppingCart
        {
            get { return _shoppingCart; }
            private set { Set(() => ShoppingCart, ref _shoppingCart, value); }
        }

        private void CartPaid(bool isCash)
        {
            IsPaid = true;
            IsCash = isCash;
            _cartPaidAction();
            Save();
        }

        private readonly Action _cartPaidAction;
        private readonly Action _cartReopenedAction;

        public ClientViewModel(Action cartPaidAction, Action cartReopenedAction)
        {
            _cartPaidAction = cartPaidAction;
            _cartReopenedAction = cartReopenedAction;

            IsPaid = false;

            ShoppingCart = new ShoppingCartViewModel(CartPaid, Save);
        }

        private void Save()
        {
            try
            {
                if (!Directory.Exists(ConfigurationManager.AppSettings["BackupPath"]))
                    Directory.CreateDirectory(ConfigurationManager.AppSettings["BackupPath"]);
                ClientCart cart = new ClientCart
                {
                    Name = Header,
                    IsPaid = IsPaid,
                    IsCash = IsCash,
                    Articles = ShoppingCart.ShoppingCartArticles.Select(x => new SoldArticle
                    {
                        Ean = x.Article.Ean,
                        Quantity = x.Quantity,
                        IsCash = IsCash // irrelevant if IsPaid is false
                    }).ToList()
                };
                string filename = $"{ConfigurationManager.AppSettings["BackupPath"]}{Header.ToLowerInvariant()}.xml";
                using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
                {
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
            IsPaid = true;
            IsCash = false;
        }
    }
}
