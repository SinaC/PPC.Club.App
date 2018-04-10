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
        private const string SessionsCollectionName = "Sessions";

        private static Guid _activeSessionId; // TODO: use lazy ?

        private IMongoCollection<Session> SessionsCollection => _db.GetCollection<Session>(SessionsCollectionName);

        private readonly IMongoDatabase _db;

        public SessionDL()
        {
            var client = new MongoClient();
            _db = client.GetDatabase(DatabaseName);
        }

        #region Transactions

        // TODO: remove and use Insert/Update/Delete
        public void SaveTransactions(IEnumerable<ShopTransaction> transactions)
        {
            SessionsCollection.UpdateOne(x => x.Guid == _activeSessionId, Builders<Session>.Update.Set(x => x.Transactions, transactions));
        }

        // Insert/Update could be replaced with SaveTransaction(ShopTransaction transaction)
        public void InsertTransaction(ShopTransaction transaction)
        {
            SessionsCollection.FindOneAndUpdate(x => x.Guid == _activeSessionId, Builders<Session>.Update.Push(x => x.Transactions, transaction));
        }

        public void UpdateTransaction(ShopTransaction transaction)
        {
            var filter = Builders<Session>.Filter;
            var transactionIdAndSessionIdFilter = filter.Eq(x => x.Guid, _activeSessionId) & filter.ElemMatch(x => x.Transactions, x => x.Guid == transaction.Guid);
            SessionsCollection.UpdateOne(transactionIdAndSessionIdFilter, Builders<Session>.Update.Set(x => x.Transactions[-1], transaction));
        }

        public void SaveTransaction(ShopTransaction transaction)
        {
            // Try to update
            var filter = Builders<Session>.Filter;
            var transactionIdAndSessionIdFilter = filter.Eq(x => x.Guid, _activeSessionId) & filter.ElemMatch(x => x.Transactions, x => x.Guid == transaction.Guid);
            UpdateResult updateResult = SessionsCollection.UpdateOne(transactionIdAndSessionIdFilter, Builders<Session>.Update.Set(x => x.Transactions[-1], transaction));
            // Update failed -> insert
            if (updateResult.ModifiedCount == 0)
                SessionsCollection.FindOneAndUpdate(x => x.Guid == _activeSessionId, Builders<Session>.Update.Push(x => x.Transactions, transaction));
        }

        public void DeleteTransaction(ShopTransaction transaction)
        {
            SessionsCollection.UpdateOne(x => x.Guid == _activeSessionId, Builders<Session>.Update.PullFilter(x => x.Transactions, x => x.Guid == transaction.Guid));
        }

        #endregion

        #region Client carts

        public void SaveClientCart(ClientCart clientCart)
        {
            //https://stackoverflow.com/questions/38839260/update-property-in-nested-array-of-entities-in-mongodb
            // Try to update
            var filter = Builders<Session>.Filter;
            var clientCartIdAndSessionIdFilter = filter.Eq(x => x.Guid, _activeSessionId) & filter.ElemMatch(x => x.ClientCarts, x => x.Guid == clientCart.Guid);
            UpdateResult updateResult = SessionsCollection.UpdateOne(clientCartIdAndSessionIdFilter, Builders<Session>.Update.Set(x => x.ClientCarts[-1], clientCart));
            // Update failed -> insert
            if (updateResult.ModifiedCount == 0)
                SessionsCollection.UpdateOne(x => x.Guid == _activeSessionId, Builders<Session>.Update.Push(x => x.ClientCarts, clientCart));
        }

        public void DeleteClientCart(ClientCart clientCart)
        {
            SessionsCollection.UpdateOne(x => x.Guid == _activeSessionId, Builders<Session>.Update.PullFilter(x => x.ClientCarts, x => x.Guid == clientCart.Guid));
        }

        #endregion

        #region Notes

        public void SaveNotes(string notes)
        {
            SessionsCollection.UpdateOne(x => x.Guid == _activeSessionId, Builders<Session>.Update.Set(x => x.Notes, notes));
        }

        #endregion

        #region Session

        public bool HasActiveSession()
        {
            return SessionsCollection.AsQueryable().Any(x => !x.ClosingTime.HasValue); // should have only one active
        }

        public void CreateActiveSession()
        {
            // Create new session and insert it
            Session session = new Session
            {
                Guid = Guid.NewGuid(),
                CreationTime = DateTime.Now,
                ClientCarts = new List<ClientCart>(),
                Transactions = new List<ShopTransaction>()
            };
            SessionsCollection.InsertOne(session);

            // Store session id
            _activeSessionId = session.Guid;
        }

        public Session GetActiveSession()
        {
            // Search session
            Session session = SessionsCollection.AsQueryable().Where(x => !x.ClosingTime.HasValue).OrderByDescending(x => x.CreationTime).First();
            // Update load reload time
            SessionsCollection.UpdateOne(x => x.Guid == session.Guid, Builders<Session>.Update.Set(x => x.LastReloadTime, DateTime.Now));

            // Store session id
            _activeSessionId = session.Guid;

            //
            return session;
        }

        public void CloseActiveSession()
        {
            // Update closing time
            SessionsCollection.UpdateOne(x => x.Guid == _activeSessionId, Builders<Session>.Update.Set(x => x.ClosingTime, DateTime.Now));
        }

        #endregion

        //// Some mongo tests
        //static readonly Session _session = new Session
        //{
        //    Guid = Guid.NewGuid(),
        //    Transactions = new List<ShopTransaction>
        //    {
        //        new ShopTransaction
        //        {
        //            Guid = Guid.NewGuid(),
        //            Cash = 10
        //        },
        //    },
        //    ClientCarts = new List<ClientCart>
        //    {
        //        new ClientCart
        //        {
        //            Guid = Guid.NewGuid(),
        //            Cash = 100
        //        }
        //    }
        //};

        //public void Test(int step)
        //{
        //    _activeSessionId = _session.Guid;
        //    switch (step)
        //    {
        //        // Create session
        //        case 1:
        //            SessionsCollection.InsertOne(_session);
        //            break;
        //        // Add new transaction in session
        //        case 2:
        //            SessionsCollection.FindOneAndUpdate(x => x.Guid == _session.Guid, Builders<Session>.Update.Push(x => x.Transactions, new ShopTransaction
        //            {
        //                Guid = Guid.NewGuid(),
        //                Cash = 20,
        //            }));
        //            break;
        //        // Update client cart in session
        //        case 3:
        //            {
        //                //ActiveSessionClientCartsCollection.ReplaceOne(
        //                //    x => x.Guid == clientCart.Guid,
        //                //    clientCart,
        //                //    new UpdateOptions
        //                //    {
        //                //        IsUpsert = true
        //                //    });
        //                ClientCart newCart = new ClientCart
        //                {
        //                    Guid = _session.ClientCarts[0].Guid,
        //                    //Guid = Guid.NewGuid(),
        //                    Cash = 0,
        //                    BankCard = 200,
        //                    IsPaid = true
        //                };
        //                var filter = Builders<Session>.Filter;
        //                var clientCartIdAndSessionIdFilter = filter.Eq(x => x.Guid, _activeSessionId) & filter.ElemMatch(x => x.ClientCarts, x => x.Guid == newCart.Guid);
        //                UpdateResult updateResult = SessionsCollection.UpdateOne(clientCartIdAndSessionIdFilter, Builders<Session>.Update.Set(x => x.ClientCarts[-1], newCart));
        //                // Update failed -> insert
        //                if (updateResult.ModifiedCount == 0)
        //                    SessionsCollection.UpdateOne(x => x.Guid == _session.Guid, Builders<Session>.Update.Push(x => x.ClientCarts, newCart));
        //                break;
        //            }
        //        case 4:
        //        {
        //            var transactionId = _session.Transactions[0].Guid;
        //            SessionsCollection.UpdateOne(x => x.Guid == _session.Guid, Builders<Session>.Update.PullFilter(x => x.Transactions, x => x.Guid == transactionId));
        //                //var filter = Builders<Session>.Filter;
        //                //var transactionIdAndSessionIdFilter = filter.Eq(x => x.Guid, _activeSessionId) & filter.ElemMatch(x => x.Transactions, x => x.Guid == transactionId);
        //                //UpdateResult result = SessionsCollection.UpdateOne(transactionIdAndSessionIdFilter, Builders<Session>.Update.PullFilter(x => x.Transactions, x => x.Guid == transactionId));
        //                break;
        //        }
        //        // Delete session
        //        default:
        //            SessionsCollection.DeleteOne(x => x.Guid == _session.Guid);
        //            break;
        //    }
        //}
    }
}
