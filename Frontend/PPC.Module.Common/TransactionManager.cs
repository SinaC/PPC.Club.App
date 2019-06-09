using System;
using System.Collections.ObjectModel;
using EasyIoc;
using PPC.Domain.v2;
using PPC.IDataAccess;
using PPC.Log;

namespace PPC.Module.Common
{
    //public class TransactionManager : ITransationManager
    //{
    //    private ILog Logger => IocContainer.Default.Resolve<ILog>();
    //    private ISessionDL SessionDL => IocContainer.Default.Resolve<ISessionDL>();

    //    #region ITransationManager

    //    public event EventHandler<Transaction> TransactionAdded;
    //    public event EventHandler<Transaction> TransactionModified;
    //    public event EventHandler<Transaction> TransactionDeleted;
    //    public event EventHandler TransactionReloaded;

    //    public ObservableCollection<Transaction> Transactions { get; protected set; }

    //    public void Reload(Session session)
    //    {
    //        Transactions.Clear();
    //        Transactions.AddRange(session.Transactions);
    //        TransactionReloaded?.Invoke(this, null);
    //    }

    //    public void AddTransaction(Transaction transaction)
    //    {
    //        try
    //        {
    //            // Change locally
    //            Transactions.Add(transaction);
    //            // Persist modification
    //            SessionDL.SaveTransaction(transaction);
    //            // Raise event
    //            TransactionAdded?.Invoke(this, transaction);
    //        }
    //        catch (Exception ex)
    //        {
    //            Logger.Exception(ex);
    //        }
    //    }

    //    public void ModifyTransaction(Transaction transaction)
    //    {
    //        try
    //        {
    //            // Change locally
    //            Transactions.RemoveAll(x => x.Id == transaction.Id);
    //            Transactions.Add(transaction);
    //            // Persist modification
    //            SessionDL.SaveTransaction(transaction);
    //            // Raise event
    //            TransactionModified?.Invoke(this, transaction);
    //        }
    //        catch (Exception ex)
    //        {
    //            Logger.Exception(ex);
    //        }
    //    }

    //    public void DeleteTransaction(Transaction transaction)
    //    {
    //        try
    //        {
    //            // Change locally
    //            Transactions.RemoveAll(x => x.Id == transaction.Id);
    //            // Persist modification
    //            SessionDL.DeleteTransaction(transaction);
    //            // Raise event
    //            TransactionDeleted?.Invoke(this, transaction);
    //        }
    //        catch (Exception ex)
    //        {
    //            Logger.Exception(ex);
    //        }
    //    }

    //    #endregion

    //    public TransactionManager()
    //    {
    //        Transactions = new ObservableCollection<Transaction>();
    //    }
    //}
}
