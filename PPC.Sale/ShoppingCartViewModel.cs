using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using PPC.MVVM;

namespace PPC.Sale
{
    public class ShoppingCartViewModel : ObservableObject
    {
        private readonly Action<bool> _cartPaidAction;
        private readonly Action _cartModifiedAction;

        private string _ean;
        public string Ean
        {
            get { return _ean; }
            set
            {
                if (Set(() => Ean, ref _ean, value))
                {
                    if (_ean != null)
                    {
                        if (_ean.Length == 13)
                            SelectedArticle = FakeArticleDb.Articles.FirstOrDefault(x => x.Ean == _ean);
                        else
                            SelectedArticle = null;
                    }
                }
            }
        }

        public IEnumerable<Article> Articles => FakeArticleDb.Articles; // TODO: hierarchical article list

        private Article _selectedArticle;
        public Article SelectedArticle
        {
            get { return _selectedArticle; }
            set
            {
                if (Set(() => SelectedArticle, ref _selectedArticle, value))
                {
                    Quantity = _selectedArticle == null
                        ? (int?) null
                        : 1;
                    _ean = _selectedArticle?.Ean; // set value without retriggering article search
                    RaisePropertyChanged(() => Ean);
                }
            }
        }

        private int? _quantity;
        public int? Quantity
        {
            get {  return _quantity;}
            set { Set(() => Quantity, ref _quantity, value); }
        }

        private ICommand _inputCommand;
        public ICommand InputCommand => _inputCommand = _inputCommand ?? new RelayCommand<Key>(Input);
        protected virtual void Input(Key key)
        {
            switch (key)
            {
                case Key.Return:
                    AddSelectedArticleCommand.Execute(null);
                    break;
                case Key.Add:
                    IncrementSelectedArticleCommand.Execute(null);
                    break;
                case Key.Subtract:
                    _decrementSelectedArticleCommand.Execute(null);
                    break;
                case Key.C:
                    CashCommand.Execute(null);
                    break;
                case Key.B:
                    BankCardCommand.Execute(null);
                    break;
            }
        }

        private ICommand _incrementSelectedArticleCommand;
        public ICommand IncrementSelectedArticleCommand => _incrementSelectedArticleCommand = _incrementSelectedArticleCommand ?? new RelayCommand(IncrementSelectedArticle);
        private void IncrementSelectedArticle()
        {
            if (SelectedArticle == null)
                return;
            if (!Quantity.HasValue)
                Quantity = 1;
            else
                Quantity++;
        }

        private ICommand _decrementSelectedArticleCommand;
        public ICommand DecrementSelectedArticleCommand => _decrementSelectedArticleCommand = _decrementSelectedArticleCommand ?? new RelayCommand(DecrementSelectedArticle);
        private void DecrementSelectedArticle()
        {
            if (SelectedArticle == null)
                return;
            if (Quantity.HasValue)
            {
                if (Quantity.Value == 1)
                    Quantity = null;
                else
                    Quantity--;
            }
        }

        private ICommand _addSelectedArticleCommand;
        public ICommand AddSelectedArticleCommand => _addSelectedArticleCommand = _addSelectedArticleCommand ?? new RelayCommand(AddSelectedArticle);
        private void AddSelectedArticle()
        {
            if (SelectedArticle == null)
                return;
            if (!Quantity.HasValue || Quantity.Value == 0)
                return;
            ShoppingCartArticleItem article = ShoppingCartArticles.FirstOrDefault(x => x.Article.Ean == SelectedArticle.Ean);
            if (article == null)
            {
                article = new ShoppingCartArticleItem
                {
                    Article = SelectedArticle,
                    Quantity = 0
                };
                ShoppingCartArticles.Add(article);
            }
            article.Quantity += Quantity.Value;
            RaisePropertyChanged(() => Total);
            _cartModifiedAction?.Invoke();
        }

        public ObservableCollection<ShoppingCartArticleItem> ShoppingCartArticles { get; set; }

        private ICommand _deleteArticleCommand;
        public ICommand DeleteArticleCommand => _deleteArticleCommand = _deleteArticleCommand ?? new RelayCommand<ShoppingCartArticleItem>(DeleteArticle);

        private void DeleteArticle(ShoppingCartArticleItem item)
        {
            ShoppingCartArticles.Remove(item);
            RaisePropertyChanged(() => Total);
            _cartModifiedAction?.Invoke();
        }

        private ICommand _incrementArticleCommand;
        public ICommand IncrementArticleCommand => _incrementArticleCommand = _incrementArticleCommand ?? new RelayCommand<ShoppingCartArticleItem>(IncrementArticle);

        private void IncrementArticle(ShoppingCartArticleItem item)
        {
            item.Quantity++;
            RaisePropertyChanged(() => Total);
            _cartModifiedAction?.Invoke();
        }

        private ICommand _decrementArticleCommand;
        public ICommand DecrementArticleCommand => _decrementArticleCommand = _decrementArticleCommand ?? new RelayCommand<ShoppingCartArticleItem>(DecrementArticle);

        private void DecrementArticle(ShoppingCartArticleItem item)
        {
            if (item.Quantity == 1)
                DeleteArticle(item);
            else
            {
                item.Quantity--;
                RaisePropertyChanged(() => Total);
            }
            _cartModifiedAction?.Invoke();
        }

        private ICommand _cashCommand;
        public ICommand CashCommand => _cashCommand = _cashCommand ?? new RelayCommand(() => _cartPaidAction(true));

        private ICommand _bankCardCommand;
        public ICommand BankCardCommand => _bankCardCommand = _bankCardCommand ?? new RelayCommand(() => _cartPaidAction(false));

        public void Clear()
        {
            ShoppingCartArticles.Clear();
            RaisePropertyChanged(() => Total);
            _cartModifiedAction?.Invoke();
        }

        public double Total => ShoppingCartArticles.Sum(x => x.Total);

        public ShoppingCartViewModel(Action<bool> cartPaidAction, Action cartModifiedAction = null)
        {
            _cartPaidAction = cartPaidAction;
            _cartModifiedAction = cartModifiedAction;

            ShoppingCartArticles = new ObservableCollection<ShoppingCartArticleItem>();
        }
    }

    public class ShoppingCartViewModelDesignData : ShoppingCartViewModel
    {
        public ShoppingCartViewModelDesignData() : base(b => { }, () => { })
        {
            ShoppingCartArticles = new ObservableCollection<ShoppingCartArticleItem>
            {
                new ShoppingCartArticleItem
                {
                    Article = FakeArticleDb.Articles[0],
                    Quantity = 2,
                },
                new ShoppingCartArticleItem
                {
                    Article = FakeArticleDb.Articles[1],
                    Quantity = 3,
                },
                new ShoppingCartArticleItem
                {
                    Article = FakeArticleDb.Articles[2],
                    Quantity = 1,
                }
            };
        }
    }
}
