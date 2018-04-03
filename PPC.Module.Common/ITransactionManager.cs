using System;
using System.Collections.Generic;
using PPC.Domain;

namespace PPC.Module.Common
{
    public interface ITransationManager
    {
        event EventHandler<ShopTransaction> TransactionAdded;
        event EventHandler<ShopTransaction> TransactionModified;
        event EventHandler<ShopTransaction> TransactionDeleted;
        event EventHandler TransactionReloaded;

        IEnumerable<ShopTransaction> Transactions { get; }

        void Reload(Session session);

        void AddTransaction(ShopTransaction transaction);
        void ModifyTransaction(ShopTransaction transaction);
        void DeleteTransaction(ShopTransaction transaction);
    }
}
