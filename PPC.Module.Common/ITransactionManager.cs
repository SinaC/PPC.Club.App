using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PPC.Domain.v2;

namespace PPC.Module.Common
{
    public interface ITransationManager
    {
        event EventHandler<Transaction> TransactionAdded;
        event EventHandler<Transaction> TransactionModified;
        event EventHandler<Transaction> TransactionDeleted;
        event EventHandler TransactionReloaded;

        ObservableCollection<Transaction> Transactions { get; }

        void Reload(Session session);

        void AddTransaction(Transaction transaction);
        void ModifyTransaction(Transaction transaction);
        void DeleteTransaction(Transaction transaction);
    }
}
