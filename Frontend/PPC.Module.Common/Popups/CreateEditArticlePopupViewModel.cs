using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using EasyIoc;
using EasyMVVM;
using PPC.Domain;
using PPC.Services.Popup;

namespace PPC.Module.Common.Popups
{
    [PopupAssociatedView(typeof(CreateEditArticlePopup))]
    public class CreateEditArticlePopupViewModel : ObservableObject
    {
        private IPopupService PopupService => IocContainer.Default.Resolve<IPopupService>();

        private readonly Action<CreateEditArticlePopupViewModel> _saveArticleAction;
        private readonly Func<string, IEnumerable<string>> _buildSubCategoriesFunc;

        // true: edition  false: creation
        private bool _isEdition;
        public bool IsEdition {
            get => _isEdition;
            set { Set(() => IsEdition, ref _isEdition, value); }
        }

        private string _ean;
        public string Ean
        {
            get => _ean;
            set { Set(() => Ean, ref _ean, value); }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set { Set(() => Description, ref _description, value); }
        }

        public ObservableCollection<string> Categories { get; }

        private string _category;
        public string Category
        {
            get => _category;
            set
            {
                if (Set(() => Category, ref _category, value))
                {
                    if (string.Compare(Category, "BOISSONS", StringComparison.InvariantCultureIgnoreCase) == 0) 
                        VatRate = VatRates.FoodDrink;
                    else
                        VatRate = VatRates.Other;
                    if (Category == null)
                    {
                        SubCategory = null;
                        SubCategories = null;
                    }
                    else
                    {
                        SubCategory = null;
                        SubCategories = new ObservableCollection<string>(_buildSubCategoriesFunc(Category).OrderBy(x => x));
                        RaisePropertyChanged(() => SubCategories);
                    }
                }
            }
        }

        public ObservableCollection<string> SubCategories { get; private set; }

        private string _subCategory;
        public string SubCategory
        {
            get => _subCategory;
            set { Set(() => SubCategory, ref _subCategory, value); }
        }

        public ObservableCollection<string> Producers { get; }

        private string _producer;
        public string Producer
        {
            get => _producer;
            set { Set(() => Producer, ref _producer, value); }
        }

        private decimal _supplierPrice;
        public decimal SupplierPrice
        {
            get => _supplierPrice;
            set { Set(() => SupplierPrice, ref _supplierPrice, value); }
        }

        private decimal _price;
        public decimal Price
        {
            get => _price;
            set { Set(() => Price, ref _price, value); }
        }

        public Dictionary<VatRates, string> VatRateList { get; }

        private VatRates _vatRate;
        public VatRates VatRate
        {
            get => _vatRate;
            set { Set(() => VatRate, ref _vatRate, value); }
        }

        private int _stock;
        public int Stock
        {
            get => _stock;
            set { Set(() => Stock, ref _stock, value); }
        }

        private ICommand _createEditArticleCommand;
        public ICommand SaveArticleCommand => _createEditArticleCommand = _createEditArticleCommand ?? new RelayCommand(SaveArticle);

        private void SaveArticle()
        {
            PopupService?.Close(this);
            _saveArticleAction(this);
        }

        public CreateEditArticlePopupViewModel(Article article, IEnumerable<string> categories, IEnumerable<string> producers, Func<string, IEnumerable<string>> buildSubCategoriesFunc, Action<CreateEditArticlePopupViewModel> saveArticleAction)
        {
            _saveArticleAction = saveArticleAction;
            _buildSubCategoriesFunc = buildSubCategoriesFunc;
            Categories = new ObservableCollection<string>(categories.OrderBy(x => x));
            Producers = new ObservableCollection<string>(producers.OrderBy(x => x));
            VatRateList = Enum.GetValues(typeof(VatRates)).Cast<VatRates>().ToDictionary(x => x, x => GetEnumDescription(x));

            Ean = article?.Ean;
            Description = article?.Description;
            Category = article?.Category;
            SubCategory = article?.SubCategory;
            Producer = article?.Producer;
            SupplierPrice = article?.SupplierPrice ?? 0;
            Price = article?.Price ?? 0;
            VatRate = article?.VatRate ?? VatRates.Other;
            Stock = article?.Stock ?? 9999;
        }

        private string GetEnumDescription(Enum value)
        {
            // Get the Description attribute value for the enum value
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes =(DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }
    }

    public class CreateEditArticlePopupViewModelDesignData : CreateEditArticlePopupViewModel
    {
        public CreateEditArticlePopupViewModelDesignData() : base(null, new[] {""}, new[] {""}, _ => new[] {""}, _ => { })
        {
            Ean = "1111111111111";
            Description = "Article1";
            Category = "Cards";
            Producer = "MTG";
            SupplierPrice = 5.45m;
            Price = 8;
            Stock = 7;
        }
    }
}
