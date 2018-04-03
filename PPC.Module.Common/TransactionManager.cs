using System;
using System.Collections.Generic;
using EasyIoc;
using PPC.Domain;
using PPC.IDataAccess;
using PPC.Log;

namespace PPC.Module.Common
{
    /*
    public class TransactionManager : ITransationManager
    {
        private ILog Logger => IocContainer.Default.Resolve<ILog>();
        private ISessionDL SessionDL => IocContainer.Default.Resolve<ISessionDL>();

        private List<ShopTransaction> _transactions;

        #region ITransationManager

        public event EventHandler<ShopTransaction> TransactionAdded;
        public event EventHandler<ShopTransaction> TransactionModified;
        public event EventHandler<ShopTransaction> TransactionDeleted;
        public event EventHandler TransactionReloaded;

        public IEnumerable<ShopTransaction> Transactions => _transactions;

        public void Reload(Session session)
        {
            _transactions = session.Transactions;
            TransactionReloaded?.Invoke(this, null);
        }

        public void AddTransaction(ShopTransaction transaction)
        {
            try
            {
                // Change locally
                _transactions.Add(transaction);
                // Persist modification
                SessionDL.InsertTransaction(transaction);
                // Raise event
                TransactionAdded?.Invoke(this, transaction);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        public void ModifyTransaction(ShopTransaction transaction)
        {
            try
            {
                // Change locally
                _transactions.RemoveAll(x => x.Guid == transaction.Guid);
                _transactions.Add(transaction);
                // Persist modification
                SessionDL.UpdateTransaction(transaction);
                // Raise event
                TransactionModified?.Invoke(this, transaction);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        public void DeleteTransaction(ShopTransaction transaction)
        {
            try
            {
                // Change locally
                _transactions.RemoveAll(x => x.Guid == transaction.Guid);
                // Persist modification
                SessionDL.DeleteTransaction(transaction);
                // Raise event
                TransactionDeleted?.Invoke(this, transaction);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        #endregion

        public TransactionManager()
        {
            _transactions = new List<ShopTransaction>();
        }
    }
    */
}
