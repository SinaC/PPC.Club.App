﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using EasyMVVM;
using PPC.Data.Contracts;

namespace PPC.Popups
{
    public enum CountCategories
    {
        Figures,
        Coins
    }

    public class CashRegisterCountItem : ObservableObject
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
            get { return _current; }
            set
            {
                if (Set(() => Current, ref _current, value))
                {
                    RaisePropertyChanged(() => CurrentTotal);
                    _currentCountModifiedAction?.Invoke();
                }
            }
        }

        public CashRegisterCountItem(CountCategories category, decimal value, int reference, Action currentCountModifiedAction)
        {
            Category = category;
            Value = value;
            Reference = reference;
            _currentCountModifiedAction = currentCountModifiedAction;
        }
    }

    public class CashRegisterCountViewModel : ObservableObject
    {
        private IPopupService PopupService => EasyIoc.IocContainer.Default.Resolve<IPopupService>();

        protected decimal[] Keys = {500, 200, 100, 50, 20, 10, 5, 2, 1, 0.5m, 0.2m, 0.1m, 0.05m, 0.02m, 0.01m};

        protected List<CashRegisterCountItem> Counts { get; set; }

        public IEnumerable<CashRegisterCountItem> Coins => Counts.Where(x => x.Category == CountCategories.Coins);
        public IEnumerable<CashRegisterCountItem> Figures => Counts.Where(x => x.Category == CountCategories.Figures);

        public decimal ReferenceTotal => Counts.Sum(x => x.ReferenceTotal);

        public decimal CurrentTotal => Counts.Sum(x => x.CurrentTotal);

        public decimal DifferenceTotal => CurrentTotal - ReferenceTotal;

        public CashRegisterCountViewModel()
        {
            Load();
        }

        private void Load()
        {
            CashRegisterCount countData = null;
            try
            {
                string filename = ConfigurationManager.AppSettings["CashRegisterCountPath"];
                using (XmlTextReader reader = new XmlTextReader(filename))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(CashRegisterCount));
                    countData = (CashRegisterCount) serializer.ReadObject(reader);
                }
            }
            catch (Exception ex)
            {
                //PopupService.DisplayError("Error reading cash register count file", ex); // TODO: this popup will be displayed under current popup :/
            }
            Counts = Keys.Select(x => new CashRegisterCountItem(x >= 5 ? CountCategories.Figures : CountCategories.Coins, x, countData?.GetCount(x) ?? 0, RefreshCurrentTotal)
            {
                Current = 0,
            }).ToList();

            //Save();
        }

        private void Save()
        {
            CashRegisterCount countData = new CashRegisterCount
            {
                Entries = Counts.Select(x => new CashRegisterCountEntry
                {
                    Value = x.Value,
                    Count = x.Reference
                }).ToList()
            };
            string filename = ConfigurationManager.AppSettings["CashRegisterCountPath"];
            using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                DataContractSerializer serializer = new DataContractSerializer(typeof(CashRegisterCount));
                serializer.WriteObject(writer, countData);
            }
        }

        private void RefreshCurrentTotal()
        {
            RaisePropertyChanged(() => CurrentTotal);
            RaisePropertyChanged(() => DifferenceTotal);
        }
    }

    public class CashRegisterCountViewModelDesignData : CashRegisterCountViewModel
    {
        public CashRegisterCountViewModelDesignData()
        {
            Counts = Keys.Select(x => new CashRegisterCountItem(x >= 5 ? CountCategories.Figures : CountCategories.Coins, x, 2, () => { })
            {
                Current = x >= 5 ? 1 : 2,
            }).ToList();
        }
    }
}
