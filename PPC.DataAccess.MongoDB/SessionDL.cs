using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using PPC.Domain;
using PPC.IDataAccess;

namespace PPC.DataAccess.MongoDB
{
    public class SessionDL : ISessionDL
    {
        private const string DatabaseName = "PPCClub";
        private const string ActiveSessionClientCartsCollectionName = "ActiveSessionClientCarts";
        private const string ActiveSessionTransactionsCollectionName = "ActiveSessionTransactions";
        private const string ActiveSessionNotesCollectionName = "ActiveSessionNotes";
        private const string SessionsCollectionName = "Sessions";

        private IMongoCollection<ClientCart> ActiveSessionClientCartsCollection => _db.GetCollection<ClientCart>(ActiveSessionClientCartsCollectionName);
        private IMongoCollection<ShopTransaction> ActiveSessionTransactionsCollection => _db.GetCollection<ShopTransaction>(ActiveSessionTransactionsCollectionName);
        private IMongoCollection<string> ActiveSessionNotesCollection => _db.GetCollection<string>(ActiveSessionNotesCollectionName);
        private IMongoCollection<Session> SessionsCollection => _db.GetCollection<Session>(SessionsCollectionName);

        private readonly IMongoDatabase _db;

        public SessionDL()
        {
            var client = new MongoClient();
            _db = client.GetDatabase(DatabaseName);
        }

        #region Transactions


        public List<ShopTransaction> GetTransactions()
        {
            return ActiveSessionTransactionsCollection.AsQueryable().ToList();
        }

        public void InsertTransaction(ShopTransaction transaction)
        {
            ActiveSessionTransactionsCollection.InsertOne(transaction);
        }

        public void UpdateTransaction(ShopTransaction transaction)
        {
            ActiveSessionTransactionsCollection.FindOneAndReplace(x => x.Guid == transaction.Guid, transaction);
        }

        public void DeleteTransaction(ShopTransaction transaction)
        {
            ActiveSessionTransactionsCollection.DeleteOne(x => x.Guid == transaction.Guid);
        }

        public void SaveTransactions(IEnumerable<ShopTransaction> transactions)
        {
            _db.DropCollection(ActiveSessionTransactionsCollectionName);
            ActiveSessionTransactionsCollection.InsertMany(transactions);
        }

        #endregion

        #region Client carts

        public List<ClientCart> GetClientCarts()
        {
            return ActiveSessionClientCartsCollection.AsQueryable().ToList();
        }

        public void SaveClientCart(ClientCart clientCart)
        {
            ActiveSessionClientCartsCollection.ReplaceOne(
                x => x.Guid == clientCart.Guid,
                clientCart,
                new UpdateOptions
                {
                    IsUpsert = true
                });
        }

        #endregion

        #region Notes

        public string GetNotes()
        {
            return ActiveSessionNotesCollection.AsQueryable().FirstOrDefault();
        }

        public void SaveNotes(string notes)
        {
            ActiveSessionNotesCollection.ReplaceOne(x => true, notes); // weird collection with only one document
        }

        #endregion

        #region Session

        public bool HasActiveSession()
        {
            return SessionsCollection.AsQueryable().Any(x => !x.ClosingTime.HasValue); // should have only one active
        }

        public void CreateActiveSession()
        {
            Session session = new Session
            {
                Guid = Guid.NewGuid(),
                CreationTime = DateTime.Now,
            };
            SessionsCollection.InsertOne(session);
        }

        public Session GetActiveSession()
        {
            Session session = SessionsCollection.AsQueryable().Where(x => !x.ClosingTime.HasValue).OrderByDescending(x => x.CreationTime).First();
            session.LastReloadTime = DateTime.Now;
            SessionsCollection.FindOneAndReplace(x => x.Guid == session.Guid, session); // save LastReloadTime

            // Crappy way to do it
            session.ClientCarts = GetClientCarts();
            session.Transactions = GetTransactions();
            session.Notes = GetNotes();

            return session;
        }

        public void CloseActiveSession()
        {
            // Get active session
            Session session = SessionsCollection.AsQueryable().Where(x => !x.ClosingTime.HasValue).OrderByDescending(x => x.CreationTime).First();

            // Set closing time
            session.ClosingTime = DateTime.Now;

            // Add carts, transactions, notes from 'active session collections'
            session.ClientCarts = GetClientCarts();
            session.Transactions = GetTransactions();
            session.Notes = GetNotes();

            // Save session
            SessionsCollection.FindOneAndReplace(x => x.Guid == session.Guid, session);

            // Truncate active session collections
            _db.DropCollection(ActiveSessionClientCartsCollectionName);
            _db.DropCollection(ActiveSessionTransactionsCollectionName);
            _db.DropCollection(ActiveSessionNotesCollectionName);
        }

        #endregion
    }
}
