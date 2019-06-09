using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PPC.Common;
using PPC.Domain;
using PPC.Helpers;
using PPC.IDataAccess;

namespace PPC.DataAccess.FileBased
{
    public class SessionDL : ISessionDL
    {
        private const string ShopFilename = "_shop.xml";
        private const string NotesFilename = "_note.xml";

        #region Transactions

        private string BuildShopFilename => $"{PPCConfigurationManager.BackupPath}{ShopFilename}";

        public List<ShopTransaction> GetTransactions()
        {
            List<ShopTransaction> transactions = new List<ShopTransaction>();

            string filename = BuildShopFilename;
            if (File.Exists(filename))
                transactions = DataContractHelpers.Read<List<ShopTransaction>>(filename);

            return transactions;
        }

        public void InsertTransaction(ShopTransaction transaction)
        {
            List<ShopTransaction> transactions = GetTransactions();
            transactions.Add(transaction);
            SaveTransactions(transactions);
        }

        public void UpdateTransaction(ShopTransaction transaction)
        {
            List<ShopTransaction> transactions = GetTransactions();
            transactions.RemoveAll(x => x.Guid == transaction.Guid);
            transactions.Add(transaction);
            SaveTransactions(transactions);
        }

        public void DeleteTransaction(ShopTransaction transaction)
        {
            List<ShopTransaction> transactions = GetTransactions();
            transactions.RemoveAll(x => x.Guid == transaction.Guid);
            SaveTransactions(transactions);
        }

        public void SaveTransaction(ShopTransaction transaction)
        {
            List<ShopTransaction> transactions = GetTransactions();
            // If transaction already exists -> update
            if (transactions.Any(x => x.Guid == transaction.Guid))
            {
                transactions.RemoveAll(x => x.Guid == transaction.Guid);
                transactions.Add(transaction);
            }
            // Else, insert
            else
                transactions.Add(transaction);
            SaveTransactions(transactions);
        }

        public void SaveTransactions(IEnumerable<ShopTransaction> transactions)
        {
            if (!Directory.Exists(PPCConfigurationManager.BackupPath))
                Directory.CreateDirectory(PPCConfigurationManager.BackupPath);
            string filename = $"{PPCConfigurationManager.BackupPath}{ShopFilename}";
            DataContractHelpers.Write(filename, transactions);
        }

        #endregion

        #region Client carts

        private Func<ClientCart, string> BuildClientFilename => cart => $"{PPCConfigurationManager.BackupPath}{(cart.HasFullPlayerInfos ? cart.DciNumber : cart.ClientName.ToLowerInvariant())}.xml";

        public List<ClientCart> GetClientCarts()
        {
            List<Exception> exceptions = null;
            List<ClientCart> carts = new List<ClientCart>();
            string path = PPCConfigurationManager.BackupPath;
            if (Directory.Exists(path))
            {
                foreach (string filename in Directory.EnumerateFiles(path, "*.xml", SearchOption.TopDirectoryOnly).Where(x => !x.Contains(ShopFilename) && !x.Contains(NotesFilename)))
                {
                    try
                    {
                        ClientCart cart = LoadClient(filename);
                        carts.Add(cart);
                    }
                    catch (Exception ex)
                    {
                        // TODO: logging
                        exceptions = exceptions ?? new List<Exception>();
                        exceptions.Add(new Exception($"Error while loading {filename ?? "??"} cart", ex));
                    }
                }
            }
            if (exceptions != null)
                throw new GetClientCartsException(carts, exceptions);
            return carts;
        }

        public void SaveClientCart(ClientCart clientCart)
        {
            if (!Directory.Exists(PPCConfigurationManager.BackupPath))
                Directory.CreateDirectory(PPCConfigurationManager.BackupPath);
            string filename = BuildClientFilename(clientCart);
            DataContractHelpers.Write(filename, clientCart);
        }

        public void DeleteClientCart(ClientCart clientCart)
        {
            // Delete backup file
            string filename = BuildClientFilename(clientCart);
            File.Delete(filename);
        }

        private ClientCart LoadClient(string filename)
        {
            ClientCart cart = DataContractHelpers.Read<ClientCart>(filename);
            return cart;
        }

        #endregion

        #region Notes

        private string BuildNotesFilename => $"{PPCConfigurationManager.BackupPath}{NotesFilename}";

        public string GetNotes()
        {
            string filename = BuildNotesFilename;
            if (File.Exists(filename))
                return File.ReadAllText(filename, Encoding.UTF8);
            return null;
        }

        public void SaveNotes(string notes)
        {
            if (!Directory.Exists(PPCConfigurationManager.BackupPath))
                Directory.CreateDirectory(PPCConfigurationManager.BackupPath);
            string filename = BuildNotesFilename;
            File.WriteAllText(filename, notes, Encoding.UTF8);
        }

        #endregion

        #region Session

        public bool HasActiveSession()
        {
            return Directory.EnumerateFiles(PPCConfigurationManager.BackupPath).Any();
        }

        public void CreateActiveSession()
        {
            // NOP
        }

        public Session GetActiveSession()
        {
            Session session = new Session
            {
                Guid = Guid.NewGuid(),
                CreationTime = DateTime.Now,
                LastReloadTime = DateTime.Now,
            };

            session.ClientCarts = GetClientCarts();
            session.Transactions = GetTransactions();
            session.Notes = GetNotes();

            return session;
        }

        public void CloseActiveSession()
        {
            string savePath = PPCConfigurationManager.BackupPath + $"{DateTime.Now:yyyy-MM-dd HH-mm-ss}\\";
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);

            // Move backup files into save folder
            string backupPath = PPCConfigurationManager.BackupPath;
            foreach (string file in Directory.EnumerateFiles(backupPath))
            {
                string saveFilename = savePath + Path.GetFileName(file);
                File.Move(file, saveFilename);
            }
        }

        #endregion
    }
}
