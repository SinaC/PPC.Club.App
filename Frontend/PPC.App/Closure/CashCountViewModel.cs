using System;
using System.Collections.Generic;
using System.Linq;
using EasyIoc;
using EasyMVVM;
using PPC.Common;
using PPC.Domain;
using PPC.Helpers;
using PPC.Log;
using PPC.Services.Popup;

namespace PPC.App.Closure
{
    public enum CountCategories
    {
        Figures,
        Coins
    }

    public class CashCountItem : ObservableObject
    {
        private readonly Action _currentCountModifiedAction;

        public CountCategories Category { get; }

        public decimal Value { get; }

        public int Reference { get; }

        public decimal ReferenceTotal => Reference*Value;
        public decimal CurrentTotal => Current * Value;

        private int _current;
        public int Current
        {
            get => _current;
            set
            {
                if (Set(() => Current, ref _current, value))
                {
                    RaisePropertyChanged(() => CurrentTotal);
                    _currentCountModifiedAction?.Invoke();
                }
            }
        }

        public CashCountItem(CountCategories category, decimal value, int reference, Action currentCountModifiedAction)
        {
            Category = category;
            Value = value;
            Reference = reference;
            _currentCountModifiedAction = currentCountModifiedAction;
        }
    }

    public class CashCountViewModel : ObservableObject
    {
        private IPopupService PopupService => IocContainer.Default.Resolve<IPopupService>();
        private ILog Logger => IocContainer.Default.Resolve<ILog>();

        protected decimal[] Keys = {500, 200, 100, 50, 20, 10, 5, 2, 1, 0.5m, 0.2m, 0.1m, 0.05m, 0.02m, 0.01m};

        protected List<CashCountItem> Counts { get; set; }

        public IEnumerable<CashCountItem> Coins => Counts.Where(x => x.Category == CountCategories.Coins);
        public IEnumerable<CashCountItem> Figures => Counts.Where(x => x.Category == CountCategories.Figures);

        public decimal ReferenceTotal => Counts.Sum(x => x.ReferenceTotal);

        public decimal CurrentTotal => Counts.Sum(x => x.CurrentTotal);

        public decimal DifferenceTotal => CurrentTotal - ReferenceTotal;

        public CashCountViewModel()
        {
            if (!DesignMode.IsInDesignModeStatic)
                Load();
        }

        private void Load()
        {
            CashRegisterCount countData = null;
            try
            {
                string filename = PPCConfigurationManager.CashRegisterCountPath;
                countData = DataContractHelpers.Read<CashRegisterCount>(filename);
            }
            catch (Exception ex)
            {
                Logger.Exception("Error loading cash register count file", ex);
                PopupService.DisplayError("Error loading cash register count file", ex);
            }
            Counts = Keys.Select(x => new CashCountItem(x >= 5 ? CountCategories.Figures : CountCategories.Coins, x, countData?.GetCount(x) ?? 0, RefreshCurrentTotal)
            {
                Current = 0,
            }).ToList();

            //Save();
        }

        private void Save()
        {
            try
            {
                CashRegisterCount countData = new CashRegisterCount
                {
                    Entries = Counts.Select(x => new CashRegisterCountEntry
                    {
                        Value = x.Value,
                        Count = x.Reference
                    }).ToList()
                };
                string filename = PPCConfigurationManager.CashRegisterCountPath;
                DataContractHelpers.Write(filename, countData);
            }
            catch(Exception ex)
            {
                Logger.Exception("Error saving cash register count file", ex);
                PopupService.DisplayError("Error saving cash register count file", ex);
            }
        }

        private
            void RefreshCurrentTotal()
        {
            RaisePropertyChanged(() => CurrentTotal);
            RaisePropertyChanged(() => DifferenceTotal);
        }
    }

    public class CashCountViewModelDesignData : CashCountViewModel
    {
        public CashCountViewModelDesignData()
        {
            Counts = Keys.Select(x => new CashCountItem(x >= 5 ? CountCategories.Figures : CountCategories.Coins, x, 2, () => { })
            {
                Current = x >= 5 ? 1 : 2,
            }).ToList();
        }
    }
}
